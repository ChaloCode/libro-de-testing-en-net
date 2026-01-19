using System.Net.Http.Json;
using ChaloStore.Orders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChaloStore.IntegrationTests;

public sealed class OrderEndpointTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        await SeedDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateOrder_ShouldDecreaseStockAndPersistOrder()
    {
        var order = new Order { ProductId = 1, CustomerEmail = "customer@test.com" };

        var response = await _client.PostAsJsonAsync("/orders", order);

        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        var product = await db.Products.FindAsync(1);
        var storedOrder = await db.Orders.FirstOrDefaultAsync(o => o.CustomerEmail == "customer@test.com");

        product!.Stock.Should().Be(99);
        storedOrder.Should().NotBeNull();
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        if (!await db.Products.AnyAsync())
        {
            db.Products.Add(new Product { Id = 1, Sku = "SKU-001", Name = "Laptop", Stock = 100 });
            await db.SaveChangesAsync();
        }
    }
}
