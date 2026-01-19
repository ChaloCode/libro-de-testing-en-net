using System.Net;
using System.Text.Json;
using Microsoft.Playwright;

namespace ChaloStore.AcceptanceTests.Support;

internal sealed class CheckoutUiDriver : IAsyncDisposable
{
    private readonly CheckoutApiDriver _apiDriver;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public CheckoutUiDriver(CheckoutApiDriver apiDriver)
    {
        _apiDriver = apiDriver;
    }

    public async Task InitializeAsync()
    {
        await PlaywrightInstaller.EnsureInstalledAsync();
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
        await _page.RouteAsync("**/orders", HandleCheckoutRouteAsync);
    }

    public async Task CompleteCheckoutAsync(int productId, string email)
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Playwright page has not been initialized.");
        }

        var html = await _apiDriver.GetCheckoutPageAsync();
        await _page.SetContentAsync(html);
        await _page.FillAsync("#product-id", productId.ToString());
        await _page.FillAsync("#customer-email", email);
        await _page.ClickAsync("#submit-button");
        
        // Esperar a que el resultado cambie de "Procesando..." a algo diferente
        await _page.WaitForFunctionAsync(
            "() => { const el = document.getElementById('result'); return el && el.textContent.trim() !== '' && el.textContent.trim() !== 'Procesando...'; }",
            new PageWaitForFunctionOptions { Timeout = 10000 });
    }

    public async Task<string> GetResultAsync()
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Playwright page has not been initialized.");
        }

        return (await _page.InnerTextAsync("#result")).Trim();
    }

    public async ValueTask DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    private async Task HandleCheckoutRouteAsync(IRoute route)
    {
        var request = route.Request;
        var body = request.PostData ?? "{}";
        var payload = JsonDocument.Parse(body);
        var productId = payload.RootElement.TryGetProperty("productId", out var productElement)
            ? productElement.GetInt32()
            : 0;
        var email = payload.RootElement.TryGetProperty("customerEmail", out var emailElement)
            ? emailElement.GetString() ?? string.Empty
            : string.Empty;

        await _apiDriver.PlaceOrderAsync(productId, email);

        var status = _apiDriver.ResponseStatus ?? HttpStatusCode.InternalServerError;
        if (_apiDriver.ResponseOrder is not null)
        {
            var json = JsonSerializer.Serialize(_apiDriver.ResponseOrder);
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = (int)status,
                Body = json,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                }
            });
            return;
        }

        var message = _apiDriver.ResponseBody ?? "Payment rejected";
        await route.FulfillAsync(new RouteFulfillOptions
        {
            Status = (int)status,
            Body = message,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "text/plain"
            }
        });
    }
}

