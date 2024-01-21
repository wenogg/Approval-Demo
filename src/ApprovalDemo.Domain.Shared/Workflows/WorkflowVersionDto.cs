namespace ApprovalDemo.Workflows;

public class WorkflowVersionDto
{
    public string Name { get; set; } = default!;

    public string DefinitionId { get; set; } = default!;

    public int Version { get; set; } = default!;

    public bool IsPublished { get; set; }
}