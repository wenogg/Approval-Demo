using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalDemo.ApprovalItems;
using ApprovalDemo.Workflows;
using Microsoft.AspNetCore.Components;

namespace ApprovalDemo.Blazor.Pages.ApprovalItem;

public partial class Index
{
    [Inject] private IApprovalItemAppService ApprovalItemAppService { get; set; }

    private bool Loading { get; set; }

    private List<WorkflowVersionDto> WorkflowVersions { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        WorkflowVersions = await ApprovalItemAppService.GetWorkflowVersions();
    }

    protected override async Task OpenCreateModalAsync()
    {
        try
        {
            if (CreateValidationsRef != null)
            {
                await CreateValidationsRef.ClearAll();
            }

            await CheckCreatePolicyAsync();

            NewEntity = new UpdateApprovalItemDto()
            {
                WorkflowDefinitionVersion = WorkflowVersions.First(s => s.IsPublished).Version
            };

            // Mapper will not notify Blazor that binded values are changed
            // so we need to notify it manually by calling StateHasChanged
            await InvokeAsync(async () =>
            {
                StateHasChanged();
                if (CreateModal != null)
                {
                    await CreateModal.Show();
                }

            });
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    protected override async Task CreateEntityAsync()
    {
        Loading = true;
        try
        {
            await base.CreateEntityAsync();
        }
        finally
        {
            Loading = false;
        }
    }
}