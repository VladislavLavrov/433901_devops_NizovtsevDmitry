using Confluent.Kafka;

namespace Calculator.Services;

public sealed class KafkaProducerService<K, V> : IDisposable
{
    private readonly IProducer<K, V> producer;
    public KafkaProducerService(KafkaProducerHandler handler) =>
        producer = new DependentProducerBuilder<K, V>(handler.Handle).Build();
    public Task<DeliveryResult<K, V>> ProduceAsync(string topic, Message<K, V> message) =>
        producer.ProduceAsync(topic, message);
    public void Dispose() => producer.Dispose();
}
