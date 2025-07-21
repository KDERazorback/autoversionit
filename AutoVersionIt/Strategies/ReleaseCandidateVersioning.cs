using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace AutoVersionIt.Strategies;

/// <summary>
/// Implements a versioning strategy for release candidate versions.
/// This strategy specifically handles versioning with dynamic suffix increments
/// and operates based on predefined rules for suffix version bump operations.
/// </summary>
/// <remarks>
/// The primary purpose of this class is to increment, decrement, and compare version
/// information that includes a canonical part, fixed suffix, and dynamic suffix.
/// Only the suffix portion of the versioning scheme is supported for increment and decrement operations.
/// </remarks>
public class ReleaseCandidateVersioning : VersioningStrategyBase
{
    /// <summary>
    /// Gets the name of the version strategy. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => "Release Candidate Versioning";

    public ReleaseCandidateVersioning(IConfiguration configuration) : base(configuration) { }
    
    /// <summary>
    /// Increments the version based on the specified version bump type.
    /// Only supports incrementing the dynamic suffix of the version.
    /// </summary>
    /// <param name="versionInformation">
    /// An instance of <see cref="VersionInformation"/> containing the current version details.
    /// </param>
    /// <param name="versionBumpType">
    /// A <see cref="VersionBumpType"/> representing the type of version bump to be applied.
    /// Only <see cref="VersionBumpType.Suffix"/> is supported.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="VersionInformation"/> with the updated version details.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the provided <paramref name="versionBumpType"/> is not <see cref="VersionBumpType.Suffix"/>.
    /// </exception>
    public override VersionInformation Increment(VersionInformation versionInformation, VersionBumpType versionBumpType)
    {
        if (versionBumpType == VersionBumpType.None) return versionInformation;
        var dynamicSuffix = 0;
        if (!string.IsNullOrWhiteSpace(versionInformation.DynamicSuffix))
            dynamicSuffix = int.Parse(versionInformation.DynamicSuffix, NumberStyles.Integer);
        
        var fixedSuffix = versionInformation.FixedSuffix;
        if (string.IsNullOrWhiteSpace(fixedSuffix) && string.IsNullOrWhiteSpace(versionInformation.DynamicSuffix))
            fixedSuffix = DefaultFixedSuffix;

        switch (versionBumpType)
        {
            case VersionBumpType.Suffix:
                dynamicSuffix++;
                break;
            default:
                throw new NotSupportedException("Only Suffix versioning is supported.");
        }

        return new VersionInformation()
        {
            CanonicalPart = new Version(versionInformation.CanonicalPart.Major,
                versionInformation.CanonicalPart.Minor,
                versionInformation.CanonicalPart.Build,
                versionInformation.CanonicalPart.Revision),
            FixedSuffix = fixedSuffix,
            DynamicSuffix = dynamicSuffix.ToString("D", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Decreases the version's dynamic suffix based on the specified bump type.
    /// </summary>
    /// <param name="versionInformation">The current version information to modify.</param>
    /// <param name="versionBumpType">The type of version bump to apply. Only supports <c>VersionBumpType.Suffix</c>.</param>
    /// <returns>A new <c>VersionInformation</c> instance with the updated dynamic suffix.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="versionBumpType"/> is <c>VersionBumpType.Suffix</c>, but the suffix value is already at its minimum.</exception>
    /// <exception cref="NotSupportedException">Thrown when the specified <paramref name="versionBumpType"/> is not supported.</exception>
    public override VersionInformation Decrement(VersionInformation versionInformation, VersionBumpType versionBumpType)
    {
        if (versionBumpType == VersionBumpType.None) return versionInformation;
        var dynamicSuffix = int.Parse(versionInformation.DynamicSuffix, NumberStyles.Integer);

        switch (versionBumpType)
        {
            case VersionBumpType.Suffix:
                if (dynamicSuffix < 1) throw new ArgumentOutOfRangeException(nameof(versionBumpType), versionBumpType, null);
                dynamicSuffix--;
                break;
            default:
                throw new NotSupportedException("Only Suffix versioning is supported.");
        }

        return new VersionInformation()
        {
            CanonicalPart = new Version(versionInformation.CanonicalPart.Major,
                versionInformation.CanonicalPart.Minor,
                versionInformation.CanonicalPart.Build,
                versionInformation.CanonicalPart.Revision),
            FixedSuffix = versionInformation.FixedSuffix,
            DynamicSuffix = dynamicSuffix.ToString("D", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Determines whether the current version information is greater than the specified other version information.
    /// </summary>
    /// <param name="versionInformation">The current version information to compare.</param>
    /// <param name="otherVersionInformation">The version information to compare against.</param>
    /// <returns>True if the current version is greater than the specified version, otherwise false.</returns>
    public override bool IsGreaterThan(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        var thisVersion = versionInformation.CanonicalPart;
        var otherVersion = otherVersionInformation.CanonicalPart;

        if (thisVersion > otherVersion)
            return true;

        if (thisVersion == otherVersion)
        {
            return int.Parse(versionInformation.DynamicSuffix, NumberStyles.Integer) >
                   int.Parse(otherVersionInformation.DynamicSuffix, NumberStyles.Integer);
        }

        return false;
    }

    /// <summary>
    /// Compares two version information objects to evaluate if the first is less than the second.
    /// </summary>
    /// <param name="versionInformation">The primary version information object to compare.</param>
    /// <param name="otherVersionInformation">The version information object to compare against.</param>
    /// <returns>Returns true if the primary version information is less than the other; otherwise, returns false.</returns>
    public override bool IsLessThan(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        var thisVersion = versionInformation.CanonicalPart;
        var otherVersion = otherVersionInformation.CanonicalPart;

        if (thisVersion < otherVersion)
            return true;

        if (thisVersion == otherVersion)
        {
            return int.Parse(versionInformation.DynamicSuffix, NumberStyles.Integer) <
                   int.Parse(otherVersionInformation.DynamicSuffix, NumberStyles.Integer);
        }

        return false;
    }

    /// <summary>
    /// Determines whether two versions are equal based on their canonical parts and dynamic suffixes.
    /// </summary>
    /// <param name="versionInformation">The first version information to compare.</param>
    /// <param name="otherVersionInformation">The second version information to compare.</param>
    /// <returns>
    /// True if the canonical parts and dynamic suffixes of the two versions are equal; otherwise, false.
    /// </returns>
    public override bool IsEqualTo(VersionInformation versionInformation, VersionInformation otherVersionInformation)
    {
        var thisVersion = versionInformation.CanonicalPart;
        var otherVersion = otherVersionInformation.CanonicalPart;

        if (thisVersion == otherVersion)
            return int.Parse(versionInformation.DynamicSuffix, NumberStyles.Integer) ==
                   int.Parse(otherVersionInformation.DynamicSuffix, NumberStyles.Integer);

        return false;
    }
}