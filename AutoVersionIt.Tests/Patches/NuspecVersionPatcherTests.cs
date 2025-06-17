using System.Xml;
using AutoVersionIt.Patches;
using AutoVersionIt.Patches.Configuration;

namespace AutoVersionIt.Tests.Patches;

public class NuspecVersionPatcherTests
{
    private const string TestFilesDir = "Files";
    
    private NuspecVersionPatcherConfig _defaultConfig = new NuspecVersionPatcherConfig()
        .PatchNuspecFiles()
        .InsertAttributesIfMissing()
        .EnableGlobber()
        .Recursive()
        .DetectFileKindByExtension() as NuspecVersionPatcherConfig ?? throw new InvalidOperationException();
    
    private string PrepareTestDirectory()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "NuspecPatcherTests_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
    
    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NuspecVersionPatcher(null!, new NuspecVersionPatcherConfig()));
    }
    
    [Fact]
    public void Constructor_WithEmptyPath_ThrowsException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new NuspecVersionPatcher("", _defaultConfig));
    }
    
    [Fact]
    public void Constructor_AddsCorrectFilter()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new NuspecVersionPatcher(tempPath, _defaultConfig);
        
        // Assert
        Assert.Single(patcher.Config.Filters);
        Assert.Equal("**/*.nuspec", patcher.Config.Filters[0]);
    }
    
    [Fact]
    public void PatchFile_UpdatesExistingVersionNode()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(2, 1, 0, 0) };
        var patcher = new NuspecVersionPatcher(testDir, _defaultConfig);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        var doc = new XmlDocument();
        doc.Load(sampleFile);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode = doc.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.NotNull(versionNode);
        Assert.Equal("2.1.0", versionNode.InnerText);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_CreatesVersionNodeWhenMissing()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleNuget2.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget2.nuspec"), sampleFile, true);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(3, 0, 1, 0),
            FixedSuffix = "beta"
        };
        var config = new NuspecVersionPatcherConfig()
            .PatchNuspecFiles()
            .InsertAttributesIfMissing();
        var patcher = new NuspecVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        var doc = new XmlDocument();
        doc.Load(sampleFile);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode = doc.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.NotNull(versionNode);
        Assert.Equal("3.0.1.0-beta", versionNode.InnerText);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void IgnoreMissingAttributes_SkipsCreatingVersionNode()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleNuget2.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget2.nuspec"), sampleFile, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(4, 2, 0, 0) };
        var config = new NuspecVersionPatcherConfig()
            .IgnoreMissingAttributes();
        var patcher = new NuspecVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        var doc = new XmlDocument();
        doc.Load(sampleFile);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode = doc.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.Null(versionNode);
        versionNode = doc.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:id", nsmgr);
        Assert.NotNull(versionNode); // Sanity check for XPath when namespaces are defined
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_HandlesPreReleaseVersions()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile, true);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(7, 0, 0, 0),
            FixedSuffix = "preview",
            DynamicSuffix = "001"
        };
        var patcher = new NuspecVersionPatcher(testDir, _defaultConfig);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        var doc = new XmlDocument();
        doc.Load(sampleFile);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode = doc.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.NotNull(versionNode);
        Assert.Equal("7.0.0.0-preview001", versionNode.InnerText);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void NonRecursive_OnlyProcessesTopLevelFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile1, true);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile2, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(8, 0, 0, 0) };
        var config = new NuspecVersionPatcherConfig()
            .PatchNuspecFiles()
            .DisableGlobber()
            .NonRecursive() as NuspecVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NuspecVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        var doc1 = new XmlDocument();
        doc1.Load(sampleFile1);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc1.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode1 = doc1.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.Equal("8.0.0", versionNode1!.InnerText);
        
        // Subdir file should NOT be patched
        var doc2 = new XmlDocument();
        doc2.Load(sampleFile2);
        nsmgr = new XmlNamespaceManager(doc2.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode2 = doc2.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.Equal("1.0.0-rc1", versionNode2!.InnerText);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Recursive_ProcessesAllFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile1, true);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleNuget1.nuspec");
        File.Copy(Path.Combine(TestFilesDir, "SampleNuget1.nuspec"), sampleFile2, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(9, 0, 0, 0) };
        var config = new NuspecVersionPatcherConfig()
            .PatchNuspecFiles()
            .Recursive() as NuspecVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NuspecVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        var doc1 = new XmlDocument();
        doc1.Load(sampleFile1);
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc1.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode1 = doc1.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.Equal("9.0.0", versionNode1!.InnerText);
        
        // Subdir file should also be patched
        var doc2 = new XmlDocument();
        doc2.Load(sampleFile2);
        nsmgr = new XmlNamespaceManager(doc2.NameTable);
        nsmgr.AddNamespace("nuspec", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionNode2 = doc2.SelectSingleNode("/nuspec:package/nuspec:metadata/nuspec:version", nsmgr);
        Assert.Equal("9.0.0", versionNode2!.InnerText);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
}