# Capítulo II: Pruebas de Integración

## Descripción

Este capítulo cubre las pruebas de integración en ASP.NET Core, donde múltiples componentes trabajan juntos. Se utiliza `WebApplicationFactory` para levantar la aplicación en memoria y probar endpoints HTTP reales.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/src/ChaloStore.Web/Program.cs`](../chalostore/src/ChaloStore.Web/Program.cs) | API mínima con endpoint `/orders` |
| [`../chalostore/src/ChaloStore.Orders/ChaloStoreDbContext.cs`](../chalostore/src/ChaloStore.Orders/ChaloStoreDbContext.cs) | DbContext con Entity Framework Core |
| [`../chalostore/src/ChaloStore.Orders/Models.cs`](../chalostore/src/ChaloStore.Orders/Models.cs) | Entidades Product y Order |
| [`../chalostore/tests/ChaloStore.IntegrationTests/OrderEndpointTests.cs`](../chalostore/tests/ChaloStore.IntegrationTests/OrderEndpointTests.cs) | Suite de pruebas de integración |

## Conceptos Demostrados

### 1. WebApplicationFactory

```csharp
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
}
```

### 2. Seed de Datos para Pruebas

```csharp
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
```

### 3. Prueba de Endpoint HTTP

```csharp
[Fact]
public async Task CreateOrder_ShouldDecreaseStockAndPersistOrder()
{
    var order = new Order { ProductId = 1, CustomerEmail = "customer@test.com" };

    var response = await _client.PostAsJsonAsync("/orders", order);

    response.EnsureSuccessStatusCode();
    
    // Verificar efectos secundarios en la base de datos
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ChaloStoreDbContext>();
    var product = await db.Products.FindAsync(1);
    
    product!.Stock.Should().Be(99);
}
```

## Cómo Ejecutar

```bash
cd ../chalostore

# Ejecutar pruebas de integración
dotnet test tests/ChaloStore.IntegrationTests

# Con logs detallados
dotnet test tests/ChaloStore.IntegrationTests --logger "console;verbosity=detailed"
```

## Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| Microsoft.AspNetCore.Mvc.Testing | 10.0.2 | WebApplicationFactory para testing |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.2 | Base de datos en memoria |
| FluentAssertions | 8.8.0 | Assertions expresivas |
| xunit | 2.9.3 | Framework de pruebas |

## Consideraciones Técnicas

### Base de Datos InMemory vs Real

- **InMemory**: Rápido, sin dependencias externas, ideal para CI.
- **SQLite/Postgres con Testcontainers**: Más realista, detecta problemas de SQL real.

### Aislamiento entre Pruebas

Cada prueba debe:
1. Limpiar la base de datos (`EnsureDeletedAsync`)
2. Crear estructura (`EnsureCreatedAsync`)
3. Insertar datos de prueba (seed)

### IAsyncLifetime

Implementar `IAsyncLifetime` permite setup/teardown asíncronos:
- `InitializeAsync()`: Se ejecuta antes de cada prueba
- `DisposeAsync()`: Se ejecuta después de cada prueba

## Relación con el Libro

Este código corresponde al **Capítulo II: Pruebas de Integración** del libro, donde se explica:

- Diferencia entre pruebas unitarias e integración
- Configuración de `WebApplicationFactory`
- Estrategias de base de datos para testing
- Testcontainers para bases de datos reales
- Pruebas de APIs REST

## Ejercicios Sugeridos

1. Agregar prueba para validar respuesta 400 cuando el producto no existe
2. Implementar prueba de concurrencia (dos órdenes simultáneas)
3. Configurar Testcontainers con PostgreSQL
