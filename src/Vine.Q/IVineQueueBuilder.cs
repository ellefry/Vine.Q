using System;

namespace Vine.Q;

public interface IVineQueueBuilder
{
    IVineWorkQueue Create<T>(string name, int capacity, Action<T> onNext);
    IVineWorkQueue Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext);
}


