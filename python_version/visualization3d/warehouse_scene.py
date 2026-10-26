from ursina import AmbientLight, DirectionalLight, Entity, Sky, Text, Vec3, color, mouse

from config.warehouse_config import WarehouseConfig
from routing.route_manager import RouteManager
from ui.hud import Hud
from visualization3d.camera_controller import CameraController
from visualization3d.player_controller import SimpleFpsPlayer
from visualization3d.route_renderer import RouteRenderer
from warehouse.warehouse_model import WarehouseLayout
from warehouse.warehouse_model import RoutePoint


class WarehouseScene:
    def __init__(self, layout: WarehouseLayout, config: WarehouseConfig):
        self.layout = layout
        self.config = config
        self.route_renderer = RouteRenderer(self._to_world)
        self.route_manager = RouteManager(layout)
        self.order = self.route_manager.create_demo_order(10)
        self.start_point = RoutePoint(layout.depot.x, max(layout.cross_zs))
        self.route = self.route_manager.make_demo_route(self.order)
        self.route = self._route_inside_warehouse(self.route)
        self.hud = Hud(self.route_manager.distance(self.route), len(self.order.items))

        self.obstacles = []
        start_world = self._to_world(self.start_point)
        start_pos = Vec3(start_world.x, 0.2, start_world.z)
        self.player = SimpleFpsPlayer(start_pos, obstacles=self.obstacles)
        self.camera_controller = CameraController(self.player, layout)

        self._build_environment()
        self._build_route_helpers()

    def _build_environment(self):
        cfg = self.config
        width = self.layout.width * cfg.world_scale
        depth = self.layout.depth * cfg.world_scale
        Entity(
            model="cube",
            color=color.rgb(62, 68, 70),
            position=(width / 2, -0.06, depth / 2),
            scale=(width + 4, 0.1, depth + 4),
        )

        for aisle_x in self.layout.aisle_xs:
            aisle_world = self._to_world(RoutePoint(aisle_x, self.layout.min_z))
            Entity(
                model="cube",
                color=color.rgb(82, 92, 96),
                position=(aisle_world.x, 0.01, depth / 2),
                scale=(self.config.corridor_width * cfg.world_scale, 0.03, depth),
            )

        self._wall(width / 2, cfg.wall_height / 2, -1.0, width + 3, cfg.wall_height, 0.25)
        self._wall(-1.0, cfg.wall_height / 2, depth / 2, 0.25, cfg.wall_height, depth + 3)
        self._wall(width + 1.0, cfg.wall_height / 2, depth / 2, 0.25, cfg.wall_height, depth + 3)
        # On tarafta tam duvar yerine giris boslugu var; oyuncu burada basliyor.
        self._wall(width * 0.25, cfg.wall_height / 2, depth + 1.0, width * 0.5 - 1.2, cfg.wall_height, 0.25)
        self._wall(width * 0.85, cfg.wall_height / 2, depth + 1.0, width * 0.3, cfg.wall_height, 0.25)

        for cell in self.layout.rack_cells:
            rack_color = color.rgb(140, 116, 76)
            if cell.selected:
                rack_color = color.rgb(35, 120, 210)
            center = self._to_world(cell.center)
            sx = cell.width * cfg.world_scale * 0.94
            sz = cell.depth * cfg.world_scale * 0.94
            Entity(
                model="cube",
                color=rack_color,
                position=(center.x, cfg.rack_height / 2, center.z),
                scale=(sx, cfg.rack_height, sz),
            )
            self.obstacles.append((center.x - sx / 2, center.x + sx / 2, center.z - sz / 2, center.z + sz / 2))

        depot = self._to_world(self.start_point)
        Entity(
            model="cube",
            color=color.lime,
            position=(depot.x, 0.05, depot.z),
            scale=(1.8, 0.08, 1.8),
        )
        Text(
            text="BASLANGIC",
            position=(-0.12, 0.4),
            origin=(0, 0),
            scale=1,
            color=color.white,
            background=True,
        )

        DirectionalLight(rotation=(45, -35, 45), shadows=True)
        AmbientLight(color=color.rgba(95, 95, 95, 255))
        Sky(color=color.rgb(28, 34, 42))

    def _wall(self, x, y, z, sx, sy, sz):
        Entity(
            model="cube",
            color=color.rgb(105, 110, 115),
            position=(x, y, z),
            scale=(sx, sy, sz),
            collider="box",
        )
        self.obstacles.append((x - sx / 2, x + sx / 2, z - sz / 2, z + sz / 2))

    def _build_route_helpers(self):
        cells = [self.layout.rack_cells[item.rack_index] for item in self.order.items]
        self.route_renderer.draw_route(self.route)
        self.route_renderer.draw_markers(cells)

    def input(self, key):
        if key == "f":
            self.camera_controller.toggle()
        elif key == "r":
            self.route_renderer.toggle_route()
        elif key == "m":
            self.route_renderer.toggle_markers()
        elif key == "t":
            depot = self._to_world(self.start_point)
            self.player.reset_to(Vec3(depot.x, 0.2, depot.z))
        elif key == "escape":
            mouse.locked = not mouse.locked

    def update(self):
        self.hud.update()

    def _to_world(self, point):
        return RoutePoint(
            (point.x - self.layout.min_x) * self.config.world_scale,
            (point.z - self.layout.min_z) * self.config.world_scale,
        )

    def _route_inside_warehouse(self, route):
        if len(route) < 2:
            return route

        inside = self.start_point
        new_route = route[:]
        new_route[0] = inside
        new_route[-1] = inside
        return new_route
