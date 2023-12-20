// using Exanite.Ecs.Systems;
// using Exanite.ResourceManagement;
// using FontStashSharp;
// using Microsoft.Xna.Framework;
// using Myra;
// using Myra.Graphics2D;
// using Myra.Graphics2D.UI;
//
// namespace Exanite.GravitationalTetris.Features.Tetris.Systems;
//
// public class TetrisUiSystem : EcsSystem, IStartSystem, IUpdateSystem, IRenderSystem
// {
//     private Desktop desktop = null!;
//
//     private readonly Game game;
//     private readonly ResourceManager resourceManager;
//     private readonly TetrisSystem tetrisSystem;
//
//     public TetrisUiSystem(Game game, ResourceManager resourceManager, TetrisSystem tetrisSystem)
//     {
//         this.game = game;
//         this.resourceManager = resourceManager;
//         this.tetrisSystem = tetrisSystem;
//     }
//
//     public void Start()
//     {
//         var assetManager = new AssetManager(resourceManager);
//
//         MyraEnvironment.Game = game;
//         MyraEnvironment.DefaultAssetManager = assetManager;
//         DefaultAssets.AssetManager = assetManager;
//
//         desktop = new Desktop();
//     }
//
//     public void Update()
//     {
//         var mainGrid = new Grid
//         {
//             RowSpacing = 8,
//             ColumnSpacing = 8,
//             Padding = new Thickness(16),
//         };
//
//         mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // 0
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // 1
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // 2
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Fill)); // 3
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // 4
//         mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // 5
//
//         // Todo Hack for now, need to research how to get screen scaling / DPI
//         var fontScaling = 1f;
//         if (game.Window.ClientBounds.Width > 1920)
//         {
//             fontScaling = 1.5f;
//         }
//
//         if (game.Window.ClientBounds.Width > 2560)
//         {
//             fontScaling = 2f;
//         }
//
//         {
//             var score = new Label
//             {
//                 Text = $"Score: {(int)tetrisSystem.Score}",
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(32 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(score);
//             Grid.SetRow(score, 0);
//             Grid.SetColumn(score, 0);
//         }
//
//         {
//             var previousScore = new Label
//             {
//                 Text = $"Previous score: {(int)tetrisSystem.PreviousScore}\n",
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(20 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(previousScore);
//             Grid.SetRow(previousScore, 1);
//             Grid.SetColumn(previousScore, 0);
//         }
//
//         {
//             var leaderboardTitle = new Label
//             {
//                 Text = "Leaderboard:",
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(28 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(leaderboardTitle);
//             Grid.SetRow(leaderboardTitle, 2);
//             Grid.SetColumn(leaderboardTitle, 0);
//         }
//
//         {
//             var text = string.Empty;
//             for (var i = 0; i < 10; i++)
//             {
//                 if (i != 0)
//                 {
//                     text += "\n";
//                 }
//
//                 var score = tetrisSystem.HighScores.Count > i ? tetrisSystem.HighScores[i] : 0;
//
//                 text += $"{i + 1}. {(int)score}";
//             }
//
//             var leaderboardEntries = new Label
//             {
//                 Text = text,
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(20 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(leaderboardEntries);
//             Grid.SetRow(leaderboardEntries, 3);
//             Grid.SetColumn(leaderboardEntries, 0);
//         }
//
//         {
//             var speed = new Label
//             {
//                 Text = $"Speed: {tetrisSystem.SpeedMultiplier:F2}x",
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(24 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(speed);
//             Grid.SetRow(speed, 4);
//             Grid.SetColumn(speed, 0);
//         }
//
//         {
//             var speed = new Label
//             {
//                 Text = $"Score multiplier: {tetrisSystem.ScoreMultiplier:F1}x",
//                 Font = resourceManager.GetResource<FontSystem>("Base:FieryTurk.ttf").Value.GetFont(28 * fontScaling),
//             };
//
//             mainGrid.Widgets.Add(speed);
//             Grid.SetRow(speed, 5);
//             Grid.SetColumn(speed, 0);
//         }
//
//         desktop.Root = mainGrid;
//     }
//
//     public void Render()
//     {
//         desktop.Render();
//     }
//
//     private class AssetManager : IAssetManager
//     {
//         private readonly ResourceManager resourceManager;
//
//         public AssetManager(ResourceManager resourceManager)
//         {
//             this.resourceManager = resourceManager;
//         }
//
//         public T GetAsset<T>(string assetKey)
//         {
//             return resourceManager.GetResource<T>(assetKey).Value;
//         }
//
//         public void Dispose()
//         {
//             // Do nothing
//         }
//     }
// }
