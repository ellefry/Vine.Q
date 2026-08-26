using System;
using System.Threading.Tasks;

namespace Vine.Q;

public interface IVineQueueBuilder
{
    IVineWorkQueue Create<T>(string name, int capacity, VineQueueOptions<T> options, Func<T, Task> onNext);
}


