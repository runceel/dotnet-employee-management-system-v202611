using System.Xml.Linq;

namespace EmployeeManagement.Common.Tests;

public sealed class ProjectDependencyTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedProjectReferences =
        new Dictionary<string, string[]>
        {
            ["EmployeeManagement.AppHost/EmployeeManagement.AppHost.csproj"] =
            [
                "EmployeeManagement.Api",
                "EmployeeManagement.Web"
            ],
            ["EmployeeManagement.ServiceDefaults/EmployeeManagement.ServiceDefaults.csproj"] = [],
            ["src/EmployeeManagement.Api/EmployeeManagement.Api.csproj"] =
            [
                "EmployeeManagement.Common.Application",
                "EmployeeManagement.Common.Infrastructure",
                "EmployeeManagement.ServiceDefaults"
            ],
            ["src/EmployeeManagement.Web/EmployeeManagement.Web.csproj"] =
            [
                "EmployeeManagement.ServiceDefaults"
            ],
            ["src/Modules/Common/EmployeeManagement.Common.Domain/EmployeeManagement.Common.Domain.csproj"] = [],
            ["src/Modules/Common/EmployeeManagement.Common.Application/EmployeeManagement.Common.Application.csproj"] =
            [
                "EmployeeManagement.Common.Domain"
            ],
            ["src/Modules/Common/EmployeeManagement.Common.Infrastructure/EmployeeManagement.Common.Infrastructure.csproj"] =
            [
                "EmployeeManagement.Common.Application",
                "EmployeeManagement.Common.Domain"
            ],
            ["tests/EmployeeManagement.Common.Tests/EmployeeManagement.Common.Tests.csproj"] = [],
            ["tests/EmployeeManagement.IntegrationTests/EmployeeManagement.IntegrationTests.csproj"] =
            [
                "EmployeeManagement.Api"
            ]
        };

    private static readonly IReadOnlyDictionary<string, string[]> AllowedPackageReferences =
        new Dictionary<string, string[]>
        {
            ["EmployeeManagement.AppHost/EmployeeManagement.AppHost.csproj"] =
            [
                "Aspire.Hosting.PostgreSQL"
            ],
            ["EmployeeManagement.ServiceDefaults/EmployeeManagement.ServiceDefaults.csproj"] =
            [
                "Microsoft.Extensions.Http.Resilience",
                "Microsoft.Extensions.ServiceDiscovery",
                "OpenTelemetry.Exporter.OpenTelemetryProtocol",
                "OpenTelemetry.Extensions.Hosting",
                "OpenTelemetry.Instrumentation.AspNetCore",
                "OpenTelemetry.Instrumentation.Http",
                "OpenTelemetry.Instrumentation.Runtime"
            ],
            ["src/EmployeeManagement.Api/EmployeeManagement.Api.csproj"] =
            [
                "Microsoft.AspNetCore.OpenApi"
            ],
            ["src/EmployeeManagement.Web/EmployeeManagement.Web.csproj"] = [],
            ["src/Modules/Common/EmployeeManagement.Common.Domain/EmployeeManagement.Common.Domain.csproj"] = [],
            ["src/Modules/Common/EmployeeManagement.Common.Application/EmployeeManagement.Common.Application.csproj"] = [],
            ["src/Modules/Common/EmployeeManagement.Common.Infrastructure/EmployeeManagement.Common.Infrastructure.csproj"] = [],
            ["tests/EmployeeManagement.Common.Tests/EmployeeManagement.Common.Tests.csproj"] =
            [
                "Microsoft.NET.Test.Sdk",
                "xunit",
                "xunit.runner.visualstudio"
            ],
            ["tests/EmployeeManagement.IntegrationTests/EmployeeManagement.IntegrationTests.csproj"] =
            [
                "Microsoft.AspNetCore.Mvc.Testing",
                "Microsoft.NET.Test.Sdk",
                "xunit",
                "xunit.runner.visualstudio"
            ]
        };

    [Fact]
    public void ProjectReferencesMatchTheArchitectureAllowlist()
    {
        var root = FindRepositoryRoot();

        foreach (var (project, allowedReferences) in AllowedProjectReferences)
        {
            var projectFile = XDocument.Load(Path.Combine(root, project));
            var actualReferences = projectFile
                .Descendants("ProjectReference")
                .Select(reference => Path.GetFileNameWithoutExtension((string?)reference.Attribute("Include")))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                allowedReferences.Order(StringComparer.Ordinal),
                actualReferences);
        }
    }

    [Fact]
    public void PackageReferencesMatchTheLayerAllowlist()
    {
        var root = FindRepositoryRoot();

        foreach (var (project, allowedPackages) in AllowedPackageReferences)
        {
            var projectFile = XDocument.Load(Path.Combine(root, project));
            var actualPackages = projectFile
                .Descendants("PackageReference")
                .Select(reference => (string?)reference.Attribute("Include"))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(allowedPackages.Order(StringComparer.Ordinal), actualPackages);
        }
    }

    [Fact]
    public void AspireStartupProjectsDoNotReferenceCommonDomainLayers()
    {
        var root = FindRepositoryRoot();
        var appHost = XDocument.Load(Path.Combine(root, "EmployeeManagement.AppHost/EmployeeManagement.AppHost.csproj"));
        var serviceDefaults = XDocument.Load(Path.Combine(root, "EmployeeManagement.ServiceDefaults/EmployeeManagement.ServiceDefaults.csproj"));
        var forbiddenReferences = new[]
        {
            "EmployeeManagement.Common.Domain",
            "EmployeeManagement.Common.Application",
            "EmployeeManagement.Common.Infrastructure"
        };
        var startupReferences = appHost
            .Descendants("ProjectReference")
            .Concat(serviceDefaults.Descendants("ProjectReference"))
            .Select(reference => Path.GetFileNameWithoutExtension((string?)reference.Attribute("Include")));

        Assert.DoesNotContain(startupReferences, forbiddenReferences.Contains);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EmployeeManagement.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate EmployeeManagement.slnx.");
    }
}
