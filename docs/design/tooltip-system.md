# Design: Tooltip System (F2)

Implementa REQ-2.1 a REQ-2.10 do [spec](../specs/tooltip-system.md).

## Arquitetura

```
JsonElement (entries)
        │
        ▼
┌────────────────────┐
│   EntryRenderer    │  Data/EntryRendering/
│   .Render(JE) → MS │  - parse recursivo de entries
└────────────────────┘  - parse de tags inline
        │               - escape HTML
        ▼
   MarkupString
        │
        ▼
┌────────────────────┐
│  EntryDisplay      │  App/Components/Shared/
│    .razor          │  - aceita JsonElement
└────────────────────┘  - chama EntryRenderer
        │               - emite spans com data-cw-*
        ▼
┌────────────────────┐
│  EntityRefSpan     │  App/Components/Shared/
│    (CSS tooltip)   │  - hover: tooltip CSS
└────────────────────┘  - click: dispara callback
        │
        ▼
┌────────────────────┐
│ EntityDetailPanel  │  App/Components/Shared/
│  (modal Razor)     │  - mostra detalhe completo
└────────────────────┘  - usa EntryDisplay (recursivo)
```

## Componentes

### `EntryRenderer` (estático, em `Data/EntryRendering/`)

Classe estática (puro, sem DI). API mínima:

```csharp
public static class EntryRenderer
{
    public static string Render(JsonElement entries);
    public static string RenderString(string text); // só inline tags
}
```

Retorna `string` com HTML — quem consome converte pra `MarkupString` (mantemos Data sem dep do Razor).

Estratégia interna:
1. Switch no `ValueKind` da entry:
   - `String` → `RenderString`
   - `Object` com `type` → despacha pra `RenderEntries`/`RenderList`/`RenderInset`/`RenderTable`/`RenderQuote`
   - `Array` → renderiza cada item e concatena
2. `RenderString` faz scan linear procurando `{@tag content}` (regex compilada `\{@(\w+)\s+([^}]+)\}`).
3. Cada match dispara um handler por tag. Handler retorna string HTML (escapada).
4. Texto fora dos matches é HTML-escaped.

### Tag handlers

Cada handler segue assinatura `string Handle(string args)`. `args` é o conteúdo entre `{@tag ` e `}`, podendo ter pipes (`Fireball|PHB|fb`).

Categorias:

| Tag | Categoria de entidade | Display fallback |
|-----|------------------------|------------------|
| `@spell` | spell | `args[2] ?? args[0]` |
| `@item` | item | idem |
| `@condition` | condition | idem |
| `@feat` | feat | idem |
| `@creature` | creature (n/a — bestiário fora) | idem (texto não clicável) |
| `@skill` | skill | idem |
| `@action` | action | idem |
| `@sense` | sense | idem |
| `@feature` `@classFeature` `@subclassFeature` `@optfeature` | class feature | idem |
| `@damage` `@dice` | (formato) | renderizar fórmula com tooltip de média/máx |
| `@hit` | (formato) | "+N" |
| `@atk` | (formato) | tradução de "mw,rw,ms" etc. |
| `@dc` | (formato) | "DC N" |
| `@scaledamage` `@scaledice` | (formato) | fórmula |
| `@b` `@bold` | inline tag | `<strong>` |
| `@i` `@italic` | inline tag | `<em>` |
| `@u` `@underline` | inline tag | `<u>` |
| `@h` | (highlight) | `<mark>` |
| `@note` | (footnote/aside) | inline italic |
| `@filter` | search jump (futuro) | display text plain |

Todas as tags clicáveis emitem:

```html
<span class="cw-ref" data-cw-cat="spell" data-cw-name="Fireball" data-cw-source="PHB">Fireball</span>
```

### `EntityResolver` (singleton, em `Data/Resolving/`)

```csharp
public sealed class EntityResolver
{
    public object? Resolve(string category, string name, string source);
}
```

Despacha pra `RaceRepository`, `SpellRepository`, etc. baseado na string `category`. Retorna `object?` — o componente Razor faz pattern matching pra renderizar conforme tipo.

### `<EntityRefSpan>` (Razor, em `App/Components/Shared/`)

