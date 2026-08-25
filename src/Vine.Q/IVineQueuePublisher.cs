using System.Threading;
using System.Threading.Tasks;

namespace Vine.Q;

public interface IVineQueuePublisher
{
    void Publish<T>(T message, string queue = Constants.DEFAULT_QUEUE);
    Task PublishAsync<T>(T message, string queue = Constants.DEFAULT_QUEUE, CancellationToken cancellationToken = default);
    bool TryPublish<T>(T message, string queue = Constants.DEFAULT_QUEUE);
}


