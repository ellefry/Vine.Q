using System.Threading;
using System.Threading.Tasks;

namespace Vine.Q;

public interface IVineQueuePublisher
{
  Task PublishAsync<T>(T message, string queue = Constants.DEFAULT_QUEUE, CancellationToken cancellationToken = default);
  ValueTask<bool> TryPublishAsync<T>(T message, string queue = Constants.DEFAULT_QUEUE);
}


