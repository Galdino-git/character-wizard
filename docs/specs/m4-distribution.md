# Spec: Distribution (F15–F17)

**Feature IDs:** F15 (Windows packaging) · F16 (Embedded data + first-run extraction) · F17 (Manual content update flow)
**Status:** Specification
**Depends on:** todo o trabalho de M0–M3.

## Context

Hoje o app só roda no ambiente de desenvolvimento — exige .NET 10 SDK, workload MAUI, build via `dotnet`, e a pasta `data/` ao lado do executável. Para que o usuário (e amigos) consigam usar sem terminal nem Visual Studio, o app precisa virar um artefato instalável Windows único contendo runtime, código e dados.

Como é ferramenta pessoal, o destino primário é a própria máquina do usuário (possivelmente outra máquina sem .NET). Não há Microsoft Store, não há servidor, não há telemetria.

## Goals

- Um único artefato (instalável ou zip) que o usuário pode baixar e abrir num Windows limpo.
- Dados do 5etools embarcados (sem precisar clonar repo separado).
- Caminho documentado para atualizar os dados quando o 5etools mudar.

## Non-goals

- Distribuição via Microsoft Store.
- Auto-update do app (do código C#) — usuário baixa nova versão manualmente.
- Auto-update dos dados via internet (5etools não tem release feed oficial estável; o usuário re-importa quando quiser).
- Portabilidade pra macOS/Linux/Android/iOS — Windows only.
- Assinatura com cert comercial — self-signed ou unsigned é aceitável para uso pessoal.

## User stories

- **US-15.1** — Como usuário, baixo `CharacterWizard-Setup.exe` (ou `CharacterWizard.zip`), executo, e o app abre. Não preciso instalar SDK.
- **US-15.2** — Levo o instalador para outro PC Windows 10/11 sem .NET — funciona.
- **US-16.1** — A primeira vez que abro o app, os dados são extraídos para `%AppData%/CharacterWizard/content/`. Subsequentes aberturas são instantâneas.
- **US-16.2** — Não preciso de pasta `data/` no diretório do exe — tudo vive no AppData.
- **US-17.1** — Quando o repo do 5etools recebe atualizações importantes, executo um tool (CLI ou botão no app) que re-importa os JSONs/imagens locais sem desinstalar nada.
- **US-17.2** — Vejo no app a data da última importação dos dados (do `MANIFEST.json`) para saber se está atualizado.

## Functional requirements

### F15 — Windows packaging

- **REQ-15.1** — Produzir um build **self-contained** para `win-x64` que inclua .NET 10 runtime + MAUI dependencies + app code.
- **REQ-15.2** — O artefato final é distribuído como **ZIP** (`CharacterWizard-v{ver}-win-x64.zip`) contendo a pasta publicada.
- **REQ-15.3** — Opcional avaliação: produzir também **MSIX unpackaged** ou **single-file exe** se o MAUI Windows suportar de forma estável.
- **REQ-15.4** — `Properties/launchSettings.json` e configs de dev não devem vazar para o pacote.
- **REQ-15.5** — Documentar em [`docs/release.md`](release.md) os passos de build do release (comando, flags, output esperado, troubleshooting).
- **REQ-15.6** — Script PowerShell `tools/Build-Release.ps1` automatiza: clean → restore → publish self-contained → zip da pasta resultante → output em `artifacts/`.

### F16 — Embedded data + first-run extraction

- **REQ-16.1** — Os dados (`5etools-json/` + `5etools-img/` + `MANIFEST.json`) são embarcados no app via `MauiAsset` — **como ZIP único** (`cw-content.zip`) para evitar custo de muitos arquivos.
- **REQ-16.2** — Ao iniciar, se `%AppData%/CharacterWizard/content/MANIFEST.json` não existir, OU se o hash do `cw-content.zip` embarcado diferir do hash registrado em `%AppData%/CharacterWizard/content/.installed-hash`, extrair o ZIP para `content/` e atualizar o `.installed-hash`.
- **REQ-16.3** — `AppPaths.DataRoot` aponta para `%AppData%/CharacterWizard/content/` em vez de fazer walk-up.
- **REQ-16.4** — Extração mostra splash/log "Preparando conteúdo..." na primeira execução; opera de forma síncrona antes do MAUI App carregar (no `MauiProgram.CreateMauiApp` antes do `Build`).
- **REQ-16.5** — Falha de extração mostra erro claro em vez de crash silencioso.

### F17 — Manual content update flow

- **REQ-17.1** — Continuar mantendo `tools/Import5eToolsData` funcional para uso CLI por dev.
- **REQ-17.2** — Em `Settings.razor`, adicionar seção "Conteúdo do 5etools" mostrando:
    - Data e hash da importação atual (lidos de `MANIFEST.json`).
    - Botão "Reextrair conteúdo embarcado" — força reextração do ZIP do app.
    - Texto: "Para atualizar a partir do upstream 5etools, baixe uma nova versão do app."
- **REQ-17.3** — Build de release re-empacota o `cw-content.zip` automaticamente lendo da pasta `data/` corrente (via `Build-Release.ps1`).

## Non-functional

- **NFR-15** — Pacote não-instalado deve ter < 200 MB (.NET self-contained ~70-100 MB + dados ~180 MB; aceitar 250 MB se necessário).
- **NFR-16** — Tempo de extração na 1ª execução < 10 s em SSD comum.
- **NFR-17** — Reextração via botão não bloqueia UI por mais de 5 s.

## Acceptance criteria

- AC-15 — Em VM Windows 11 limpa (sem .NET): descompactar zip, executar `CharacterWizard.exe`, app abre e mostra catálogo carregado.
- AC-16.1 — Excluir `%AppData%/CharacterWizard/content/`, reabrir app — conteúdo é re-extraído automaticamente.
- AC-16.2 — Conferir que pasta de install não precisa do diretório `data/` ao lado.
- AC-17.1 — Em Settings, ver "Conteúdo importado em <data>" — bate com `MANIFEST.json` interno.
- AC-17.2 — Clicar "Reextrair" e reverificar contagens na tela.

## Open questions (a decidir antes de execução)

- **Q1** — MSIX vale o esforço para uso pessoal? (Recomendação: skip; ZIP basta. Reavaliar se o usuário quiser instalar pra outras pessoas.)
- **Q2** — Como versionar releases? (Sugestão: semver simples em `CharacterWizard.App.csproj` `ApplicationDisplayVersion`, tag git `vX.Y.Z`.)
- **Q3** — `cw-content.zip` versionado no repo ou regenerado a cada build? (Sugestão: regenerado por `Build-Release.ps1`, **não** commitado — adicionar `.gitignore`.)
