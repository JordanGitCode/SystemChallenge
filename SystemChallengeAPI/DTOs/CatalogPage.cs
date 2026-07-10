using SystemChallengeAPI.ReadModel;

namespace SystemChallengeAPI.DTOs
{
    public class CatalogPage
    {
        public IReadOnlyList<ProductReadModel> Items { get; set; } = [];
        public long NextCursor { get; set; }
        public bool HasMore { get; set; }
    }
}
