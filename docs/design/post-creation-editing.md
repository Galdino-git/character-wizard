# Design: Post-Creation Editing (F3)

Implementa REQ-3.1 a REQ-3.9 do [spec](../specs/post-creation-editing.md). Camada pura de UI — model e persistência já existem.

## Arquitetura

```
┌────────────────────────────────────┐
│  CharacterView.razor               │  /character/{id}
│  ┌──────────────────────────────┐  │
│  │ Tabs: Ficha | Inv | Buffs    │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ Tab content (switch)         │  │
│  │  - SheetTab (current)        │  │
│  │  - InventoryTab              │──┼──▶ ItemPickerModal ─▶ SearchService
│  │  - BuffsTab                  │  │     │
│  └──────────────────────────────┘  │     ▼
│   onChange → Store.Save(c)         │  ItemEditorModal (custom name, bonuses)
└────────────────────────────────────┘
```

## Componentes novos

### `<CharacterView>` (refator de existente)

Switch de tabs no topo, conteúdo abaixo. Mantém `_character` carregado uma vez; cada tab opera sobre o mesmo objeto. Após cada alteração, chama `Store.Save(_character)` e força `StateHasChanged()`.

### `<InventoryTab>` (novo, `Components/Pages/CharacterView/`)

Props:
- `Character Char`
- `EventCallback OnChange`

Renderiza tabela de `InventoryItem`s. Botão `+ Adicionar item` abre `<ItemPickerModal>`. Click em item da lista abre `<ItemEditorModal>`.

### `<BuffsTab>` (novo)

Lista de `AbilityOverride`s (delta, ability, reason) + formulário inline para adicionar. Textarea para `Notes`. Mesma estratégia de save imediato.

### `<ItemPickerModal>` (novo)

Modal com input + lista filtrada via `SearchService.Search(query, categories: ["Item"])`. Retorna `EntityRef` selecionado via callback. Reusa o estilo `.cw-modal` já criado em `cw-tooltip.css`.

### `<ItemEditorModal>` (novo)

Modal com form bindando todos os campos de `InventoryItem`. Botões `Salvar` / `Remover` / `Cancelar`. Callback emite `InventoryItem` editado ou null (remove).

## Estado

O `Character` é mutável em memória durante a sessão de edição (records → não; mas as listas dentro dele e os campos com setter público são editáveis). O modelo `Character` já tem setters públicos em todos os campos editáveis. Edições são feitas direto na referência.

Por que não copy-on-write? Em UI Blazor com `@bind`, mutação direta é a norma. O `CharacterStore.Save` recebe a referência atual e serializa — simples e funciona.

## Tests (TDD onde adiciona valor)

Foco: garantir round-trip do `CharacterStore` para os cenários expandidos.

- `CharacterStore_preserves_multiple_inventory_items_with_bonuses`
- `CharacterStore_preserves_ability_overrides_list`
- `CharacterStore_history_keeps_previous_versions`

A UI em si é validada por smoke-test manual.

## Decisões

- **Sem confirmação de remoção** — o backup automático em `_history/` é o safety net (NFR-3.3). Simples e alinhado com a vibe de ferramenta pessoal.
- **Save imediato em cada blur/change** — não há botão "Salvar"; reduz fricção.
- **Tabs simples** (Bootstrap nav-tabs) — sem roteamento por URL. Estado de tab é local no componente.
- **Sem `bUnit`** — testar Blazor visual sem MAUI é trabalhoso e dá pouco valor. Foco testar Core/Data.
