using ChaloStore.Orders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace ChaloStore.AcceptanceTests.Support;

internal sealed class ChaloStoreApplication : WebApplicationFactory<Program>
{
    private readonly Uri _paymentBaseAddress;
    private readonly string _databaseName = $"ChaloStore.AcceptanceTests-{Guid.NewGuid()}";

    public CapturingEmailService EmailService { get; } = new();
    public CapturingEventBus EventBus { get; } = new();

    public ChaloStoreApplication(Uri paymentBaseAddress)
    {
        _paymentBaseAddress = paymentBaseAddress;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceRegistration(services, typeof(DbContextOptions<ChaloStoreDbContext>));
            ReplaceRegistration(services, typeof(IEmailService));
            ReplaceRegistration(services, typeof(IEventBus));
            ReplaceRegistration(services, typeof(IPaymentGateway));

            services.AddDbContext<ChaloStoreDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddSingleton<IEmailService>(_ => EmailService);
            services.AddSingleton<IEventBus>(_ => EventBus);
            services.AddHttpClient("Payments", client => client.BaseAddress = _paymentBaseAddress);
            services.AddSingleton<IPaymentGateway>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new HttpPaymentGateway(httpClientFactory.CreateClient("Payments"));
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
            db.Database.EnsureCreated();
            db.Products.RemoveRange(db.Products);
            db.Orders.RemoveRange(db.Orders);
            db.SaveChanges();
        });
    }

    private static void ReplaceRegistration(IServiceCollection services, Type serviceType)
    {
        var descriptors = services.Where(d => d.ServiceType == serviceType).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    public async Task ResetStateAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        db.Products.RemoveRange(db.Products);
        db.Orders.RemoveRange(db.Orders);
        await db.SaveChangesAsync();
        EmailService.Reset();
        EventBus.Reset();
    }

    public async Task SeedProductAsync(Product product)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        db.Products.Add(product);
        await db.SaveChangesAsync();
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        return await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<Order?> GetOrderByProductAsync(int productId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        return await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.ProductId == productId);
    }

    internal sealed class CapturingEmailService : IEmailService
    {
        private readonly List<(string Email, Order Order)> _messages = new();

        public IReadOnlyCollection<(string Email, Order Order)> Messages => _messages;

        public Task SendOrderConfirmationAsync(string customerEmail, Order order)
        {
            _messages.Add((customerEmail, order));
            return Task.CompletedTask;
        }

        public void Reset() => _messages.Clear();
    }

    internal sealed class CapturingEventBus : IEventBus
    {
        private readonly List<Order> _orders = new();

        public IReadOnlyCollection<Order> Orders => _orders;

        public Task PublishOrderCreatedAsync(Order order)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public void Reset() => _orders.Clear();
    }

    private sealed class HttpPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _client;

        public HttpPaymentGateway(HttpClient client)
        {
            _client = client;
        }

        public async Task<PaymentResult> ChargeAsync(Order order)
        {
            var payload = new
            {
                order.ProductId,
                order.CustomerEmail
            };

            try
            {
                var response = await _client.PostAsJsonAsync("/payments", payload);
                if (response.IsSuccessStatusCode)
                {
                    return PaymentResult.Ok();
                }

                var message = await response.Content.ReadAsStringAsync();
                return PaymentResult.Fail(string.IsNullOrWhiteSpace(message) ? "Payment rejected" : message);
            }
            catch (Exception ex)
            {
                return PaymentResult.Fail(ex.Message);
            }
        }
    }
}

