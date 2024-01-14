namespace ApprovalDemo.Orders;

public enum OrderStatusType
{
    New = 1,
    Preparing = 2,
    ReturnedForCorrection = 3,
    Shipping = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
}