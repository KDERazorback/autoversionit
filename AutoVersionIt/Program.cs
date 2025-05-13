using AutoVersionIt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var minimumLogLevel = LogLevel.Information;
if (args.Contains("-v", StringComparer.Ordinal)) minimumLogLevel = LogLevel.Debug;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(minimumLogLevel);
});

var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .FromEnvironmentSpecificJsonFile(environment)
    .Build();
    
var services = new ServiceCollection()
    .AddSingleton(configuration)
    .AddSingleton(loggerFactory)
    .ConfigureServices(configuration)
    .BuildServiceProvider();
    
new CliProcess(args, services)
    .Run();