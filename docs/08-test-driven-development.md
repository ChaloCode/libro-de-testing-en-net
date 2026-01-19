# Capítulo VIII: Test-Driven Development (TDD)

## Descripción

Este capítulo presenta Test-Driven Development (TDD), una práctica donde las pruebas se escriben **antes** del código de producción. Se sigue el ciclo Red-Green-Refactor para desarrollar funcionalidad de forma incremental.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/tests/ChaloStore.UnitTests/`](../chalostore/tests/ChaloStore.UnitTests/) | Suite donde aplicar TDD |
| [`../chalostore/src/ChaloStore.Inventory/InventoryService.cs`](../chalostore/src/ChaloStore.Inventory/InventoryService.cs) | Ejemplo de código desarrollado con TDD |

> **Nota**: El documento `chalostore-overview.md` menciona un `CartService` para demostrar TDD, pero actualmente no está implementado. Los ejemplos usan `InventoryService` como referencia.

## El Ciclo Red-Green-Refactor

```
    ┌─────────┐
    │  RED    │ ← Escribir prueba que falla
    └────┬────┘
         │
         ▼
    ┌─────────┐
    │  GREEN  │ ← Escribir código mínimo para pasar
    └────┬────┘
         │
         ▼
    ┌──────────┐
    │ REFACTOR │ ← Mejorar código sin romper pruebas
    └────┬─────┘
         │
         └──────────► Repetir
```

## Ejemplo Práctico: Desarrollando AddProduct con TDD

### Paso 1: RED - Escribir la prueba primero

```csharp
// La prueba se escribe ANTES de que exista AddProduct
[Fact]
public void AddProduct_ValidData_ShouldPersist()
{
    // Arrange
    var repo = Substitute.For<IInventoryRepository>();
    var service = new InventoryService(repo);
    var dto = new ProductDto("SKU-001", "Laptop", 5, 1000m);

    // Act
    var result = service.AddProduct(dto);  // ❌ No compila aún

    // Assert
    result.Success.Should().BeTrue();
    repo.Received(1).Add(dto);
}
```

### Paso 2: GREEN - Implementación mínima

```csharp
// Implementación más simple que hace pasar la prueba
public ServiceResult AddProduct(ProductDto product)
{
    _repository.Add(product);
    return ServiceResult.Ok();
}
```

### Paso 3: RED - Agregar validación

```csharp
[Fact]
public void AddProduct_EmptyId_ShouldFail()
{
    var dto = new ProductDto("", "Laptop", 5, 1000m);
    
    var result = _service.AddProduct(dto);
    
    result.Success.Should().BeFalse();  // ❌ Falla
    result.Errors.Should().Contain("ID es obligatorio");
}
```

### Paso 4: GREEN - Agregar validación

```csharp
public ServiceResult AddProduct(ProductDto product)
{
    if (string.IsNullOrWhiteSpace(product.Id))
        return ServiceResult.Fail("ID es obligatorio");
    
    _repository.Add(product);
    return ServiceResult.Ok();
}
```

### Paso 5: REFACTOR - Mejorar estructura

```csharp
public ServiceResult AddProduct(ProductDto product)
{
    var errors = new List<string>();
    
    if (string.IsNullOrWhiteSpace(product.Id)) 
        errors.Add("ID es obligatorio");
    if (string.IsNullOrWhiteSpace(product.Name)) 
        errors.Add("Nombre es obligatorio");
    if (product.Quantity < 0) 
        errors.Add("Cantidad no puede ser negativa");
    if (product.Price < 0m) 
        errors.Add("Precio no puede ser negativo");

    if (errors.Count > 0)
        return ServiceResult.Fail(errors.ToArray());

    _repository.Add(product);
    return ServiceResult.Ok();
}
```

## Las Tres Leyes de TDD (Robert C. Martin)

1. **No escribas código de producción** hasta que tengas una prueba que falle
2. **No escribas más de una prueba** que falle a la vez
3. **No escribas más código de producción** del necesario para pasar la prueba

## Beneficios de TDD

| Beneficio | Descripción |
|-----------|-------------|
| **Diseño emergente** | El código se estructura naturalmente |
| **Documentación viva** | Las pruebas documentan el comportamiento |
| **Confianza en refactoring** | Las pruebas protegen contra regresiones |
| **Menos debugging** | Los errores se detectan inmediatamente |

## Cómo Ejecutar

```bash
cd ../chalostore

# Ejecutar pruebas continuamente (watch mode)
dotnet watch test --project tests/ChaloStore.UnitTests

# Ejecutar una prueba específica durante desarrollo
dotnet test tests/ChaloStore.UnitTests --filter "AddProduct_ValidData"
```

## Consideraciones Técnicas

### Cuándo Usar TDD

- Lógica de negocio nueva
- Corrección de bugs (escribir prueba que reproduzca el bug primero)
- APIs públicas
- Algoritmos complejos

### Cuándo NO Usar TDD

- Código exploratorio/prototipo
- UI (mejor E2E o pruebas visuales)
- Código de infraestructura simple (getters/setters)

### Test Doubles en TDD

```csharp
// Stub: Devuelve valores predefinidos
repo.FindById("SKU-001").Returns(new ProductDto(...));

// Mock: Verifica interacciones
repo.Received(1).Add(Arg.Any<ProductDto>());

// Fake: Implementación simplificada
var fakeRepo = new InMemoryInventoryRepository();
```

## Relación con el Libro

Este código corresponde al **Capítulo VIII: Test-Driven Development** del libro, donde se explica:

- Historia y principios de TDD
- El ciclo Red-Green-Refactor en detalle
- Las tres leyes de TDD
- Cuándo aplicar TDD y cuándo no
- TDD vs BDD: diferencias y complementos

## Ejercicios Sugeridos

1. Implementar `UpdateStock` siguiendo TDD estricto
2. Agregar validación de precio máximo con TDD
3. Desarrollar un `CartService` completo usando TDD
