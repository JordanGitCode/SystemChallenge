using System.ComponentModel.DataAnnotations;

namespace SystemChallengeAPI.DTOs
{
    public class UpdateProductRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Sku { get; set; }

    }
}
