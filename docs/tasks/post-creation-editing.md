# Tasks: Post-Creation Editing (F3)

Spec: [`../specs/post-creation-editing.md`](../specs/post-creation-editing.md) · Design: [`../design/post-creation-editing.md`](../design/post-creation-editing.md)

Cada task tem arquivos e critério de done. Commit atômico por task. Marque `[x]` quando done.

## F3.1 — Testes de round-trip expandidos (TDD)

- [x] **T3.1.1** Em `tests/CharacterWizard.Tests/CoreTests.cs`, adicionar testes:
    - `CharacterStore_preserves_multiple_inventory_items_with_bonuses`
    - `CharacterStore_preserves_ability_overrides_list`
    - `CharacterStore_save_creates_history_entry_on_overwrite`
    - `CharacterStore_history_keeps_at_most_five_versions`
- [x] **T3.1.2** Verify: testes verdes (o model já suporta; valida que nada quebra).
- [x] **T3.1.3** Commit: `test(core): expand CharacterStore round-trip coverage (F3.1)`.

## F3.2 — CharacterView com estrutura de tabs

- [x] **T3.2.1** Refatorar `Components/Pages/CharacterView.razor`: introduzir nav-tabs Bootstrap com 3 abas (`Ficha` / `Inventário` / `Buffs & Notas`). Variável `_activeTab`. Conteúdo da Ficha atual fica intacto na primeira aba.
- [x] **T3.2.2** Smoke build OK.
- [x] **T3.2.3** Commit: `refactor(app): CharacterView tabs structure (F3.2)`.

## F3.3 — Tab Inventário com CRUD + ItemPicker

- [x] **T3.3.1** Criar `Components/Shared/ItemPickerModal.razor` — modal com input + resultados de `SearchService.Search(query, ["Item"])`. Callback `EventCallback<EntityRef> OnPick`.
- [x] **T3.3.2** Criar `Components/Shared/ItemEditorModal.razor` — form com `CustomName`, `Quantity`, `BonusAttack`, `BonusDamage`, `BonusAc`, `Equipped`, `Notes`. Callbacks `OnSave` / `OnRemove` / `OnCancel`.
- [x] **T3.3.3** Criar `Components/Pages/CharacterView/InventoryTab.razor` — tabela de items + botão Adicionar (abre picker) + click em row (abre editor). Chama `Store.Save` após cada mudança.
- [x] **T3.3.4** Wirear `InventoryTab` em `CharacterView`.
- [x] **T3.3.5** Commit: `feat(app): inventory tab with item picker and editor (F3.3)`.

## F3.4 — Tab Buffs com CRUD de AbilityOverrides

- [ ] **T3.4.1** Criar `Components/Pages/CharacterView/BuffsTab.razor` — lista de `AbilityOverride`s + form inline pra adicionar (dropdown de Ability, input numérico de Delta, input de Reason). Botão remover por linha.
- [ ] **T3.4.2** Mostrar agregado por ability ("CHA total: +2 de buffs").
- [ ] **T3.4.3** Salvar imediatamente após cada change.
- [ ] **T3.4.4** Wirear em `CharacterView`.
- [ ] **T3.4.5** Commit: `feat(app): buffs tab with ability overrides CRUD (F3.4)`.

## F3.5 — Notas livres

- [ ] **T3.5.1** Adicionar `<textarea>` bindando `Char.Notes` na tab Buffs & Notas. Salvar onblur.
- [ ] **T3.5.2** Commit: `feat(app): free-form notes editor in BuffsTab (F3.5)`.

## F3.6 — Build final + ROADMAP

- [ ] **T3.6.1** `dotnet test` — todos verdes.
- [ ] **T3.6.2** `dotnet build src/CharacterWizard.App` — OK.
- [ ] **T3.6.3** Atualizar `docs/ROADMAP.md` marcando F3 done.
- [ ] **T3.6.4** Commit: `docs: mark F3 complete in ROADMAP`.
