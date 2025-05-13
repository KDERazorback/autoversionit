namespace AutoVersionIt.Strategies;

/// <summary>
/// Provides a base implementation for handling versioning strategies in an application or system.
/// This abstract class defines methods for incrementing, decrementing, and comparing version information,
/// as well as a utility for comparing two versions based on the strategy.
/// </summary>
public abstract class VersioningStrategyBase : IVersioningStrategy
{
    /// <summary>
    /// Gets the name of the version strategy. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Increments the given version information to the next version based on the strategy's rules.
    /// </summary>
    /// <param name="versionInformation">The current version information to be incremented.</param>
    /// <param name="versionBumpType">Specifies how much to increase the version information. (Major increase, minor increase, etc.)</param>
    /// <returns>A new <see cref="VersionInformation"/> object representing the incremented version.</returns>
    public abstract VersionInformation Increment(VersionInformation versionInformation, VersionBumpType versionBumpType);

    /// <summary>
    /// Decrements the provided version information based on a specific versioning strategy.
    /// This method adjusts the version's canonical part or suffix to represent the previous version
    /// according to the implemented strategy.
    /// </summary>
    /// <param name="versionInformation">
    /// The <see cref="VersionInformation"/> object to be decremented.
    /// This includes the canonical version information and any associated suffixes.
    /// </param>
    /// <param name="versionBumpType">Specifies how much to decrease the version information. (Major decrease, minor decrease, etc.)</param>
    /// <returns>
    /// A new <see cref="VersionInformation"/> object that represents the decremented version.
    /// The returned version depends on the specific strategy's rules for decreasing version values.
    /// </returns>
    public abstract VersionInformation Decrement(VersionInformation versionInformation, VersionBumpType versionBumpType);

    /// <summary>
    /// Determines if a given version is greater than another version based on a specific versioning strategy.
    /// </summary>
    /// <param name="versionInformation">The version to be compared.</param>
    /// <param name="otherVersionInformation">The version to compare against.</param>
    /// <returns>
    /// True if the first version is greater than the second; otherwise, false.
    /// </returns>
    public abstract bool IsGreaterThan(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Determines if the given version information is less than the other version information.
    /// </summary>
    /// <param name="versionInformation">The first version information to compare.</param>
    /// <param name="otherVersionInformation">The version information to compare against.</param>
    /// <returns>True if the first version information is less than the second; otherwise, false.</returns>
    public abstract bool IsLessThan(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Determines whether the specified version information is equal to another version information object.
    /// </summary>
    /// <param name="versionInformation">The first version information to compare.</param>
    /// <param name="otherVersionInformation">The second version information to compare.</param>
    /// <returns>True if the specified version information is equal; otherwise, false.</returns>
    public abstract bool IsEqualTo(VersionInformation versionInformation, VersionInformation otherVersionInformation);

    /// <summary>
    /// Compares two <see cref="VersionInformation"/> objects and returns an integer that indicates their relative order.
    /// </summary>
    /// <param name="x">The first <see cref="VersionInformation"/> object to compare.</param>
    /// <param name="y">The second <see cref="VersionInformation"/> object to compare.</param>
    /// <returns>
    /// A signed integer indicating the relative order of the compared objects:
    /// -1 if <paramref name="x"/> is less than <paramref name="y"/>,
    /// 1 if <paramref name="x"/> is greater than <paramref name="y"/>,
    /// or 0 if they are equal.
    /// </returns>
    public virtual int Compare(VersionInformation? x, VersionInformation? y)
    {
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;
        
        if (IsLessThan(x, y)) return -1;
        if (IsGreaterThan(x, y)) return 1;
        return 0;
    }
}