using AutoVersionIt.Interop;
using AutoVersionIt.Patches;
using AutoVersionIt.Patches.Configuration;
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
    private static readonly Dictionary<string, Type> AvailableTargets = new()
    {
        { "file", typeof(FileBasedSimpleVersionControl) },
        { "env", typeof(EnvironmentVariableVersionControl) },
        { "git", typeof(GitTagVersionControl) }
    };
    private static readonly Dictionary<string, Type> AvailablePatchers = new()
    {
        { "netfx", typeof(NetFxVersionPatcher) },
        { "netcore", typeof(NetCoreVersionPatcher) },
        { "nuspec", typeof(NuspecVersionPatcher) },
        { "text", typeof(TextFilePatcher) }
    };
    private static readonly Dictionary<string, Type> AvailableStrategies = new()
    {
        { "simple", typeof(SimpleCanonicalVersioning) },
        { "rc", typeof(ReleaseCandidateVersioning) },
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
        var patchers = configuration.GetSection("patch").GetChildren().Select(x => x.Get<string>())
            .ToList()
            .Distinct(StringComparer.OrdinalIgnoreCase);

        AddVersionReader(configuration, services);
        AddChildProcessFactory(configuration, services);
        
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

    private static void AddVersionReader(IConfiguration configuration, IServiceCollection services)
    {
        var suffix = configuration.GetValue("suffix", string.Empty);

        var reader = new VersionReader()
            .ThrowIfEmpty();

        if (!string.IsNullOrWhiteSpace(suffix))
            reader.WithDefaultFixedSuffix(suffix.Trim());
        
        services.AddSingleton(reader);
    }

    private static void AddChildProcessFactory(IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton<IChildProcessFactory, SimpleShellProcessFactory>();
    }

    private static void AddSourceByName(string source, IServiceCollection services)
    {
        if (!AvailableSources.TryGetValue(source, out var sourceType))
            throw new InvalidOperationException($"Unknown source '{source}' specified in config file.");
        
        services.AddSingleton(typeof(IVersionSource), sourceType);
    }
    
    private static void AddStrategyByName(string strategy, IServiceCollection services)
    {
        if (!AvailableStrategies.TryGetValue(strategy, out var strategyType))
            throw new InvalidOperationException($"Unknown strategy '{strategy}' specified in config file.");
        
        services.AddSingleton(typeof(IVersioningStrategy), strategyType);
    }
    
    private static void AddTargetByName(string target, IServiceCollection services)
    {
        if (!AvailableTargets.TryGetValue(target, out var targetType))
            throw new InvalidOperationException($"Unknown target '{target}' specified in config file.");
        
        services.AddSingleton(typeof(IVersionTarget), targetType);
    }
    
    private static void AddPatcherByName(string patcher, IServiceCollection services)
    {
        if (!AvailablePatchers.TryGetValue(patcher, out var patcherType))
            throw new InvalidOperationException($"Unknown patcher '{patcher}' specified in config file.");
        
        services.AddSingleton(typeof(IVersionPatcher), patcherType);
    }

    private static void AddConfigurations(IConfiguration configuration, IServiceCollection services)
    {
        var versionEnv = configuration.GetValue("versionEnv", "VERSION");
        var versionEnvFile = configuration.GetValue("versionEnvFile", ".version");
        var versionFile = configuration.GetValue("versionFile", "version.txt");
        
        services.AddSingleton(typeof(EnvironmentVariableVersionControlConfig), new EnvironmentVariableVersionControlConfig(versionEnv, versionEnvFile));
        services.AddSingleton(typeof(FileBasedSimpleVersionControlConfig), new FileBasedSimpleVersionControlConfig(versionFile));

        services.AddSingleton(typeof(NetCoreVersionPatcherConfig), new NetCoreVersionPatcherConfig()
            .InsertAttributesIfMissing()
            .PatchCSharpProjects()
            .PatchVbProjects()
            .Recursive()
            .DetectFileKindByExtension()
            .EnableGlobber());
        
        services.AddSingleton(typeof(NetFxVersionPatcherConfig), new NetFxVersionPatcherConfig()
            .InsertAttributesIfMissing()
            .PatchCSharpProjects()
            .PatchVbProjects()
            .CheckUsingStatements()
            .Recursive()
            .DetectFileKindByExtension()
            .EnableGlobber());
        
        services.AddSingleton(typeof(NuspecVersionPatcherConfig), new NuspecVersionPatcherConfig()
            .PatchNuspecFiles()
            .InsertAttributesIfMissing()
            .Recursive()
            .DetectFileKindByExtension()
            .EnableGlobber());
        
        services.AddSingleton(typeof(TextFilePatcherConfig), new TextFilePatcherConfig()
            .Recursive()
            .DetectFileKindByExtension()
            .EnableGlobber());
    }
}