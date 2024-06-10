using CommunityToolkit.Mvvm.ComponentModel;

namespace Exanite.GravitationalTetris.Features.UserInterface.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string scoreText = "500";
    [ObservableProperty] private string previousScoreText = "250";
    [ObservableProperty] private string leaderboardContentText = """
         1. 1000
         2. 1000
         3. 1000
         4. 1000
         5. 1000
         6. 1000
         7. 1000
         8. 1000
         9. 1000
         10. 1000
         """;
    [ObservableProperty] private string speedText = "1.00x";
    [ObservableProperty] private string scoreMultiplierText = "2.0x";
}
