#include "raylib.h"
#include <flecs.h>
#include <string>
namespace fl = flecs;
using namespace std;

struct Position {
  int x;
  int y;
};
struct Display {
  string character;
  Color color;
};
struct Player {};

fl::world world;

Texture2D tileset_texture;
void setup() {
  world = fl::world();
  fl::entity player = world.entity();
  player.set<Position>({10, 10});
  player.set<Display>({"@", WHITE});
  player.add<Player>();
  Image tileset_image = LoadImage("assets/urizen_tileset.png");
  for (int x = 0; x < tileset_image.width; x++) {
    for (int y = 0; y < tileset_image.height; y++) {
      Color pixel = GetImageColor(tileset_image, x, y);
      if (pixel.a == 0 or (pixel.r == 0 and pixel.g == 0 and pixel.b == 0)) {
        continue;
      }
      ImageDrawPixel(&tileset_image, x, y, WHITE);
    }
  }
  tileset_texture = LoadTextureFromImage(tileset_image);
}

void process() {
  BeginDrawing();
  DrawTexture(tileset_texture, 0, 0, RED);
  EndDrawing();
}

int main(int argc, char **argv) {
  InitWindow(800, 450, "YALDABARANTH");
  setup();
  auto sys = world.system<Position, Display>("display").each(
      [](Position &p, const Display &d) {
        DrawText(d.character.c_str(), p.x, p.y, 48, d.color);
      });
  while (!WindowShouldClose()) {
    process();
  }
  CloseWindow();
  return 0;
}
