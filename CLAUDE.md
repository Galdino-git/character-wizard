# CharacterWizard — Project Guide for Claude

Desktop D&D 5e character creator/manager consuming 5etools data. Local-only Windows app, .NET MAUI Blazor Hybrid. Inspirado no criador do Foundry VTT.

> Comunicação com o usuário: sempre em **pt-BR**. Identificadores, comentários técnicos e mensagens de commit em **inglês**.

---

## Stack

| Camada | Tech |
|--------|------|
| UI desktop | .NET MAUI Blazor Hybrid (Windows 10.0.19041.0+) |
| Renderização | Blazor (Razor) dentro de WebView |
| Estilos | Bootstrap 5 (já vem no template MAUI Blazor) |
| Backend in-process | .NET 10 (SDK 10.0.204) |
| Serialização | System.Text.Json (records + JsonElement para campos heterogêneos) |
| Testes | xUnit 2.9 |
| CLI tools | System.CommandLine |

Targeted single framework: `net10.0-windows10.0.19041.0`. iOS/Android/MacCatalyst foram removidos do csproj — não recolocar sem motivo.

---

## Estrutura do repo

```
CharacterWizard.slnx              (formato .slnx, NÃO .sln)
src/
  CharacterWizard.App/            MAUI Blazor Hybrid (entry point + UI Razor)
  CharacterWizard.Core/           Domínio puro: Character, regras, persistência
  CharacterWizard.Data/           Dialeto 5etools: models, loader, repos, filtro
tools/
  Import5eToolsData/              Console — copia JSON/imagens do repo 5etools
tests/
  CharacterWizard.Tests/          xUnit (Core + Data)
data/                             JSON + imagens importadas (gitignored)
docs/                             Specs, designs, ADRs (criados pela skill spec-driven)
```

### Princípio de separação

- **Core** nunca depende de Data ou App. Mexer só com C# puro.
- **Data** depende só de Core. Toda particularidade do schema 5etools (pipe-separated refs, tags `{@spell …}`, campos opcionais) fica isolada aqui.
- **App** depende de Core + Data. UI e DI.
- **Personagens persistidos** nunca duplicam regras — só referências `(Name, Source)`. Stats derivados são re-computados ao ler.

---

## Setup

Pré-requisitos: .NET 10 SDK, workload `maui` (já instalada nesta máquina).

```powershell
# Importar dados do 5etools (fonte em ..\5etools)
dotnet run --project tools/Import5eToolsData -- `
  --source "..\5etools" `
  --target ".\data"

# Build full solution
dotnet build CharacterWizard.slnx

# Testes
dotnet test CharacterWizard.slnx

# Rodar app
dotnet build src/CharacterWizard.App -f net10.0-windows10.0.19041.0 -t:Run
```

Workspace é Windows-only — sempre use PowerShell tool, **não** Bash (o Bash do Git interpreta `\` como escape e quebra paths).

---

## Convenções de código

- **`record`** para imutáveis (DTOs, models de dado, refs).
- **`class`** para serviços com estado (CharacterStore, ImageService).
- **Sem comentários óbvios.** Comente só o "porquê" não-trivial (workaround, invariante surpreendente). Veja `EntryRenderer` quando existir — comentários lá são justificados.
- Identificadores em inglês. Strings de UI em pt-BR. Logs em inglês.
- Não criar abstrações pra futuro hipotético. Inline > helper > abstração.
- **Não** adicionar tratamento de erro defensivo dentro de fronteiras internas. Validar só nas bordas (JSON parsing, input do usuário, FS).
- Use `JsonElement` para campos do 5etools cujo schema é heterogêneo/inconsistente. Não tentar modelar tudo.

---

## TDD + Spec-driven workflow

Toda feature nova passa por:

1. **Spec** (`docs/specs/<feature>.md`) — requisitos, comportamento esperado, casos de borda.
2. **Design** (`docs/design/<feature>.md`) — componentes afetados, novas APIs, integrações.
3. **Tasks** (`docs/tasks/<feature>.md`) — lista atômica e ordenada, cada uma com critério de verificação. Marcar `[x]` quando concluída.
4. **Testes primeiro** (xUnit em `tests/`). Vermelho → implementação mínima → verde → refactor.
5. **Commit atômico** por task concluída.

A skill `tlc-spec-driven` automatiza esse fluxo. Invocá-la para qualquer feature não-trivial.

Onde olhar o que está pronto e o que está pendente:

- `docs/ROADMAP.md` — visão de alto nível (features grandes).
- `docs/tasks/*.md` — tracking detalhado por feature, com checkboxes.

---

## Pontos de extensão importantes

### Adicionar uma nova entidade do 5etools (ex.: Monsters)
1. Criar model `XData` em `src/CharacterWizard.Data/Models/`.
2. Adicionar ao `Catalog` + `CatalogLoader.Load`.
3. Criar `XRepository` em `Repositories/Repositories.cs`.
4. Registrar singleton em `MauiProgram.cs`.
5. Se houver imagens, estender `ImageService.TryGetImage` com a categoria.

### Adicionar uma nova tag inline (`{@xxx ...}`)
1. Adicionar caso ao `EntryRenderer` (uma vez implementado em `CharacterWizard.Data/EntryRendering/`).
2. Tag clicável → registrar em `TooltipResolver` (entidade-alvo + repo de lookup).
3. Adicionar teste em `EntryRendererTests`.

### Adicionar passo no wizard de criação
1. Criar `<NomeStep>.razor` em `src/CharacterWizard.App/Components/Wizard/`.
2. Adicionar string ao array `_steps` em `NewCharacter.razor`.
3. Adicionar `case` no switch que renderiza.
4. Persistir escolha em `CharacterDraft`.

---

## Filtro de sources

`AppSettings.EnabledGroups` controla quais grupos do `books.json` (core, supplement, setting, etc.) aparecem. Default: tudo exceto `homecraft`, `organized-play`, `screen`, `recipe`, `other`. Para o usuário ativar conteúdo extra, editar `%AppData%/CharacterWizard/settings.json` (UI de configurações é trabalho futuro).

---

## Notas para Claude

- O usuário tem `data/` versionado pelo `.gitignore`. **Não** commitar.
- O repo `..\5etools` é a fonte upstream (clone público). **Nunca** mexer nele.
- `dotnet workload install maui` já foi rodado. Não rodar de novo a menos que o usuário peça.
- Build do App MAUI usa o multi-target compile do MAUI workload — sempre passar `-f net10.0-windows10.0.19041.0` explicitamente para builds/runs do App.
- Smoke-test do app (`dotnet build -t:Run`) abre uma janela e hangs — o usuário valida UX manualmente. Não tentar inspecionar a UI rodando via tooling.
- O plano original detalhado vive em `C:\Users\vinic\.claude\plans\estou-preparando-um-app-merry-hellman.md` (read-only para referência).
