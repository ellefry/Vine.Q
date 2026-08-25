using System;
using System.Threading.Tasks;

namespace Vine.Q;

public sealed class VineQueueFailureContext<T>
{
    public VineQueueFailureContext(T message, Exception exception, int attempt)
    {
        Message = message;
        Exception = exception;
        Attempt = attempt;
    }

    public T Message { get; }

    public Exception Exception { get; }

    public int Attempt { get; }
}

public sealed class VineQueueOptions<T>
{
    public int MaxConcurrency { get; set; } = 1;

    public int MaxRetryCount { get; set; }

    public TimeSpan RetryDelay { get; set; } = TimeSpan.Zero;

    public Func<VineQueueFailureContext<T>, Task>? OnFailureAsync { get; set; }

    public Action<VineQueueEvent<T>>? OnEvent { get; set; }
}
