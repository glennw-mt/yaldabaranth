import esper
import ecs.components as C


def player(x: int, y: int):
    return esper.create_entity(
        C.Position(x, y), C.Display("red"), C.Velocity(0, 0), C.Player()
    )
