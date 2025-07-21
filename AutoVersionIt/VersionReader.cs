using System.Globalization;

namespace AutoVersionIt;

/// <summary>
/// Provides functionality to parse a version string into a structured representation
/// and configure parsing behavior for version strings that may be empty or lack specific components.
/// </summary>
public class VersionReader
{
    /// <summary>
    /// Gets or sets a value indicating whether an exception should be thrown when the input version string
    /// is null, empty, or consists only of whitespace.
    /// </summary>
    /// <remarks>
    /// When set to true, attempting to parse an invalid or empty version string will result in an
    /// <see cref="ArgumentNullException"/> being thrown. If set to false, no exception will be thrown,
    /// and a default version information object will be returned instead.
    /// </remarks>
    public bool ShouldThrowIfEmpty { get; set; } = true;

    /// <summary>
    /// Converts a given version string into a <see cref="VersionInformation"/> object,
    /// parsing the canonical version parts and any optional suffixes.
    /// </summary>
    /// <param name="versionString">
    /// The version string to parse. It must follow the format "major.minor.build.revision-suffix",
    /// where "suffix" can include fixed and dynamic parts.
    /// </param>
    /// <returns>
    /// A <see cref="VersionInformation"/> object containing the parsed version details,
    /// including canonical version numbers and optional suffixes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="versionString"/> is null or empty, and the behavior to throw
    /// is enabled by the <see cref="ShouldThrowIfEmpty"/> property.
    /// </exception>
    public VersionInformation FromString(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            if (ShouldThrowIfEmpty) 
                throw new ArgumentNullException(nameof(versionString), "Version string cannot be null or empty.");
            return new VersionInformation();
        }
        
        string[] parts = versionString.Split('-');
        string[] versionParts = parts[0].Split('.');
        string fixedPart = string.Empty;
        string dynamicPart = string.Empty;

        if (parts.Length > 1)
        {
            var suffix = parts[1].Trim();
            int i = suffix.Length - 1;
            for (; i >= 0; i--)
                if (suffix[i] < '0' || suffix[i] > '9') break;

            fixedPart = suffix.Substring(0, i + 1);
            if (i + 1 < suffix.Length)
                dynamicPart = suffix.Substring(i + 1);
        }

        var major = versionParts[0].Trim();
        var minor = versionParts.Length > 1 ? versionParts[1].Trim() : "0";
        var build = versionParts.Length > 2 ? versionParts[2].Trim() : "0";
        var revision = versionParts.Length > 3 ? versionParts[3].Trim() : "0";
        return new VersionInformation()
        {
            CanonicalPart = new Version(int.Parse(major, NumberStyles.Integer, CultureInfo.InvariantCulture),
                int.Parse(minor, NumberStyles.Integer, CultureInfo.InvariantCulture),
                int.Parse(build, NumberStyles.Integer, CultureInfo.InvariantCulture),
                int.Parse(revision, NumberStyles.Integer, CultureInfo.InvariantCulture)),
            FixedSuffix = fixedPart,
            DynamicSuffix = dynamicPart
        };
    }

    /// <summary>
    /// Configures the <see cref="VersionReader"/> instance to throw an exception
    /// when an empty or null version string is encountered during parsing.
    /// </summary>
    /// <returns>The current <see cref="VersionReader"/> instance, with the behavior updated to throw on empty version strings.</returns>
    public VersionReader ThrowIfEmpty()
    {
        ShouldThrowIfEmpty = true;
        return this;
    }

    /// <summary>
    /// Configures the <see cref="VersionReader"/> to handle empty version strings silently without throwing an exception.
    /// By calling this method, if the version string is empty or null, the processing will not raise an error but return a default version information object.
    /// </summary>
    /// <returns>The current instance of <see cref="VersionReader"/> with updated behavior for handling empty strings.</returns>
    public VersionReader IgnoreEmpty()
    {
        ShouldThrowIfEmpty = false;
        return this;
    }
}