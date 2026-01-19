# ChaloStore - Ejemplos del libro

Este repositorio acompaña los capítulos del libro **"Testing en .NET"** y agrupa el código de ejemplo unificado:

- `src/ChaloStore.Inventory`: lógica de inventario y validaciones.
- `src/ChaloStore.Orders`: entidades y servicios relacionados al checkout.
- `src/ChaloStore.Web`: API mínima para crear órdenes.
- `tests/ChaloStore.UnitTests`: pruebas unitarias del módulo de inventario.
- `tests/ChaloStore.IntegrationTests`: pruebas de integración del endpoint `/orders`.
- `tests/ChaloStore.AcceptanceTests`: escenarios BDD con Reqnroll y pruebas E2E con Playwright.

## Requisitos

- **.NET 10 SDK** (versión 10.0.2 o superior recomendada).

## Dependencias principales

| Paquete | Versión |
|---------|---------|
| Entity Framework Core | 10.0.2 |
| xUnit | 2.9.3 |
| FluentAssertions | 8.8.0 |
| NSubstitute | 5.3.0 |
| Reqnroll | 3.3.2 |
| Microsoft.Playwright | 1.57.0 |
| WireMock.Net | 1.18.0 |

## Cómo ejecutar pruebas

```bash
# Pruebas unitarias
dotnet test tests/ChaloStore.UnitTests

# Pruebas de integración
dotnet test tests/ChaloStore.IntegrationTests

# Pruebas de aceptación (requiere playwright install previo)
dotnet test tests/ChaloStore.AcceptanceTests
```

Para las pruebas de aceptación con UI, ejecuta primero:

```bash
pwsh bin/Debug/net10.0/playwright.ps1 install
```

## Seed y contratos

- `infra/seed-data`: espacio reservado para scripts o archivos JSON de inicialización.
- `contracts/`: directorios para almacenar pacts (pagos/envíos) usados en el capítulo de contract testing.
