using AutoVersionIt.Patches;
using AutoVersionIt.Patches.Configuration;

namespace AutoVersionIt.Tests.Patches;

public class TextFilePatcherTests
{
    private const string TestFilesDir = "Files";
    
    private TextFilePatcherConfig _defaultConfig = new TextFilePatcherConfig()
        .EnableGlobber()
        .Recursive()
        .DetectFileKindByExtension() as TextFilePatcherConfig ?? throw new InvalidOperationException();
    
    private string PrepareTestDirectory()
    {
        // Create a temporary directory for test files
        string tempDir = Path.Combine(Path.GetTempPath(), "TextFilePatcherTests_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
    
    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TextFilePatcher(null!, new TextFilePatcherConfig()));
    }
    
    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new TextFilePatcher("", _defaultConfig));
    }
    
    [Fact]
    public void Constructor_DoesNotAddAnyFiltersByDefault()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new TextFilePatcher(tempPath, _defaultConfig);
        
        // Assert
        Assert.Empty(patcher.Config.Filters);
    }
    
    [Fact]
    public void WithFilter_AddsFilterToFilterList()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        var filter = "*.txt";
        
        // Act
        var config = new TextFilePatcherConfig()
            .WithCustomFilter(filter);
        var patcher = new TextFilePatcher(tempPath, config);
        
        // Assert
        Assert.Single(patcher.Config.Filters);
        Assert.Equal(filter, patcher.Config.Filters[0]);
    }
    
    [Fact]
    public void WithFilters_AddsMultipleFiltersToFilterList()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        var filters = new[] { "*.txt", "*.config", "*.ini" };
        
        // Act
        var config = new TextFilePatcherConfig()
            .WithFilters(filters);
        var patcher = new TextFilePatcher(tempPath, config);
        
        // Assert
        Assert.Equal(3, patcher.Config.Filters.Count);
        Assert.Equal(filters[0], patcher.Config.Filters[0]);
        Assert.Equal(filters[1], patcher.Config.Filters[1]);
        Assert.Equal(filters[2], patcher.Config.Filters[2]);
    }
    
    [Fact]
    public void WithFilter_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        var patcher = new TextFilePatcher(tempPath, _defaultConfig);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => patcher.Config.WithCustomFilter(null!));
    }
    
    [Fact]
    public void ClearFilters_RemovesAllFilters()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt")
            .WithCustomFilter("*.config");
        var patcher = new TextFilePatcher(tempPath, config);
        
        // Act
        patcher.Config.ClearFilters();
        
        // Assert
        Assert.Empty(patcher.Config.Filters);
    }
    
    [Fact]
    public void PatchFile_UpdatesVersionLineInTextFile()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(2, 1, 0, 0) };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt");
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string content = File.ReadAllText(sampleFile);
        Assert.Contains("Version = 2.1.0", content);
        Assert.DoesNotContain("vErSiOn =         1", content);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_CreatesVersionLineWhenMissing()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleVersionInfo4.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo4.txt"), sampleFile, true);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(3, 0, 1, 0),
            FixedSuffix = "beta"
        };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt");
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string content = File.ReadAllText(sampleFile);
        Assert.Contains("Version = 3.0.1.0-beta", content);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_HandlesPreReleaseVersions()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile, true);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(7, 0, 0, 0),
            FixedSuffix = "preview",
            DynamicSuffix = "001"
        };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt");
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string content = File.ReadAllText(sampleFile);
        Assert.Contains("Version = 7.0.0.0-preview001", content);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void NonRecursive_OnlyProcessesTopLevelFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile1, true);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile2, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(8, 0, 0, 0) };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt")
            .DisableGlobber()
            .NonRecursive() as TextFilePatcherConfig ?? throw new InvalidOperationException();
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("Version = 8.0.0", content1);
        
        // Subdir file should NOT be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.Contains("vErSiOn =         1", content2);
        Assert.DoesNotContain("Version = 8.0.0.0", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Recursive_ProcessesAllFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile1, true);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile2, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(9, 0, 0, 0) };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt")
            .DisableGlobber()
            .Recursive() as TextFilePatcherConfig ?? throw new InvalidOperationException();
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("Version = 9.0.0", content1);
        
        // Subdir file should also be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.Contains("Version = 9.0.0", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void EnableGlobber_UsesGlobPatternMatching()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        
        // Create directory structure with sample files at different levels
        var sampleFile1 = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile1, true);
        
        var subDir1 = Directory.CreateDirectory(Path.Combine(testDir, "Level1"));
        var sampleFile2 = Path.Combine(subDir1.FullName, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile2, true);
        
        var subDir2 = Directory.CreateDirectory(Path.Combine(subDir1.FullName, "Level2"));
        var sampleFile3 = Path.Combine(subDir2.FullName, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile3, true);
        
        var version = new VersionInformation { CanonicalPart = new Version(10, 0, 0, 0) };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("**/*.txt") // Glob pattern to match all txt files in any directory
            .EnableGlobber() as TextFilePatcherConfig ?? throw new InvalidOperationException();
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // All files should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("Version = 10.0.0", content1);
        
        string content2 = File.ReadAllText(sampleFile2);
        Assert.Contains("Version = 10.0.0", content2);
        
        string content3 = File.ReadAllText(sampleFile3);
        Assert.Contains("Version = 10.0.0", content3);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_PreservesFileFormatting()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleVersionInfo3.txt");
        File.Copy(Path.Combine(TestFilesDir, "SampleVersionInfo3.txt"), sampleFile, true);
        
        string originalContent = File.ReadAllText(sampleFile);
        var version = new VersionInformation { CanonicalPart = new Version(14, 0, 0, 0) };
        var config = new TextFilePatcherConfig()
            .WithCustomFilter("*.txt");
        var patcher = new TextFilePatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string newContent = File.ReadAllText(sampleFile);
        
        // Comments should be preserved
        Assert.Contains("# this is a comment", newContent);
        Assert.Contains("// This is another comment", newContent);
        Assert.Contains("// This is commented:", newContent);
        Assert.Contains("//Version=2.3.4.5", newContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
}