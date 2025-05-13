using AutoVersionIt.Interop;
using AutoVersionIt.Sources;
using Moq;

namespace AutoVersionIt.Tests.Sources;

public class GitTagVersionControlTests
{
    [Fact]
    public void GetCurrentVersion_ShouldReturnVersionFromGitTag()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();

        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns("1.2.3.4");
        stderrMock.Setup(s => s.ReadToEnd()).Returns(string.Empty);
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(0);

        // Act
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        var result = git.GetCurrentVersion();
            
        // Assert
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Once);
        processMock.Verify(x => x.WaitForExit(), Times.Once);
        Verify(result);
    }

    [Fact]
    public void GetCurrentVersion_WhenNoTagsFound_ShouldThrowException()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();
        
        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns("");
        stderrMock.Setup(s => s.ReadToEnd()).Returns("");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(128);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act & Assert
        Assert.Throws<Exception>(() => git.GetCurrentVersion());
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Once);
        processMock.Verify(x => x.WaitForExit(), Times.Once);
    }
        
    [Fact] 
    public void GetCurrentVersion_WhenGitCommandFails_ShouldThrowException()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();

        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns("");
        stderrMock.Setup(s => s.ReadToEnd()).Returns("fatal: No names found, cannot describe anything.");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(128);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act & Assert
        Assert.Throws<Exception>(() => git.GetCurrentVersion());
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce); 
        processMock.Verify(x => x.Run(), Times.Once);
        processMock.Verify(x => x.WaitForExit(), Times.Once);
    }
        
    [Fact]
    public void SetNewVersion_ShouldCreateGitTag()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(2, 0, 0, 0)
        };
        
        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns("abcdef1234567890");
        stderrMock.Setup(s => s.ReadToEnd()).Returns("");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(0);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act
        git.SetNewVersion(version);
        
        // Assert
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Exactly(2));
        processMock.Verify(x => x.WaitForExit(), Times.Exactly(2));
    }
        
    [Fact]
    public void GetHeadHash_ShouldReturnCommitHash()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();
        
        var expectedHash = "abcdef1234567890";
        
        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns(expectedHash);
        stderrMock.Setup(s => s.ReadToEnd()).Returns("");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(0);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act
        var result = git.GetHeadHash();
        
        // Assert
        Assert.Equal(expectedHash, result);
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Once);
        processMock.Verify(x => x.WaitForExit(), Times.Once);
    }
        
    [Fact]
    public void GetHeadVersion_WhenSuccessful_ShouldReturnCommitHash()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();
        
        var expectedHash = "abcdef1234567890";
        
        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns(expectedHash);
        stderrMock.Setup(s => s.ReadToEnd()).Returns("");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(0);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act
        var result = git.GetHeadVersion();
        
        // Assert
        Assert.Equal(expectedHash, result);
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Once);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Once);
    }
        
    [Fact]
    public void GetHeadVersion_WhenFailed_ShouldReturnEmptyString()
    {
        // Arrange
        var processFactoryMock = new Mock<IChildProcessFactory>();
        var processMock = new Mock<ChildProcess>();
        var stdoutMock = new Mock<StreamReader>(new MemoryStream());
        var stderrMock = new Mock<StreamReader>(new MemoryStream());
        var versionReader = new VersionReader();
        
        // Setup
        processFactoryMock.Setup(f => f.Create()).Returns(processMock.Object);
        stdoutMock.Setup(s => s.ReadToEnd()).Returns("");
        stderrMock.Setup(s => s.ReadToEnd()).Returns("fatal: not a git repository");
        processMock.Setup(p => p.StdOut()).Returns(stdoutMock.Object);
        processMock.Setup(p => p.StdErr()).Returns(stderrMock.Object);
        processMock.Setup(p => p.Run()).Returns(processMock.Object);
        processMock.Setup(p => p.WaitForExit()).Returns(processMock.Object);
        processMock.Setup(p => p.ExitCode()).Returns(128);
        
        var git = new GitTagVersionControl(versionReader, processFactoryMock.Object);
        
        // Act
        var result = git.GetHeadVersion();
        
        // Assert
        Assert.Empty(result);
        stdoutMock.Verify(x => x.ReadToEnd(), Times.Never);
        processMock.Verify(x => x.ExitCode(), Times.AtLeastOnce);
        processMock.Verify(x => x.Run(), Times.Once);
    }
}