using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessActivityParamConfiguration : IEntityTypeConfiguration<ProcessActivityParam>
{
    public void Configure(EntityTypeBuilder<ProcessActivityParam> builder)
    {
        builder.ToTable("ProcessActivityParams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(1000);

        builder.HasIndex(x => new { x.IdProcessActivity, x.Key }).IsUnique();
    }
}
