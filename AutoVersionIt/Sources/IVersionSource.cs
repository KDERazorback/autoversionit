namespace AutoVersionIt.Sources;

/// <summary>
/// Defines an interface for managing version information. This interface includes methods to
/// retrieve the current version
/// </summary>
public interface IVersionSource
{
    /// <summary>
    /// Gets the name of the version source. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Retrieves the current version information.
    /// </summary>
    /// <returns>
    /// A <see cref="VersionInformation"/> object that contains details about the current version,
    /// including the canonical version and any suffixes indicating a pre-release state.
    /// </returns>
    public VersionInformation GetCurrentVersion();
}