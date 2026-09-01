[![NuGet](https://img.shields.io/nuget/v/Vine.Q.svg)](https://www.nuget.org/packages/Vine.Q/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Vine.Q.svg)](https://www.nuget.org/packages/Vine.Q/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ellefry/Vine.Q/blob/main/LICENSE)

# Vine.Q

Vine.Q is a lightweight, high-performance in-memory message queue for C# applications.

It is designed for asynchronous message processing within a single process.

## Features

- In-memory FIFO message queue
- Support for multiple independent queues
- Asynchronous message handling
- Configurable queue capacity
- Configurable maximum concurrency
- Failure retry support
- Configurable retry delay
- Message processing event callbacks
- Supports .NET 10

> Vine.Q is intended for single-process scenarios. It does not provide cross-process messaging or message persistence.

## Installation

```bash
dotnet add package Vine.Q
```

## Quick Start

### 1. Define a message and handler

```csharp
using Vine.Q;

public sealed class Message
{
    public string? Id { get; set; }
}

public sealed class MessageHandler : IVineQueueHandler<Message>
{
    public async Task Handle(Message message)
    {
        Console.WriteLine($"Consumed message: {message.Id}");
        await Task.CompletedTask;
    }
}
```

### 2. Register a queue

```csharp
using Microsoft.Extensions.DependencyInjection;
using Vine.Q;

var services = new ServiceCollection();

services.AddVineQueue<Message, MessageHandler>(
    queue: "local",
    capacity: 5_000);

using var serviceProvider = services.BuildServiceProvider();
```

`queue` specifies the queue name, and `capacity` specifies the maximum number of messages that the queue can hold.

You can also register the default queue:

```csharp
services.AddDefaultVineQueue<Message, MessageHandler>();
```

The default queue configuration is:

- Queue name: `local`
- Queue capacity: `2_000`

### 3. Publish a message

```csharp
var publisher = serviceProvider.GetRequiredService<IVineQueuePublisher>();

await publisher.PublishAsync(
    new Message { Id = "demo" },
    queue: "local");
```

When using the default queue, the queue name can be omitted:

```csharp
await publisher.PublishAsync(new Message { Id = "demo" });
```

### 4. Try to publish a message

`TryPublishAsync` returns a Boolean value indicating whether the message was successfully added to the queue:

```csharp
var published = await publisher.TryPublishAsync(
    new Message { Id = "demo" },
    queue: "local");

if (!published)
{
    Console.WriteLine("The queue is full.");
}
```

## Configure Queue Options

Use `VineQueueOptions<T>` to configure concurrency, retries, retry delays, and event callbacks:

```csharp
var options = new VineQueueOptions<Message>
{
    MaxConcurrency = 4,
    MaxRetryCount = 3,
    RetryDelay = TimeSpan.FromSeconds(1),
    OnFailureAsync = context =>
    {
        Console.WriteLine(
            $"Message failed on attempt {context.Attempt}: " +
            $"{context.Exception.Message}");

        return Task.CompletedTask;
    },
    OnEvent = queueEvent =>
    {
        Console.WriteLine($"Queue event: {queueEvent.Kind}");
    }
};

services.AddVineQueue<Message, MessageHandler>(
    queue: "local",
    capacity: 5_000,
    options);
```

## Register Multiple Queues

Each queue can have its own message type and handler:

```csharp
services.AddVineQueue<Message, MessageHandler>(
    queue: "messages",
    capacity: 1_000);

services.AddVineQueue<Notification, NotificationHandler>(
    queue: "notifications",
    capacity: 500);
```

Publish to a specific queue:

```csharp
await publisher.PublishAsync(
    new Notification { Text = "Hello" },
    queue: "notifications");
```

## API

### `IVineQueuePublisher`

```csharp
public interface IVineQueuePublisher
{
    Task PublishAsync<T>(
        T message,
        string queue = "local",
        CancellationToken cancellationToken = default);

    ValueTask<bool> TryPublishAsync<T>(
        T message,
        string queue = "local");
}
```

### `IVineQueueHandler<T>`

```csharp
public interface IVineQueueHandler<T>
{
    Task Handle(T message);
}
```

## Important Notes

- Queues and messages are stored only in the current process memory.
- Unprocessed messages are lost when the application stops or restarts.
- Each queue can have only one registered message handler.
- Message handlers are registered as keyed singletons.
- Queue names cannot be null, empty, or whitespace.
- Queue capacity must be greater than `0`.
- Avoid using excessively large queue capacities, such as `int.MaxValue`.
- For reliable delivery, persistence, or cross-process communication, consider using RabbitMQ, Azure Service Bus, or another external messaging system.

## Sample Project

A sample application is available at:

```text
src/Sample.App
```

## License

This project is licensed under the MIT License.
