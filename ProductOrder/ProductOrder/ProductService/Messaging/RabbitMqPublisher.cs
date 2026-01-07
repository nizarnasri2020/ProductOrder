using RabbitMQ.Client;
using Shared.Events;
using System.Text;
using System.Text.Json;

namespace ProductService.Messaging
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;

        public RabbitMqPublisher()
        {
            ConnectionFactory factory = new()
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
        }

        public void PublishProductCreated(ProductCreatedEvent evt)
        {
            using IModel channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: "product-created",
                durable: true,
                exclusive: false,
                autoDelete: false);

            byte[] body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(evt));

            channel.BasicPublish(
                exchange: "",
                routingKey: "product-created",
                body: body);
        }
        public void PublishProductUpdated(ProductUpdatedEvent evt)
        {
            using IModel channel = _connection.CreateModel();
            channel.QueueDeclare("product-updated", durable: true, exclusive: false, autoDelete: false);
            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            channel.BasicPublish("", "product-updated", body: body);
        }

        public void PublishProductDeleted(ProductDeletedEvent evt)
        {
            using IModel channel = _connection.CreateModel();
            channel.QueueDeclare("product-deleted", durable: true, exclusive: false, autoDelete: false);
            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            channel.BasicPublish("", "product-deleted", body: body);
        }
    }
}
