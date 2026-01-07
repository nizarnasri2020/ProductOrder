using OrderService.Entities;
using OrderService.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;

namespace OrderService.Messaging
{
    public class ProductEventsConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductEventsConsumer> _logger;

        public ProductEventsConsumer(IServiceScopeFactory scopeFactory, ILogger<ProductEventsConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionFactory factory = new() { HostName = "localhost" };
            IConnection connection = factory.CreateConnection();
            IModel channel = connection.CreateModel();

            // Listen to all 3 queues
            channel.QueueDeclare("product-created", true, false, false);
            channel.QueueDeclare("product-updated", true, false, false);
            channel.QueueDeclare("product-deleted", true, false, false);

            EventingBasicConsumer consumer = new(channel);

            consumer.Received += async (_, ea) =>
            {
                string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                using IServiceScope scope = _scopeFactory.CreateScope();
                OrderDbContext db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                try
                {
                    if (ea.RoutingKey == "product-created")
                    {
                        ProductCreatedEvent evt = JsonSerializer.Deserialize<ProductCreatedEvent>(json)!;
                        if (!db.Products.Any(p => p.Id == evt.ProductId))
                        {
                            db.Products.Add(new ProductCache
                            {
                                Id = evt.ProductId,
                                Name = evt.Name,
                                Price = evt.Price
                            });
                            await db.SaveChangesAsync();
                            _logger.LogInformation("Product cached in OrderService: {ProductId} - {Name}", evt.ProductId, evt.Name);
                        }
                    }
                    else if (ea.RoutingKey == "product-updated")
                    {
                        ProductUpdatedEvent evt = JsonSerializer.Deserialize<ProductUpdatedEvent>(json)!;
                        ProductCache? product = await db.Products.FindAsync(evt.ProductId);
                        if (product != null)
                        {
                            product.Name = evt.Name;
                            product.Price = evt.Price;
                            await db.SaveChangesAsync();
                            _logger.LogInformation("Product updated in cache: {ProductId} - {Name}", evt.ProductId, evt.Name);
                        }
                    }
                    else if (ea.RoutingKey == "product-deleted")
                    {
                        ProductDeletedEvent evt = JsonSerializer.Deserialize<ProductDeletedEvent>(json)!;
                        ProductCache? product = await db.Products.FindAsync(evt.ProductId);
                        if (product != null)
                        {
                            db.Products.Remove(product);
                            await db.SaveChangesAsync();
                            _logger.LogInformation("Product removed from cache: {ProductId}", evt.ProductId);
                        }
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "Error processing event: {RoutingKey}", ea.RoutingKey);
                }

            };

            channel.BasicConsume("product-created", true, consumer);
            channel.BasicConsume("product-updated", true, consumer);
            channel.BasicConsume("product-deleted", true, consumer);

            return Task.CompletedTask;
        }
    }
}
