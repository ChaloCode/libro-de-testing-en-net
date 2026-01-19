# Capítulo VI: Mutation Testing

## Descripción

Este capítulo introduce el mutation testing, una técnica que evalúa la calidad de las pruebas introduciendo cambios (mutantes) en el código y verificando si las pruebas los detectan. Usamos Stryker.NET como herramienta principal.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/src/ChaloStore.Inventory/InventoryService.cs`](../chalostore/src/ChaloStore.Inventory/InventoryService.cs) | Código fuente a mutar |
| [`../chalostore/tests/ChaloStore.UnitTests/`](../chalostore/tests/ChaloStore.UnitTests/) | Pruebas que deben detectar mutantes |

> **Nota**: El documento `chalostore-overview.md` menciona un módulo `ChaloStore.Pricing` para este capítulo, pero actualmente no está implementado. Los ejemplos usan `InventoryService` como alternativa.

## ¿Qué es Mutation Testing?

El mutation testing introduce cambios pequeños en el código (mutantes) y ejecuta las pruebas:

- **Mutante muerto**: La prueba falla → La prueba es efectiva
- **Mutante vivo**: La prueba pasa → La prueba es débil o falta cobertura

### Ejemplos de Mutaciones

| Tipo | Original | Mutante |
|------|----------|---------|
| Operador aritmético | `a + b` | `a - b` |
| Operador relacional | `a > b` | `a >= b` |
| Negación | `if (x)` | `if (!x)` |
| Valor de retorno | `return true` | `return false` |
| Eliminación | `x = x + 1` | `;` (línea vacía) |

## Instalación de Stryker.NET

```bash
# Instalar como herramienta global
dotnet tool install -g dotnet-stryker

# O como herramienta local del proyecto
dotnet new tool-manifest
dotnet tool install dotnet-stryker
```

## Cómo Ejecutar

```bash
cd ../chalostore

# Ejecutar mutation testing en el proyecto de inventario
dotnet stryker \
  --project "src/ChaloStore.Inventory/ChaloStore.Inventory.csproj" \
  --test-project "tests/ChaloStore.UnitTests/ChaloStore.UnitTests.csproj"

# Con configuración personalizada
dotnet stryker -c stryker-config.json
```

## Configuración (stryker-config.json)

```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/stryker-config.schema.json",
  "stryker-config": {
    "project": "src/ChaloStore.Inventory/ChaloStore.Inventory.csproj",
    "test-project": "tests/ChaloStore.UnitTests/ChaloStore.UnitTests.csproj",
    "reporters": ["html", "progress"],
    "thresholds": {
      "high": 80,
      "low": 60,
      "break": 50
    },
    "mutate": [
      "src/**/*.cs",
      "!src/**/*Generated*.cs"
    ]
  }
}
```

## Interpretación del Reporte

### Mutation Score

```
Mutation score: 75.00%
- Killed: 15
- Survived: 5
- Timeout: 0
- No coverage: 0
```

| Métrica | Significado |
|---------|-------------|
| **Killed** | Mutantes detectados por pruebas (bueno) |
| **Survived** | Mutantes no detectados (pruebas débiles) |
| **Timeout** | Mutantes que causaron loops infinitos |
| **No coverage** | Código sin pruebas asociadas |

### Umbrales Recomendados

| Nivel | Score | Acción |
|-------|-------|--------|
| Alto | ≥80% | Excelente calidad de pruebas |
| Bajo | 60-80% | Aceptable, mejorar pruebas débiles |
| Crítico | <60% | Revisar urgentemente la suite |

## Ejemplo de Mutante Sobreviviente

### Código Original

```csharp
public ServiceResult AddProduct(ProductDto product)
{
    if (product.Quantity < 0)  // Mutante: < → <=
        errors.Add("Cantidad no puede ser negativa");
}
```

### Prueba Actual (no detecta el mutante)

```csharp
[Fact]
public void AddProduct_NegativeQuantity_ShouldFail()
{
    var dto = new ProductDto("SKU", "Name", -1, 100m);  // Solo prueba -1
    // No prueba el caso borde: quantity = 0
}
```

### Prueba Mejorada

```csharp
[Theory]
[InlineData(-1)]
[InlineData(0)]  // Caso borde agregado
public void AddProduct_InvalidQuantity_ShouldFail(int quantity)
{
    var dto = new ProductDto("SKU", "Name", quantity, 100m);
    // Ahora detecta el mutante < vs <=
}
```

## Consideraciones Técnicas

### Rendimiento

- Mutation testing es **lento** (ejecuta todas las pruebas por cada mutante)
- Ejecutar solo en CI nocturno o antes de releases
- Filtrar archivos/mutadores para reducir tiempo

### Falsos Positivos

Algunos mutantes sobrevivientes son aceptables:
- Cambios en logging
- Código defensivo redundante
- Optimizaciones sin impacto funcional

## Relación con el Libro

Este código corresponde al **Capítulo VI: Mutation Testing** del libro, donde se explica:

- Fundamentos del mutation testing
- Instalación y configuración de Stryker.NET
- Interpretación de reportes y mutation score
- Estrategias para mejorar pruebas débiles
- Integración con pipelines de CI/CD

## Ejercicios Sugeridos

1. Ejecutar Stryker en `ChaloStore.Inventory` y analizar sobrevivientes
2. Agregar pruebas para matar mutantes vivos
3. Configurar umbral de mutation score en CI
