using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Only seed if database is empty
        if (await context.PipelineFailures.AnyAsync())
        {
            return;
        }

        var baseDate = DateTime.UtcNow;
        var failures = new List<PipelineFailure>
        {
            new()
            {
                RunId = "run-001",
                PipelineName = "Build Pipeline",
                ActivityName = "CompileCode",
                ErrorMessage = "CS0246: The type or namespace name 'InvalidClass' could not be found",
                Classification = "CompilationError",
                Confidence = 0.95,
                Summary = "Compilation failed due to missing type reference",
                RootCause = "Missing using statement or invalid type reference",
                SuggestedFix = "Add using statement or verify class name",
                SourceJson = "{\"compiler\":\"csc\",\"exitCode\":1,\"line\":42}",
                Status = FailureStatus.Open,
                JiraCreated = false,
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-10),
                UpdatedAt = baseDate.AddHours(-10)
            },
            new()
            {
                RunId = "run-002",
                PipelineName = "Deploy Pipeline",
                ActivityName = "DeployToProduction",
                ErrorMessage = "Connection timeout while connecting to database server",
                Classification = "NetworkError",
                Confidence = 0.88,
                Summary = "Deployment failed due to network connectivity issues",
                RootCause = "Database server unreachable or firewall blocking connection",
                SuggestedFix = "Verify network connectivity and firewall rules",
                SourceJson = "{\"timeout\":30000,\"host\":\"db.example.com\"}",
                Status = FailureStatus.Resolved,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1001",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1001",
                RetryAttempted = true,
                CreatedAt = baseDate.AddHours(-48),
                UpdatedAt = baseDate.AddHours(-2)
            },
            new()
            {
                RunId = "run-003",
                PipelineName = "Test Pipeline",
                ErrorMessage = "NullReferenceException: Object reference not set to an instance of an object",
                Classification = "RuntimeError",
                Confidence = 0.75,
                Summary = "Test execution failed with null reference exception",
                RootCause = "Uninitialized object in test setup",
                SuggestedFix = "Initialize all required objects before test execution",
                Status = FailureStatus.NeedsIntervention,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1002",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1002",
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-5),
                UpdatedAt = baseDate.AddHours(-1)
            },
            new()
            {
                RunId = "run-004",
                PipelineName = "Build Pipeline",
                ActivityName = "RestorePackages",
                ErrorMessage = "Unable to find package 'Newtonsoft.Json' version 13.0.3",
                Classification = "DependencyError",
                Confidence = 0.92,
                Summary = "Package restore failed for specified version",
                RootCause = "Package version not available in configured NuGet feeds",
                SuggestedFix = "Update package version or add additional package source",
                SourceJson = "{\"package\":\"Newtonsoft.Json\",\"version\":\"13.0.3\",\"source\":\"nuget.org\"}",
                Status = FailureStatus.Open,
                JiraCreated = false,
                RetryAttempted = true,
                CreatedAt = baseDate.AddHours(-3),
                UpdatedAt = baseDate.AddHours(-3)
            },
            new()
            {
                RunId = "run-005",
                PipelineName = "Integration Test Pipeline",
                ActivityName = "RunIntegrationTests",
                ErrorMessage = "Test 'UserAuthenticationTests.Login_WithValidCredentials_Succeeds' failed with timeout",
                Classification = "TestTimeout",
                Confidence = 0.68,
                Summary = "Integration test exceeded maximum execution time",
                RootCause = "External service dependency not responding",
                SuggestedFix = "Check external service availability or increase timeout",
                Status = FailureStatus.Resolved,
                JiraCreated = false,
                RetryAttempted = true,
                CreatedAt = baseDate.AddDays(-2),
                UpdatedAt = baseDate.AddDays(-1)
            },
            new()
            {
                RunId = "run-006",
                PipelineName = "Deploy Pipeline",
                ErrorMessage = "Insufficient permissions to access resource '/subscriptions/xxx/resourceGroups/prod'",
                Classification = "PermissionError",
                Confidence = 0.98,
                Summary = "Deployment failed due to insufficient Azure permissions",
                RootCause = "Service principal lacks required RBAC role",
                SuggestedFix = "Grant Contributor role to service principal on resource group",
                SourceJson = "{\"principal\":\"sp-deploy-prod\",\"resource\":\"rg-prod\",\"requiredRole\":\"Contributor\"}",
                Status = FailureStatus.NeedsIntervention,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1003",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1003",
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-24),
                UpdatedAt = baseDate.AddHours(-20)
            },
            new()
            {
                RunId = "run-007",
                PipelineName = "Build Pipeline",
                ActivityName = "RunUnitTests",
                ErrorMessage = "Assert.Equal() Failure: Expected: 42, Actual: 43",
                Classification = "TestFailure",
                Confidence = 0.85,
                Summary = "Unit test assertion failed with mismatched values",
                RootCause = "Logic error in calculation method",
                SuggestedFix = "Review and correct calculation logic",
                Status = FailureStatus.Open,
                JiraCreated = false,
                RetryAttempted = false,
                CreatedAt = baseDate.AddMinutes(-30),
                UpdatedAt = baseDate.AddMinutes(-30)
            },
            new()
            {
                RunId = "run-008",
                PipelineName = "Security Scan Pipeline",
                ErrorMessage = "High severity vulnerability detected: CVE-2023-12345 in package 'log4net'",
                Classification = "SecurityVulnerability",
                Confidence = 0.99,
                Summary = "Critical security vulnerability found in dependencies",
                RootCause = "Outdated package version with known vulnerability",
                SuggestedFix = "Update log4net to version 2.0.15 or later",
                SourceJson = "{\"cve\":\"CVE-2023-12345\",\"severity\":\"High\",\"package\":\"log4net\",\"currentVersion\":\"2.0.10\",\"fixedVersion\":\"2.0.15\"}",
                Status = FailureStatus.NeedsIntervention,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1004",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1004",
                RetryAttempted = false,
                CreatedAt = baseDate.AddDays(-1),
                UpdatedAt = baseDate.AddHours(-6)
            },
            new()
            {
                RunId = "run-009",
                PipelineName = "Docker Build Pipeline",
                ActivityName = "BuildDockerImage",
                ErrorMessage = "failed to solve with frontend dockerfile.v0: failed to create LLB definition: unexpected EOF",
                Classification = "DockerBuildError",
                Confidence = 0.72,
                Summary = "Docker image build failed with syntax error",
                RootCause = "Invalid or incomplete Dockerfile syntax",
                SuggestedFix = "Validate Dockerfile syntax and ensure all commands are complete",
                Status = FailureStatus.Resolved,
                JiraCreated = false,
                RetryAttempted = true,
                CreatedAt = baseDate.AddHours(-15),
                UpdatedAt = baseDate.AddHours(-14)
            },
            new()
            {
                RunId = "run-010",
                PipelineName = "Performance Test Pipeline",
                ErrorMessage = "Response time 5432ms exceeded threshold of 2000ms for endpoint /api/users",
                Classification = "PerformanceIssue",
                Confidence = 0.90,
                Summary = "API endpoint response time exceeds acceptable threshold",
                RootCause = "Database query not optimized, missing indexes",
                SuggestedFix = "Add database index on frequently queried columns",
                SourceJson = "{\"endpoint\":\"/api/users\",\"responseTime\":5432,\"threshold\":2000,\"method\":\"GET\"}",
                Status = FailureStatus.Open,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1005",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1005",
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-8),
                UpdatedAt = baseDate.AddHours(-8)
            },
            new()
            {
                RunId = "run-011",
                PipelineName = "Deploy Pipeline",
                ActivityName = "ApplyDatabaseMigrations",
                ErrorMessage = "Column 'NewColumn' already exists in table 'Users'",
                Classification = "MigrationError",
                Confidence = 0.87,
                Summary = "Database migration failed due to duplicate column",
                RootCause = "Migration applied out of sequence or duplicate migration",
                SuggestedFix = "Review migration history and remove duplicate migration",
                Status = FailureStatus.Resolved,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1006",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1006",
                RetryAttempted = true,
                CreatedAt = baseDate.AddDays(-3),
                UpdatedAt = baseDate.AddDays(-2)
            },
            new()
            {
                RunId = "run-012",
                PipelineName = "Build Pipeline",
                ErrorMessage = "Disk quota exceeded: Unable to write to /tmp/build-cache",
                Classification = "ResourceError",
                Confidence = 0.94,
                Summary = "Build failed due to insufficient disk space",
                RootCause = "Build agent disk space exhausted",
                SuggestedFix = "Clean up build artifacts or increase agent disk quota",
                Status = FailureStatus.Open,
                JiraCreated = false,
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-4),
                UpdatedAt = baseDate.AddHours(-4)
            },
            new()
            {
                RunId = "run-013",
                PipelineName = "Code Quality Pipeline",
                ActivityName = "SonarQubeAnalysis",
                ErrorMessage = "Quality Gate failed: 15 code smells, 3 bugs, 2 vulnerabilities detected",
                Classification = "CodeQualityIssue",
                Confidence = 0.80,
                Summary = "Code quality analysis failed quality gate criteria",
                RootCause = "Multiple code quality issues identified by static analysis",
                SuggestedFix = "Address reported code smells, bugs, and vulnerabilities",
                SourceJson = "{\"smells\":15,\"bugs\":3,\"vulnerabilities\":2,\"coverage\":72.5}",
                Status = FailureStatus.Open,
                JiraCreated = false,
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-12),
                UpdatedAt = baseDate.AddHours(-12)
            },
            new()
            {
                RunId = "run-014",
                PipelineName = "Terraform Pipeline",
                ActivityName = "TerraformApply",
                ErrorMessage = "Error: Error creating Virtual Machine: compute.VirtualMachinesClient#CreateOrUpdate: Failure responding to request: StatusCode=409 -- Original Error: Code=\"OperationNotAllowed\" Message=\"Operation could not be completed as it results in exceeding quota\"",
                Classification = "QuotaExceeded",
                Confidence = 0.96,
                Summary = "Infrastructure provisioning failed due to quota limitations",
                RootCause = "Azure subscription resource quota exceeded",
                SuggestedFix = "Request quota increase or delete unused resources",
                SourceJson = "{\"resource\":\"VirtualMachine\",\"quota\":\"vCPUs\",\"limit\":20,\"requested\":24}",
                Status = FailureStatus.NeedsIntervention,
                JiraCreated = true,
                JiraTicketId = "TRIAGE-1007",
                JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1007",
                RetryAttempted = false,
                CreatedAt = baseDate.AddHours(-6),
                UpdatedAt = baseDate.AddHours(-5)
            },
            new()
            {
                RunId = "run-015",
                PipelineName = "API Test Pipeline",
                ErrorMessage = "HTTP 503 Service Unavailable returned from https://api.external-service.com/v1/data",
                Classification = "ExternalServiceError",
                Confidence = 0.78,
                Summary = "External API dependency unavailable during test execution",
                RootCause = "Third-party service experiencing outage",
                SuggestedFix = "Retry when service is restored or implement circuit breaker pattern",
                Status = FailureStatus.Resolved,
                JiraCreated = false,
                RetryAttempted = true,
                CreatedAt = baseDate.AddHours(-18),
                UpdatedAt = baseDate.AddHours(-16)
            }
        };

        context.PipelineFailures.AddRange(failures);
        await context.SaveChangesAsync();
    }
}
