using Calculator.Data;

namespace Calculator.Models;

public sealed record CalculationResultViewModel(Guid RequestId, DataInputVariant? Record);
