using ApiService.Data;
using ApiService.DTOs;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Endpoints;

public static class FailureActionsEndpoints
{
    public static void MapFailureActionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/failures");

        // POST /failures/{id}/jira - Create Jira ticket for a failure
        group.MapPost("/{id:guid}/jira", async (Guid id, ApplicationDbContext db) =>
        {
            var failure = await db.PipelineFailures.FindAsync(id);
            if (failure is null)
            {
                return Results.NotFound();
            }

            failure.JiraCreated = true;
            failure.JiraTicketId = $"TRIAGE-{Random.Shared.Next(1000, 9999)}";
            failure.JiraTicketUrl = $"https://jira.example.com/browse/{failure.JiraTicketId}";
            failure.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(MapToDto(failure));
        })
        .WithName("CreateJiraTicket")
        .WithOpenApi();

        // POST /failures/{id}/retry - Retry a failed pipeline
        group.MapPost("/{id:guid}/retry", async (Guid id, ApplicationDbContext db) =>
        {
            var failure = await db.PipelineFailures.FindAsync(id);
            if (failure is null)
            {
                return Results.NotFound();
            }

            if (!failure.RetryAttempted)
            {
                // First retry attempt
                failure.RetryAttempted = true;
                failure.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.Ok(MapToDto(failure));
            }
            else
            {
                // Second retry attempt - needs manual intervention
                failure.Status = FailureStatus.NeedsIntervention;
                failure.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.Ok(MapToDto(failure));
            }
        })
        .WithName("RetryFailure")
        .WithOpenApi();
    }

    private static FailureDto MapToDto(PipelineFailure failure)
    {
        return new FailureDto
        {
            Id = failure.Id,
            RunId = failure.RunId,
            PipelineName = failure.PipelineName,
            ActivityName = failure.ActivityName,
            ErrorMessage = failure.ErrorMessage,
            Classification = failure.Classification,
            Confidence = failure.Confidence,
            Summary = failure.Summary,
            RootCause = failure.RootCause,
            SuggestedFix = failure.SuggestedFix,
            SourceJson = failure.SourceJson,
            AutoHandled = failure.AutoHandled,
            JiraCreated = failure.JiraCreated,
            JiraTicketId = failure.JiraTicketId,
            JiraTicketUrl = failure.JiraTicketUrl,
            RetryAttempted = failure.RetryAttempted,
            Status = failure.Status.ToString(),
            CreatedAt = failure.CreatedAt,
            UpdatedAt = failure.UpdatedAt
        };
    }
}
