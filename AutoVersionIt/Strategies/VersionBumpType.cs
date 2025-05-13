namespace AutoVersionIt.Strategies;

/// <summary>
/// Defines the types of version increments or decrements that can be applied
/// to a versioning system based on specified strategies.
/// </summary>
public enum VersionBumpType
{
    /// <summary>
    /// Indicates that no version bump must be done.
    /// </summary>
    None,
    /// <summary>
    /// Represents a major version bump in the versioning process.
    /// </summary>
    /// <remarks>
    /// A major bump typically increments the most significant part of the version number,
    /// resetting lesser parts (e.g., "1.0.0" to "2.0.0").
    /// Major version increases commonly denote breaking changes or significant updates
    /// that might not be compatible with previous versions.
    /// </remarks>
    Major,

    /// <summary>
    /// Represents a minor version increment within the versioning scheme.
    /// </summary>
    /// <remarks>
    /// A minor version bump is typically used to introduce new features
    /// or enhancements in a backward-compatible manner without breaking
    /// existing functionality and APIs
    /// </remarks>
    Minor,

    /// <summary>
    /// Represents a version bump of the build number within a versioning system.
    /// </summary>
    /// <remarks>
    /// A "Build" bump type typically refers to an increment in the build component of a version,
    /// often used to track changes or updates that do not modify the major or minor functionality
    /// of the software. Commonly used for internal builds or iterative progress during development.
    /// </remarks>
    Build,

    /// <summary>
    /// Represents a revision-level increment or decrement for a version number.
    /// </summary>
    /// <remarks>
    /// This level is used to adjust the smallest defined sub-component of the version.
    /// Typically, revisions denote fixes or very minor changes that do not affect functionality majorly.
    /// </remarks>
    Revision,

    /// <summary>
    /// Represents a versioning operation that affects the suffix of the version.
    /// </summary>
    /// <remarks>
    /// A suffix typically refers to additional information appended to the canonical version
    /// (e.g., pre-release identifiers or build metadata).
    /// </remarks>
    Suffix,
}