using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessExecutionConfiguration : IEntityTypeConfiguration<ProcessExecution>
{
    public void Configure(EntityTypeBuilder<ProcessExecution> builder)
    {
        builder.ToTable("ProcessExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => new { x.IdProcessFlowExecution, x.Status });

        builder.HasMany(x => x.ActivityExecutions)
            .WithOne(x => x.ProcessExecution)
            .HasForeignKey(x => x.IdProcessExecution)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
