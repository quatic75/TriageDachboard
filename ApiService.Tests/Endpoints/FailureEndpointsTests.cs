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

public class FailureEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public FailureEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all DbContext-related registrations (including Aspire's pooling)
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType.IsGenericType && 
                                d.ServiceType.GetGenericArguments().Any(t => t == typeof(ApplicationDbContext)))
                    .ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }
                
                // Also remove non-generic ApplicationDbContext
                services.RemoveAll<ApplicationDbContext>();
                
                // Add a test-specific in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });
    }

    [Fact]
    public async Task GetFailures_ReturnsAllFailures()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure1 = new PipelineFailure
        {
            RunId = "run-001",
            PipelineName = "Build Pipeline",
            ErrorMessage = "Build failed"
        };
        var failure2 = new PipelineFailure
        {
            RunId = "run-002",
            PipelineName = "Deploy Pipeline",
            ErrorMessage = "Deployment failed"
        };
        
        dbContext.PipelineFailures.AddRange(failure1, failure2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/failures");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var failures = await response.Content.ReadFromJsonAsync<List<FailureDto>>();
        Assert.NotNull(failures);
        Assert.Equal(2, failures.Count);
    }

    [Fact]
    public async Task GetFailureById_ReturnsFailure_WhenExists()
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
            ErrorMessage = "Build failed"
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/failures/{failure.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(returnedFailure);
        Assert.Equal(failure.Id, returnedFailure.Id);
        Assert.Equal(failure.RunId, returnedFailure.RunId);
    }

    [Fact]
    public async Task GetFailureById_Returns404_WhenNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/failures/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFailure_CreatesFailure_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateFailureDto
        {
            RunId = "run-003",
            PipelineName = "Test Pipeline",
            ActivityName = "Test Activity",
            ErrorMessage = "Test error message",
            Classification = "NetworkError",
            Confidence = 0.95,
            Summary = "Network timeout",
            RootCause = "DNS resolution failed",
            SuggestedFix = "Check DNS configuration"
        };

        // Act
        var response = await client.PostAsJsonAsync("/failures", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(createdFailure);
        Assert.NotEqual(Guid.Empty, createdFailure.Id);
        Assert.Equal(createDto.RunId, createdFailure.RunId);
        Assert.Equal(createDto.PipelineName, createdFailure.PipelineName);
        Assert.Equal(createDto.ErrorMessage, createdFailure.ErrorMessage);
        Assert.Equal("Open", createdFailure.Status);
        
        // Verify it was actually saved to the database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedFailure = await dbContext.PipelineFailures.FindAsync(createdFailure.Id);
        Assert.NotNull(savedFailure);
    }

    [Fact]
    public async Task PatchFailure_UpdatesFields_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure = new PipelineFailure
        {
            RunId = "run-004",
            PipelineName = "Update Test Pipeline",
            ErrorMessage = "Original error"
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateFailureDto
        {
            Classification = "ConfigurationError",
            Confidence = 0.88,
            Summary = "Updated summary",
            RootCause = "Missing config file",
            SuggestedFix = "Add config file",
            Status = "Resolved"
        };

        // Act
        var response = await client.PatchAsync($"/failures/{failure.Id}", JsonContent.Create(updateDto));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(updatedFailure);
        Assert.Equal(updateDto.Classification, updatedFailure.Classification);
        Assert.Equal(updateDto.Confidence, updatedFailure.Confidence);
        Assert.Equal(updateDto.Summary, updatedFailure.Summary);
        Assert.Equal(updateDto.Status, updatedFailure.Status);
        
        // Verify UpdatedAt was changed
        Assert.True(updatedFailure.UpdatedAt > failure.CreatedAt);
    }

    [Fact]
    public async Task PatchFailure_Returns404_WhenNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateFailureDto
        {
            Classification = "ConfigurationError"
        };

        // Act
        var response = await client.PatchAsync($"/failures/{nonExistentId}", JsonContent.Create(updateDto));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFailure_WithSourceJson_StoresAndReturnsSourceJson()
    {
        // Arrange
        var client = _factory.CreateClient();
        var sourceJson = "{\"logs\":[\"Error: Connection timeout\",\"Retry failed\"],\"timestamp\":\"2026-03-25T10:30:00Z\"}";
        var createDto = new CreateFailureDto
        {
            RunId = "run-005",
            PipelineName = "AI Analysis Pipeline",
            ErrorMessage = "AI processing failed",
            Classification = "TimeoutError",
            Confidence = 0.92,
            Summary = "Connection timeout during processing",
            RootCause = "Network latency",
            SuggestedFix = "Increase timeout threshold",
            SourceJson = sourceJson
        };

        // Act
        var response = await client.PostAsJsonAsync("/failures", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(createdFailure);
        Assert.Equal(sourceJson, createdFailure.SourceJson);
        
        // Verify it's stored in the database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedFailure = await dbContext.PipelineFailures.FindAsync(createdFailure.Id);
        Assert.NotNull(savedFailure);
        Assert.Equal(sourceJson, savedFailure.SourceJson);
    }

    [Fact]
    public async Task PatchFailure_UpdatesSourceJson()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var failure = new PipelineFailure
        {
            RunId = "run-006",
            PipelineName = "Update SourceJson Test",
            ErrorMessage = "Original error",
            SourceJson = "{\"original\":\"data\"}"
        };
        
        dbContext.PipelineFailures.Add(failure);
        await dbContext.SaveChangesAsync();

        var newSourceJson = "{\"updated\":\"data\",\"version\":2}";
        var updateDto = new UpdateFailureDto
        {
            SourceJson = newSourceJson
        };

        // Act
        var response = await client.PatchAsync($"/failures/{failure.Id}", JsonContent.Create(updateDto));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedFailure = await response.Content.ReadFromJsonAsync<FailureDto>();
        Assert.NotNull(updatedFailure);
        Assert.Equal(newSourceJson, updatedFailure.SourceJson);
    }
}
