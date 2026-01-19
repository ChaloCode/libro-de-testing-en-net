using ChaloStore.Orders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChaloStoreDbContext>(options =>
    options.UseInMemoryDatabase("ChaloStoreDb"));

builder.Services.AddScoped<IEmailService, FakeEmailService>();
builder.Services.AddScoped<IEventBus, FakeEventBus>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

var app = builder.Build();

app.MapPost("/orders", async (
    Order order,
    ChaloStoreDbContext db,
    IEmailService email,
    IEventBus bus,
    IPaymentGateway payments) =>
{
    var product = await db.Products.FindAsync(order.ProductId);
    if (product is null || product.Stock <= 0)
    {
        return Results.Text("Product not available", statusCode: 400);
    }

    var paymentResult = await payments.ChargeAsync(order);
    if (!paymentResult.Success)
    {
        var message = paymentResult.ErrorMessage ?? "Payment rejected";
        return Results.Text(message, statusCode: 503);
    }

    product.Stock -= 1;
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await email.SendOrderConfirmationAsync(order.CustomerEmail, order);
    await bus.PublishOrderCreatedAsync(order);

    return Results.Ok(order);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/checkout", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    const string html = """
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>ChaloStore Checkout</title>
    <base href="http://chalostore.test/">
    <style>
      body { font-family: system-ui, sans-serif; margin: 2rem auto; max-width: 480px; }
      form { display: grid; gap: 1rem; margin-bottom: 1.5rem; }
      label { display: grid; gap: 0.5rem; font-weight: 600; }
      input { padding: 0.6rem; font-size: 1rem; border: 1px solid #ccc; border-radius: 4px; }
      button { padding: 0.75rem 1rem; font-size: 1rem; border: none; border-radius: 4px; background: #2563eb; color: white; cursor: pointer; }
      button:hover { background: #1d4ed8; }
      pre { background: #f4f4f5; padding: 1rem; border-radius: 4px; min-height: 2.5rem; white-space: pre-wrap; }
    </style>
  </head>
  <body>
    <h1>Checkout de ChaloStore</h1>
    <p>Ingresa el ID del producto y tu correo para confirmar el pedido.</p>
    <form id="checkout-form">
      <label>
        ID de producto
        <input id="product-id" type="number" min="1" required />
      </label>
      <label>
        Correo electrónico
        <input id="customer-email" type="email" required />
      </label>
      <button id="submit-button" type="submit">Confirmar pedido</button>
    </form>
    <pre id="result"></pre>
    <script>
      const form = document.getElementById('checkout-form');
      const result = document.getElementById('result');

      form.addEventListener('submit', async (event) => {
        event.preventDefault();
        result.textContent = 'Procesando...';

        const productId = parseInt(document.getElementById('product-id').value, 10);
        const email = document.getElementById('customer-email').value;

        try {
          const response = await fetch('/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, customerEmail: email })
          });

          if (response.ok) {
            const order = await response.json();
            result.textContent = `Pedido creado para el producto ${order.productId ?? productId}`;
          } else {
            const message = await response.text();
            result.textContent = `Error: ${message}`;
          }
        } catch (error) {
          result.textContent = `Error: ${error}`;
        }
      });
    </script>
  </body>
</html>
""";
    await context.Response.WriteAsync(html);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
    db.Database.EnsureCreated();
    if (!db.Products.Any())
    {
        db.Products.Add(new Product { Id = 1, Sku = "SKU-001", Name = "Laptop", Stock = 100 });
        db.SaveChanges();
    }
}

app.Run();

public sealed class FakeEmailService : IEmailService
{
    public Task SendOrderConfirmationAsync(string customerEmail, Order order) => Task.CompletedTask;
}

public sealed class FakeEventBus : IEventBus
{
    public Task PublishOrderCreatedAsync(Order order) => Task.CompletedTask;
}

public partial class Program { }
