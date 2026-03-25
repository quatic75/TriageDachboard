using System.ComponentModel.DataAnnotations;

namespace ApiService.DTOs;

public class CreateFailureDto
{
    [Required]
    public string RunId { get; set; } = string.Empty;

    [Required]
    public string PipelineName { get; set; } = string.Empty;

    public string? ActivityName { get; set; }

    [Required]
    public string ErrorMessage { get; set; } = string.Empty;

    public string? Classification { get; set; }

    public double? Confidence { get; set; }

    public string? Summary { get; set; }

    public string? RootCause { get; set; }

    public string? SuggestedFix { get; set; }

    public string? SourceJson { get; set; }

    public string? JiraTicketId { get; set; }

    public string? JiraTicketUrl { get; set; }
}
