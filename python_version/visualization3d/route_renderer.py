from ursina import Entity, Text, color, destroy


class RouteRenderer:
    def __init__(self, to_world=None):
        self.route_entities: list[Entity] = []
        self.marker_entities: list[Entity] = []
        self.to_world = to_world or (lambda p: p)

    def draw_route(self, route):
        self.clear_route()
        world_route = [self.to_world(point) for point in route]
        for a, b in zip(world_route, world_route[1:]):
            mid_x = (a.x + b.x) / 2
            mid_z = (a.z + b.z) / 2
            length_x = abs(a.x - b.x)
            length_z = abs(a.z - b.z)
            if length_x >= length_z:
                scale = (max(length_x, 0.08), 0.04, 0.12)
            else:
                scale = (0.12, 0.04, max(length_z, 0.08))
            self.route_entities.append(
                Entity(
                    model="cube",
                    color=color.red,
                    position=(mid_x, 0.04, mid_z),
                    scale=scale,
                )
            )

    def draw_markers(self, cells):
        self.clear_markers()
        for number, cell in enumerate(cells, start=1):
            center = self.to_world(cell.center)
            marker = Entity(
                model="sphere",
                color=color.azure,
                position=(center.x, 0.45, center.z),
                scale=0.35,
            )
            label = Text(
                text=str(number),
                parent=marker,
                y=1.0,
                scale=7,
                billboard=True,
                color=color.white,
            )
            self.marker_entities.extend([marker, label])

    def toggle_route(self):
        for entity in self.route_entities:
            entity.enabled = not entity.enabled

    def toggle_markers(self):
        for entity in self.marker_entities:
            entity.enabled = not entity.enabled

    def clear_route(self):
        for entity in self.route_entities:
            destroy(entity)
        self.route_entities.clear()

    def clear_markers(self):
        for entity in self.marker_entities:
            destroy(entity)
        self.marker_entities.clear()
