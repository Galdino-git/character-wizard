# Spec: UX & Polish iteration 1 (M6)

**Status:** Specification
**Origem:** Feedback hands-on do usuário após executar v0.2.0. 10 pontos listados; agrupados em 3 fases.

## Fase A — Bugs (esta sessão)

Bloqueiam UX básica.

- **B1** (`#6`) Bullets vazios em listas. Causa: `EntryRenderer.RenderList` despacha items objetos pra `Render()` que retorna `""` quando `type == "item"`. **Fix**: handler `RenderListItem` que monta `<li><strong>Name.</strong> body</li>` lendo `entry`/`entries`.
- **B4** (`#7`) Tooltip/click vazios em `{@skill}` `{@condition}` `{@action}` `{@sense}` `{@feature}` `{@classFeature}` `{@subclassFeature}`. Causa: `EntityResolver` só trata 6 categorias. **Fix**: novos repositórios mínimos (`ConditionRepository`, `SkillRepository`, `ActionRepository`, `SenseRepository`) carregando os JSONs já importados; pra class/subclass features, indexar arrays `classFeature[]`/`subclassFeature[]` dos arquivos de class. Resolver adiciona handlers.
- **B2** (`#9`) `EntityDetailPanel` de Class fica em branco. Causa: `EntityResolver` passa `Entries = null` para classes (no design original, classes não têm um campo `entries` único). **Fix**: sintetizar `entries` programaticamente: header (hit dice, saves, casting) + lista das `FeaturesAtLevel(20)` com refs clicáveis para detalhe individual + lista de subclasses.
- **B3** (`#10`) Backgrounds sem info. **Fix**: verificar; provavelmente resolvido por B1 (alguns backgrounds têm listas com items-objeto). Senão, investigar shape concreta e ajustar.

## Fase B — Filtros e dados (próxima sessão)

- **G1** (`#1`) Dedup por `reprintedAs[]`. Default mostra só "última versão" (entries que não estão em nenhum `reprintedAs`). Toggle "mostrar versões antigas" em Settings.
- **G2** (`#3`) Refazer UI de filtro de sources: hierarquia por edição (2014 PHB family / 2024 XPHB family) com toggles em massa.

## Fase C — Visual com tema D&D + efeitos gacha (próxima)

Direção do usuário: "tematizado como criação de ficha de personagem mesmo; efeitos estilo gacha pra visualizar escolhas; mais fluido".

- **P1** (`#2`) Paleta D&D (parchment/dourado/escarlate) + tipografia serif decorativa pra títulos.
- **P2** (`#4`) Cards de race/class/background como "summon banners": imagem em aspect ratio preservado, overlay com nome, glow/border animada na seleção, transition de slide-in. Painel lateral abre como "reveal" com fade.
- **P3** (`#5`) Painel lateral com scroll próprio (`max-height: calc(100vh - 8rem); overflow-y: auto`).
- **P4** (`#8`) Footer fixo no wizard: container largura total, botões Voltar/Avançar/Salvar bem posicionados.

## Acceptance per fase

- **AC-A** Hover/click em `Perception` numa descrição abre painel da skill. Class "Wizard" detalhe completo mostra Spellcasting, Arcane Recovery, Arcane Tradition, etc. Bullets de Aasimar (Necrotic Shroud/Radiant Consumption/Radiant Soul) aparecem com texto.
- **AC-B** Compêndio default mostra 1 Aasimar (XPHB) em vez de 4. Toggle "antigas" mostra os 4.
- **AC-C** Selecionar Wizard tem efeito visual de "summon"; cards em parchment; scroll funciona.

## Não-objetivos

- 3D / WebGL effects (CSS + minimal JS apenas).
- Animations complexas (manter <200ms transitions).
- Reescrever EntryRenderer do zero (incremental).
