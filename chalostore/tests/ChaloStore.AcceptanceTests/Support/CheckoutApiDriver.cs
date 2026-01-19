using System.Net;
using System.Net.Http.Json;
using ChaloStore.Orders;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ChaloStore.AcceptanceTests.Support;

internal sealed class CheckoutApiDriver : IAsyncDisposable
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();
    private readonly ChaloStoreApplication _application;
    private readonly HttpClient _client;
    private HttpResponseMessage? _response;
    private string? _responseBody;

    public CheckoutApiDriver()
    {
        _application = new ChaloStoreApplication(new Uri(_wireMockServer.Urls[0]));
        _client = _application.CreateClient();
    }

    public HttpStatusCode? ResponseStatus => _response?.StatusCode;
    public string? ResponseBody => _responseBody;
    public Order? ResponseOrder { get; private set; }
    public IReadOnlyCollection<(string Email, Order Order)> EmailMessages => _application.EmailService.Messages;
    public IReadOnlyCollection<Order> PublishedEvents => _application.EventBus.Orders;
    public Uri BaseAddress => _client.BaseAddress!;

    public async Task ResetAsync()
    {
        await _application.ResetStateAsync();
        ResetPaymentSuccess();
        ResponseOrder = null;
        if (_response != null)
        {
            _response.Dispose();
            _response = null;
        }
        _responseBody = null;
    }

    private void ResetPaymentSuccess()
    {
        _wireMockServer.Reset();
        _wireMockServer
            .Given(Request.Create().WithPath("/payments").UsingPost())
            .AtPriority(10)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"status\":\"accepted\"}"));
    }

    public void SimulatePaymentRejection()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/payments").UsingPost())
            .AtPriority(0)
            .RespondWith(Response.Create()
                .WithStatusCode(502)
                .WithBody("Payment rejected"));
    }

    public async Task SeedProductAsync(int id, string sku, string name, int stock)
    {
        await _application.SeedProductAsync(new Product
        {
            Id = id,
            Sku = sku,
            Name = name,
            Stock = stock
        });
    }

    public async Task PlaceOrderAsync(int productId, string customerEmail)
    {
        var order = new Order
        {
            ProductId = productId,
            CustomerEmail = customerEmail
        };

        _response = await _client.PostAsJsonAsync("/orders", order);
        _responseBody = await _response.Content.ReadAsStringAsync();

        if (_response.IsSuccessStatusCode)
        {
            ResponseOrder = await _response.Content.ReadFromJsonAsync<Order>();
        }
        else
        {
            ResponseOrder = null;
        }
    }

    public async Task<Order?> FindOrderAsync(int productId) =>
        await _application.GetOrderByProductAsync(productId);

    public async Task<Product?> FindProductAsync(int productId) =>
        await _application.GetProductAsync(productId);

    public async Task<string> GetCheckoutPageAsync() =>
        await _client.GetStringAsync("/checkout");

    public ValueTask DisposeAsync()
    {
        _response?.Dispose();
        _client.Dispose();
        _application.Dispose();
        _wireMockServer.Dispose();
        return ValueTask.CompletedTask;
    }
}

