import asyncio
import esper
import pygame as pg
import options as O
import ecs.entities as E
import ecs.systems as S


async def main():
    pg.init()
    pg.display.set_caption("YALDABARANTH")
    clock = pg.time.Clock()
    screen = pg.display.set_mode((O.SCREEN_WIDTH, O.SCREEN_HEIGHT))
    E.player(O.SCREEN_WIDTH // 2, O.SCREEN_HEIGHT // 2)
    esper.add_processor(S.Input())
    esper.add_processor(S.Movement())
    esper.add_processor(S.Display())
    while True:
        esper.process(screen)
        clock.tick(60)


asyncio.run(main())
