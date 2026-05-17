# Design: Configurations & Polish (F10–F14)

## F10 — Settings page

`Components/Pages/Settings.razor`:
- Injeta `AppSettings`, `AppSettingsStore`, `CatalogProvider`, `BookRepository`.
- Renderiza `BookRepo.ByGroup()` com checkbox por grupo + lista de books com checkbox individual.
- Botão "Salvar e aplicar" persiste e chama `CatalogProvider.ApplySettings(_settings)`.
- Mostra "X races habilitadas / Y spells / etc" baseado nos repos após apply.

NavMenu ganha link.

## F11 — Dark theme

`AppSettings.Theme` (string `"light"` | `"dark"`, default `"light"`).

`wwwroot/cw-theme.css`:
```css
body.cw-dark { background:#1a1a1a; color:#e6e6e6; }
body.cw-dark .card, body.cw-dark .cw-modal { background:#252525; color:#e6e6e6; border-color:#333; }
body.cw-dark .table { color:#e6e6e6; }
body.cw-dark .cw-roll { background:#3a2e1a; color:#f3c673; }
body.cw-dark .cw-inset { background:#2a261a; border-color:#7a5b30; }
/* …etc */
```

Helper JS em `cw-theme.js`:
```js
window.cwTheme = {
    apply(t){ document.body.classList.toggle('cw-dark', t === 'dark'); }
};
```

Toggle no `MainLayout.razor` (botão no header) chama `JS.InvokeAsync("cwTheme.apply", t)`, atualiza `AppSettings.Theme`, salva.

Aplicar ao startup: `MainLayout.OnAfterRenderAsync(firstRender)` lê settings e chama `apply`.

## F12 — Export/Import

Export (`CharacterView` header, "Exportar JSON"):
```csharp
var json = JsonSerializer.Serialize(_character, …);
await JS.InvokeVoidAsync("cwIo.download", $"{c.Name}.json", json);
```

Import (Home page, button "Importar JSON"):
- `<InputFile OnChange="OnImport" accept=".json">`
- Read file content, `JsonSerializer.Deserialize<Character>`, `c.Id = Guid.NewGuid()`, `Store.Save(c)`, refresh list.

JS helper `cw-io.js`:
```js
window.cwIo = {
    download(name, content){
        const blob = new Blob([content], {type:'application/json'});
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = name; a.click();
        URL.revokeObjectURL(url);
    }
};
```

## F13 — `_copy` resolution

Hoje em `CatalogLoader.LoadCollection<T>`:
```csharp
if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("_copy", out _))
    continue; // skipped silently
```

Mudança: passar duas vezes pelo array.
1. **Pass 1**: indexar todas as entries não-copy por `{Name|Source}` (via `JsonElement`).
2. **Pass 2**: para cada entry, se tem `_copy`, montar o JSON merged (parent overridden by child) e deserializar.

`_copy` shape comum:
```json
{
    "_copy": { "name": "Elf", "source": "PHB" },
    "name": "High Elf", "source": "PHB",
    "ability": [{"int": 1}],
    "entries": [...]
}
```

Algoritmo de merge:
- Começa do parent.
- Para cada property no child (exceto `_copy`), substitui no parent.
- Para arrays, child substitui inteiramente (não há diff RAW; o 5etools tem operadores como `_mod` mas no MVP ignoramos — uma child com `_mod` no array vira override completo).

Implementar como um pequeno `CopyResolver` em `CharacterWizard.Data.Loading`:

```csharp
internal static class CopyResolver
{
    public static JsonElement Merge(JsonElement parent, JsonElement child);
}
```

E `LoadCollection<T>` faz: duas passes, monta o JsonElement final, deserializa.

TDD:
- `CopyResolver_merges_basic_overrides`
- `CopyResolver_child_array_replaces_parent_array`
- `CatalogLoader_loads_copy_subraces_after_change`

## F14 — Multiclass at creation

Mudança maior. `CharacterDraft`:
- Substituir `ClassRef`/`SubclassRef`/`InitialLevel` por `List<CharacterClassEntry> Classes`.
- `InitialLevel` torna-se `Draft.Classes.Sum(c => c.Levels)`.

`NewCharacter.razor`:
- Step "Classe" vira step "Classes" — mostra lista de classes adicionadas + botão "+ Adicionar classe".
- Para cada classe adicionada: form inline com nível, subclass (se aplicável), botão remover.
- Steps "Subclass" e "Spells" se transformam em sub-blocos por classe (ou se mantêm como steps separados iterando sobre cada classe — mais complexo).

Para MVP: manter Subclass e Spells como steps separados que iteram por cada classe (skip se já preenchido).

Decisão: por enxutez, **renderizar Subclass + Spells inline dentro do step Classes** (cada classe na lista tem accordion expandindo Subclass + Spells próprio). Steps separados ficam mais simples mas pioram fluxo com multiclass.

`ToCharacter` propaga `Draft.Classes` direto.

## Tests

F10/F11/F12: smoke-test manual basta (sem lógica pura nova).
F13: TDD do `CopyResolver` (3+ testes).
F14: refactor — adicionar 1 teste em CharacterDraft.ToCharacter (multiclass list → Character.Classes).

## Decisões

- **Settings UI sem hot-reload de catálogo** — só filtro; recarregar JSONs custaria 5s+.
- **Dark via classe** + variável CSS, sem switch no Razor por componente (CSS herda).
- **Export/Import via JS** (não MAUI FilePicker) — mais simples, mantém arquitetura puro Razor.
- **`_copy` parcial**: só override de field-level; operadores `_mod`/`_preserve` ignorados (raros em races).
- **Multiclass no wizard inline** em vez de iterar steps por classe — mais simples, menos clicks.
