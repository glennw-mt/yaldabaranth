use macroquad::prelude::*;

use crate::tileset::Tile;

pub type Position = Vec2;
pub struct Display {
    pub tile: Tile,
    pub color: Color,
}
pub struct Player;

pub struct InView;
