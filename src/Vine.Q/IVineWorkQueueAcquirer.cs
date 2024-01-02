using System.Collections.Generic;
using System.Linq;

namespace Vine.Q;

public interface IVineWorkQueueAcquirer
{
    IVineWorkQueue<T>? GetWorkQueue<T>(string name);
}

public class VineWorkQueueAcquirer : IVineWorkQueueAcquirer
{
    private readonly IEnumerable<IVineWorkQueue> _vineWorkQueues;

    public VineWorkQueueAcquirer(IEnumerable<IVineWorkQueue> vineWorkQueues)
    {
        _vineWorkQueues = vineWorkQueues;
    }

    public IVineWorkQueue<T>? GetWorkQueue<T>(string name)
    {
        return _vineWorkQueues.FirstOrDefault(x => x.Name == name) as IVineWorkQueue<T>;
    }
}

