# Spec: Character Lifecycle Polish (F9)

**Feature ID:** F9 (multiclass + feat picker + spells on level-up + auto-features)
**Status:** Specification
**Depends on:** F5 (subclass), F6 (spells), F8 (level-up).

## Context

A criação inicial e o level-up já existem (M2). Faltam refinamentos importantes:
- Pick de feat ao escolher Feat no ASI/Feat (hoje só name/source manual).
- Spells aprendidos no level-up (hoje só na criação).
- Lista das features de classe/subclass ganhas no nível.
- Multiclass (subir uma classe diferente da existente).

## User stories

- **US-9.1** — No level-up de Fighter nível 4, escolho Feat e o app abre um modal de pesquisa de feats (similar ao item picker), com filtro por source.
- **US-9.2** — Como Wizard nível 1 → 2 (full caster), quero aprender 2 spells novos no spellbook + 0 cantrips novos. O level-up modal mostra a quota e o picker filtrado por classe+nível.
- **US-9.3** — Ao subir do nível 4 → 5, o app me lembra: "Fighter ganhou Extra Attack neste nível" (clicável → painel de detalhe).
- **US-9.4** — Tenho um Fighter 3 e quero pegar 1 nível de Rogue. Clico "+ Subir nível", escolho "Adicionar classe (multiclass)", seleciono Rogue, aplico. Personagem fica Fighter 3 / Rogue 1.

## Functional requirements

### F9.1 — Feat picker modal

- **REQ-9.1** — Existir `<FeatPickerModal Open OnPick OnCancel>` em `Components/Shared/`. Estrutura idêntica ao `ItemPickerModal` mas categoria `Feat`.
- **REQ-9.2** — `LevelUpModal`, quando `_asiOrFeat == "feat"`, mostra botão "🔍 Escolher feat" que abre `FeatPickerModal`; ao selecionar, popula `_featName` e `_featSource`.
- **REQ-9.3** — Manter fallback de input manual (caso usuário queira homebrew).

### F9.2 — Spells on level-up

- **REQ-9.4** — Em `LevelUpModal`, se `IsCaster(class)`:
    - Calcular delta de cantrips: `CantripsKnownAtLevel(new) − CantripsKnownAtLevel(old)`.
    - Calcular delta de spells: `SpellsKnownAtLevel(new) − SpellsKnownAtLevel(old)` (PHB Wizard: +2 por nível após 1).
    - Se algum delta > 0, mostrar painel "Aprender spells novos" com lista filtrada (`SpellRepository.ForClassAtLevel`) e quota.
- **REQ-9.5** — Acrescentar selecionados em `Character.KnownSpells`.

### F9.3 — Features gained

- **REQ-9.6** — `LevelUpModal`, antes do "Aplicar", mostra uma lista "Features ganhas neste nível":
    - Class features: parse de `classFeatures[]` filtrando por `Level == newLevel`.
    - Subclass features (se há subclass selecionada): parse de `subclassFeatures[]` da subclass.
    - Cada item é um link clicável que abre `EntityDetailPanel` (category `classfeature`/`subclassfeature` — `EntityResolver` retorna null pra esses no MVP; fallback graceful).
- **REQ-9.7** — Em `SheetTab`, listar todas as features adquiridas até o nível atual por classe/subclass (read-only).

### F9.4 — Multiclass

- **REQ-9.8** — `LevelUpModal` ganha radio "Subir classe existente <X>" (default) | "Adicionar classe (multiclass)".
- **REQ-9.9** — Quando "Adicionar classe": mostrar dropdown de classes disponíveis (todas as classes ativas, exceto as que o personagem já tem). Selecionar adiciona novo `CharacterClassEntry` com `Levels = 1`.
- **REQ-9.10** — Para multiclass: HP usa hit die da classe nova; outras regras (proficiencies parciais, prereqs RAW de STR/DEX/CHA 13) **não** são enforced no MVP — exibir warning informativo.
- **REQ-9.11** — `Character.TotalLevel` continua somando todas as classes; ficha mostra `Fighter 3 / Rogue 1`.

## Non-functional

- **NFR-9.1** — `FeatPickerModal` reusa visual/CSS do `ItemPickerModal` (mesmas classes `cw-modal`).
- **NFR-9.2** — Sem regressão em level-up de classe única.

## Out of scope

- Validação de pré-requisitos RAW de multiclass (STR/DEX 13, CHA 13 etc.) — só aviso.
- Spell preparation rules (preparado vs known) — para futuro.
- Cantrip swap em level-up de Bard/Sorcerer/Warlock — ignorado (RAW só permite trocar 1 cantrip ao subir).
- Sub-feat (escolhas dentro de features de subclass, ex. Maneuvers, Eldritch Invocations).

## Acceptance criteria

- AC-9.1 — Fighter 3 → 4 com feat → modal abre, "Buscar feat" → seleciona "Great Weapon Master|PHB" → Aplicar → ficha mostra `LevelChoices[4].FeatRef = Great Weapon Master|PHB`.
- AC-9.2 — Wizard 1 → 2 → modal mostra "Aprender 2 spells" → seleciona Magic Missile e Shield → ambos em `Character.KnownSpells` após apply.
- AC-9.3 — Wizard 1 → 2 modal mostra "Features ganhas: Arcane Tradition (Wizard nv 2)" como link (subclass também aparece quando escolhida).
- AC-9.4 — Fighter 3 → escolher "Adicionar classe" → Rogue. Personagem: Fighter 3 + Rogue 1, total 4. Ficha mostra ambos.
