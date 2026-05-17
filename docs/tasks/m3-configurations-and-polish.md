# Tasks: Configurations & Polish (F10–F14)

Spec: [`../specs/m3-configurations-and-polish.md`](../specs/m3-configurations-and-polish.md) · Design: [`../design/m3-configurations-and-polish.md`](../design/m3-configurations-and-polish.md)

## F10 — Settings UI

- [ ] **T10.1** Página `Components/Pages/Settings.razor` em `/settings` com toggles de groups + books.
- [ ] **T10.2** Botão "Salvar e aplicar" persiste via `AppSettingsStore` + `CatalogProvider.ApplySettings`.
- [ ] **T10.3** NavMenu link "Configurações".
- [ ] **T10.4** Commit `feat(app): settings page with source toggles (F10)`.

## F11 — Dark theme

- [ ] **T11.1** `AppSettings.Theme` (string, default "light").
- [ ] **T11.2** `wwwroot/cw-theme.css` com overrides para componentes principais.
- [ ] **T11.3** `wwwroot/cw-theme.js` mínimo (`cwTheme.apply(t)`).
- [ ] **T11.4** Toggle no `MainLayout.razor` + auto-apply ao startup.
- [ ] **T11.5** Commit `feat(app): dark theme toggle (F11)`.

## F12 — Export/Import

- [ ] **T12.1** `wwwroot/cw-io.js` (`cwIo.download(name, content)`).
- [ ] **T12.2** Botão "Exportar JSON" no `CharacterView` (chama JS via `IJSRuntime`).
- [ ] **T12.3** Botão "Importar JSON" na `Home` com `<InputFile>`, deserializa, `c.Id = Guid.NewGuid()`, salva.
- [ ] **T12.4** Commit `feat(app): export/import character JSON (F12)`.

## F13 — `_copy` resolution

- [ ] **T13.1** TDD `CopyResolverTests` — `merges_overrides`, `child_array_replaces_parent`, `nested_object_merge`.
- [ ] **T13.2** Implementar `CharacterWizard.Data/Loading/CopyResolver.cs` (`Merge(parent, child) → JsonElement`).
- [ ] **T13.3** Refatorar `CatalogLoader.LoadCollection<T>` para duas passes: index → resolve copies → deserialize.
- [ ] **T13.4** TDD: `CatalogLoader_loads_subrace_copy` (ex.: Eladrin pra Elf, ou outra subraça `_copy`).
- [ ] **T13.5** Commit `feat(data): resolve _copy entries in races and subraces (F13)`.

## F14 — Multiclass no wizard inicial

- [ ] **T14.1** Refatorar `CharacterDraft`: trocar `ClassRef/SubclassRef/InitialLevel` por `List<CharacterClassEntry> Classes`. Adapter `ToCharacter`. Manter `InitialLevel` como prop derivada.
- [ ] **T14.2** Refatorar `ClassSelectStep` para mostrar lista + "Adicionar classe"; renderizar inline subclass + nível por entry.
- [ ] **T14.3** Atualizar `SubclassSelectStep`/`SpellSelectStep` ou marcá-los obsoletos (em favor do inline).
- [ ] **T14.4** Atualizar `BuildSteps()` em `NewCharacter.razor` para a nova estrutura.
- [ ] **T14.5** Commit `feat(app): multiclass at character creation (F14)`.

## Final

- [ ] **T-M3** `dotnet test` verde + smoke build + ROADMAP marcando M3 done. Commit `docs: mark M3 complete`.
