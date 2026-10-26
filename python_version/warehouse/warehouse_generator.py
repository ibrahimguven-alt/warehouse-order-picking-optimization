from config.warehouse_config import WarehouseConfig
from warehouse.warehouse_model import CrossoverPoint, RackCell, RoutePoint, WarehouseLayout


class WarehouseGenerator:
    def __init__(self, config: WarehouseConfig):
        self.config = config

    def build_layout(self) -> WarehouseLayout:
        cfg = self.config
        aisle_step = cfg.corridor_width + 2 * cfg.cell_width
        block_depth = cfg.locations * cfg.cell_depth + cfg.cross_aisle_width

        aisle_xs = [cfg.start_x + i * aisle_step for i in range(cfg.aisles)]
        cross_zs = [cfg.start_z + j * block_depth for j in range(cfg.blocks + 1)]

        crossovers = [
            CrossoverPoint(x=x, z=z)
            for x in aisle_xs
            for z in cross_zs
        ]

        rack_cells: list[RackCell] = []
        index = 0

        for block in range(cfg.blocks):
            block_start_z = cross_zs[block] + cfg.cross_aisle_width / 2

            for a in range(cfg.aisles + 1):
                if a == 0:
                    start_x = aisle_xs[0] - cfg.corridor_width / 2 - cfg.cell_width
                    columns = 1
                elif a == cfg.aisles:
                    start_x = aisle_xs[a - 1] + cfg.corridor_width / 2
                    columns = 1
                else:
                    start_x = aisle_xs[a - 1] + cfg.corridor_width / 2
                    columns = 2

                for col in range(columns):
                    x = start_x + col * cfg.cell_width
                    for loc in range(cfg.locations):
                        z = block_start_z + loc * cfg.cell_depth
                        rack_cells.append(
                            RackCell(
                                index=index,
                                x=x,
                                z=z,
                                width=cfg.cell_width,
                                depth=cfg.cell_depth,
                            )
                        )
                        index += 1

        min_x = aisle_xs[0] - cfg.corridor_width / 2 - cfg.cell_width - 20
        max_x = aisle_xs[-1] + cfg.corridor_width / 2 + cfg.cell_width + 20
        min_z = cross_zs[0] - cfg.cross_aisle_width / 2 - 20
        max_z = cross_zs[-1] + cfg.cross_aisle_width / 2 + 20
        depot = RoutePoint(min_x + 50, max_z + 13)

        return WarehouseLayout(
            rack_cells=rack_cells,
            crossover_points=crossovers,
            aisle_xs=aisle_xs,
            cross_zs=cross_zs,
            width=max_x - min_x,
            depth=max_z - min_z,
            depot=depot,
            min_x=min_x,
            min_z=min_z,
            max_x=max_x,
            max_z=max_z,
        )
