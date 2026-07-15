using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence.Configurations;

public class ProcessConfiguration : IEntityTypeConfiguration<Process>
{
    public void Configure(EntityTypeBuilder<Process> builder)
    {
        builder.ToTable("Processes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasMany(x => x.ProcessFlows)
            .WithOne(x => x.Process)
            .HasForeignKey(x => x.IdProcess)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Activities)
            .WithOne(x => x.Process)
            .HasForeignKey(x => x.IdProcess)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
