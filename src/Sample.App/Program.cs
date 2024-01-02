using Microsoft.Extensions.DependencyInjection;
using Vine.Q;

namespace Sample.App;

internal static class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddDefaultVineQueueWithReturn<Message, Task, MessageHandler>();

        services.AddVineQueueWithReturn<Message, Task, MessageHandler2>("local2", 5_000);

        services.AddVineQueue<Message, MessageHandler3>("local3", 10_0000);

        services.AddVineQueueWithReturn<Message, Task, MessageHandler4>("local4", 1_0000);

        var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetRequiredService<IVineQueuePublisher>();

        Enumerable.Range(1, 10).AsParallel().ForAll(idx =>
        {
            publisher.Publish(new Message { Id = idx.ToString() });
            publisher.Publish(new Message { Id = idx.ToString() }, "local2");
            publisher.Publish(new Message { Id = idx.ToString() }, "local3");
            publisher.Publish(new Message { Id = idx.ToString() }, "local4");
        });

        Console.ReadKey();
    }
}


public class Message
{
    public string? Id { get; set; }
}

public class MessageHandler : IVineQueueHandlerWithReturn<Message, Task>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[1] Consume message : {message.Id}");
    }
}

public class MessageHandler2 : IVineQueueHandlerWithReturn<Message, Task>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[2] Consume message : {message.Id}");
    }
}

public class MessageHandler3 : IVineQueueHandler<Message>
{
    public void Handle(Message message)
    {
        Console.WriteLine($"[3] Consume message : {message.Id}");
    }
}

public class MessageHandler4 : IVineQueueHandlerWithReturn<Message, Task>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[4] Consume message : {message.Id}");
    }
}
