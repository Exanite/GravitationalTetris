using Exanite.ResourceManagement;
using Exanite.WarGames.Systems;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace Exanite.WarGames.Features.Tiles.Systems;

public class TetrisUiSystem : EcsSystem, IStartSystem, IUpdateSystem, IDrawSystem
{
    private Desktop desktop = null!;

    private readonly Game game;
    private readonly ResourceManager resourceManager;
    private readonly TetrisSystem tetrisSystem;

    public TetrisUiSystem(Game game, ResourceManager resourceManager, TetrisSystem tetrisSystem)
    {
        this.game = game;
        this.resourceManager = resourceManager;
        this.tetrisSystem = tetrisSystem;
    }

    public void Start()
    {
        var assetManager = new AssetManager(resourceManager);

        MyraEnvironment.Game = game;
        MyraEnvironment.DefaultAssetManager = assetManager;
        DefaultAssets.AssetManager = assetManager;

        desktop = new Desktop();
    }

    public void Update()
    {
        var mainGrid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8,
        };

        mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        {
            var score = new Label
            {
                Text = $"Score: {tetrisSystem.Score}\n",
                Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(32),
            };

            mainGrid.Widgets.Add(score);
            Grid.SetRow(score, 0);
            Grid.SetColumn(score, 0);
        }

        {
            var leaderboardTitle = new Label
            {
                Text = $"Leaderboard:",
                Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(28),
            };

            mainGrid.Widgets.Add(leaderboardTitle);
            Grid.SetRow(leaderboardTitle, 1);
            Grid.SetColumn(leaderboardTitle, 0);
        }

        {
            var text = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                if (i != 0)
                {
                    text += "\n";
                }

                var score = tetrisSystem.HighScores.Count > i ? tetrisSystem.HighScores[i] : 0;

                text += $"{i + 1}. {score}";
            }

            var leaderboardEntries = new Label
            {
                Text = text,
                Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(20),
            };

            mainGrid.Widgets.Add(leaderboardEntries);
            Grid.SetRow(leaderboardEntries, 2);
            Grid.SetColumn(leaderboardEntries, 0);
        }

        desktop.Root = mainGrid;
    }

    public void Draw()
    {
        desktop.Render();
    }

    private class AssetManager : IAssetManager
    {
        private readonly ResourceManager resourceManager;

        public AssetManager(ResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
        }

        public T GetAsset<T>(string assetKey)
        {
            return resourceManager.GetResource<T>(assetKey).Value;
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
