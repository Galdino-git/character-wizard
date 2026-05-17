# Tasks: Wizard Enrichment (F4–F7)

Spec: [`../specs/wizard-enrichment.md`](../specs/wizard-enrichment.md) · Design: [`../design/wizard-enrichment.md`](../design/wizard-enrichment.md)

## F4 — Subrace

- [x] **T4.1** Estender `CharacterDraft` com `SubraceName` (e mapear em `ToCharacter`).
- [x] **T4.2** Criar `Components/Wizard/SubraceSelectStep.razor` — grid de subraças via `RaceRepo.SubracesOf(Draft.RaceRef)`. Botão ⓘ detalhes (reusar `EntityDetailPanel` com category `race`).
- [x] **T4.3** Refatorar `NewCharacter.razor` para passos dinâmicos: adicionar "Subraça" condicional após Raça.
- [x] **T4.4** Smoke build + commit `feat(app): subrace selection step (F4)`.

## F5 — Subclass

- [ ] **T5.1** TDD: `ClassData_SubclassChoiceLevel_returns_correct_level` (Wizard=2, Cleric=1, Fighter=3, Sorcerer=1, Warlock=1).
- [ ] **T5.2** Criar extension `ClassDataExtensions.SubclassChoiceLevel()` parseando `classFeatures` JsonElement.
- [ ] **T5.3** Estender `CharacterDraft.SubclassRef`.
- [ ] **T5.4** Criar `Components/Wizard/SubclassSelectStep.razor`.
- [ ] **T5.5** Adicionar passo condicional em `NewCharacter.razor` (só quando `InitialLevel >= subclassLevel`).
- [ ] **T5.6** Commit `feat(app): subclass selection step when initial level qualifies (F5)`.

## F6 — Initial Spells

- [ ] **T6.1** TDD: `SpellRepository_ForClassAtLevel_filters_by_class` e `..._filters_by_level` (usando catálogo real).
- [ ] **T6.2** Adicionar método `SpellRepository.ForClassAtLevel(className, classSource, spellLevel)`.
- [ ] **T6.3** TDD: helpers em `ClassDataExtensions` — `CantripsKnownAtLevel(level)` e `SpellsKnownAtLevel(level)` (lendo `cantripProgression` e `spellsKnownProgressionFixed`).
- [ ] **T6.4** Estender `CharacterDraft.KnownSpells`.
- [ ] **T6.5** Criar `Components/Wizard/SpellSelectStep.razor` — tabs por nível de spell, contadores, checkboxes.
- [ ] **T6.6** Adicionar passo condicional em `NewCharacter.razor` (`IsCaster(class)`).
- [ ] **T6.7** Commit `feat(app): initial spell selection for spellcasting classes (F6)`.

## F7 — Initial Equipment

- [ ] **T7.1** Estender `CharacterDraft.Inventory` (list de `InventoryItem`).
- [ ] **T7.2** Criar `Components/Wizard/EquipmentStep.razor` — texto bruto de `class.startingEquipment` e `background.startingEquipment` (renderizado via `EntryRenderer` quando estruturado, ou stringify simples). Botão "+ Adicionar item" usando `ItemPickerModal` existente. Lista de itens já adicionados com remover.
- [ ] **T7.3** Adicionar passo antes da Revisão em `NewCharacter.razor`.
- [ ] **T7.4** Commit `feat(app): initial equipment step with picker and starting equipment hints (F7)`.

## Final

- [ ] **T-Final** `dotnet test` verde + smoke build + atualizar ROADMAP marcando F4-F7. Commit `docs: mark F4-F7 complete`.
