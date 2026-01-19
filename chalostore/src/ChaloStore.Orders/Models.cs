namespace ChaloStore.Orders;

public sealed class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public sealed class Order
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class CheckoutResult
{
    public string Status { get; init; } = "Pending Payment";
}
