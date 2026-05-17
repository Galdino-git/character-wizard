using CharacterWizard.App.Services;
using CharacterWizard.Core.Persistence;
using CharacterWizard.Core.Settings;
using CharacterWizard.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace CharacterWizard.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Settings & paths
        builder.Services.AddSingleton<AppPaths>();
        builder.Services.AddSingleton<AppSettingsStore>(_ => new AppSettingsStore());
        builder.Services.AddSingleton<AppSettings>(sp => sp.GetRequiredService<AppSettingsStore>().Load());

        // Data catalog
        builder.Services.AddSingleton<CatalogProvider>();
        builder.Services.AddSingleton(sp => sp.GetRequiredService<CatalogProvider>().Catalog);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<CatalogProvider>().Filter);

        // Repositories
        builder.Services.AddSingleton<BookRepository>();
        builder.Services.AddSingleton<RaceRepository>();
        builder.Services.AddSingleton<ClassRepository>();
        builder.Services.AddSingleton<BackgroundRepository>();
        builder.Services.AddSingleton<SpellRepository>();
        builder.Services.AddSingleton<ItemRepository>();
        builder.Services.AddSingleton<FeatRepository>();

        // Character persistence
        builder.Services.AddSingleton<CharacterStore>(sp =>
            new CharacterStore(sp.GetRequiredService<AppPaths>().UserDataRoot));

        // App-level singletons
        builder.Services.AddSingleton<ImageService>();
        builder.Services.AddScoped<CharacterDraft>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
