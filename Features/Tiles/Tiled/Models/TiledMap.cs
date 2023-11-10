using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exanite.Extraction.Features.Tiles.Tiled.Models;

public class TiledMap
{
    /// <summary>
    /// Number of tile rows.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Number of tile columns.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }

    /// <summary>
    /// Map grid height in pixels.
    /// </summary>
    [JsonProperty("tileheight")]
    public int TileHeight { get; set; }

    /// <summary>
    /// Map grid width in pixels.
    /// </summary>
    [JsonProperty("tilewidth")]
    public int TileWidth { get; set; }

    /// <summary>
    /// The tilesets used by the map.
    /// </summary>
    [JsonProperty("tilesets")]
    public List<TiledMapTileset> Tilesets { get; set; } = new();

    /// <summary>
    /// The layers contained in the map.
    /// </summary>
    [JsonProperty("layers")]
    public List<TiledMapLayer> Layers { get; set; } = new();
}

public class TiledMapLayer
{
    /// <summary>
    /// Incremental ID - unique across all layers.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Name assigned to this layer.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Value between 0 and 1.
    /// </summary>
    [JsonProperty("opacity")]
    public float Opacity { get; set; }

    /// <summary>
    /// X coordinate where layer content starts (for infinite maps).
    /// </summary>
    [JsonProperty("startx")]
    public int StartX { get; set; }

    /// <summary>
    /// Y coordinate where layer content starts (for infinite maps).
    /// </summary>
    [JsonProperty("starty")]
    public int StartY { get; set; }

    /// <summary>
    /// Row count. Same as map height for fixed-size maps.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Column count. Same as map width for fixed-size maps.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }

    /// <summary>
    /// The chunks contained in the layer.
    /// </summary>
    [JsonProperty("chunks")]
    public List<TiledChunk> Chunks { get; set; } = new();
}

public class TiledChunk
{
    /// <summary>
    /// List of GIDs representing the tiles in the chunk.
    /// </summary>
    [JsonProperty("data")]
    public List<int> Data { get; set; } = new();

    /// <summary>
    /// Height in tiles.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Width in tiles.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }

    /// <summary>
    /// X coordinate in tiles.
    /// </summary>
    [JsonProperty("x")]
    public int X { get; set; }

    /// <summary>
    /// Y coordinate in tiles.
    /// </summary>
    [JsonProperty("y")]
    public int Y { get; set; }
}

public class TiledMapTileset
{
    /// <summary>
    /// GID corresponding to the first tile in the set.
    /// </summary>
    [JsonProperty("firstgid")]
    public int FirstGid { get; set; }

    /// <summary>
    /// Path to the external file containing the this tileset's data.
    /// </summary>
    [JsonProperty("source")]
    public string Source { get; set; } = string.Empty;
}
