using Microsoft.Extensions.DependencyInjection;
using Vine.Q;

namespace Sample.App;

internal class Program
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


        publisher.Publish(new Message { Id = 1.ToString() });
        publisher.Publish(new Message { Id = 2.ToString() }, "local2");
        publisher.Publish(new Message { Id = 3.ToString() }, "local3");
        publisher.Publish(new Message { Id = 4.ToString() }, "local4");


        Console.ReadKey();
    }
}

public class Message : IVineQueueHandler
{
    public string? Id { get; set; }
}

public class MessageHandler : IVineQueueHandler
{
    public async Task Handle(Message message)
    {
        Console.WriteLine($"[1] - Consume message : {message.Id}");
        await Task.FromResult(0);
    }
}

public class MessageHandler2 : IVineQueueHandler
{
    public async Task Handle(Message message)
    {
        Console.WriteLine($"[2] Consume message : {message.Id}");
        await Task.FromResult(0);
    }
}

public class MessageHandler3 : IVineQueueHandler
{
    public void Handle(Message message)
    {
        Console.WriteLine($"[3] Consume message : {message.Id}");
    }
}

public class MessageHandler4 : IVineQueueHandler
{
    public async Task Handle(Message message)
    {
        await Task.Delay(2000);
        Console.WriteLine($"[4] Consume message : {message.Id}");
    }
}
