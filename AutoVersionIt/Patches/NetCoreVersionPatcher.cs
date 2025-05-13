using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class NetCoreVersionPatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => ".NET Core Version Patcher";
    public bool ShouldInsertAttributesIfMissing { get; protected set; } = true;

    public NetCoreVersionPatcher(string path, ILogger? logger = null)
        :base(new DirectoryInfo(path))
    {
        Logger = logger;
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
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

        if (ShouldInsertAttributesIfMissing)
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

    public NetCoreVersionPatcher InsertAttributesIfMissing()
    {
        ShouldInsertAttributesIfMissing = true;
        
        return this;
    }

    public NetCoreVersionPatcher IgnoreMissingAttributes()
    {
        ShouldInsertAttributesIfMissing = false;
        
        return this;
    }

    public NetCoreVersionPatcher PatchCSharpProjects()
    {
        if (ShouldUseGlobber)
        {
            WithFilter("**/*.csproj");

            return this;
        }
        
        WithFilter("*.csproj");

        return this;
    }

    public NetCoreVersionPatcher PatchVbProjects()
    {
        if (ShouldUseGlobber)
        {
            WithFilter("**/*.vbproj");

            return this;
        }
        
        WithFilter("*.vbproj");
        
        return this;
    }

    public NetCoreVersionPatcher WithFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) throw new ArgumentNullException(nameof(filter));
        FilterList.Add(filter);

        return this;
    }

    public NetCoreVersionPatcher WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithFilter(filter);
        
        return this;
    }

    public NetCoreVersionPatcher ClearFilters()
    {
        FilterList.Clear();
        return this;
    }

    public NetCoreVersionPatcher Recursive()
    {
        ShouldRecurse = true;

        return this;
    }

    public NetCoreVersionPatcher NonRecursive()
    {
        ShouldRecurse = false;
        
        return this;
    }

    public NetCoreVersionPatcher EnableGlobber()
    {
        ShouldUseGlobber = true;
        return this;
    }

    public NetCoreVersionPatcher DisableGlobber()
    {
        ShouldUseGlobber = false;
        return this;
    }

    public NetCoreVersionPatcher DetectFileKindByExtension()
    {
        FileKindDetectionFunc = GetFileDataKindByExtension;
        return this;
    }

    public NetCoreVersionPatcher DetectFileKindWithFunc(Func<FileInfo, FileDataKind> func)
    {
        FileKindDetectionFunc = func;
        return this;
    }
}