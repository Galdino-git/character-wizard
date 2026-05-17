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

## 🔄 M1 — Compendium Search + Tooltip System (em planejamento)

Especificação: [`docs/specs/compendium-search.md`](specs/compendium-search.md) · [`docs/specs/tooltip-system.md`](specs/tooltip-system.md)
Tasks: [`docs/tasks/compendium-search.md`](tasks/compendium-search.md) · [`docs/tasks/tooltip-system.md`](tasks/tooltip-system.md)

### Tooltip System (Feature 2)

- [ ] `EntryRenderer` em `CharacterWizard.Data/EntryRendering/` que aceita `JsonElement` (entries) e retorna `MarkupString`
- [ ] Suporte às tags inline do MVP: `@spell @item @condition @feat @creature @skill @action @sense @feature @classFeature @damage @dice @b @i @u`
- [ ] HTML-safe (escaping correto contra XSS)
- [ ] Suporte a estruturas aninhadas: `entries` recursivo, `list`, `table`, `inset`
- [ ] `EntityResolver` service: dado `(category, name, source)`, retorna a entidade
- [ ] `<EntityRefSpan>` componente Blazor: renderiza mention com tooltip on hover (CSS-only, sem JS interop)
- [ ] `<EntityDetailPanel>` componente Blazor compartilhado com Search: card detalhado da entidade
- [ ] Click em mention abre o `EntityDetailPanel` em modal/sidebar
- [ ] Integração: `EntryDisplay.razor` consome tudo acima e é usado por todas as telas que mostram descrições

### Compendium Search (Feature 1)

- [ ] `SearchService` em `CharacterWizard.App/Services/`: query com filtros (categoria, source) e ordenação
- [ ] Página `Search.razor` em `/search`: input de texto + chips de filtros + lista virtualizada de resultados
- [ ] Click em resultado → `EntityDetailPanel` (mesmo componente do tooltip)
- [ ] Botão "Buscar" disponível no NavMenu e dentro do wizard (assist)
- [ ] Componente `<CompendiumSearchBox>` reutilizável (autocomplete leve)

### Cross-cutting

- [ ] Documentação dos `data-` attributes usados pelo `EntityRefSpan` em `CONVENTIONS.md` (se necessário)
- [ ] Testes xUnit (TDD) para `EntryRenderer`, `EntityResolver`, `SearchService`

---

## 🗺️ M2 — Pós-criação e level-up (a especificar)

- [ ] Edição de inventário (CRUD de items, custom name, bonus attack/damage/AC)
- [ ] Edição de overrides de ability (buffs permanentes)
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
