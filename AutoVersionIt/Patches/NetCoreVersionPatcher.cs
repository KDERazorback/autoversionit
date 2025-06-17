using System.Text;
using System.Xml;
using AutoVersionIt.Patches.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class NetCoreVersionPatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => ".NET Core Version Patcher";

    public new NetCoreVersionPatcherConfig Config { get; }

    public NetCoreVersionPatcher(NetCoreVersionPatcherConfig config, ILogger? logger = null)
        :base(config, new DirectoryInfo(Directory.GetCurrentDirectory()))
    {
        Config = config;
        Logger = logger;
    }

    public NetCoreVersionPatcher(string path, NetCoreVersionPatcherConfig config, ILogger? logger = null)
        :base(config, new DirectoryInfo(path))
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        Config = config;
        Logger = logger;
    }

    protected override void PatchFile(FileInfo file, VersionInformation version)
    {
        if (!file.Exists)
        {
            Logger?.LogWarning(" --> File {file} does not exist. Skipping.", file.FullName);
            return;
        }

        var fileKind = Config.DetectFileKind(file);
        Logger?.LogDebug("--> Detected file kind {fileKind} for file {file}", fileKind, file.FullName);

        switch (fileKind)
        {
            case FileDataKind.CSharpProject:
            case FileDataKind.VbProject:
                PatchProjectFile(file, version);
                break;
            default:
                throw new NotSupportedException($"File extension {file.Extension} is not supported.");
        }
    }

    protected void PatchProjectFile(FileInfo file, VersionInformation version) 
    {
        using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Encoding enc = DetectFileEncoding(fs);
        Logger?.LogDebug("--> Detected encoding {enc} for file {file}", enc, file.FullName);

        var doc = new XmlDocument();
        using (var reader = new StreamReader(fs, enc, leaveOpen: true))
            doc.Load(reader);

        if (Config.ShouldInsertAttributesIfMissing)
        {
            AppendOrUpdateXmlNode(doc, "/Project/PropertyGroup/AssemblyVersion", version.AsFullCanonicalString());
            AppendOrUpdateXmlNode(doc, "/Project/PropertyGroup/FileVersion", version.AsFullCanonicalString());
            AppendOrUpdateXmlNode(doc, "/Project/PropertyGroup/AssemblyInformationalVersion", version.ToString());
        }
        else
        {
            UpdateXmlNodeIfExists(doc, "/Project/PropertyGroup/AssemblyVersion", version.AsFullCanonicalString());
            UpdateXmlNodeIfExists(doc, "/Project/PropertyGroup/FileVersion", version.AsFullCanonicalString());
            UpdateXmlNodeIfExists(doc, "/Project/PropertyGroup/AssemblyInformationalVersion", version.ToString());
        }

        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        using var writer = new StreamWriter(fs, enc);
        doc.Save(writer);
        
        writer.Flush();
        fs.Flush();
    }
}