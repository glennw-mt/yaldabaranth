use crate::ecs::components as C;
use crate::tileset::Tile;
use hecs::*;
use macroquad::prelude::*;

pub fn player(ecs: &mut World, pos: &Vec2) {
    ecs.spawn((
        C::Position::new(pos.x, pos.y),
        C::Display {
            tile: Tile::MAN,
            color: WHITE,
        },
        C::Player,
    ));
}

pub fn tree(ecs: &mut World, pos: &Vec2) {
    ecs.spawn((
        C::Position::new(pos.x, pos.y),
        C::Display {
            tile: Tile::TREE,
            color: GREEN,
        },
    ));
}
