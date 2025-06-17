using System.Text;
using AutoVersionIt.Patches;
using AutoVersionIt.Patches.Configuration;

namespace AutoVersionIt.Tests.Patches;

public class NetCoreVersionPatcherTests
{
    private NetCoreVersionPatcherConfig _defaultConfig = new NetCoreVersionPatcherConfig()
        .InsertAttributesIfMissing()
        .PatchCSharpProjects()
        .PatchVbProjects()
        .EnableGlobber()
        .Recursive()
        .DetectFileKindByExtension() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
    
    private string PrepareTestDirectory()
    {
        // Create a temporary directory for test files
        string tempDir = Path.Combine(Path.GetTempPath(), "NetCoreVersionPatcherTests_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NetCoreVersionPatcher(null!, new NetCoreVersionPatcherConfig()));
    }
    
    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new NetCoreVersionPatcher("", _defaultConfig));
    }
    
    [Fact]
    public void Constructor_DoesNotAddAnyFiltersByDefault()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new NetCoreVersionPatcher(tempPath, new NetCoreVersionPatcherConfig());
        
        // Assert
        Assert.Empty(patcher.Config.Filters);
    }
    
    [Fact]
    public void PatchCSharpProjects_AddsCorrectFilters()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var config = new NetCoreVersionPatcherConfig()
            .PatchCSharpProjects()
            .EnableGlobber() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NetCoreVersionPatcher(tempPath, config);
        
        // Assert
        Assert.Single(patcher.Config.Filters);
        Assert.Equal("**/*.csproj", patcher.Config.Filters[0]);
    }
    
    [Fact]
    public void PatchVbProjects_AddsCorrectFilters()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var config = new NetCoreVersionPatcherConfig()
            .PatchVbProjects()
            .EnableGlobber() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NetCoreVersionPatcher(tempPath, config);
        
        // Assert
        Assert.Single(patcher.Config.Filters);
        Assert.Equal("**/*.vbproj", patcher.Config.Filters[0]);
    }
    
    [Fact]
    public void PatchCSProjFile_UpdatesExistingVersionAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

