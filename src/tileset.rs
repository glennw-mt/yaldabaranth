use macroquad::math::Vec2;

#[derive(Clone, Copy, Debug)]
pub enum Tile {
    TREE,
    MAN,
}
impl Tile {
    pub const fn uv(self) -> Vec2 {
        match self {
            Tile::TREE => Vec2::new(3., 8.),
            Tile::MAN => Vec2::new(104., 0.),
        }
    }
}
