using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Vine.Q;

public static class ServicesExtensions
{
    /// <summary>
    /// Add default queue name equals local and capacity equals DEFAULT_CAPACITY(2000) and the handler without return 
    /// </summary>
    /// <typeparam name="T">Message type, parameter of a handler</typeparam>
    /// <typeparam name="THandler">Implementation of a handler</typeparam>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDefaultVineQueue<T, THandler>(this IServiceCollection services)
        where THandler : class, IVineQueueHandler
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
        where THandler : class, IVineQueueHandler
    {
        services.AddCommons();

        services.TryAddSingleton<THandler>();
        var handler = CreateHandler<T>(typeof(THandler));

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IVineQueueBuilder>();
        builder.Create<T>(queue, capacity, handler);

        return services;
    }

    /// <summary>
    /// Add default queue name equals local and capacity equals DEFAULT_CAPACITY(2000) and the handler with return
    /// </summary>
    /// <typeparam name="T">Message type, parameter of a handler</typeparam>
    /// <typeparam name="TReturn">Return type, return type of a handler</typeparam>
    /// <typeparam name="THandler">Implementation of a handler</typeparam>
    /// <param name="queue">Queue name</param>
    /// <param name="capacity">Queue capacity</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDefaultVineQueueWithReturn<T, TReturn, THandler>(this IServiceCollection services)
        where THandler : class, IVineQueueHandler
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
        where THandler : class, IVineQueueHandler
    {
        services.AddCommons();
        services.TryAddSingleton<THandler>();
        var handler = CreateHandler<T, TReturn>(typeof(THandler));

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IVineQueueBuilder>();
        builder.Create<T, TReturn>(queue, capacity, handler);

        return services;
    }

    private static IServiceCollection AddCommons(this IServiceCollection services)
    {
        services.TryAddSingleton<IVineQueueBuilder, VineQueueBuilder>();
        services.TryAddSingleton<IVineWorkQueueAcquirer, VineQueueBuilder>();
        services.TryAddSingleton<IVineQueuePublisher, VineQueuePublisher>();

        return services;
    }

    private static Func<T, TReturn> CreateHandler<T, TReturn>(Type type)
    {
        var methodInfos = type.GetMethods().ToArray();
        var method = methodInfos.First(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(T)
            && m.ReturnParameter.ParameterType == typeof(TReturn));
        var handler = (Func<T, TReturn>)Delegate.CreateDelegate(typeof(Func<T, TReturn>), null, method!);
        return handler;
    }

    private static Action<T> CreateHandler<T>(Type type)
    {
        var methodInfos = type.GetMethods().ToArray();
        var method = methodInfos.First(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(T));
        var handler = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), null, method!);
        return handler;
    }
}
