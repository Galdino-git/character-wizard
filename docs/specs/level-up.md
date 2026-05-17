# Spec: Level Up (F8)

**Feature ID:** F8
**Status:** Specification
**Depends on:** F3 (`CharacterView` tabs, `Store.Save`), F5 (subclass selection — necessário se atinge o nível de subclass agora).

## Context

Após a criação, o personagem evolui. O DM concede um nível; o jogador precisa subir o personagem registrando: HP ganho, ASI/feat (se aplicável), eventual subclass (quando atinge o nível), e spells novos (se conjurador).

## User stories

- **US-8.1** — Como jogador, ao chegar do nível 4 → 5 com meu Wizard, clico "Subir nível" e marco que escolhi um ASI de +2 INT.
- **US-8.2** — Wizard nível 1 → 2 deve me obrigar a escolher uma Arcane Tradition (subclass aparece no nível 2).
- **US-8.3** — Ao subir, o HP é incrementado automaticamente pela média do dado de hit + CON modifier.
- **US-8.4** — Posso decidir entre rolar o HP ou pegar a média (default: média).

## Functional requirements

- **REQ-8.1** — Botão "Subir nível" no header de `CharacterView` (visível quando `TotalLevel < 20`).
- **REQ-8.2** — Abrir um modal com:
    - Display da classe que vai subir (apenas a classe existente — multiclass é M3).
    - Modo de HP: `média (default)` | `máximo` | `valor manual`.
    - Se o novo nível é 4/8/12/16/19 para a classe: form "ASI vs Feat" — ASI = `Dictionary<Ability,int>`, Feat = picker.
    - Se o novo nível é o nível de subclass (`SubclassChoiceLevel`) e o personagem não tem subclass: forçar pick.
- **REQ-8.3** — Confirmar aplica:
    - `CharacterClassEntry.Levels++`
    - `Character.ChoicesByLevel[newTotalLevel] = { AsiOrFeat, AsiAllocation/FeatRef }` (quando aplicável)
    - `CharacterClassEntry.HitPointRolls.Add(rolledOrAverage)` para preservar histórico
    - `CharacterClassEntry.SubclassRef = picked` quando subclass nova
    - `Store.Save`
- **REQ-8.4** — Aba `Ficha` reflete: nível total atualizado, ability scores recomputados, proficiency bonus novo.

## Non-functional

- **NFR-8.1** — Sem rollback de level-up no MVP (o backup automático em `_history/` é o safety net).
- **NFR-8.2** — Sem visualização de "previa diferenças antes de aplicar" — direto.

## Out of scope

- Multiclass (subir para uma classe diferente da atual).
- Cálculo automático de spells aprendidos por level-up (usuário usa search/picker pós-level-up via aba Spells — fica para feature futura).
- Features de classe/subclass auto-anunciadas no level-up — usuário consulta o painel de detalhe.
- Roll de HP via dice notation visual.

## Acceptance criteria

- AC-8.1 — Wizard nível 1 (Schoolless): clicar "Subir nível" → modal pede subclass (REQ-8.2). Selecionar Evocation, HP=média (4 + CON mod), confirmar. Ficha mostra nível 2, subclass Evocation, HP novo.
- AC-8.2 — Fighter nível 4 → 5: pula prompt de ASI (não é nível ASI). HP+= average. Pronto.
- AC-8.3 — Fighter nível 3 → 4: pede ASI vs Feat. Escolher ASI +1 STR +1 CON. Ficha mostra STR +1, CON +1 via `AbilityScoreCalculator`.
- AC-8.4 — Reload do app: tudo persiste.
