using System.Globalization;
using System.Text.Json;
using Calculator.Data;
using Calculator.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Services;

public sealed class KafkaConsumerService(
    IServiceScopeFactory scopes,
    KafkaOptions options,
    KafkaProducerService producer,
    ILogger<KafkaConsumerService> logger) : BackgroundService
{
    // Consume blocks a thread, so run it outside the web server startup thread.
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoopAsync(stoppingToken), stoppingToken);

    private async Task ConsumeLoopAsync(CancellationToken token)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.GroupId,
            ClientId = "nizovtsev13-calculator-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            MaxPollIntervalMs = 300000
        }).Build();
        consumer.Subscribe(options.RequestTopic);
        logger.LogInformation("Consumer started: topic={Topic}, group={Group}", options.RequestTopic, options.GroupId);
        try
        {
            while (!token.IsCancellationRequested)
            {
                var message = consumer.Consume(token);
                // Retry the SAME message, rather than reading and skipping to the next one.
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await ProcessAsync(message, token);
                        break;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
                    catch (Exception error)
                    {
                        logger.LogError(error, "Processing failed at {Offset}; retrying in 2 seconds", message.TopicPartitionOffset);
                        await Task.Delay(TimeSpan.FromSeconds(2), token);
                    }
                }
                token.ThrowIfCancellationRequested();
                // Commit only AFTER the DB write and result publication succeed.
                // A rebalance/commit failure stops this service; the uncommitted request is replayed on restart.
                consumer.Commit(message);
                logger.LogInformation("Committed {Offset}", message.TopicPartitionOffset);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally { consumer.Close(); }
    }

    private async Task ProcessAsync(ConsumeResult<string, string> message, CancellationToken token)
    {
        CalculationRequest request;
        try
        {
            request = JsonSerializer.Deserialize<CalculationRequest>(message.Message.Value, KafkaJson.Options)
                ?? throw new JsonException("Empty calculation request.");
            if (request.RequestId == Guid.Empty) throw new ArgumentException("RequestId is required.");
            var error = CalculatorLibrary.Validate(request.Operand1, request.Operand2, request.Operation);
            if (error is not null) throw new ArgumentException(error);
        }
        catch (Exception error) when (error is JsonException or ArgumentException)
        {
            // Malformed external messages go to a separate topic instead of blocking the queue forever.
            await producer.SendJsonAsync(options.ErrorTopic, message.Message.Key ?? "invalid", new
            {
                SourceTopic = message.Topic,
                Partition = message.Partition.Value,
                Offset = message.Offset.Value,
                RawValue = message.Message.Value,
                Error = error.Message
            }, token);
            logger.LogWarning("Rejected malformed request at {Offset}: {Error}", message.TopicPartitionOffset, error.Message);
            return;
        }

        var key = request.RequestId.ToString();
        await using var scope = scopes.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
        var row = await context.DataInputVariants.SingleOrDefaultAsync(x => x.RequestId == key, token);
        if (row is null)
        {
            string value;
            try
            {
                value = CalculatorLibrary.CalculateOperation(request.Operand1, request.Operand2, request.Operation)
                    .ToString("R", CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException error) { value = "Ошибка: " + error.Message; }
            row = new DataInputVariant
            {
                RequestId = key,
                Operand_1 = request.Operand1.ToString("R", CultureInfo.InvariantCulture),
                Operand_2 = request.Operand2.ToString("R", CultureInfo.InvariantCulture),
                Type_operation = request.Operation.ToString(),
                Result = value
            };
            context.DataInputVariants.Add(row);
            await context.SaveChangesAsync(token);
        }
        await producer.SendJsonAsync(options.ResultTopic, key, new
        {
            RequestId = key,
            DatabaseId = row.ID_DataInputVariant,
            row.Operand_1,
            row.Operand_2,
            row.Type_operation,
            row.Result
        }, token);
        logger.LogInformation("Processed request={RequestId}, result={Result}, DB id={Id}",
            key, row.Result, row.ID_DataInputVariant);
    }
}
