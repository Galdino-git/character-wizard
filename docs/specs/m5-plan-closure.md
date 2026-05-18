# Spec: Plan Closure / Mechanical Completeness (M5)

**Status:** Specification
**Origem:** Cross-check do plano original (`C:\Users\vinic\.claude\plans\estou-preparando-um-app-merry-hellman.md`) contra o entregue após M4. Estes são os itens que estavam no plano e ficaram pendentes ou parciais.

## Features

### F18 — HP máximo na Ficha
Plano (verificação E2E item 4): "Verificar: 6 HP iniciais...". Hoje `CharacterClassEntry.HitPointRolls[]` armazena rolls mas não há widget agregador.
- `HitPointCalculator.ComputeMaxHp(Character, ClassRepository)` puro.
- Widget na SheetTab: "HP máximo: 45".

### F19 — Level-ups percorridos durante criação em nível avançado
Plano (wizard step 7): "Escolhas por nível (se nível inicial > 1, percorre cada level-up)". Hoje cria em nv 6 e não pergunta ASI/feat de nv 4. Apenas o `LevelUpModal` pós-criação faz isso.
- Novo step no wizard: "Escolhas por nível" — aparece quando `InitialLevel > 1`.
- Lista os níveis 2..N com painéis colapsáveis; em cada nível ASI/feat, HP mode, subclass pick (se cruza o threshold), spells aprendidos.
- Persiste em `Character.ChoicesByLevel` e `HitPointRolls`.
- MVP: só pra classe primária; multiclass adicional fica com Levels-but-no-choices (registrar como aceito).

### F20 — Aba Spells dedicada no CharacterView
Plano (editar personagem): "Spells (slots/known/prepared)". Hoje não existe.
- Nova aba "Spells" mostrando:
    - Tabela de spell slots por nível (via `SpellSlotTable.GetSlotRow(casterLevel)`).
    - Lista de cantrips conhecidos.
    - Lista de spells conhecidos agrupados por nível.
    - Toggle de "preparada" por spell (quando classe usa preparação — Wizard/Cleric/Druid/Paladin).
    - Contador de preparados vs limite (`level + ability_mod`).
    - Botão "+ Aprender spell" → `SpellPickerModal` (novo, irmão do `ItemPickerModal`).

### F21 — Métodos extras de ability scores
Plano (wizard step 1): "método de ability scores (point-buy / standard array / manual / roll)". Hoje só standard array + manual.
- Adicionar **Point-buy** ao `AbilitiesStep`: pool 27 pts, custos PHB (8→0, 9→1, 10→2, 11→3, 12→4, 13→5, 14→7, 15→9), scores limitados a 8-15.
- Adicionar **Roll (4d6 drop lowest)**: botão rola 6 valores, usuário arrasta/atribui a cada ability.

### F22 — Painel lateral de preview no wizard
Plano (wizard Race step): "grid com imagens, painel lateral com descrição renderizada". Hoje só grid + modal por ⓘ.
- Layout 2 colunas em Race/Class/Background steps: esquerda=grid, direita=preview da seleção atual com `<EntryDisplay>`.
- Mantém o botão ⓘ (que abre modal full) como atalho.

### F23 — ProficiencyResolver (saves)
Plano (arquivo crítico 7): "ProficiencyResolver". Hoje saves/skills aparecem só implicitamente.
- `ProficiencyResolver.ComputeSaves(Character, ClassRepo)` retorna `Dictionary<Ability, int>` (DC = score + prof se save proficient).
- Lê `cls.SavingThrowProficiencies` (já modelado).
- Widget na SheetTab: linha "Saves: STR -1 / DEX +2 / CON +2 / INT +5* / WIS +2* / CHA +0" (asterisco = proficient).
- Skills ficam fora (depende de pick UI inexistente — TODO).

### F24 — Recarregar catálogo do disco
Plano (tela inicial): "Reimportar dados". Em release o usuário não tem como re-importar (sem repo 5etools); em dev a CLI faz. Compromisso útil pra ambos:
- Botão "Recarregar catálogo do disco" na Settings page — chama `CatalogProvider.Reload()` (re-roda `CatalogLoader.Load`).
- Útil em dev quando você roda a CLI fora; em release seria útil se manualmente trocar arquivos em `data/`.

### F26 — Auto-open último personagem
Plano (settings): "último personagem aberto". Hoje `AppSettings.LastOpenedCharacterId` está no model mas nada lê/escreve.
- `CharacterView.OnParametersSet` salva o id atual no settings.
- Home mostra um botão "Reabrir <Nome>" no topo quando `LastOpenedCharacterId` aponta pra um personagem existente.

### F25 — Refactor `CharacterCreationService` / `LevelUpService` *(opcional)*
Plano (arquivo crítico 8). Hoje a lógica vive em componentes Razor. Refactor pra services em Core não muda comportamento, só estilo.
- **Adiada** salvo se sobrar tempo no fim — não é regressão funcional.

## Non-functional

- **NFR-1** — Todas as alterações cobertas por testes onde a lógica é pura (calculators, resolver, point-buy cost, roll).
- **NFR-2** — Sem regressão em fluxos existentes.

## Out of scope

- Pick UI de skills (dependeria de modelo separado de "skill choices made"; futuro M6).
- Current HP / damage / temp HP (combat tracker, não é escopo).
- Refactor de services se não couber.
- Spells "always known" via subclass features (Domain spells, Cleric, etc.) — listadas só implicitamente via features.

## Acceptance criteria por feature

- AC-18 — Personagem Fighter nv 5 com CON 14 mostra "HP máximo: 38" na Ficha (10+CON + 4×(6+CON) = 10+2 + 4×8 = 44 — exato depende dos rolls).
- AC-19 — Criar Wizard nv 4 do zero: wizard pede ASI/feat no nv 4 antes de salvar.
- AC-20 — Wizard nv 5 com 6 spells conhecidos: aba Spells mostra slots 4/3/2 + 6 spells; marcar 4 como prepared funciona.
- AC-21 — Em Atributos, escolher Point-buy, alocar 15/15/15/8/8/8 → custo total 27 (3×9 + 0×3); contador zera. Roll: gerar 6 valores e arrastar pra slots.
- AC-22 — Selecionar uma race no grid mostra descrição renderizada à direita imediatamente, sem clicar.
- AC-23 — Wizard mostra "Save INT: +5*" (asterisco pra proficient).
- AC-24 — Clicar "Recarregar catálogo" na Settings reflete mudanças manuais no `data/`.
- AC-26 — Após abrir um personagem, fechar app, reabrir: botão "Reabrir <Nome>" no topo da Home.
