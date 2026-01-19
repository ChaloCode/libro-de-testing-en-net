# Capítulo VII: Contract Testing

## Descripción

Este capítulo introduce el contract testing, una técnica para verificar que los servicios consumidores y proveedores cumplen con un contrato acordado. Usamos PactNet para generar y verificar contratos, y WireMock.Net para simular proveedores externos.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/contracts/`](../chalostore/contracts/) | Carpeta para almacenar contratos Pact |
| [`../chalostore/contracts/payments/`](../chalostore/contracts/payments/) | Contratos con pasarela de pagos |
| [`../chalostore/contracts/shipping/`](../chalostore/contracts/shipping/) | Contratos con proveedor de envíos |
| [`../chalostore/src/ChaloStore.Orders/PaymentGateway.cs`](../chalostore/src/ChaloStore.Orders/PaymentGateway.cs) | Cliente de pagos (consumidor) |

> **Nota**: Los archivos de contratos (pacts) están pendientes de crear. Esta documentación describe la estructura esperada.

## ¿Qué es Contract Testing?

El contract testing verifica la comunicación entre servicios:

```
┌─────────────┐         Contrato         ┌─────────────┐
│  Consumidor │ ◄──────────────────────► │  Proveedor  │
│  (ChaloStore)│                          │  (Payments) │
└─────────────┘                          └─────────────┘
```

- **Consumidor**: Define qué espera del proveedor
- **Proveedor**: Verifica que cumple el contrato
- **Contrato (Pact)**: Documento JSON con las expectativas

## Instalación

```bash
# Agregar PactNet al proyecto de pruebas
cd ../chalostore
dotnet add tests/ChaloStore.IntegrationTests package PactNet --version 5.0.0
```

## Ejemplo de Prueba del Consumidor

```csharp
public class PaymentContractTests
{
    private readonly IPactBuilderV4 _pact;

    public PaymentContractTests()
    {
        var config = new PactConfig
        {
            PactDir = "../../../contracts/payments",
            LogLevel = PactLogLevel.Information
        };

        _pact = Pact.V4("ChaloStore", "PaymentGateway", config)
            .WithHttpInteractions();
    }

    [Fact]
    public async Task ChargeCard_ValidPayment_ReturnsSuccess()
    {
        // Arrange: Definir la interacción esperada
        _pact
            .UponReceiving("a valid payment request")
            .Given("the card is valid")
            .WithRequest(HttpMethod.Post, "/api/charges")
            .WithJsonBody(new
            {
                amount = 100.00,
                currency = "USD",
                cardToken = "tok_valid"
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithJsonBody(new
            {
                chargeId = Match.Type("ch_123"),
                status = "succeeded"
            });

        // Act & Assert
        await _pact.VerifyAsync(async ctx =>
        {
            var client = new PaymentClient(ctx.MockServerUri);
            var result = await client.ChargeAsync(100.00m, "USD", "tok_valid");
            
            result.Status.Should().Be("succeeded");
        });
    }
}
```

## Ejemplo de Verificación del Proveedor

```csharp
public class PaymentProviderTests
{
    [Fact]
    public void VerifyPactWithChaloStore()
    {
        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        var verifier = new PactVerifier("PaymentGateway", config);

        verifier
            .WithHttpEndpoint(new Uri("http://localhost:5000"))
            .WithPactBrokerSource(new Uri("https://pact-broker.example.com"))
            .WithProviderStateUrl(new Uri("http://localhost:5000/provider-states"))
            .Verify();
    }
}
```

## Estructura de un Pact (JSON)

```json
{
  "consumer": { "name": "ChaloStore" },
  "provider": { "name": "PaymentGateway" },
  "interactions": [
    {
      "description": "a valid payment request",
      "providerState": "the card is valid",
      "request": {
        "method": "POST",
        "path": "/api/charges",
        "body": {
          "amount": 100.00,
          "currency": "USD",
          "cardToken": "tok_valid"
        }
      },
      "response": {
        "status": 200,
        "body": {
          "chargeId": "ch_123",
          "status": "succeeded"
        }
      }
    }
  ]
}
```

## WireMock.Net para Mocking

Mientras no tengas acceso al proveedor real, usa WireMock:

```csharp
public class PaymentMockTests
{
    [Fact]
    public async Task ChargeCard_WithMockedProvider()
    {
        // Arrange
        var server = WireMockServer.Start();
        
        server
            .Given(Request.Create()
                .WithPath("/api/charges")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { chargeId = "ch_mock", status = "succeeded" }));

        // Act
        var client = new HttpClient { BaseAddress = new Uri(server.Url) };
        var response = await client.PostAsJsonAsync("/api/charges", new { amount = 100 });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        server.Stop();
    }
}
```

## Flujo de Contract Testing

```
1. Consumidor escribe prueba → Genera Pact JSON
2. Pact se publica en Broker (o carpeta compartida)
3. Proveedor descarga Pact
4. Proveedor verifica contra su implementación
5. ✅ Contrato cumplido → Deploy seguro
```

## Consideraciones Técnicas

### Consumer-Driven Contracts

- El **consumidor** define el contrato
- El **proveedor** se adapta o negocia cambios
- Evita breaking changes accidentales

### Pact Broker

Para equipos distribuidos, usar Pact Broker:

```bash
# Publicar pact
pact-broker publish ./contracts --broker-base-url https://broker.example.com

# Verificar con can-i-deploy
pact-broker can-i-deploy --pacticipant ChaloStore --version 1.0.0
```

## Relación con el Libro

Este código corresponde al **Capítulo VII: Contract Testing** del libro, donde se explica:

- Diferencia entre contract testing e integration testing
- Consumer-Driven Contracts (CDC)
- PactNet: consumidor y proveedor
- Pact Broker para CI/CD
- WireMock.Net como complemento

## Ejercicios Sugeridos

1. Crear contrato para endpoint de pagos
2. Crear contrato para endpoint de envíos
3. Configurar verificación en GitHub Actions
