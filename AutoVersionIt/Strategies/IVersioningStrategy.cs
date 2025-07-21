namespace AutoVersionIt.Strategies;

/// <summary>
/// Defines a strategy for managing version increments or comparisons.
/// This interface provides methods for incrementing, decrementing, and comparing version information.
/// </summary>
public interface IVersioningStrategy : IComparer<VersionInformation>
{
    /// <summary>
    /// Gets the name of the version strategy. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Default fixed suffix to be used when parsing version information.
    /// This value is used when the fixed suffix is not provided in the version parsing.
    /// </summary>
    string DefaultFixedSuffix { get; }
    /// <summary>
    /// Increments the version information based on the implemented strategy.
    /// </summary>
    /// <param name="versionInformation">
    /// The current version information to be incremented. This includes the canonical part of the version
    /// as well as any suffixes indicating prerelease or other adjustments.
    /// </param>
    /// <param name="versionBumpType">Specifies how much to increase the version information. (Major increase, minor increase, etc.)</param>
    /// <returns>
    /// A new <see cref="VersionInformation"/> object representing the incremented version.
    /// </returns>
    public VersionInformation Increment(VersionInformation versionInformation, VersionBumpType versionBumpType);

    /// <summary>
    /// Decrements the version information to its previous state.
    /// </summary>
    /// <param name="versionInformation">
    /// The current version information that will be decremented.
    /// </param>
    /// <param name="versionBumpType">Specifies how much to decrease the version information. (Major decrease, minor decrease, etc.)</param>
    /// <returns>
    /// A new <see cref="VersionInformation"/> instance that represents the previous version.
    /// </returns>
    public VersionInformation Decrement(VersionInformation versionInformation, VersionBumpType versionBumpType);

    /// <summary>
    /// Compares two <see cref="VersionInformation"/> objects to determine if the first
    /// version is greater than the second version.
    /// </summary>
    /// <param name="versionInformation">The first version to compare.</param>
    /// <param name="otherVersionInformation">The second version to compare against.</param>
    /// <returns>
    /// A boolean value indicating whether the first version is greater than the second version.
    /// Returns true if the first version is greater; otherwise, false.
    /// </returns>
    public bool IsGreaterThan(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Determines whether a version is less than another version based on their canonical parts
    /// and any applicable dynamic or fixed suffixes.
    /// </summary>
    /// <param name="versionInformation">
    /// The first <see cref="VersionInformation"/> instance for comparison.
    /// </param>
    /// <param name="otherVersionInformation">
    /// The second <see cref="VersionInformation"/> instance to compare against.
    /// </param>
    /// <returns>
    /// True if the first version is less than the second version; otherwise, false.
    /// </returns>
    public bool IsLessThan(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Determines whether the specified version information is equal to another version information object.
    /// </summary>
    /// <param name="versionInformation">The first version information to compare.</param>
    /// <param name="otherVersionInformation">The second version information to compare.</param>
    /// <returns>
    /// A boolean value indicating whether the two version information objects are equal.
    /// </returns>
    public bool IsEqualTo(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Sets a default fixed suffix for version information and updates the current instance.
    /// </summary>
    /// <param name="fixedSuffix">The default fixed suffix to set. If the fixed suffix in version parsing is not provided, this value will be used.</param>
    /// <returns>The current <see cref="IVersioningStrategy"/> instance with the specified default fixed suffix applied.</returns>
    IVersioningStrategy WithDefaultFixedSuffix(string fixedSuffix);

    /// <summary>
    /// Configures the VersionReader to have no default fixed suffix.
    /// This sets the DefaultFixedSuffix property to an empty string.
    /// </summary>
    /// <returns>
    /// Returns the current IVersioningStrategy instance with the DefaultFixedSuffix property cleared.
    /// </returns>
    IVersioningStrategy NoDefaultFixedSuffix();
}