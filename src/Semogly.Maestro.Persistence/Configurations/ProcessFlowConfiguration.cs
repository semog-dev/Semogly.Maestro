using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessFlowConfiguration : IEntityTypeConfiguration<ProcessFlow>
{
    public void Configure(EntityTypeBuilder<ProcessFlow> builder)
    {
        builder.ToTable("ProcessFlows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CronParam).HasMaxLength(100);

        builder.HasMany(x => x.Processes)
            .WithOne(x => x.ProcessFlow)
            .HasForeignKey(x => x.IdProcessFlow)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Executions)
            .WithOne(x => x.ProcessFlow)
            .HasForeignKey(x => x.IdProcessFlow)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
