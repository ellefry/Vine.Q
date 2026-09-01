using System.Threading.Tasks;

namespace Vine.Q;

public interface IVineQueueHandler<T>
{
  Task Handle(T message);
}