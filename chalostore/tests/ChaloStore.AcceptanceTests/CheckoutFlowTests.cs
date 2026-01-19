using System.Net;
using ChaloStore.AcceptanceTests.Support;
using FluentAssertions;
using Xunit;

namespace ChaloStore.AcceptanceTests;

public sealed class CheckoutFlowTests : IAsyncLifetime
{
    private readonly CheckoutApiDriver _driver = new();

    public async Task InitializeAsync()
    {
        await _driver.ResetAsync();
        await _driver.SeedProductAsync(1, "SKU-001", "Laptop", stock: 5);
    }

    public async Task DisposeAsync()
    {
        await _driver.DisposeAsync();
    }

    [Fact]
    public async Task ConfirmCheckout_ShouldPersistOrderAndNotifyDependencies()
    {
        await _driver.PlaceOrderAsync(1, "customer@test.com");

        _driver.ResponseStatus.Should().Be(HttpStatusCode.OK);
        var storedOrder = await _driver.FindOrderAsync(1);
        storedOrder.Should().NotBeNull();
        storedOrder!.CustomerEmail.Should().Be("customer@test.com");

        var product = await _driver.FindProductAsync(1);
        product.Should().NotBeNull();
        product!.Stock.Should().Be(4);

        _driver.EmailMessages.Should().ContainSingle(m => m.Email == "customer@test.com");
        _driver.PublishedEvents.Should().ContainSingle(o => o.ProductId == 1);
    }
}
