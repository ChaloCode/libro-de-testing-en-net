[![Portada del libro Testing en .NET](https://m.media-amazon.com/images/I/61QO+j1vbKL._SY466_.jpg)](https://a.co/d/eY75l4q)

> El libro **Testing en .NET** ya está disponible.

No es otro libro de testing lleno de recetas mágicas: es una guía pensada para programadores que quieren tocar código sin miedo.

En estas páginas encontrarás un recorrido práctico, desde los fundamentos hasta técnicas avanzadas de testing en .NET, con ejemplos reales, criterios de diseño, TDD, BDD y automatización aplicada al día a día de equipos de desarrollo. El foco no está en la herramienta, sino en cómo pensar y diseñar pruebas que generen confianza, sean rápidas y se mantengan en el tiempo.

<a href="https://a.co/d/eY75l4q" target="_blank" rel="noopener noreferrer" class="btn btn--primary btn--large">Comprar en Amazon</a>

# Documentación de Ejemplos por Capítulo

Esta carpeta contiene documentación detallada que relaciona los ejemplos de código con cada capítulo del libro **"Testing en .NET"**.

## Propósito

Cada documento explica:

- Qué código corresponde al capítulo
- Cómo ejecutar los ejemplos
- Consideraciones técnicas y dependencias
- Relación directa con el contenido del libro

## Índice de Documentos

| Capítulo | Documento | Tema |
|----------|-----------|------|
| I | [01-pruebas-unitarias.md](docs/01-pruebas-unitarias.md) | Pruebas unitarias con xUnit, NSubstitute y FluentAssertions |
| II | [02-pruebas-de-integracion.md](docs/02-pruebas-de-integracion.md) | Pruebas de integración con WebApplicationFactory |
| III | [03-pruebas-de-aceptacion.md](docs/03-pruebas-de-aceptacion.md) | Pruebas de aceptación con Reqnroll y Playwright |
| IV | [04-piramide-de-testing.md](docs/04-piramide-de-testing.md) | Pirámide de testing y distribución de suites |
| V | [05-code-coverage.md](docs/05-code-coverage.md) | Cobertura de código con Coverlet |
| VI | [06-mutation-testing.md](docs/06-mutation-testing.md) | Mutation testing con Stryker.NET |
| VII | [07-contract-testing.md](docs/07-contract-testing.md) | Contract testing con PactNet |
| VIII | [08-test-driven-development.md](docs/08-test-driven-development.md) | Test-Driven Development (TDD) |
| IX | [09-behavior-driven-development.md](docs/09-behavior-driven-development.md) | Behavior-Driven Development (BDD) |
| X | [10-futuro-es-hoy.md](docs/10-futuro-es-hoy.md) | IA aplicada a testing |

## Estructura del Proyecto

```
libro-de-testing-en-net/
├── docs/                    # Documentación por capítulo
│   ├── 01-pruebas-unitarias.md
│   ├── 02-pruebas-de-integracion.md
│   ├── 03-pruebas-de-aceptacion.md
│   ├── 04-piramide-de-testing.md
│   ├── 05-code-coverage.md
│   ├── 06-mutation-testing.md
│   ├── 07-contract-testing.md
│   ├── 08-test-driven-development.md
│   ├── 09-behavior-driven-development.md
│   └── 10-futuro-es-hoy.md
└── chalostore/              # Código de ejemplo
    ├── src/
    │   ├── ChaloStore.Inventory/
    │   ├── ChaloStore.Orders/
    │   └── ChaloStore.Web/
    ├── tests/
    │   ├── ChaloStore.UnitTests/
    │   ├── ChaloStore.IntegrationTests/
    │   └── ChaloStore.AcceptanceTests/
    ├── contracts/
    └── infra/
```

## Requisitos

- **.NET 10 SDK** (versión 10.0.2 o superior)
- Para pruebas de aceptación: `playwright install`

## Ejecución Rápida

```bash
cd chalostore

# Todas las pruebas
dotnet test

# Por suite específica
dotnet test tests/ChaloStore.UnitTests
dotnet test tests/ChaloStore.IntegrationTests
dotnet test tests/ChaloStore.AcceptanceTests
```
