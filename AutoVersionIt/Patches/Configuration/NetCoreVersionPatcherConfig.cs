namespace AutoVersionIt.Patches.Configuration;

public class NetCoreVersionPatcherConfig : VersionPatcherConfig
{
    public bool ShouldInsertAttributesIfMissing { get; protected set; } = true;

    public NetCoreVersionPatcherConfig InsertAttributesIfMissing()
    {
        ShouldInsertAttributesIfMissing = true;
        
        return this;
    }

    public NetCoreVersionPatcherConfig IgnoreMissingAttributes()
    {
        ShouldInsertAttributesIfMissing = false;
        
        return this;
    }

    public NetCoreVersionPatcherConfig PatchCSharpProjects()
    {
        GlobberFilterList.Add("**/*.csproj");
        WildcardFilterList.Add("*.csproj");

        return this;
    }

    public NetCoreVersionPatcherConfig PatchVbProjects()
    {
        GlobberFilterList.Add("**/*.vbproj");
        WildcardFilterList.Add("*.vbproj");
        
        return this;
    }

    public NetCoreVersionPatcherConfig WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithCustomFilter(filter);
        
        return this;
    }

    public NetCoreVersionPatcherConfig WithCustomFilter(string filter)
    {
        AddCustomFilter(filter);

        return this;
    }

    public NetCoreVersionPatcherConfig ClearFilters()
    {
        GlobberFilterList.Clear();
        WildcardFilterList.Clear();
        CustomFilterList.Clear();
        return this;
    }
}