using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class TextFilePatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => "Text File Patcher";
    public TextFilePatcher(string path, ILogger? logger = null)
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
            case FileDataKind.Text:
                PatchTextFile(file, version);
                break;
            default:
                throw new NotSupportedException($"File extension {file.Extension} is not supported.");
        }
    }

    protected void PatchTextFile(FileInfo file, VersionInformation version) 
    {
        using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Encoding enc = DetectFileEncoding(fs);
        Logger?.LogDebug("--> Detected encoding {enc} for file {file}", enc, file.FullName);

        List<string> lines = new List<string>();
        using (var reader = new StreamReader(fs, enc, leaveOpen: true))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                
                lines.Add(line);
            }
        }

        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        
        AppendOrUpdateRegexString(lines, "Version\\s*=.*", string.Format("Version = {0}", version));
        
        using var writer = new StreamWriter(fs, enc);
        foreach (var line in lines)
            writer.WriteLine(line);
        
        writer.Flush();
        fs.Flush();
    }
    
    public TextFilePatcher WithFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) throw new ArgumentNullException(nameof(filter));
        FilterList.Add(filter);

        return this;
    }

    public TextFilePatcher WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithFilter(filter);
        
        return this;
    }

    public TextFilePatcher ClearFilters()
    {
        FilterList.Clear();
        return this;
    }

    public TextFilePatcher Recursive()
    {
        ShouldRecurse = true;

        return this;
    }

    public TextFilePatcher NonRecursive()
    {
        ShouldRecurse = false;
        
        return this;
    }

    public TextFilePatcher EnableGlobber()
    {
        ShouldUseGlobber = true;
        return this;
    }

    public TextFilePatcher DisableGlobber()
    {
        ShouldUseGlobber = false;
        return this;
    }

    public TextFilePatcher DetectFileKindByExtension()
    {
        FileKindDetectionFunc = GetFileDataKindByExtension;
        return this;
    }

    public TextFilePatcher DetectFileKindWithFunc(Func<FileInfo, FileDataKind> func)
    {
        FileKindDetectionFunc = func;
        return this;
    }
}