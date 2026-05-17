# Spec: Compendium Search

**Feature ID:** F1
**Status:** Specification
**Owner:** —
**Depends on:** F2 (Tooltip System) para o `<EntityDetailPanel>` compartilhado.

## Context

O usuário precisa explorar e consultar entidades do 5etools (races, classes, spells, items, etc.) sem necessariamente estar criando um personagem. Similar à barra de busca/filtros do site 5etools.

## User stories

- **US-1.1** — Como jogador, quero ter uma página de busca global do app onde digito "fireball" e vejo a entidade.
- **US-1.2** — Como jogador criando um personagem, quero ter um botão "Buscar no compêndio" no wizard que abre a mesma busca sem perder o estado do draft.
- **US-1.3** — Como jogador, quero filtrar resultados por tipo de entidade (só spells, só items, etc.) e por source (só PHB, só XGE).
- **US-1.4** — Como jogador, quero clicar num resultado e ver o detalhe completo da entidade no mesmo painel usado pelos tooltips (F2).

## Functional requirements

- **REQ-1.1** — Existir uma página Razor `/search` com input de texto, chips de filtro de categoria, dropdown de source, e lista de resultados.
- **REQ-1.2** — A busca é case-insensitive, "contains" (substring match no nome). Ranking: prefixo > contém > similar (fuzzy é opcional, deixar fora do MVP).
- **REQ-1.3** — Categorias suportadas no MVP: Race, Class, Subclass, Background, Spell, Item, Feat, OptionalFeature, Condition.
- **REQ-1.4** — Filtro por source respeita o `SourceFilter` ativo (settings do usuário). Adicionalmente, dropdown pode restringir ainda mais (subset do habilitado).
- **REQ-1.5** — Resultados mostram: ícone/imagem (quando aplicável via `ImageService`), nome, categoria, source, page.
- **REQ-1.6** — Click em resultado abre `<EntityDetailPanel>` (componente compartilhado com F2).
- **REQ-1.7** — Existir `SearchService` em `CharacterWizard.App/Services/` com método `Search(query, categoryFilter, sourceFilter, pageSize, pageIndex) → SearchResultPage`.
- **REQ-1.8** — O wizard de criação ganha um botão "🔍 Buscar no compêndio" no header que abre a busca em modal sem perder o `CharacterDraft`.
- **REQ-1.9** — NavMenu ganha entrada "Compêndio" apontando para `/search`.

## Non-functional

- **NFR-1.1** — Busca em ~10k entidades deve responder em <100ms em hardware desktop comum (in-memory, linear scan aceitável no MVP).
- **NFR-1.2** — Lista de resultados deve usar `Virtualize` do Blazor se passar de 50 entries para evitar render pesado.
- **NFR-1.3** — Sem dependências novas. Tudo com a stack existente.

## Out of scope

- Fuzzy/typo-tolerant search (substring é suficiente no MVP).
- Busca dentro do corpo das descrições (só nome).
- Salvar buscas favoritas.
- Histórico de buscas.
- Busca de monsters/creatures (bestiário não foi importado).

## Acceptance criteria

- AC-1.1 — Em `/search`, digitar "fire" e ver Fireball, Fire Bolt, Wall of Fire, etc., agrupados ou filtráveis por categoria.
- AC-1.2 — Marcar "só Items" remove spells da lista.
- AC-1.3 — Click em Fireball abre o painel de detalhe (mesmo do tooltip).
- AC-1.4 — Dentro do wizard, abrir busca, fechar busca, e voltar para o passo onde estava com o draft intacto.
- AC-1.5 — `SearchService.Search` retorna paginado e ordenado por relevância (prefixo primeiro, depois substring).
