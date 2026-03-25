using ApiService.Data;
using ApiService.DTOs;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Endpoints;

public static class FailureEndpoints
{
    public static void MapFailureEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/failures");

        // GET /failures - Get all failures
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var failures = await db.PipelineFailures.ToListAsync();
            return Results.Ok(failures.Select(MapToDto));
        })
        .WithName("GetFailures")
        .WithOpenApi();

        // GET /failures/{id} - Get specific failure by ID
        group.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
        {
            var failure = await db.PipelineFailures.FindAsync(id);
            return failure is null ? Results.NotFound() : Results.Ok(MapToDto(failure));
        })
        .WithName("GetFailureById")
        .WithOpenApi();

        // POST /failures - Create new failure
        group.MapPost("/", async (CreateFailureDto createDto, ApplicationDbContext db) =>
        {
            var failure = new PipelineFailure
            {
                RunId = createDto.RunId,
                PipelineName = createDto.PipelineName,
                ActivityName = createDto.ActivityName,
                ErrorMessage = createDto.ErrorMessage,
                Classification = createDto.Classification,
                Confidence = createDto.Confidence ?? 0.0,
                Summary = createDto.Summary,
                RootCause = createDto.RootCause,
                SuggestedFix = createDto.SuggestedFix,
                SourceJson = createDto.SourceJson
            };

            db.PipelineFailures.Add(failure);
            await db.SaveChangesAsync();

            return Results.Created($"/failures/{failure.Id}", MapToDto(failure));
        })
        .WithName("CreateFailure")
        .WithOpenApi();

        // PATCH /failures/{id} - Update existing failure (partial update)
        group.MapPatch("/{id:guid}", async (Guid id, UpdateFailureDto updateDto, ApplicationDbContext db) =>
        {
            var failure = await db.PipelineFailures.FindAsync(id);
            if (failure is null)
            {
                return Results.NotFound();
            }

            ApplyPartialUpdate(failure, updateDto);
            failure.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(MapToDto(failure));
        })
        .WithName("UpdateFailure")
        .WithOpenApi();
    }

    private static void ApplyPartialUpdate(PipelineFailure failure, UpdateFailureDto updateDto)
    {
        if (updateDto.Classification is not null)
            failure.Classification = updateDto.Classification;
        
        if (updateDto.Confidence.HasValue)
            failure.Confidence = updateDto.Confidence.Value;
        
        if (updateDto.Summary is not null)
            failure.Summary = updateDto.Summary;
        
        if (updateDto.RootCause is not null)
            failure.RootCause = updateDto.RootCause;
        
        if (updateDto.SuggestedFix is not null)
            failure.SuggestedFix = updateDto.SuggestedFix;
        
        if (updateDto.SourceJson is not null)
            failure.SourceJson = updateDto.SourceJson;
        
        if (updateDto.Status is not null && Enum.TryParse<FailureStatus>(updateDto.Status, out var status))
            failure.Status = status;
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
            RetryAttempted = failure.RetryAttempted,
            Status = failure.Status.ToString(),
            CreatedAt = failure.CreatedAt,
            UpdatedAt = failure.UpdatedAt
        };
    }
}
