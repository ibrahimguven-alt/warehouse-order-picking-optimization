import subprocess
import sys
import tkinter as tk
from tkinter import messagebox, ttk
from pathlib import Path

from config.warehouse_config import WarehouseConfig
from routing.route_manager import RouteManager
from warehouse.warehouse_generator import WarehouseGenerator


class DesktopRouteApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Ibonun Deposu - Rota Optimizasyonu")
        self.root.geometry("1120x720")

        self.config = WarehouseConfig()
        self.layout = None
        self.route_manager = None
        self.route = []
        self.active_method = "-"

        self._build_widgets()
        self.build_warehouse()

    def run(self):
        self.root.mainloop()

    def _build_widgets(self):
        main = ttk.Frame(self.root, padding=10)
        main.pack(fill="both", expand=True)

        left = ttk.Frame(main, width=270)
        left.pack(side="left", fill="y", padx=(0, 10))
        left.pack_propagate(False)

        self.canvas = tk.Canvas(main, bg="white", highlightthickness=1, highlightbackground="#999")
        self.canvas.pack(side="right", fill="both", expand=True)
        self.canvas.bind("<Button-1>", self._canvas_click)

        ttk.Label(left, text="Depo olculeri", font=("Segoe UI", 12, "bold")).pack(anchor="w")
        self.blocks_var = self._entry(left, "Blok sayisi", self.config.blocks)
        self.aisles_var = self._entry(left, "Koridor sayisi", self.config.aisles)
        self.locations_var = self._entry(left, "Koridordaki lokasyon", self.config.locations)
        self.order_count_var = self._entry(left, "Siparis urun sayisi", 10)

        ttk.Button(left, text="Depoyu olustur", command=self.build_warehouse).pack(fill="x", pady=(10, 4))
        ttk.Button(left, text="Rastgele siparis olustur", command=self.random_order).pack(fill="x", pady=4)
        ttk.Button(left, text="Siparisi temizle", command=self.clear_order).pack(fill="x", pady=4)

        ttk.Separator(left).pack(fill="x", pady=12)
        ttk.Label(left, text="Rota algoritmalari", font=("Segoe UI", 12, "bold")).pack(anchor="w")
        ttk.Button(left, text="S-Shape", command=lambda: self.calculate_route("S-Shape")).pack(fill="x", pady=4)
        ttk.Button(left, text="Combined +", command=lambda: self.calculate_route("Combined +")).pack(fill="x", pady=4)
        ttk.Button(left, text="Roodbergen", command=lambda: self.calculate_route("Roodbergen")).pack(fill="x", pady=4)

        ttk.Separator(left).pack(fill="x", pady=12)
        ttk.Button(left, text="3D depoda gez", command=self.open_3d).pack(fill="x", pady=4)

        self.info = tk.StringVar()
        ttk.Label(left, textvariable=self.info, justify="left").pack(anchor="w", pady=12)
        ttk.Label(
            left,
            text="Mavi: siparis urunu\nSiyah: raf\nKirmizi: rota\nYesil: depo girisi\n\nUrun secmek icin rafa tiklayabilirsin.",
            justify="left",
        ).pack(anchor="w", pady=8)

    def _entry(self, parent, label, value):
        ttk.Label(parent, text=label).pack(anchor="w", pady=(8, 0))
        var = tk.StringVar(value=str(value))
        ttk.Entry(parent, textvariable=var).pack(fill="x")
        return var

    def build_warehouse(self):
        self.config.blocks = self._read_int(self.blocks_var, 2)
        self.config.aisles = self._read_int(self.aisles_var, 5)
        self.config.locations = self._read_int(self.locations_var, 5)
        self.layout = WarehouseGenerator(self.config).build_layout()
        self.route_manager = RouteManager(self.layout)
        self.route = []
        self.active_method = "-"
        self._draw()

    def random_order(self):
        count = self._read_int(self.order_count_var, 10)
        self.route_manager.select_random_order(count)
        self.route = []
        self.active_method = "-"
        self._draw()

    def clear_order(self):
        self.route_manager.clear_order()
        self.route = []
        self.active_method = "-"
        self._draw()

    def calculate_route(self, method):
        if not self.route_manager.selected_cells():
            messagebox.showinfo("Siparis yok", "Once rastgele siparis olustur veya rafa tiklayarak urun sec.")
            return

        if method == "S-Shape":
            self.route = self.route_manager.s_shape_route()
        elif method == "Combined +":
            self.route = self.route_manager.combined_plus_route()
        else:
            self.route = self.route_manager.roodbergen_route()
        self.active_method = method
        self._draw()

    def open_3d(self):
        if getattr(sys, "frozen", False):
            candidates = [
                Path(sys.executable).parent.parent / "Ibonun_Deposu_3D_App" / "Ibonun_Deposu_3D_App.exe",
                Path(sys.executable).parent.parent / "Ibonun_Deposu_3D" / "Ibonun_Deposu_3D.exe",
                Path(sys.executable).with_name("Ibonun_Deposu_3D_App.exe"),
                Path(sys.executable).with_name("Ibonun_Deposu_3D.exe"),
            ]
            for exe in candidates:
                if exe.exists():
                    subprocess.Popen([str(exe)], cwd=str(exe.parent))
                    return

        main_py = Path(__file__).resolve().parents[1] / "main_3d.py"
        if not main_py.exists():
            messagebox.showerror("3D yok", "main_3d.py bulunamadi.")
            return
        subprocess.Popen([sys.executable, str(main_py)], cwd=str(main_py.parent))

    def _canvas_click(self, event):
        cell = self._cell_at(event.x, event.y)
        if cell:
            self.route_manager.toggle_cell(cell)
            self.route = []
            self.active_method = "-"
            self._draw()

    def _cell_at(self, x, y):
        for cell in self.layout.rack_cells:
            if cell.x <= x <= cell.x + cell.width and cell.z <= y <= cell.z + cell.depth:
                return cell
        return None

    def _draw(self):
        self.canvas.delete("all")
        self.canvas.create_rectangle(
            self.layout.min_x,
            self.layout.min_z,
            self.layout.max_x,
            self.layout.max_z,
            outline="#333",
            width=2,
        )
        self.canvas.create_rectangle(
            self.layout.depot.x - 30,
            self.layout.depot.z - 13,
            self.layout.depot.x + 30,
            self.layout.depot.z + 13,
            fill="#2e9d48",
            outline="#1c6b30",
        )
        self.canvas.create_text(self.layout.depot.x, self.layout.depot.z, text="depot", fill="white")

        for cp in self.layout.crossover_points:
            self.canvas.create_text(cp.x, cp.z, text="#", fill="#888", font=("Segoe UI", 8))

        for cell in self.layout.rack_cells:
            fill = "#ffffff"
            if cell.selected:
                fill = "#1976d2"
            if cell.picked:
                fill = "#111111"
            self.canvas.create_rectangle(
                cell.x,
                cell.z,
                cell.x + cell.width,
                cell.z + cell.depth,
                fill=fill,
                outline="#555",
            )

        if len(self.route) >= 2:
            points = []
            for point in self.route:
                points.extend([point.x, point.z])
            self.canvas.create_line(*points, fill="#d71920", width=4, capstyle="round", joinstyle="round")

            for i, point in enumerate(self.route, start=1):
                self.canvas.create_oval(point.x - 3, point.z - 3, point.x + 3, point.z + 3, fill="#d71920", outline="")
                if i < len(self.route):
                    nxt = self.route[i]
                    mx = (point.x + nxt.x) / 2
                    mz = (point.z + nxt.z) / 2
                    self.canvas.create_text(mx, mz - 8, text=">", fill="#d71920", font=("Segoe UI", 10, "bold"))

        selected = len(self.route_manager.selected_cells())
        dist = self.route_manager.distance(self.route)
        self.info.set(
            f"Algoritma: {self.active_method}\n"
            f"Secili urun: {selected}\n"
            f"Rota mesafesi: {dist}\n"
            f"Rota nokta sayisi: {len(self.route)}"
        )

    def _read_int(self, var, default):
        try:
            return max(1, int(var.get()))
        except ValueError:
            return default


def main():
    DesktopRouteApp().run()


if __name__ == "__main__":
    main()
