using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessFlowExecutionConfiguration : IEntityTypeConfiguration<ProcessFlowExecution>
{
    public void Configure(EntityTypeBuilder<ProcessFlowExecution> builder)
    {
        builder.ToTable("ProcessFlowExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => new { x.IdProcessFlow, x.Status });

        builder.HasMany(x => x.ProcessExecutions)
            .WithOne(x => x.ProcessFlowExecution)
            .HasForeignKey(x => x.IdProcessFlowExecution)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
