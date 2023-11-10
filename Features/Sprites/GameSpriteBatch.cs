using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Sprites;

public class GameSpriteBatch
{
    public SpriteBatch SpriteBatch = null!;

    private readonly GraphicsDeviceManager graphicsDeviceManager;

    public GameSpriteBatch(GraphicsDeviceManager graphicsDeviceManager)
    {
        this.graphicsDeviceManager = graphicsDeviceManager;

        graphicsDeviceManager.DeviceCreated += (_, _) =>
        {
            Initialize();
        };
    }

    private void Initialize()
    {
        SpriteBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
    }
}
