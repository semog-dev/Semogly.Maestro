using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ActivityExecutionConfiguration : IEntityTypeConfiguration<ActivityExecution>
{
    public void Configure(EntityTypeBuilder<ActivityExecution> builder)
    {
        builder.ToTable("ActivityExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => new { x.IdProcessExecution, x.Status });
    }
}
