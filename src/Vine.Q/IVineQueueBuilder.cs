using System;

namespace Vine.Q;

public interface IVineQueueBuilder
{
    void Create<T>(string name, int capacity, Action<T> onNext);
    void Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext);
}


