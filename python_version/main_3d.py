try:
    from ursina import Ursina, window
except ImportError as exc:
    raise SystemExit(
        "Ursina kurulu degil. Once su komutu calistirin:\n"
        "python -m pip install -r requirements.txt"
    ) from exc

from config.warehouse_config import WarehouseConfig
from visualization3d.warehouse_scene import WarehouseScene
from warehouse.warehouse_generator import WarehouseGenerator


scene = None


def input(key):
    if scene:
        scene.input(key)


def update():
    if scene:
        scene.update()


def main():
    global scene
    app = Ursina()
    window.title = "Ibonun Deposu - 3D Gezinme"
    window.borderless = False
    window.fps_counter.enabled = True

    config = WarehouseConfig()
    layout = WarehouseGenerator(config).build_layout()
    scene = WarehouseScene(layout, config)

    app.run()


if __name__ == "__main__":
    main()
