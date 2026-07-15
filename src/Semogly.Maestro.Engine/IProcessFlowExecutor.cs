using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Engine;

public interface IProcessFlowExecutor
{
    /// <summary>
    /// Runs <paramref name="processFlow"/> once, given its full graph already loaded (Processes, their Activities,
    /// and all Params navigations populated). The caller owns persistence: it loads the graph beforehand and
    /// saves the returned execution tree afterwards.
    /// </summary>
    Task<ProcessFlowExecution> ExecuteAsync(ProcessFlow processFlow, CancellationToken cancellationToken = default);
}
