namespace Vine.Q;

public class VineQueuePublisher : IVineQueuePublisher
{
    private readonly IVineWorkQueueAcquirer _queueAcquirer;

    public VineQueuePublisher(IVineWorkQueueAcquirer queueAcquirer, IVineWorkQueue queueInstance)
    {
        _queueAcquirer = queueAcquirer;
    }

    public void Publish<T>(T message, string queue = Constants.DEFAULT_QUEUE)
    {
        var workQueue = _queueAcquirer.GetWorkQueue<T>(queue);
        workQueue?.Send(message);
    }
}


