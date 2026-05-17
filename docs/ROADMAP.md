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

## 🔄 M2 — Pós-criação e level-up

### ✅ F3 — Post-creation editing (entregue 2026-05-17)

Spec: [`docs/specs/post-creation-editing.md`](specs/post-creation-editing.md) · Tasks: [`docs/tasks/post-creation-editing.md`](tasks/post-creation-editing.md)

- [x] `CharacterView` com tabs (`Ficha` / `Inventário` / `Buffs & Notas`)
- [x] CRUD de inventário com bônus customizados (CustomName, BonusAttack/Damage/AC, Equipped, Notes)
- [x] `ItemPickerModal` integrado com `SearchService` (categoria Item)
- [x] `ItemEditorModal` para edição completa
- [x] CRUD de `AbilityOverride` (delta + reason) com agregado por ability
- [x] Editor de notas livres (textarea persistido em `Char.Notes`)
- [x] Save imediato + backup automático em `_history/` (4 testes novos: 86 verdes total)

### Pendente

- [ ] Wizard de level-up de personagem existente (escolha de class para subir, HP, ASI/feat, spells aprendidos)
- [ ] Seleção de subclass no wizard de criação (quando o nível inicial >= nível de subclass)
- [ ] Seleção de subraça
- [ ] Seleção de spells iniciais (cantrips + 1st level)
- [ ] Seleção de equipamento inicial

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
