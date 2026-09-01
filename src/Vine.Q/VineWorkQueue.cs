using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Vine.Q;

public interface IVineWorkQueue
{
  string Name { get; }
  VineQueueMetrics GetMetrics();
}

public interface IVineWorkQueue<in T> : IVineWorkQueue
{
  Task SendAsync(T item, CancellationToken cancellationToken = default);
  ValueTask<bool> TrySendAsync(T item);
}

public class VineWorkQueue<T> : IVineWorkQueue<T>, IDisposable
{
  private readonly Channel<QueuedMessage> _queue;
  private readonly VineQueueOptions<T> _options;
  private readonly CancellationTokenSource _stopCts = new();
  private readonly object _startLock = new();
  private readonly Counter<long> _publishedCounter;
  private readonly Counter<long> _successfulCounter;
  private readonly Counter<long> _failedCounter;
  private readonly Counter<long> _retryCounter;
  private readonly Histogram<double> _consumptionLatency;
  private long _publishedMessages;
  private long _successfulMessages;
  private long _failedMessages;
  private long _totalConsumptionLatencyTicks;
  private long _queuedMessages;
  private Task[]? _workers;

  public string Name { get; }

  public VineWorkQueue(string name = Constants.DEFAULT_QUEUE, int capacity = Constants.DEFAULT_QUEUE_SIZE)
      : this(name, capacity, new VineQueueOptions<T>())
  {
  }