Componente recebe parâmetros `Category Name Source DisplayText`. Renderiza:

```html
<span class="cw-ref" @onclick="...">
    @DisplayText
    <span class="cw-tooltip">
        <strong>@Name</strong><br />
        <small>@Source</small>
        @if (summary is not null) { <p>@summary</p> }
    </span>
</span>
```

CSS:
- `.cw-ref` é `position: relative; cursor: pointer; border-bottom: 1px dotted`.
- `.cw-tooltip` é `position: absolute; display: none; z-index: 1000`.
- `.cw-ref:hover .cw-tooltip` é `display: block`.

Posicionamento via CSS é "above" por padrão; quando colidir com top da viewport, JS-free fallback é "below" via `:hover` em pseudo-classe `data-pos`. Decisão: começar com sempre "above" e ajustar depois se feedback ruim.

### `<EntityDetailPanel>` (Razor, em `App/Components/Shared/`)

Recebe `Category Name Source` e callback `OnClose`. Renderiza modal Bootstrap (`modal.show` via classe CSS, sem JS) com:

- Header: nome + categoria + source/page
- Body: `<EntryDisplay>` da descrição
- Imagem se disponível (via `ImageService`)

### `<EntryDisplay>` (Razor, em `App/Components/Shared/`)

Bridge entre `JsonElement` e DOM. Recebe `JsonElement Entries`, chama `EntryRenderer.Render`, embrulha em `MarkupString`. Depois faz **post-processing** dos `<span class="cw-ref">` substituindo por instâncias reais de `<EntityRefSpan>` — feito via parsing simples do output ou (mais limpo) gerar `RenderFragment` no próprio renderer.

**Decisão**: por simplicidade e perf, `EntryDisplay` renderiza o HTML cru via `MarkupString` e usa um único `@onclick` handler no container que delega o click conforme `data-cw-*` attributes do target. Hover funciona puramente via CSS no `.cw-ref`. Click handler usa `IJSRuntime.InvokeAsync` mínimo para ler atributos do `event.target` (única exceção à regra "sem JS" — necessário pra event delegation).

Atualização: pra evitar IJSRuntime, opção alternativa é gerar markup com `data-cw-*` e um pequeno `<script>` global que registra delegation no carregamento. Mas isso polui o HTML. **Decisão final**: usar `IJSRuntime` só pra ler os attrs do target no click. Tooltips continuam 100% CSS.

## Fluxo de click

1. Usuário clica em `<span class="cw-ref" data-cw-name=... data-cw-cat=... data-cw-source=...>`.
2. Click event borbulha pro container `<EntryDisplay>`.
3. Handler `OnClickContainer(MouseEventArgs)` chama JS pra ler `event.target.dataset.cwCat/cwName/cwSource`.
4. Se preenchido, emite evento Blazor `OnEntityClicked` com `EntityRef`.
5. Página/componente pai abre `<EntityDetailPanel>` com o ref.

## Estratégia de teste

Foco: `EntryRenderer` é puro → 100% testável sem MAUI.

- `RenderString_escapes_html`
- `RenderString_renders_b_tag_as_strong`
- `RenderString_renders_spell_as_cw_ref_span`
- `RenderString_unknown_tag_falls_back_to_display_text`
- `RenderString_handles_pipe_separated_args`
- `Render_handles_object_entry_with_name`
- `Render_handles_list_entry`
- `Render_handles_nested_entries`
- `Render_damage_formats_dice_expression`

`EntityResolver` testado via repositórios mockados ou catálogo real.

`EntityRefSpan` e `EntityDetailPanel` validados via smoke-test manual (não vale a pena bUnit no MVP).

## Riscos

- **Tooltips CSS fora da viewport**: aceitar por ora; se ficar ruim, escalar pra Popper.js (NÃO instala dependência ainda).
- **IJSRuntime no click**: dep aceita; alternativa é renderizar cada `cw-ref` como `<EntityRefSpan>` componente real, mas vira post-processing pesado. Decisão tomada acima.
- **Entries com schema imprevisto**: fallback é mostrar `[unsupported entry type: foo]` em vermelho debug; em release vira string vazia.
