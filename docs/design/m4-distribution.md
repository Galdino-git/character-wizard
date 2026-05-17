# Design: Distribution (F15–F17)

## F15 — Windows packaging

### Comando de publish (alvo)

```powershell
dotnet publish src/CharacterWizard.App `
  -c Release `
  -f net10.0-windows10.0.19041.0 `
  -r win-x64 `
  --self-contained true `
  -p:WindowsPackageType=None `
  -p:PublishReadyToRun=true `
  -p:PublishSingleFile=false `
  -o artifacts/publish
```

Notas:
- `--self-contained true` empacota o runtime — usuário sem .NET roda.
- `PublishSingleFile=false` porque MAUI Blazor com WebView2 não embarca recursos nativos em um único `.exe` de forma confiável; manter como pasta publicada e zipar.
- `PublishReadyToRun=true` reduz tempo de cold start em troca de tamanho.
- `WindowsPackageType=None` mantém modo unpackaged (já está assim no csproj).

### `tools/Build-Release.ps1`

Esqueleto:

```powershell
param(
    [string]$Version = (Get-Date -Format 'yyyy.MM.dd'),
    [string]$DataSource = "C:\Prog\Projetos\CharacterWizard\5etools",
    [string]$Output = "$PSScriptRoot\..\artifacts"
)

$ErrorActionPreference = 'Stop'

# 1. Refresh data folder from upstream 5etools
dotnet run --project tools\Import5eToolsData -- --source $DataSource --target .\data

# 2. Pack data into single zip (cw-content.zip) for MauiAsset embedding
Compress-Archive -Path .\data\5etools-json, .\data\5etools-img, .\data\MANIFEST.json `
    -DestinationPath .\src\CharacterWizard.App\Resources\Raw\cw-content.zip -Force

# 3. Publish self-contained
dotnet publish src\CharacterWizard.App `
    -c Release -f net10.0-windows10.0.19041.0 -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -o "$Output\publish"

# 4. Zip the publish folder for distribution
$zip = "$Output\CharacterWizard-v$Version-win-x64.zip"
Compress-Archive -Path "$Output\publish\*" -DestinationPath $zip -Force
Write-Host "Done: $zip"
```

`docs/release.md` explica como rodar e troubleshooting (espaço em disco, antivírus do Windows etc.).

### Versionamento

`ApplicationDisplayVersion` no csproj atualizado por flag `-p:Version=...` no publish. Tags git `v{ver}` para rastreabilidade.

## F16 — Embedded data + first-run extraction

### MauiAsset

Em `CharacterWizard.App.csproj` já existe:

```xml
<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
```

`cw-content.zip` cabe nesse glob. Não precisa mudar nada no csproj.

`.gitignore` adiciona `src/CharacterWizard.App/Resources/Raw/cw-content.zip` para não commitar (regenerado por build).

### Bootstrap extraction

Novo serviço `ContentBootstrapper` em `CharacterWizard.App/Services/`:

```csharp
public sealed class ContentBootstrapper
{
    public string ContentRoot { get; }              // %AppData%/CharacterWizard/content
    public string EmbeddedHashFile => Path.Combine(ContentRoot, ".installed-hash");

    public ContentBootstrapper(AppPaths paths) { ... }

    /// <summary>Run before MauiApp.Build(). Synchronous, idempotent.</summary>
    public async Task EnsureExtractedAsync()
    {
        await using var zip = await FileSystem.OpenAppPackageFileAsync("cw-content.zip");
        var hash = await ShortHashAsync(zip);
        zip.Position = 0;

        var installedHash = File.Exists(EmbeddedHashFile) ? await File.ReadAllTextAsync(EmbeddedHashFile) : null;
        if (installedHash == hash && Directory.Exists(Path.Combine(ContentRoot, "5etools-json")))
            return; // up-to-date

        if (Directory.Exists(ContentRoot)) Directory.Delete(ContentRoot, recursive: true);
        Directory.CreateDirectory(ContentRoot);
        using var archive = new ZipArchive(zip, ZipArchiveMode.Read);
        archive.ExtractToDirectory(ContentRoot, overwriteFiles: true);
        await File.WriteAllTextAsync(EmbeddedHashFile, hash);
    }
}
```

