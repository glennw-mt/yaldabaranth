using System.Collections.Generic;
using Godot;

public enum Tile
{
  Man, Tree
}

public class Tileset
{
  readonly Image image;
  public readonly int tile_size = 12;
  public readonly int scale = 4;
  readonly Dictionary<Tile, ImageTexture> cache = [];
  public Tileset(string path)
  {
    image = GD.Load<CompressedTexture2D>(path).GetImage();
    float tileset_width = image.GetWidth();
    float tileset_height = image.GetHeight();
    for (int x = 0; x < tileset_width; x++)
    {
      for (int y = 0; y < tileset_height; y++)
      {
        Color pixel = image.GetPixel(x, y);
        if (!(pixel.A == 0 || pixel == Color.Color8(0, 0, 0)))
        {
          image.SetPixel(x, y, Color.Color8(255, 255, 255));
        }
        if (pixel == Color.Color8(0, 0, 0))
        {
          image.SetPixel(x, y, Color.Color8(0, 0, 0, 0));
        }
      }
    }
  }
  public ImageTexture GetTile(Tile tile)
  {
    if (image == null) GD.PushError("Tileset image not found.");
    int x;
    int y;
    switch (tile)
    {
      case Tile.Man:
        x = 104; y = 0;
        break;
      case Tile.Tree:
        x = 1; y = 8;
        break;
      default:
        x = 0; y = 0;
        break;
    }
    ImageTexture tile_texture;
    if (cache.ContainsKey(tile))
    {
      tile_texture = cache[tile];
    }
    else
    {
      tile_texture = ImageTexture.CreateFromImage(image.GetRegion(new Rect2I(
        position: new Vector2I(x * tile_size + x + 1, y * tile_size + y + 1),
        size: new Vector2I(tile_size, tile_size)
      )));
      cache[tile] = tile_texture;
    }
    tile_texture.SetSizeOverride(new Vector2I(tile_size * scale, tile_size * scale));
    return tile_texture;
  }
}
