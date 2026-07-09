namespace SystemChallengeAPI.DTOs
{
    public class PendingVersionResponse
    {

        public Guid ProductId { get; set; }
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
