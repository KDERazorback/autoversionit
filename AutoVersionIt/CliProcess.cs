using AutoVersionIt.Patches;
using AutoVersionIt.Sources;
using AutoVersionIt.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt;

public class CliProcess
{
    protected ILogger? Logger { get; }
    protected ILoggerFactory? LoggerFactory { get; }
    protected IConfiguration? Configuration { get; }
    protected string[] Arguments { get; }
    protected IServiceProvider Services { get; }
    protected IVersionSource VersionSource { get; }
    protected IList<IVersionTarget> VersionTargetList { get; }
    protected IList<IVersionPatcher> VersionPatcherList { get; }
    protected IVersioningStrategy VersioningStrategy { get; }

    public CliProcess(string[] args, IServiceProvider services)
    {
        Services = services;
        Arguments = args;
        
        LoggerFactory = services.GetService<ILoggerFactory>();
        Configuration = services.GetService<IConfiguration>();
        VersionSource = services.GetRequiredService<IVersionSource>();
        VersionTargetList = services.GetServices<IVersionTarget>().ToList();
        VersionPatcherList = services.GetServices<IVersionPatcher>().ToList();
        VersioningStrategy = services.GetRequiredService<IVersioningStrategy>();
        
        Logger = LoggerFactory?.CreateLogger<CliProcess>();
    }

    public void Run()
    {
        var targetNames = string.Join(", ", VersionTargetList.Select(x =>
        {
            if (string.IsNullOrWhiteSpace(x.Name))
                return x.GetType().Name;
            return x.Name;
        }));
        if (string.IsNullOrWhiteSpace(targetNames)) targetNames = "None";
        
        var patcherNames = string.Join(", ", VersionPatcherList.Select(x =>
        {
            if (string.IsNullOrWhiteSpace(x.Name))
                return x.GetType().Name;
            return x.Name;
        }));
        if (string.IsNullOrWhiteSpace(patcherNames)) patcherNames = "None";
        
        Logger?.LogInformation("AutoVersionIT CLI process started.");
        Logger?.LogInformation("Arguments: {args}", Arguments.Any() ? string.Join(" ", Arguments) : "None");
        Logger?.LogInformation("\t--> Version source     : {source}", VersionSource.Name);
        Logger?.LogInformation("\t--> Version targets    : {targets}", targetNames);
        Logger?.LogInformation("\t--> Version patchers   : {patchers}", patcherNames);
        Logger?.LogInformation("\t--> Versioning strategy: {strategy}", VersioningStrategy.Name);

        var bumpMethod = GetVersionBumpMethod();
        Logger?.LogInformation("\t-->Version bump method: {bumpMethod}", bumpMethod);
        
        var version = VersionSource.GetCurrentVersion();
        Logger?.LogInformation("Current version: {version}", version);

        var nextVersion = VersioningStrategy.Increment(version, bumpMethod);
        Logger?.LogInformation("Next version: {nextVersion}", nextVersion);

        if (VersionPatcherList.Any())
        {
            foreach (var versionPatcher in VersionPatcherList)
            {
                string name = versionPatcher.Name;
                if (string.IsNullOrWhiteSpace(name)) name = versionPatcher.GetType().Name;
                Logger?.LogInformation("Applying version to patcher \"{name}\"...", name);
                versionPatcher.Patch(nextVersion);
            }
        }
        
        if (VersionTargetList.Any())
        {
            foreach (var versionTarget in VersionTargetList)
            {
                string name = versionTarget.Name;
                if (string.IsNullOrWhiteSpace(name)) name = versionTarget.GetType().Name;
                Logger?.LogInformation("Applying version to destination patcher \"{name}\"...", name);
                versionTarget.SetNewVersion(nextVersion);
            }
        }
        
        Logger?.LogInformation("AutoVersionIT CLI process completed.");
    }
    
    protected VersionBumpType GetVersionBumpMethod()
    {
        if (Arguments.Length > 0)
        {
            if (Arguments.Contains("--major", StringComparer.OrdinalIgnoreCase)) return VersionBumpType.Major;
            if (Arguments.Contains("--minor", StringComparer.OrdinalIgnoreCase)) return VersionBumpType.Minor;
            if (Arguments.Contains("--build", StringComparer.OrdinalIgnoreCase)) return VersionBumpType.Build;
            if (Arguments.Contains("--revision", StringComparer.OrdinalIgnoreCase)) return VersionBumpType.Revision;
            if (Arguments.Contains("--nobump", StringComparer.OrdinalIgnoreCase)) return VersionBumpType.None;
        }
        
        var envName = Environment.GetEnvironmentVariable("AUTOVERSIONIT_VERSION_BUMP_METHOD");
        if (!string.IsNullOrWhiteSpace(envName) && Enum.TryParse(envName, true, out VersionBumpType envResult))
            return envResult;

        if (Configuration is not null)
        {
            var versionBump = Configuration["bumpMethod"];
            if (Enum.TryParse(versionBump, true, out VersionBumpType result))
                return result;
            Logger?.LogError("Invalid version bump method specified in configuration: {versionBump}", versionBump);
        }
        
        throw new InvalidOperationException("No valid version bump method specified.");
    }
}