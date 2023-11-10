using Microsoft.Xna.Framework;

namespace Exanite.WarGames.Features.Cameras.Components;

public struct CameraProjectionComponent
{
    // Typically 3 matrices are used for rendering: World, View, and Projection.
    // Two are provided here.
    //
    // Note: Cameras cannot define the World matrix because
    // the World matrix is specific to the mesh being rendered.
    //
    // The World matrix is a MeshToWorld matrix that tells the mesh where it is in world space.

    /// <summary>
    /// Also called a View matrix.
    /// </summary>
    public Matrix WorldToLocal;
    public Matrix LocalToWorld => Matrix.Invert(WorldToLocal);

    /// <summary>
    /// Also called a Projection matrix.
    /// </summary>
    public Matrix LocalToScreen;
    public Matrix ScreenToLocal => Matrix.Invert(LocalToScreen);

    public Matrix WorldToScreen => WorldToLocal * LocalToScreen;
    public Matrix ScreenToWorld => Matrix.Invert(WorldToScreen);

    public float MetersToPixels;
    public float Rotation;

    public CameraProjectionComponent()
    {
        WorldToLocal = Matrix.Identity;
        LocalToScreen = Matrix.Identity;
        MetersToPixels = 1;
    }
}
