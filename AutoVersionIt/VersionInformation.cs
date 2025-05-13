namespace AutoVersionIt;

/// <summary>
/// Represents version information for an application or system. This class includes a canonical version
/// and optional suffixes to indicate dynamic or fixed adjustments to the version.
/// </summary>
public record VersionInformation
{
    /// <summary>
    /// Gets the canonical version part of the version information.
    /// This represents the main version number in the format Major.Minor.Build.Revision
    /// and does not include any dynamic or fixed suffixes.
    /// </summary>
    public Version CanonicalPart { get; init; } = new Version(0, 0, 0, 0);

    /// <summary>
    /// Gets the dynamic suffix appended to the version string, representing
    /// special or prerelease builds. Most commonly used for build numbers
    /// added to "beta" or "alpha" versions.
    /// </summary>
    public string DynamicSuffix { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fixed suffix associated with the version information. The fixed suffix
    /// is a static, predefined string added at the end of the Canonical Version and used to 
    /// denote a specific characteristic or state of the version (e.g., "beta", "alpha").
    /// </summary>
    public string FixedSuffix { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the version is a prerelease version.
    /// A version is considered a prerelease if the <see cref="DynamicSuffix"/> or <see cref="FixedSuffix"/>
    /// properties are not null or consist of non-whitespace characters.
    /// </summary>
    public bool IsPrerelease => !string.IsNullOrWhiteSpace(DynamicSuffix) || !string.IsNullOrWhiteSpace(FixedSuffix);

    /// <summary>
    /// Returns a string representation of the version information.
    /// If the version is a pre-release, the string includes the canonical version
    /// followed by any dynamic or fixed suffixes. Otherwise, it includes only the canonical version.
    /// </summary>
    /// <returns>
    /// A string representing the version information. For pre-release versions, this includes
    /// the canonical version followed by the dynamic and/or fixed suffixes. For released versions,
    /// only the canonical part is included.
    /// </returns>
    public override string ToString()
    {
        if (!IsPrerelease)
        {
            if (CanonicalPart.Revision == 0)
                return string.Format("{0}.{1}.{2}", CanonicalPart.Major, CanonicalPart.Minor, CanonicalPart.Build);

            return AsFullCanonicalString();
        }

        return AsPrereleaseString();
    }

    /// <summary>
    /// Returns a full canonical string representation of the version information.
    /// This includes the major, minor, build, and revision components of the version,
    /// regardless of whether the version is a release or pre-release.
    /// </summary>
    /// <returns>
    /// A string formatted as the full canonical version, including all components:
    /// major, minor, build, and revision.
    /// </returns>
    public string AsFullCanonicalString()
    {
        return string.Format("{0}.{1}.{2}.{3}", CanonicalPart.Major, CanonicalPart.Minor, CanonicalPart.Build,
            CanonicalPart.Revision);
    }

    /// <summary>
    /// Returns a string representation of the version information as a pre-release.
    /// The string includes the canonical version followed by the fixed and dynamic suffixes
    /// if it is a pre-release version. Otherwise, returns the full canonical version as a string.
    /// </summary>
    /// <returns>
    /// A pre-release string if the version includes suffixes; otherwise, the full canonical version string.
    /// </returns>
    public string AsPrereleaseString()
    {
        if (!IsPrerelease) return AsFullCanonicalString();
        
        return string.Format("{0}.{1}.{2}.{3}-{4}{5}", CanonicalPart.Major, CanonicalPart.Minor, CanonicalPart.Build,
            CanonicalPart.Revision, FixedSuffix, DynamicSuffix);
    }
}