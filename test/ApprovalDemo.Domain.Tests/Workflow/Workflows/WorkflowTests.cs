using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalDemo.Orders;
using ApprovalDemo.Orders.Workflow;
using ApprovalDemo.Workflow.Activities;
using Elsa.Common.Models;
using Elsa.Extensions;
using Elsa.Testing.Shared;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Options;
using Elsa.Workflows.Runtime.Results;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ApprovalDemo.Workflow.Workflows;

public class WorkflowTests
{
    private readonly CapturingTextWriter _capturingTextWriter = new();
    private readonly ITestOutputHelper _testOutputHelper;

    public WorkflowTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact(DisplayName = "Complete activity must not cascade.")]
    public async Task GivenOrder_WhenStartWorkflow_ThenSuspendsForOrderSubmission()
    {
        // Arrange.
        const int orderId = 1;
        var order = CreateOrder(false);
        var services = CreateServiceProvider(order);
        await services.PopulateRegistriesAsync();

        var runtime = services.GetRequiredService<IWorkflowRuntime>();
        var startOptions = CreateStartWorkflowOptions(orderId);

        // Act
        var workflowState = await runtime.StartWorkflowAsync("23c253cec175fb66", startOptions);

        // Assert
        order.Status.ShouldBe(OrderStatusType.New);
        workflowState.Status.ShouldBe(WorkflowStatus.Running);
        workflowState.SubStatus.ShouldBe(WorkflowSubStatus.Suspended);
        var bookmarks = workflowState.Bookmarks.ToList();
        bookmarks.Count.ShouldBe(2);
        bookmarks[0].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Submit Order");
        bookmarks[1].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Cancel Order");
    }

    [Fact(DisplayName = "Complete activity must not cascade.")]
    public async Task GivenOrderPendingSubmission_WhenStartWorkflow_WhenApplyActionSubmitOrder_ThenTransitionsToPendingOrderPreparation()
    {
        // Arrange.
        const int orderId = 1;
        var order = CreateOrder(false);
        var services = CreateServiceProvider(order);
        await services.PopulateRegistriesAsync();
        var runtime = services.GetRequiredService<IWorkflowRuntime>();
        var startOptions = CreateStartWorkflowOptions(orderId);
        var workflowState = await runtime.StartWorkflowAsync("23c253cec175fb66", startOptions);

        // Act
        var result = await ApplyAction(runtime, workflowState, "Submit Order", "tester");

        // Assert
        order.Status.ShouldBe(OrderStatusType.Preparing);
        result.Status.ShouldBe(WorkflowStatus.Running);
        result.SubStatus.ShouldBe(WorkflowSubStatus.Suspended);
        var bookmarks = workflowState.Bookmarks.ToList();
        bookmarks.Count.ShouldBe(2);
        bookmarks[0].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Mark Prepared");
        bookmarks[1].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Return for correction");
    }

    [Fact(DisplayName = "Complete activity must not cascade.")]
    public async Task GivenOrderIsHot_WhenSubmit_WhenApplyActionSubmitOrder_ThenTransitionsToPendingOrderPreparation()
    {
        // Arrange.
        const int orderId = 1;
        var order = CreateOrder(true);
        var services = CreateServiceProvider(order);
        await services.PopulateRegistriesAsync();
        var runtime = services.GetRequiredService<IWorkflowRuntime>();
        var startOptions = CreateStartWorkflowOptions(orderId);
        var workflowState = await runtime.StartWorkflowAsync("23c253cec175fb66", startOptions);

        // Act
        var result = await ApplyAction(runtime, workflowState, "Submit Order", "tester");

        // Assert
        order.Status.ShouldBe(OrderStatusType.Preparing);
        result.Status.ShouldBe(WorkflowStatus.Running);
        result.SubStatus.ShouldBe(WorkflowSubStatus.Suspended);
        var bookmarks = workflowState.Bookmarks.ToList();
        bookmarks.Count.ShouldBe(2);
        bookmarks[0].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Mark Prepared");
        bookmarks[1].Payload.ShouldBeOfType<UserActionBookmarkPayload>().Action.ShouldBe("Return for correction");
    }

    private static Bookmark GetBookmark(WorkflowExecutionResult workflowState, string action)
    {
        return workflowState.Bookmarks
            .First(s => s.Payload is UserActionBookmarkPayload payload && payload.Action == action);
    }

    private async Task<WorkflowExecutionResult> ApplyAction(IWorkflowRuntime runtime, WorkflowExecutionResult workflowState, string action, string userName)
    {
        var bookmark = GetBookmark(workflowState, action);
        var userTaskInput = new AuthorizedUserTaskInput(action, userName);
        var inputPayload = new Dictionary<string, object>
        {
            [UserActionBookmarkPayload.InputName] = userTaskInput
        };
        var options = new ResumeWorkflowRuntimeOptions
        {
            BookmarkId = bookmark.Id,
            Input = inputPayload
        };
        var result = await runtime.ResumeWorkflowAsync(workflowState.WorkflowInstanceId, options);
        return result;
    }

    private static StartWorkflowRuntimeOptions CreateStartWorkflowOptions(int orderId)
    {
        var startOptions = new StartWorkflowRuntimeOptions
        {
            CorrelationId = $"{Order.CorrelationIdPrefix}{orderId}",
            Input = new Dictionary<string, object>
            {
                ["item.Id"] = orderId
            },
            VersionOptions = VersionOptions.Published
        };
        return startOptions;
    }

    private Order CreateOrder(bool isHot) =>
        new Order()
            {
                Item = "item",
                Description = "description",
                IsHot = isHot
            };

    private IServiceProvider CreateServiceProvider(Order order)
    {
        var services = new TestApplicationBuilder(_testOutputHelper)
            .ConfigureServices(services =>
            {
                var orderManager = Substitute.For<IOrderManager>();
                var setOrderStatus = (OrderStatusType status) =>
                {
                    order.Status = status;
                    return Task.CompletedTask;
                };
                orderManager.SetStatus(Arg.Any<int>(), Arg.Any<OrderStatusType>())
                    .Returns(opt => setOrderStatus(opt.ArgAt<OrderStatusType>(1)));

                var orderRepository = Substitute.For<IRepository<Order, int>>();
                orderRepository.FindAsync(Arg.Any<int>()).Returns(order);
                services.AddSingleton(orderManager);
                services.AddSingleton(orderRepository);
            })
            .WithCapturingTextWriter(_capturingTextWriter)
            .WithWorkflowsFromDirectory("Workflow", "Definitions")
            .ConfigureElsa(elsa =>
            {
                elsa
                    .UseCSharp(opt => opt.Assemblies.Add(typeof(Order).Assembly))
                    .AddActivitiesFrom<SetOrderStatusActivity>()
                    .AddActivitiesFrom<AuthorizedUserTask>();
            })
            .Build();

        return services;
    }
}