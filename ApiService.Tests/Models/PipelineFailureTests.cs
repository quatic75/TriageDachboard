using ApiService.Models;
using System.ComponentModel.DataAnnotations;

namespace ApiService.Tests.Models;

public class PipelineFailureTests
{
    [Fact]
    public void ValidateRequiredFields_ShouldHaveRequiredAttributes()
    {
        // Arrange & Act - Get property info and check for Required attribute
        var runIdProperty = typeof(PipelineFailure).GetProperty(nameof(PipelineFailure.RunId));
        var pipelineNameProperty = typeof(PipelineFailure).GetProperty(nameof(PipelineFailure.PipelineName));
        var errorMessageProperty = typeof(PipelineFailure).GetProperty(nameof(PipelineFailure.ErrorMessage));
        
        // Assert - Verify properties exist and have [Required] attribute
        Assert.NotNull(runIdProperty);
        Assert.NotNull(pipelineNameProperty);
        Assert.NotNull(errorMessageProperty);
        
        Assert.True(runIdProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(pipelineNameProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(errorMessageProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var failure = new PipelineFailure();

        // Assert
        Assert.NotEqual(Guid.Empty, failure.Id);
        Assert.False(failure.AutoHandled);
        Assert.False(failure.JiraCreated);
        Assert.False(failure.RetryAttempted);
        Assert.Equal(FailureStatus.Open, failure.Status);
        Assert.NotEqual(default(DateTime), failure.CreatedAt);
        Assert.NotEqual(default(DateTime), failure.UpdatedAt);
    }

    [Fact]
    public void PipelineFailure_ShouldHaveAllRequiredProperties()
    {
        // Arrange
        var failure = new PipelineFailure
        {
            RunId = "run-123",
            PipelineName = "CI Pipeline",
            ActivityName = "Build",
            ErrorMessage = "Build failed",
            Classification = "BuildError",
            Confidence = 0.95,
            Summary = "Build step failed",
            RootCause = "Missing dependency",
            SuggestedFix = "Install package X"
        };

        // Act & Assert
        Assert.Equal("run-123", failure.RunId);
        Assert.Equal("CI Pipeline", failure.PipelineName);
        Assert.Equal("Build", failure.ActivityName);
        Assert.Equal("Build failed", failure.ErrorMessage);
        Assert.Equal("BuildError", failure.Classification);
        Assert.Equal(0.95, failure.Confidence);
        Assert.Equal("Build step failed", failure.Summary);
        Assert.Equal("Missing dependency", failure.RootCause);
        Assert.Equal("Install package X", failure.SuggestedFix);
    }

    [Fact]
    public void FailureStatus_ShouldHaveAllEnumValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(FailureStatus), FailureStatus.Open));
        Assert.True(Enum.IsDefined(typeof(FailureStatus), FailureStatus.Resolved));
        Assert.True(Enum.IsDefined(typeof(FailureStatus), FailureStatus.NeedsIntervention));
    }

    [Fact]
    public void JiraTicketFields_ShouldExistAndBeNullable()
    {
        // Arrange & Act - Get property info
        var jiraTicketIdProperty = typeof(PipelineFailure).GetProperty(nameof(PipelineFailure.JiraTicketId));
        var jiraTicketUrlProperty = typeof(PipelineFailure).GetProperty(nameof(PipelineFailure.JiraTicketUrl));
        
        // Assert - Verify properties exist
        Assert.NotNull(jiraTicketIdProperty);
        Assert.NotNull(jiraTicketUrlProperty);
        
        // Assert - Verify properties are nullable strings
        Assert.Equal(typeof(string), Nullable.GetUnderlyingType(jiraTicketIdProperty.PropertyType) ?? jiraTicketIdProperty.PropertyType);
        Assert.Equal(typeof(string), Nullable.GetUnderlyingType(jiraTicketUrlProperty.PropertyType) ?? jiraTicketUrlProperty.PropertyType);
        
        // Assert - Verify properties can be set
        var failure = new PipelineFailure
        {
            RunId = "test-run",
            PipelineName = "test-pipeline",
            ErrorMessage = "test error",
            JiraTicketId = "TRIAGE-1234",
            JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1234"
        };
        
        Assert.Equal("TRIAGE-1234", failure.JiraTicketId);
        Assert.Equal("https://jira.example.com/browse/TRIAGE-1234", failure.JiraTicketUrl);
    }

    [Fact]
    public void JiraTicketFields_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var failure = new PipelineFailure();

        // Assert
        Assert.Null(failure.JiraTicketId);
        Assert.Null(failure.JiraTicketUrl);
    }
}
