use crate::ecs::components as C;
use crate::options::*;
use hecs::*;
use macroquad::miniquad::window::screen_size;
use macroquad::prelude::*;
use rayon::prelude::*;

pub fn draw(ecs: &mut World, camera: &Camera2D, tileset: &Texture2D) {
    set_camera(camera);
    clear_background(BLACK);
    let mut rendered_entities = Vec::new();
    for (entity, (pos, disp, _)) in ecs
        .query::<(&C::Position, &C::Display, &C::InView)>()
        .iter()
    {
        let uv = disp.tile.uv().clone();
        draw_texture_ex(
            tileset,
            pos.x * GRID_SIZE,
            pos.y * GRID_SIZE,
            disp.color,
            DrawTextureParams {
                dest_size: Some(Vec2::new(GRID_SIZE, GRID_SIZE)),
                source: Some(Rect {
                    x: uv.x * 12. + uv.x + 1.,
                    y: uv.y * 12. + uv.y + 1.,
                    w: 12.,
                    h: 12.,
                }),
                ..Default::default()
            },
        );
        rendered_entities.push(entity);
    }
    rendered_entities.iter_mut().for_each(|entity| {
        ecs.remove_one::<C::InView>(*entity).unwrap();
    });
}

pub fn update_camera(ecs: &World, camera: &mut Camera2D) {
    let mut query = ecs.query::<(&C::Position, &C::Player)>();
    let player_position = query.iter().next().unwrap().1.0;
    let old_target = camera.target.clone();
    *camera = Camera2D::from_display_rect(Rect::new(0., 0., screen_width(), screen_height()));
    camera.target = old_target;
    camera.target.x +=
        ((player_position.x * GRID_SIZE) - camera.target.x) * get_frame_time() * CAMERA_SPEED;
    camera.target.y +=
        ((player_position.y * GRID_SIZE) - camera.target.y) * get_frame_time() * CAMERA_SPEED;
    camera.offset.x = 0.0;
    camera.offset.y = 0.0;
    camera.rotation = 180.0;
    camera.zoom.x = -camera.zoom.x;
}

pub fn control(ecs: &mut World) {
    for (_, (pos, _)) in ecs.query_mut::<(&mut C::Position, &C::Player)>() {
        if is_key_pressed(KeyCode::Up) {
            pos.y -= 1.;
        }
        if is_key_pressed(KeyCode::Down) {
            pos.y += 1.;
        }
        if is_key_pressed(KeyCode::Left) {
            pos.x -= 1.;
        }
        if is_key_pressed(KeyCode::Right) {
            pos.x += 1.;
        }
    }
}

pub fn see_entities(ecs: &mut World, camera: &Camera2D) {
    let camera_pos = camera.target;
    let screen_size = screen_size();
    let entities_view: Vec<Entity>;
    {
        let mut query = ecs.query::<(&C::Position, &C::Display)>();
        let entities: Vec<(Entity, (&C::Position, &C::Display))> = query.iter().collect();
        entities_view = entities
            .par_iter()
            .filter_map(|(entity, (pos, _))| {
                let real_pos = Vec2::new(pos.x * GRID_SIZE, pos.y * GRID_SIZE);
                if real_pos.distance(camera_pos) < screen_size.0 {
                    return Some(entity.clone());
                }
                return None;
            })
            .collect();
    }
    for entity in entities_view {
        ecs.insert_one(entity, C::InView {}).unwrap();
    }
}
