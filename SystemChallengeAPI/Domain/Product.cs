namespace SystemChallengeAPI.Domain;

public class Product
{
    public Guid Id {get; set;}
    
    public bool IsDeleted {get; set;}
    public DateTime? DeletedAt {get; set;}
    public string? DeletedBy {get;set;}

    public string CreatedBy {get; set;}
    public DateTime CreatedAt {get; set;}

    public Guid? CurrentApprovedVersionId {get; set;}

    public ICollection<ProductVersion> Versions {get; set;}
}