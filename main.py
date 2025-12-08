import asyncio
import pygame as pg


async def main():
    running = True
    screen = pg.display.set_mode((1280, 720))
    pg.init()
    while running:
        for event in pg.event.get():
            if event.type == pg.QUIT:
                running = False
        screen.fill("red")
        pg.display.flip()
        await asyncio.sleep(1.0 / 60.0)


asyncio.run(main())

