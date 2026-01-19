# Capítulo IX: Behavior-Driven Development (BDD)

## Descripción

Este capítulo introduce Behavior-Driven Development (BDD), una práctica que extiende TDD enfocándose en el comportamiento del sistema desde la perspectiva del usuario. Usamos Reqnroll (fork open-source de SpecFlow) para escribir escenarios en lenguaje Gherkin.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Features/Checkout.feature`](../chalostore/tests/ChaloStore.AcceptanceTests/Features/Checkout.feature) | Escenarios Gherkin del checkout |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutSteps.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Steps/CheckoutSteps.cs) | Step definitions |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutApiDriver.cs`](../chalostore/tests/ChaloStore.AcceptanceTests/Support/CheckoutApiDriver.cs) | Driver de pruebas |

## Lenguaje Gherkin

### Estructura Básica

```gherkin
Feature: [Nombre de la funcionalidad]
  Como [rol de usuario]
  Quiero [acción]
  Para [beneficio]

  Scenario: [Nombre del escenario]
    Given [contexto inicial]
    When [acción del usuario]
    Then [resultado esperado]
```

### Ejemplo Real de ChaloStore

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
    And an email should be sent to "test@example.com"

  Scenario: Rechazar checkout sin stock
    Given the catalog has product 2 with sku "SKU-002" and stock 0
    When the shopper places an order for product 2 with email "test@example.com"
    Then the order should be rejected with message "Product not available"
```

## Step Definitions con Reqnroll

### Binding de Steps

```csharp
[Binding]
public sealed class CheckoutSteps
{
    private readonly CheckoutApiDriver _driver;
    private readonly ScenarioContext _context;

    public CheckoutSteps(CheckoutApiDriver driver, ScenarioContext context)
    {
        _driver = driver;
        _context = context;
    }

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

    [Then("the order should be persisted")]
    public async Task ThenTheOrderShouldBePersisted()
    {
        var productId = _context.Get<int>("lastProductId");
        var order = await _driver.FindOrderAsync(productId);
        order.Should().NotBeNull();
    }
}
```

### Compartir Datos entre Steps

```csharp
// Usando ScenarioContext
_context["lastProductId"] = productId;
var id = _context.Get<int>("lastProductId");

// O con dependencias inyectadas
public class CheckoutContext
{
    public int LastProductId { get; set; }
    public Order? LastOrder { get; set; }
}
```

## Hooks de Ciclo de Vida

```csharp
[Binding]
public sealed class CheckoutHooks
{
    [BeforeScenario]
    public async Task BeforeScenario(CheckoutApiDriver driver)
    {
        await driver.ResetAsync();  // Limpiar estado
    }

    [AfterScenario]
    public async Task AfterScenario(CheckoutApiDriver driver)
    {
        await driver.DisposeAsync();
    }

    [BeforeFeature]
    public static void BeforeFeature()
    {
        // Setup a nivel de feature
    }
}
```

## Tags para Organización

```gherkin
@smoke @checkout
Feature: Checkout API

  @happy-path
  Scenario: Confirmar checkout con stock disponible
    ...

  @edge-case @wip
  Scenario: Checkout con cantidad exacta de stock
    ...
```

```bash
# Ejecutar solo tests con tag específico
dotnet test --filter "Category=smoke"

# Excluir tests en progreso
dotnet test --filter "Category!=wip"
```

## Scenario Outline (Datos Parametrizados)

```gherkin
Scenario Outline: Validar emails en checkout
  Given the catalog has product 1 with sku "SKU-001" and stock 10
  When the shopper places an order for product 1 with email "<email>"
  Then the response should be "<result>"

  Examples:
    | email              | result  |
    | valid@example.com  | success |
    | invalid-email      | error   |
    | @no-user.com       | error   |
```

## Cómo Ejecutar

```bash
cd ../chalostore

# Ejecutar todas las pruebas BDD
dotnet test tests/ChaloStore.AcceptanceTests

# Solo escenarios con tag específico
dotnet test tests/ChaloStore.AcceptanceTests --filter "Category=checkout"

# Generar reporte de living documentation
dotnet reqnroll livingdoc test-assembly tests/ChaloStore.AcceptanceTests/bin/Debug/net10.0/ChaloStore.AcceptanceTests.dll
```

## Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| Reqnroll | 3.3.2 | Framework BDD |
| Reqnroll.xUnit | 3.3.2 | Integración con xUnit |

## BDD vs TDD

| Aspecto | TDD | BDD |
|---------|-----|-----|
| **Enfoque** | Código/unidad | Comportamiento/usuario |
| **Lenguaje** | Código (C#) | Natural (Gherkin) |
| **Audiencia** | Desarrolladores | Equipo completo |
| **Nivel** | Unitario | Aceptación |

## Relación con el Libro

Este código corresponde al **Capítulo IX: Behavior-Driven Development** del libro, donde se explica:

- Origen y principios de BDD
- Sintaxis Gherkin y buenas prácticas
- Reqnroll vs SpecFlow
- Escribir escenarios efectivos
- Integración con Playwright para UI
- Living documentation

## Ejercicios Sugeridos

1. Agregar escenario de devolución de pedido
2. Crear feature para gestión de carrito
3. Implementar escenario con múltiples productos
