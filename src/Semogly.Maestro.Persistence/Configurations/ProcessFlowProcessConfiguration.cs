using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessFlowProcessConfiguration : IEntityTypeConfiguration<ProcessFlowProcess>
{
    public void Configure(EntityTypeBuilder<ProcessFlowProcess> builder)
    {
        builder.ToTable("ProcessFlowProcesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CronParam).HasMaxLength(100);

        builder.HasIndex(x => new { x.IdProcessFlow, x.Order });

        builder.HasMany(x => x.Executions)
            .WithOne(x => x.ProcessFlowProcess)
            .HasForeignKey(x => x.IdProcessFlowProcess)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
