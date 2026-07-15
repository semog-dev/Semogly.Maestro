using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Type).HasMaxLength(100).IsRequired();

        builder.HasMany(x => x.Params)
            .WithOne(x => x.Activity)
            .HasForeignKey(x => x.IdActivity)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProcessActivities)
            .WithOne(x => x.Activity)
            .HasForeignKey(x => x.IdActivity)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
