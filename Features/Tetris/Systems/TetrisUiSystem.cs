using System;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.PaperUi;
using Exanite.ResourceManagement;
using Prowl.PaperUI.LayoutEngine;
using Prowl.Scribe;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class TetrisUiSystem : GameSystem, IRenderUpdateSystem, IDisposable
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
        var paper = display.Paper;
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

    public void Dispose()
    {
        lifetime.Dispose();
    }
}
