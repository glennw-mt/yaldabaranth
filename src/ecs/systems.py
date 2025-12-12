import esper
from esper import Processor
import ecs.components as C
import pygame as pg
import options as O
import sys


class Input(Processor):
    def process(self, screen, delta: float):
        for _, (_, velocity) in esper.get_components(C.Player, C.Velocity):
            for event in pg.event.get():
                print(event)
                if event.type == pg.QUIT:
                    pg.quit()
                    sys.exit()
                elif event.type == pg.KEYDOWN:
                    match event.key:
                        case pg.K_LEFT:
                            velocity.x -= 1
                        case pg.K_RIGHT:
                            velocity.x += 1
                        case pg.K_UP:
                            velocity.y -= 1
                        case pg.K_DOWN:
                            velocity.y += 1


class Movement(Processor):
    def process(self, screen, delta: float):
        for _, (position, velocity) in esper.get_components(C.Position, C.Velocity):
            position.x += velocity.x
            position.y += velocity.y
            velocity.x = 0
            velocity.y = 0


class Camera(Processor):
    def process(self, screen, delta: float):
        _, (_, player_position) = esper.get_components(C.Player, C.Position)[0]
        _, (_, camera_position) = esper.get_components(C.Camera, C.Position)[0]
        camera_position.x += (player_position.x - camera_position.x) * delta * 2.0
        camera_position.y += (player_position.y - camera_position.y) * delta * 2.0


class Display(Processor):
    def process(self, screen: pg.Surface, delta: float):
        screen.fill("black")
        _, (_, camera_position) = esper.get_components(C.Camera, C.Position)[0]
        for _, (position, display) in esper.get_components(C.Position, C.Display):
            pg.draw.rect(
                screen,
                display.color,
                (
                    (position.x - camera_position.x) * O.GRID_SIZE
                    + screen.width // 2
                    - O.GRID_SIZE / 2,
                    (position.y - camera_position.y) * O.GRID_SIZE
                    + screen.height // 2
                    - O.GRID_SIZE / 2,
                    O.GRID_SIZE,
                    O.GRID_SIZE,
                ),
            )
        pg.display.flip()
