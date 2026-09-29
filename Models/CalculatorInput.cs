using System.ComponentModel.DataAnnotations;

namespace Calculator.Models;

public enum Operation { Add, Subtract, Multiply, Divide }

public sealed class CalculatorInput
{
    [Required(ErrorMessage = "Введите первое число.")]
    public double? Num1 { get; set; }

    [Required(ErrorMessage = "Введите второе число.")]
    public double? Num2 { get; set; }

    public Operation Operation { get; set; }
    public double? Result { get; set; }
    public string? Error { get; set; }
}
