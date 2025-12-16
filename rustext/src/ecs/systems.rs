use crate::Game;
use crate::ecs::components as C;
use godot::classes::{Input, Sprite2D};
use godot::prelude::*;

pub fn sync_sprite_positions(game: &mut Game) {
    let mut synced_entities = Vec::new();
    for (entity, (disp, pos, _)) in game
        .ecs
        .query::<(&C::Display, &C::Position, &C::SyncSpritePosition)>()
        .iter()
    {
        let mut sprite: Gd<Sprite2D> = Gd::from_instance_id(disp.sprite_id);
        sprite.set_global_position(Vector2::new(
            pos.0.x as f32 * game.tileset.scale as f32,
            pos.0.y as f32 * game.tileset.scale as f32,
        ));
        synced_entities.push(entity);
    }
    for entity in synced_entities {
        game.ecs
            .remove::<(C::SyncSpritePosition,)>(entity)
            .expect("Could not remove SyncSprite from entity");
    }
}

pub fn handle_input(game: &mut Game) {
    let input = Input::singleton();
    for (_, (vel, _)) in game.ecs.query_mut::<(&mut C::Velocity, &C::Player)>() {
        if input.is_action_just_pressed("ui_left") {
            vel.0.x -= 1
        }
        if input.is_action_just_pressed("ui_right") {
            vel.0.x += 1
        }
        if input.is_action_just_pressed("ui_up") {
            vel.0.y -= 1
        }
        if input.is_action_just_pressed("ui_down") {
            vel.0.y += 1
        }
    }
}

pub fn move_entities(game: &mut Game) {
    let mut entities_to_sync = Vec::new();
    for (entity, (pos, vel)) in game.ecs.query_mut::<(&mut C::Position, &mut C::Velocity)>() {
        if vel.0 != Vector2i::ZERO {
            pos.0 += vel.0;
            vel.0 = Vector2i::ZERO;
            entities_to_sync.push(entity);
        }
    }
    for entity in entities_to_sync {
        game.ecs
            .insert(entity, (C::SyncSpritePosition,))
            .expect("Could not add SyncSprite to entity");
    }
}

pub fn move_camera(game: &mut Game, delta: f64) {
    let player_position_int = game
        .ecs
        .query::<(&C::Player, &C::Position)>()
        .iter()
        .next()
        .unwrap()
        .1
        .1
        .0;
    let player_position_global = Vector2::new(
        player_position_int.x as f32 * game.tileset.scale as f32,
        player_position_int.y as f32 * game.tileset.scale as f32,
    );
    let camera_position = game.camera.get_global_position();
    game.camera.translate(
        (player_position_global - camera_position) * Vector2::new(delta as f32, delta as f32),
    );
}

pub fn despawn_far_entities(game: &mut Game) {
    let mut entities_to_despawn = Vec::new();
    let camera_position = game.camera.get_global_position();
    for (entity, pos) in game.ecs.query::<&C::Position>().iter() {
        let global_pos = Vector2::new(
            pos.0.x as f32 * game.tileset.scale,
            pos.0.y as f32 * game.tileset.scale,
        );
        if global_pos.distance_to(camera_position) > 10000. {
            entities_to_despawn.push(entity);
        }
    }
    for entity in entities_to_despawn {
        game.ecs
            .insert(entity, (C::Despawn,))
            .expect("Could not add despawn component to entity");
    }
}
