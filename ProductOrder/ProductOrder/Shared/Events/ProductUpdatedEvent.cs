namespace Shared.Events
{
    public class ProductUpdatedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
