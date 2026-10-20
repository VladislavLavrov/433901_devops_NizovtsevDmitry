namespace Calculator.Models;

public static class CalculatorLibrary
{
    public static string? Validate(double a, double b, Operation operation)
    {
        if (!double.IsFinite(a) || !double.IsFinite(b)) return "Введите конечные числа.";
        if (!Enum.IsDefined(operation)) return "Неизвестная операция.";
        if (operation == Operation.Divide && b == 0) return "Деление на ноль невозможно.";
        return null;
    }

    // Only the Kafka consumer calls the calculation method.
    public static double CalculateOperation(double a, double b, Operation operation)
    {
        var error = Validate(a, b, operation);
        if (error is not null) throw new ArgumentException(error);
        var result = operation switch
        {
            Operation.Add => a + b,
            Operation.Subtract => a - b,
            Operation.Multiply => a * b,
            Operation.Divide => a / b,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        if (!double.IsFinite(result)) throw new ArithmeticException("Результат выходит за допустимый диапазон.");
        return result;
    }
}
