from ursina import Entity, Vec3, camera


class CameraController:
    def __init__(self, player, layout):
        self.player = player
        self.layout = layout
        self.mode = "fps"
        self.overview_holder = Entity()
        self.world_width = layout.width * 0.08
        self.world_depth = layout.depth * 0.08

    def toggle(self):
        if self.mode == "fps":
            self.mode = "overview"
            camera.parent = self.overview_holder
            camera.position = (self.world_width / 2, 28, self.world_depth / 2)
            camera.rotation = (90, 0, 0)
        else:
            self.mode = "fps"
            camera.parent = self.player.camera_pivot
            camera.position = Vec3(0, 0, 0)
            camera.rotation = Vec3(0, 0, 0)
