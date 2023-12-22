namespace Vine.Q;

public interface IVineQueuePublisher
{
    void Publish<T>(T message, string queue = Constants.DEFAULT_QUEUE);
}


