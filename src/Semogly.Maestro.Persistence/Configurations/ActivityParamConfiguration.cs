using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ActivityParamConfiguration : IEntityTypeConfiguration<ActivityParam>
{
    public void Configure(EntityTypeBuilder<ActivityParam> builder)
    {
        builder.ToTable("ActivityParams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DefaultValue).HasMaxLength(1000);

        builder.HasIndex(x => new { x.IdActivity, x.Key }).IsUnique();
    }
}
