using System.Globalization;
using Calculator.Data;
using Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Controllers;

public sealed class CalculatorController(CalculatorContext context) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new CalculatorInput());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(CalculatorInput input)
    {
        if (!ModelState.IsValid) return View("Index", input);
        var a = input.Num1!.Value;
        var b = input.Num2!.Value;
        if (!double.IsFinite(a) || !double.IsFinite(b) || !Enum.IsDefined(input.Operation))
        {
            input.Error = "Проверьте числа и выбранную операцию.";
            return View("Index", input);
        }
        if (input.Operation == Operation.Divide && b == 0)
        {
            input.Error = "Деление на ноль невозможно.";
            return View("Index", input);
        }
        var result = input.Operation switch
        {
            Operation.Add => a + b,
            Operation.Subtract => a - b,
            Operation.Multiply => a * b,
            Operation.Divide => a / b,
            _ => throw new ArgumentOutOfRangeException(nameof(input.Operation))
        };
        if (!double.IsFinite(result))
        {
            input.Error = "Результат выходит за допустимый диапазон.";
            return View("Index", input);
        }
        context.DataInputVariants.Add(new DataInputVariant
        {
            Operand_1 = a.ToString("R", CultureInfo.InvariantCulture),
            Operand_2 = b.ToString("R", CultureInfo.InvariantCulture),
            Type_operation = input.Operation.ToString(),
            Result = result.ToString("R", CultureInfo.InvariantCulture)
        });
        await context.SaveChangesAsync();
        input.Result = result;
        return View("Index", input);
    }

    [HttpGet]
    public async Task<IActionResult> History() => View(await context.DataInputVariants
        .AsNoTracking().OrderByDescending(x => x.ID_DataInputVariant).Take(100).ToListAsync());

    [HttpGet]
    public IActionResult Error() => View();
}
