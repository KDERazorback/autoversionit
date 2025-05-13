using AutoVersionIt.Sources;

namespace AutoVersionIt.Tests;

public class VersionReaderTests
{
    [Fact]
    public Task ShouldParseVersionWithOnlyMajorAndMinor()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act
        var versionInfo = reader.FromString("1.2");
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldParseFullVersionWithSuffix()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act
        var versionInfo = reader.FromString("1.2.3.4-rc5");
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldRespectDefaultFixedSuffix()
    {
        // Arrange
        var reader = new VersionReader().WithDefaultFixedSuffix("beta");
        
        // Act
        var versionInfo = reader.FromString("1.2.3.4");
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public void ShouldThrowWhenEmptyStringProvided()
    {
        // Arrange
        var reader = new VersionReader().ThrowIfEmpty();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => reader.FromString(""));
    }
    
    [Fact]
    public Task ShouldHandleEmptyStringWhenConfiguredToIgnore()
    {
        // Arrange
        var reader = new VersionReader().IgnoreEmpty();
        
        // Act
        var versionInfo = reader.FromString("");
        
        // Assert
        return Verify(versionInfo);
    }
    
    [Fact]
    public Task ShouldExtractFixedAndDynamicPartsFromSuffix()
    {
        // Arrange
        var reader = new VersionReader();
        
        // Act
        var versionInfo = reader.FromString("1.2.3.4-alpha123");
        
        // Assert
        return Verify(versionInfo);
    }
}