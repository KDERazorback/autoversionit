using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AutoVersionIt.Patches.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public abstract class VersionPatcherBase : IVersionPatcher
{
    public abstract string Name { get; }
    protected VersionPatcherBase(VersionPatcherConfig config, DirectoryInfo sourceDirectory)
    {
        SourceDirectory = sourceDirectory;
        Config = config;
    }

    protected ILogger? Logger { get; init; }
    public DirectoryInfo SourceDirectory { get; init; }
    public VersionPatcherConfig Config { get; }
    
    public void Patch(VersionInformation versionInformation)
    {
        IList<FileInfo> files;
        if (Config.ShouldUseGlobber)
            files = GetFilesToPatch_Globber();
        else
            files = GetFilesToPatch_Simple();
        
        Logger?.LogInformation("Found {0:N0} files to patch.", files.Count);

        foreach (var file in files)
        {
            Logger?.LogInformation("Patching file {file}", file.FullName);
            PatchFile(file, versionInformation);
        }
    }

    protected IList<FileInfo> GetFilesToPatch_Simple()
    {
        List<FileInfo> filesToPatch = new List<FileInfo>();
        
        foreach (var filter in Config.Filters)
        {
            var files = SourceDirectory.GetFiles(filter, Config.ShouldRecurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);;
            filesToPatch.AddRange(files);
        }
        
        return filesToPatch;
    }

    protected IList<FileInfo> GetFilesToPatch_Globber()
    {
        if (!Config.ShouldRecurse) throw new NotSupportedException("Cannot disable recursive search when using globber. Use the simple search instead."); 
        var matcher = new Matcher();
        matcher.AddIncludePatterns(Config.Filters);
        var results = matcher.GetResultsInFullPath(SourceDirectory.FullName);

        var list = (from x in results select new FileInfo(x)).ToList();

        return list;
    }

    protected abstract void PatchFile(FileInfo file, VersionInformation version);

    protected Encoding DetectFileEncoding(FileStream fs)
    {
        var result = StreamEncodingHelper.GuessEncodingFor(fs);
        fs.Seek(0, SeekOrigin.Begin);

        return result;
    }

    protected void AppendOrUpdateRegexString(IList<string> lines, string regex, string replacement)
    {
        if (PatchLinesIfRegexMatches(lines, regex, replacement) == 0)
            lines.Add(replacement);
    }

    protected void InsertOrUpdateRegexString(IList<string> lines, string regex, string replacement)
    {
        if (PatchLinesIfRegexMatches(lines, regex, replacement) == 0)
        {
            var items = lines.ToArray();
            lines.Clear();
            lines.Add(replacement);
            
            foreach (var item in items)
                lines.Add(item);
        }
    }

    protected int PatchLinesIfRegexMatches(IList<string> lines, string regex, string replacement)
    {
        var replacements = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (!Regex.IsMatch(lines[i], string.Format("^\\s*{0}\\s*$", regex),
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) continue;
            
            lines[i] = Regex.Replace(lines[i], string.Format("{0}\\s*$", regex), replacement, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            replacements++;
        }
        
        return replacements;
    }

    protected void AppendOrUpdateXmlNode(XmlDocument doc, string path, string value)
    {
        var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        XmlNode node = doc;
        for (int i = 0; i < pathSegments.Length; i++)
        {
            var name = pathSegments[i];
            var children = (from XmlNode x in node.ChildNodes
                where string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)
                select x).ToList();
        
            if (!children.Any())
            {
                var child = doc.CreateElement(string.Empty, name, node.NamespaceURI);
                node.AppendChild(child);
                node = child;
                continue;
            }

            if (children.Count == 1 || i == pathSegments.Length - 1)
            {
                node = children.First();
                continue;
            }

            bool foundMatch = false;
            foreach (var child in children)
            {
                var subNode = (from XmlNode x in child.ChildNodes
                    where string.Equals(x.Name, pathSegments[i + 1], StringComparison.OrdinalIgnoreCase)
                    select x).FirstOrDefault();

                if (subNode is not null)
                {
                    node = child;
                    foundMatch = true;
                    break;
                }
            }
        
            if (!foundMatch)
                node = children.First();
        }
    
        node.InnerText = value;
    }

    protected void UpdateXmlNodeIfExists(XmlDocument doc, string path, string value)
    {
        var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        XmlNode? node = doc;
        for (int i = 0; i < pathSegments.Length && node is not null; i++)
        {
            var name = pathSegments[i];
            var children = (from XmlNode x in node.ChildNodes
                where string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)
                select x).ToList();

            if (!children.Any())
                return;

            if (children.Count == 1 || i == pathSegments.Length - 1)
            {
                node = children.First();
                continue;
            }

            bool foundMatch = false;
            foreach (var child in children)
            {
                var subNode = (from XmlNode x in child.ChildNodes
                    where string.Equals(x.Name, pathSegments[i + 1], StringComparison.OrdinalIgnoreCase)
                    select x).FirstOrDefault();

                if (subNode is not null)
                {
                    node = child;
                    foundMatch = true;
                    break;
                }
            }
        
            if (!foundMatch)
                node = children.First();
        }
    
        if (node is null) 
            return;
        
        node.InnerText = value;
    }
}