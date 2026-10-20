namespace Calculator.Models;

public sealed record CalculationRequest
{
    public required Guid RequestId { get; init; }
    public required double Operand1 { get; init; }
    public required double Operand2 { get; init; }
    public required Operation Operation { get; init; }
    public required DateTimeOffset SentAtUtc { get; init; }
}
