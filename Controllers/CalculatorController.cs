using Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers;

public sealed class CalculatorController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new CalculatorInput());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Calculate(CalculatorInput input)
    {
        if (!ModelState.IsValid)
            return View("Index", input);

        var a = input.Num1!.Value;
        var b = input.Num2!.Value;
        if (input.Operation == Operation.Divide && b == 0)
        {
            input.Error = "Деление на ноль невозможно.";
            return View("Index", input);
        }

        input.Result = input.Operation switch
        {
            Operation.Add => a + b,
            Operation.Subtract => a - b,
            Operation.Multiply => a * b,
            Operation.Divide => a / b,
            _ => throw new ArgumentOutOfRangeException(nameof(input.Operation))
        };
        return View("Index", input);
    }

    [HttpGet]
    public IActionResult Error() => View();
}
