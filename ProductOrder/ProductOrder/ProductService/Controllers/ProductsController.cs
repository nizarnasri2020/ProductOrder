using Microsoft.AspNetCore.Mvc;
using ProductService.Entities;
using ProductService.Messaging;
using ProductService.Persistence;
using Shared.Events;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _db;
        private readonly RabbitMqPublisher _publisher;
        private readonly ILogger<ProductsController> _logger;


        public ProductsController(
            ProductDbContext db,
            RabbitMqPublisher publisher, ILogger<ProductsController> logger)
        {
            _db = db;
            _publisher = publisher;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            _publisher.PublishProductCreated(new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            });
            _logger.LogInformation("Product created: {ProductId} - {Name}", product.Id, product.Name);
            return Ok(product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product updatedProduct)
        {
            Product? product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;

            await _db.SaveChangesAsync();

            _publisher.PublishProductUpdated(new ProductUpdatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            });
            _logger.LogInformation("Product Updated: {ProductId} - {Name}", product.Id, product.Name);
            return Ok(product);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Product? product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            _publisher.PublishProductDeleted(new ProductDeletedEvent
            {
                ProductId = product.Id
            });
            _logger.LogInformation("Product Deleted: {ProductId} - {Name}", product.Id, product.Name);
            return NoContent();
        }
    }
}
