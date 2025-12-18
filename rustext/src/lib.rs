mod ecs;
mod tileset;

use ecs::systems::*;
use godot::{classes::Camera2D, global::randf_range, prelude::*};
use hecs::*;
use sled::Db;
use tileset::Tileset;

use crate::ecs::entities::{spawn_player, spawn_tree};

struct RustExt;
#[gdextension]
unsafe impl ExtensionLibrary for RustExt {}

#[derive(GodotClass)]
#[class(base=Node2D)]
pub struct Game {
    base: Base<Node2D>,
    tileset: Tileset,
    camera: Gd<Camera2D>,
    ecs: World,
    db: Db,
}
#[godot_api]
impl INode2D for Game {
    fn init(base: Base<Node2D>) -> Self {
        let tileset = Tileset::new("res://assets/urizen_tileset.png", 12, 48.);
        let camera = Camera2D::new_alloc();
        let ecs = World::new();
        let db = sled::open("ecs.db").expect("Could not open ecs database.");
        Self {
            base,
            tileset,
            camera,
            ecs,
            db,
        }
    }
    fn ready(&mut self) {
        self.to_gd().add_child(&self.camera);
        spawn_player(self, 0, 0);
        for x in -500..500 {
            for y in -500..500 {
                if randf_range(0., 100.) > 90. {
                    spawn_tree(self, x, y);
                }
            }
        }
    }
    fn process(&mut self, delta: f64) {
        handle_input(self);
        move_entities(self);
        move_camera(self, delta);
        sync_sprite_positions(self);
        despawn_far_entities(self);
    }
    fn draw(&mut self) {}
}
