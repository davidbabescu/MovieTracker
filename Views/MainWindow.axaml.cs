using Avalonia.Controls;

namespace MovieTracker.Views;

// "partial class" lucreaza impreuna cu MainWindow.axaml
// Avalonia genereaza automat codul de initializare XAML in cealalta jumatate a clasei
// In mod normal nu avem nevoie de logica suplimentara aici - totul e in ViewModel
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();  // ← asta lipsea
    }
}