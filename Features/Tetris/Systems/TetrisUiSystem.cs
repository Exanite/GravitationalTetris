using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.UserInterface.ViewModels;
using Exanite.GravitationalTetris.Features.UserInterface.Views;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : GameSystem, ISetupSystem, IRenderSystem
{
    private readonly MainViewModel viewModel = new();

    private readonly AvaloniaDisplaySystem avaloniaDisplaySystem;
    private readonly TetrisSystem tetrisSystem;
    private readonly Window window;

    public TetrisUiSystem(AvaloniaDisplaySystem avaloniaDisplaySystem, TetrisSystem tetrisSystem, Window window)
    {
        this.avaloniaDisplaySystem = avaloniaDisplaySystem;
        this.tetrisSystem = tetrisSystem;
        this.window = window;
    }

    public void Setup()
    {
        var view = new MainView();
        view.DataContext = viewModel;

        avaloniaDisplaySystem.Root.Content = view;
    }

    public void Render()
    {
        var renderScaling = 1f;
        if (window.Size.X > 1920)
        {
            renderScaling = 1.5f;
        }

        if (window.Size.X > 2560)
        {
            renderScaling = 2f;
        }

        avaloniaDisplaySystem.Root.ContentScale = renderScaling;

        viewModel.ScoreText = $"{(int)tetrisSystem.Score}";
        viewModel.PreviousScoreText = $"{(int)tetrisSystem.PreviousScore}";
        {
            var leaderboardContentText = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                if (i != 0)
                {
                    leaderboardContentText += "\n";
                }

                var score = tetrisSystem.HighScores.Count > i ? tetrisSystem.HighScores[i] : 0;

                leaderboardContentText += $"{i + 1}. {(int)score}";
            }

            viewModel.LeaderboardContentText = leaderboardContentText;
        }
        viewModel.SpeedText = $"{tetrisSystem.SpeedMultiplier:F2}x";
        viewModel.ScoreMultiplierText = $"{tetrisSystem.ScoreMultiplier:F1}x";
    }
}
