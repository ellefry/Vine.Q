using System;

namespace Vine.Q;

public class VineQueueBuilder : IVineQueueBuilder
{
    public static VineWorkQueue<T> Create<T>(string name, int capacity)
    {
        var queue = new VineWorkQueue<T>(name, capacity);
        return queue;
    }

    public static VineWorkQueue<T> Create<T>(string name, int capacity, VineQueueOptions<T> options)
    {
        var queue = new VineWorkQueue<T>(name, capacity, options);
        return queue;
    }

    public IVineWorkQueue Create<T>(string name, int capacity, Action<T> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
        return queue;
    }

    public IVineWorkQueue Create<T>(string name, int capacity, VineQueueOptions<T> options, Action<T> onNext)
    {
        var queue = Create<T>(name, capacity, options);
        queue.RegisterHandler(onNext);
        return queue;
    }

    public IVineWorkQueue Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
        return queue;
    }

    public IVineWorkQueue Create<T, TReturn>(string name, int capacity, VineQueueOptions<T> options, Func<T, TReturn> onNext)
    {
        var queue = Create<T>(name, capacity, options);
        queue.RegisterHandler(onNext);
        return queue;
    }
}


