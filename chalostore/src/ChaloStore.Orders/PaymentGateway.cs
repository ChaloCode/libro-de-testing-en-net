namespace ChaloStore.Orders;

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(Order order);
}

public sealed record PaymentResult(bool Success, string? ErrorMessage = null)
{
    public static PaymentResult Ok() => new(true, null);
    public static PaymentResult Fail(string message) => new(false, message);
}

public sealed class FakePaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(Order order) => Task.FromResult(PaymentResult.Ok());
}

