using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PipelineFailure> PipelineFailures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PipelineFailure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RunId).IsRequired();
            entity.Property(e => e.PipelineName).IsRequired();
            entity.Property(e => e.ErrorMessage).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}
