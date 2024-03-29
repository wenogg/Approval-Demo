﻿using System.Collections.Generic;
using ApprovalDemo.ApprovalItems;
using Volo.Abp.Application.Dtos;

namespace ApprovalDemo.Orders;

public class OrderDto : AuditedEntityDto<int>
{
    public string Item { get; set; } = default!;

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public OrderStatusType Status { get; set; } = OrderStatusType.New;

    public bool IsHot { get; set; }

    public List<string> Actions { get; set; } = [];

    public List<JournalEntryDto> Journal { get; set; } = [];
}