using ApiService.DTOs;

namespace ApiService.Tests.DTOs;

public class FailureDtoTests
{
    [Fact]
    public void FailureDto_ShouldHaveJiraTicketFields()
    {
        // Arrange & Act
        var dto = new FailureDto
        {
            Id = Guid.NewGuid(),
            RunId = "run-123",
            PipelineName = "Test Pipeline",
            ErrorMessage = "Test error",
            JiraTicketId = "TRIAGE-1234",
            JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-1234"
        };

        // Assert
        Assert.Equal("TRIAGE-1234", dto.JiraTicketId);
        Assert.Equal("https://jira.example.com/browse/TRIAGE-1234", dto.JiraTicketUrl);
    }

    [Fact]
    public void FailureDto_JiraTicketFields_ShouldBeNullable()
    {
        // Arrange & Act
        var dto = new FailureDto
        {
            Id = Guid.NewGuid(),
            RunId = "run-123",
            PipelineName = "Test Pipeline",
            ErrorMessage = "Test error",
            JiraTicketId = null,
            JiraTicketUrl = null
        };

        // Assert
        Assert.Null(dto.JiraTicketId);
        Assert.Null(dto.JiraTicketUrl);
    }

    [Fact]
    public void CreateFailureDto_ShouldHaveJiraTicketFields()
    {
        // Arrange & Act
        var dto = new CreateFailureDto
        {
            RunId = "run-123",
            PipelineName = "Test Pipeline",
            ErrorMessage = "Test error",
            JiraTicketId = "TRIAGE-5678",
            JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-5678"
        };

        // Assert
        Assert.Equal("TRIAGE-5678", dto.JiraTicketId);
        Assert.Equal("https://jira.example.com/browse/TRIAGE-5678", dto.JiraTicketUrl);
    }

    [Fact]
    public void UpdateFailureDto_ShouldHaveJiraTicketFields()
    {
        // Arrange & Act
        var dto = new UpdateFailureDto
        {
            JiraTicketId = "TRIAGE-9999",
            JiraTicketUrl = "https://jira.example.com/browse/TRIAGE-9999"
        };

        // Assert
        Assert.Equal("TRIAGE-9999", dto.JiraTicketId);
        Assert.Equal("https://jira.example.com/browse/TRIAGE-9999", dto.JiraTicketUrl);
    }
}
