using Microsoft.Extensions.DependencyInjection;
using Vine.Q;

namespace Sample.App;

internal static class Program
{
  static async Task Main(string[] args)
  {
    var services = new ServiceCollection();
    const int messagesPerQueue = 10;
    const int queueCount = 4;
    var consumedMessages = 0;
    var allMessagesConsumed = new TaskCompletionSource(
        TaskCreationOptions.RunContinuationsAsynchronously);
    var options = new VineQueueOptions<Message>
    {
      OnEvent = queueEvent =>
      {
        if (queueEvent.Kind == VineQueueEventKind.Succeeded &&
                  Interlocked.Increment(ref consumedMessages) == messagesPerQueue * queueCount)
        {
          allMessagesConsumed.TrySetResult();
        }
      }
    };

    services.AddDefaultVineQueue<Message, MessageHandler>(options);

    services.AddVineQueue<Message, MessageHandler2>("local2", 100, options);

    services.AddVineQueue<Message, MessageHandler3>("local3", 100, options);

    services.AddVineQueue<Message, MessageHandler4>("local4", 100, options);

    using var serviceProvider = services.BuildServiceProvider();
    var publisher = serviceProvider.GetRequiredService<IVineQueuePublisher>();

    var publishTasks = Enumerable.Range(1, messagesPerQueue).Select(async idx =>
{
  await publisher.PublishAsync(new Message { Id = idx.ToString() });
  await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local2");
  await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local3");
  await publisher.PublishAsync(new Message { Id = idx.ToString() }, "local4");
});

    await Task.WhenAll(publishTasks);
    await allMessagesConsumed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    Console.WriteLine($"Consumed messages: {consumedMessages}");
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
    await Task.Delay(1000);
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
