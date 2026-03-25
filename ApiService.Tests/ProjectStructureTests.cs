using System.IO;
using System.Xml.Linq;
using Xunit;

namespace ApiService.Tests;

public class ProjectStructureTests
{
    private readonly string _workspaceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [Fact]
    public void Solution_File_Should_Exist()
    {
        var solutionPath = Path.Combine(_workspaceRoot, "TriageDashboard.sln");
        Assert.True(File.Exists(solutionPath), $"Solution file should exist at {solutionPath}");
    }

    [Fact]
    public void AppHost_Project_Should_Exist()
    {
        var projectPath = Path.Combine(_workspaceRoot, "AppHost", "AppHost.csproj");
        Assert.True(File.Exists(projectPath), $"AppHost project should exist at {projectPath}");
    }

    [Fact]
    public void ApiService_Project_Should_Exist()
    {
        var projectPath = Path.Combine(_workspaceRoot, "ApiService", "ApiService.csproj");
        Assert.True(File.Exists(projectPath), $"ApiService project should exist at {projectPath}");
    }

    [Fact]
    public void WorkerService_Project_Should_Exist()
    {
        var projectPath = Path.Combine(_workspaceRoot, "WorkerService", "WorkerService.csproj");
        Assert.True(File.Exists(projectPath), $"WorkerService project should exist at {projectPath}");
    }

    [Fact]
    public void ApiService_Tests_Project_Should_Exist()
    {
        var projectPath = Path.Combine(_workspaceRoot, "ApiService.Tests", "ApiService.Tests.csproj");
        Assert.True(File.Exists(projectPath), $"ApiService.Tests project should exist at {projectPath}");
    }

    [Fact]
    public void AppHost_Should_Reference_Aspire_Hosting_Package()
    {
        var projectPath = Path.Combine(_workspaceRoot, "AppHost", "AppHost.csproj");
        var doc = XDocument.Load(projectPath);

        // Check for either explicit package reference or Aspire SDK
        var packageReferences = doc.Descendants("PackageReference")
            .Where(x => x.Attribute("Include")?.Value == "Aspire.Hosting.AppHost")
            .ToList();

        var sdkAttribute = doc.Root?.Attribute("Sdk")?.Value;
        bool hasAspireSdk = sdkAttribute?.Contains("Aspire.AppHost.Sdk") == true;

        Assert.True(packageReferences.Any() || hasAspireSdk,
            "AppHost should reference Aspire.Hosting.AppHost package or use Aspire.AppHost.Sdk");
    }

    [Fact]
    public void AppHost_Should_Reference_ApiService_Project()
    {
        var projectPath = Path.Combine(_workspaceRoot, "AppHost", "AppHost.csproj");
        var doc = XDocument.Load(projectPath);
        var projectReferences = doc.Descendants("ProjectReference")
            .Where(x => x.Attribute("Include")?.Value.Contains("ApiService.csproj") == true)
            .ToList();

        Assert.NotEmpty(projectReferences);
    }

    [Fact]
    public void AppHost_Should_Reference_WorkerService_Project()
    {
        var projectPath = Path.Combine(_workspaceRoot, "AppHost", "AppHost.csproj");
        var doc = XDocument.Load(projectPath);
        var projectReferences = doc.Descendants("ProjectReference")
            .Where(x => x.Attribute("Include")?.Value.Contains("WorkerService.csproj") == true)
            .ToList();

        Assert.NotEmpty(projectReferences);
    }

    [Fact]
    public void ApiService_Tests_Should_Reference_ApiService_Project()
    {
        var projectPath = Path.Combine(_workspaceRoot, "ApiService.Tests", "ApiService.Tests.csproj");
        var doc = XDocument.Load(projectPath);
        var projectReferences = doc.Descendants("ProjectReference")
            .Where(x => x.Attribute("Include")?.Value.Contains("ApiService.csproj") == true)
            .ToList();

        Assert.NotEmpty(projectReferences);
    }

    [Fact]
    public void All_Projects_Should_Target_Net8()
    {
        var projects = new[]
        {
            Path.Combine(_workspaceRoot, "AppHost", "AppHost.csproj"),
            Path.Combine(_workspaceRoot, "ApiService", "ApiService.csproj"),
            Path.Combine(_workspaceRoot, "WorkerService", "WorkerService.csproj"),
            Path.Combine(_workspaceRoot, "ApiService.Tests", "ApiService.Tests.csproj")
        };

        foreach (var projectPath in projects)
        {
            var doc = XDocument.Load(projectPath);
            var targetFramework = doc.Descendants("TargetFramework")
                .FirstOrDefault()?.Value;

            Assert.Equal("net8.0", targetFramework);
        }
    }
}
