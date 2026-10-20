using Confluent.Kafka;

namespace Calculator.Services;

public sealed class KafkaProducerHandler : IDisposable
{
    private readonly IProducer<byte[], byte[]> producer;
    public KafkaProducerHandler(IConfiguration configuration)
    {
        var config = new ProducerConfig();
        configuration.GetSection("Kafka:ProducerSettings").Bind(config);
        producer = new ProducerBuilder<byte[], byte[]>(config).Build();
    }
    public Handle Handle => producer.Handle;
    public void Dispose()
    {
        producer.Flush(TimeSpan.FromSeconds(5));
        producer.Dispose();
    }
}
