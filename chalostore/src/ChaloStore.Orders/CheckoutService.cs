namespace ChaloStore.Orders;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string customerEmail, Order order);
}

public interface IEventBus
{
    Task PublishOrderCreatedAsync(Order order);
}

public interface IInventoryGateway
{
    Task<bool> HasStockAsync(string sku, int quantity);
    Task ReserveAsync(string sku, int quantity);
}

public sealed class CheckoutService
{
    private readonly IInventoryGateway _inventory;
    private readonly IEmailService _email;
    private readonly IEventBus _bus;

    public CheckoutService(IInventoryGateway inventory, IEmailService email, IEventBus bus)
    {
        _inventory = inventory;
        _email = email;
        _bus = bus;
    }

    public async Task<CheckoutResult> ProcessAsync(Order order, Product product)
    {
        if (!await _inventory.HasStockAsync(product.Sku, 1))
        {
            throw new InvalidOperationException("No stock available");
        }

        await _inventory.ReserveAsync(product.Sku, 1);
        await _email.SendOrderConfirmationAsync(order.CustomerEmail, order);
        await _bus.PublishOrderCreatedAsync(order);
        return new CheckoutResult { Status = "Pending Payment" };
    }
}
