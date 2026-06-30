class ProductVersion
{
    public Guid Id {get; set;}
    public Guid ProductId {get; set;}
    public int VersionNumber {get; set;}

    public string Name {get; set;}
    public string Description {get; set;}
    public decimal Price {get; set;}
    public string Sku {get; set;}

    public WorkflowStatus Status {get; set;}

    public string CreatedBy {get; set;}
    public DateTime CreatedAt {get; set;}

    public string? DecidedBy {get; set;}
    public DateTime? DecidedAt {get; set;}
    public string? DecisionReason {get; set;}
}