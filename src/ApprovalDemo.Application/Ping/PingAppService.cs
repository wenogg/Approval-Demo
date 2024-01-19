using System.Threading.Tasks;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.Ping;

public interface IPingAppService
{
    Task<string> Get();
}

/// <summary>
/// Runs a code workflow test
/// </summary>
public class PingAppService(IWorkflowRunner workflowRunner) : ApplicationService, IPingAppService
{
    public async Task<string> Get()
    {
        await workflowRunner.RunAsync(new WriteLine("Hello ASP.NET world!"));
        return "Pong";
    }
}