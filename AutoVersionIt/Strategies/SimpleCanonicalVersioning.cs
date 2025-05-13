namespace AutoVersionIt.Strategies;

/// <summary>
/// Represents a versioning strategy that follows a simple canonical versioning approach.
/// This strategy organizes versions using the format `Major.Minor.Build.Revision`,
/// and allows incrementing or comparing versions based on specific version bump types.
/// </summary>
public class SimpleCanonicalVersioning : VersioningStrategyBase
{
    /// <summary>
    /// Gets the name of the version strategy. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => "Canonical Versioning";
    
    /// <summary>
    /// Increments the version information based on the specified version bump type,
    /// resetting lower components if necessary.
    /// </summary>
    /// <param name="versionInformation">
    /// The current version information to be incremented.
    /// </param>
    /// <param name="versionBumpType">
    /// The type of version increment to apply, such as Major, Minor, Build, or Revision.
    /// </param>
    /// <returns>
    /// A new <see cref="VersionInformation"/> instance with the incremented version values.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the <paramref name="versionBumpType"/> is set to Suffix as suffix versioning is not supported.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an undefined version bump type is provided.
    /// </exception>
    public override VersionInformation Increment(VersionInformation versionInformation, VersionBumpType versionBumpType)
    {
        if (versionBumpType == VersionBumpType.None) return versionInformation;
        var major = versionInformation.CanonicalPart.Major;
        var minor = versionInformation.CanonicalPart.Minor;
        var build = versionInformation.CanonicalPart.Build;
        var revision = versionInformation.CanonicalPart.Revision;
        
        switch (versionBumpType)
        {
            case VersionBumpType.Major:
                major++;
                minor = 0;
                build = 0;
                revision = 0;
                break;
            case VersionBumpType.Minor:
                minor++;
                build = 0;
                revision = 0;
                break;
            case VersionBumpType.Build:
                build++;
                revision = 0;
                break;
            case VersionBumpType.Revision:
                revision++;
                break;
            case VersionBumpType.Suffix:
                throw new NotSupportedException("Suffix versioning is not supported.");
            default:
                throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
        }
        
        return new VersionInformation()
        {
            CanonicalPart = new Version(major, minor, build, revision),
            FixedSuffix = string.Empty,
            DynamicSuffix = string.Empty
        };
    }

    /// <summary>
    /// Decrements the version information based on the specified version bump type.
    /// The decrement operation modifies the appropriate component of the canonical version
    /// (Major, Minor, Build, or Revision) by reducing it by one, resetting lower components if necessary.
    /// </summary>
    /// <param name="versionInformation">The current version information to be decremented.</param>
    /// <param name="versionBumpType">The type of version component to decrement (e.g., Major, Minor, Build, Revision).</param>
    /// <returns>A new <see cref="VersionInformation"/> object with the decremented version details.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified version component cannot be decremented
    /// (e.g., attempting to decrement a component that is already zero).
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the specified <paramref name="versionBumpType"/> is Suffix,
    /// as suffix versioning is not supported.
    /// </exception>
    public override VersionInformation Decrement(VersionInformation versionInformation, VersionBumpType versionBumpType)
    {
        if (versionBumpType == VersionBumpType.None) return versionInformation;
        var major = versionInformation.CanonicalPart.Major;
        var minor = versionInformation.CanonicalPart.Minor;
        var build = versionInformation.CanonicalPart.Build;
        var revision = versionInformation.CanonicalPart.Revision;
        
        switch (versionBumpType)
        {
            case VersionBumpType.Major:
                if (major < 1) throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
                major--;
                minor = 0;
                build = 0;
                revision = 0;
                break;
            case VersionBumpType.Minor:
                if (minor < 1) throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
                minor--;
                build = 0;
                revision = 0;
                break;
            case VersionBumpType.Build:
                if (build < 1) throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
                build--;
                revision = 0;
                break;
            case VersionBumpType.Revision:
                if (revision < 1) throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
                revision--;
                break;
            case VersionBumpType.Suffix:
                throw new NotSupportedException("Suffix versioning is not supported.");
            default:
                throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
        }
        
        return new VersionInformation()
        {
            CanonicalPart = new Version(major, minor, build, revision),
            FixedSuffix = string.Empty,
            DynamicSuffix = string.Empty
        };
    }

    /// <summary>
    /// Compares two <see cref="VersionInformation"/> objects to determine if the first is greater than the second.
    /// </summary>
    /// <param name="versionInformation">
    /// The first <see cref="VersionInformation"/> object to compare.
    /// </param>
    /// <param name="otherVersionInformation">
    /// The second <see cref="VersionInformation"/> object to compare.
    /// </param>
    /// <returns>
    /// True if the canonical part of the first version is greater than the canonical part of the second version,
    /// and both versions have the same non-canonical components; otherwise, false.
    /// </returns>
    public override bool IsGreaterThan(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        return versionInformation.CanonicalPart > otherVersionInformation.CanonicalPart
            && SameNonCanonicalComponents(versionInformation, otherVersionInformation);
    }

    /// <summary>
    /// Determines if the given version information is less than another version information
    /// by comparing their canonical parts and ensuring their non-canonical components are the same.
    /// </summary>
    /// <param name="versionInformation">The current version information to compare.</param>
    /// <param name="otherVersionInformation">The version information to compare against.</param>
    /// <returns>True if the current version information is less than the other version information; otherwise, false.</returns>
    public override bool IsLessThan(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        return versionInformation.CanonicalPart < otherVersionInformation.CanonicalPart
            && SameNonCanonicalComponents(versionInformation, otherVersionInformation);
    }

    /// <summary>
    /// Determines whether the specified version information is equal to another version information
    /// instance by comparing their canonical parts and non-canonical components.
    /// </summary>
    /// <param name="versionInformation">The primary version information to compare.</param>
    /// <param name="otherVersionInformation">The version information to compare against.</param>
    /// <returns>
    /// True if the canonical parts and non-canonical components of both version information instances
    /// are equal; otherwise, false.
    /// </returns>
    public override bool IsEqualTo(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        return versionInformation.CanonicalPart == otherVersionInformation.CanonicalPart &&
            SameNonCanonicalComponents(versionInformation, otherVersionInformation);
    }

    /// <summary>
    /// Compares the non-canonical components of two versions to determine if they are the same.
    /// This method evaluates the equality of the FixedSuffix and DynamicSuffix properties
    /// in a case-insensitive manner.
    /// </summary>
    /// <param name="versionInformation">The first version information instance to compare.</param>
    /// <param name="otherVersionInformation">The second version information instance to compare.</param>
    /// <returns>
    /// A boolean value indicating whether the non-canonical components of the two versions are equal.
    /// Returns true if both FixedSuffix and DynamicSuffix are equal (case-insensitive), otherwise false.
    /// </returns>
    protected bool SameNonCanonicalComponents(VersionInformation versionInformation,
        VersionInformation otherVersionInformation)
    {
        return string.Equals(versionInformation.FixedSuffix, otherVersionInformation.FixedSuffix,
                   StringComparison.OrdinalIgnoreCase)
               && string.Equals(versionInformation.DynamicSuffix, otherVersionInformation.DynamicSuffix,
                   StringComparison.OrdinalIgnoreCase);
    }
}