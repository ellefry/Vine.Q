using System;

namespace Vine.Q;

public enum VineQueueEventKind
{
  Published,
  Retried,
  Succeeded,
  Failed
}

public sealed class VineQueueEvent<T>
{
  public VineQueueEvent(VineQueueEventKind kind, string queueName, T message, int attempt, Exception? exception, TimeSpan elapsed)
  {
    Kind = kind;
    QueueName = queueName;
    Message = message;
    Attempt = attempt;
    Exception = exception;
    Elapsed = elapsed;
  }

  public VineQueueEventKind Kind { get; }

  public string QueueName { get; }

  public T Message { get; }

  public int Attempt { get; }

  public Exception? Exception { get; }

  public TimeSpan Elapsed { get; }
}
