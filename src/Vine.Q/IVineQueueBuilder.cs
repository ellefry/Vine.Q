using System;

namespace Vine.Q;

public interface IVineQueueBuilder
{
    IVineWorkQueue Create<T>(string name, int capacity, Action<T> onNext);
    IVineWorkQueue Create<T>(string name, int capacity, VineQueueOptions<T> options, Action<T> onNext);
    IVineWorkQueue Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext);
    IVineWorkQueue Create<T, TReturn>(string name, int capacity, VineQueueOptions<T> options, Func<T, TReturn> onNext);
}


