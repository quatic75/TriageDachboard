namespace ApiService.DTOs;

public class FailureDto
{
    public Guid Id { get; set; }
    public string RunId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? ActivityName { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? Classification { get; set; }
    public double Confidence { get; set; }
    public string? Summary { get; set; }
    public string? RootCause { get; set; }
    public string? SuggestedFix { get; set; }
    public string? SourceJson { get; set; }
    public bool AutoHandled { get; set; }
    public bool JiraCreated { get; set; }
    public bool RetryAttempted { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
