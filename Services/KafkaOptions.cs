namespace Calculator.Services;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "kafka:9092";
    public string GroupId { get; set; } = "nizovtsev13-calculator-workers";
    public string RequestTopic { get; set; } = "nizovtsev13-calculation-requests";
    public string ResultTopic { get; set; } = "nizovtsev13-calculation-results";
    public string ErrorTopic { get; set; } = "nizovtsev13-calculation-errors";
}
