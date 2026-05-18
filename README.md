# CharacterWizard

Desktop character creator/manager for D&D 5e, consuming game data from the [5etools](https://github.com/TheGiddyLimit/TheGiddyLimit.github.io) project (MIT-licensed).

Inspired by the character creator workflow in Foundry VTT (D&D 5e system). Runs 100% locally as a single Windows desktop application — no server, no cloud, no account.

---

## 🎲 Para usar (não-dev)

1. Baixe o ZIP mais recente em **[Releases](https://github.com/Galdino-git/character-wizard/releases)**.
2. Extraia em qualquer pasta (não precisa de admin).
3. Duplo clique em `CharacterWizard.App.exe`.

**Avisos comuns:**
- *"Windows protegeu o seu PC"* (SmartScreen): clique em **Mais informações → Executar mesmo assim**. Acontece porque o exe não é assinado com cert comercial.
- *Tela branca* (raro): instale o [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Windows 11 já tem; Windows 10 atualizado também.

**Requisitos:** Windows 10 1809+ ou 11, x64. ~600 MB de espaço.

---

## 🛠 Para desenvolver

### Stack

- **.NET 10** (SDK 10.0.204+)
- **MAUI Blazor Hybrid** (UI em Razor/HTML/CSS, empacotado como `.exe` Windows)
- **xUnit** (testes)
- **System.Text.Json** (parsing dos JSONs do 5etools)

### Estrutura

```
src/
  CharacterWizard.App/    # MAUI Blazor Hybrid (UI + entry point)
  CharacterWizard.Core/   # Domínio: Character, regras, level-up, persistência
  CharacterWizard.Data/   # Ingestão e acesso aos dados do 5etools
tools/
  Import5eToolsData/      # Console que copia data + imagens do repo do 5etools
  Build-Release.ps1       # Empacota release ZIP
tests/
  CharacterWizard.Tests/  # 122 testes xUnit
data/                     # JSON e imagens importadas (não versionado)
docs/                     # Specs, designs, tasks por feature + release.md
```

Detalhes de convenções e setup completo em [`CLAUDE.md`](CLAUDE.md). Roadmap por milestone em [`docs/ROADMAP.md`](docs/ROADMAP.md).

### Setup

Pré-requisitos:
- .NET 10 SDK
- Workload MAUI: `dotnet workload install maui`
- Repositório do [5etools](https://github.com/TheGiddyLimit/TheGiddyLimit.github.io) clonado em `..\5etools` (relativo a este repo)

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

Rodar testes:

```powershell
dotnet test CharacterWizard.slnx
```

### Gerar release

Ver [`docs/release.md`](docs/release.md). Resumo:

```powershell
.\tools\Build-Release.ps1 -Version 0.1.0
# zip vai pra artifacts\
```

---

## Licença

O código deste repositório é seu (defina a licença). Os dados consumidos pertencem ao projeto **5etools** (MIT, © TheGiddyLimit e contribuidores) e fazem referência a regras © Wizards of the Coast. Uso pessoal/local.
