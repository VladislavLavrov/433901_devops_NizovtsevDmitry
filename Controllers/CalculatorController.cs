using System.Globalization;
using System.Text.Json;
using Calculator.Data;
using Calculator.Models;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Controllers;

public sealed class CalculatorController(
    CalculatorContext context,
    KafkaProducerService<Null, string> producer,
    IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index() => View(await context.DataInputVariants
        .AsNoTracking().OrderByDescending(x => x.ID_DataInputVariant).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
    {
        var data = new DataInputVariant
        {
            Operand_1 = num1.ToString("R", CultureInfo.InvariantCulture),
            Operand_2 = num2.ToString("R", CultureInfo.InvariantCulture),
            Type_operation = operation.ToString()
        };
        await producer.ProduceAsync(configuration["Kafka:TopicName"]!,
            new Message<Null, string> { Value = JsonSerializer.Serialize(data) });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Callback([FromBody] DataInputVariant data)
    {
        context.DataInputVariants.Add(data);
        await context.SaveChangesAsync();
        return Ok();
    }

    public IActionResult Error() => View();
}
