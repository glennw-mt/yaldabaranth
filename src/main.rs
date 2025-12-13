pub mod ecs;
pub mod options;
pub mod tileset;

use ecs::{entities as E, systems as S};
use hecs::*;
use macroquad::{prelude::*, rand};

fn test_spawn(ecs: &mut World) {
    E::player(ecs, &Vec2::new(0., 0.));
    for x in -1000..1000 {
        for y in -1000..1000 {
            if rand::gen_range(0, 100) > 75 {
                E::tree(ecs, &Vec2::new(x as f32, y as f32));
            }
        }
    }
}

#[macroquad::main("MyGame")]
async fn main() {
    let mut ecs = World::new();
    let mut tileset_img = load_image("assets/urizen_tileset.png").await.unwrap();
    let tileset_data = tileset_img.get_image_data_mut();
    for pixel in tileset_data.iter_mut() {
        let is_visible = pixel[3] != 0;
        let is_black = pixel[0] == 0 && pixel[1] == 0 && pixel[2] == 0;
        if is_visible && !is_black {
            pixel[0] = 255;
            pixel[1] = 255;
            pixel[2] = 255;
        }
    }

    let tileset = Texture2D::from_image(&tileset_img);
    tileset.set_filter(FilterMode::Nearest);
    test_spawn(&mut ecs);
    set_fullscreen(true);
    let mut camera =
        Camera2D::from_display_rect(Rect::new(0., 0., screen_width(), screen_height()));
    camera.target.x = 0.;
    camera.target.y = 0.;
    loop {
        S::control(&mut ecs);
        S::update_camera(&ecs, &mut camera);
        S::see_entities(&mut ecs, &camera);
        S::draw(&mut ecs, &camera, &tileset);
        next_frame().await
    }
}
