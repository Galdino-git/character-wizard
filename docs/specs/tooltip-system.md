# Spec: Tooltip System for Entity Mentions

**Feature ID:** F2
**Status:** Specification
**Owner:** —
**Depends on:** existing `Catalog` + repositories in `CharacterWizard.Data`.

## Context

As descrições do 5etools (`entries`) são texto rico com tags inline tipo `{@spell Fireball}`, `{@condition prone}`, `{@damage 8d6}`. Hoje exibimos esse texto cru. Esta feature renderiza essas tags como elementos interativos (clicáveis com tooltip on hover), permitindo ao jogador entender termos sem sair da tela.

## User stories

- **US-2.1** — Como jogador, ao ler a descrição de Barbarian na seleção de classe, quero ver "Rage" destacado e poder fazer hover pra ler um resumo do que Rage faz, sem trocar de tela.
- **US-2.2** — Como jogador, ao ler uma spell, quero ver `{@damage 8d6}` formatado como "8d6 dano" com hover mostrando média e máximo.
- **US-2.3** — Como jogador, clicar em "Fireball" dentro de qualquer descrição abre um painel lateral com a stat block completa da spell.
- **US-2.4** — Como jogador, quero que itens, condições e habilidades referenciadas em qualquer texto fiquem clicáveis de forma consistente em todo o app.

## Functional requirements

- **REQ-2.1** — Existir um serviço `EntryRenderer` que recebe um `JsonElement` (entries do 5etools) ou string com tags e retorna `MarkupString` pronto pro Blazor.
- **REQ-2.2** — Suportar tags inline no MVP: `@spell @item @condition @feat @creature @skill @action @sense @feature @classFeature @subclassFeature @optfeature @damage @dice @b @i @u @hit @atk @dc @scaledamage @scaledice @h @note @filter`.
- **REQ-2.3** — Tags desconhecidas devem ser renderizadas com fallback (texto visível, sem quebrar o pipeline).
- **REQ-2.4** — Suportar estruturas de entries aninhadas: `string`, objeto `{type: "entries", name, entries[]}`, `{type: "list", items[]}`, `{type: "inset"}`, `{type: "table"}`.
- **REQ-2.5** — Saída HTML deve escapar conteúdo de usuário/dados (segurança contra XSS — embora dados sejam confiáveis, escapar é prática mínima).
- **REQ-2.6** — Tags clicáveis devem ser renderizadas como `<span>` com atributos `data-cw-ref="<category>" data-cw-name="<name>" data-cw-source="<source>"` para o JS/Blazor interceptar.
- **REQ-2.7** — Existir componente Blazor `<EntityRefSpan Category Name Source DisplayText>` que renderiza o span clicável + tooltip on hover.
- **REQ-2.8** — Existir componente Blazor `<EntityDetailPanel EntityRef OnClose>` que mostra detalhes ricos da entidade (nome, source, page, descrição renderizada via `EntryRenderer`).
- **REQ-2.9** — Click numa `<EntityRefSpan>` abre o `<EntityDetailPanel>` em modal/sidebar (decisão de UI: modal centralizado por padrão).
- **REQ-2.10** — Existir serviço `EntityResolver` que dado `(category, name, source)` retorna a entidade do catálogo (delegando aos repositórios existentes).

## Non-functional

- **NFR-2.1** — `EntryRenderer.Render(entries)` deve ser puro (sem side effects) e idempotente — chamável de qualquer thread, cacheável.
- **NFR-2.2** — Tooltips devem ser CSS-only (sem JS interop). Decisão: usar `position: fixed` com `:hover` em wrapper, ou um portal Blazor com posicionamento via `IJSRuntime`. **Decisão arquitetural: começar CSS-only** — se ficar inviável para conteúdo rico/posicionamento, escalar para componente Blazor stateful com tracking de mouse.
- **NFR-2.3** — Renderização de um bloco de entries grande (ex.: descrição completa de uma class) não deve passar de 50ms.

## Out of scope (deste milestone)

- Tag `@filter` (rota pra busca pré-filtrada) — renderiza como texto simples por enquanto.
- Imagens dentro de entries (mapas, ilustrações) — entries do tipo `image` viram placeholder texto.
- Tags de áudio (`@soundClip`) — ignoradas.
- Edição/criação de entries (read-only).

## Acceptance criteria

- AC-2.1 — Abrir descrição do Wizard mostra `Arcane Recovery`, `Spellbook` etc. como links destacados; hover mostra resumo; click abre painel.
- AC-2.2 — Texto puro `{@damage 8d6 fire}` renderiza como `8d6 fogo` com tooltip mostrando média 28 e máx 48.
- AC-2.3 — Texto `{@condition prone}` renderiza linkado pra condition Prone; click abre painel mostrando "While prone, ...".
- AC-2.4 — Tag desconhecida `{@xyz foo}` não quebra a página, renderiza como `foo`.
- AC-2.5 — Conteúdo malicioso `{@spell <script>alert(1)</script>}` é escapado e não executa.
