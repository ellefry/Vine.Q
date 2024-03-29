[![Static Badge](https://img.shields.io/badge/nuget-1.0.1-blue)](https://www.nuget.org/packages/Vine.Q/)
# Vine.Q
## A Lightweight Message Queue
Vine.Q is an implementation of a lightweight memory message queue in C#. It serves as an efficient and easy-to-use mechanism for managing and processing queued data within a C# application. This queue implementation is designed to be lightweight, making it suitable for scenarios where a simple and fast in-memory queue is needed. Vine.Q enables developers to seamlessly handle and organize data in a first-in, first-out (FIFO) fashion, providing a reliable solution for various queuing requirements within C# applications.

## How To Use
* Define message handler
```
public class Message
{
    public string? Id { get; set; }
}

public class MessageHandler : IVineQueueHandlerWithReturn<Message, Task>
{
    public async Task Handle(Message message)
    {
        Console.WriteLine($"Consumed message : {message.Id}");
        await Task.CompletedTask;
    }
}
```
> if the handler has no return, use IVineQueueHandler\<T\>

* Add services
```
services.AddVineQueueWithReturn<Message, Task, MessageHandler>("local", 5_000);
```
> **local** means queue name and **5_000** mean queue size

* Inject publish service
```
private readonly IVineQueuePublisher _publisher

public Constructor(IVineQueuePublisher publisher)
{
    _publisher = publisher;
    ...
}

public async Task PublishMessage()
{
    ...
    _publisher.Publish(new Message { Id = "demo" }, "local");
    ...
}
```
## Limitations
* MessageHandler will be registered as singleton service
* 1 queue 1 handler
* Do not make your queue size too big, e.g. int.MaxValue
