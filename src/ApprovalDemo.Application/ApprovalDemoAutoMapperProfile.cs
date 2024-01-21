using ApprovalDemo.ApprovalItems;
using ApprovalDemo.Orders;
using ApprovalDemo.Workflows;
using AutoMapper;
using Elsa.Workflows.Management.Entities;

namespace ApprovalDemo;

public class ApprovalDemoAutoMapperProfile : Profile
{
    public ApprovalDemoAutoMapperProfile()
    {
        CreateMap<ApprovalItem, ApprovalItemDto>()
            .ReverseMap();

        CreateMap<ApprovalItemDto, UpdateApprovalItemDto>();
        CreateMap<UpdateApprovalItemDto, ApprovalItem>();
        // CreateMap<WorkflowExecutionLogRecord, JournalEntryDto>()
        //     .ForMember(dest => dest.Message, opt => opt.MapFrom(source => source.ActivityType)); ;

        CreateMap<Order, OrderDto>()
            .ReverseMap();

        CreateMap<OrderDto, UpdateOrderDto>();
        CreateMap<UpdateOrderDto, Order>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<WorkflowDefinition, WorkflowVersionDto>();
    }
}