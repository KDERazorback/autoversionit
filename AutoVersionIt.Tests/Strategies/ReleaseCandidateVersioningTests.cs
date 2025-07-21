using AutoVersionIt.Strategies;
using Microsoft.Extensions.Configuration;

namespace AutoVersionIt.Tests.Strategies;

public class ReleaseCandidateVersioningTests
{
    private static ReleaseCandidateVersioning GetStrategy()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("suffix", "rc")
            ])
            .Build();
        var versioning = new ReleaseCandidateVersioning(config);
        return versioning;
    }
    
    [Fact]
    public void Increment_SuffixVersion_ShouldIncreaseDynamicSuffixOnly()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "5"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Suffix);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(2, result.CanonicalPart.Minor);
        Assert.Equal(3, result.CanonicalPart.Build);
        Assert.Equal(4, result.CanonicalPart.Revision);
        Assert.Equal("rc", result.FixedSuffix);
        Assert.Equal("6", result.DynamicSuffix);
    }

    [Theory]
    [InlineData(VersionBumpType.Major)]
    [InlineData(VersionBumpType.Minor)]
    [InlineData(VersionBumpType.Build)]
    [InlineData(VersionBumpType.Revision)]
    public void Increment_NonSuffixVersion_ShouldThrowNotSupportedException(VersionBumpType versionBumpType)
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "5"
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => versioning.Increment(versionInfo, versionBumpType));
    }

    [Fact]
    public void Increment_SuffixVersionWithLargeNumber_ShouldIncreaseDynamicSuffixCorrectly()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "999"
        };

        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Suffix);

        // Assert
        Assert.Equal("1000", result.DynamicSuffix);
        Assert.Equal("beta", result.FixedSuffix);
        Assert.Equal(new Version(1, 2, 3, 4), result.CanonicalPart);
    }

    [Fact]
    public void Decrement_SuffixVersion_ShouldDecreaseDynamicSuffixOnly()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "5"
        };

        // Act
        var result = versioning.Decrement(versionInfo, VersionBumpType.Suffix);

        // Assert
        Assert.Equal(1, result.CanonicalPart.Major);
        Assert.Equal(2, result.CanonicalPart.Minor);
        Assert.Equal(3, result.CanonicalPart.Build);
        Assert.Equal(4, result.CanonicalPart.Revision);
        Assert.Equal("rc", result.FixedSuffix);
        Assert.Equal("4", result.DynamicSuffix);
    }

    [Fact]
    public void Decrement_MajorVersion_ShouldThrowNotSupportedException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "5"
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => versioning.Decrement(versionInfo, VersionBumpType.Major));
    }

    [Fact]
    public void Decrement_SuffixVersionToZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "0"
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => versioning.Decrement(versionInfo, VersionBumpType.Suffix));
    }

    [Fact]
    public void IsGreaterThan_HigherMajorVersion_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0),
            FixedSuffix = "rc",
            DynamicSuffix = "1"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 0, 0, 0),
            FixedSuffix = "rc",
            DynamicSuffix = "9"
        };

        // Act
        var result = versioning.IsGreaterThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_SameVersionHigherDynamicSuffix_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "10"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "9"
        };

        // Act
        var result = versioning.IsGreaterThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_SameVersionLowerDynamicSuffix_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "5"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "rc",
            DynamicSuffix = "10"
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
            CanonicalPart = new Version(1, 0, 0, 0),
            FixedSuffix = "rc",
            DynamicSuffix = "10"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(2, 0, 0, 0),
            FixedSuffix = "rc",
            DynamicSuffix = "1"
        };

        // Act
        var result = versioning.IsLessThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLessThan_SameVersionLowerDynamicSuffix_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha",
            DynamicSuffix = "5"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha",
            DynamicSuffix = "6"
        };

        // Act
        var result = versioning.IsLessThan(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLessThan_SameVersionHigherDynamicSuffix_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha",
            DynamicSuffix = "20"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "alpha",
            DynamicSuffix = "19"
        };

        // Act
        var result = versioning.IsLessThan(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_SameVersionsSameDynamicSuffix_ShouldReturnTrue()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "7"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "7"
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_SameVersionsDifferentDynamicSuffix_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "7"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "8"
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_DifferentVersionsSameDynamicSuffix_ShouldReturnFalse()
    {
        // Arrange
        var versioning = GetStrategy();
        var versionInfo1 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = "beta",
            DynamicSuffix = "7"
        };
        var versionInfo2 = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 5),
            FixedSuffix = "beta",
            DynamicSuffix = "7"
        };

        // Act
        var result = versioning.IsEqualTo(versionInfo1, versionInfo2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldAddSuffix_WhenSourceVersionHasEmptySuffix()
    {
        // Arrange
        var versioning = GetStrategy();
        versioning.WithDefaultFixedSuffix("beta");
        var versionInfo = new VersionInformation
        {
            CanonicalPart = new Version(1, 2, 3, 4),
            FixedSuffix = string.Empty,
            DynamicSuffix = string.Empty
        };
        
        // Act
        var result = versioning.Increment(versionInfo, VersionBumpType.Suffix);
        
        // Assert
        Assert.Equal("1.2.3.4-beta1", result.ToString());
        Assert.Equal("beta", result.FixedSuffix);
        Assert.Equal("1", result.DynamicSuffix);
    }
}