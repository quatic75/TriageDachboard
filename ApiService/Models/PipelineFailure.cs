using System.ComponentModel.DataAnnotations;

namespace ApiService.Models;

public class PipelineFailure
{
    public Guid Id { get; set; }

    [Required]
    public string RunId { get; set; } = string.Empty;

    [Required]
    public string PipelineName { get; set; } = string.Empty;

    public string? ActivityName { get; set; }

    [Required]
    public string ErrorMessage { get; set; } = string.Empty;

    public string? Classification { get; set; }

    public double Confidence { get; set; }

    public string? Summary { get; set; }

    public string? RootCause { get; set; }

    public string? SuggestedFix { get; set; }

    public bool AutoHandled { get; set; }

    public bool JiraCreated { get; set; }

    public bool RetryAttempted { get; set; }

    public FailureStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PipelineFailure()
    {
        Id = Guid.NewGuid();
        AutoHandled = false;
        JiraCreated = false;
        RetryAttempted = false;
        Status = FailureStatus.Open;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
