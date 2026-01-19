using System.Net;
using ChaloStore.AcceptanceTests.Support;
using FluentAssertions;
using Reqnroll;

namespace ChaloStore.AcceptanceTests.Steps;

[Binding]
public sealed class CheckoutSteps
{
    private const string ApiDriverKey = "CheckoutApiDriver";
    private readonly ScenarioContext _scenarioContext;

    public CheckoutSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario(Order = 0)]
    public async Task BeforeScenario()
    {
        var driver = new CheckoutApiDriver();
        await driver.ResetAsync();
        _scenarioContext[ApiDriverKey] = driver;
    }

    [AfterScenario(Order = 100)]
    public async Task AfterScenario()
    {
        if (_scenarioContext.TryGetValue(ApiDriverKey, out var value) && value is CheckoutApiDriver driver)
        {
            await driver.DisposeAsync();
            _scenarioContext.Remove(ApiDriverKey);
        }
    }

    [Given("the catalog has product {int} with sku {string} and stock {int}")]
    public async Task GivenTheCatalogHasProduct(int productId, string sku, int stock)
    {
        await GetApiDriver().SeedProductAsync(productId, sku, $"{sku} name", stock);
    }

    [Given("the payment provider will reject requests")]
    public void GivenThePaymentProviderWillRejectRequests()
    {
        GetApiDriver().SimulatePaymentRejection();
    }

    [When("the customer places an order for product {int} with email {string}")]
    public async Task WhenTheCustomerPlacesAnOrder(int productId, string email)
    {
        await GetApiDriver().PlaceOrderAsync(productId, email);
    }

    [Then("the API should respond with status {int}")]
    public void ThenTheApiShouldRespondWithStatus(int expectedStatus)
    {
        GetApiDriver().ResponseStatus.Should().Be((HttpStatusCode)expectedStatus);
    }

    [Then("the response body should be {string}")]
    public void ThenTheResponseBodyShouldBe(string expectedBody)
    {
        var driver = GetApiDriver();
        driver.ResponseBody.Should().NotBeNull("the API should return a response body");
        var normalized = driver.ResponseBody!.Trim();
        normalized = normalized.Trim('"');
        normalized.Should().Be(expectedBody);
    }

    [Then("the stored order for product {int} should exist")]
    public async Task ThenTheStoredOrderShouldExist(int productId)
    {
        var driver = GetApiDriver();
        var order = await driver.FindOrderAsync(productId);
        order.Should().NotBeNull("the order should have been persisted");
        driver.ResponseOrder.Should().NotBeNull("the API should return the created order");
        driver.ResponseOrder!.ProductId.Should().Be(productId);
    }

    [Then("the product {int} should have stock {int}")]
    public async Task ThenTheProductShouldHaveStock(int productId, int expectedStock)
    {
        var product = await GetApiDriver().FindProductAsync(productId);
        product.Should().NotBeNull($"product {productId} should exist");
        product!.Stock.Should().Be(expectedStock);
    }

    [Then("an order confirmation email should be sent to {string}")]
    public void ThenAnOrderConfirmationEmailShouldBeSentTo(string expectedEmail)
    {
        GetApiDriver().EmailMessages.Should().ContainSingle(
            message => message.Email == expectedEmail,
            "the confirmation email should be sent exactly once");
    }

    [Then("an order created event should be published for product {int}")]
    public void ThenAnOrderCreatedEventShouldBePublished(int productId)
    {
        GetApiDriver().PublishedEvents.Should().ContainSingle(
            order => order.ProductId == productId,
            "the event bus should receive a notification for the created order");
    }

    private CheckoutApiDriver GetApiDriver()
    {
        if (_scenarioContext.TryGetValue(ApiDriverKey, out var value) && value is CheckoutApiDriver driver)
        {
            return driver;
        }

        throw new InvalidOperationException("Checkout API driver is not available in the current scenario context.");
    }
}
