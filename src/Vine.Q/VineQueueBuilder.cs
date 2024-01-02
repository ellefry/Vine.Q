using System;
using System.Collections.Concurrent;

namespace Vine.Q;

public class VineQueueBuilder : IVineQueueBuilder
{
    public static VineWorkQueue<T> Create<T>(string name, int capacity)
    {
        var queue = new VineWorkQueue<T>(name, capacity);
        return queue;
    }

    public IVineWorkQueue Create<T>(string name, int capacity, Action<T> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
        return queue;
    }

    public IVineWorkQueue Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
        return queue;
    }
}


