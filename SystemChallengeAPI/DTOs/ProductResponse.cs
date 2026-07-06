using SystemChallengeAPI.Domain;

namespace SystemChallengeAPI.DTOs
{
    public class ProductResponse
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public Guid CurrentVersionId { get; set; }
        public WorkflowStatus Status { get; set; }

    }
}
