from dataclasses import dataclass


@dataclass
class Player:
    pass


@dataclass
class Position:
    x: int
    y: int


@dataclass
class Velocity:
    x: int
    y: int


@dataclass
class Display:
    color: str | tuple
