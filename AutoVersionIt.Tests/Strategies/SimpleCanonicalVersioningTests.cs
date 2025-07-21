using AutoVersionIt.Strategies;
using Microsoft.Extensions.Configuration;

namespace AutoVersionIt.Tests.Strategies;

public class SimpleCanonicalVersioningTests
{
    private static SimpleCanonicalVersioning GetStrategy()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("suffix", "rc")
            ])
            .Build();
        var versioning = new SimpleCanonicalVersioning(config);
        return versioning;
    }
    
    [Fact]
    public void Increment_MajorVersion_ShouldIncreaseMajorAndResetOthers()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Major);

        // Assert
        Assert.Equal(2, result.CanonicalPart.Major);
        Assert.Equal(0, result.CanonicalPart.Minor);
        Assert.Equal(0, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }

    [Fact]
    public void Increment_MinorVersion_ShouldIncreaseMinorAndResetLowerComponents()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "2"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Minor);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(3, result.CanonicalPart.Minor);
        Assert.Equal(0, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }

    [Fact]
    public void Increment_BuildVersion_ShouldIncreaseBuildAndResetRevision()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha",
            DynamicSuffix = "5"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Build);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(2, result.CanonicalPart.Minor);
        Assert.Equal(4, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }

    [Fact]
    public void Increment_RevisionVersion_ShouldIncreaseRevisionOnly()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "preview",
            DynamicSuffix = "3"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Revision);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(2, result.CanonicalPart.Minor);
        Assert.Equal(3, result.CanonicalPart.Build);
        Assert.Equal(5, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }

    [Fact]
    public void Increment_SuffixVersion_ShouldThrowNotSupportedException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => versioning.Increment(versionInfo, VersionBumpType.Suffix));
    }

    [Fact]
    public void Decrement_MajorVersion_ShouldDecreaseMajorAndResetOthers()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 5),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };

        // Act
        var result = versioning.Decrement(versionInfo, VersionBumpType.Major);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(0, result.CanonicalPart.Minor);
        Assert.Equal(0, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }
    
    [Fact]
    public void Decrement_MajorVersionZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(0, 3, 4, 5)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => versioning.Decrement(versionInfo, VersionBumpType.Major));
    }

    [Fact]
    public void Decrement_MinorVersion_ShouldDecreaseMinorAndResetLowerComponents()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 5),
            FixedSuffix = "rc",
            DynamicSuffix = "2"
        };

        // Act
        var result = versioning.Decrement(versionInfo, VersionBumpType.Minor);

        // Assert
        Assert.Equal(2, result.CanonicalPart.Major);
        Assert.Equal(2, result.CanonicalPart.Minor);
        Assert.Equal(0, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }
    
    [Fact]
    public void Decrement_MinorVersionZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 4, 5)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => versioning.Decrement(versionInfo, VersionBumpType.Minor));
    }

    [Fact]
    public void Decrement_BuildVersion_ShouldDecreaseBuildAndResetRevision()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 5),
            FixedSuffix = "alpha",
            DynamicSuffix = "5"
        };

        // Act
        var result = versioning.Decrement(versionInfo, VersionBumpType.Build);

        // Assert
        Assert.Equal(2, result.CanonicalPart.Major);
        Assert.Equal(3, result.CanonicalPart.Minor);
        Assert.Equal(3, result.CanonicalPart.Build);
        Assert.Equal(0, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }
    
    [Fact]
    public void Decrement_BuildVersionZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 0, 5)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => versioning.Decrement(versionInfo, VersionBumpType.Build));
    }

    [Fact]
    public void Decrement_RevisionVersion_ShouldDecreaseRevisionOnly()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 5),
            FixedSuffix = "preview",
            DynamicSuffix = "3"
        };

        // Act
        var result = versioning.Decrement(versionInfo, VersionBumpType.Revision);

        // Assert
        Assert.Equal(2, result.CanonicalPart.Major);
        Assert.Equal(3, result.CanonicalPart.Minor);
        Assert.Equal(4, result.CanonicalPart.Build);
        Assert.Equal(4, result.CanonicalPart.Revision);
        Assert.Equal(string.Empty, result.FixedSuffix);
        Assert.Equal(string.Empty, result.DynamicSuffix);
    }
    
    [Fact]
    public void Decrement_RevisionVersionZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 0)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => versioning.Decrement(versionInfo, VersionBumpType.Revision));
    }

    [Fact]
    public void Decrement_SuffixVersion_ShouldThrowNotSupportedException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(2, 3, 4, 5),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => versioning.Decrement(versionInfo, VersionBumpType.Suffix));
    }

    [Fact]
    public void IsGreaterThan_HigherMajorVersion_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0)
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 0, 0, 0)
        };

        // Act
        var result = versioning.IsGreaterThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_SameMajorHigherMinor_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 0, 0)
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 1, 0, 0)
        };

        // Act
        var result = versioning.IsGreaterThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_DifferentSuffixes_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0),
            FixedSuffix = "beta"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 0, 0, 0),
            FixedSuffix = "alpha"
        };

        // Act
        var result = versioning.IsGreaterThan(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsLessThan_LowerMajorVersion_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 0, 0, 0)
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0)
        };

        // Act
        var result = versioning.IsLessThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLessThan_DifferentSuffixes_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 0, 0, 0),
            FixedSuffix = "beta"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0),
            FixedSuffix = "alpha"
        };

        // Act
        var result = versioning.IsLessThan(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_SameVersions_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4)
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4)
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_SameVersionsDifferentSuffixes_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta"
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_SameVersionsSameSuffixesDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "BETA"
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SameNonCanonicalComponents_DifferentDynamicSuffixes_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "2"
        };

        // Act
        var privateMethod = typeof(SimpleCanonicalVersioning).GetMethod("SameNonCanonicalComponents", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (bool?)privateMethod?.Invoke(versioning, new object[] { versionInfo1, versionInfo2 });

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ShouldRemoveSuffix_WhenSourceVersionHasSuffix()
    {
        // Arrange
        var versioning = GetStrategy();
        versioning.WithDefaultFixedSuffix("beta");
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "4"
        };
        
        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Revision);
        
        // Assert
        Assert.Equal("1.2.3.5", result.ToString());
        Assert.Empty(result.FixedSuffix);
        Assert.Empty(result.DynamicSuffix);
    }
}