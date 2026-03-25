using ApiService.Data;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Tests.Data;

public class DbContextTests
{
    [Fact]
    public void ConfigureEntity_ShouldHavePipelineFailureConfigured()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ConfigureEntity")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PipelineFailure));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal(nameof(PipelineFailure), entityType.ClrType.Name);
    }

    [Fact]
    public void CanCreateInMemoryContext_ShouldInstantiateSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateContext")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.PipelineFailures);
    }

    [Fact]
    public async Task CanAddAndRetrievePipelineFailure_ShouldWorkCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddRetrieve")
            .Options;

        var failure = new PipelineFailure
        {
            RunId = "run-456",
            PipelineName = "Test Pipeline",
            ErrorMessage = "Test error"
        };

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            context.PipelineFailures.Add(failure);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new ApplicationDbContext(options))
        {
            var retrieved = await context.PipelineFailures.FirstOrDefaultAsync(f => f.RunId == "run-456");
            Assert.NotNull(retrieved);
            Assert.Equal("Test Pipeline", retrieved.PipelineName);
            Assert.Equal("Test error", retrieved.ErrorMessage);
        }
    }
}
