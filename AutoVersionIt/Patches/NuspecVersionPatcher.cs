using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class NuspecVersionPatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => "NuSpec Version Patcher";
    public bool ShouldInsertAttributesIfMissing { get; protected set; } = true;

    public NuspecVersionPatcher(string path, ILogger? logger = null)
        :base(new DirectoryInfo(path))
    {
        Logger = logger;
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        EnableGlobber();
    }

    protected override void PatchFile(FileInfo file, VersionInformation version)
    {
        if (!file.Exists)
        {
            Logger?.LogWarning(" --> File {file} does not exist. Skipping.", file.FullName);
            return;
        }
        
        var fileKind = FileKindDetectionFunc?.Invoke(file) ?? GetFileDataKindByExtension(file);
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
        
        if (ShouldInsertAttributesIfMissing)
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

    public NuspecVersionPatcher InsertAttributesIfMissing()
    {
        ShouldInsertAttributesIfMissing = true;
        
        return this;
    }

    public NuspecVersionPatcher IgnoreMissingAttributes()
    {
        ShouldInsertAttributesIfMissing = false;
        
        return this;
    }

    public NuspecVersionPatcher Recursive()
    {
        ShouldRecurse = true;

        return this;
    }

    public NuspecVersionPatcher NonRecursive()
    {
        ShouldRecurse = false;
        
        return this;
    }

    public NuspecVersionPatcher EnableGlobber()
    {
        ShouldUseGlobber = true;
        var indices = new Stack<int>();
        for (var index = 0; index < FilterList.Count; index++)
        {
            var filter = FilterList[index];
            if (string.Equals(filter, "*.nuspec", StringComparison.OrdinalIgnoreCase))
                indices.Push(index);
        }
        while (indices.Count > 0)
            FilterList.RemoveAt(indices.Pop());

        FilterList.Add("**/*.nuspec");
        return this;
    }

    public NuspecVersionPatcher DisableGlobber()
    {
        ShouldUseGlobber = false;
        var indices = new Stack<int>();
        for (var index = 0; index < FilterList.Count; index++)
        {
            var filter = FilterList[index];
            if (string.Equals(filter, "**/*.nuspec", StringComparison.OrdinalIgnoreCase))
                indices.Push(index);
        }
        while (indices.Count > 0)
            FilterList.RemoveAt(indices.Pop());

        FilterList.Add("*.nuspec");
        return this;
    }

    public NuspecVersionPatcher DetectFileKindByExtension()
    {
        FileKindDetectionFunc = GetFileDataKindByExtension;
        return this;
    }

    public NuspecVersionPatcher DetectFileKindWithFunc(Func<FileInfo, FileDataKind> func)
    {
        FileKindDetectionFunc = func;
        return this;
    }
}