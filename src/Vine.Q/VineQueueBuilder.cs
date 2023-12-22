using System;
using System.Collections.Concurrent;

namespace Vine.Q;
public class VineQueueBuilder : IVineQueueBuilder, IVineWorkQueueAcquirer
{
    private static readonly ConcurrentDictionary<string, IVineWorkQueue> _dictionary = [];

    public static VineWorkQueue<T> Create<T>(string name, int capacity)
    {
        var queue = new VineWorkQueue<T>(name, capacity);
        _dictionary.TryAdd(name, queue);
        return queue;
    }

    public void Create<T>(string name, int capacity, Action<T> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
    }

    public void Create<T, TReturn>(string name, int capacity, Func<T, TReturn> onNext)
    {
        var queue = Create<T>(name, capacity);
        queue.RegisterHandler(onNext);
    }

    public IVineWorkQueue<T>? GetWorkQueue<T>(string name)
    {
        if (_dictionary.TryGetValue(name, out var workQueue) && workQueue is IVineWorkQueue<T> wq)
        {
            return wq;
        }
        return null;
    }
}


