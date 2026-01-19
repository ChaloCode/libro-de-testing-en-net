# ChaloStore.AcceptanceTests

Este proyecto implementa los escenarios BDD del libro utilizando **Reqnroll 3.3.x** y **Playwright 1.57.x** sobre **.NET 10**.

## Estructura del proyecto

- `Features/`: archivos `.feature` con escenarios Gherkin.
- `Steps/`: step definitions que implementan los pasos de los escenarios.
- `Support/`: drivers y helpers para interactuar con la aplicación.

## Dependencias principales

| Paquete | Versión |
|---------|---------|
| Reqnroll | 3.3.2 |
| Reqnroll.xUnit | 3.3.2 |
| Microsoft.Playwright | 1.57.0 |
| WireMock.Net | 1.18.0 |
| FluentAssertions | 8.8.0 |

## Cómo ejecutar

```bash
# Instalar navegadores de Playwright (solo la primera vez)
pwsh bin/Debug/net10.0/playwright.ps1 install

# Ejecutar pruebas
dotnet test
```

## Recomendaciones

1. Crear features en `Features/` con los escenarios de checkout de ChaloStore.
2. Implementar los step definitions en `Steps/` reutilizando `CheckoutApiDriver`.
3. Usar Playwright para automatizar la UI cuando sea necesario.

> Este directorio mantiene la estructura esperada (`tests/ChaloStore.AcceptanceTests`) para que los comandos y rutas del libro funcionen.
