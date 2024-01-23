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
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Options;
using Elsa.Workflows.Runtime.Results;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using TechTalk.SpecFlow;
using Volo.Abp.Domain.Repositories;
using Xunit.Abstractions;

namespace ApprovalDemo.Workflow.Workflows;

[Binding]
public class OrderWorkflowSteps
{
    private const string WorkflowDefinitionId = "23c253cec175fb66";
    private IServiceProvider _serviceProvider;
    private IWorkflowRuntime _workflowRuntime;
    private WorkflowExecutionResult _workflowState;
    private Order _order;

    [BeforeScenario]
    public void BeforeScenario()
    {
    }

    [Given(@"order")]
    public async Task GivenAnOrder()
    {
        _order = new Order()
        {
            Item = "item",
            Description = "description",
            Status = OrderStatusType.New
        };
        _serviceProvider = CreateServiceProvider(_order);
        await _serviceProvider.PopulateRegistriesAsync();
        _workflowRuntime = _serviceProvider.GetRequiredService<IWorkflowRuntime>();
        var startOptions = CreateStartWorkflowOptions(1);
        _workflowState = await _workflowRuntime.StartWorkflowAsync(WorkflowDefinitionId, startOptions);
    }

    [When(@"I take the action of \((.*)\)")]
    public async Task WhenITakeTheActionOf(string action)
    {
        var bookmark = GetBookmark(_workflowState, action);
        var userTaskInput = new AuthorizedUserTaskInput(action, "xxx");
        var inputPayload = new Dictionary<string, object>
        {
            [UserActionBookmarkPayload.InputName] = userTaskInput
        };
        var options = new ResumeWorkflowRuntimeOptions
        {
            BookmarkId = bookmark.Id,
            Input = inputPayload
        };
        _ = await _workflowRuntime.ResumeWorkflowAsync(_workflowState.WorkflowInstanceId, options);
    }

    [Then(@"the order status should be \((.*)\)")]
    public void ThenTheOrderStatusShouldBe(string status)
    {
        _order.Status.ToString().ShouldBe(status);
    }

    [Then(@"the next actions are \((.*)\)")]
    public void ThenTheNextActionsAre(string actions)
    {
        var expectedActions = actions.Split(',');
        var bookmarks = _workflowState.Bookmarks.ToList();
        bookmarks.Count.ShouldBe(expectedActions.Length);

        var nextActions = bookmarks
            .Select(s => s.Payload)
            .Cast<UserActionBookmarkPayload>()
            .Select(s => s.Action)
            .ToList();

        nextActions.ShouldBe(expectedActions);
    }

    [Then(@"the workflow is complete")]
    public void ThenTheWorkflowIsComplete()
    {
        var bookmarks = _workflowState.Bookmarks.ToList();
        bookmarks.ShouldBeEmpty();
    }

    private IServiceProvider CreateServiceProvider(Order order)
    {
        var testOutputHelper = Substitute.For<ITestOutputHelper>();
        var services = new TestApplicationBuilder(testOutputHelper)
            .ConfigureServices(services =>
            {
                var orderManager = CreateOrderManagerMock(order);
                var orderRepository = Substitute.For<IRepository<Order, int>>();
                orderRepository.FindAsync(Arg.Any<int>()).Returns(order);
                services.AddSingleton(orderManager);
                services.AddSingleton(orderRepository);
            })
            // .WithCapturingTextWriter(_capturingTextWriter)
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

    private IOrderManager CreateOrderManagerMock(Order order)
    {
        var orderManager = Substitute.For<IOrderManager>();

        Task SetOrderStatus(OrderStatusType status)
        {
            order.Status = status;
            return Task.CompletedTask;
        }

        orderManager.SetStatus(Arg.Any<int>(), Arg.Any<OrderStatusType>())
            .Returns(opt => SetOrderStatus(opt.ArgAt<OrderStatusType>(1)));
        return orderManager;
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

    private static Bookmark GetBookmark(WorkflowExecutionResult workflowState, string action)
    {
        return workflowState.Bookmarks
            .First(s => s.Payload is UserActionBookmarkPayload payload && payload.Action == action);
    }
}