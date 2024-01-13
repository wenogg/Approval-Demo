using System;

namespace ApprovalDemo.ApprovalItems;

public class JournalEntryDto
{
    public string Message { get; set; } = default!;

    public DateTime TimeStamp { get; set; }
}