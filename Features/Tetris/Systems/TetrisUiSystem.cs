using Exanite.Engine.Avalonia;
using Exanite.Engine.Avalonia.Components;
using Exanite.Engine.Cameras.Components;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.GravitationalTetris.Features.UserInterface.ViewModels;
using Exanite.GravitationalTetris.Features.UserInterface.Views;
using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : GameSystem, ISetupSystem, IRenderSystem
{
    private EcsCommandBuffer commandBuffer = null!;

    private readonly MainViewModel viewModel = new();

    private readonly AvaloniaContext avaloniaContext;

    private readonly TetrisSystem tetrisSystem;
    private readonly Swapchain swapchain;

    public AvaloniaRootElement UiRoot { get; private set; } = null!;

    public TetrisUiSystem(TetrisSystem tetrisSystem, Swapchain swapchain, AvaloniaContext avaloniaContext)
    {
        this.tetrisSystem = tetrisSystem;
        this.swapchain = swapchain;
        this.avaloniaContext = avaloniaContext;
    }

    public void Setup()
    {
        commandBuffer = new EcsCommandBuffer(World);

        // UI
        UiRoot = avaloniaContext.CreateRootElement();
        UiRoot.Content = new MainView()
        {
            DataContext = viewModel,
        };

        commandBuffer.Create()
            .Set(new ComponentAvaloniaDisplay(UiRoot))
            .Set(new ComponentWindowViewport(swapchain));

        commandBuffer.Execute();
    }

    public void Render()
    {
        var renderScaling = 1f;
        if (swapchain.Texture.Desc.Size.X > 1920)
        {
            renderScaling = 1.5f;
        }

        if (swapchain.Texture.Desc.Size.X > 2560)
        {
            renderScaling = 2f;
        }

        UiRoot.ContentScale = renderScaling;

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
