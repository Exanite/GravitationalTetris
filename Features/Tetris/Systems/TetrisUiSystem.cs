using System;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.PaperUi;
using Exanite.Prowl.Paper;
using Exanite.Prowl.Paper.Scribe;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : EngineSystem, IRenderUpdateSystem, IDisposable
{
    private readonly IHandle<FontFile> font;

    private readonly TetrisSystem tetrisSystem;
    private readonly PaperDisplay display;

    private readonly Lifetime lifetime = new();

    public TetrisUiSystem(TetrisSystem tetrisSystem, ResourceManager resourceManager, PaperDisplay display)
    {
        this.tetrisSystem = tetrisSystem;
        this.display = display;

        font = resourceManager.GetResource(GravitationalTetrisResources.Font);
    }

    public void RenderUpdate()
    {
        using (display.Column("Container").Grow().Margin(4).BetweenVertical(2).Enter())
        {
            display.Box("Score").Text($"Score: {(int)tetrisSystem.Score}", font.Value).FontSize(20);
            display.Box("PreviousScore").Text($"Previous Score: {(int)tetrisSystem.PreviousScore}", font.Value).FontSize(12).MarginBottom(4);

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

            display.Box("Leaderboard").Text("Leaderboard:", font.Value).FontSize(16);
            display.Box("LeaderboardContent").Text($"{leaderboardContentText}", font.Value).FontSize(12);

            display.Box("Speed").Text($"Speed: {tetrisSystem.SpeedMultiplier:F2}x", font.Value).FontSize(12).MarginTop(LayoutSize.Grow);
            display.Box("ScoreMultiplier").Text($"Score Multiplier: {tetrisSystem.ScoreMultiplier:F1}x", font.Value).FontSize(12);
        }
    }

    public void Dispose()
    {
        lifetime.Dispose();
    }
}
