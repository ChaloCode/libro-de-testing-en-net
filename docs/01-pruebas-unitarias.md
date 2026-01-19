# Capítulo I: Pruebas Unitarias

## Descripción

Este capítulo introduce las pruebas unitarias en .NET, mostrando cómo aislar componentes individuales para verificar su comportamiento de forma independiente. Se utiliza el servicio de inventario de ChaloStore como caso de estudio.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/src/ChaloStore.Inventory/InventoryService.cs`](../chalostore/src/ChaloStore.Inventory/InventoryService.cs) | Servicio de dominio con lógica de validación de productos |
| [`../chalostore/tests/ChaloStore.UnitTests/InventoryServiceTests.cs`](../chalostore/tests/ChaloStore.UnitTests/InventoryServiceTests.cs) | Suite de pruebas unitarias del servicio |
| [`../chalostore/tests/ChaloStore.UnitTests/ChaloStore.UnitTests.csproj`](../chalostore/tests/ChaloStore.UnitTests/ChaloStore.UnitTests.csproj) | Configuración del proyecto de pruebas |

## Conceptos Demostrados

### 1. Estructura AAA (Arrange-Act-Assert)

```csharp
[Fact]
public void AddProduct_ValidData_ShouldPersist()
{
    // Arrange
    var dto = new ProductDto("SKU-001", "Laptop", 5, 1000m);

    // Act
    var result = _service.AddProduct(dto);

    // Assert
    result.Success.Should().BeTrue();
    _repo.Received(1).Add(dto);
}
```

### 2. Mocking con NSubstitute

```csharp
private readonly IInventoryRepository _repo = Substitute.For<IInventoryRepository>();
```

### 3. Assertions con FluentAssertions

```csharp
result.Success.Should().BeFalse();
result.Errors.Should().Contain("ID es obligatorio");
```

### 4. Pruebas Parametrizadas con Theory

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
public void AddProduct_InvalidId_ShouldFail(string? id)
{
    // ...
}
```

## Cómo Ejecutar

```bash
cd ../chalostore

# Ejecutar todas las pruebas unitarias
dotnet test tests/ChaloStore.UnitTests

# Con salida detallada
dotnet test tests/ChaloStore.UnitTests --logger "console;verbosity=detailed"

# Filtrar por nombre de prueba
dotnet test tests/ChaloStore.UnitTests --filter "AddProduct"
```

## Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| xunit | 2.9.3 | Framework de pruebas |
| xunit.runner.visualstudio | 3.0.2 | Runner para VS/CLI |
| FluentAssertions | 8.8.0 | Assertions expresivas |
| NSubstitute | 5.3.0 | Mocking/stubbing |
| Microsoft.TestPlatform.TestHost | 18.0.1 | Infraestructura de pruebas |

## Consideraciones Técnicas

- **Aislamiento**: Las pruebas unitarias no deben depender de bases de datos, APIs externas ni el sistema de archivos.
- **Velocidad**: Deben ejecutarse en milisegundos.
- **Determinismo**: El mismo input siempre produce el mismo resultado.
- **Mensajes en español**: Los mensajes de error del `InventoryService` están en español (ej: "ID es obligatorio").

## Relación con el Libro

Este código corresponde al **Capítulo I: Pruebas Unitarias** del libro, donde se explica:

- Qué es una prueba unitaria y por qué es importante
- El patrón AAA (Arrange-Act-Assert)
- Frameworks de testing en .NET (xUnit vs NUnit vs MSTest)
- Mocking y stubbing de dependencias
- Buenas prácticas de naming y organización

## Ejercicios Sugeridos

1. Agregar validación de precio máximo en `InventoryService`
2. Crear pruebas para el método `UpdateStock`
3. Implementar pruebas de borde (edge cases) para cantidades límite
