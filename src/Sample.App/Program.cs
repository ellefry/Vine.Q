using Microsoft.Extensions.DependencyInjection;
using Vine.Q;
using System.Linq;

namespace Sample.App;

internal static class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddDefaultVineQueueWithReturn<Message, Task, MessageHandler>();

        services.AddVineQueueWithReturn<Message, Task, MessageHandler2>("local2", 2000);

        services.AddVineQueue<Message, MessageHandler3>("local3", int.MaxValue);

        services.AddVineQueueWithReturn<Message, Task, MessageHandler4>("local4", 2000);

        var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetRequiredService<IVineQueuePublisher>();

        Enumerable.Range(1, 1000_0000).AsParallel().ForAll(idx => {
            publisher.Publish(new Message { Id = 1.ToString() });
            publisher.Publish(new Message { Id = 2.ToString() }, "local2");
            publisher.Publish(new Message { Id = 3.ToString() }, "local3");
            publisher.Publish(new Message { Id = 4.ToString() }, "local4");
        });
        
        Console.ReadKey();
    }
}

public class Message
{
    public string? Id { get; set; }
}

public class MessageHandler : IVineQueueHandlerWithReturn<Message,Task>
{
    private readonly string _preCondition;

    public MessageHandler()
    {
        _preCondition = "MessageHandler";
        Console.WriteLine($"MessageHandler constrcut.");
    }

    public async Task Handle(Message message)
    {
        message.Id = _preCondition;
        Console.WriteLine($"[1] Consume message : {message.Id}");
        await Task.FromResult(0);
    }
}

public class MessageHandler2 : IVineQueueHandlerWithReturn<Message, Task>
{
    public async Task Handle(Message message)
    {
        Console.WriteLine($"[2] Consume message : {message.Id}");
        await Task.CompletedTask;
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
        Console.WriteLine($"[4] Consume message : {message.Id}");
        await Task.CompletedTask;
    }
}
