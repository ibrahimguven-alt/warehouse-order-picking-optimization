import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "python_version"))
from routing.route_manager import RouteManager
from warehouse.warehouse_model import RoutePoint

class RouteGeometryTests(unittest.TestCase):
    def test_simplification_preserves_distance_and_turnbacks(self):
        manager = RouteManager(None)
        cases = [
            [(0, 10), (0, 2), (0, 10)],
            [(10, 0), (2, 0), (10, 0)],
            [(0, 0), (0, 2), (0, 4), (3, 4)],
            [(0, 0), (0, 0), (0, 2), (0, 0)],
            [(0, 0), (2, 0), (1, 0), (3, 0)],
        ]
        for coordinates in cases:
            with self.subTest(coordinates=coordinates):
                route = [RoutePoint(*p) for p in coordinates]
                simplified = manager._simplify(route)
                self.assertEqual(manager.distance_raw(route), manager.distance_raw(simplified))
                self.assertEqual(route[0], simplified[0])
                self.assertEqual(route[-1], simplified[-1])

if __name__ == "__main__":
    unittest.main()
