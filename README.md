# Semogly.Maestro

Biblioteca .NET 10 para modelar e executar pipelines de processos agendados (workflow engine). Um `ProcessFlow` roda um conjunto ordenado de `Process`, cada `Process` roda um conjunto ordenado de `Activity`, e cada `Activity` é executada por um `IActivityHandler` registrado sob a string `Activity.Type`.

## Pacotes

| Pacote | Do que depende | Para que serve |
|---|---|---|
| `Semogly.Maestro.Core` | — | Entidades do domínio (`ProcessFlow`, `Process`, `Activity`, `*Execution`, etc.) |
| `Semogly.Maestro.Abstractions` | Core | Contrato de execução de activities (`IActivityHandler`, `IActivityHandlerResolver`) + wiring de DI |
| `Semogly.Maestro.Engine` | Core, Abstractions | `IProcessFlowExecutor` — orquestrador puro, sem persistência |
| `Semogly.Maestro.Persistence` | Core, Abstractions, EF Core | `MaestroDbContext` (agnóstico de provedor — você escolhe SQL Server, Postgres, etc.) |
| `Semogly.Maestro.Activities.FileSystem` | Core, Abstractions | Handlers `file-move`, `file-copy`, `file-delete` |
| `Semogly.Maestro.Activities.Http` | Core, Abstractions | Handler `http-request` |

Instale só o que for usar — os pacotes de Activities são opcionais e cada categoria nova (email, etc.) vem em um pacote próprio.

## Consumindo os pacotes localmente

Enquanto a lib não está publicada em um feed público, gere os `.nupkg` e aponte um source local do NuGet para eles:

```bash
dotnet pack -c Release
```

Isso gera os pacotes em `artifacts/packages/` (configurado em `Directory.Build.props`). No projeto consumidor:

```bash
dotnet add package Semogly.Maestro.Engine --source /caminho/para/semogly/artifacts/packages
dotnet add package Semogly.Maestro.Persistence --source /caminho/para/semogly/artifacts/packages
dotnet add package Semogly.Maestro.Activities.FileSystem --source /caminho/para/semogly/artifacts/packages
dotnet add package Semogly.Maestro.Activities.Http --source /caminho/para/semogly/artifacts/packages
```

`--source` precisa do caminho literal (o nome de um source cadastrado via `dotnet nuget add source --name` não é resolvido por `dotnet add package` de forma confiável). Se preferir não repetir o caminho, cadastre-o como source padrão do projeto num `nuget.config` local (`dotnet new nugetconfig` e edite `<add key="semogly-local" value="/caminho/.../artifacts/packages" />`).

## Registrando no DI

Cada pacote expõe sua própria extensão de `IServiceCollection` — componha só o que precisar.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Semogly.Maestro.Activities.FileSystem;
using Semogly.Maestro.Activities.Http;
using Semogly.Maestro.Engine;
using Semogly.Maestro.Persistence;

var services = new ServiceCollection();

// Executor + resolver padrão de IActivityHandler (baseado em keyed services)
services.AddMaestroEngine();

// Handlers built-in — registre só os pacotes que for usar
services.AddFileSystemActivities();
services.AddHttpActivities(); // registra HttpRequestActivityHandler com HttpClient tipado via IHttpClientFactory

// Persistence é agnóstico de provedor — você escolhe (SQL Server, Postgres, etc.)
services.AddDbContext<MaestroDbContext>(options =>
    options.UseSqlServer("Server=...;Database=...;"));

var provider = services.BuildServiceProvider();
```

Para registrar um `IActivityHandler` próprio (ex.: um handler de e-mail que você mesmo implementou):

```csharp
using Semogly.Maestro.Abstractions.Activities;

services.AddActivityHandler<MyEmailActivityHandler>("email-send");

// ou, quando o handler precisa de wiring customizado (ex.: um client tipado):
services.AddActivityHandler("email-send", sp => sp.GetRequiredService<MyEmailActivityHandler>());
```

## Montando e executando um ProcessFlow

O `ProcessFlowExecutor` é puro: recebe um `ProcessFlow` já carregado com todas as navegações populadas e devolve uma árvore de execução (`ProcessFlowExecution` → `ProcessExecution` → `ActivityExecution`). Quem chama é responsável por carregar o grafo antes e persistir a árvore devolvida depois.

```csharp
using Semogly.Maestro.Activities.FileSystem;
using Semogly.Maestro.Core.Entities;
using Semogly.Maestro.Engine;

// 1. Definir o Activity a partir do factory do pacote (mantém os ActivityParam em sincronia com o handler)
var moveFileActivity = FileSystemActivityDefinitions.MoveFile();

var process = new Process { Name = "Arquivar relatório", ContinueOnActivityFailure = false };
var processActivity = new ProcessActivity
{
    IdProcess = process.Id,
    IdActivity = moveFileActivity.Id,
    Order = 0,
    Process = process,
    Activity = moveFileActivity,
};
processActivity.Params.Add(new ProcessActivityParam { IdProcessActivity = processActivity.Id, Key = "SourcePath", Value = "/dados/relatorio.csv" });
processActivity.Params.Add(new ProcessActivityParam { IdProcessActivity = processActivity.Id, Key = "DestinationPath", Value = "/arquivo/relatorio.csv" });
process.Activities.Add(processActivity);

var flow = new ProcessFlow { Name = "Rotina diária", CronParam = "0 3 * * *", ContinueOnProcessFailure = false };
var link = new ProcessFlowProcess { IdProcessFlow = flow.Id, IdProcess = process.Id, Order = 0, ProcessFlow = flow, Process = process };
flow.Processes.Add(link);

// 2. Executar
var executor = provider.GetRequiredService<IProcessFlowExecutor>();
var execution = await executor.ExecuteAsync(flow);

// 3. Persistir a árvore de execução inteira (EF percorre as navegações a partir de um único Add)
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MaestroDbContext>();
db.Add(execution);
await db.SaveChangesAsync();
```

### Comportamento de falha e retry

- `Activity.MaxRetryCount`: tentativas extras imediatas (sem backoff) antes de marcar a `ActivityExecution` como `Failed`. `ActivityHandlerNotFoundException` nunca é retentada.
- `Process.ContinueOnActivityFailure = false` (padrão): uma activity falha interrompe o resto daquele `Process`.
- `ProcessFlow.ContinueOnProcessFailure = false` (padrão): um `Process` falho interrompe os `Process`es seguintes do flow.
- Um `ActivityParam` `Required` sem valor (nem override, nem `DefaultValue`) falha a `ActivityExecution` antes mesmo de chamar o handler, e não é retentado.

## Build e testes

```bash
dotnet build
dotnet test
```

Requer .NET 10 SDK. O arquivo de solução é `Semogly.Maestro.slnx`.
