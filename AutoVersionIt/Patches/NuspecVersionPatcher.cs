using System.Text;
using System.Xml;
using AutoVersionIt.Patches.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class NuspecVersionPatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => "NuSpec Version Patcher";

    public new NuspecVersionPatcherConfig Config { get; }

    public NuspecVersionPatcher(NuspecVersionPatcherConfig config, ILogger? logger = null)
        : base(config, new DirectoryInfo(Directory.GetCurrentDirectory()))
    {
        Logger = logger;
        Config = config;
    }

    public NuspecVersionPatcher(string path, NuspecVersionPatcherConfig config, ILogger? logger = null)
        : base(config, new DirectoryInfo(path))
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        Logger = logger;
        Config = config;
    }

    protected void PatchNuspecFile(FileInfo file, VersionInformation version)
    {
        Encoding enc;
        using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        enc = DetectFileEncoding(fs);
        Logger?.LogDebug("--> Detected encoding {enc} for file {file}", enc, file.FullName);
        fs.Seek(0, SeekOrigin.Begin);

        var doc = new XmlDocument();
        using (var reader = new StreamReader(fs, enc, leaveOpen: true))
            doc.Load(reader);

        if (Config.ShouldInsertAttributesIfMissing)
            AppendOrUpdateXmlNode(doc, "/package/metadata/version", version.ToString());
        else

            UpdateXmlNodeIfExists(doc, "/package/metadata/version", version.ToString());

        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        if (Equals(enc, Encoding.ASCII)) enc = Encoding.UTF8;
        using (var writer = new StreamWriter(fs, enc, leaveOpen: true))
            doc.Save(writer);

        fs.Flush();
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
            case FileDataKind.NuSpec:
                PatchNuspecFile(file, version);
                break;
            default:
                throw new NotSupportedException($"File extension {file.Extension} is not supported.");
        }
    }
}