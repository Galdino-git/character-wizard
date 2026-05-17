# Spec: Wizard Enrichment (F4–F7)

**Feature IDs:** F4 (Subrace) · F5 (Subclass) · F6 (Initial Spells) · F7 (Initial Equipment)
**Status:** Specification
**Depends on:** existing wizard (`NewCharacter.razor`), `EntityDetailPanel`, `SearchService`.

## Context

O wizard atual cria personagens com race, class, background e atributos. Para que o personagem nasça já com tudo necessário para sentar à mesa, faltam: subraça, subclass (quando aplicável pelo nível inicial), spells conhecidos iniciais (se conjurador) e equipamento inicial.

Todos os dados já estão no catálogo. Esta feature é majoritariamente UI + pequenas extensões de modelo no `CharacterDraft`.

## User stories

- **US-W.1** — Escolhi `Elf` e quero selecionar `High Elf` (subraça) na mesma página, vendo um pequeno cartão por subraça com bônus de ability e features.
- **US-W.2** — Escolhi `Wizard` e nível inicial 6 — devo poder selecionar a Arcane Tradition (subclass). Se o nível inicial fosse 1, o passo é pulado.
- **US-W.3** — Como Wizard nível 1, devo selecionar 3 cantrips e 6 spells de 1º nível do meu spellbook a partir da lista de Wizard spells.
- **US-W.4** — Como Fighter nível 1, devo ver as opções de equipamento inicial da classe (ex.: "(a) chain mail OR (b) leather armor, longbow, 20 arrows") e selecionar uma opção; o background adiciona seus próprios itens automaticamente.
- **US-W.5** — Tudo isto persiste no `Character` salvo, e a Ficha mostra os itens, spells e features corretas.

## Functional requirements

### F4 — Subrace

- **REQ-W.1** — Após selecionar uma raça, se houver subraças aplicáveis (via `RaceRepository.SubracesOf`), mostrar cards de subraça na mesma tela.
- **REQ-W.2** — Selecionar subraça é opcional quando a raça não tem subraças; obrigatório quando há.
- **REQ-W.3** — `CharacterDraft.SubraceName` (string?) já existe no `Character`. Adicionar ao `CharacterDraft`.

### F5 — Subclass

- **REQ-W.4** — Detectar o nível em que a classe escolhe subclass via parse de `classFeatures` (procurar `gainSubclassFeature: true`).
- **REQ-W.5** — Se `Draft.InitialLevel >= subclassLevel`, adicionar um passo "Subclass" no wizard imediatamente após Class.
- **REQ-W.6** — Listar subclasses via `ClassRepository.SubclassesOf(classRef)`. Persistir em `Draft.SubclassRef`.

### F6 — Initial Spells

- **REQ-W.7** — Se a classe tem `spellcastingAbility` não-nulo, mostrar passo "Spells".
- **REQ-W.8** — Calcular # de cantrips e # de spells de 1º nível conforme `cantripProgression` e `spellsKnownProgressionFixed` no nível atual.
- **REQ-W.9** — Listar spells filtradas por classe (via `spell.classes.fromClassList[]`) e nível.
- **REQ-W.10** — Selecionar até o limite por nível; mostrar contadores ("3/3 cantrips").
- **REQ-W.11** — Persistir em `Draft.KnownSpells`.

### F7 — Initial Equipment

- **REQ-W.12** — Mostrar passo "Equipamento" antes da revisão.
- **REQ-W.13** — Listar **sugestões** parseadas de `class.startingEquipment.default[]` e `background.startingEquipment` (quando existem) — formato 5etools é heterogêneo, MVP mostra texto + checkbox "adicionar".
- **REQ-W.14** — Permitir adicionar itens livres via `ItemPickerModal` (já existe).
- **REQ-W.15** — Persistir em `Draft.Inventory`.

## Non-functional

- **NFR-W.1** — Steps devem ser pulados condicionalmente sem deixar slot vazio (índice dos passos é dinâmico).
- **NFR-W.2** — Re-navegação para um passo já preenchido preserva a seleção.

## Out of scope (deste milestone)

- Múltiplas classes (multiclass) no wizard inicial — futuro M3.
- Resolver `startingEquipment` com sintaxe completa de "OR" / "AND" / "choose N from list" — MVP usa parsing simples + edição manual.
- Validação rígida de pré-requisitos de subclass (`prerequisite`).
- Animações / transições.

## Acceptance criteria

- AC-W.1 — Criar `High Elf` nível 6 Wizard School of Evocation com 3 cantrips e 6 spells PHB, salvar, reabrir: ficha mostra subraça, subclass, spells conhecidos.
- AC-W.2 — Tentar criar Wizard nível 1 — passo de Subclass não aparece (sub aparece em nv 2).
- AC-W.3 — Criar Fighter nível 1 — passo de Spells não aparece.
- AC-W.4 — No passo Equipamento, marcar "Longsword" sugerido + adicionar manualmente "Healing Potion ×2" via picker; ambos aparecem no inventário pós-criação.
