from dataclasses import dataclass
from math import fabs

from orders.order_model import Order, OrderItem
from warehouse.warehouse_model import RackCell, RoutePoint, WarehouseLayout


@dataclass
class AisleInfo:
    x: float
    items: list[RackCell]

    @property
    def has_items(self) -> bool:
        return bool(self.items)

    @property
    def min_z(self) -> float:
        return min(cell.z for cell in self.items)

    @property
    def max_z(self) -> float:
        return max(cell.z + cell.depth for cell in self.items)

    @property
    def top_item_center(self) -> float:
        return min(cell.center.z for cell in self.items)


class RouteManager:
    def __init__(self, layout: WarehouseLayout):
        self.layout = layout

    def create_demo_order(self, count: int = 10) -> Order:
        step = max(1, len(self.layout.rack_cells) // count)
        chosen = self.layout.rack_cells[::step][:count]

        for cell in self.layout.rack_cells:
            cell.selected = False
            cell.picked = False
            cell.selected_shelves.clear()

        for cell in chosen:
            cell.selected = True
            cell.selected_shelves.add(1)

        return Order("Ornek Siparis", [OrderItem(cell.index, 1) for cell in chosen])

    def make_demo_route(self, order: Order) -> list[RoutePoint]:
        route = [self.layout.depot]
        bottom_z = max(self.layout.cross_zs)
        route.append(RoutePoint(self.layout.depot.x, bottom_z))

        selected_cells = [self.layout.rack_cells[item.rack_index] for item in order.items]
        selected_cells.sort(key=lambda c: (self._nearest_aisle_x(c), c.z))

        last_aisle = None
        for cell in selected_cells:
            aisle_x = self._nearest_aisle_x(cell)
            if aisle_x != last_aisle:
                route.append(RoutePoint(aisle_x, bottom_z))
                last_aisle = aisle_x
            route.append(RoutePoint(aisle_x, cell.center.z))

        if last_aisle is not None:
            route.append(RoutePoint(last_aisle, bottom_z))
        route.append(RoutePoint(self.layout.depot.x, bottom_z))
        route.append(self.layout.depot)
        return self._simplify(route)

    def selected_cells(self) -> list[RackCell]:
        return [cell for cell in self.layout.rack_cells if cell.selected]

    def select_random_order(self, count: int, seed: int | None = None) -> Order:
        import random

        rng = random.Random(seed)
        for cell in self.layout.rack_cells:
            cell.selected = False
            cell.picked = False
            cell.selected_shelves.clear()
            cell.picked_shelves.clear()

        cells = self.layout.rack_cells[:]
        rng.shuffle(cells)
        chosen = cells[: max(0, min(count, len(cells)))]
        for cell in chosen:
            cell.selected = True
            cell.selected_shelves.add(1)
        return Order("Siparis", [OrderItem(cell.index, 1) for cell in chosen])

    def clear_order(self) -> None:
        for cell in self.layout.rack_cells:
            cell.selected = False
            cell.picked = False
            cell.selected_shelves.clear()
            cell.picked_shelves.clear()

    def toggle_cell(self, cell: RackCell) -> None:
        cell.selected = not cell.selected
        cell.picked = False
        if cell.selected:
            cell.selected_shelves.add(1)
        else:
            cell.selected_shelves.clear()
            cell.picked_shelves.clear()

    def s_shape_route(self) -> list[RoutePoint]:
        groups = self._aisles_with_items()
        if not groups:
            return []

        route = [self.layout.depot]
        top_z = min(self.layout.cross_zs)
        bottom_z = max(self.layout.cross_zs)
        current = RoutePoint(self.layout.depot.x, bottom_z)
        route.append(current)
        going_up = True

        for i, group in enumerate(groups):
            next_group = groups[i + 1] if i + 1 < len(groups) else group
            route.append(RoutePoint(group.x, current.z))

            if going_up:
                highest = min(group.min_z, next_group.min_z)
                possible = [z for z in self.layout.cross_zs if z <= highest]
                target_z = possible[-1] if possible else top_z
            else:
                lowest = max(group.max_z, next_group.max_z)
                possible = [z for z in self.layout.cross_zs if z >= lowest]
                target_z = possible[0] if possible else bottom_z

            current = RoutePoint(group.x, target_z)
            route.append(current)
            going_up = not going_up

        if current.z != bottom_z:
            route.append(RoutePoint(groups[-1].x, bottom_z))
        route.append(RoutePoint(self.layout.depot.x, bottom_z))
        route.append(self.layout.depot)
        return self._simplify(route)

    def combined_plus_route(self) -> list[RoutePoint]:
        groups = self._aisles_with_items()
        if not groups:
            return []

        top_z = min(self.layout.cross_zs)
        bottom_z = max(self.layout.cross_zs)
        depot = self.layout.depot
        n = len(groups)
        cost_top = [0.0] * n
        cost_bot = [0.0] * n
        path_top: list[list[RoutePoint]] = [[] for _ in groups]
        path_bot: list[list[RoutePoint]] = [[] for _ in groups]

        for i, group in enumerate(groups):
            min_z = group.min_z
            max_z = group.max_z
            full = abs(bottom_z - top_z)
            return_top = 2.0 * abs(max_z - top_z)
            return_bot = 2.0 * abs(bottom_z - min_z)

            if i == 0:
                dx = abs(group.x - depot.x)
                cost_top[i] = dx + full
                path_top[i] = [
                    depot,
                    RoutePoint(depot.x, bottom_z),
                    RoutePoint(group.x, bottom_z),
                    RoutePoint(group.x, top_z),
                ]
                cost_bot[i] = dx + return_bot
                path_bot[i] = [
                    depot,
                    RoutePoint(depot.x, bottom_z),
                    RoutePoint(group.x, bottom_z),
                    RoutePoint(group.x, min_z),
                    RoutePoint(group.x, bottom_z),
                ]
            else:
                prev_x = groups[i - 1].x
                dx = abs(group.x - prev_x)

                top_from_top = cost_top[i - 1] + dx + return_top
                top_from_bot = cost_bot[i - 1] + dx + full
                if top_from_top <= top_from_bot:
                    cost_top[i] = top_from_top
                    path_top[i] = path_top[i - 1] + [
                        RoutePoint(group.x, top_z),
                        RoutePoint(group.x, max_z),
                        RoutePoint(group.x, top_z),
                    ]
                else:
                    cost_top[i] = top_from_bot
                    path_top[i] = path_bot[i - 1] + [
                        RoutePoint(group.x, bottom_z),
                        RoutePoint(group.x, top_z),
                    ]

                bot_from_top = cost_top[i - 1] + dx + full
                bot_from_bot = cost_bot[i - 1] + dx + return_bot
                if bot_from_top <= bot_from_bot:
                    cost_bot[i] = bot_from_top
                    path_bot[i] = path_top[i - 1] + [
                        RoutePoint(group.x, top_z),
                        RoutePoint(group.x, bottom_z),
                    ]
                else:
                    cost_bot[i] = bot_from_bot
                    path_bot[i] = path_bot[i - 1] + [
                        RoutePoint(group.x, bottom_z),
                        RoutePoint(group.x, min_z),
                        RoutePoint(group.x, bottom_z),
                    ]

        final_route = path_bot[-1] + [RoutePoint(depot.x, bottom_z), depot]
        return self._simplify(final_route)

    def roodbergen_route(self) -> list[RoutePoint]:
        groups = self._all_aisle_infos()
        useful = [g for g in groups if g.has_items]
        if not useful:
            return []

        top_z = min(self.layout.cross_zs)
        bottom_z = max(self.layout.cross_zs)
        depot = self.layout.depot
        depot_front = RoutePoint(depot.x, bottom_z)

        best = self._front_only_candidate(useful, depot, depot_front, bottom_z)
        for up in range(len(groups)):
            for down in range(up, len(groups)):
                candidate = self._loop_candidate(groups, up, down, depot, depot_front, top_z, bottom_z)
                if self.distance_raw(candidate) < self.distance_raw(best):
                    best = candidate
        return self._simplify(best)

    def distance(self, route: list[RoutePoint]) -> float:
        if not route:
            return 0.0
        total = 0.0
        for a, b in zip(route, route[1:]):
            total += fabs(a.x - b.x) + fabs(a.z - b.z)
        return round(total / 18.0, 1)

    def distance_raw(self, route: list[RoutePoint]) -> float:
        total = 0.0
        for a, b in zip(route, route[1:]):
            total += fabs(a.x - b.x) + fabs(a.z - b.z)
        return total

    def _nearest_aisle_x(self, cell: RackCell) -> float:
        center_x = cell.center.x
        return min(self.layout.aisle_xs, key=lambda x: abs(x - center_x))

    def _aisles_with_items(self) -> list[AisleInfo]:
        return [info for info in self._all_aisle_infos() if info.has_items]

    def _all_aisle_infos(self) -> list[AisleInfo]:
        by_aisle = {x: [] for x in self.layout.aisle_xs}
        for cell in self.selected_cells():
            by_aisle[self._nearest_aisle_x(cell)].append(cell)
        return [AisleInfo(x, by_aisle[x]) for x in sorted(by_aisle)]

    def _front_only_candidate(self, groups, depot, depot_front, bottom_z):
        route = [depot, depot_front]
        for group in sorted(groups, key=lambda g: abs(g.x - depot.x)):
            route.extend([
                RoutePoint(group.x, bottom_z),
                RoutePoint(group.x, group.top_item_center),
                RoutePoint(group.x, bottom_z),
            ])
        route.extend([depot_front, depot])
        return route

    def _loop_candidate(self, groups, up, down, depot, depot_front, top_z, bottom_z):
        up_group = groups[up]
        down_group = groups[down]
        route = [
            depot,
            depot_front,
            RoutePoint(up_group.x, bottom_z),
            RoutePoint(up_group.x, top_z),
        ]

        for i in range(up + 1, down):
            group = groups[i]
            route.append(RoutePoint(group.x, top_z))
            if group.has_items:
                gap_start, _ = self._largest_gap(group, top_z, bottom_z)
                route.extend([RoutePoint(group.x, gap_start), RoutePoint(group.x, top_z)])

        route.extend([RoutePoint(down_group.x, top_z), RoutePoint(down_group.x, bottom_z)])

        for group in [g for g in groups[down + 1 :] if g.has_items]:
            route.extend([
                RoutePoint(group.x, bottom_z),
                RoutePoint(group.x, group.top_item_center),
                RoutePoint(group.x, bottom_z),
            ])

        for i in range(down - 1, up, -1):
            group = groups[i]
            route.append(RoutePoint(group.x, bottom_z))
            if group.has_items:
                _, gap_end = self._largest_gap(group, top_z, bottom_z)
                route.extend([RoutePoint(group.x, gap_end), RoutePoint(group.x, bottom_z)])

        for group in [g for g in groups[:up] if g.has_items][::-1]:
            route.extend([
                RoutePoint(group.x, bottom_z),
                RoutePoint(group.x, group.top_item_center),
                RoutePoint(group.x, bottom_z),
            ])

        route.extend([depot_front, depot])
        return route

    def _largest_gap(self, group: AisleInfo, top_z: float, bottom_z: float) -> tuple[float, float]:
        item_zs = sorted(cell.center.z for cell in group.items)
        best_start = top_z
        best_end = item_zs[0]
        best_gap = best_end - best_start
        for a, b in zip(item_zs, item_zs[1:]):
            if b - a > best_gap:
                best_start, best_end, best_gap = a, b, b - a
        if bottom_z - item_zs[-1] > best_gap:
            best_start, best_end = item_zs[-1], bottom_z
        return best_start, best_end

    def _simplify(self, route: list[RoutePoint]) -> list[RoutePoint]:
        if len(route) < 3:
            return route

        cleaned = [route[0]]
        for i in range(1, len(route) - 1):
            prev = cleaned[-1]
            cur = route[i]
            nxt = route[i + 1]
            same_x = prev.x == cur.x == nxt.x
            same_z = prev.z == cur.z == nxt.z
            vertical_turnback = same_x and (cur.z - prev.z) * (nxt.z - cur.z) < 0
            horizontal_turnback = same_z and (cur.x - prev.x) * (nxt.x - cur.x) < 0
            if (not same_x and not same_z) or vertical_turnback or horizontal_turnback:
                cleaned.append(cur)
        cleaned.append(route[-1])
        return cleaned
