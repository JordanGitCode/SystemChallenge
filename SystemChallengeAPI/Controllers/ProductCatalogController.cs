using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<IEnumerable<ProductReadModel>>> GetAll()
        {
            var products = await _readDb.Products
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
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