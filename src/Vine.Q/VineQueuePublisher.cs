using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vine.Q;

public class VineQueuePublisher : IVineQueuePublisher
{
    private readonly IVineWorkQueueAcquirer _queueAcquirer;

    public VineQueuePublisher(IVineWorkQueueAcquirer queueAcquirer)
    {
        _queueAcquirer = queueAcquirer;
    }

    public void Publish<T>(T message, string queue = Constants.DEFAULT_QUEUE)
    {
        GetRequiredQueue<T>(queue).Send(message);
    }

    public Task PublishAsync<T>(T message, string queue = Constants.DEFAULT_QUEUE, CancellationToken cancellationToken = default)
    {
        return GetRequiredQueue<T>(queue).SendAsync(message, cancellationToken);
    }

    public bool TryPublish<T>(T message, string queue = Constants.DEFAULT_QUEUE)
    {
        var workQueue = _queueAcquirer.GetWorkQueue<T>(queue);
        if (workQueue is null)
        {
            return false;
        }

        try
        {
            workQueue.Send(message);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private IVineWorkQueue<T> GetRequiredQueue<T>(string queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            throw new ArgumentException("Queue name cannot be null or whitespace.", nameof(queue));
        }
        return _queueAcquirer.GetWorkQueue<T>(queue)
            ?? throw new InvalidOperationException($"No Vine.Q queue is registered for name '{queue}' and message type '{typeof(T).FullName}'.");
    }
}


