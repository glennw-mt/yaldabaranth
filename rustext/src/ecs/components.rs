use godot::prelude::*;

#[derive(Clone, Debug)]
pub struct Display {
    pub sprite_id: InstanceId,
    pub base_color: Color,
}
pub struct Player;
pub struct Position(pub Vector2i);
pub struct Velocity(pub Vector2i);
pub struct SyncSpritePosition;
pub struct Despawn;
