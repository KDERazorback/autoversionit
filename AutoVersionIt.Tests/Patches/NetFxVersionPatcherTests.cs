using System.Text.RegularExpressions;
using AutoVersionIt.Patches;

namespace AutoVersionIt.Tests.Patches;

public class NetFxVersionPatcherTests
{
    private const string TestFilesDir = "Files";
    
    private string PrepareTestDirectory()
    {
        // Create a temporary directory for test files
        string tempDir = Path.Combine(Path.GetTempPath(), "NetFxVersionPatcherTests_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
    
    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NetFxVersionPatcher(null!));
    }
    
    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new NetFxVersionPatcher(""));
    }
    
    [Fact]
    public void Constructor_DoesNotAddAnyFiltersByDefault()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new NetFxVersionPatcher(tempPath);
        
        // Assert
        Assert.Empty(patcher.Filters);
    }
    
    [Fact]
    public void PatchCSharpProjects_AddsCorrectFilters()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new NetFxVersionPatcher(tempPath)
            .EnableGlobber()
            .PatchCSharpProjects();
        
        // Assert
        Assert.Single(patcher.Filters);
        Assert.Equal("**/Properties/AssemblyInfo.cs", patcher.Filters[0]);
    }
    
    [Fact]
    public void PatchVbProjects_AddsCorrectFilters()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        
        // Act
        var patcher = new NetFxVersionPatcher(tempPath)
            .EnableGlobber()
            .PatchVbProjects();
        
        // Assert
        Assert.Equal(2, patcher.Filters.Count);
        Assert.Equal("**/Properties/AssemblyInfo.vb", patcher.Filters[0]);
        Assert.Equal("**/My Project/AssemblyInfo.vb", patcher.Filters[1]);
    }
    
    [Fact]
    public void PatchCSharpFile_UpdatesExistingVersionAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""SampleProject"")]
