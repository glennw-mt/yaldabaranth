use dashmap::DashMap;
use macroquad::audio::{PlaySoundParams, Sound, play_sound};

#[derive(PartialEq, Eq, Hash, Debug)]
pub enum SoundType {
    WALK,
    WIND,
}

pub struct AudioEngine {
    sounds: DashMap<SoundType, Sound>,
}
impl AudioEngine {
    pub async fn new() -> Self {
        let sounds = DashMap::new();
        sounds.insert(SoundType::WALK, load_sound("walk.wav".to_string()).await);
        sounds.insert(SoundType::WIND, load_sound("wind.wav".to_string()).await);

        Self { sounds }
    }
    pub fn play_sound(&self, sound_type: SoundType) {
        match sound_type {
            SoundType::WALK => {
                play_sound(
                    &self
                        .sounds
                        .get(&sound_type)
                        .expect("Sound not in AudioEngine instance!"),
                    PlaySoundParams {
                        looped: false,
                        volume: 0.1,
                    },
                );
            }
            SoundType::WIND => {
                play_sound(
                    &self
                        .sounds
                        .get(&sound_type)
                        .expect("Sound not in AudioEngine instance!"),
                    PlaySoundParams {
                        looped: true,
                        volume: 0.2,
                    },
                );
            }
        }
    }
}
pub async fn load_sound(asset_path: String) -> Sound {
    let sound_file = macroquad::audio::load_sound(&format!("assets/sounds/{}", asset_path))
        .await
        .unwrap();
    return sound_file;
}
