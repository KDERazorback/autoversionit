namespace AutoVersionIt.Patches.Configuration;

public class NuspecVersionPatcherConfig : VersionPatcherConfig
{
    public bool ShouldInsertAttributesIfMissing { get; protected set; } = true;
    
    public NuspecVersionPatcherConfig InsertAttributesIfMissing()
    {
        ShouldInsertAttributesIfMissing = true;
        
        return this;
    }

    public NuspecVersionPatcherConfig IgnoreMissingAttributes()
    {
        ShouldInsertAttributesIfMissing = false;
        
        return this;
    }

    public NuspecVersionPatcherConfig PatchNuspecFiles()
    {
        GlobberFilterList.Add("**/*.nuspec");
        WildcardFilterList.Add("*.nuspec");

        return this;
    }
    
    public NuspecVersionPatcherConfig WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithCustomFilter(filter);
        
        return this;
    }

    public NuspecVersionPatcherConfig WithCustomFilter(string filter)
    {
        AddCustomFilter(filter);

        return this;
    }

    public NuspecVersionPatcherConfig ClearFilters()
    {
        GlobberFilterList.Clear();
        WildcardFilterList.Clear();
        CustomFilterList.Clear();
        return this;
    }
}