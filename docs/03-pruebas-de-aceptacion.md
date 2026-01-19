# Capítulo III: Pruebas de Aceptación

## Descripción

Este capítulo introduce las pruebas de aceptación (end-to-end), que verifican flujos completos desde la perspectiva del usuario. Se utiliza Reqnroll para BDD con escenarios Gherkin y Playwright para automatización de UI.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/tests/ChaloStore.AcceptanceTests/`](../chalostore/tests/ChaloStore.AcceptanceTests/) | Proyecto de pruebas de aceptación |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Features/Checkout.feature`](../chalostore/tests/ChaloStore.AcceptanceTests/Features/Checkout.feature) | Escenarios Gherkin del checkout |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutSteps.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutSteps.cs) | Step definitions para API |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutUiSteps.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutUiSteps.cs) | Step definitions para UI |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutApiDriver.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutApiDriver.cs) | Driver para interacción con API |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutUiDriver.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutUiDriver.cs) | Driver para automatización UI con Playwright |

## Conceptos Demostrados

### 1. Escenarios Gherkin

```gherkin
Feature: Checkout API
  Como cliente de ChaloStore
  Quiero completar el proceso de checkout
  Para recibir mi pedido

  Scenario: Confirmar checkout con stock disponible
    Given the catalog has product 1 with sku "SKU-001" and stock 5
    When the shopper places an order for product 1 with email "test@example.com"
    Then the order should be persisted
    And the stock should decrease to 4
```

### 2. Step Definitions con Reqnroll

```csharp
[Binding]
public sealed class CheckoutSteps
{
    private readonly CheckoutApiDriver _driver;

    [Given("the catalog has product {int} with sku {string} and stock {int}")]
    public async Task GivenTheCatalogHasProduct(int id, string sku, int stock)
    {
        await _driver.SeedProductAsync(id, sku, "Product", stock);
    }

    [When("the shopper places an order for product {int} with email {string}")]
    public async Task WhenTheShopperPlacesAnOrder(int productId, string email)
    {
        await _driver.PlaceOrderAsync(productId, email);
    }
}
```

### 3. Automatización UI con Playwright

```csharp
public async Task CompleteCheckoutAsync(int productId, string email)
{
    await _page.FillAsync("#product-id", productId.ToString());
    await _page.FillAsync("#customer-email", email);
    await _page.ClickAsync("#submit-button");
    
    // Esperar a que el resultado cambie
    await _page.WaitForFunctionAsync(
        "() => document.getElementById('result').textContent !== 'Procesando...'");
}
```

## Cómo Ejecutar

```bash
cd ../chalostore

# Instalar navegadores de Playwright (solo la primera vez)
pwsh bin/Debug/net10.0/playwright.ps1 install

# Ejecutar todas las pruebas de aceptación
dotnet test tests/ChaloStore.AcceptanceTests

# Solo pruebas de API (sin UI)
dotnet test tests/ChaloStore.AcceptanceTests --filter "Category!=ui"

# Solo pruebas de UI
dotnet test tests/ChaloStore.AcceptanceTests --filter "Category=ui"
```

## Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| Reqnroll | 3.3.2 | Framework BDD para .NET |
| Reqnroll.xUnit | 3.3.2 | Integración con xUnit |
| Microsoft.Playwright | 1.57.0 | Automatización de navegadores |
| WireMock.Net | 1.18.0 | Mock de servicios externos |
| FluentAssertions | 8.8.0 | Assertions expresivas |

## Estructura del Proyecto

```
ChaloStore.AcceptanceTests/
├── Features/
│   └── Checkout.feature       # Escenarios Gherkin
├── Steps/
│   ├── CheckoutSteps.cs       # Steps para API
│   └── CheckoutUiSteps.cs     # Steps para UI
└── Support/
    ├── CheckoutApiDriver.cs   # Interacción con API
    ├── CheckoutUiDriver.cs    # Interacción con UI
    └── PlaywrightInstaller.cs # Instalación de navegadores
```

## Consideraciones Técnicas

### Tags en Gherkin

```gherkin
@ui
Scenario: Completing checkout from the web UI
```

Los tags permiten filtrar escenarios y aplicar configuraciones específicas.

### Drivers Pattern

Los drivers encapsulan la interacción con el sistema:
- **CheckoutApiDriver**: HTTP requests al backend
- **CheckoutUiDriver**: Automatización con Playwright

### Headless vs Headed

Por defecto, Playwright corre en modo headless (sin ventana visible):

```csharp
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true  // Cambiar a false para depuración visual
});
```

## Relación con el Libro

Este código corresponde al **Capítulo III: Pruebas de Aceptación** del libro, donde se explica:

- Diferencia entre pruebas de integración y aceptación
- Introducción a BDD y Gherkin
- Reqnroll como alternativa open-source a SpecFlow
- Automatización de UI con Playwright
- Estrategias de espera y sincronización

## Ejercicios Sugeridos

1. Agregar escenario para checkout con carrito vacío
2. Implementar escenario de devolución de pedido
3. Crear prueba de UI para validación de formulario
