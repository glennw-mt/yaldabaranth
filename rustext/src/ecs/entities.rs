use crate::Game;
use crate::ecs::components as C;
use crate::tileset::*;
use godot::prelude::*;

pub fn spawn_player(game: &mut Game, x: i32, y: i32) {
    let mut sprite = game.tileset.get_tile_sprite(Tile::Man, Color::RED);
    sprite.translate(Vector2::new(
        x as f32 * game.tileset.scale,
        y as f32 * game.tileset.scale,
    ));
    sprite.set_z_index(Tile::Man as i32);
    let sprite_id = sprite.instance_id();
    game.ecs.spawn((
        C::Display {
            sprite_id,
            base_color: Color::RED,
        },
        C::Player,
        C::Position(Vector2i::new(x, y)),
        C::Velocity(Vector2i::new(0, 0)),
        C::SyncSpritePosition,
    ));
    game.to_gd().add_child(&sprite)
}

pub fn spawn_tree(game: &mut Game, x: i32, y: i32) {
    let mut sprite = game.tileset.get_tile_sprite(Tile::Tree, Color::GREEN);
    sprite.translate(Vector2::new(
        x as f32 * game.tileset.scale,
        y as f32 * game.tileset.scale,
    ));
    sprite.set_z_index(Tile::Tree as i32);
    let sprite_id = sprite.instance_id();
    game.ecs.spawn((
        C::Display {
            sprite_id,
            base_color: Color::GREEN,
        },
        C::Position(Vector2i::new(x, y)),
        C::Velocity(Vector2i::new(0, 0)),
        C::SyncSpritePosition,
    ));
    game.to_gd().add_child(&sprite);
}
