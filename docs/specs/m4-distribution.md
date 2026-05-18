# Spec: Distribution (M4)

**Status:** Specification
**Depends on:** todo o trabalho de M0–M3.

## Context

App é uma ferramenta pessoal. Quero conseguir mandar uma cópia funcional para um amigo (ou rodar em outro PC meu) sem que ele precise instalar SDK, MAUI workload ou abrir terminal. Quero também ter o código no GitHub e usar GitHub Releases para hospedar os ZIPs versionados.

## Goals

- Um único ZIP que o amigo baixa, extrai e executa.
- Sem instalar nada além (na pior das hipóteses, WebView2 Runtime — universal no Windows moderno).
- Código no GitHub, releases pela aba **Releases** do repo.
- Documentação clara em `docs/release.md` sobre como gerar e publicar uma nova versão.

## Non-goals

- MSIX / Microsoft Store
- Assinatura comercial (deixa SmartScreen avisar; OK pra uso entre amigos)
- Auto-update do app
- Embedding via `MauiAsset` + bootstrapper (over-engineering — basta `data/` ao lado do exe)
- Cross-platform (Windows only)
- CI/CD automatizado (futuro, fora deste milestone)

## User stories

- **US-1** — Como dev, rodo um script `Build-Release.ps1` que produz `artifacts/CharacterWizard-v{ver}-win-x64.zip`.
- **US-2** — Como dev, sigo `docs/release.md` para subir o ZIP em uma nova GitHub Release (via `gh` CLI ou web UI).
- **US-3** — Amigo abre o link, baixa o ZIP, extrai, dá duplo clique no `.exe` — funciona.

## Functional requirements

- **REQ-1** — `dotnet publish` self-contained `win-x64` produz pasta funcional (runtime + libs + exe).
- **REQ-2** — Script `tools/Build-Release.ps1` automatiza: importar dados do upstream 5etools → publish → copiar `data/` para `publish/data/` → zipar.
- **REQ-3** — Pasta `data/` empacotada inclui apenas o subset já filtrado (sem 12GB do bestiário; igual ao que o app usa hoje).
- **REQ-4** — `AppPaths.ResolveDataRoot()` continua usando walk-up — funciona idêntico em dev (root do repo) e em release (`{instalação}/data/`).
- **REQ-5** — `docs/release.md` documenta: pré-requisitos, comando do script, como criar GitHub Release com `gh release create`, instruções pro amigo.
- **REQ-6** — `README.md` atualizado mencionando o link da Releases e as instruções rápidas pro usuário final.

## Non-functional

- **NFR-1** — ZIP final < 250 MB (estimado: ~80 MB runtime + ~180 MB de dados/imagens).
- **NFR-2** — Tempo de build do release < 3 min em SSD.
- **NFR-3** — `release.md` deve ser executável passo-a-passo sem ambiguidade.

## Acceptance criteria

- **AC-1** — Rodar `tools/Build-Release.ps1 -Version 0.1.0` produz `artifacts/CharacterWizard-v0.1.0-win-x64.zip` sem erros.
- **AC-2** — Extrair o ZIP em uma pasta vazia e executar `CharacterWizard.App.exe` abre o app com o catálogo carregado.
- **AC-3** — Criar release no GitHub via `gh release create v0.1.0 artifacts/...zip` aparece na aba Releases do repo.
- **AC-4** — Seguir `docs/release.md` sem conhecimento prévio reproduz o resultado.
