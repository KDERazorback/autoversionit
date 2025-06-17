namespace AutoVersionIt.Patches.Configuration;

public class NetFxVersionPatcherConfig : VersionPatcherConfig
{
    public bool ShouldInsertAttributesIfMissing { get; protected set; } = true;
    public bool ShouldCheckUsingStatements { get; protected set; } = true;

    public NetFxVersionPatcherConfig InsertAttributesIfMissing()
    {
        ShouldInsertAttributesIfMissing = true;
        
        return this;
    }

    public NetFxVersionPatcherConfig IgnoreMissingAttributes()
    {
        ShouldInsertAttributesIfMissing = false;
        
        return this;
    }
    
    public NetFxVersionPatcherConfig CheckUsingStatements()
    {
        ShouldCheckUsingStatements = true;

        return this;
    }

    public NetFxVersionPatcherConfig IgnoreUsingStatements()
    {
        ShouldCheckUsingStatements = false;

        return this;
    }
    
    public NetFxVersionPatcherConfig PatchCSharpProjects()
    {
        GlobberFilterList.Add("**/Properties/AssemblyInfo.cs");
        WildcardFilterList.Add("AssemblyInfo.cs");

        return this;
    }

    public NetFxVersionPatcherConfig PatchVbProjects()
    {
        GlobberFilterList.Add("**/Properties/AssemblyInfo.vb");
        GlobberFilterList.Add("**/My Project/AssemblyInfo.vb");
        WildcardFilterList.Add("AssemblyInfo.vb");
        
        return this;
    }
    
    public NetFxVersionPatcherConfig WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithCustomFilter(filter);
        
        return this;
    }

    public NetFxVersionPatcherConfig WithCustomFilter(string filter)
    {
        AddCustomFilter(filter);

        return this;
    }

    public NetFxVersionPatcherConfig ClearFilters()
    {
        GlobberFilterList.Clear();
        WildcardFilterList.Clear();
        CustomFilterList.Clear();
        return this;
    }
}