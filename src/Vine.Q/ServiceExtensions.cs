using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Vine.Q;

public static class ServiceExtensions
{
  /// <summary>
  /// Add default queue name equals local and capacity equals DEFAULT_CAPACITY(2000) and the handler without return type 
  /// </summary>
  /// <typeparam name="T">Message type, parameter of a handler</typeparam>
  /// <typeparam name="THandler">Implementation of a handler</typeparam>
  /// <returns>IServiceCollection</returns>
  public static IServiceCollection AddDefaultVineQueue<T, THandler>(this IServiceCollection services, VineQueueOptions<T>? options = null)
      where THandler : class, IVineQueueHandler<T>
  {
    services.AddVineQueue<T, THandler>(Constants.DEFAULT_QUEUE, Constants.DEFAULT_QUEUE_SIZE, options);
    return services;
  }

  /// <summary>
  /// Add Vine queue and the handler without return
  /// </summary>
  /// <typeparam name="T">Message type, parameter of a handler</typeparam>
  /// <typeparam name="THandler">Implementation of a handler</typeparam>
  /// <param name="queue">Queue name</param>
  /// <param name="capacity">Queue capacity</param>
  /// <returns></returns>
  public static IServiceCollection AddVineQueue<T, THandler>(this IServiceCollection services, string queue, int capacity, VineQueueOptions<T>? options = null)
      where THandler : class, IVineQueueHandler<T>
  {
    ValidateAndRegisterQueue<T>(services, queue, capacity);
    services.AddCommons();
    services.TryAddKeyedSingleton<IVineQueueHandler<T>, THandler>(queue);

    services.AddSingleton(sp =>
    {
      var builder = sp.GetRequiredService<IVineQueueBuilder>();
      var handler = sp.GetRequiredKeyedService<IVineQueueHandler<T>>(queue);
      var q = builder.Create<T>(queue, capacity, options ?? new VineQueueOptions<T>(), handler.Handle);
      return q;
    });

    return services;
  }

  private static void AddCommons(this IServiceCollection services)
  {
    services.TryAddSingleton<IVineQueueBuilder, VineQueueBuilder>();
    services.TryAddSingleton<IVineWorkQueueAcquirer, VineWorkQueueAcquirer>();
    services.TryAddSingleton<IVineQueuePublisher, VineQueuePublisher>();
  }

  private static void ValidateAndRegisterQueue<T>(IServiceCollection services, string queue, int capacity)
  {
    ArgumentNullException.ThrowIfNull(services);
    if (string.IsNullOrWhiteSpace(queue))
    {
      throw new ArgumentException("Queue name cannot be null or whitespace.", nameof(queue));
    }
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be greater than zero.");
    }

    if (services.Any(descriptor =>
            descriptor.ServiceType == typeof(VineQueueRegistration) &&
            descriptor.ImplementationInstance is VineQueueRegistration registration &&
            string.Equals(registration.Name, queue, StringComparison.Ordinal)))
    {
      throw new InvalidOperationException($"A Vine.Q queue named '{queue}' is already registered.");
    }

    services.AddSingleton(new VineQueueRegistration(queue, typeof(T)));
  }
}

internal sealed class VineQueueRegistration
{
  public VineQueueRegistration(string name, Type messageType)
  {
    Name = name;
    MessageType = messageType;
  }

  public string Name { get; }

  public Type MessageType { get; }
}
