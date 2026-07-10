using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemChallengeAPI.DTOs;
using SystemChallengeAPI.Infrastructure;
using SystemChallengeAPI.ReadModel;

namespace SystemChallengeAPI.Controllers
{
    [ApiController]
    [Route("catalog")]
    [Authorize]
    public class ProductCatalogController : ControllerBase
    {
        private readonly ProductReadDbContext _readDb;

        public ProductCatalogController(ProductReadDbContext readDb)
        {
            _readDb = readDb;
        }

        [HttpGet]
        public async Task<ActionResult<CatalogPage>> GetAll([FromQuery] long after = 0, [FromQuery] int take = 10)
        {
            take = Math.Clamp(take, 1, 100);

            var rows = await _readDb.Products
                .AsNoTracking()
                .Where(p => p.Sequence > after)
                .OrderBy(p => p.Sequence)
                .Take(take + 1)                 // one extra row tells us if there's a next page
                .ToListAsync();

            var hasMore = rows.Count > take;
            if (hasMore)
                rows.RemoveAt(rows.Count - 1);

            return Ok(new CatalogPage
            {
                Items = rows,
                NextCursor = rows.Count > 0 ? rows[^1].Sequence : after,
                HasMore = hasMore,
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductReadModel>> GetById(Guid id)
        {
            var product = await _readDb.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProductId == id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }
}