using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calculator.Data;

// EF CLI / Visual Studio can create the context without starting the web application.
public sealed class CalculatorContextFactory : IDesignTimeDbContextFactory<CalculatorContext>
{
    public CalculatorContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=5043;Database=13-WebCalculatorDb;User=calculator13;Password=SET_IN_ENV;";
        var options = new DbContextOptionsBuilder<CalculatorContext>()
            .UseMySql(connection, new MariaDbServerVersion(new Version(10, 11, 0)))
            .Options;
        return new CalculatorContext(options);
    }
}
