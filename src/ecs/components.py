from dataclasses import dataclass


@dataclass
class Camera:
    pass


@dataclass
class Player:
    pass


@dataclass
class Position:
    x: float
    y: float


@dataclass
class Velocity:
    x: float
    y: float


@dataclass
class Display:
    color: str | tuple
