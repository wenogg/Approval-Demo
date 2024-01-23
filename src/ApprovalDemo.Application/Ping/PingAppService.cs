using System;
using System.Threading.Tasks;
using Elsa.Common.Models;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.Ping;

public interface IPingAppService
{
    Task<string> Get();

    Task<string> GetCodeActivity();

    Task<string> SendReminders();
}

/// <summary>
/// Runs a code workflow test
/// </summary>
public class PingAppService(
    IWorkflowDefinitionStore workflowDefinitionStore,
    IWorkflowRunner workflowRunner,
    IWorkflowRuntime workflowRuntime) : ApplicationService, IPingAppService
{
    private const string WorkflowDefinitionName = "Ping";
    private const string ReminderDefinitionName = "Reminders";

    public async Task<string> Get()
    {
        var workflowDefinition = await FindWorkflowDefinition(WorkflowDefinitionName);
        await RunWorkflow(WorkflowDefinitionName);
        return "Pong";
    }

    public async Task<string> GetCodeActivity()
    {
        await workflowRunner.RunAsync(new WriteLine("Hello ASP.NET world!"));
        return "Pong";
    }

    public async Task<string> SendReminders()
    {
        await RunWorkflow(ReminderDefinitionName);
        return "Pong";
    }

    private async Task RunWorkflow(string workflowName)
    {

        var workflowDefinition = await FindWorkflowDefinition(workflowName);
        if (workflowDefinition == null)
        {
            throw new UserFriendlyException($"Could not find workflow definition {WorkflowDefinitionName}");
        }

        var startOptions = new StartWorkflowRuntimeOptions
        {
            CorrelationId = Guid.NewGuid().ToString(),
            VersionOptions = VersionOptions.SpecificVersion(workflowDefinition.Version)
        };
        await workflowRuntime.StartWorkflowAsync(workflowDefinition.DefinitionId, startOptions);

        await workflowRunner.RunAsync(new WriteLine("Hello ASP.NET world!"));
    }

    private async Task<WorkflowDefinition?> FindWorkflowDefinition(string workflowName)
    {
        var filter = new WorkflowDefinitionFilter()
        {
            Name = workflowName,
            VersionOptions = VersionOptions.Published
        };
        var workflowBlueprint = await workflowDefinitionStore.FindAsync(filter);

        if (workflowBlueprint == null)
        {
            throw new UserFriendlyException($"Could not find workflow blueprint with tag {WorkflowDefinitionName}");
        }

        return workflowBlueprint;
    }
}