[assembly: AssemblyCopyright(""Copyright © 2023"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""12345678-1234-1234-1234-123456789012"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(2, 1, 0, 0),
            FixedSuffix = "beta",
            DynamicSuffix = "1"
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .DisableGlobber()
            .Recursive()
            .WithFilter("AssemblyInfo.cs");
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("[assembly: AssemblyVersion(\"2.1.0.0\")]", updatedContent);
        Assert.Contains("[assembly: AssemblyFileVersion(\"2.1.0.0\")]", updatedContent);
        Assert.Contains("[assembly: AssemblyInformationalVersion(\"2.1.0.0-beta1\")]", updatedContent);
        Assert.DoesNotContain("[assembly: AssemblyVersion(\"1.0.0.0\")]", updatedContent);
        Assert.DoesNotContain("[assembly: AssemblyFileVersion(\"1.0.0.0\")]", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchVbFile_UpdatesExistingVersionAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.vb");
        
        string vbContent = @"Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.
<Assembly: AssemblyTitle(""SampleVbProject"")>
<Assembly: AssemblyDescription("""")>
<Assembly: AssemblyCompany("""")>
<Assembly: AssemblyProduct(""SampleVbProject"")>
<Assembly: AssemblyCopyright(""Copyright © 2023"")>
<Assembly: AssemblyTrademark("""")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid(""87654321-4321-4321-4321-210987654321"")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers
' by using the '*' as shown below:
' <Assembly: AssemblyVersion(""1.0.*"")>

<Assembly: AssemblyVersion(""1.0.0.0"")>
<Assembly: AssemblyFileVersion(""1.0.0.0"")>";

        File.WriteAllText(sampleFile, vbContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(3, 2, 1, 0),
            FixedSuffix = "alpha",
            DynamicSuffix = "2"
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.vb");
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<Assembly: AssemblyVersion(\"3.2.1.0\")>", updatedContent);
        Assert.Contains("<Assembly: AssemblyFileVersion(\"3.2.1.0\")>", updatedContent);
        Assert.Contains("<Assembly: AssemblyInformationalVersion(\"3.2.1.0-alpha2\")>", updatedContent);
        Assert.DoesNotContain("<Assembly: AssemblyVersion(\"1.0.0.0\")>", updatedContent);
        Assert.DoesNotContain("<Assembly: AssemblyFileVersion(\"1.0.0.0\")>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchCSharpFile_WithMissingAttributes_InsertsNewAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""SampleProject"")]
[assembly: AssemblyCopyright(""Copyright © 2023"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""12345678-1234-1234-1234-123456789012"")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(4, 3, 2, 1)
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cs")
            .InsertAttributesIfMissing();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("[assembly: AssemblyVersion(\"4.3.2.1\")]", updatedContent);
        Assert.Contains("[assembly: AssemblyFileVersion(\"4.3.2.1\")]", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void PatchVbFile_WithMissingAttributes_InsertsNewAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.vb");
        
        string vbContent = @"Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.
<Assembly: AssemblyTitle(""SampleVbProject"")>
<Assembly: AssemblyDescription("""")>
<Assembly: AssemblyCompany("""")>
<Assembly: AssemblyProduct(""SampleVbProject"")>
<Assembly: AssemblyCopyright(""Copyright © 2023"")>
<Assembly: AssemblyTrademark("""")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid(""87654321-4321-4321-4321-210987654321"")>";

        File.WriteAllText(sampleFile, vbContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(5, 4, 3, 2),
            FixedSuffix = "rc",
            DynamicSuffix = "1"
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.vb")
            .InsertAttributesIfMissing();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("<Assembly: AssemblyVersion(\"5.4.3.2\")>", updatedContent);
        Assert.Contains("<Assembly: AssemblyFileVersion(\"5.4.3.2\")>", updatedContent);
        Assert.Contains("<Assembly: AssemblyInformationalVersion(\"5.4.3.2-rc1\")>", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void IgnoreMissingAttributes_DoesNotInsertAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""SampleProject"")]
[assembly: AssemblyCopyright(""Copyright © 2023"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(6, 5, 4, 3)
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cs")
            .IgnoreMissingAttributes();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.DoesNotContain("[assembly: AssemblyVersion(", updatedContent);
        Assert.DoesNotContain("[assembly: AssemblyFileVersion(", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void CheckUsingStatements_InsertsRequiredUsings()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(7, 6, 5, 4)
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cs")
            .CheckUsingStatements()
            .InsertAttributesIfMissing();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("using System.Reflection;", updatedContent);
        Assert.Contains("using System.Runtime.CompilerServices;", updatedContent);
        Assert.Contains("using System.Runtime.InteropServices;", updatedContent);
        Assert.Contains("[assembly: AssemblyVersion(\"7.6.5.4\")]", updatedContent);
        Assert.Contains("[assembly: AssemblyFileVersion(\"7.6.5.4\")]", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void IgnoreUsingStatements_DoesNotInsertUsings()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(8, 7, 6, 5)
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cs")
            .IgnoreUsingStatements()
            .InsertAttributesIfMissing();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.DoesNotContain("using System.Reflection;", updatedContent);
        Assert.DoesNotContain("using System.Runtime.CompilerServices;", updatedContent);
        Assert.DoesNotContain("using System.Runtime.InteropServices;", updatedContent);
        Assert.Contains("[assembly: AssemblyVersion(\"8.7.6.5\")]", updatedContent);
        Assert.Contains("[assembly: AssemblyFileVersion(\"8.7.6.5\")]", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void CppFile_UpdatesVersionAttributes()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cpp");
        
        string cppContent = @"using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;
using namespace System::Runtime::CompilerServices;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly:AssemblyTitleAttribute(L""SampleCppProject"")];
[assembly:AssemblyDescriptionAttribute(L"""")];
[assembly:AssemblyConfigurationAttribute(L"""")];
[assembly:AssemblyCompanyAttribute(L"""")];
[assembly:AssemblyProductAttribute(L""SampleCppProject"")];
[assembly:AssemblyCopyrightAttribute(L""Copyright (c) 2023"")];
[assembly:AssemblyTrademarkAttribute(L"""")];
[assembly:AssemblyCultureAttribute(L"""")];

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:

[assembly:AssemblyVersion(""1.0.0.0"")];
[assembly:AssemblyFileVersion(""1.0.0.0"")];";

        File.WriteAllText(sampleFile, cppContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(9, 8, 7, 6),
            FixedSuffix = "preview"
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cpp");
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        Assert.Contains("[assembly: AssemblyVersion(\"9.8.7.6\")];", updatedContent);
        Assert.Contains("[assembly: AssemblyFileVersion(\"9.8.7.6\")];", updatedContent);
        Assert.Contains("[assembly: AssemblyInformationalVersion(\"9.8.7.6-preview\")];", updatedContent);
        Assert.DoesNotContain("[assembly:AssemblyVersion(\"1.0.0.0\")];", updatedContent);
        Assert.DoesNotContain("[assembly:AssemblyFileVersion(\"1.0.0.0\")];", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void NonRecursive_OnlyProcessesTopLevelFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "AssemblyInfo.cs");
        string csharpContent = @"using System;
using System.Reflection;

[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";
        File.WriteAllText(sampleFile1, csharpContent);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "AssemblyInfo.cs");
        File.WriteAllText(sampleFile2, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(10, 9, 8, 7)
        };
        var patcher = new NetFxVersionPatcher(testDir)
            .DisableGlobber()
            .WithFilter("AssemblyInfo.cs")
            .NonRecursive();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("[assembly: AssemblyVersion(\"10.9.8.7\")]", content1);
        Assert.Contains("[assembly: AssemblyFileVersion(\"10.9.8.7\")]", content1);
        
        // Subdir file should NOT be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.DoesNotContain("[assembly: AssemblyVersion(\"10.9.8.7\")]", content2);
        Assert.DoesNotContain("[assembly: AssemblyFileVersion(\"10.9.8.7\")]", content2);
        Assert.Contains("[assembly: AssemblyVersion(\"1.0.0.0\")]", content2);
        Assert.Contains("[assembly: AssemblyFileVersion(\"1.0.0.0\")]", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Recursive_ProcessesAllFiles()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile1 = Path.Combine(testDir, "AssemblyInfo.cs");
        string csharpContent = @"using System;
using System.Reflection;

[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";
        File.WriteAllText(sampleFile1, csharpContent);
        
        // Create subdirectory with a copy of the sample file
        var subDir = Directory.CreateDirectory(Path.Combine(testDir, "Subdir"));
        var sampleFile2 = Path.Combine(subDir.FullName, "AssemblyInfo.cs");
        File.WriteAllText(sampleFile2, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(11, 10, 9, 8)
        };
        var patcher = new NetFxVersionPatcher(testDir)
            .DisableGlobber()
            .WithFilter("AssemblyInfo.cs")
            .Recursive();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        // Root file should be patched
        string content1 = File.ReadAllText(sampleFile1);
        Assert.Contains("[assembly: AssemblyVersion(\"11.10.9.8\")]", content1);
        Assert.Contains("[assembly: AssemblyFileVersion(\"11.10.9.8\")]", content1);
        
        // Subdir file should also be patched
        string content2 = File.ReadAllText(sampleFile2);
        Assert.Contains("[assembly: AssemblyVersion(\"11.10.9.8\")]", content2);
        Assert.Contains("[assembly: AssemblyFileVersion(\"11.10.9.8\")]", content2);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void EnableGlobber_UsesGlobPatternMatching()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        
        // Create proper directory structure for standard .NET Framework projects
        var propertiesDir = Directory.CreateDirectory(Path.Combine(testDir, "Properties"));
        var sampleFile = Path.Combine(propertiesDir.FullName, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;
using System.Reflection;

[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";
        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(12, 11, 10, 9)
        };
        var patcher = new NetFxVersionPatcher(testDir)
            .EnableGlobber()
            .PatchCSharpProjects();
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string content = File.ReadAllText(sampleFile);
        Assert.Contains("[assembly: AssemblyVersion(\"12.11.10.9\")]", content);
        Assert.Contains("[assembly: AssemblyFileVersion(\"12.11.10.9\")]", content);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Patch_PreservesFileFormatting()
    {
        // Arrange
        var testDir = PrepareTestDirectory();
        var sampleFile = Path.Combine(testDir, "AssemblyInfo.cs");
        
        string csharpContent = @"using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""SampleProject"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""SampleProject"")]
[assembly: AssemblyCopyright(""Copyright © 2023"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""12345678-1234-1234-1234-123456789012"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

        File.WriteAllText(sampleFile, csharpContent);
        
        var version = new VersionInformation 
        { 
            CanonicalPart = new Version(13, 12, 11, 10)
        };
        
        var patcher = new NetFxVersionPatcher(testDir)
            .WithFilter("AssemblyInfo.cs");
        
        // Act
        patcher.Patch(version);
        
        // Assert
        string updatedContent = File.ReadAllText(sampleFile);
        
        // Comments should be preserved
        Assert.Contains("// General Information about an assembly", updatedContent);
        Assert.Contains("// Setting ComVisible to false", updatedContent);
        Assert.Contains("// Version information for an assembly", updatedContent);
        
        // Cleanup
        Directory.Delete(testDir, true);
    }
}