# Tasks: Character Lifecycle Polish (F9)

Spec: [`../specs/character-lifecycle-polish.md`](../specs/character-lifecycle-polish.md) · Design: [`../design/character-lifecycle-polish.md`](../design/character-lifecycle-polish.md)

## F9.1 — Feat picker modal

- [ ] **T9.1.1** Criar `Components/Shared/FeatPickerModal.razor` (cópia adaptada de `ItemPickerModal`, filtro "Feat").
- [ ] **T9.1.2** Em `LevelUpModal`, integrar botão "🔍 Escolher feat" no bloco Feat.
- [ ] **T9.1.3** Commit `feat(app): feat picker modal in level-up (F9.1)`.

## F9.2 — Spells on level-up

- [ ] **T9.2.1** Em `LevelUpModal`, adicionar bloco condicional pra caster: calcular cantripDelta + spellDelta, mostrar listas selecionáveis.
- [ ] **T9.2.2** Apply: somar selecionados em `Character.KnownSpells`.
- [ ] **T9.2.3** Commit `feat(app): learn new spells during level-up (F9.2)`.

## F9.3 — Features gained

- [ ] **T9.3.1** TDD: `ClassDataExtensions.FeaturesAtLevel(level)` (Fighter nv 5 → contém "Extra Attack"; Wizard nv 2 → contém "Arcane Tradition").
- [ ] **T9.3.2** Implementar parser que aceita strings e objetos com `classFeature`.
- [ ] **T9.3.3** Em `LevelUpModal`, mostrar lista de features ganhas. Em `SheetTab`, listar todas as features até nível atual.
- [ ] **T9.3.4** Commit `feat(app): show class/subclass features gained at level (F9.3)`.

## F9.4 — Multiclass

- [ ] **T9.4.1** Em `LevelUpModal`, adicionar radio "Existente|Multiclass" e dropdown de classes disponíveis.
- [ ] **T9.4.2** Apply para multiclass: adicionar novo `CharacterClassEntry`.
- [ ] **T9.4.3** Tratar edge cases: subclass forçada em classes que pegam sub no nv 1 (Cleric/Sorcerer/Warlock); alert sobre prereqs.
- [ ] **T9.4.4** Commit `feat(app): multiclass option in level-up modal (F9.4)`.

## Final

- [ ] **T9.F** `dotnet test` verde + smoke build + atualizar ROADMAP. Commit `docs: mark F9 complete`.
