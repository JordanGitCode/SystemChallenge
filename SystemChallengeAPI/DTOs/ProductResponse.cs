using System.ComponentModel.DataAnnotations;
using SystemChallengeAPI.Domain;

namespace SystemChallengeAPI.DTOs
{
    public class ProductResponse
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;
        public Guid CurrentVersionId { get; set; }
        public WorkflowStatus Status { get; set; }

    }
}
