using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exanite.Extraction.Features.Tiles.Tiled.Models;

public class TiledTileset
{
    /// <summary>
    /// Name given to this tileset.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Common grid settings used by tiles.
    /// </summary>
    [JsonProperty("grid")]
    public TiledGrid Grid { get; set; } = new();

    /// <summary>
    /// Buffer between image edge and first tile in pixels.
    /// </summary>
    [JsonProperty("margin")]
    public int Margin { get; set; }

    /// <summary>
    /// Spacing between adjacent tiles in image in pixels.
    /// </summary>
    [JsonProperty("spacing")]
    public int Spacing { get; set; }

    /// <summary>
    /// The tiles contained in the tileset.
    /// </summary>
    [JsonProperty("tiles")]
    public List<TiledTile> Tiles { get; set; } = new();
}

public class TiledGrid
{
    /// <summary>
    /// Cell height of tile grid.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Cell width of tile grid.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }
}

public class TiledTile
{
    /// <summary>
    /// Local ID of the tile.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Path to the external image representing this tile.
    /// </summary>
    [JsonProperty("image")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// The X position of the sub-rectangle representing this tile.
    /// </summary>
    [JsonProperty("x")]
    public int X { get; set; } = 0;

    /// <summary>
    /// The Y position of the sub-rectangle representing this tile
    /// </summary>
    [JsonProperty("y")]
    public int Y { get; set; } = 0;

    /// <summary>
    /// The width of the sub-rectangle representing this tile.
    /// </summary>
    [JsonProperty("imageheight")]
    public int ImageHeight { get; set; }

    /// <summary>
    /// The height of the sub-rectangle representing this tile.
    /// </summary>
    [JsonProperty("imagewidth")]
    public int ImageWidth { get; set; }
}
