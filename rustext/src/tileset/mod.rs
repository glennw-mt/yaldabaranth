use godot::{
    classes::{CompressedTexture2D, Image, ImageTexture, Sprite2D},
    prelude::*,
};

#[derive(Clone, Copy, Debug)]
pub enum Tile {
    Man = 2000,
    Tree = 1,
}

pub struct Tileset {
    image: Gd<Image>,
    pub tile_size: i32,
    pub scale: f32,
}
impl Tileset {
    pub fn new(path: &str, tile_size: i32, scale: f32) -> Self {
        let tileset_compressed_texture: Gd<CompressedTexture2D> = load(path);
        let mut tileset_image = tileset_compressed_texture.get_image().unwrap();
        let width = tileset_image.get_width();
        let height = tileset_image.get_height();
        for x in 0..width {
            for y in 0..height {
                let pixel = tileset_image.get_pixel(x, y);
                if pixel == Color::BLACK {
                    tileset_image.set_pixel(x, y, Color::TRANSPARENT_BLACK);
                }
                if pixel == Color::WHITE || pixel == Color::BLACK || pixel.a == 0.0 {
                    continue;
                };
                tileset_image.set_pixel(x, y, Color::WHITE);
            }
        }
        Self {
            image: tileset_image,
            tile_size: tile_size,
            scale,
        }
    }
    pub fn get_xy(&self, x: i32, y: i32, color: Color) -> Gd<Image> {
        let mut base_image = self
            .image
            .get_region(Rect2i {
                position: Vector2i::new(
                    x * self.tile_size + (x + 1) * 1,
                    y * self.tile_size + (y + 1) * 1,
                ),
                size: Vector2i::new(self.tile_size, self.tile_size),
            })
            .expect("Could not get tileset xy region");
        for x in 0..base_image.get_width() {
            for y in 0..base_image.get_height() {
                let pixel = base_image.get_pixel(x, y);
                if pixel == Color::WHITE {
                    base_image.set_pixel(x, y, color);
                }
            }
        }
        base_image
    }
    pub fn get_tile(&self, tile: Tile, color: Color) -> Gd<Image> {
        match tile {
            Tile::Man => self.get_xy(104, 0, color),
            Tile::Tree => self.get_xy(1, 8, color),
        }
    }
    pub fn get_tile_sprite(&self, tile: Tile, color: Color) -> Gd<Sprite2D> {
        let scale = self.scale / self.tile_size as f32;
        let tile_image = self.get_tile(tile, color);
        let tile_texture = ImageTexture::create_from_image(&tile_image).unwrap();
        let mut tile_sprite = Sprite2D::new_alloc();
        tile_sprite.set_texture(&tile_texture);
        tile_sprite.set_scale(Vector2::new(scale, scale));
        tile_sprite
    }
}
