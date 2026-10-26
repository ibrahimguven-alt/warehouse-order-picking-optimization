from dataclasses import dataclass, field


@dataclass(frozen=True)
class RoutePoint:
    x: float
    z: float


@dataclass
class RackCell:
    index: int
    x: float
    z: float
    width: float
    depth: float
    selected: bool = False
    picked: bool = False
    selected_shelves: set[int] = field(default_factory=set)
    picked_shelves: set[int] = field(default_factory=set)

    @property
    def center(self) -> RoutePoint:
        return RoutePoint(self.x + self.width / 2, self.z + self.depth / 2)


@dataclass
class CrossoverPoint:
    x: float
    z: float
    visited: bool = False


@dataclass
class WarehouseLayout:
    rack_cells: list[RackCell]
    crossover_points: list[CrossoverPoint]
    aisle_xs: list[float]
    cross_zs: list[float]
    width: float
    depth: float
    depot: RoutePoint
    min_x: float = 0
    min_z: float = 0
    max_x: float = 0
    max_z: float = 0
