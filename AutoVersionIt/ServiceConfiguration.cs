using AutoVersionIt.Patches;
using AutoVersionIt.Sources;
using AutoVersionIt.Sources.Configuration;
using AutoVersionIt.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoVersionIt;

public static class ServiceConfiguration
{
    private static readonly Dictionary<string, Type> AvailableSources = new()
    {
        { "file", typeof(FileBasedSimpleVersionControl) },
        { "env", typeof(EnvironmentVariableVersionControl) },
        { "git", typeof(GitTagVersionControl) }
    };
    private static readonly Dictionary<string, Type> AvailableStrategies = new()
    {
        { "simple", typeof(SimpleCanonicalVersioning) },
        { "rc", typeof(ReleaseCandidateVersioning) },
    };
    private static readonly Dictionary<string, Type> AvailableTargets = new()
    {
        { "netfx", typeof(NetFxVersionPatcher) },
        { "netcore", typeof(NetCoreVersionPatcher) },
        { "nuspec", typeof(NuspecVersionPatcher) },
        { "text", typeof(TextFilePatcher) }
    };
    
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var source = configuration.GetValue("source", string.Empty);
        if (string.IsNullOrWhiteSpace(source)) throw new InvalidOperationException("No 'source' specified in config file.");
        var strategy = configuration.GetValue("strategy", string.Empty);
        if (string.IsNullOrWhiteSpace(strategy)) throw new InvalidOperationException("No 'strategy' specified in config file.");
        var targets = configuration.GetSection("targets").GetChildren().Select(x => x.Get<string>())
            .ToList()
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var patchers = configuration.GetSection("patchers").GetChildren().Select(x => x.Get<string>())
            .ToList()
            .Distinct(StringComparer.OrdinalIgnoreCase);

        AddConfigurations(configuration, services);
        AddSourceByName(source, services);
        AddStrategyByName(strategy, services);
        
        foreach (var target in targets)
        {
            if (string.IsNullOrWhiteSpace(target)) continue;
            AddTargetByName(target, services);
        }
        
        foreach (var patcher in patchers)
        {
            if (string.IsNullOrWhiteSpace(patcher)) continue;
            AddPatcherByName(patcher, services);
        }
        
        return services;
    }

    private static void AddSourceByName(string source, IServiceCollection services)
    {
        if (!AvailableSources.TryGetValue(source, out var sourceType))
            throw new InvalidOperationException($"Unknown source '{source}' specified in config file.");
        
        services.AddSingleton(sourceType);
    }
    
    private static void AddStrategyByName(string strategy, IServiceCollection services)
    {
        if (!AvailableStrategies.TryGetValue(strategy, out var strategyType))
            throw new InvalidOperationException($"Unknown strategy '{strategy}' specified in config file.");
        
        services.AddSingleton(strategyType);
    }
    
    private static void AddTargetByName(string target, IServiceCollection services)
    {
        if (!AvailableTargets.TryGetValue(target, out var targetType))
            throw new InvalidOperationException($"Unknown target '{target}' specified in config file.");
        
        services.AddSingleton(targetType);
    }
    
    private static void AddPatcherByName(string patcher, IServiceCollection services)
    {
        if (!AvailableTargets.TryGetValue(patcher, out var patcherType))
            throw new InvalidOperationException($"Unknown patcher '{patcher}' specified in config file.");
        
        services.AddSingleton(patcherType);
    }

    private static void AddConfigurations(IConfiguration configuration, IServiceCollection services)
    {
        var versionEnv = configuration.GetValue("versionEnv", "VERSION");
        var versionFile = configuration.GetValue("versionFile", "version.txt");
        
        services.AddSingleton(() => new EnvironmentVariableVersionControlConfig(versionEnv));
        services.AddSingleton(() => new FileBasedSimpleVersionControlConfig(versionFile));
    }
}