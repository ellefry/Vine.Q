namespace Vine.Q;

public interface IVineWorkQueueAcquirer
{
    IVineWorkQueue<T>? GetWorkQueue<T>(string name);
}


