namespace AutoVersionIt.Sources;

/// <summary>
/// Defines an interface for managing version information. This interface includes methods to
/// set a new version
/// </summary>
public interface IVersionTarget
{
    /// <summary>
    /// Gets the name of the version target. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Updates the version information in the implementing source.
    /// </summary>
    /// <param name="versionInformation">
    /// The new version information to be set. This includes the canonical version details
    /// and any optional dynamic or fixed suffixes.
    /// </param>
    public void SetNewVersion(VersionInformation versionInformation);
}