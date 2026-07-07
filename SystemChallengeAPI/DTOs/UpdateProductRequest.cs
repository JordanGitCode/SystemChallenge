using System.ComponentModel.DataAnnotations;

namespace SystemChallengeAPI.DTOs
{
    public class UpdateProductRequest
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public required string Sku { get; set; }

    }
}
