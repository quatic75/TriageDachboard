using System.Net;
using System.Net.Http.Json;
using ApiService.Data;
using ApiService.DTOs;
using ApiService.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApiService.Tests.Endpoints;

public class FailureActionsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public FailureActionsTests(WebApplicationFactory<Program> factory)
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                
                // Add a test-specific in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });
    }

    [Fact]
    public async Task CreateJira_SetsJiraCreatedTrue()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure = new PipelineFailure
        {
            RunId = "run-001",
            PipelineName = "Build Pipeline",
            ErrorMessage = "Build failed",
            JiraCreated = false
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await client.PostAsync($"/failures/{failure.Id}/jira", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(updatedFailure);
        Assert.True(updatedFailure.JiraCreated);
        Assert.Equal(failure.Id, updatedFailure.Id);
    }

    [Fact]
    public async Task CreateJira_Returns404_WhenNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/failures/{nonExistentId}/jira", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Retry_FirstAttempt_SetsRetryAttempted()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure = new PipelineFailure
        {
            RunId = "run-003",
            PipelineName = "Deploy Pipeline",
            ErrorMessage = "Deployment failed",
            RetryAttempted = false,
            Status = FailureStatus.Open
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await client.PostAsync($"/failures/{failure.Id}/retry", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(updatedFailure);
        Assert.True(updatedFailure.RetryAttempted);
        Assert.Equal("Open", updatedFailure.Status);
        Assert.Equal(failure.Id, updatedFailure.Id);
    }

    [Fact]
    public async Task Retry_SecondAttempt_SetsNeedsIntervention()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure = new PipelineFailure
        {
            RunId = "run-004",
            PipelineName = "Test Pipeline",
            ErrorMessage = "Test failed",
            RetryAttempted = true, // Already retried once
            Status = FailureStatus.Open
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await client.PostAsync($"/failures/{failure.Id}/retry", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(updatedFailure);
        Assert.True(updatedFailure.RetryAttempted);
        Assert.Equal("NeedsIntervention", updatedFailure.Status);
        Assert.Equal(failure.Id, updatedFailure.Id);
    }

    [Fact]
    public async Task Retry_Returns404_WhenNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/failures/{nonExistentId}/retry", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
