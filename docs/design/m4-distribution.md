# Design: Distribution (M4)

Arquitetura **deliberadamente simples**: nada além do `dotnet publish` padrão + script PowerShell + guia.

## Fluxo

```
┌──────────────────────────────────────────────┐
│ Build-Release.ps1                            │
│                                              │
│   1. dotnet run --project tools/Import5e...  │  refresh dados locais
│   2. dotnet publish src/CharacterWizard.App  │  self-contained win-x64
│       -c Release -r win-x64                  │
│       --self-contained true                  │
│       -o artifacts/publish                   │
│   3. Copy-Item data → artifacts/publish/data │  inclui dados ao lado do exe
│   4. Compress-Archive artifacts/publish      │  gera ZIP final
│       → artifacts/CharacterWizard-vX.Y.Z.zip │
└──────────────────────────────────────────────┘
                       │
                       ▼
              gh release create vX.Y.Z artifacts\CW...zip
                       │
                       ▼
              GitHub Releases (público)
                       │
                       ▼
              Amigo baixa + extrai + duplo-clique
```

## Estrutura do ZIP

```
CharacterWizard-v0.1.0-win-x64.zip
├── CharacterWizard.App.exe          ← entrypoint
├── *.dll                             ← runtime .NET + MAUI (auto)
├── runtimes/                         ← native libs (WebView2 loader etc)
├── Microsoft.Web.WebView2.*.dll
├── …
└── data/                             ← injetado pelo script
    ├── 5etools-json/
    ├── 5etools-img/
    └── MANIFEST.json
```

## Componentes

### `tools/Build-Release.ps1`

Único arquivo novo. Idempotente, parametrizado:

```powershell
param(
    [string]$Version = "0.0.0-dev",
    [string]$DataSource = "$PSScriptRoot\..\..\5etools",
    [string]$Output = "$PSScriptRoot\..\artifacts"
)
$ErrorActionPreference = 'Stop'

$repo = Resolve-Path "$PSScriptRoot\.."
$publishDir = Join-Path $Output "publish"

if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir | Out-Null

# 1. Refresh data
Push-Location $repo
dotnet run --project tools\Import5eToolsData -- --source $DataSource --target .\data

# 2. Publish self-contained
dotnet publish src\CharacterWizard.App `
    -c Release -f net10.0-windows10.0.19041.0 -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -p:WindowsPackageType=None `
    -o $publishDir

# 3. Copy data alongside exe
Copy-Item -Recurse .\data $publishDir\data

# 4. Zip
$zip = Join-Path $Output "CharacterWizard-v$Version-win-x64.zip"
Compress-Archive -Path "$publishDir\*" -DestinationPath $zip -Force

Pop-Location
Write-Host "Release ready: $zip"
```

### `docs/release.md`

Guia executável, segue os passos:

```
1. Pré-requisitos (dev): .NET 10 SDK, MAUI workload, gh CLI, ..\5etools clonado
2. Rodar: .\tools\Build-Release.ps1 -Version 0.1.0
3. Testar: extrair zip pra pasta temp, executar exe, verificar carregamento
4. Criar tag git: git tag v0.1.0 && git push origin v0.1.0
5. Publicar: gh release create v0.1.0 artifacts\CharacterWizard-v0.1.0-win-x64.zip ^
              --title "v0.1.0" --notes "..." --target main
6. Conferir no GitHub
```

E uma seção "Instruções para o usuário final":

```
1. Baixe o ZIP da última release
2. Extraia em qualquer pasta (não precisa de admin)
3. Duplo-clique em CharacterWizard.App.exe
4. SmartScreen: clique "Mais informações → Executar mesmo assim" (exe não-assinado)
5. (Se faltar WebView2): instale de https://go.microsoft.com/fwlink/p/?LinkId=2124703
```

### Atualizações em arquivos existentes

- `README.md`: link pra Releases + instruções rápidas.
- `.gitignore`: já ignora `bin/`, `obj/`, `data/`. Adicionar `artifacts/`.
- `AppPaths.cs`: nada muda. O walk-up encontra `data/` na pasta de release.

## Decisões registradas

- **Sem MSIX, sem bootstrapper, sem hash check** — over-engineering pra escopo "passar pra um amigo".
- **`data/` ao lado do exe** > embedding via MauiAsset — debug trivial, sem código novo no app.
- **Versionamento manual** via flag do script + tag git — sem semver automation.
- **Release via `gh` CLI manual** — sem GitHub Actions agora; documentado como item futuro.
- **Não distribuir o source `data/` no repo** — segue gitignored; script regenera do upstream a cada release.

## Riscos

- **SmartScreen warning** na 1ª execução — documentar em `release.md` (usuário final).
- **Tamanho ~250 MB** — aceitável; Releases comporta até 2 GB por arquivo.
- **WebView2 missing em Win10 muito velho** — incluir link no README.
- **MAUI publish flakiness** — caso encontre, registrar fix em `release.md`.
