# Tasks: Plan Closure (M5)

> Status: **completed (MVP)** — F25 (refactor de services) deferido como opcional desde o início.

Spec: [`../specs/m5-plan-closure.md`](../specs/m5-plan-closure.md)

## F18 — HP máximo
- [x] **T18.1** TDD `HitPointCalculatorTests` — 5 cenários (L1, com rolls, sem rolls, multiclass, unknown class).
- [x] **T18.2** `Core/Rules/HitPointCalculator.cs` com delegate de lookup pra Faces.
- [x] **T18.3** Widget na `SheetTab`: HP máximo / Proficiência / Nível total como cards.
- [x] **T18.4** Commit `feat(core): hit point calculator + HP/Prof/Level widgets in sheet (F18)`.

## F24 — Recarregar catálogo
- [x] **T24.1** `CatalogProvider.Reload()` + `ICatalogSource` interface; repos refatorados para propagar reload sem restart.
- [x] **T24.2** Botão "↻ Recarregar catálogo" em Settings.
- [x] **T24.3** Commit incluído em `feat(app): reload catalog button + ... (F24+F26)`.

## F26 — Auto-reabrir
- [x] **T26.1** `CharacterView.OnParametersSet` grava `Cfg.LastOpenedCharacterId`.
- [x] **T26.2** Home mostra "↻ Reabrir <Nome>" quando há last id válido.
- [x] **T26.3** Commit incluído em `feat(app): reload catalog button + ... (F24+F26)`.

## F22 — Painel lateral
- [x] **T22.1** `RaceSelectStep` em layout 2 colunas com `EntryDisplay` da seleção.
- [x] **T22.2** `ClassSelectStep` e `BackgroundSelectStep` idem.
- [x] **T22.3** Commit `feat(app): side preview panel in race/class/background steps (F22)`.

## F23 — ProficiencyResolver (saves)
- [x] **T23.1** TDD `ProficiencyResolverTests` (2 cenários: Wizard saves + multiclass usa 1ª classe).
- [x] **T23.2** `Core/Rules/ProficiencyResolver.cs` com `SaveTable` record.
- [x] **T23.3** Widget de saves na `SheetTab` com ★ pra proficient.
- [x] **T23.4** Commit `feat(core): proficiency resolver for saves + sheet widget (F23)`.

## F21 — Ability methods
- [x] **T21.1** TDD `PointBuyTests` — cost table, scores válidos, total para arrays canônicos.
- [x] **T21.2** `Core/Rules/PointBuy.cs` + `AbilityRoller.cs` (3 testes deterministic seed).
- [x] **T21.3** `AbilitiesStep` com pills: Manual / Standard array / Point-buy / Roll.
- [x] **T21.4** UI Point-buy: contador "X/27" com cor (info/success/danger).
- [x] **T21.5** UI Roll: botão 🎲 gera 6 valores, dropdown por ability.
- [x] **T21.6** Commit `feat(app): point-buy and roll ability score methods (F21)`.

## F20 — Spells tab
- [x] **T20.1** `Components/Shared/SpellPickerModal.razor`.
- [x] **T20.2** `Components/Pages/CharacterViewParts/SpellsTab.razor` com slots/cantrips/spells/prepared/DC/atq.
- [x] **T20.3** Aba "Spells" em `CharacterView`.
- [x] **T20.4** Commit `feat(app): spells tab with slots, known, prepared, and spell picker (F20)`.

## F19 — Level-ups na criação
- [x] **T19.1** Step "Escolhas por nível" condicional a `InitialLevel > 1`.
- [x] **T19.2** Accordion por nível 2..N com HP input e ASI form quando aplicável.
- [x] **T19.3** `CharacterDraft.HitPointRolls` + `LevelAsi` adicionados; `ToCharacter()` propaga.
- [x] **T19.4** Commit `feat(app): per-level decisions step during high-level creation (F19)`.

## F25 — Refactor services *(deferido)*
- [ ] **T25.1** Adiado — sem regressão funcional, é estilo.
- [ ] **T25.2** Idem.
- [ ] **T25.3** Idem.

## Final
- [x] **T-M5** 145 testes verdes + build OK + ROADMAP marca M5.