  public VineWorkQueue(string name, int capacity, VineQueueOptions<T> options)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    _options = options ?? throw new ArgumentNullException(nameof(options));
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be greater than zero.");
    }

    if (_options.MaxConcurrency <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "MaxConcurrency must be greater than zero.");
    }

    if (_options.MaxRetryCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "MaxRetryCount cannot be negative.");
    }

    if (_options.RetryDelay < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "RetryDelay cannot be negative.");
    }

    _queue = Channel.CreateBounded<QueuedMessage>(new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait,
      SingleWriter = false,
      SingleReader = _options.MaxConcurrency == 1,
      AllowSynchronousContinuations = false
    });
    var meter = new Meter("Vine.Q");
    _publishedCounter = meter.CreateCounter<long>("vineq.messages.published");
    _successfulCounter = meter.CreateCounter<long>("vineq.messages.succeeded");
    _failedCounter = meter.CreateCounter<long>("vineq.messages.failed");
    _retryCounter = meter.CreateCounter<long>("vineq.messages.retried");
    _consumptionLatency = meter.CreateHistogram<double>("vineq.message.consumption_latency_ms");
  }

  public void RegisterHandler(Func<T, Task> onNext)
  {
    ArgumentNullException.ThrowIfNull(onNext);
    Start(onNext);
  }

  public async Task SendAsync(T item, CancellationToken cancellationToken = default)
  {
    var queuedMessage = CreateQueuedMessage(item);
    Interlocked.Increment(ref _queuedMessages);
    try
    {
      await _queue.Writer.WriteAsync(queuedMessage, cancellationToken).ConfigureAwait(false);
    }
    catch
    {
      Interlocked.Decrement(ref _queuedMessages);
      throw;
    }

    RecordPublished(queuedMessage);
  }

  public ValueTask<bool> TrySendAsync(T item)
  {
    var queuedMessage = CreateQueuedMessage(item);
    Interlocked.Increment(ref _queuedMessages);
    if (!_queue.Writer.TryWrite(queuedMessage))
    {
      Interlocked.Decrement(ref _queuedMessages);
      return ValueTask.FromResult(false);
    }

    RecordPublished(queuedMessage);
    return ValueTask.FromResult(true);
  }

  public VineQueueMetrics GetMetrics()
  {
    return new VineQueueMetrics(
        (int)Math.Max(0, Interlocked.Read(ref _queuedMessages)),
        Interlocked.Read(ref _publishedMessages),
        Interlocked.Read(ref _successfulMessages),
        Interlocked.Read(ref _failedMessages),
        StopwatchTicksToTimeSpan(Interlocked.Read(ref _totalConsumptionLatencyTicks)));
  }

  public void Complete()
  {
    _queue.Writer.TryComplete();
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    Complete();

    var workers = _workers;
    if (workers is null)
    {
      return;
    }

    try
    {
      await Task.WhenAll(workers).WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      _stopCts.Cancel();
      await Task.WhenAll(workers).ConfigureAwait(false);
      throw;
    }
  }

  public void Dispose()
  {
    StopAsync().GetAwaiter().GetResult();
    _stopCts.Dispose();
  }

  private void Start(Func<T, Task> handler)
  {
    lock (_startLock)
    {
      if (_workers is not null)
      {
        throw new InvalidOperationException("A handler has already been registered for this queue.");
      }

      _workers = new Task[_options.MaxConcurrency];
      for (var index = 0; index < _workers.Length; index++)
      {
        _workers[index] = Task.Run(() => ConsumeAsync(handler), _stopCts.Token);
      }
    }
  }

  private async Task ConsumeAsync(Func<T, Task> handler)
  {
    try
    {
      await foreach (var item in _queue.Reader.ReadAllAsync(_stopCts.Token).ConfigureAwait(false))
      {
        Interlocked.Decrement(ref _queuedMessages);
        await ProcessAsync(item, handler).ConfigureAwait(false);
      }
    }
    catch (OperationCanceledException) when (_stopCts.IsCancellationRequested)
    {
    }
  }

  private async Task ProcessAsync(QueuedMessage item, Func<T, Task> handler)
  {
    for (var attempt = 1; ; attempt++)
    {
      try
      {
        await handler(item.Message).ConfigureAwait(false);
        Interlocked.Increment(ref _successfulMessages);
        var elapsed = AddConsumptionLatency(item.EnqueuedAt);
        _successfulCounter.Add(1);
        RaiseEvent(new VineQueueEvent<T>(VineQueueEventKind.Succeeded, Name, item.Message, attempt, null, elapsed));
        return;
      }
      catch (Exception exception) when (attempt <= _options.MaxRetryCount)
      {
        _retryCounter.Add(1);
        RaiseEvent(new VineQueueEvent<T>(VineQueueEventKind.Retried, Name, item.Message, attempt, exception, StopwatchTicksToTimeSpan(Stopwatch.GetTimestamp() - item.EnqueuedAt)));
        if (_options.RetryDelay > TimeSpan.Zero)
        {
          await Task.Delay(_options.RetryDelay, _stopCts.Token).ConfigureAwait(false);
        }
      }
      catch (Exception exception)
      {
        if (_options.OnFailureAsync is not null)
        {
          try
          {
            await _options.OnFailureAsync(
                new VineQueueFailureContext<T>(item.Message, exception, attempt)).ConfigureAwait(false);
          }
          catch
          {
          }
        }

        Interlocked.Increment(ref _failedMessages);
        var elapsed = AddConsumptionLatency(item.EnqueuedAt);
        _failedCounter.Add(1);
        RaiseEvent(new VineQueueEvent<T>(VineQueueEventKind.Failed, Name, item.Message, attempt, exception, elapsed));
        return;
      }
    }
  }

  private QueuedMessage CreateQueuedMessage(T item)
  {
    return new QueuedMessage(item, Stopwatch.GetTimestamp());
  }

  private void RecordPublished(QueuedMessage queuedMessage)
  {
    Interlocked.Increment(ref _publishedMessages);
    _publishedCounter.Add(1);
    RaiseEvent(new VineQueueEvent<T>(VineQueueEventKind.Published, Name, queuedMessage.Message, 0, null, TimeSpan.Zero));
  }

  private TimeSpan AddConsumptionLatency(long enqueuedAt)
  {
    var elapsed = StopwatchTicksToTimeSpan(Stopwatch.GetTimestamp() - enqueuedAt);
    Interlocked.Add(ref _totalConsumptionLatencyTicks, elapsed.Ticks);
    _consumptionLatency.Record(elapsed.TotalMilliseconds);
    return elapsed;
  }

  private static TimeSpan StopwatchTicksToTimeSpan(long ticks)
  {
    return TimeSpan.FromSeconds((double)ticks / Stopwatch.Frequency);
  }

  private void RaiseEvent(VineQueueEvent<T> queueEvent)
  {
    if (_options.OnEvent is null)
    {
      return;
    }

    try
    {
      _options.OnEvent(queueEvent);
    }
    catch
    {
    }
  }

  private sealed class QueuedMessage
  {
    public QueuedMessage(T message, long enqueuedAt)
    {
      Message = message;
      EnqueuedAt = enqueuedAt;
    }

    public T Message { get; }

    public long EnqueuedAt { get; }
  }
}
