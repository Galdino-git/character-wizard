# CharacterWizard

Desktop character creator/manager for D&D 5e, consuming game data from the [5etools](https://github.com/TheGiddyLimit/TheGiddyLimit.github.io) project (MIT-licensed).

Inspired by the character creator workflow in Foundry VTT (D&D 5e system). Runs 100% locally as a single Windows desktop application — no server, no cloud, no account.

## Stack

- **.NET 10** (SDK 10.0.204+)
- **MAUI Blazor Hybrid** (UI em Razor/HTML/CSS, empacotado como `.exe` Windows)
- **xUnit** (testes)
- **System.Text.Json** (parsing dos JSONs do 5etools)

## Estrutura

```
src/
  CharacterWizard.App/    # MAUI Blazor Hybrid (UI + entry point)
  CharacterWizard.Core/   # Domínio: Character, regras, level-up, persistência
  CharacterWizard.Data/   # Ingestão e acesso aos dados do 5etools
tools/
  Import5eToolsData/      # Console que copia data + imagens do repo do 5etools
tests/
  CharacterWizard.Tests/  # Testes xUnit
data/                     # JSON e imagens importadas (não versionado)
```

## Setup

Pré-requisitos:
- .NET 10 SDK
- Workload MAUI: `dotnet workload install maui`
- Repositório do [5etools](https://github.com/TheGiddyLimit/TheGiddyLimit.github.io) clonado localmente (default esperado: `..\5etools` relativo a este repo)

Primeira importação:

```powershell
dotnet run --project tools/Import5eToolsData -- `
  --source "..\5etools" `
  --target ".\data"
```

Rodar o app:

```powershell
dotnet build src/CharacterWizard.App -t:Run -f net10.0-windows10.0.19041.0
```

## Licença

O código deste repositório é seu (defina a licença). Os dados consumidos pertencem ao projeto **5etools** (MIT, © TheGiddyLimit e contribuidores) e fazem referência a regras © Wizards of the Coast. Uso pessoal/local.
