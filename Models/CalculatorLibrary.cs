namespace Calculator.Models;

public static class CalculatorLibrary
{
    public static double CalculateOperation(double num1, double num2, Operation operation) => operation switch
    {
        Operation.Add => num1 + num2,
        Operation.Subtract => num1 - num2,
        Operation.Multiply => num1 * num2,
        Operation.Divide => num1 / num2,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
}
