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

---

## ✅ F9 — Character Lifecycle Polish (entregue 2026-05-17)

Spec: [`docs/specs/character-lifecycle-polish.md`](specs/character-lifecycle-polish.md) · Tasks: [`docs/tasks/character-lifecycle-polish.md`](tasks/character-lifecycle-polish.md)

- [x] **F9.1** `FeatPickerModal` integrado no level-up (sem input manual obrigatório)
- [x] **F9.2** Spells aprendidos no level-up (deltas de cantrips/spells por classe + caster level table; respeita quotas)
- [x] **F9.3** Features de classe/subclass ganhas mostradas no level-up modal e listadas no `SheetTab` (`ClassDataExtensions.FeaturesAtLevel`/`FeaturesUpToLevel`/`SubclassDataExtensions.FeaturesAtLevel` — 3 testes)
- [x] **F9.4** Multiclass — radio "Subir existente | Adicionar classe" + dropdown de classes (excluindo as já no personagem); cria novo `CharacterClassEntry` quando aplicado; subclass forçada quando classe nova tem sub no nv 1

### Métrica
**118 testes verdes**.

---

## ✅ M3 — UX e polish (entregue 2026-05-17)

Spec: [`docs/specs/m3-configurations-and-polish.md`](specs/m3-configurations-and-polish.md) · Tasks: [`docs/tasks/m3-configurations-and-polish.md`](tasks/m3-configurations-and-polish.md)

- [x] **F10** Settings UI em `/settings` — toggles de groups + per-source overrides, contadores ao vivo, NavMenu link
- [x] **F11** Dark theme — `cw-theme.css`/`.js` + toggle no `MainLayout`, persistido em `AppSettings.Theme`
- [x] **F12** Export/Import JSON — botão "Exportar" no `CharacterView` (JS Blob download), `<InputFile>` na Home pra importar com novo `Id`
- [x] **F13** Resolver `_copy` — `CopyResolver.Merge` (4 testes TDD) + integração no `CatalogLoader` com 2-pass (index → resolve → deserialize). Subraças derivadas (Eladrin etc.) agora aparecem
- [x] **F14** Multiclass no wizard inicial — step "Multiclass" opcional após classe primária, lista editável de classes secundárias com nível + subclass inline, total agregado

### Métrica
**122 testes verdes** (+4 `CopyResolver`).

### Descopo definitivo
- Telemetria local — fora do escopo de ferramenta pessoal.

## 🔄 M4 — Distribuição (planejada)

Spec: [`docs/specs/m4-distribution.md`](specs/m4-distribution.md) · Design: [`docs/design/m4-distribution.md`](design/m4-distribution.md) · Tasks: [`docs/tasks/m4-distribution.md`](tasks/m4-distribution.md)

Objetivo: virar `.exe` self-contained Windows distribuível com dados embarcados e fluxo de update documentado.

### F15 — Windows packaging
- [ ] `dotnet publish` self-contained `win-x64` validado em Release
- [ ] `docs/release.md` com instruções + troubleshooting (WebView2, antivírus, espaço)
- [ ] `tools/Build-Release.ps1` (import dados → empacota zip de conteúdo → publish → zip de distribuição)
- [ ] Validação em VM Windows limpa

### F16 — Embedded data + first-run extraction
- [ ] `cw-content.zip` único embarcado via `MauiAsset` (em vez de muitos arquivos soltos)
- [ ] `ContentBootstrapper` extrai pra `%AppData%/CharacterWizard/content/` no startup, com check de hash
- [ ] `AppPaths.DataRoot` aponta para AppData com fallback dev via env `CW_DATA_OVERRIDE`
- [ ] TDD: extraction / skip / re-extract scenarios

### F17 — Manual content update flow
- [ ] Seção "Conteúdo do 5etools" no Settings mostrando manifesto e botão "Reextrair"
- [ ] CLI `Import5eToolsData` segue funcional para devs
- [ ] README explica upgrade = baixar novo zip

### Descopo (registrado na spec)
- MSIX e Microsoft Store
- Auto-update do app via internet
- Cross-platform (macOS/Linux/mobile)
- Assinatura comercial
