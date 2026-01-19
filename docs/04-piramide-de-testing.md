# Capítulo IV: Pirámide de Testing

## Descripción

Este capítulo explica la pirámide de testing y cómo distribuir las pruebas entre los diferentes niveles. ChaloStore sirve como ejemplo práctico de una solución que implementa los tres niveles de la pirámide.

## Código Relacionado

| Carpeta | Nivel | Cantidad Aprox. |
|---------|-------|-----------------|
| [`../chalostore/tests/ChaloStore.UnitTests/`](../chalostore/tests/ChaloStore.UnitTests/) | Unitarias (base) | 70% |
| [`../chalostore/tests/ChaloStore.IntegrationTests/`](../chalostore/tests/ChaloStore.IntegrationTests/) | Integración (medio) | 20% |
| [`../chalostore/tests/ChaloStore.AcceptanceTests/`](../chalostore/tests/ChaloStore.AcceptanceTests/) | Aceptación (cima) | 10% |

## La Pirámide de Testing

```
        /\
       /  \
      / E2E\        ← Pruebas de Aceptación (lentas, frágiles, costosas)
     /------\
    /        \
   /Integración\    ← Pruebas de Integración (velocidad media)
  /--------------\
 /                \
/    Unitarias     \  ← Pruebas Unitarias (rápidas, estables, económicas)
--------------------
```

## Distribución Recomendada (70/20/10)

| Nivel | Porcentaje | Características |
|-------|------------|-----------------|
| **Unitarias** | 70% | Rápidas (<100ms), aisladas, sin I/O |
| **Integración** | 20% | Velocidad media, prueban colaboración entre componentes |
| **Aceptación** | 10% | Lentas, prueban flujos completos de usuario |

## Aplicación en ChaloStore

### Nivel Unitario

```
tests/ChaloStore.UnitTests/
└── InventoryServiceTests.cs    # Lógica de negocio aislada
```

- Valida reglas de negocio del `InventoryService`
- Usa mocks para dependencias
- Ejecución: ~300ms

### Nivel de Integración

```
tests/ChaloStore.IntegrationTests/
└── OrderEndpointTests.cs       # API + Base de datos
```

- Prueba endpoint `/orders` real
- Usa base de datos InMemory
- Ejecución: ~2s

### Nivel de Aceptación

```
tests/ChaloStore.AcceptanceTests/
├── Features/Checkout.feature   # Escenarios BDD
└── CheckoutFlowTests.cs        # E2E con UI
```

- Prueba flujos completos de checkout
- Usa Playwright para UI
- Ejecución: ~10-50s

## Cómo Ejecutar por Nivel

```bash
cd ../chalostore

# Solo unitarias (rápido, ejecutar frecuentemente)
dotnet test tests/ChaloStore.UnitTests

# Solo integración (antes de commit)
dotnet test tests/ChaloStore.IntegrationTests

# Solo aceptación (antes de merge/deploy)
dotnet test tests/ChaloStore.AcceptanceTests

# Todas las pruebas
dotnet test
```

## Métricas de Ejecución

| Suite | Tests | Tiempo Aprox. |
|-------|-------|---------------|
| UnitTests | 4 | ~300ms |
| IntegrationTests | 1 | ~2s |
| AcceptanceTests | 6 | ~10s |
| **Total** | **11** | **~12s** |

## Consideraciones Técnicas

### Cuándo Usar Cada Nivel

- **Unitarias**: Lógica de negocio, validaciones, cálculos
- **Integración**: APIs, base de datos, servicios externos mockeados
- **Aceptación**: Flujos críticos de negocio, regresiones importantes

### Anti-patrones

- **Pirámide invertida**: Más E2E que unitarias (lento, frágil)
- **Helado**: Solo pruebas manuales arriba, pocas automatizadas
- **Todo unitario**: Sin validar integración real entre componentes

### Balance Costo/Beneficio

| Nivel | Costo de Escritura | Costo de Mantenimiento | Confianza |
|-------|-------------------|------------------------|-----------|
| Unitarias | Bajo | Bajo | Media |
| Integración | Medio | Medio | Alta |
| Aceptación | Alto | Alto | Muy Alta |

## Relación con el Libro

Este código corresponde al **Capítulo IV: Pirámide de Testing** del libro, donde se explica:

- Origen y evolución de la pirámide de testing
- Distribución óptima de pruebas
- Cuándo romper la regla 70/20/10
- Estrategias para proyectos legacy
- El concepto de "Testing Trophy" como alternativa

## Ejercicios Sugeridos

1. Calcular la distribución actual de pruebas en tu proyecto
2. Identificar pruebas E2E que podrían ser de integración
3. Mover lógica de integración a unitarias donde sea posible
