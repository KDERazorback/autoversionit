using AutoVersionIt.Sources;
using AutoVersionIt.Sources.Configuration;

namespace AutoVersionIt.Tests.Sources;

public class EnvironmentVariableVersionControlTests
{
    private const string TestEnvVarName = "AUTOVERSIONING_TEST_VAR";
    
    [Fact]
    public void Constructor_WithNullEnvironmentVarName_ThrowsArgumentNullException()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EnvironmentVariableVersionControl(null!, reader));
    }
    
    [Fact]
    public void Constructor_WithEmptyEnvironmentVarName_ThrowsArgumentNullException()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(string.Empty), reader));
    }
    
    [Fact]
    public void Constructor_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(TestEnvVarName),null!));
    }
    
    [Fact]
    public void Constructor_WithValidParameters_InitializesProperties()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(TestEnvVarName), reader);
        
        // Assert
        Assert.Equal(TestEnvVarName, control.EnvironmentVariableName);
        Assert.Same(reader, control.VersionReader);
    }
    
    [Fact]
    public void GetCurrentVersion_WhenVariableNotSet_ReturnsDefaultVersion()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        // Act
        var version = control.GetCurrentVersion();
        
        // Assert
        Assert.Equal(0, version.CanonicalPart.Major);
        Assert.Equal(0, version.CanonicalPart.Minor);
        Assert.Equal(0, version.CanonicalPart.Build);
        Assert.Equal(0, version.CanonicalPart.Revision);
        Assert.Empty(version.FixedSuffix);
        Assert.Empty(version.DynamicSuffix);
    }
    
    [Fact]
    public void GetCurrentVersion_WhenVariableSetToEmptyString_ReturnsDefaultVersion()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(0, version.CanonicalPart.Major);
            Assert.Equal(0, version.CanonicalPart.Minor);
            Assert.Equal(0, version.CanonicalPart.Build);
            Assert.Equal(0, version.CanonicalPart.Revision);
            Assert.Empty(version.FixedSuffix);
            Assert.Empty(version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void GetCurrentVersion_WithSimpleVersion_ParsesCorrectly()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "1.2.3.4");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(1, version.CanonicalPart.Major);
            Assert.Equal(2, version.CanonicalPart.Minor);
            Assert.Equal(3, version.CanonicalPart.Build);
            Assert.Equal(4, version.CanonicalPart.Revision);
            Assert.Empty(version.FixedSuffix);
            Assert.Empty(version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void GetCurrentVersion_WithPreReleaseVersion_ParsesCorrectly()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "2.3.4.5-beta");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(2, version.CanonicalPart.Major);
            Assert.Equal(3, version.CanonicalPart.Minor);
            Assert.Equal(4, version.CanonicalPart.Build);
            Assert.Equal(5, version.CanonicalPart.Revision);
            Assert.Equal("beta", version.FixedSuffix);
            Assert.Empty(version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void GetCurrentVersion_WithComplexVersion_ParsesCorrectly()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "3.4.5.6-alpha001");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(3, version.CanonicalPart.Major);
            Assert.Equal(4, version.CanonicalPart.Minor);
            Assert.Equal(5, version.CanonicalPart.Build);
            Assert.Equal(6, version.CanonicalPart.Revision);
            Assert.Equal("alpha", version.FixedSuffix);
            Assert.Equal("001", version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void GetCurrentVersion_WithWhitespace_TrimsAndParsesCorrectly()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "  4.5.6.7-rc  ");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(4, version.CanonicalPart.Major);
            Assert.Equal(5, version.CanonicalPart.Minor);
            Assert.Equal(6, version.CanonicalPart.Build);
            Assert.Equal(7, version.CanonicalPart.Revision);
            Assert.Equal("rc", version.FixedSuffix);
            Assert.Empty(version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void GetCurrentVersion_RemovesInternalWhitespace()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "5.6.7.8 - preview");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        
        try
        {
            // Act
            var version = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(5, version.CanonicalPart.Major);
            Assert.Equal(6, version.CanonicalPart.Minor);
            Assert.Equal(7, version.CanonicalPart.Build);
            Assert.Equal(8, version.CanonicalPart.Revision);
            Assert.Equal("preview", version.FixedSuffix);
            Assert.Empty(version.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void SetNewVersion_WithSimpleVersion_SetsEnvironmentVariable()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        var version = new VersionInformation { CanonicalPart = new Version(6, 7, 8, 9) };
        
        try
        {
            // Act
            control.SetNewVersion(version);
            
            // Assert
            var envValue = Environment.GetEnvironmentVariable(uniqueEnvVar);
            Assert.Equal("6.7.8.9", envValue);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void SetNewVersion_WithPreReleaseVersion_SetsEnvironmentVariable()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(7, 8, 9, 0),
            FixedSuffix = "beta"
        };
        
        try
        {
            // Act
            control.SetNewVersion(version);
            
            // Assert
            var envValue = Environment.GetEnvironmentVariable(uniqueEnvVar);
            Assert.Equal("7.8.9.0-beta", envValue);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void SetNewVersion_WithComplexVersion_SetsEnvironmentVariable()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(8, 9, 0, 1),
            FixedSuffix = "alpha",
            DynamicSuffix = "002"
        };
        
        try
        {
            // Act
            control.SetNewVersion(version);
            
            // Assert
            var envValue = Environment.GetEnvironmentVariable(uniqueEnvVar);
            Assert.Equal("8.9.0.1-alpha002", envValue);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void RoundTrip_EnsuresCorrectVersionIsMaintained()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        var originalVersion = new VersionInformation 
        { 
            CanonicalPart = new Version(9, 8, 7, 6),
            FixedSuffix = "rc",
            DynamicSuffix = "2"
        };
        
        try
        {
            // Act - Set and then get back
            control.SetNewVersion(originalVersion);
            var retrievedVersion = control.GetCurrentVersion();
            
            // Assert
            Assert.Equal(originalVersion.CanonicalPart.Major, retrievedVersion.CanonicalPart.Major);
            Assert.Equal(originalVersion.CanonicalPart.Minor, retrievedVersion.CanonicalPart.Minor);
            Assert.Equal(originalVersion.CanonicalPart.Build, retrievedVersion.CanonicalPart.Build);
            Assert.Equal(originalVersion.CanonicalPart.Revision, retrievedVersion.CanonicalPart.Revision);
            Assert.Equal(originalVersion.FixedSuffix, retrievedVersion.FixedSuffix);
            Assert.Equal(originalVersion.DynamicSuffix, retrievedVersion.DynamicSuffix);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
    
    [Fact]
    public void SetNewVersion_OverwritesExistingValue()
    {
        // Arrange
        var reader = new VersionReader();
        var uniqueEnvVar = $"{TestEnvVarName}_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(uniqueEnvVar, "1.0.0.0");
        var control = new EnvironmentVariableVersionControl(new EnvironmentVariableVersionControlConfig(uniqueEnvVar), reader);
        var newVersion = new VersionInformation { CanonicalPart = new Version(2, 0, 0, 0) };
        
        try
        {
            // Act
            control.SetNewVersion(newVersion);
            
            // Assert
            var envValue = Environment.GetEnvironmentVariable(uniqueEnvVar);
            Assert.Equal("2.0.0", envValue);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(uniqueEnvVar, null);
        }
    }
}