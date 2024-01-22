using System;
using System.Threading.Tasks;
using ApprovalDemo.Orders;
using ApprovalDemo.Orders.Workflow;
using Elsa.Expressions.Models;
using Elsa.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ApprovalDemo.Workflow.Workflows;

public class SetOrderStatusActivityTests
{
    private readonly CapturingTextWriter _capturingTextWriter = new();
    private readonly ITestOutputHelper _testOutputHelper;

    public SetOrderStatusActivityTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(OrderStatusType.New)]
    [InlineData(OrderStatusType.Preparing)]
    [InlineData(OrderStatusType.ReturnedForCorrection)]
    [InlineData(OrderStatusType.Delivered)]
    [InlineData(OrderStatusType.Shipping)]
    [InlineData(OrderStatusType.Shipped)]
    [InlineData(OrderStatusType.Cancelled)]
    public async Task GivenSetOrderStatusActivity_WhenExecuted_ThenOrderStatusIsUpdated(OrderStatusType status)
    {
        // Arrange
        var order = new Order()
        {
            Item = "item",
            Description = "description",
            Status = OrderStatusType.New
        };
        var services = CreateServiceProvider(order);
        var id = new Literal<string>("1");
        var activity = new SetOrderStatusActivity(id, status.ToString());

        // Act
        await services.RunActivityAsync(activity);

       // Assert
       order.Status.ShouldBe(status);
    }

    private IServiceProvider CreateServiceProvider(Order order)
    {
        var services = new TestApplicationBuilder(_testOutputHelper)
            .ConfigureServices(services =>
            {
                var orderManager = CreateOrderManagerMock(order);
                var orderRepository = Substitute.For<IRepository<Order, int>>();
                orderRepository.FindAsync(Arg.Any<int>()).Returns(order);
                services.AddSingleton(orderManager);
                services.AddSingleton(orderRepository);
            })
            .WithCapturingTextWriter(_capturingTextWriter)
            .AddActivitiesFrom<SetOrderStatusActivity>()
            .Build();
        return services;
    }

    private IOrderManager CreateOrderManagerMock(Order order)
    {
        var orderManager = Substitute.For<IOrderManager>();
        var setOrderStatus = (OrderStatusType status) =>
        {
            order.Status = status;
            return Task.CompletedTask;
        };
        orderManager.SetStatus(Arg.Any<int>(), Arg.Any<OrderStatusType>())
            .Returns(opt => setOrderStatus(opt.ArgAt<OrderStatusType>(1)));
        return orderManager;
    }
}