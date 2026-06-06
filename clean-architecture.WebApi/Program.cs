using clean_architecture.application;
using clean_architecture.infrastructure;
using clean_architecture.WebApi;
using clean_architecture.WebApi.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog programmatically — avoids fragile JSON array merging between appsettings and Key Vault
Serilog.ILogger logger;
try
{
    var logConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

    // Determine the connection string based on environment
    string? dbConnectionString = null;

    if (builder.Environment.IsDevelopment())
    {
        // Development always uses LocalDb
        dbConnectionString = builder.Configuration.GetConnectionString("LocalDb");
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            Console.WriteLine("Warning: LocalDb connection string not found in development. Serilog will use console-only logging.");
        }
    }
    else
    {
        // Production/Staging must use ProdDb - fail explicitly if missing
        dbConnectionString = builder.Configuration.GetConnectionString("ProdDb");
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            throw new InvalidOperationException(
                "Missing required connection string 'ProdDb' in production environment. " +
                "Ensure the 'ConnectionStrings__ProdDb' is configured in Azure Key Vault or appsettings.Production.json");
        }
    }

    if (!string.IsNullOrWhiteSpace(dbConnectionString))
    {
        try
        {
            logConfig = logConfig.WriteTo.MSSqlServer(
                connectionString: dbConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true,
                    SchemaName = "dbo"
                });

            logger = logConfig.CreateLogger();
            logger.Information("Serilog configured for {Environment} (Console + MSSqlServer)",
                builder.Environment.EnvironmentName);
            logger.Information("Database connection: {ConnectionString}",
                dbConnectionString.Replace("Password=", "Password=[REDACTED]"));
        }
        catch (Exception sqlEx)
        {
            Console.WriteLine($"Failed to configure MSSqlServer sink: {sqlEx.Message}");
            logger = logConfig.CreateLogger();
            logger.Error(sqlEx, "Failed to add MSSqlServer sink. Using console-only.");
        }
    }
    else
    {
        logger = logConfig.CreateLogger();
        logger.Warning("No database connection string configured. Using console-only logging.");
    }
}
catch (Exception ex)
{
    logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    logger.Error(ex, "Failed to configure Serilog. Using console-only fallback.");
}

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Host.UseSerilog(logger);

builder.Services
    .AddOpenApi()
    .AddResponseCompression()
    .AddApplication(builder.Configuration)
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRequestContextLogging();
app.UseSerilogRequestLogging();

app.UseResponseCompression();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Application API")
               .WithTheme(ScalarTheme.Default)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

app.MapEndpoints();


// Health check response writer
static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var response = new
    {
        status = report.Status.ToString(),
        timestamp = DateTime.UtcNow,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    };
    return context.Response.WriteAsJsonAsync(response);
}

app.Run();