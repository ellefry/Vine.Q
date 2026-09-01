using System;
using System.Threading.Tasks;

namespace Vine.Q;

public class VineQueueBuilder : IVineQueueBuilder
{
  public static VineWorkQueue<T> Create<T>(string name, int capacity, VineQueueOptions<T> options)
  {
    var queue = new VineWorkQueue<T>(name, capacity, options);
    return queue;
  }

  public IVineWorkQueue Create<T>(string name, int capacity, VineQueueOptions<T> options, Func<T, Task> onNext)
  {
    var queue = Create<T>(name, capacity, options);
    queue.RegisterHandler(onNext);
    return queue;
  }
}


