namespace AutoVersionIt.Patches;

/// <summary>
/// Describes a class that can be used to patch version information on various destinations
/// </summary>
public interface IVersionPatcher
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Patch the version information defined on the destination with the provided version object.
    /// </summary>
    /// <param name="versionInformation">New version information that will be written to the destination</param>
    public void Patch(VersionInformation versionInformation);
}