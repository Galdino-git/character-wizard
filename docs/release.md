# Release Guide

Como gerar um novo release ZIP e publicar no GitHub.

---

## Pré-requisitos (uma vez por máquina dev)

- **.NET 10 SDK** (`dotnet --list-sdks` → deve incluir 10.x)
- **MAUI workload** instalada (`dotnet workload install maui`)
- Repositório do **5etools** clonado em `..\5etools` (ou outro path; passe via `-DataSource`):
  ```powershell
  cd C:\Prog\Projetos\CharacterWizard
  git clone https://github.com/TheGiddyLimit/TheGiddyLimit.github.io.git 5etools
  ```
- **git** configurado com seu usuário GitHub (push autenticado funcionando).
- **gh CLI** opcional (https://cli.github.com/) — facilita criar a release via terminal. Sem ele, criar pela web UI funciona igualmente.

---

## Gerar uma nova release

```powershell
cd C:\Prog\Projetos\CharacterWizard\CharacterWizard
.\tools\Build-Release.ps1 -Version 0.1.0
```

O script:
1. Re-importa dados do 5etools (passe `-SkipImport` se já estiver atualizado).
2. `dotnet publish` self-contained `win-x64`.
3. Copia `data/` ao lado do `.exe`.
4. Empacota `artifacts/CharacterWizard-v0.1.0-win-x64.zip` (~266 MB).

Flags úteis:
- `-DataSource "D:\path\to\5etools"` — se o repo 5etools está em outro local.
- `-SkipImport` — pula etapa 1, reusa o que estiver em `.\data` (útil pra testar o script).

---

## Validar localmente antes de publicar

```powershell
$tmp = "$env:TEMP\cw-test-$(Get-Date -Format yyyyMMddHHmmss)"
Expand-Archive artifacts\CharacterWizard-v0.1.0-win-x64.zip -DestinationPath $tmp
Start-Process "$tmp\CharacterWizard.App.exe"
```

Checklist:
- [ ] Janela abre sem erro
- [ ] Lista de personagens renderiza
- [ ] Compêndio (`/search`) mostra entidades
- [ ] Criação de personagem funciona até salvar

Se OK, prossiga.

---

## Publicar release no GitHub

### Opção A — `gh` CLI (recomendado)

```powershell
git tag v0.1.0
git push origin v0.1.0

gh release create v0.1.0 `
    artifacts\CharacterWizard-v0.1.0-win-x64.zip `
    --title "v0.1.0" `
    --notes-file docs\release-notes\v0.1.0.md
```

Crie `docs/release-notes/v0.1.0.md` antes (ou passe `--notes "texto curto"`).

### Opção B — Web UI

1. `git tag v0.1.0 && git push origin v0.1.0`
2. Abra https://github.com/Galdino-git/character-wizard/releases/new
3. "Choose a tag" → selecione `v0.1.0` (que acabou de subir)
4. Title: `v0.1.0`
5. Description: cole o release notes
6. "Attach binaries" → arraste `artifacts/CharacterWizard-v0.1.0-win-x64.zip`
7. Publish release

---

## Instruções para o usuário final

Coloque no corpo de cada release notes:

```markdown
### Como usar

1. Baixe `CharacterWizard-vX.Y.Z-win-x64.zip` abaixo.
2. Extraia em qualquer pasta (não precisa de admin).
3. Duplo clique em `CharacterWizard.App.exe`.

### Possíveis avisos

- **SmartScreen**: "Windows protegeu o seu PC" → clique em **Mais informações** → **Executar mesmo assim**.
  O aviso aparece porque o `.exe` não está assinado com certificado comercial.
- **WebView2 não encontrado** (raro): instale de https://go.microsoft.com/fwlink/p/?LinkId=2124703.
  Windows 11 já vem com ele; Windows 10 atualizado também.

### Requisitos

- Windows 10 1809+ ou Windows 11 (x64).
- ~600 MB de espaço em disco para a pasta extraída.
- Não precisa instalar .NET, MAUI ou qualquer dependência.
```

---

## Troubleshooting

| Sintoma | Causa | Solução |
|---------|-------|---------|
| `dotnet publish` falha com "workload not installed" | MAUI não instalado | `dotnet workload install maui` |
| `Build-Release.ps1` falha em "Importer failed" | Pasta `..\5etools` ausente | Clonar 5etools ou passar `-DataSource` |
| Antivírus apaga o exe | Self-contained sem assinatura é falso-positivo comum | Adicionar exclusão temporária; documentar no release notes |
| App abre mas tela branca | WebView2 ausente | Instalar do link acima |
| Catálogo vazio | `data/` não foi copiado para o publish | Verificar que `Build-Release.ps1` rodou até o fim |
| Erro `JsonException` no startup | JSON corrompido durante export do data | Re-rodar o importer (`dotnet run --project tools\Import5eToolsData`) |
| ZIP > 2 GB | Saiu fora do escopo | Conferir se não copiou bestiário acidentalmente |
