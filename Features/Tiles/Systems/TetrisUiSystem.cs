using Exanite.ResourceManagement;
using Exanite.WarGames.Systems;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Exanite.WarGames.Features.Tiles.Systems;

public class TetrisUiSystem : EcsSystem, IStartSystem, IUpdateSystem, IDrawSystem
{
    private Desktop desktop = null!;

    private readonly Game game;
    private readonly ResourceManager resourceManager;

    public TetrisUiSystem(Game game, ResourceManager resourceManager)
    {
        this.game = game;
        this.resourceManager = resourceManager;
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
                Text = $"Score: {12345}\n",
            };

            mainGrid.Widgets.Add(score);
            Grid.SetRow(score, 0);
            Grid.SetColumn(score, 0);
        }

        {
            var leaderboardTitle = new Label
            {
                Text = $"Leaderboard:",
            };

            mainGrid.Widgets.Add(leaderboardTitle);
            Grid.SetRow(leaderboardTitle, 1);
            Grid.SetColumn(leaderboardTitle, 0);
        }

        {
            var leaderboardEntries = new Label
            {
                Text = $"1. 34534534\n1. 34534534\n1. 34534534\n1. 34534534",
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
