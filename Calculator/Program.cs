using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Пункт 3.5.5 пособия: метрики ASP.NET Core и экспорт в Prometheus.
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddPrometheusExporter();
        metrics.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "Microsoft.AspNetCore.Http.Connections");
        metrics.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[]
                {
                    0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1,
                    0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                }
            });
    });

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Calculator/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.MapPrometheusScrapingEndpoint();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");
app.Run();
