using System.Text;
using AutoVersionIt.Sources.Configuration;

namespace AutoVersionIt.Sources;

/// <summary>
/// Represents a simple file-based version control mechanism that allows reading and updating
/// version information stored in a file. Implements the <see cref="IVersionSource"/> and <see cref="IVersionTarget"/> interfaces.
/// </summary>
public class FileBasedSimpleVersionControl : IVersionSource, IVersionTarget
{
    /// <summary>
    /// Gets the name of the version source. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public string Name => "File-Based Simple Version Control";
    
    /// <summary>
    /// Stores the configuration for the file-based version control.
    /// </summary>
    public FileBasedSimpleVersionControlConfig Config { get; }

    /// <summary>
    /// Gets the full file path that represents the version control file managed by the instance.
    /// The file is used to store version information and is created if it does not exist.
    /// </summary>
    public string FileName => Config.Filename;

    /// <summary>
    /// Gets the <see cref="VersionReader"/> instance used to parse and interpret version strings.
    /// This property is initialized at the time of object construction and cannot be modified afterward.
    /// </summary>
    public VersionReader VersionReader { get; init; }

    /// <summary>
    /// Represents a file-based implementation of the <see cref="IVersionSource"/> interface,
    /// used to manage version information stored in a file.
    /// </summary>
    public FileBasedSimpleVersionControl(FileBasedSimpleVersionControlConfig config, VersionReader reader)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.Filename)) throw new ArgumentException("Filename cannot be null or empty.", nameof(config));
        Config = config;
        FileInfo fi = new FileInfo(config.Filename);
        if (!fi.Directory?.Exists ?? false) fi.Directory?.Create();
        VersionReader = reader ?? throw new ArgumentNullException(nameof(reader));
    }


    /// <summary>
    /// Retrieves the current version information from the file specified by the FileName property.
    /// If the file does not exist or contains invalid data, a default VersionInformation instance is returned.
    /// </summary>
    /// <returns>
    /// An instance of VersionInformation representing the current version found in the file,
    /// or a default instance if the file is missing or does not contain valid version data.
    /// </returns>
    public VersionInformation GetCurrentVersion()
    {
        if (!File.Exists(FileName)) return new VersionInformation();

        using FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new StreamReader(fs, Encoding.ASCII, leaveOpen: true);
        
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            line = line.Trim();
            if (line.StartsWith("//") || line.StartsWith("#")) continue;
            line = line.Replace(" ", "").Replace("\t", "");

            if (line.StartsWith("Version=", StringComparison.OrdinalIgnoreCase))
            {
                line = line.Split('=')[1].Trim();
                if (string.IsNullOrWhiteSpace(line)) return new VersionInformation();
                
                return VersionReader.FromString(line);
            }
        }
        
        return new VersionInformation();
    }

    /// <summary>
    /// Updates the version information in the file associated with this instance.
    /// This method writes the given version details to the file, replacing any existing content.
    /// </summary>
    /// <param name="versionInformation">
    /// The version information to be written to the file. It includes the canonical version
    /// and optional suffixes to indicate dynamic or fixed adjustments to the version.
    /// </param>
    public void SetNewVersion(VersionInformation versionInformation)
    {
        var canonicalVersion = versionInformation.AsFullCanonicalString();
        var majorVersion = versionInformation.CanonicalPart.Major.ToString();
        var minorVersion = versionInformation.CanonicalPart.Minor.ToString();
        var buildVersion = versionInformation.CanonicalPart.Build.ToString();
        var revisionVersion = versionInformation.CanonicalPart.Revision.ToString();
        var suffixVersion = versionInformation.DynamicSuffix.Trim();
        
        using FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs.SetLength(0);
        
        using StreamWriter writer = new StreamWriter(fs, Encoding.ASCII, leaveOpen: true);
        
        writer.WriteLine("Version = {0}", versionInformation);
        writer.WriteLine("CanonicalVersion = {0}", canonicalVersion);
        writer.WriteLine("VersionMajor = {0}", majorVersion);
        writer.WriteLine("VersionMinor = {0}", minorVersion);
        writer.WriteLine("VersionBuild = {0}", buildVersion);
        writer.WriteLine("VersionRevision = {0}", revisionVersion);
        if (!string.IsNullOrWhiteSpace(suffixVersion))
            writer.WriteLine("VersionSuffix = {0}", suffixVersion);
    }
}