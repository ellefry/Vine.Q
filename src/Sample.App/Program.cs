using Microsoft.Extensions.DependencyInjection;
using Vine.Q;

namespace Sample.App;

internal static class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddDefaultVineQueue<Message, MessageHandler>();

        services.AddVineQueue<Message, MessageHandler2>("local2", 5_000);

        services.AddVineQueue<Message, MessageHandler3>("local3", 10_0000);

        services.AddVineQueue<Message, MessageHandler4>("local4", 1_0000);

        var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetRequiredService<IVineQueuePublisher>();

        var publishTasks = Enumerable.Range(1, 10).Select(async idx =>
        {
            await publisher.PublishAsync(new Message { Id = idx.ToString() });
            await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local2");
            await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local3");
            await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local4");
        });

        await Task.WhenAll(publishTasks);

        Console.ReadKey();
    }
}


public class Message
{
    public string? Id { get; set; }
}

public class MessageHandler : IVineQueueHandler<Message>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[1] Consume message : {message.Id}");
    }
}

public class MessageHandler2 : IVineQueueHandler<Message>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[2] Consume message : {message.Id}");
    }
}

public class MessageHandler3 : IVineQueueHandler<Message>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[3] Consume message : {message.Id}");
    }
}

public class MessageHandler4 : IVineQueueHandler<Message>
{
    public async Task Handle(Message message)
    {
        await Console.Out.WriteLineAsync($"[4] Consume message : {message.Id}");
    }
}
