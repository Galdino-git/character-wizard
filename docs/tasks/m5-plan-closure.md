# Tasks: Plan Closure (M5)

Spec: [`../specs/m5-plan-closure.md`](../specs/m5-plan-closure.md)

Ordem de execução por valor × complexidade.

## F18 — HP máximo
- [ ] **T18.1** TDD `HitPointCalculatorTests`: nv 1 d10 con+2 = 12; nv 5 com rolls [6,5,8,7] + con+2 = 10+2 + 26 + 4×2 = 46; sem rolls (nv alto sem level-up) usa média = 6 por nível após o 1º.
- [ ] **T18.2** Implementar `Core/Rules/HitPointCalculator.cs`.
- [ ] **T18.3** Widget na `SheetTab`: "HP máximo: X".
- [ ] **T18.4** Commit `feat(core): hit point calculator + sheet display (F18)`.

## F24 — Recarregar catálogo
- [ ] **T24.1** Adicionar `CatalogProvider.Reload()` (re-roda loader).
- [ ] **T24.2** Botão "Recarregar catálogo do disco" em Settings.
- [ ] **T24.3** Commit `feat(app): reload catalog from disk action in settings (F24)`.

## F26 — Auto-reabrir
- [ ] **T26.1** `CharacterView` grava `Cfg.LastOpenedCharacterId` ao abrir.
- [ ] **T26.2** Home mostra botão "Reabrir <Nome>" se settings tem id válido.
- [ ] **T26.3** Commit `feat(app): track and surface last opened character (F26)`.

## F22 — Painel lateral
- [ ] **T22.1** Refator `RaceSelectStep` pra layout 2 colunas (grid esquerda + preview direita via EntryDisplay).
- [ ] **T22.2** Idem `ClassSelectStep` e `BackgroundSelectStep`.
- [ ] **T22.3** Commit `feat(app): side preview panel in race/class/background steps (F22)`.

## F23 — ProficiencyResolver (saves)
- [ ] **T23.1** TDD `ProficiencyResolverTests`: Wizard tem saves INT+WIS proficient; Fighter STR+CON; multiclass usa proficient set da primeira classe (RAW).
- [ ] **T23.2** `Core/Rules/ProficiencyResolver.cs` (saves).
- [ ] **T23.3** Widget na `SheetTab` mostrando saves com asterisco para proficient.
- [ ] **T23.4** Commit `feat(core): proficiency resolver for saves + sheet display (F23)`.

## F21 — Ability methods
- [ ] **T21.1** TDD `PointBuyTests`: cost(8)=0, cost(13)=5, cost(14)=7, cost(15)=9; total para 15/15/15/8/8/8 = 27.
- [ ] **T21.2** `Core/Rules/PointBuy.cs` com `Cost(score)` e `TotalCost(scores)`.
- [ ] **T21.3** `AbilitiesStep` ganha tabs/buttons: Manual / Standard / Point-buy / Roll.
- [ ] **T21.4** UI Point-buy: inputs 8-15, mostra "Pontos: 14/27", desabilita +/- quando estoura.
- [ ] **T21.5** UI Roll: botão "Rolar 4d6 drop lowest × 6" gera lista de 6 valores; assign via select por ability.
- [ ] **T21.6** Commit `feat(app): point-buy and roll ability score methods (F21)`.

## F20 — Spells tab
- [ ] **T20.1** Criar `Components/Shared/SpellPickerModal.razor` (gêmeo de FeatPickerModal, filtro Spell).
- [ ] **T20.2** Criar `Components/Pages/CharacterViewParts/SpellsTab.razor`:
    - Tabela de spell slots (`SpellSlotTable.GetSlotRow(totalCasterLevel)`).
    - Listas: Cantrips, Spells (agrupados por nível).
    - Para classes preparation (Wizard/Cleric/Druid/Paladin/Artificer): checkbox "preparada" + contador `level+abilityMod`.
    - Botão "+ Aprender spell" abre picker.
    - Remover spell.
- [ ] **T20.3** Adicionar aba "Spells" em `CharacterView`.
- [ ] **T20.4** Commit `feat(app): spells tab with slots, known, prepared (F20)`.

## F19 — Level-ups na criação
- [ ] **T19.1** Novo step no wizard: "Escolhas por nível" condicional a `Draft.InitialLevel > 1`.
- [ ] **T19.2** Renderiza um painel por nível 2..InitialLevel da classe primária com formulários inline:
    - HP (mode + value)
    - ASI/feat se IsAsiLevel(level)
    - Spells aprendidos se caster (delta calculado)
- [ ] **T19.3** `Draft.ToCharacter()` propaga rolls e ChoicesByLevel.
- [ ] **T19.4** Commit `feat(app): per-level decisions step during high-level creation (F19)`.

## F25 — Refactor services *(opcional, último)*
- [ ] **T25.1** Mover regras do `LevelUpModal.Apply` para `Core/Services/LevelUpService.cs`.
- [ ] **T25.2** Idem `CharacterDraft.ToCharacter` → `CharacterCreationService`.
- [ ] **T25.3** Commit `refactor(core): extract level-up and creation services (F25)`.

## Final
- [ ] **T-M5** Tests verdes + smoke build + ROADMAP marca M5. Commit `docs: mark M5 complete`.
