using ChaloStore.AcceptanceTests.Support;
using FluentAssertions;
using Reqnroll;

namespace ChaloStore.AcceptanceTests.Steps;

[Binding]
[Scope(Tag = "ui")]
public sealed class CheckoutUiSteps
{
    private const string UiDriverKey = "CheckoutUiDriver";
    private readonly ScenarioContext _scenarioContext;

    public CheckoutUiSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeUiScenario()
    {
        var driver = new CheckoutUiDriver(GetApiDriver());
        await driver.InitializeAsync();
        _scenarioContext[UiDriverKey] = driver;
    }

    [AfterScenario(Order = 0)]
    public async Task AfterUiScenario()
    {
        if (_scenarioContext.TryGetValue(UiDriverKey, out var value) && value is CheckoutUiDriver driver)
        {
            await driver.DisposeAsync();
            _scenarioContext.Remove(UiDriverKey);
        }
    }

    [When("the shopper completes the checkout form for product {int} with email {string}")]
    public async Task WhenTheShopperCompletesTheCheckoutForm(int productId, string email)
    {
        await GetUiDriver().CompleteCheckoutAsync(productId, email);
    }

    [Then("the UI should show {string}")]
    public async Task ThenTheUiShouldShow(string expectedText)
    {
        var result = await GetUiDriver().GetResultAsync();
        result.Should().Contain(expectedText);
    }

    private CheckoutApiDriver GetApiDriver()
    {
        if (_scenarioContext.TryGetValue("CheckoutApiDriver", out var value) && value is CheckoutApiDriver driver)
        {
            return driver;
        }

        throw new InvalidOperationException("API driver not available for UI scenario.");
    }

    private CheckoutUiDriver GetUiDriver()
    {
        if (_scenarioContext.TryGetValue(UiDriverKey, out var value) && value is CheckoutUiDriver driver)
        {
            return driver;
        }

        throw new InvalidOperationException("UI driver not initialized for this scenario.");
    }
}