Chamada em `MauiProgram.CreateMauiApp`:

```csharp
var paths = new AppPaths();
var bootstrap = new ContentBootstrapper(paths);
bootstrap.EnsureExtractedAsync().GetAwaiter().GetResult();  // intentionally sync — must complete pre-DI
```

### `AppPaths.DataRoot`

Mudar para apontar diretamente para `%AppData%/CharacterWizard/content/`. Remove o walk-up. Em dev, se a pasta não existir, fallback para `..\data` do projeto via env var `CW_DATA_OVERRIDE` ou via checagem secundária — pra preservar o fluxo de dev sem ter que regenerar a content.zip a cada execução.

```csharp
private static string ResolveDataRoot()
{
    var envOverride = Environment.GetEnvironmentVariable("CW_DATA_OVERRIDE");
    if (!string.IsNullOrEmpty(envOverride) && Directory.Exists(envOverride))
        return envOverride;

    var appData = Path.Combine(...AppData..., "CharacterWizard", "content");
    if (Directory.Exists(Path.Combine(appData, "5etools-json")))
        return appData;

    // Dev fallback
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null) { ...walk-up... }
    return appData; // bootstrapper will populate
}
```

## F17 — Manual content update flow

### Settings panel

Em `Settings.razor`, nova seção no topo:

```razor
<h5>Conteúdo do 5etools</h5>
@if (_manifest is not null)
{
    <div class="small text-muted">
        Importado em @_manifest.ImportedAtUtc · hash <code>@_manifest.SourceHash</code> · 
        @_manifest.Counts.Json JSONs, @_manifest.Counts.Images imagens
    </div>
}
<button class="btn btn-sm btn-outline-secondary" @onclick="ReExtract">Reextrair conteúdo embarcado</button>
```

`ReExtract`: chama `ContentBootstrapper.ForceReextractAsync()`, mostra status.

### Manifest model

Reusa o que o importer já grava:

```csharp
public sealed record ContentManifest(
    string ImportedAtUtc,
    string SourcePath,
    string? SourceHash,
    ManifestCounts Counts);
public sealed record ManifestCounts(int Json, int Images, long ImageBytes);
```

Carregado por `ManifestReader` no Settings page.

### CLI update path

`tools/Import5eToolsData` continua funcionando. Para regenerar o conteúdo embarcado:

```powershell
.\tools\Build-Release.ps1 -Version 0.2.0
```

Re-publica do zero. Sem mecanismo "live update" do app instalado — usuário baixa novo zip.

## Tests

- `ContentBootstrapper_extracts_when_hash_missing` (in-memory test com ZIP construído na memória).
- `ContentBootstrapper_skips_extraction_when_hash_matches`.
- `ContentBootstrapper_reextracts_when_hash_differs`.
- `ManifestReader_parses_valid_manifest`.

Não há teste E2E do publish (custa muito tempo CI); valida-se manualmente conforme AC-15.

## Decisões registradas

- **ZIP > MSIX** para uso pessoal — menos fricção, sem assinatura.
- **ZIP único de content** > muitos `MauiAsset` individuais — performance de cópia 100× melhor.
- **Extração no startup** > on-demand — usuário pagaria custo na primeira navegação a /search.
- **Reextração via app** mas **sem auto-update via internet** — escopo claro.
- **Hash de invalidação** em vez de timestamp — survive copy/move da pasta.

## Riscos

- **MAUI publish quirks** em .NET 10 — pode haver issues com runtime version. Mitigação: documentar resolução em `release.md`.
- **WebView2 runtime** — Windows 11 já tem; Windows 10 pode precisar de install separado (Microsoft Edge Update). Documentar em README.
- **Antivírus** marca self-contained `.exe` desconhecido como suspeito — incluir hash no release.
- **Tamanho do `.zip`** — se chegar perto de 300 MB, considerar reduzir imagens (compressão WebP mais agressiva ou descartar covers).
