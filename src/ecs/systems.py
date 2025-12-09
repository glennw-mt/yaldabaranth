import esper
from esper import Processor
import ecs.components as C
import pygame as pg
import options as O
import sys


class Input(Processor):
    def process(self, screen):
        for _, (_, velocity) in esper.get_components(C.Player, C.Velocity):
            for event in pg.event.get():
                print(event)
                if event.type == pg.QUIT:
                    pg.quit()
                    sys.exit()
                elif event.type == pg.KEYDOWN:
                    match event.key:
                        case pg.K_LEFT:
                            velocity.x -= O.GRID_SIZE
                        case pg.K_RIGHT:
                            velocity.x += O.GRID_SIZE
                        case pg.K_UP:
                            velocity.y -= O.GRID_SIZE
                        case pg.K_DOWN:
                            velocity.y += O.GRID_SIZE


class Movement(Processor):
    def process(self, screen):
        for _, (position, velocity) in esper.get_components(C.Position, C.Velocity):
            position.x += velocity.x
            position.y += velocity.y
            velocity.x = 0
            velocity.y = 0


class Display(Processor):
    def process(self, screen):
        screen.fill("black")
        for _, (position, display) in esper.get_components(C.Position, C.Display):
            pg.draw.rect(
                screen,
                display.color,
                (position.x, position.y, O.GRID_SIZE, O.GRID_SIZE),
            )
        pg.display.flip()
