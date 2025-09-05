using System;
using System.Drawing;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.Engine.Inputs;
using Exanite.Engine.PaperUi;
using Exanite.Engine.Timing;
using Exanite.Engine.Windowing.Sdl;
using Exanite.ResourceManagement;
using Prowl.PaperUI.LayoutEngine;
using Prowl.Scribe;
using TextAlignment = Prowl.PaperUI.TextAlignment;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : GameSystem, IRenderSystem, IDisposable
{
    private readonly IHandle<FontFile> font;

    private readonly TetrisSystem tetrisSystem;
    private readonly Swapchain swapchain;
    private readonly ITime time;

    private readonly DisposableCollection disposables = new();

    public PaperDisplay Display { get; private set; }

    public TetrisUiSystem(TetrisSystem tetrisSystem, Swapchain swapchain, ResourceManager resourceManager, GraphicsContext graphicsContext, Input input, SdlContext sdlContext, ITime time)
    {
        this.tetrisSystem = tetrisSystem;
        this.swapchain = swapchain;
        this.time = time;

        Display = new PaperDisplay(graphicsContext, resourceManager, input, sdlContext).AddTo(disposables);
        font = resourceManager.GetResource(GravitationalTetrisResources.Font);
    }

    public void Render()
    {
        var contentScale = 1.5f;
        if (swapchain.Texture.Desc.Size.X > 1920)
        {
            contentScale *= 1.5f;
        }

        if (swapchain.Texture.Desc.Size.X > 2560)
        {
            contentScale *= 2f;
        }

        using (Display.BeginFrame(swapchain.CommandBuffer, time.DeltaTime, swapchain.Texture.Desc.Size))
        {
            var paper = Display.Paper;

            using (paper.Box("Container").Margin(4 * contentScale).Enter())
            {
                paper.Box("Score")
                    .Text($"Score: {(int)tetrisSystem.Score}", font.Value)
                    .FontSize(20 * contentScale)
                    .Height(24 * contentScale);

                paper.Box("PreviousScore")
                    .Text($"Previous Score: {(int)tetrisSystem.PreviousScore}", font.Value)
                    .FontSize(12 * contentScale)
                    .Height(16 * contentScale);

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

                paper.Box("Leaderboard")
                    .Text($"Leaderboard:", font.Value)
                    .FontSize(16 * contentScale)
                    .Height(20 * contentScale);

                paper.Box("LeaderboardContent")
                    .Text($"{leaderboardContentText}", font.Value)
                    .FontSize(12 * contentScale)
                    .Height(16 * contentScale);

                paper.Box("Spacing").Size(UnitValue.Stretch());

                paper.Box("Speed")
                    .Text($"Speed: {tetrisSystem.SpeedMultiplier:F2}x", font.Value)
                    .FontSize(12 * contentScale)
                    .Height(16 * contentScale);

                paper.Box("ScoreMultiplier")
                    .Text($"Score Multiplier: {tetrisSystem.ScoreMultiplier:F1}x", font.Value)
                    .FontSize(12 * contentScale)
                    .Height(16 * contentScale);
            }
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
