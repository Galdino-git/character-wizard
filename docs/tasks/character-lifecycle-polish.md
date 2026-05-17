# Tasks: Character Lifecycle Polish (F9)

> Status: **completed (MVP)**.

Spec: [`../specs/character-lifecycle-polish.md`](../specs/character-lifecycle-polish.md) · Design: [`../design/character-lifecycle-polish.md`](../design/character-lifecycle-polish.md)

## F9.1 — Feat picker modal

- [x] **T9.1.1** Criar `Components/Shared/FeatPickerModal.razor` (cópia adaptada de `ItemPickerModal`, filtro "Feat").
- [x] **T9.1.2** Em `LevelUpModal`, integrar botão "🔍 Escolher feat" no bloco Feat.
- [x] **T9.1.3** Commit `feat(app): feat picker modal in level-up (F9.1)`.

## F9.2 — Spells on level-up

- [x] **T9.2.1** Em `LevelUpModal`, bloco condicional pra caster: calcular cantripDelta + spellDelta + listas selecionáveis com cap.
- [x] **T9.2.2** Apply: somar selecionados em `Character.KnownSpells`.
- [x] **T9.2.3** Commit `feat(app): learn new spells during level-up (F9.2)`.

## F9.3 — Features gained

- [x] **T9.3.1** TDD: `ClassDataExtensions.FeaturesAtLevel(level)` (Fighter nv 5 → Extra Attack; Wizard nv 2 → Arcane Tradition; FeaturesUpToLevel cumulativo).
- [x] **T9.3.2** Parser aceita strings e objetos com `classFeature`. Adicionado `SubclassDataExtensions.FeaturesAtLevel` também.
- [x] **T9.3.3** `LevelUpModal` mostra features ganhas; `SheetTab` lista todas até o nível atual (por classe + por subclass).
- [x] **T9.3.4** Commit `feat(app): show class/subclass features gained at level (F9.3)`.

## F9.4 — Multiclass

- [x] **T9.4.1** Radio "Subir existente | Adicionar classe" + dropdown de classes não-presentes.
- [x] **T9.4.2** Apply para multiclass: cria novo `CharacterClassEntry`, adiciona em `Char.Classes`.
- [x] **T9.4.3** Alert info sobre prereqs RAW não verificados; subclass prompt funciona pra classes que pegam sub no nv 1 (Cleric/Sorcerer/Warlock).
- [x] **T9.4.4** Commit `feat(app): multiclass option in level-up modal (F9.4)`.

## Final

- [x] **T9.F** `dotnet test` (118 verdes) + smoke build OK + ROADMAP atualizado.
