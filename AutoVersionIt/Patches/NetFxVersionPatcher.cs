using System.Text;
using AutoVersionIt.Patches.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Patches;

public class NetFxVersionPatcher : VersionPatcherBase
{
    /// <summary>
    /// Gets the name of the version patcher. This property is typically used for logging or
    /// diagnostic purposes.
    /// </summary>
    public override string Name => ".NET Framework Version Patcher";

    public new NetFxVersionPatcherConfig Config { get; }

    public NetFxVersionPatcher(NetFxVersionPatcherConfig config, ILogger? logger = null)
        :base(config, new DirectoryInfo(Directory.GetCurrentDirectory()))
    {
        Logger = logger;
        Config = config;
    }
    
    public NetFxVersionPatcher(string path, NetFxVersionPatcherConfig config, ILogger? logger = null)
        :base(config, new DirectoryInfo(path))
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        Logger = logger;
        Config = config;
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
            case FileDataKind.CSharp:
                PatchCSharpFile(file, version);
                break;
            case FileDataKind.Vb:
                PatchVbFile(file, version);
                break;
            case FileDataKind.Cpp:
                PatchCppFile(file, version);
                break;
            default:
                throw new NotSupportedException($"File extension {file.Extension} is not supported.");
        }
    }

    protected void PatchCSharpFile(FileInfo file, VersionInformation version) 
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

        if (Config.ShouldCheckUsingStatements)
        {
            InsertOrUpdateRegexString(lines, "using\\s+System\\.Reflection;", "using System.Reflection;");
            InsertOrUpdateRegexString(lines, "using\\s+System\\.Runtime\\.CompilerServices;", "using System.Runtime.CompilerServices;");
            InsertOrUpdateRegexString(lines, "using\\s+System\\.Runtime\\.InteropServices;", "using System.Runtime.InteropServices;");
        }

        if (Config.ShouldInsertAttributesIfMissing)
        {
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyInformationalVersion(\"{0}\")];", version));
        }
        else
        {
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*\\]", string.Format("[assembly: AssemblyInformationalVersion(\"{0}\")]", version));
        }

        using var writer = new StreamWriter(fs, enc);
        foreach (var line in lines)
            writer.WriteLine(line);
        
        writer.Flush();
        fs.Flush();
    }

    protected void PatchVbFile(FileInfo file, VersionInformation version)
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
        
        if (Config.ShouldCheckUsingStatements)
        {
            InsertOrUpdateRegexString(lines, "Imports\\s+System", "Imports System");
            InsertOrUpdateRegexString(lines, "Imports\\s+System\\.Reflection", "Imports System.Reflection");
            InsertOrUpdateRegexString(lines, "Imports\\s+System\\.Runtime\\.InteropServices", "Imports System.Runtime.InteropServices");
        }

        if (Config.ShouldInsertAttributesIfMissing)
        {
            AppendOrUpdateRegexString(lines, "<Assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyVersion(\"{0}\")>", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "<Assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyFileVersion(\"{0}\")>", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "<Assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyInformationalVersion(\"{0}\")>", version));
        }
        else
        {
            PatchLinesIfRegexMatches(lines, "<Assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyVersion(\"{0}\")>", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "<Assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyFileVersion(\"{0}\")>", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "<Assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*>", string.Format("<Assembly: AssemblyInformationalVersion(\"{0}\")>", version));
        }

        using var writer = new StreamWriter(fs, enc);
        foreach (var line in lines)
            writer.WriteLine(line);
        
        writer.Flush();
        fs.Flush();
    }
    
    protected void PatchCppFile(FileInfo file, VersionInformation version)
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
        
        if (Config.ShouldCheckUsingStatements)
        {
            InsertOrUpdateRegexString(lines, "using\\s+namespace\\s+System::Reflection\\s*;", "using namespace System::Reflection;");
            InsertOrUpdateRegexString(lines, "using\\s+namespace\\s+System::Runtime::InteropServices\\s*;", "using namespace System::Runtime::InteropServices;");
        }

        if (Config.ShouldInsertAttributesIfMissing)
        {
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyVersion(\"{0}\")];", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyFileVersion(\"{0}\")];", version.AsFullCanonicalString()));
            AppendOrUpdateRegexString(lines, "\\[assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyInformationalVersion(\"{0}\")];", version));
        }
        else
        {
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyVersion(\"{0}\")];", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyFileVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyFileVersion(\"{0}\")];", version.AsFullCanonicalString()));
            PatchLinesIfRegexMatches(lines, "\\[assembly:\\s*AssemblyInformationalVersion\\s*\\(.*\\)\\s*\\]\\s*;", string.Format("[assembly: AssemblyInformationalVersion(\"{0}\")];", version));
        }

        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        using var writer = new StreamWriter(fs, enc);
        foreach (var line in lines)
            writer.WriteLine(line);
        
        writer.Flush();
        fs.Flush();
    }
}