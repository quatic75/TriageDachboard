using ApiService.Data;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApiService.Tests.Data;

public class DbSeederTests
{
    [Fact]
    public async Task Seed_CreatesTestData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var failures = await context.PipelineFailures.ToListAsync();
        Assert.Equal(15, failures.Count);

        // Verify diverse data
        Assert.Contains(failures, f => f.Status == FailureStatus.Open);
        Assert.Contains(failures, f => f.Status == FailureStatus.Resolved);
        Assert.Contains(failures, f => f.Status == FailureStatus.NeedsIntervention);

        Assert.Contains(failures, f => f.JiraCreated);
        Assert.Contains(failures, f => !f.JiraCreated);

        Assert.Contains(failures, f => f.RetryAttempted);
        Assert.Contains(failures, f => !f.RetryAttempted);

        Assert.Contains(failures, f => !string.IsNullOrEmpty(f.ActivityName));
        Assert.Contains(failures, f => string.IsNullOrEmpty(f.ActivityName));

        Assert.Contains(failures, f => !string.IsNullOrEmpty(f.SourceJson));

        // Verify all required fields are populated
        Assert.All(failures, f =>
        {
            Assert.NotEqual(Guid.Empty, f.Id);
            Assert.NotEmpty(f.RunId);
            Assert.NotEmpty(f.PipelineName);
            Assert.NotEmpty(f.ErrorMessage);
            Assert.InRange(f.Confidence, 0, 1);
        });
    }

    [Fact]
    public async Task Seed_OnlyExecutesWhenDatabaseIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        // Add one record
        context.PipelineFailures.Add(new PipelineFailure
        {
            RunId = "existing-run",
            PipelineName = "Existing Pipeline",
            ErrorMessage = "Existing error"
        });
        await context.SaveChangesAsync();

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert - should still only have 1 record
        var failures = await context.PipelineFailures.ToListAsync();
        Assert.Single(failures);
        Assert.Equal("existing-run", failures[0].RunId);
    }

    [Fact]
    public async Task Seed_PopulatesJiraTicketDataForJiraCreatedRecords()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var failures = await context.PipelineFailures.ToListAsync();
        var jiraCreatedFailures = failures.Where(f => f.JiraCreated).ToList();
        
        // Verify all JiraCreated records have ticket ID and URL
        Assert.All(jiraCreatedFailures, f =>
        {
            Assert.NotNull(f.JiraTicketId);
            Assert.NotEmpty(f.JiraTicketId);
            Assert.Matches(@"^TRIAGE-\d{4}$", f.JiraTicketId);
            
            Assert.NotNull(f.JiraTicketUrl);
            Assert.NotEmpty(f.JiraTicketUrl);
            Assert.StartsWith("https://jira.example.com/browse/", f.JiraTicketUrl);
            Assert.Equal($"https://jira.example.com/browse/{f.JiraTicketId}", f.JiraTicketUrl);
        });
        
        // Verify records without JiraCreated don't have ticket data
        var nonJiraFailures = failures.Where(f => !f.JiraCreated).ToList();
        Assert.All(nonJiraFailures, f =>
        {
            Assert.Null(f.JiraTicketId);
            Assert.Null(f.JiraTicketUrl);
        });
    }
}
