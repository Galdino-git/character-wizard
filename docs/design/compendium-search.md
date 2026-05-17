# Design: Compendium Search (F1)

Implementa REQ-1.1 a REQ-1.9 do [spec](../specs/compendium-search.md).

## Arquitetura

```
                ┌──────────────────┐
                │  Search.razor    │  /search page
                │                  │
                │  ┌────────────┐  │
                │  │ Search box │  │
                │  └────────────┘  │
                │  ┌────────────┐  │
                │  │ Cat chips  │  │
                │  └────────────┘  │
                │  ┌────────────┐  │
                │  │ Source DD  │  │
                │  └────────────┘  │
                │  ┌────────────┐  │
                │  │ Virtualize │──┼──▶ <EntityDetailPanel>
                │  │ results    │  │     (compartilhado c/ F2)
                │  └────────────┘  │
                └────────┬─────────┘
                         │
                         ▼
              ┌──────────────────┐
              │  SearchService   │  App/Services/
              │                  │
              │ Search(q, cats,  │
              │   srcs, page) →  │
              │   SearchResultPg │
              └────────┬─────────┘
                       │ reads
                       ▼
         existing repos (RaceRepo, SpellRepo, ...)
         already filtered by SourceFilter
```

## Componentes

### `SearchEntry` record

DTO de resultado de busca, agnóstico da categoria:

```csharp
public sealed record SearchEntry(
    string Category,   // "Race" | "Class" | "Spell" | ...
    string Name,
    string Source,
    int? Page,
    string? ShortDescription = null);
```

### `SearchService`

```csharp
public sealed class SearchService
{
    public SearchResultPage Search(
        string query,
        IReadOnlyCollection<string>? categories = null,
        IReadOnlyCollection<string>? sources = null,
        int pageSize = 50,
        int pageIndex = 0);
}

public sealed record SearchResultPage(
    IReadOnlyList<SearchEntry> Items,
    int TotalCount,
    int PageIndex,
    int PageSize);
```

Comportamento:
1. Normalizar `query` (trim, ToLowerInvariant).
2. Para cada repositório registrado (via `IEnumerable<ICategorySource>` ou hardcoded), enumerar entidades já filtradas pela `SourceFilter`.
3. Filtrar por categoria, depois por source extra.
4. Filtrar por contains do query no nome (case-insensitive).
5. Ordenar:
   - 1º: nomes que **começam com** o query
   - 2º: nomes que **contêm** o query
   - dentro de cada bucket, alfabético
6. Paginar e retornar.

Implementação inicial: simples, in-memory, sem indexação. NFR-1.1 (<100ms para 10k entries) cumprido facilmente com linear scan.

### `ICategorySource` (opcional, decidir na implementação)

Abstração leve para o serviço enumerar todas as categorias sem hardcode. Cada repositório implementa:

```csharp
internal interface ICategorySource
{
    string CategoryName { get; }   // "Race", "Spell", ...
    IEnumerable<SearchEntry> Enumerate();
}
```

**Decisão**: começar SEM essa interface (hardcode a lista de repositórios no `SearchService`). Refatorar pra interface se ficar repetitivo (provavelmente sim, mas YAGNI até dor real).

### `Search.razor` (page)

Layout 2 colunas em desktop:
- Esquerda (40%): controles + lista virtualizada
- Direita (60%): preview do item selecionado via `<EntityDetailPanel>`

Em mobile: stack vertical, click em resultado abre modal.

Estado:
- `_query` (string, debounce 200ms)
- `_selectedCategories` (HashSet<string>)
- `_selectedSources` (HashSet<string>)
- `_selectedEntry` (SearchEntry?)
- `_resultsPage` (SearchResultPage)

Chama `SearchService.Search` sempre que `_query`/`_selectedCategories`/`_selectedSources` mudam.

### `<CompendiumSearchBox>` (Razor)

Componente reutilizável (input + sugestões inline). Usado dentro do wizard via `<CompendiumSearchBox OnSelect="..."/>`. Por dentro chama `SearchService` com pageSize curto (top 10).

### Integração com Wizard

Em `NewCharacter.razor`, header ganha botão "🔍 Compêndio" que abre `<CompendiumSearchBox>` num modal Bootstrap (`modal.show` via CSS-class toggle). Modal NÃO desmonta `NewCharacter.razor` — `CharacterDraft` (scoped service) preserva estado intacto.

### NavMenu

Acrescentar entrada:
```html
<NavLink href="search">
    <span class="bi bi-search-nav-menu"></span> Compêndio
</NavLink>
```

## Tests

Foco: `SearchService` (puro, fácil de testar com catálogo carregado).

- `Search_returns_empty_when_query_empty_and_no_filters`
- `Search_finds_by_substring_case_insensitive`
- `Search_ranks_prefix_above_substring`
- `Search_filters_by_category`
- `Search_filters_by_source`
- `Search_respects_pagination`
- `Search_returns_total_count_independent_of_page`

`Search.razor` validado via smoke-test manual.

## Riscos

- **Performance com 10k+ entidades**: linear scan + sort. Se ficar lento, indexar por prefixo do nome.
- **UI lenta ao digitar**: usar debounce de 200ms no input.
- **Categorias futuras** (monsters etc.): refatorar pra `ICategorySource` quando entrar M3+.
