using Avalonia;
using System;

namespace MovieTracker;

class Program
{
    // Punctul de intrare al aplicatiei
    // STAThread e necesar pe Windows pentru UI threading
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()   // detecteaza automat Windows / macOS / Linux
            .WithInterFont()       // fonturi Inter (definite in NuGet)
            .LogToTrace();         // loguri in Debug Output
}
