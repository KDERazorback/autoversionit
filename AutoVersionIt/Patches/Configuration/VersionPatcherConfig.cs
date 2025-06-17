namespace AutoVersionIt.Patches.Configuration;

public class VersionPatcherConfig
{
    protected IList<string> GlobberFilterList { get; } = new List<string>();
    protected IList<string> WildcardFilterList { get; } = new List<string>();
    protected IList<string> CustomFilterList { get; } = new List<string>();
    public IReadOnlyList<string> Filters => 
        (ShouldUseGlobber ? GlobberFilterList : WildcardFilterList)
        .Concat(CustomFilterList)
        .ToList()
        .AsReadOnly();
    public bool ShouldRecurse { get; protected set; } = true;
    public bool ShouldUseGlobber { get; protected set; } = true;
    protected Func<FileInfo, FileDataKind>? FileKindDetectionFunc { get; set; } = null;

    protected void AddCustomFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("Filter cannot be null or whitespace.", nameof(filter));
        CustomFilterList.Add(filter);
    }
    
    public VersionPatcherConfig Recursive()
    {
        ShouldRecurse = true;

        return this;
    }

    public VersionPatcherConfig NonRecursive()
    {
        ShouldRecurse = false;
        
        return this;
    }

    public VersionPatcherConfig EnableGlobber()
    {
        ShouldUseGlobber = true;
        return this;
    }

    public VersionPatcherConfig DisableGlobber()
    {
        ShouldUseGlobber = false;
        return this;
    }

    public VersionPatcherConfig DetectFileKindByExtension()
    {
        FileKindDetectionFunc = GetFileDataKindByExtension;
        return this;
    }

    public VersionPatcherConfig DetectFileKindWithFunc(Func<FileInfo, FileDataKind> func)
    {
        FileKindDetectionFunc = func;
        return this;
    }

    public FileDataKind DetectFileKind(FileInfo file)
    {
        return FileKindDetectionFunc?.Invoke(file) ?? GetFileDataKindByExtension(file);
    }
    
    public FileDataKind GetFileDataKindByExtension(FileInfo file)
    {
        if (string.Equals(file.Extension, ".cs", StringComparison.OrdinalIgnoreCase)) return FileDataKind.CSharp;
        if (string.Equals(file.Extension, ".vb", StringComparison.OrdinalIgnoreCase)) return FileDataKind.Vb;
        if (string.Equals(file.Extension, ".cpp", StringComparison.OrdinalIgnoreCase)) return FileDataKind.Cpp;
        if (string.Equals(file.Extension, ".txt", StringComparison.OrdinalIgnoreCase)) return FileDataKind.Text;
        if (string.Equals(file.Extension, ".nuspec", StringComparison.OrdinalIgnoreCase)) return FileDataKind.NuSpec;
        if (string.Equals(file.Extension, ".csproj", StringComparison.OrdinalIgnoreCase)) return FileDataKind.CSharpProject;
        if (string.Equals(file.Extension, ".vbproj", StringComparison.OrdinalIgnoreCase)) return FileDataKind.VbProject;
        
        throw new NotSupportedException($"File extension {file.Extension} is not supported.");
    }
}