# CharacterWizard — Roadmap

Visão de alto nível do que foi entregue e o que está planejado. Detalhes de cada feature: `docs/specs/` + `docs/tasks/`.

> Marque `[x]` ao concluir um item. Mantenha esta visão sincronizada — é a fonte de verdade do progresso, não o contexto da conversa.

---

## ✅ M0 — Setup e MVP do Core (entregue 2026-05-17)

- [x] Repo git, solution `.slnx`, 5 projetos (App, Core, Data, Tests, Import5eToolsData)
- [x] `Import5eToolsData` console que copia 51 JSONs + 1414 imagens (175 MB) do repo `..\5etools`
- [x] `CharacterWizard.Data`: models, `CatalogLoader`, `SourceFilter`, 7 repositórios, `ClassFeatureRefParser`
- [x] `CharacterWizard.Core`: `Character`, `AbilityScores`, regras (`AbilityScoreCalculator`, `ProficiencyBonus`, `SpellSlotTable`), `CharacterStore` (JSON em `%AppData%`), `AppSettings`
- [x] `CharacterWizard.App` MAUI Blazor Hybrid (Windows): Home, wizard 6 passos (Origem/Raça/Classe/Background/Atributos/Revisão), CharacterView
- [x] `ImageService` (WebP via data URL) — imagens de Race/Class/Background aparecem no wizard
- [x] 26 testes xUnit verdes (carga real do catálogo, parser, regras, persistência)
- [x] CLAUDE.md com setup e convenções

---

## ✅ M1 — Compendium Search + Tooltip System (entregue 2026-05-17)

Especificação: [`docs/specs/compendium-search.md`](specs/compendium-search.md) · [`docs/specs/tooltip-system.md`](specs/tooltip-system.md)
Tasks: [`docs/tasks/compendium-search.md`](tasks/compendium-search.md) · [`docs/tasks/tooltip-system.md`](tasks/tooltip-system.md)

### Tooltip System (F2)

- [x] `EntryRenderer` (`CharacterWizard.Data/EntryRendering/`) processando `JsonElement` (entries) e strings → HTML safe (XSS escape em todo input)
- [x] Tags inline suportadas: `@b @bold @i @italic @u @underline @spell @item @condition @feat @creature @skill @action @sense @feature @classFeature @subclassFeature @optfeature @damage @dice @scaledamage @scaledice @hit @atk @dc @h @note`
- [x] Estruturas aninhadas: `entries`, `list`, `inset`, `table`, `quote`, arrays e strings
- [x] `EntityResolver` (`Data/Resolving/`) — despacha refs aos repos existentes
- [x] `<EntryDisplay>` componente Blazor — renderiza entries + delegation de click
- [x] `<EntityDetailPanel>` — modal com header + imagem + body via `EntryDisplay`. Navegação push-over para refs aninhadas
- [x] Hover tooltip CSS-only (`cw-tooltip.css`, helper JS mínimo em `cw-ref.js`)
- [x] Integrado nos passos do wizard (Race/Class/Background) via botão "ⓘ detalhes"

### Compendium Search (F1)

- [x] `SearchService` (`Data/Search/`) — busca substring case-insensitive, ranking prefix > contains, filtros categoria/source, paginação
- [x] Página `Search.razor` (`/search`) — input com debounce 200ms, chips de categoria, lista clicável
- [x] Click em resultado abre `<EntityDetailPanel>`
- [x] NavMenu com link "Compêndio"
- [x] Assist no wizard (`NewCharacter.razor`) — botão 🔍 abre overlay de busca sem perder draft

### Cobertura de testes

82 testes xUnit verdes:
- 8 base de `EntryRenderer` (b/i/u + escape)
- 16 entity-ref tags
- 9 formatting tags
- 9 structured entries
- 5 `EntityResolver`
- 9 `SearchService`
- 26 anteriores (catalog loader, character store, regras de jogo)

---

## ✅ M2 — Pós-criação e level-up (entregue 2026-05-17)

### F3 — Post-creation editing
Spec: [`docs/specs/post-creation-editing.md`](specs/post-creation-editing.md) · Tasks: [`docs/tasks/post-creation-editing.md`](tasks/post-creation-editing.md)

- [x] `CharacterView` com tabs (`Ficha` / `Inventário` / `Buffs & Notas`)
- [x] CRUD de inventário com bônus customizados via `ItemPickerModal` + `ItemEditorModal`
- [x] CRUD de `AbilityOverride` com agregado por ability
- [x] Editor de notas livres
- [x] Save imediato + backup automático em `_history/`

### F4 — Subrace
- [x] `SubraceSelectStep` condicional após Race; `Draft.SubraceName` persistido

### F5 — Subclass
- [x] `ClassDataExtensions.SubclassChoiceLevel()` parseia `classFeatures` (8 testes)
- [x] `SubclassSelectStep` condicional quando nível inicial qualifica

### F6 — Initial spells
- [x] `ClassSpellListIndex` carregando `gendata-spell-source-lookup.json`
- [x] `SpellRepository.ForClassAtLevel(class, level)` (3 testes)
- [x] `SpellSelectStep` com tabs por nível e cantrips/spells quotas

### F7 — Initial equipment
- [x] `EquipmentStep` mostra `startingEquipment` da classe e background via `EntryRenderer`
- [x] Reusa `ItemPickerModal` para adicionar itens livres

### F8 — Level-up
Spec: [`docs/specs/level-up.md`](specs/level-up.md)
- [x] `LevelUpRules` (`AverageHpPerLevel`, `IsAsiLevel`, `HpGainedAtLevel`) — 6 testes
- [x] `LevelUpModal` com HP médio/máximo/manual, prompt de subclass quando aplicável, prompt de ASI/feat em níveis 4/8/12/16/19
- [x] Atualiza `CharacterClassEntry.Levels`, `HitPointRolls`, `SubclassRef` e `Character.ChoicesByLevel`

### Métrica
**115 testes verdes** (86 → 115: +12 LevelUp/ASI/HP, +8 SubclassLevel, +3 progressions, +3 ForClassAtLevel, +3 itens extras)

---

### Pendente (movido para M3+)

- [ ] Multiclass (subir para classe diferente da atual)
- [ ] Auto-aplicar features de classe/subclass no level-up (hoje só registra `Levels` e `SubclassRef`)
- [ ] Pick de feat com modal de pesquisa (hoje aceita name/source manual no level-up)
- [ ] Spells aprendidos no level-up (hoje passo de criação só; usuário edita via aba após)

## 🗺️ M3 — UX e polish (a especificar)

- [ ] Tela de Configurações: toggles de sources (grupos do `books.json`)
- [ ] Tema dark
- [ ] Export/Import de personagem em JSON
- [ ] Suporte a multiclass no wizard
- [ ] Resolver `_copy` entries do 5etools (raças e subclasses que herdam de outra)
- [ ] Telemetria local (qual class é mais escolhida, etc.) — opcional

## 🗺️ M4 — Distribuição (a especificar)

- [ ] Empacotar como MSIX ou single-file `.exe` instalável
- [ ] Embutir dados via `MauiAsset` em vez de pasta `data/` lado a lado
- [ ] Auto-update dos dados quando o repo 5etools mudar
