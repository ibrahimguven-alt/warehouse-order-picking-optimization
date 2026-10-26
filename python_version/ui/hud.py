from ursina import Text, color, time


class Hud:
    def __init__(self, distance, product_count):
        self.distance = distance
        self.product_count = product_count
        self.text = Text(
            text="",
            position=(-0.86, 0.47),
            origin=(-0.5, 0.5),
            scale=1,
            color=color.white,
            background=True,
        )

    def update(self):
        self.text.text = (
            f"Mesafe: {self.distance} | Urun: {self.product_count} | "
            f"FPS: {int(1 / max(time.dt, 0.001))}\n"
            "WASD yuru | Mouse bak | F kamera | R rota | M marker | T reset | Esc mouse"
        )
