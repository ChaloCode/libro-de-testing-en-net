# Capítulo V: Cobertura de Código

## Descripción

Este capítulo explica cómo medir y analizar la cobertura de código usando Coverlet y ReportGenerator. La cobertura indica qué porcentaje del código es ejecutado por las pruebas.

## Código Relacionado

| Archivo | Propósito |
|---------|-----------|
| [`../chalostore/tests/ChaloStore.UnitTests/`](../chalostore/tests/ChaloStore.UnitTests/) | Suite principal para análisis de cobertura |
| [`../chalostore/src/ChaloStore.Inventory/InventoryService.cs`](../chalostore/src/ChaloStore.Inventory/InventoryService.cs) | Código fuente a analizar |

## Herramientas

| Herramienta | Propósito |
|-------------|-----------|
| **Coverlet** | Recolector de cobertura para .NET |
| **ReportGenerator** | Genera reportes HTML desde archivos de cobertura |

## Cómo Ejecutar

### 1. Instalar Herramientas Globales

```bash
# Instalar ReportGenerator (solo una vez)
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### 2. Ejecutar Pruebas con Cobertura

```bash
cd ../chalostore

# Ejecutar con cobertura (formato Cobertura XML)
dotnet test tests/ChaloStore.UnitTests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

# Con umbral mínimo (falla si < 80%)
dotnet test tests/ChaloStore.UnitTests \
  /p:CollectCoverage=true \
  /p:Threshold=80
```

### 3. Generar Reporte HTML

```bash
# Generar reporte visual
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html

# Abrir reporte
open ./coverage/report/index.html
```

## Configuración en .csproj

Para habilitar cobertura automáticamente, agregar al archivo de pruebas:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Configuración de Coverlet -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
    <Threshold>80</Threshold>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>
</Project>
```

## Métricas de Cobertura

| Métrica | Descripción |
|---------|-------------|
| **Line Coverage** | % de líneas ejecutadas |
| **Branch Coverage** | % de ramas (if/else) ejecutadas |
| **Method Coverage** | % de métodos invocados |
| **Class Coverage** | % de clases tocadas |

## Interpretación de Resultados

### Ejemplo de Salida

```
+------------------+--------+--------+--------+
| Module           | Line   | Branch | Method |
+------------------+--------+--------+--------+
| ChaloStore.Inventory | 85.7% | 75.0% | 100% |
+------------------+--------+--------+--------+
```

### Umbrales Recomendados

| Nivel | Umbral | Contexto |
|-------|--------|----------|
| Mínimo | 60% | Proyectos legacy en transición |
| Bueno | 80% | Proyectos activos |
| Excelente | 90%+ | Código crítico |

## Consideraciones Técnicas

### Cobertura Alta ≠ Calidad Alta

- 100% de cobertura no garantiza ausencia de bugs
- Las pruebas pueden ejecutar código sin verificar comportamiento
- Mutation testing complementa la cobertura

### Exclusiones Comunes

```csharp
// Excluir de cobertura con atributo
[ExcludeFromCodeCoverage]
public class GeneratedCode { }
```

O en el archivo de configuración:

```xml
<CoverletExclude>[*]*.Migrations.*</CoverletExclude>
```

### Integración con CI

```yaml
# Ejemplo GitHub Actions
- name: Test with coverage
  run: dotnet test --collect:"XPlat Code Coverage"
  
- name: Upload coverage
  uses: codecov/codecov-action@v3
```

## Relación con el Libro

Este código corresponde al **Capítulo V: Cobertura de Código** del libro, donde se explica:

- Qué es la cobertura y qué mide realmente
- Configuración de Coverlet en proyectos .NET
- Generación de reportes con ReportGenerator
- Integración con CI/CD (GitHub Actions, Azure DevOps)
- Cuándo la cobertura engaña y cómo evitarlo

## Ejercicios Sugeridos

1. Ejecutar cobertura y analizar áreas sin cubrir
2. Agregar pruebas para alcanzar 90% de cobertura
3. Configurar umbral en CI para fallar bajo 80%
