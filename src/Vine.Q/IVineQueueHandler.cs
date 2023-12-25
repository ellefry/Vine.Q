namespace Vine.Q;

public interface IVineQueueHandler<T>
{
    void Handle(T message);
}

public interface IVineQueueHandlerWithReturn<in T, out TReturn>
{
    TReturn Handle(T message);
}


