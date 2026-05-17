# Spec: Configurations & Polish (F10–F14)

**Feature IDs:** F10 (Settings UI) · F11 (Dark theme) · F12 (Export/Import) · F13 (_copy resolution) · F14 (Multiclass at creation)
**Status:** Specification

## Context

Refinos de UX e completude do conteúdo. Hoje:
- Filtro de sources só editável manualmente no JSON em `%AppData%`.
- Tema único (claro). Letreiros e cards usam Bootstrap default.
- Personagens não trocam entre máquinas — só vivem em `%AppData%`.
- Algumas raças/subclasses não aparecem no app porque o CatalogLoader pula entries `_copy` (que herdam de outra).
- Multiclass só funciona via level-up (criar já multiclass exige criar single-class e subir).

## User stories

- **US-10** — Vou ao menu "Configurações" e habilito o grupo `setting` para ver conteúdo de Eberron na busca e no wizard.
- **US-11** — Ativo "tema dark" e o app inteiro fica em paleta escura.
- **US-12** — Exporto meu personagem como `.json` para mandar pra um amigo, ou importo um arquivo recebido.
- **US-13** — Procuro "Eladrin" (subraça que é `_copy` de Elf) e ela aparece nas opções de raça/subraça.
- **US-14** — Crio um Fighter 3 / Wizard 1 direto no wizard (sem precisar subir nível depois).

## Functional requirements

### F10 — Settings UI

- **REQ-10.1** — Página `/settings` no NavMenu.
- **REQ-10.2** — Listar todos os `group`s presentes em `books.json` com checkbox (habilitado por default conforme `AppSettings.EnabledGroups`).
- **REQ-10.3** — Listar todos os books agrupados por `group`, com checkbox individual permitindo override (`EnabledSources` / `DisabledSources`).
- **REQ-10.4** — Botão "Salvar e aplicar" — chama `AppSettingsStore.Save` e `CatalogProvider.ApplySettings`. UI se atualiza para refletir novos filtros.
- **REQ-10.5** — Mostrar contagem de items totais habilitados (races / classes / spells / etc) para feedback imediato.

### F11 — Dark theme

- **REQ-11.1** — Toggle no header global ou em Settings.
- **REQ-11.2** — Aplicar via classe `cw-dark` no `<body>` ou container raiz; CSS customizado redefine paleta.
- **REQ-11.3** — Persistir em `AppSettings.Theme` (`"light"` | `"dark"`).
- **REQ-11.4** — Aplicar ao carregar via JS interop pequeno no startup.

### F12 — Export / Import

- **REQ-12.1** — Botão "Exportar JSON" no header do `CharacterView`. Gera download do arquivo `{Name}.json` com o serializado completo.
- **REQ-12.2** — Botão "Importar JSON" na Home; abre file picker (`<input type="file">`), parse, valida, salva no `CharacterStore` com novo `Id` (evita colisão).
- **REQ-12.3** — Formato é o mesmo já gravado em disco — não há schema separado de "export".

### F13 — `_copy` resolution

- **REQ-13.1** — `CatalogLoader` carrega entries com `_copy` ao invés de pular.
- **REQ-13.2** — Resolver `_copy` recursivamente: merge dos campos do parent na entry filha, com a filha sobrescrevendo onde definir.
- **REQ-13.3** — Aplicar pelo menos para `race` + `subrace`. Subclasses podem entrar depois (geralmente os arquivos das subclasses não usam _copy).
- **REQ-13.4** — Entries de `_copy` que apontam para um source desabilitado/inexistente são puladas com warning silencioso.

### F14 — Multiclass at creation

- **REQ-14.1** — `BasicsStep` ou novo step "Classes" permite adicionar múltiplos `CharacterClassEntry`s antes mesmo de seguir.
- **REQ-14.2** — Cada entry tem sua própria contagem de níveis; a soma é o `TotalLevel`.
- **REQ-14.3** — Cada classe entra com sua opção de subclass (se aplicável pelo nível) e bloco de spells (se caster).
- **REQ-14.4** — `Draft.Classes` (lista) substitui `Draft.ClassRef` (single).

## Non-functional

- **NFR-M3.1** — Settings save/apply é instantâneo (in-memory + arquivo). Catálogo não recarrega; só filtro.
- **NFR-M3.2** — Dark theme deve cobrir todos os componentes custom (`cw-modal`, `cw-tooltip`, `cw-table`, `cw-inset`, etc).
- **NFR-M3.3** — Export/Import via browser-style download/upload (JS interop), sem dependência adicional.

## Out of scope

- Telemetria local (deferida indefinidamente).
- Múltiplas paletas (só light + dark).
- Import de personagem de outros formatos (Foundry, D&D Beyond) — só nosso JSON.
- `_copy` em sub-classes (raro nos JSONs reais; só races/subraces).
- Validação de pré-requisitos de multiclass no wizard inicial (igual ao level-up — alert info).

## Acceptance criteria

- AC-10 — Em `/settings`, desabilitar grupo `supplement-alt`, salvar, voltar à Search e confirmar que items de TCE somem.
- AC-11 — Ativar dark, fechar e reabrir app, dark persiste.
- AC-12 — Exportar personagem A, deletar local, importar o arquivo: personagem volta com novo Id mas mesmos dados.
- AC-13 — Buscar "Eladrin" no compêndio mostra a subraça (que é `_copy` de Elf).
- AC-14 — Criar personagem novo escolhendo "Fighter 3 + Wizard 1" no step de Classes — character salvo tem 2 entries, total 4.
