# Tasks: Tooltip System (F2)

Spec: [`../specs/tooltip-system.md`](../specs/tooltip-system.md) · Design: [`../design/tooltip-system.md`](../design/tooltip-system.md)

Cada task tem: arquivos afetados · testes · critério de done. **TDD: red → green → refactor.** Commit atômico por task concluída. Marque `[x]` quando done.

## F2.1 — EntryRenderer foundation (inline tags simples)

- [ ] **T2.1.1** Criar `tests/CharacterWizard.Tests/EntryRendererTests.cs` com testes vermelhos:
    - `RenderString_returns_empty_for_null_or_empty`
    - `RenderString_escapes_html_special_chars`
    - `RenderString_wraps_b_tag_as_strong`
    - `RenderString_wraps_i_tag_as_em`
    - `RenderString_wraps_u_tag_as_u`
    - Verify: `dotnet test` — testes falham com "EntryRenderer not found".
- [ ] **T2.1.2** Criar `src/CharacterWizard.Data/EntryRendering/EntryRenderer.cs` com método `RenderString(string text)` que processa `@b @i @u` e escapa HTML. Verify: testes verdes.
- [ ] **T2.1.3** Commit: `feat(data): EntryRenderer base with @b @i @u tags`.

## F2.2 — EntryRenderer: tags de entidade clicáveis

- [ ] **T2.2.1** Adicionar testes vermelhos:
    - `RenderString_renders_spell_tag_as_cw_ref_span`
    - `RenderString_renders_item_with_explicit_source`
    - `RenderString_renders_condition_with_display_alias`
    - `RenderString_renders_pipe_segments_correctly`
    - `RenderString_renders_unknown_tag_with_display_fallback`
- [ ] **T2.2.2** Estender `EntryRenderer` com handlers para `@spell @item @condition @feat @creature @skill @action @sense @feature @classFeature @subclassFeature @optfeature`. Emitir `<span class="cw-ref" data-cw-cat=... data-cw-name=... data-cw-source=...>display</span>`. Tag desconhecida → fallback texto.
- [ ] **T2.2.3** Commit: `feat(data): EntryRenderer supports clickable entity ref tags`.

## F2.3 — EntryRenderer: formato (damage/dice/hit/atk/dc)

- [ ] **T2.3.1** Testes vermelhos:
    - `RenderString_renders_damage_with_dice_formula`
    - `RenderString_renders_dice_tag_with_average_tooltip`
    - `RenderString_renders_hit_tag_as_signed_modifier`
    - `RenderString_renders_dc_tag_as_DC_N`
- [ ] **T2.3.2** Adicionar handlers para `@damage @dice @scaledamage @scaledice @hit @atk @dc @h @note`. Emitir spans com tooltip texto (sem ser entidade clicável).
- [ ] **T2.3.3** Commit: `feat(data): EntryRenderer supports formatting tags`.

## F2.4 — EntryRenderer: estruturas (entries/list/inset/table)

- [ ] **T2.4.1** Testes vermelhos:
    - `Render_processes_string_entry`
    - `Render_processes_entries_object_with_name_header`
    - `Render_processes_list_with_items`
    - `Render_processes_nested_entries`
    - `Render_processes_inset_with_border`
    - `Render_processes_table_basic`
    - `Render_returns_empty_for_unknown_type`
- [ ] **T2.4.2** Implementar `EntryRenderer.Render(JsonElement)` que despacha por `type` e recursivamente renderiza filhos. Tabelas básicas: rows + cells.
- [ ] **T2.4.3** Commit: `feat(data): EntryRenderer handles structured entries`.

## F2.5 — EntityResolver

- [ ] **T2.5.1** Testes vermelhos em `EntityResolverTests`:
    - `Resolve_spell_returns_spell_data`
    - `Resolve_unknown_category_returns_null`
    - `Resolve_missing_entity_returns_null`
- [ ] **T2.5.2** Criar `src/CharacterWizard.Data/Resolving/EntityResolver.cs` com switch por categoria, delegando aos repos. Registrar no DI (App MauiProgram).
- [ ] **T2.5.3** Commit: `feat(data): EntityResolver dispatches refs to repositories`.

## F2.6 — EntryDisplay component (Blazor)

- [ ] **T2.6.1** Criar `src/CharacterWizard.App/Components/Shared/EntryDisplay.razor` com parâmetro `JsonElement? Entries` e callback `EventCallback<EntityRef> OnEntityClicked`. Renderiza `MarkupString` do `EntryRenderer.Render`. Container tem `@onclick` que via JS interop lê `event.target.dataset.cwCat/cwName/cwSource` e dispara callback.
- [ ] **T2.6.2** Criar `wwwroot/cw-tooltip.css` e `wwwroot/cw-ref.js` (mínimo — só uma função `getRefData(el)` chamada do .NET). Importar em `index.html`.
- [ ] **T2.6.3** Smoke-test: abrir uma classe via wizard e ver tags renderizadas.
- [ ] **T2.6.4** Commit: `feat(app): EntryDisplay renders entries with clickable refs`.

## F2.7 — EntityDetailPanel modal

- [ ] **T2.7.1** Criar `src/CharacterWizard.App/Components/Shared/EntityDetailPanel.razor` que mostra: nome, source, page, imagem (se aplicável), `<EntryDisplay>` da descrição. Receber `EntityRef Ref` + `EventCallback OnClose`.
- [ ] **T2.7.2** Modal Bootstrap-like via CSS-class toggle (sem JS bootstrap). Backdrop com click fechando.
- [ ] **T2.7.3** Integrar em `CharacterView.razor` e nos passos do wizard (Race/Class/Background): hover/click nos cards já existentes abre o panel.
- [ ] **T2.7.4** Commit: `feat(app): EntityDetailPanel and click integration`.

## F2.8 — Wire-up no wizard e CharacterView

- [ ] **T2.8.1** Em `RaceSelectStep`, `ClassSelectStep`, `BackgroundSelectStep`: adicionar botão "ⓘ Ver detalhe" no card que abre `EntityDetailPanel`.
- [ ] **T2.8.2** Em `CharacterView`: renderizar descrição da classe/raça/background via `EntryDisplay`.
- [ ] **T2.8.3** Commit: `feat(app): integrate EntityDetailPanel into wizard and character view`.
