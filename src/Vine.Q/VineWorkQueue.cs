using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Vine.Q;

public interface IVineWorkQueue
{
    string Name { get; }
}

public interface IVineWorkQueue<in T> : IVineWorkQueue
{
    void Send(T item);
}

public class VineWorkQueue<T> : IVineWorkQueue<T>
{
    private readonly BlockingCollection<T> _queue;
    private readonly IObservable<T> _observable;

    public string Name { get; private set; }

    public VineWorkQueue(string name = Constants.DEFAULT_QUEUE, int capacity = Constants.DEFAULT_QUEUE_SIZE)
    {
        Name = name;
        _queue = new BlockingCollection<T>(capacity);
        _observable = _queue.
                  GetConsumingEnumerable().
                  ToObservable(TaskPoolScheduler.Default);
    }

    public void RegisterHandler(Action<T> onNext)
    {
        _observable.Subscribe(onNext);
    }

    public void RegisterHandler<TReturn>(Func<T, TReturn> onNext)
    {
        _observable.Select(onNext)
            .Subscribe();
    }

    public void Send(T item)
    {
        _queue.Add(item);
    }
}
