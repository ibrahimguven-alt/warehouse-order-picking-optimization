from dataclasses import dataclass


@dataclass
class OrderItem:
    rack_index: int
    shelf: int = 1


@dataclass
class Order:
    name: str
    items: list[OrderItem]