</Project>";

        File.WriteAllText(sampleFile, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(2, 1, 0, 0),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<AssemblyVersion>2.1.0.0</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>2.1.0.0</FileVersion>", updatedContent);
        Assert.Contains("<AssemblyInformationalVersion>2.1.0.0-beta1</AssemblyInformationalVersion>", updatedContent);
        Assert.DoesNotContain("<AssemblyVersion>1.0.0.0</AssemblyVersion>", updatedContent);
        Assert.DoesNotContain("<FileVersion>1.0.0.0</FileVersion>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchVBProjFile_UpdatesExistingVersionAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.vbproj");
        
        string vbprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <RootNamespace>SampleVbProject</RootNamespace>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

</Project>";

        File.WriteAllText(sampleFile, vbprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(3, 2, 1, 0),
            FixedSuffix = "alpha",
            DynamicSuffix = "2"
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.vbproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<AssemblyVersion>3.2.1.0</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>3.2.1.0</FileVersion>", updatedContent);
        Assert.Contains("<AssemblyInformationalVersion>3.2.1.0-alpha2</AssemblyInformationalVersion>", updatedContent);
        Assert.DoesNotContain("<AssemblyVersion>1.0.0.0</AssemblyVersion>", updatedContent);
        Assert.DoesNotContain("<FileVersion>1.0.0.0</FileVersion>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_WithMissingAttributes_InsertsNewAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>";

        File.WriteAllText(sampleFile, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(4, 3, 2, 1)
        };

        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj")
            .InsertAttributesIfMissing();
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<AssemblyVersion>4.3.2.1</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>4.3.2.1</FileVersion>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void IgnoreMissingAttributes_DoesNotInsertAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>";

        File.WriteAllText(sampleFile, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(5, 4, 3, 2)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj")
            .IgnoreMissingAttributes();
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.DoesNotContain("<AssemblyVersion>", updatedContent);
        Assert.DoesNotContain("<FileVersion>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_WithRealSampleFiles_UpdatesVersions()
    {
        // Copy sample files from the test directory to a temporary location
        var testDir = PrepareTestDirectory();
        var sampleCsProj = Path.Combine(testDir, "SampleNetCoreProj.csproj");
        var sampleVbProj = Path.Combine(testDir, "SampleNetCoreProj.vbproj");

        var sample1 = @"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <InformationalVersion>1.2.3.4</InformationalVersion>
    <FileVersion>1.0.0.0</FileVersion>
    </PropertyGroup>

    <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.FileSystemGlobbing"" Version=""9.0.4"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Console"" Version=""9.0.4"" />
    </ItemGroup>

</Project>";
        
        var sample2 = @"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.Extensions.FileSystemGlobbing"" Version=""9.0.4"" />
        <PackageReference Include=""Microsoft.Extensions.Logging.Console"" Version=""9.0.4"" />
    </ItemGroup>

</Project>";
        
        File.WriteAllText(sampleCsProj, sample1);
        File.WriteAllText(sampleVbProj, sample2);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(7, 6, 5, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "3"
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj")
            .WithCustomFilter("*.vbproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string csProjContent = File.ReadAllText(sampleCsProj);
        string vbProjContent = File.ReadAllText(sampleVbProj);
        
        Assert.Contains("<AssemblyVersion>7.6.5.4</AssemblyVersion>", csProjContent);
        Assert.Contains("<FileVersion>7.6.5.4</FileVersion>", csProjContent);
        Assert.Contains("<AssemblyInformationalVersion>7.6.5.4-rc3</AssemblyInformationalVersion>", csProjContent);
        Assert.Contains("<AssemblyVersion>7.6.5.4</AssemblyVersion>", vbProjContent);
        Assert.Contains("<FileVersion>7.6.5.4</FileVersion>", vbProjContent);
        Assert.Contains("<AssemblyInformationalVersion>7.6.5.4-rc3</AssemblyInformationalVersion>", vbProjContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchFile_PreservesProjectStructure()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>

</Project>";

        File.WriteAllText(sampleFile, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(8, 7, 6, 5)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        
        // Check that version is updated
        Assert.Contains("<AssemblyVersion>8.7.6.5</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>8.7.6.5</FileVersion>", updatedContent);
        
        // Check that other parts are preserved
        Assert.Contains("<TargetFramework>net6.0</TargetFramework>", updatedContent);
        Assert.Contains("<ImplicitUsings>enable</ImplicitUsings>", updatedContent);
        Assert.Contains("<Nullable>enable</Nullable>", updatedContent);
        Assert.Contains("<PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void NonRecursive_OnlyProcessesTopLevelFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleProject.csproj");
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>
</Project>";
        
        File.WriteAllText(sampleFile1, csprojContent);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleProject.csproj");
        File.WriteAllText(sampleFile2, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(9, 8, 7, 6)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj")
            .DisableGlobber()
            .NonRecursive() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("<AssemblyVersion>9.8.7.6</AssemblyVersion>", content1);
        Assert.Contains("<FileVersion>9.8.7.6</FileVersion>", content1);
        
        // Subdir file should NOT be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.DoesNotContain("<AssemblyVersion>9.8.7.6</AssemblyVersion>", content2);
        Assert.DoesNotContain("<FileVersion>9.8.7.6</FileVersion>", content2);
        Assert.Contains("<AssemblyVersion>1.0.0.0</AssemblyVersion>", content2);
        Assert.Contains("<FileVersion>1.0.0.0</FileVersion>", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Recursive_ProcessesAllFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "SampleProject.csproj");
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>
</Project>";
        
        File.WriteAllText(sampleFile1, csprojContent);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "SampleProject.csproj");
        File.WriteAllText(sampleFile2, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(10, 9, 8, 7)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("**/*.csproj")
            .Recursive()
            .EnableGlobber() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("<AssemblyVersion>10.9.8.7</AssemblyVersion>", content1);
        Assert.Contains("<FileVersion>10.9.8.7</FileVersion>", content1);
        
        // Subdir file should also be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.Contains("<AssemblyVersion>10.9.8.7</AssemblyVersion>", content2);
        Assert.Contains("<FileVersion>10.9.8.7</FileVersion>", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void EnableGlobber_UsesGlobPatternMatching()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        
        // Create a directory structure with nested project files
        var rootCsProj = Path.Combine(testDir, "Root.csproj");
        var subDir1 = Directory.CreateDirectory(Path.Combine(testDir, "Level1"));
        var level1CsProj = Path.Combine(subDir1.FullName, "Level1.csproj");
        var subDir2 = Directory.CreateDirectory(Path.Combine(subDir1.FullName, "Level2"));
        var level2CsProj = Path.Combine(subDir2.FullName, "Level2.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>
</Project>";
        
        File.WriteAllText(rootCsProj, csprojContent);
        File.WriteAllText(level1CsProj, csprojContent);
        File.WriteAllText(level2CsProj, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(11, 10, 9, 8)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("**/*.csproj")
            .EnableGlobber() as NetCoreVersionPatcherConfig ?? throw new InvalidOperationException();
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert - All files should be patched
        string rootContent = File.ReadAllText(rootCsProj);
        string level1Content = File.ReadAllText(level1CsProj);
        string level2Content = File.ReadAllText(level2CsProj);
        
        Assert.Contains("<AssemblyVersion>11.10.9.8</AssemblyVersion>", rootContent);
        Assert.Contains("<FileVersion>11.10.9.8</FileVersion>", rootContent);
        
        Assert.Contains("<AssemblyVersion>11.10.9.8</AssemblyVersion>", level1Content);
        Assert.Contains("<FileVersion>11.10.9.8</FileVersion>", level1Content);
        
        Assert.Contains("<AssemblyVersion>11.10.9.8</AssemblyVersion>", level2Content);
        Assert.Contains("<FileVersion>11.10.9.8</FileVersion>", level2Content);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void HandlesSelfClosingPropertyElements()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion/>
    <FileVersion/>
  </PropertyGroup>
</Project>";

        File.WriteAllText(sampleFile, csprojContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(12, 11, 10, 9),
            FixedSuffix = "preview"
        };

        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<AssemblyVersion>12.11.10.9</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>12.11.10.9</FileVersion>", updatedContent);
        Assert.Contains("<AssemblyInformationalVersion>12.11.10.9-preview</AssemblyInformationalVersion>", updatedContent);
        Assert.DoesNotContain("<AssemblyVersion/>", updatedContent);
        Assert.DoesNotContain("<FileVersion/>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Patch_PreservesFileEncoding()
    {
        // This test will create a UTF-8 file with BOM and check that the encoding is preserved
        
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "SampleProject.csproj");
        
        string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>
</Project>";

        // Write with UTF-8 and BOM
        using (var fileStream = new FileStream(sampleFile, FileMode.Create, FileAccess.Write))
        using (var writer = new StreamWriter(fileStream, new UTF8Encoding(true)))
        {
            writer.Write(csprojContent);
        }
        
        // Check that BOM exists before test
        bool hasBomBefore;
        using (var fileStream = new FileStream(sampleFile, FileMode.Open, FileAccess.Read))
        {
            var buffer = new byte[3];
            fileStream.ReadExactly(buffer, 0, 3);
            hasBomBefore = buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF;
        }
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(13, 12, 11, 10)
        };
        
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.csproj");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act
        patcher.Patch(version);
        
        // Assert
        
        // Check content is updated
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<AssemblyVersion>13.12.11.10</AssemblyVersion>", updatedContent);
        Assert.Contains("<FileVersion>13.12.11.10</FileVersion>", updatedContent);
        
        // Check that BOM is preserved
        bool hasBomAfter;
        using (var fileStream = new FileStream(sampleFile, FileMode.Open, FileAccess.Read))
        {
            var buffer = new byte[3];
            fileStream.ReadExactly(buffer, 0, 3);
            hasBomAfter = buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF;
        }
        
        Assert.Equal(hasBomBefore, hasBomAfter);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void UnsupportedFileExtension_ThrowsNotSupportedException()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var unsupportedFile = Path.Combine(testDir, "Unsupported.txt");
        
        string content = "This is not a project file";
        File.WriteAllText(unsupportedFile, content);
        
        var version = new VersionInformation { CanonicalPart = new Version(1, 0, 0, 0) };
        var config = new NetCoreVersionPatcherConfig()
            .WithCustomFilter("*.txt");
        var patcher = new NetCoreVersionPatcher(testDir, config);
        
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => patcher.Patch(version));
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
}