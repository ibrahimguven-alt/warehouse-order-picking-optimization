from dataclasses import dataclass


@dataclass
class WarehouseConfig:
    blocks: int = 2
    aisles: int = 5
    locations: int = 5
    shelves: int = 1
    cell_width: float = 18.0
    cell_depth: float = 18.0
    corridor_width: float = 30.0
    cross_aisle_width: float = 30.0
    start_x: float = 60.0
    start_z: float = 50.0
    rack_height: float = 2.5
    wall_height: float = 3.0
    world_scale: float = 0.08
