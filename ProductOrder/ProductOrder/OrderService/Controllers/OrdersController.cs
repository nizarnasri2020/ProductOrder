using Microsoft.AspNetCore.Mvc;
using OrderService.Entities;
using OrderService.Persistence;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _db;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderDbContext db, ILogger<OrdersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, int quantity)
        {
            ProductCache? product = await _db.Products.FindAsync(productId);
            if (product == null)
                return BadRequest("Product not available yet");

            Order order = new()
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Ok(order);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Order? order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} deleted", id);
            return NoContent();
        }
    }
}
