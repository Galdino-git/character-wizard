# Tasks: Configurations & Polish (F10–F14)

> Status: **completed (MVP)**.

Spec: [`../specs/m3-configurations-and-polish.md`](../specs/m3-configurations-and-polish.md) · Design: [`../design/m3-configurations-and-polish.md`](../design/m3-configurations-and-polish.md)

## F10 — Settings UI

- [x] **T10.1** Página `Components/Pages/Settings.razor` em `/settings` com toggles de groups + books.
- [x] **T10.2** Botão "Salvar e aplicar" persiste via `AppSettingsStore` + `CatalogProvider.ApplySettings`.
- [x] **T10.3** NavMenu link "Configurações".
- [x] **T10.4** Commit `feat(app): settings page with source toggles (F10)`.

## F11 — Dark theme

- [x] **T11.1** `AppSettings.Theme` (string, default "light").
- [x] **T11.2** `wwwroot/cw-theme.css` cobrindo cards, modais, tabelas, tabs, forms, cw-* customs.
- [x] **T11.3** `wwwroot/cw-theme.js` (`cwTheme.apply(t)`).
- [x] **T11.4** Toggle no `MainLayout.razor` + auto-apply ao startup via `OnAfterRenderAsync`.
- [x] **T11.5** Commit `feat(app): dark theme toggle (F11)`.

## F12 — Export/Import

- [x] **T12.1** `wwwroot/cw-io.js` (`cwIo.download(name, content)` via Blob URL).
- [x] **T12.2** Botão "Exportar JSON" no `CharacterView`.
- [x] **T12.3** Botão "Importar JSON" na `Home` com `<InputFile>`; novo `Guid.NewGuid()` no import pra evitar colisão.
- [x] **T12.4** Commit `feat(app): export/import character JSON (F12)`.

## F13 — `_copy` resolution

- [x] **T13.1** TDD `CopyResolverTests` (4 cenários: override, add, array replace, _copy stripped).
- [x] **T13.2** `CharacterWizard.Data/Loading/CopyResolver.cs` (`Merge(parent, child) → JsonElement` + `GetCopyRef`).
- [x] **T13.3** `CatalogLoader.LoadCollection<T>` agora faz 2-pass: index → resolve copies → deserialize.
- [x] **T13.4** Children que apontam para parent inexistente são silenciosamente droppadas.
- [x] **T13.5** Commit `feat(data): resolve _copy entries in CatalogLoader (F13)`.

## F14 — Multiclass no wizard inicial

- [x] **T14.1** `CharacterDraft.AdditionalClasses` (List) sem quebrar `ClassRef` primária; `ToCharacter` concatena.
- [x] **T14.2** Novo `Components/Wizard/MulticlassStep.razor` com lista editável + form de adicionar (classe + nível + subclass inline quando aplicável).
- [x] **T14.3** Passo "Multiclass" adicionado dinamicamente em `BuildSteps()` depois da classe primária estar escolhida.
- [x] **T14.4** Total de níveis agregado (primária + adicionais) mostrado no step.
- [x] **T14.5** Commit `feat(app): multiclass step at character creation (F14)`.

## Final

- [x] **T-M3** 122 testes verdes + build OK + ROADMAP marcando M3 done.
