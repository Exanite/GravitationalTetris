using Exanite.ResourceManagement;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Myra;
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

        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8,
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var helloWorld = new Label
        {
            Id = "label",
            Text = "Hello, World!",
        };
        grid.Widgets.Add(helloWorld);

        // ComboBox
        var combo = new ComboBox();
        Grid.SetColumn(combo, 1);
        Grid.SetRow(combo, 0);

        combo.Items.Add(new ListItem("Red", Color.Red));
        combo.Items.Add(new ListItem("Green", Color.Green));
        combo.Items.Add(new ListItem("Blue", Color.Blue));
        grid.Widgets.Add(combo);

        // Button
        var button = new Button
        {
            Content = new Label
            {
                Text = "Show",
            },
        };
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, 1);

        button.Click += (s, a) =>
        {
            var messageBox = Dialog.CreateMessageBox("Message", "Some message!");
            messageBox.ShowModal(desktop);
        };

        grid.Widgets.Add(button);

        // Spin button
        var spinButton = new SpinButton
        {
            Width = 100,
            Nullable = true,
        };
        Grid.SetColumn(spinButton, 1);
        Grid.SetRow(spinButton, 1);

        grid.Widgets.Add(spinButton);

        desktop.Root = grid;
    }

    public void Update() {}

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
