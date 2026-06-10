using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MovieTracker.Services;
using MovieTracker.ViewModels;
using MovieTracker.Views;

namespace MovieTracker;

public partial class App : Application
{
    public override void Initialize()
    {
        // Incarca fisierul App.axaml si aplica stilurile/resursele din el
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Instantiaza serviciile si le injecteaza in ViewModel (Dependency Injection manual)
            // MovieApiService  -> apeluri HTTP catre TMDB
            // StorageService   -> citire/scriere JSON pe disc
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(
                    new MovieApiService(),
                    new StorageService()
                ),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
