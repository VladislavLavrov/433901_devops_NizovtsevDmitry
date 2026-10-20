using Calculator.Data;
using Calculator.Models;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Controllers;

public sealed class CalculatorController(
    CalculatorContext context,
    KafkaProducerService producer,
    ILogger<CalculatorController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new CalculatorInput());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(CalculatorInput input, CancellationToken token)
    {
        if (!ModelState.IsValid) return View("Index", input);
        input.Error = CalculatorLibrary.Validate(input.Num1!.Value, input.Num2!.Value, input.Operation);
        if (input.Error is not null) return View("Index", input);
        var request = new CalculationRequest
        {
            RequestId = Guid.NewGuid(),
            Operand1 = input.Num1.Value,
            Operand2 = input.Num2.Value,
            Operation = input.Operation,
            SentAtUtc = DateTimeOffset.UtcNow
        };
        try { await producer.SendRequestAsync(request, token); }
        catch (KafkaException error)
        {
            logger.LogError(error, "Kafka delivery failed for {RequestId}", request.RequestId);
            input.Error = "Не удалось подтвердить отправку в Kafka. Проверьте историю перед повторной отправкой.";
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("Index", input);
        }
        // Calculation and persistence happen only in KafkaConsumerService.
        return RedirectToAction(nameof(Result), new { id = request.RequestId });
    }

    [HttpGet]
    public async Task<IActionResult> Result(Guid id, CancellationToken token)
    {
        var key = id.ToString();
        var row = await context.DataInputVariants.AsNoTracking()
            .SingleOrDefaultAsync(x => x.RequestId == key, token);
        return View(new CalculationResultViewModel(id, row));
    }

    [HttpGet]
    public async Task<IActionResult> Status(Guid id, CancellationToken token)
    {
        var key = id.ToString();
        var row = await context.DataInputVariants.AsNoTracking()
            .SingleOrDefaultAsync(x => x.RequestId == key, token);
        return Json(new { requestId = key, completed = row is not null, result = row?.Result, databaseId = row?.ID_DataInputVariant });
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken token) => View(await context.DataInputVariants
        .AsNoTracking().OrderByDescending(x => x.ID_DataInputVariant).Take(100).ToListAsync(token));

    [HttpGet]
    public IActionResult Error() => View();
}
