using Xunit;

namespace Vine.Q.Tests;

public sealed class VineWorkQueueTests
{
    [Fact]
    public async Task AsyncHandlerIsAwaitedAndMessageIsAutomaticallyAcknowledged()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var queue = new VineWorkQueue<int>("test", 10, new VineQueueOptions<int>
        {
            OnEvent = queueEvent =>
            {
                if (queueEvent.Kind == VineQueueEventKind.Succeeded)
                {
                    completed.TrySetResult();
                }
            }
        });

        queue.RegisterHandler(async _ =>
        {
            await Task.Delay(25);
        });
        queue.Send(1);

        await completed.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var metrics = queue.GetMetrics();
        Assert.Equal(1, metrics.PublishedMessages);
        Assert.Equal(1, metrics.SuccessfulMessages);
    }

    [Fact]
    public async Task FailedMessageIsRetriedAndReportedAfterRetryLimit()
    {
        var attempts = 0;
        var failed = new TaskCompletionSource<VineQueueFailureContext<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var queue = new VineWorkQueue<int>("test", 10, new VineQueueOptions<int>
        {
            MaxRetryCount = 2,
            OnFailureAsync = context =>
            {
                failed.TrySetResult(context);
                return Task.CompletedTask;
            }
        });

        queue.RegisterHandler<int>(_ =>
        {
            Interlocked.Increment(ref attempts);
            throw new InvalidOperationException("test failure");
        });
        queue.Send(1);

        var failure = await failed.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(3, attempts);
        Assert.Equal(3, failure.Attempt);
        Assert.Equal(1, queue.GetMetrics().FailedMessages);
    }

    [Fact]
    public async Task DefaultConcurrencyProcessesMessagesSerially()
    {
        var active = 0;
        var maximumActive = 0;
        var completedCount = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var queue = new VineWorkQueue<int>("test", 10, new VineQueueOptions<int>
        {
            OnEvent = queueEvent =>
            {
                if (queueEvent.Kind == VineQueueEventKind.Succeeded && Interlocked.Increment(ref completedCount) == 3)
                {
                    completed.TrySetResult();
                }
            }
        });

        queue.RegisterHandler(async _ =>
        {
            var current = Interlocked.Increment(ref active);
            InterlockedExtensions.Max(ref maximumActive, current);
            await Task.Delay(10);
            Interlocked.Decrement(ref active);
        });

        queue.Send(1);
        queue.Send(2);
        queue.Send(3);
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(1, maximumActive);
    }

    [Fact]
    public async Task StopAsyncCompletesQueuedMessages()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var queue = new VineWorkQueue<int>("test", 10);
        queue.RegisterHandler(_ => completed.TrySetResult());
        queue.Send(1);

        await queue.StopAsync();

        await completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(1, queue.GetMetrics().SuccessfulMessages);
    }

    private static class InterlockedExtensions
    {
        public static void Max(ref int location, int value)
        {
            while (true)
            {
                var current = Volatile.Read(ref location);
                if (value <= current || Interlocked.CompareExchange(ref location, value, current) == current)
                {
                    return;
                }
            }
        }
    }
}
