using Microsoft.EntityFrameworkCore;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Persistence;

public class MaestroDbContext(DbContextOptions<MaestroDbContext> options) : DbContext(options)
{
    public DbSet<ProcessFlow> ProcessFlows => Set<ProcessFlow>();

    public DbSet<Process> Processes => Set<Process>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<ActivityParam> ActivityParams => Set<ActivityParam>();

    public DbSet<ProcessFlowProcess> ProcessFlowProcesses => Set<ProcessFlowProcess>();

    public DbSet<ProcessActivity> ProcessActivities => Set<ProcessActivity>();

    public DbSet<ProcessActivityParam> ProcessActivityParams => Set<ProcessActivityParam>();

    public DbSet<ProcessFlowExecution> ProcessFlowExecutions => Set<ProcessFlowExecution>();

    public DbSet<ProcessExecution> ProcessExecutions => Set<ProcessExecution>();

    public DbSet<ActivityExecution> ActivityExecutions => Set<ActivityExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaestroDbContext).Assembly);
    }
}
