from random import randrange
import asyncio
import esper
import pygame as pg
import pygame.camera as pgc
import options as O
import ecs.entities as E
import ecs.systems as S


async def main():
    pg.init()
    pgc.init()
    pg.display.set_caption("YALDABARANTH")
    clock = pg.time.Clock()
    screen = pg.display.set_mode((O.SCREEN_WIDTH, O.SCREEN_HEIGHT))
    E.camera(0, 0)
    E.player(0, 0)
    for x in range(100):
        for y in range(100):
            if randrange(0, 100, 1) > 90:
                E.tree(x, y)
    esper.add_processor(S.Camera())
    esper.add_processor(S.Input())
    esper.add_processor(S.Movement())
    esper.add_processor(S.Display())
    clock.tick(60)
    while True:
        delta: float = clock.get_time() / 1000.0
        esper.process(screen, delta)
        clock.tick(60)


asyncio.run(main())
