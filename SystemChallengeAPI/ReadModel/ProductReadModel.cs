namespace SystemChallengeAPI.ReadModel
{
    public class ProductReadModel
    {
        public Guid ProductId { get; set; }
        public long Sequence { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;

        public int VersionNumber { get; set; }
        public Guid VersionId { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ApprovedAt { get; set; }
    }
}
