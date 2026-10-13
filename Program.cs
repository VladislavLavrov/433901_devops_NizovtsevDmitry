using Calculator.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection.");
builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(connection, new MariaDbServerVersion(new Version(10, 11, 0))));

var app = builder.Build();
// Single web instance in this practical exercise. The database healthcheck runs first.
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
    await database.Database.MigrateAsync();
}
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Calculator/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Calculator}/{action=Index}/{id?}");
app.Run();
