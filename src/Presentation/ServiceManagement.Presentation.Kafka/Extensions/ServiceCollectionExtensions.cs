using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceManagement.Presentation.Kafka.Consumer;
using ServiceManagement.Presentation.Kafka.Consumer.Handlers;
using ServiceManagement.Presentation.Kafka.Consumer.Services;
using ServiceManagement.Presentation.Kafka.Producer;
using ServiceManagement.Presentation.Kafka.Tools;
using Tasks.Kafka.Contracts;

namespace ServiceManagement.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection collection)
    {
        collection.AddScoped<IKafkaMessageHandler<FlightCreationKey,
            FlightCreationValue>, CreateFlightHandler>();
        return collection;
    }

    public static IServiceCollection AddKafkaConsumer<TKey, TValue>(
        this IServiceCollection collection,
        string optionsKey) where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        collection.AddKeyedSingleton<IDeserializer<TKey>, ProtobufSerializer<TKey>>(
            optionsKey);
        collection.AddKeyedSingleton<IDeserializer<TValue>, ProtobufSerializer<TValue>>(
            optionsKey);
        collection.AddHostedService(p
            => ActivatorUtilities.CreateInstance<MyBatchingKafkaConsumerService<TKey, TValue>>(p, optionsKey));
        return collection;
    }

    public static IServiceCollection AddKafkaProducer<TKey, TValue>(
        this IServiceCollection collection,
        string optionsKey) where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        collection.AddKeyedSingleton<ISerializer<TKey>, ProtobufSerializer<TKey>>(optionsKey);
        collection.AddKeyedSingleton<ISerializer<TValue>, ProtobufSerializer<TValue>>(optionsKey);
        collection.TryAddScoped<IKafkaProducer<TKey, TValue>>(
            p => ActivatorUtilities
                .CreateInstance<KafkaProducer<TKey, TValue>>(
                    p,
                    optionsKey));

        return collection;
    }
}