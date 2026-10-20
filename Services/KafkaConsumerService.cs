using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Calculator.Data;
using Calculator.Models;
using Confluent.Kafka;

namespace Calculator.Services;

public sealed class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration configuration;
    private readonly IHttpClientFactory clientFactory;
    private readonly ILogger<KafkaConsumerService> logger;

    public KafkaConsumerService(IConfiguration configuration, IHttpClientFactory clientFactory,
        ILogger<KafkaConsumerService> logger)
    {
        this.configuration = configuration;
        this.clientFactory = clientFactory;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private async Task ConsumeLoop(CancellationToken token)
    {
        var settings = new ConsumerConfig();
        configuration.GetSection("Kafka:ConsumerSettings").Bind(settings);
        using var consumer = new ConsumerBuilder<Null, string>(settings).Build();
        consumer.Subscribe(configuration["Kafka:TopicName"]!);
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var message = consumer.Consume(token);
                    var data = JsonSerializer.Deserialize<DataInputVariant>(message.Message.Value)
                        ?? throw new JsonException("Empty calculation message.");
                    var result = CalculatorLibrary.CalculateOperation(
                        double.Parse(data.Operand_1!, CultureInfo.InvariantCulture),
                        double.Parse(data.Operand_2!, CultureInfo.InvariantCulture),
                        Enum.Parse<Operation>(data.Type_operation!));
                    data.Result = result.ToString("R", CultureInfo.InvariantCulture);
                    using var client = clientFactory.CreateClient();
                    using var response = await client.PostAsJsonAsync(
                        "http://localhost:5055/Calculator/Callback", data, token);
                    response.EnsureSuccessStatusCode();
                    logger.LogInformation("Message key: {Key}, value: {Value}",
                        message.Message.Key, message.Message.Value);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (ConsumeException error)
                {
                    logger.LogError(error, "Kafka consume error");
                    if (error.Error.IsFatal) break;
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Calculation/Callback failed");
                    break;
                }
            }
        }
        finally { consumer.Close(); }
    }
}
