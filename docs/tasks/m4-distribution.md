# Tasks: Distribution (F15–F17)

Spec: [`../specs/m4-distribution.md`](../specs/m4-distribution.md) · Design: [`../design/m4-distribution.md`](../design/m4-distribution.md)

## F15 — Windows packaging

- [ ] **T15.1** Testar comando de `dotnet publish` self-contained `win-x64` no projeto App; resolver eventuais erros de MAUI/Razor sourcegen no Release.
- [ ] **T15.2** Criar `docs/release.md` com instruções passo-a-passo e troubleshooting (espaço em disco, antivírus, WebView2).
- [ ] **T15.3** Criar `tools/Build-Release.ps1` que: importa dados → gera `cw-content.zip` → publica → zipa publish folder.
- [ ] **T15.4** Adicionar `artifacts/` ao `.gitignore`.
- [ ] **T15.5** Validação manual: rodar `Build-Release.ps1` → executar o zip extraído numa VM/usuário limpo. Documentar resultado.
- [ ] **T15.6** Commit `build: self-contained windows release pipeline (F15)`.

## F16 — Embedded data + first-run extraction

- [ ] **T16.1** Adicionar `cw-content.zip` ao `.gitignore` (regenerado por build).
- [ ] **T16.2** Implementar `Services/ContentBootstrapper.cs` (`EnsureExtractedAsync`, `ForceReextractAsync`, hash check).
- [ ] **T16.3** Adaptar `AppPaths.ResolveDataRoot` para priorizar `%AppData%/CharacterWizard/content/` com fallback dev via env `CW_DATA_OVERRIDE`.
- [ ] **T16.4** Chamar `bootstrap.EnsureExtractedAsync().GetAwaiter().GetResult()` em `MauiProgram.CreateMauiApp` antes do DI.
- [ ] **T16.5** TDD `ContentBootstrapperTests` (in-memory ZIP):
    - `Extracts_when_hash_missing`
    - `Skips_when_hash_matches`
    - `Reextracts_when_hash_differs`
- [ ] **T16.6** Documentar variável `CW_DATA_OVERRIDE` em `CLAUDE.md` (fluxo dev preserva pasta `data/`).
- [ ] **T16.7** Commit `feat(app): bootstrap embedded content into AppData on startup (F16)`.

## F17 — Manual content update flow

- [ ] **T17.1** `ManifestReader` em Data: lê `MANIFEST.json`, retorna `ContentManifest` record.
- [ ] **T17.2** Seção "Conteúdo do 5etools" no `Settings.razor` mostrando data/hash/contagens.
- [ ] **T17.3** Botão "Reextrair conteúdo embarcado" chama `ContentBootstrapper.ForceReextractAsync` + refresh do `CatalogProvider` (reload do catálogo a partir do disco — custoso, mostrar spinner).
- [ ] **T17.4** Atualizar README com o fluxo de upgrade: "baixe nova versão do zip".
- [ ] **T17.5** Commit `feat(app): content manifest display and re-extract action in settings (F17)`.

## Final

- [ ] **T-M4** Build local + smoke test do `.zip` em uma pasta limpa. ROADMAP marca M4 done. Commit `docs: mark M4 complete`.
