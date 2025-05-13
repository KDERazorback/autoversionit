using AutoVersionIt.Sources;
using AutoVersionIt.Sources.Configuration;

namespace AutoVersionIt.Tests.Sources;

public class FileBasedSimpleVersionControlTests
{
    private readonly string _filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");
    
    [Fact]
    public Task ShouldReadVersionFromSampleVersionInfo1()
    {
        // Arrange
        var reader = new VersionReader();
        var filePath = Path.Combine(_filesDirectory, "SampleVersionInfo1.txt");
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(filePath), reader);
        
        // Act
        var versionInfo = fileControl.GetCurrentVersion();
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldReadVersionFromSampleVersionInfo2()
    {
        // Arrange
        var reader = new VersionReader();
        var filePath = Path.Combine(_filesDirectory, "SampleVersionInfo2.txt");
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(filePath), reader);
        
        // Act
        var versionInfo = fileControl.GetCurrentVersion();
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldReadVersionFromSampleVersionInfo3()
    {
        // Arrange
        var reader = new VersionReader();
        var filePath = Path.Combine(_filesDirectory, "SampleVersionInfo3.txt");
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(filePath), reader);
        
        // Act
        var versionInfo = fileControl.GetCurrentVersion();
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldHandleEmptyFileWithSampleVersionInfo4()
    {
        // Arrange
        var reader = new VersionReader().IgnoreEmpty();
        var filePath = Path.Combine(_filesDirectory, "SampleVersionInfo4.txt");
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(filePath), reader);
        
