using Exanite.Ecs.Systems;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.UserInterface.ViewModels;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : EcsSystem, ISetupSystem, IRenderSystem
{
    private readonly MainViewModel viewModel = new MainViewModel();

    private readonly AvaloniaRenderSystem avaloniaRenderSystem;
    private readonly TetrisSystem tetrisSystem;
    private readonly Window window;

    public TetrisUiSystem(AvaloniaRenderSystem avaloniaRenderSystem, TetrisSystem tetrisSystem, Window window)
    {
        this.avaloniaRenderSystem = avaloniaRenderSystem;
        this.tetrisSystem = tetrisSystem;
        this.window = window;
    }

    public void Setup()
    {
        avaloniaRenderSystem.TopLevel.Content = viewModel;
    }

    public void Render()
    {
        var scaleFactor = 1f;
        if (window.Size.X > 1920)
        {
            scaleFactor = 1.5f;
        }

        if (window.Size.X > 2560)
        {
            scaleFactor = 2f;
        }

        avaloniaRenderSystem.TopLevel.Impl.RenderScaling = scaleFactor;

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
