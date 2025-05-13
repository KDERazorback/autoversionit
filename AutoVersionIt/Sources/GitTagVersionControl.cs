using System.Runtime.InteropServices;
using AutoVersionIt.Interop;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Sources;

public class GitTagVersionControl : IVersionSource, IVersionTarget
{
    /// <summary>
    /// Gets the name of the version source. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name => "Git Tag Version Control";
    protected ILogger? Logger { get; set; }
    protected VersionReader VersionReader { get; set; }
    protected IChildProcessFactory ChildProcessFactory { get; }

    public GitTagVersionControl(VersionReader reader, IChildProcessFactory factory, ILogger? logger = null)
    {
        Logger = logger;
        VersionReader = reader;
        ChildProcessFactory = factory;
    }
    
    public VersionInformation GetCurrentVersion()
    {
        var git = GitCallSuccess("describe --tags --first-parent --match \"[0-9]*\" --abbrev=0 HEAD");
        var output = git.StdOut().ReadToEnd();
        
        if (string.IsNullOrWhiteSpace(output))
            throw new Exception("Git command failed. No tags found.");
        
        var version = VersionReader.FromString(output.Trim());
        
        return version;
    }

    public void SetNewVersion(VersionInformation versionInformation)
    {
        var headId = GetHeadHash();
        var git = GitCallSuccess(string.Format("tag \"{0}\" HEAD", 
            versionInformation
                .ToString()
                .Replace("\"", "")
                .Replace("\\", "")));
        
        Logger?.LogInformation("Successfully created a new tag {0} on commit: {1}", versionInformation, headId);        
    }

    public string GetHeadHash()
    {
        return GitCallSuccess("rev-parse HEAD")
            .StdOut()
            .ReadToEnd()
            .Trim()
            .ToLowerInvariant();
    }

    public string GetHeadVersion()
    {
        var gitCall = GitCall("rev-parse HEAD");
        if (gitCall.ExitCode() != 0) return string.Empty;
        
        return gitCall
            .StdOut()
            .ReadToEnd()
            .Trim()
            .ToLowerInvariant();
    }

    protected virtual IChildProcess GitCall(string arguments)
    {
        var forcedEnvironment = Environment.GetEnvironmentVariable("AUTOVERSIONIT_GIT_PLATFORM") ?? String.Empty;
        var forcedLinuxEnv = !string.IsNullOrWhiteSpace(forcedEnvironment) && forcedEnvironment.Equals("linux", StringComparison.OrdinalIgnoreCase);
        var forcedWindowsEnv = !string.IsNullOrWhiteSpace(forcedEnvironment) && forcedEnvironment.Equals("windows", StringComparison.OrdinalIgnoreCase);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || forcedLinuxEnv)
            return GitCall_Linux(arguments);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || forcedWindowsEnv)
            return GitCall_Windows(arguments);
        
        throw new PlatformNotSupportedException("Unsupported operating system");
    }

    protected IChildProcess GitCallSuccess(string arguments)
    {
        var process = GitCall(arguments)
            .WaitForExit();
        
        if (process.ExitCode() != 0)
        {
            var output = process.StdOut().ReadToEnd();
            var error = process.StdErr().ReadToEnd();
            Logger?.LogError("=========================================================================");
            Logger?.LogError("Git command failed with exit code {0}.", process.ExitCode());
            Logger?.LogError("Output: {0}", output);
            Logger?.LogError("Error: {0}", error);
            Logger?.LogError("=========================================================================");
            throw new Exception(string.Format("Git command failed with exit code {0}", process.ExitCode()));
        }

        return process;
    }

    protected IChildProcess GitCall_Linux(string arguments)
    {
        return ChildProcessFactory.Create()
            .Binary("git")
            .WithArguments(arguments)
            .InBackground()
            .Run();
    }

    protected IChildProcess GitCall_Windows(string arguments)
    {
        return ChildProcessFactory.Create()
            .Binary("git")
            .WithArguments(arguments)
            .InBackground()
            .Run();
    }
}