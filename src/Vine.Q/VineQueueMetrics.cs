using System;

namespace Vine.Q;

public sealed class VineQueueMetrics
{
    public VineQueueMetrics(int queueLength, long publishedMessages, long successfulMessages, long failedMessages, TimeSpan totalConsumptionLatency)
    {
        QueueLength = queueLength;
        PublishedMessages = publishedMessages;
        SuccessfulMessages = successfulMessages;
        FailedMessages = failedMessages;
        TotalConsumptionLatency = totalConsumptionLatency;
    }

    public int QueueLength { get; }

    public long PublishedMessages { get; }

    public long SuccessfulMessages { get; }

    public long FailedMessages { get; }

    public TimeSpan TotalConsumptionLatency { get; }

    public TimeSpan AverageConsumptionLatency => SuccessfulMessages + FailedMessages == 0
        ? TimeSpan.Zero
        : TimeSpan.FromTicks(TotalConsumptionLatency.Ticks / (SuccessfulMessages + FailedMessages));
}
