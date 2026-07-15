using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessActivityConfiguration : IEntityTypeConfiguration<ProcessActivity>
{
    public void Configure(EntityTypeBuilder<ProcessActivity> builder)
    {
        builder.ToTable("ProcessActivities");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.IdProcess, x.Order });

        builder.HasMany(x => x.Params)
            .WithOne(x => x.ProcessActivity)
            .HasForeignKey(x => x.IdProcessActivity)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Executions)
            .WithOne(x => x.ProcessActivity)
            .HasForeignKey(x => x.IdProcessActivity)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
