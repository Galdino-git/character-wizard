# Tasks: Distribution (M4)

Spec: [`../specs/m4-distribution.md`](../specs/m4-distribution.md) · Design: [`../design/m4-distribution.md`](../design/m4-distribution.md)

Tudo simples — sem mudança no código do app.

## T1 — Validar publish self-contained

- [ ] Rodar `dotnet publish src/CharacterWizard.App -c Release -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained true -p:WindowsPackageType=None -o artifacts/publish-test`.
- [ ] Copiar `data/` para `artifacts/publish-test/data/`.
- [ ] Executar o `.exe` da pasta gerada num cmd limpo (fora do dev environment). Verificar que abre, lista personagens, e o catálogo carrega.
- [ ] Anotar em `release.md` qualquer warning/erro encontrado.

## T2 — Script `Build-Release.ps1`

- [ ] Criar `tools/Build-Release.ps1` (ver design).
- [ ] Adicionar `artifacts/` ao `.gitignore`.
- [ ] Testar `tools/Build-Release.ps1 -Version 0.1.0-test` → deve produzir `artifacts/CharacterWizard-v0.1.0-test-win-x64.zip` sem erros.
- [ ] Extrair o ZIP em pasta temp e validar que executa.
- [ ] Commit `build: release packaging script (T2)`.

## T3 — Guia `docs/release.md`

- [ ] Escrever `docs/release.md` com:
    - Pré-requisitos
    - Passo-a-passo do `Build-Release.ps1`
    - Validação local
    - Tag git + `gh release create`
    - Seção "Instruções para o usuário final" (SmartScreen, WebView2)
    - Troubleshooting (erros comuns)
- [ ] Commit `docs: release guide (T3)`.

## T4 — README pro usuário final

- [ ] Atualizar `README.md` com seção "Para usar (não-dev)" apontando para Releases + 4 passos rápidos.
- [ ] Commit `docs: end-user install section in README (T4)`.

## T5 — Push pro GitHub

- [ ] Configurar `git remote add origin <URL do user>`.
- [ ] `git push -u origin main`.
- [ ] Conferir que docs/ e código aparecem no repo.

## T6 — Primeira release

- [ ] Rodar `Build-Release.ps1 -Version 0.1.0`.
- [ ] `git tag v0.1.0 && git push origin v0.1.0`.
- [ ] `gh release create v0.1.0 artifacts\CharacterWizard-v0.1.0-win-x64.zip --title "v0.1.0 — first public build" --notes-file docs/release-notes/v0.1.0.md` (criar release notes inline ou em arquivo).
- [ ] Conferir o link público da release; baixar e testar uma vez mais.
- [ ] Commit nada novo (tag + release já estão no GitHub).

## Final

- [ ] ROADMAP marca M4 done.
- [ ] Commit `docs: mark M4 complete`.
