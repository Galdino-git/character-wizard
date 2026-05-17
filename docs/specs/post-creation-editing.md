# Spec: Post-Creation Editing

**Feature ID:** F3
**Status:** Specification
**Owner:** —
**Depends on:** F1 (SearchService — usado pelo item picker).

## Context

O personagem salvo é mostrado em `CharacterView.razor` como ficha read-only. Esta feature adiciona edição direta de campos que mudam durante a campanha: inventário (com bônus customizados) e overrides de ability score (ex.: "Tomo da Chama Bardica deu +2 CHA permanente"). É a forma do jogador reagir a eventos da mesa sem ter que recriar o personagem.

## User stories

- **US-3.1** — Como jogador, quando meu personagem ganha um item mágico na campanha, quero adicioná-lo ao inventário escolhendo da lista existente do compêndio.
- **US-3.2** — Quero renomear esse item para refletir um bônus (`Longsword` → `Espada Élfica +2`) e registrar `+2 atq`/`+2 dano` separados.
- **US-3.3** — Quero remover itens consumidos ou perdidos e alterar quantidades de itens empilháveis.
- **US-3.4** — Quero registrar um buff permanente que o DM concedeu (ex.: +2 CHA por tomo). A ficha deve mostrar o score final com o buff aplicado.
- **US-3.5** — Quero anotar coisas livres na ficha (objetivos da campanha, contatos, etc.).
- **US-3.6** — Todas as edições persistem automaticamente; ao reabrir o app vejo as mesmas alterações.

## Functional requirements

- **REQ-3.1** — `CharacterView` ganha estrutura de abas: `Ficha` (atual, read-only) · `Inventário` · `Buffs & Notas`.
- **REQ-3.2** — Tab `Inventário`: lista todos os `InventoryItem`s do personagem com colunas: nome (CustomName ?? ItemRef.Name), quantidade, ataque/dano/AC bônus, equipado (toggle), ação remover.
- **REQ-3.3** — Tab `Inventário` tem botão `+ Adicionar item` que abre modal com `ItemPicker` — busca via `SearchService` restrita a categoria `Item`.
- **REQ-3.4** — Ao adicionar um item, o usuário pode editar `CustomName`, `Quantity`, `BonusAttack`, `BonusDamage`, `BonusAc`, `Notes` antes de salvar.
- **REQ-3.5** — Itens existentes na lista podem ser editados (mesma form em modal).
- **REQ-3.6** — Tab `Buffs & Notas`: lista de `AbilityOverride`s com Ability, Delta (+/−), Reason. Adicionar/editar/remover. Exibir total agregado por ability.
- **REQ-3.7** — Tab `Buffs & Notas` inclui um textarea de `Notes` (campo já existente no `Character`) para texto livre.
- **REQ-3.8** — Toda alteração chama `CharacterStore.Save(c)` imediatamente (sem botão de salvar separado).
- **REQ-3.9** — A tab `Ficha` continua mostrando o cálculo final de stats — `AbilityScoreCalculator.ComputeFinal` já agrega os overrides.

## Non-functional

- **NFR-3.1** — Salvamento é síncrono (in-memory + file write). Operação considera-se completa quando o arquivo JSON está atualizado em `%AppData%/CharacterWizard/characters/{id}.json`.
- **NFR-3.2** — Backup automático já existente no `CharacterStore` (`_history/{id}/`) deve continuar funcionando: cada edição gera uma versão histórica.
- **NFR-3.3** — Sem confirmação de "tem certeza?" para remover itens — o backup serve de safety net. Se ficar incômodo, adicionar depois.

## Out of scope (deste milestone)

- Drag-and-drop de reorder no inventário.
- Categorização de itens (ex.: "consumíveis" vs "equipados").
- Histórico de transações (ganhei/perdi item em X data).
- Cálculo automático de capacity (peso/encumbrance).
- Edição de spells conhecidas/preparadas (próximo milestone).
- Edição de race/class/background (mexer no personagem original requer recriar).

## Acceptance criteria

- AC-3.1 — Abrir personagem existente e clicar na tab `Inventário` mostra a lista. Clicar `+ Adicionar item`, buscar "longsword", selecionar, ajustar `CustomName = "Espada Élfica +2"`, `BonusAttack = 2`, `BonusDamage = 2`, salvar. Item aparece na lista com nome customizado.
- AC-3.2 — Fechar e reabrir o app: item persiste com mesmos valores.
- AC-3.3 — Editar quantidade de um item para 0 não remove automaticamente; usuário precisa clicar `Remover`.
- AC-3.4 — Na tab `Buffs & Notas`, adicionar `Cha +2 "Tomo"` muda o valor mostrado na tab `Ficha` no header (ex.: CHA passa de 14 para 16).
- AC-3.5 — Remover o override volta o score original.
- AC-3.6 — Editar campo `Notes` e mudar de tab/voltar mantém o texto.
- AC-3.7 — Verificar `%AppData%/CharacterWizard/characters/_history/{id}/` recebe uma nova versão a cada save.