        // Act
        var versionInfo = fileControl.GetCurrentVersion();
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public void ShouldWriteVersionToTemporaryFile()
    {
        // Arrange
        var reader = new VersionReader();
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"VersionTest_{Guid.NewGuid()}.txt");
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(tempFilePath), reader);
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "5"
        };
        
        try
        {
            // Act
            fileControl.SetNewVersion(versionInfo);
            
            // Assert
            Assert.True(File.Exists(tempFilePath));
            var content = File.ReadAllText(tempFilePath);
            Assert.Contains("Version = 1.2.3.4-beta5", content);
            
            // Verify we can read what we wrote
            var readVersionInfo = fileControl.GetCurrentVersion();
            Assert.Equal(1, readVersionInfo.CanonicalPart.Major);
            Assert.Equal(2, readVersionInfo.CanonicalPart.Minor);
            Assert.Equal(3, readVersionInfo.CanonicalPart.Build);
            Assert.Equal(4, readVersionInfo.CanonicalPart.Revision);
            Assert.Equal("beta", readVersionInfo.FixedSuffix);
            Assert.Equal("5", readVersionInfo.DynamicSuffix);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ShouldOverwriteExistingVersionInFile()
    {
        // Arrange
        var reader = new VersionReader();
        var sourceFilePath = Path.Combine(_filesDirectory, "SampleVersionInfo1.txt");
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"VersionTest_{Guid.NewGuid()}.txt");
        
        // Copy the source file to a temporary location
        File.Copy(sourceFilePath, tempFilePath, overwrite: true);
        
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(tempFilePath), reader);
        var initialVersionInfo = fileControl.GetCurrentVersion();
        var newVersionInfo = new VersionInformation
        {
            CanonicalPart = new Version(9, 8, 7, 6),
            FixedSuffix = "rc",
            DynamicSuffix = "2"
        };
        
        try
        {
            // Act
            fileControl.SetNewVersion(newVersionInfo);
            
            // Assert
            var updatedVersionInfo = fileControl.GetCurrentVersion();
            Assert.NotEqual(initialVersionInfo.CanonicalPart, updatedVersionInfo.CanonicalPart);
            Assert.Equal(9, updatedVersionInfo.CanonicalPart.Major);
            Assert.Equal(8, updatedVersionInfo.CanonicalPart.Minor);
            Assert.Equal(7, updatedVersionInfo.CanonicalPart.Build);
            Assert.Equal(6, updatedVersionInfo.CanonicalPart.Revision);
            Assert.Equal("rc", updatedVersionInfo.FixedSuffix);
            Assert.Equal("2", updatedVersionInfo.DynamicSuffix);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ShouldCreateNewFileWhenTargetDoesNotExist()
    {
        // Arrange
        var reader = new VersionReader();
        var tempDir = Path.Combine(Path.GetTempPath(), $"VersionTestDir_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var tempFilePath = Path.Combine(tempDir, "NewVersion.txt");
        
        // Ensure the file doesn't exist
        if (File.Exists(tempFilePath))
            File.Delete(tempFilePath);
        
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(tempFilePath), reader);
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(3, 2, 1, 0)
        };
        
        try
        {
            // Act
            fileControl.SetNewVersion(versionInfo);
            
            // Assert
            Assert.True(File.Exists(tempFilePath));
            var readVersionInfo = fileControl.GetCurrentVersion();
            Assert.Equal(3, readVersionInfo.CanonicalPart.Major);
            Assert.Equal(2, readVersionInfo.CanonicalPart.Minor);
            Assert.Equal(1, readVersionInfo.CanonicalPart.Build);
            Assert.Equal(0, readVersionInfo.CanonicalPart.Revision);
            Assert.Empty(readVersionInfo.FixedSuffix);
            Assert.Empty(readVersionInfo.DynamicSuffix);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
    
    [Fact]
    public void ShouldCreateDirectoryStructureWhenMissing()
    {
        // Arrange
        var reader = new VersionReader();
        var tempBaseDir = Path.Combine(Path.GetTempPath(), $"VersionTestBase_{Guid.NewGuid()}");
        var nestedDir = Path.Combine(tempBaseDir, "Level1", "Level2");
        var tempFilePath = Path.Combine(nestedDir, "Version.txt");
        
        // Ensure directory structure doesn't exist
        if (Directory.Exists(tempBaseDir))
            Directory.Delete(tempBaseDir, recursive: true);
        
        var fileControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(tempFilePath), reader);
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(5, 4, 3, 2),
            FixedSuffix = "alpha"
        };
        
        try
        {
            // Act
            fileControl.SetNewVersion(versionInfo);
            
            Assert.True(Directory.Exists(nestedDir));
            Assert.True(File.Exists(tempFilePath));
            var readVersionInfo = fileControl.GetCurrentVersion();
            Assert.Equal(5, readVersionInfo.CanonicalPart.Major);
            Assert.Equal(4, readVersionInfo.CanonicalPart.Minor);
            Assert.Equal(3, readVersionInfo.CanonicalPart.Build);
            Assert.Equal(2, readVersionInfo.CanonicalPart.Revision);
            Assert.Equal("alpha", readVersionInfo.FixedSuffix);
            Assert.Empty(readVersionInfo.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempBaseDir))
                Directory.Delete(tempBaseDir, recursive: true);
        }
    }
    
    [Theory]
    [InlineData("SampleVersionInfo1.txt")]
    [InlineData("SampleVersionInfo2.txt")]
    [InlineData("SampleVersionInfo3.txt")]
    public void ShouldRoundTripVersionInformationUsingFile(string sampleFileName)
    {
        // Arrange
        var reader = new VersionReader();
        var sampleFile = Path.Combine(_filesDirectory, sampleFileName);
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"VersionTest_{Guid.NewGuid()}.txt");
    
        try
        {
            // Read original version
            var sourceControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(sampleFile), reader);
            var originalVersion = sourceControl.GetCurrentVersion();
        
            // Write to temp file
            var tempControl = new FileBasedSimpleVersionControl(new FileBasedSimpleVersionControlConfig(tempFilePath), reader);
            tempControl.SetNewVersion(originalVersion);
        
            // Read back the written version
            var roundTrippedVersion = tempControl.GetCurrentVersion();
        
            Assert.Equal(originalVersion.CanonicalPart.Major, roundTrippedVersion.CanonicalPart.Major);
            Assert.Equal(originalVersion.CanonicalPart.Minor, roundTrippedVersion.CanonicalPart.Minor);
            Assert.Equal(originalVersion.CanonicalPart.Build, roundTrippedVersion.CanonicalPart.Build);
            Assert.Equal(originalVersion.CanonicalPart.Revision, roundTrippedVersion.CanonicalPart.Revision);
            Assert.Equal(originalVersion.FixedSuffix, roundTrippedVersion.FixedSuffix);
            Assert.Equal(originalVersion.DynamicSuffix, roundTrippedVersion.DynamicSuffix);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}