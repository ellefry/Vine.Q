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
    public static IServiceCollection AddDefaultVineQueue<T, THandler>(this IServiceCollection services)
        where THandler : class, IVineQueueHandler<T>
    {
        services.AddVineQueue<T, THandler>(Constants.DEFAULT_QUEUE, Constants.DEFAULT_QUEUE_SIZE);
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
    public static IServiceCollection AddVineQueue<T, THandler>(this IServiceCollection services, string queue, int capacity)
        where THandler : class, IVineQueueHandler<T>
    {
        services.AddCommons();
        services.TryAddKeyedSingleton<IVineQueueHandler<T>, THandler>(queue);

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IVineQueueBuilder>();

        var handler = sp.GetRequiredKeyedService<IVineQueueHandler<T>>(queue);
        builder.Create<T>(queue, capacity, handler.Handle);

        return services;
    }

    /// <summary>
    /// Add default queue name equals local and capacity equals DEFAULT_CAPACITY(2000) and the handler with TReturn type
    /// </summary>
    /// <typeparam name="T">Message type, parameter of a handler</typeparam>
    /// <typeparam name="TReturn">Return type, return type of a handler</typeparam>
    /// <typeparam name="THandler">Implementation of a handler</typeparam>
    /// <param name="queue">Queue name</param>
    /// <param name="capacity">Queue capacity</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDefaultVineQueueWithReturn<T, TReturn, THandler>(this IServiceCollection services)
        where THandler : class, IVineQueueHandlerWithReturn<T, TReturn>
    {
        services.AddVineQueueWithReturn<T, TReturn, THandler>(Constants.DEFAULT_QUEUE, Constants.DEFAULT_QUEUE_SIZE);
        return services;
    }

    /// <summary>
    /// Add Vine queue
    /// </summary>
    /// <typeparam name="T">Message type, parameter of a handler</typeparam>
    /// <typeparam name="TReturn">Return type, return type of a handler</typeparam>
    /// <typeparam name="THandler">Implementation of a handler</typeparam>
    /// <param name="queue">Queue name</param>
    /// <param name="capacity">Queue capacity</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddVineQueueWithReturn<T, TReturn, THandler>(this IServiceCollection services, string queue, int capacity)
        where THandler : class, IVineQueueHandlerWithReturn<T, TReturn>
    {
        services.AddCommons();
        services.TryAddKeyedSingleton<IVineQueueHandlerWithReturn<T, TReturn>, THandler>(queue);

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IVineQueueBuilder>();

        var handler = sp.GetRequiredKeyedService<IVineQueueHandlerWithReturn<T, TReturn>>(queue);
        builder.Create<T, TReturn>(queue, capacity, handler.Handle);

        return services;
    }

    private static void AddCommons(this IServiceCollection services)
    {
        services.TryAddSingleton<IVineQueueBuilder, VineQueueBuilder>();
        services.TryAddSingleton<IVineWorkQueueAcquirer, VineQueueBuilder>();
        services.TryAddSingleton<IVineQueuePublisher, VineQueuePublisher>();
    }
}
