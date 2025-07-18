using AutoVersionIt.Sources.Configuration;

namespace AutoVersionIt.Sources;

/// <summary>
/// Represents a version control system that reads and writes versioning information
/// to an environment variable.
/// </summary>
public class EnvironmentVariableVersionControl : IVersionSource, IVersionTarget
{
    /// <summary>
    /// Gets the name of the version source. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name => "Environment Variable Version Control";
    
    /// <summary>
    /// Stores the configuration for the environment variable version control.
    /// </summary>
    public EnvironmentVariableVersionControlConfig Config { get; }

    /// <summary>
    /// Gets the name of the environment variable to be used for storing or retrieving version information.
    /// This property is immutable and initialized during the creation of the instance.
    /// </summary>
    public string EnvironmentVariableName => Config.EnvironmentVariableName;

    /// <summary>
    /// Gets or initializes the VersionReader instance used for parsing and handling version strings.
    /// </summary>
    public VersionReader VersionReader { get; init; }

    /// <summary>
    /// Represents a version control mechanism that operates on environment variables.
    /// This class acts as both a source and a target for managing version information,
    /// using environment variables to store and retrieve version data.
    /// </summary>
    public EnvironmentVariableVersionControl(EnvironmentVariableVersionControlConfig config, VersionReader reader)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.EnvironmentVariableName)) throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(config));
        Config = config;
        VersionReader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    /// <summary>
    /// Retrieves the current version information by reading the value of the specified environment variable.
    /// The version data is processed and converted into a <see cref="VersionInformation"/> object.
    /// </summary>
    /// <returns>
    /// A <see cref="VersionInformation"/> object that represents the current version. If the environment variable is
    /// not set or contains invalid data, a default version is returned.
    /// </returns>
    public VersionInformation GetCurrentVersion()
    {
        var data = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(data))
        {
            var segments = new string[4];
            segments[0] = Environment.GetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "MAJOR"))?.Trim() ?? string.Empty;
            segments[1] = Environment.GetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "MINOR"))?.Trim() ?? string.Empty;
            segments[2] = Environment.GetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "BUILD"))?.Trim() ?? string.Empty;
            segments[3] = Environment.GetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "REVISION"))?.Trim() ?? string.Empty;
            
            var suffix = Environment.GetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "SUFFIX"))?.Trim() ?? string.Empty;
            
            var versionString = string.Join(".", segments.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(data)) return new VersionInformation();
            
            if (!string.IsNullOrWhiteSpace(suffix))
                versionString += string.Format("-{0}", suffix);

            data = versionString;
        }

        data = data.Trim();
        data = data.Replace(" ", "").Replace("\t", "");

        return VersionReader.FromString(data);
    }

    /// <summary>
    /// Updates the version information in the environment variable specified by <c>EnvironmentVariableName</c>.
    /// </summary>
    /// <param name="versionInformation">The version information to set in the environment variable.</param>
    public void SetNewVersion(VersionInformation versionInformation)
    {
        Environment.SetEnvironmentVariable(EnvironmentVariableName, versionInformation.ToString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "CANONICAL"), versionInformation.AsFullCanonicalString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "MAJOR"), versionInformation.CanonicalPart.Major.ToString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "MINOR"), versionInformation.CanonicalPart.Minor.ToString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "BUILD"), versionInformation.CanonicalPart.Build.ToString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "REVISION"), versionInformation.CanonicalPart.Revision.ToString());
        Environment.SetEnvironmentVariable(string.Format("{0}_{1}", EnvironmentVariableName, "SUFFIX"), versionInformation.DynamicSuffix.Trim());
    }
}