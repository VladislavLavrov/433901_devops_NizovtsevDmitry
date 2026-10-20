using System.Text.Json;
using Calculator.Models;
using Confluent.Kafka;

namespace Calculator.Services;

public sealed class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(KafkaOptions options, ILogger<KafkaProducerService> logger)
    {
        _options = options;
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            ClientId = "nizovtsev13-calculator-producer",
            EnableIdempotence = true,
            Acks = Acks.All,
            MessageTimeoutMs = 10000
        }).Build();
    }

    public Task SendRequestAsync(CalculationRequest request, CancellationToken token) =>
        SendJsonAsync(_options.RequestTopic, request.RequestId.ToString(), request, token);

    public async Task SendJsonAsync<T>(string topic, string key, T payload, CancellationToken token)
    {
        var delivery = await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(payload, KafkaJson.Options)
        }, token);
        _logger.LogInformation("Produced key={Key}, topic={Topic}, partition={Partition}, offset={Offset}",
            key, delivery.Topic, delivery.Partition.Value, delivery.Offset.Value);
    }

    public void Dispose()
    {
        try { _producer.Flush(TimeSpan.FromSeconds(5)); }
        finally { _producer.Dispose(); }
    }
}
