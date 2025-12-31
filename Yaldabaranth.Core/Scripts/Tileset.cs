using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yaldabaranth.Core;

public enum Tile
{
  Man, Tree
}

public class Tileset
{
  readonly YaldabaranthGame game;
  readonly Texture2D texture;
  public readonly int TileSize;
  readonly int margin;
  readonly int spacing;
  public int Scale = 4;
  readonly Dictionary<Tile, Rectangle> tileSourceRectangles = [];
  public Tileset(YaldabaranthGame game, string path, int size, int margin, int spacing)
  {
    this.game = game;
    TileSize = size;
    this.margin = margin;
    this.spacing = spacing;
    FileStream stream = new(path, FileMode.Open);
    texture = Texture2D.FromStream(game.GraphicsDevice, stream);
    RegisterTile(Tile.Man, 104, 0);
    RegisterTile(Tile.Tree, 1, 8);
  }
  public void DrawTile(Tile tile, int x, int y, Color color)
  {
    var renderSize = TileSize * Scale;
    game.Canvas.Draw(
        texture: texture,
        sourceRectangle: tileSourceRectangles[tile],
        destinationRectangle: new Rectangle(x * renderSize, y * renderSize, renderSize, renderSize),
        color: color);
  }
  private void RegisterTile(Tile tile, int x, int y)
  {
    tileSourceRectangles[tile] = new Rectangle(
      location: new Point(
        x: margin + (TileSize + spacing) * x,
        y: margin + (TileSize + spacing) * y
      ),
      size: new Point(
        x: TileSize,
        y: TileSize
      )
    );
  }

}
