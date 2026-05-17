# Tasks: Compendium Search (F1)

Spec: [`../specs/compendium-search.md`](../specs/compendium-search.md) · Design: [`../design/compendium-search.md`](../design/compendium-search.md)

Dependência forte: F2.5 (`EntityResolver`) e F2.7 (`EntityDetailPanel`). Pode começar `SearchService` em paralelo a F2.1-F2.4 (não depende).

## F1.1 — SearchService base

- [x] **T1.1.1** Testes vermelhos em `tests/CharacterWizard.Tests/SearchServiceTests.cs`:
    - `Search_returns_empty_when_query_empty_and_no_filters`
    - `Search_finds_by_substring_case_insensitive`
    - `Search_ranks_prefix_above_substring`
    - `Search_alphabetic_within_same_rank`
- [x] **T1.1.2** Criar `src/CharacterWizard.App/Services/SearchService.cs` + record `SearchEntry`, `SearchResultPage`. Construtor injeta os repositórios. Verify: testes verdes.
- [x] **T1.1.3** Commit: `feat(app): SearchService base with substring + prefix ranking`.

## F1.2 — SearchService filtros e paginação

- [ ] **T1.2.1** Testes vermelhos:
    - `Search_filters_by_category`
    - `Search_filters_by_source`
    - `Search_paginates_results`
    - `Search_total_count_independent_of_page`
- [ ] **T1.2.2** Estender `SearchService` com parâmetros `categories`, `sources`, `pageSize`, `pageIndex`.
- [ ] **T1.2.3** Commit: `feat(app): SearchService filters and pagination`.

## F1.3 — Search page (UI)

- [ ] **T1.3.1** Criar `src/CharacterWizard.App/Components/Pages/Search.razor` em `/search`. Layout 2 colunas: controles+lista à esquerda, `<EntityDetailPanel>` à direita.
- [ ] **T1.3.2** Input de query com debounce 200ms (via `Task.Delay` + token).
- [ ] **T1.3.3** Chips de categoria (multi-toggle).
- [ ] **T1.3.4** Dropdown de source (multi-select).
- [ ] **T1.3.5** `<Virtualize>` na lista de resultados.
- [ ] **T1.3.6** Click em resultado seleciona e mostra no panel.
- [ ] **T1.3.7** NavLink "Compêndio" no NavMenu.
- [ ] **T1.3.8** Commit: `feat(app): Search page with filters and detail panel`.

## F1.4 — CompendiumSearchBox e integração no wizard

- [ ] **T1.4.1** Criar `src/CharacterWizard.App/Components/Shared/CompendiumSearchBox.razor`. Input + lista de top-10 sugestões inline. `EventCallback<SearchEntry> OnSelect`.
- [ ] **T1.4.2** Em `NewCharacter.razor`, header: botão "🔍 Compêndio" abre modal com `<CompendiumSearchBox>` + `<EntityDetailPanel>`.
- [ ] **T1.4.3** Garantir que `CharacterDraft` persiste após abrir/fechar modal (já é scoped — só validar).
- [ ] **T1.4.4** Commit: `feat(app): compendium search assist in wizard`.
