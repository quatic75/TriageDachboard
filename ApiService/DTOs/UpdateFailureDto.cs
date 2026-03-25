namespace ApiService.DTOs;

public class UpdateFailureDto
{
    public string? Classification { get; set; }

    public double? Confidence { get; set; }

    public string? Summary { get; set; }

    public string? RootCause { get; set; }

    public string? SuggestedFix { get; set; }

    public string? SourceJson { get; set; }

    public string? Status { get; set; }

    public string? JiraTicketId { get; set; }

    public string? JiraTicketUrl { get; set; }
}
