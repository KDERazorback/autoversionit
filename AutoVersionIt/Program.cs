using AutoVersionIt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var minimumLogLevel = LogLevel.Information;
if (args.Contains("-v", StringComparer.Ordinal)) minimumLogLevel = LogLevel.Debug;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(x =>
    {
        x.ColorBehavior = LoggerColorBehavior.Default;
        x.IncludeScopes = true;
        x.SingleLine = true;
    });
    builder.SetMinimumLevel(minimumLogLevel);
});

var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .FromEnvironmentSpecificJsonFile(environment)
    .Build();
    
var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddSingleton(loggerFactory)
    .ConfigureServices(configuration)
    .BuildServiceProvider();
    
new CliProcess(args, services)
    .Run();