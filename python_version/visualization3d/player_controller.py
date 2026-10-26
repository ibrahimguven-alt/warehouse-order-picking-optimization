from ursina import Entity, Vec3, camera, held_keys, mouse, time


class SimpleFpsPlayer(Entity):
    def __init__(self, start_pos, speed=4.0, fast_speed=7.0, obstacles=None):
        super().__init__()
        self.position = start_pos
        self.speed = speed
        self.fast_speed = fast_speed
        self.obstacles = obstacles or []
        self.radius = 0.32
        self.collider = "box"
        self.height = 1.8
        self.camera_pivot = Entity(parent=self, y=self.height)
        camera.parent = self.camera_pivot
        camera.position = (0, 0, 0)
        camera.rotation = (0, 0, 0)
        self.rotation_y = 180
        mouse.locked = True

    def update(self):
        if not mouse.locked:
            return

        self.rotation_y += mouse.velocity[0] * 80
        self.camera_pivot.rotation_x -= mouse.velocity[1] * 80
        self.camera_pivot.rotation_x = max(-85, min(85, self.camera_pivot.rotation_x))

        direction = Vec3(
            held_keys["d"] - held_keys["a"],
            0,
            held_keys["w"] - held_keys["s"],
        ).normalized()

        if direction.length() == 0:
            return

        walk_speed = self.fast_speed if held_keys["shift"] else self.speed
        move = (self.forward * direction.z + self.right * direction.x) * walk_speed * time.dt
        self._try_move(move)

    def _try_move(self, move):
        if move.length() == 0:
            return

        next_x = Vec3(self.x + move.x, self.y, self.z)
        if not self._blocked(next_x):
            self.x += move.x

        next_z = Vec3(self.x, self.y, self.z + move.z)
        if not self._blocked(next_z):
            self.z += move.z

    def _blocked(self, position):
        px = position.x
        pz = position.z
        for min_x, max_x, min_z, max_z in self.obstacles:
            if (
                px + self.radius > min_x
                and px - self.radius < max_x
                and pz + self.radius > min_z
                and pz - self.radius < max_z
            ):
                return True
        return False

    def reset_to(self, pos):
        self.position = pos
        self.rotation_y = 180
        self.camera_pivot.rotation_x = 0
