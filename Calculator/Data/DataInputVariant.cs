using Calculator.Models;

namespace Calculator.Data;

public sealed class DataInputVariant
{
    public double Operand_1 { get; set; }
    public double Operand_2 { get; set; }
    public Operation Type_operation { get; set; }
}
