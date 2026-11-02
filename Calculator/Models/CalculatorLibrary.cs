namespace Calculator.Models;

public enum Operation { Add, Subtract, Multiply, Divide }

public static class CalculatorLibrary
{
    public static double CalculateOperation(double num1, double num2, Operation operation)
    {
        return operation switch
        {
            Operation.Add => num1 + num2,
            Operation.Subtract => num1 - num2,
            Operation.Multiply => num1 * num2,
            Operation.Divide when num2 != 0 => num1 / num2,
            Operation.Divide => throw new DivideByZeroException("Деление на ноль невозможно."),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }
}
