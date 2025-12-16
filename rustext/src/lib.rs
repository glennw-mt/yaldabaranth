use godot::{
    classes::{CompressedTexture2D, ImageTexture, Sprite2D, Texture2D},
    prelude::*,
};

struct RustExt;

#[gdextension]
unsafe impl ExtensionLibrary for RustExt {}

#[derive(GodotClass)]
#[class(base=Node2D)]
struct Game {
    base: Base<Node2D>,
}
#[godot_api]
impl INode2D for Game {
    fn init(base: Base<Node2D>) -> Self {
        Self { base }
    }
    fn ready(&mut self) {
        let tileset_compressed_texture: Gd<CompressedTexture2D> =
            load("res://assets/urizen_tileset.png");
        let mut tileset_image = tileset_compressed_texture.get_image().unwrap();
        let width = tileset_image.get_width();
        let height = tileset_image.get_height();
        for x in 0..width {
            for y in 0..height {
                let pixel = tileset_image.get_pixel(x, y);
                if pixel == Color::WHITE || pixel == Color::BLACK || pixel.a == 0.0 {
                    continue;
                };
                tileset_image.set_pixel(x, y, Color::WHITE);
            }
        }
        let tileset_texture = ImageTexture::create_from_image(&tileset_image);
        let mut tileset_sprite = Sprite2D::new_alloc();
        tileset_sprite.set_texture(&tileset_texture.unwrap());
        tileset_sprite.set_scale(Vector2::new(2.0, 2.0));
        self.to_gd().add_child(&tileset_sprite);
    }
    fn process(&mut self, delta: f64) {}
    fn physics_process(&mut self, delta: f64) {}
}
