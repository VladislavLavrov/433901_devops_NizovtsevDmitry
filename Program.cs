using Calculator.Data;
using Calculator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection.");
builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(connection, new MariaDbServerVersion(new Version(10, 11, 0))));
var kafka = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>() ?? new KafkaOptions();
if (new[] { kafka.BootstrapServers, kafka.GroupId, kafka.RequestTopic, kafka.ResultTopic, kafka.ErrorTopic }
    .Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("Check Kafka settings.");
builder.Services.AddSingleton(kafka);
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CalculatorContext>().Database.MigrateAsync();
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Calculator/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Calculator}/{action=Index}/{id?}");
app.Run();
