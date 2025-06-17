namespace AutoVersionIt.Patches.Configuration;

public class TextFilePatcherConfig : VersionPatcherConfig
{
    public TextFilePatcherConfig WithFilters(IEnumerable<string> filters)
    {
        foreach (var filter in filters)
            WithCustomFilter(filter);
        
        return this;
    }

    public TextFilePatcherConfig WithCustomFilter(string filter)
    {
        AddCustomFilter(filter);

        return this;
    }

    public TextFilePatcherConfig ClearFilters()
    {
        GlobberFilterList.Clear();
        WildcardFilterList.Clear();
        CustomFilterList.Clear();
        return this;
    }
}