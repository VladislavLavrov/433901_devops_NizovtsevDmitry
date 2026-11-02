using Calculator.Data;
using Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers;

public sealed class CalculatorController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(double num1, double num2, Operation operation)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Error = "Проверьте введённые числа.";
            return View();
        }

        var dataInputVariant = new DataInputVariant
        {
            Operand_1 = num1,
            Operand_2 = num2,
            Type_operation = operation
        };

        try
        {
            ViewBag.Result = CalculatorLibrary.CalculateOperation(
                dataInputVariant.Operand_1,
                dataInputVariant.Operand_2,
                dataInputVariant.Type_operation).ToString();
        }
        catch (DivideByZeroException exception)
        {
            ViewBag.Error = exception.Message;
        }
        return View();
    }

    [HttpGet]
    public IActionResult Error() => View();
}
