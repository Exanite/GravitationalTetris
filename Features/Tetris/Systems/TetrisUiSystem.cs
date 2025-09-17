using System;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.Engine.PaperUi;
using Exanite.Engine.Timing;
using Exanite.ResourceManagement;
using Prowl.PaperUI.LayoutEngine;
using Prowl.Scribe;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : GameSystem, IRenderUpdateSystem, IDisposable
{
    private readonly IHandle<FontFile> font;

    private readonly TetrisSystem tetrisSystem;
    private readonly Swapchain swapchain;
    private readonly ITime time;

    private readonly Lifetime lifetime = new();

    public PaperDisplay Display { get; }

    public TetrisUiSystem(TetrisSystem tetrisSystem, Swapchain swapchain, ResourceManager resourceManager, GraphicsContext graphicsContext, PaperContext paperContext, ITime time)
    {
        this.tetrisSystem = tetrisSystem;
        this.swapchain = swapchain;
        this.time = time;

        Display = new PaperDisplay(paperContext).DisposeWith(lifetime);
        font = resourceManager.GetResource(GravitationalTetrisResources.Font);
    }

    public void RenderUpdate()
    {
        var contentScale = 1.5f;
        if (swapchain.Size.X > 1920)
        {
            contentScale *= 1.25f;
        }

        if (swapchain.Size.X > 2560)
        {
            contentScale *= 1.5f;
        }

        using (Display.BeginFrame(swapchain.CommandBuffer, time.DeltaTime, swapchain.Size, contentScale))
        {
            var paper = Display.Paper;

            using (paper.Box("Container").Margin(4).Enter())
            {
                paper.Box("Score")
                    .Text($"Score: {(int)tetrisSystem.Score}", font.Value)
                    .FontSize(20)
                    .Height(24);

                paper.Box("PreviousScore")
                    .Text($"Previous Score: {(int)tetrisSystem.PreviousScore}", font.Value)
                    .FontSize(12)
                    .Height(16)
                    .Margin(0, 0, 0, 4);

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
                    .Text("Leaderboard:", font.Value)
                    .FontSize(16)
                    .Height(20);

                paper.Box("LeaderboardContent")
                    .Text($"{leaderboardContentText}", font.Value)
                    .FontSize(12)
                    .Height(16);

                paper.Box("Spacing").Size(UnitValue.Stretch());

                paper.Box("Speed")
                    .Text($"Speed: {tetrisSystem.SpeedMultiplier:F2}x", font.Value)
                    .FontSize(12)
                    .Height(16);

                paper.Box("ScoreMultiplier")
                    .Text($"Score Multiplier: {tetrisSystem.ScoreMultiplier:F1}x", font.Value)
                    .FontSize(12)
                    .Height(16);
            }
        }
    }

    public void Dispose()
    {
        lifetime.Dispose();
    }
}
