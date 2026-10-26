using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ibonundeposu.formapp
{
    internal static class StatsService
    {
        private static readonly string StorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ibonundeposu.formapp",
            "route-stats.db"
        );

        public static void Record(string username, string routeMethod, int orderCount)
        {
            Record(username, routeMethod, orderCount, "Human", 0m);
        }

        public static void Record(string username, string routeMethod, int orderCount, string vehicle)
        {
            Record(username, routeMethod, orderCount, vehicle, 0m);
        }

        public static void Record(string username, string routeMethod, int orderCount, string vehicle, decimal distance)
        {
            EnsureStore();
            ImportLegacyStats();

            string safeUser = (username ?? "unknown").Replace("|", " ");
            string safeMethod = (routeMethod ?? "unknown").Replace("|", " ");
            string safeVehicle = NormalizeVehicle(vehicle).Replace("|", " ");
            VehicleUsage usage = CalculateUsage(safeVehicle, orderCount);

            SqlDatabaseService.ExecuteNonQuery(
                @"INSERT INTO RouteStats
                  (CreatedAt, Username, Method, OrderCount, Distance, Vehicle, GasLiters, GasCost, ElectricCost, TotalCost)
                  VALUES
                  (@createdAt, @username, @method, @orderCount, @distance, @vehicle, @gasLiters, @gasCost, @electricCost, @totalCost)",
                new SQLiteParameter("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@username", safeUser),
                new SQLiteParameter("@method", safeMethod),
                new SQLiteParameter("@orderCount", orderCount),
                new SQLiteParameter("@distance", distance),
                new SQLiteParameter("@vehicle", safeVehicle),
                new SQLiteParameter("@gasLiters", usage.GasLiters),
                new SQLiteParameter("@gasCost", usage.GasCost),
                new SQLiteParameter("@electricCost", usage.ElectricCost),
                new SQLiteParameter("@totalCost", usage.TotalCost)
            );
        }

        public static List<RouteStat> Load()
        {
            EnsureStore();
            ImportLegacyStats();

            List<RouteStat> stats = new List<RouteStat>();

            DataTable table = SqlDatabaseService.ExecuteTable(
                @"SELECT Id, CreatedAt, Username, Method, OrderCount, Distance, Vehicle,
                         GasLiters, GasCost, ElectricCost, TotalCost
                  FROM RouteStats
                  ORDER BY CreatedAt DESC, Id DESC"
            );

            foreach (DataRow row in table.Rows)
            {
                stats.Add(new RouteStat
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Date = row["CreatedAt"].ToString(),
                    Username = row["Username"].ToString(),
                    Method = row["Method"].ToString(),
                    OrderCount = Convert.ToInt32(row["OrderCount"]),
                    Distance = Convert.ToDecimal(row["Distance"]),
                    Vehicle = row["Vehicle"].ToString(),
                    GasLiters = Convert.ToDecimal(row["GasLiters"]),
                    GasCost = Convert.ToDecimal(row["GasCost"]),
                    ElectricCost = Convert.ToDecimal(row["ElectricCost"]),
                    TotalCost = Convert.ToDecimal(row["TotalCost"])
                });
            }

            return stats;
        }

        public static List<string> SummaryLines()
        {
            return Load()
                .GroupBy(s => new { s.Username, s.Method, s.Vehicle })
                .OrderBy(g => g.Key.Username)
                .ThenBy(g => g.Key.Method)
                .ThenBy(g => g.Key.Vehicle)
                .Select(g =>
                    g.Key.Username + " - " + g.Key.Method + " - " + g.Key.Vehicle + ": " +
                    g.Sum(x => x.OrderCount) + " orders, fuel " +
                    FormatDecimal(g.Sum(x => x.GasLiters)) + " L, electric " +
                    FormatMoney(g.Sum(x => x.ElectricCost)) + ", total " +
                    FormatMoney(g.Sum(x => x.TotalCost)))
                .ToList();
        }

        public static MonthlyUsage GetCurrentMonthUsage()
        {
            DateTime now = DateTime.Now;
            List<RouteStat> monthStats = Load()
                .Where(s => s.Timestamp.Year == now.Year && s.Timestamp.Month == now.Month)
                .ToList();

            return new MonthlyUsage
            {
                GasLiters = monthStats.Sum(s => s.GasLiters),
                GasCost = monthStats.Sum(s => s.GasCost),
                ElectricCost = monthStats.Sum(s => s.ElectricCost),
                TotalCost = monthStats.Sum(s => s.TotalCost),
                OrderCount = monthStats.Sum(s => s.OrderCount)
            };
        }

        public static List<string> EmployeeOfMonthLines()
        {
            DateTime now = DateTime.Now;
            return Load()
                .Where(s => s.Timestamp.Year == now.Year && s.Timestamp.Month == now.Month)
                .GroupBy(s => s.Username)
                .Select(g => new
                {
                    Username = g.Key,
                    Orders = g.Sum(x => x.OrderCount),
                    Cost = g.Sum(x => x.TotalCost),
                    Gas = g.Sum(x => x.GasLiters),
                    Electric = g.Sum(x => x.ElectricCost)
                })
                .OrderByDescending(x => x.Orders)
                .ThenBy(x => x.Cost)
                .ThenBy(x => x.Gas)
                .ThenBy(x => x.Electric)
                .Select((x, index) => (index + 1) + ". " + x.Username + " - " + x.Orders + " orders - " + FormatMoney(x.Cost))
                .ToList();
        }

        public static VehicleUsage CalculateUsage(string vehicle, int orderCount)
        {
            vehicle = NormalizeVehicle(vehicle);
            orderCount = Math.Max(0, orderCount);
            VehicleSettings settings = GetVehicleSettings(vehicle);

            if (vehicle == "Forklift")
            {
                decimal gasLiters = orderCount * settings.LitersPerHour / Math.Max(1m, settings.OrdersPerHour);
                return new VehicleUsage
                {
                    GasLiters = gasLiters,
                    GasCost = gasLiters * settings.FuelPricePerLiter,
                    ElectricCost = 0m
                };
            }

            if (vehicle == "Robot")
            {
                return new VehicleUsage
                {
                    GasLiters = 0m,
                    GasCost = 0m,
                    ElectricCost = orderCount * settings.ChargeCostPerHour / Math.Max(1m, settings.OrdersPerHour)
                };
            }

            return new VehicleUsage();
        }

        public static string NormalizeVehicle(string vehicle)
        {
            string value = (vehicle ?? "").Trim().ToLowerInvariant();

            if (value == "forklift")
                return "Forklift";

            if (value == "robot")
                return "Robot";

            return "Human";
        }

        public static string FormatMoney(decimal value)
        {
            return FormatDecimal(value) + " TL";
        }

        public static string FormatDecimal(decimal value)
        {
            return Math.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static decimal ReadDecimal(string[] parts, int index)
        {
            if (parts.Length <= index)
                return 0m;

            decimal.TryParse(parts[index], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value);
            return value;
        }

        public static VehicleSettings GetVehicleSettings(string vehicle)
        {
            vehicle = NormalizeVehicle(vehicle);
            DataTable table = SqlDatabaseService.ExecuteTable(
                @"SELECT Vehicle, FuelPricePerLiter, LitersPerHour, ChargeCostPerHour, OrdersPerHour
                  FROM VehicleSettings
                  WHERE Vehicle = @vehicle
                  LIMIT 1",
                new SQLiteParameter("@vehicle", vehicle)
            );

            if (table.Rows.Count == 0)
                return new VehicleSettings { Vehicle = vehicle };

            DataRow row = table.Rows[0];
            return new VehicleSettings
            {
                Vehicle = row["Vehicle"].ToString(),
                FuelPricePerLiter = Convert.ToDecimal(row["FuelPricePerLiter"]),
                LitersPerHour = Convert.ToDecimal(row["LitersPerHour"]),
                ChargeCostPerHour = Convert.ToDecimal(row["ChargeCostPerHour"]),
                OrdersPerHour = Convert.ToDecimal(row["OrdersPerHour"])
            };
        }

        public static void SaveVehicleSettings(VehicleSettings forklift, VehicleSettings robot)
        {
            SaveVehicleSetting("Forklift", forklift.FuelPricePerLiter, forklift.LitersPerHour, 0m, forklift.OrdersPerHour);
            SaveVehicleSetting("Robot", 66m, 0m, robot.ChargeCostPerHour, robot.OrdersPerHour);
        }

        private static void SaveVehicleSetting(string vehicle, decimal fuelPrice, decimal litersPerHour, decimal chargeCost, decimal ordersPerHour)
        {
            SqlDatabaseService.ExecuteNonQuery(
                @"UPDATE VehicleSettings
                  SET FuelPricePerLiter = @fuelPrice,
                      LitersPerHour = @litersPerHour,
                      ChargeCostPerHour = @chargeCost,
                      OrdersPerHour = @ordersPerHour
                  WHERE Vehicle = @vehicle",
                new SQLiteParameter("@vehicle", vehicle),
                new SQLiteParameter("@fuelPrice", fuelPrice),
                new SQLiteParameter("@litersPerHour", litersPerHour),
                new SQLiteParameter("@chargeCost", chargeCost),
                new SQLiteParameter("@ordersPerHour", Math.Max(1m, ordersPerHour))
            );
        }

        private static void ImportLegacyStats()
        {
            if (SqlDatabaseService.GetSetting("LegacyRouteStatsImported", "0") == "1")
                return;

            foreach (string line in File.ReadAllLines(StorePath, Encoding.UTF8))
            {
                string[] parts = line.Split('|');

                if (parts.Length < 4)
                    continue;

                int.TryParse(parts[3], out int count);
                string vehicle = parts.Length >= 5 ? NormalizeVehicle(parts[4]) : "Human";
                decimal gasLiters = ReadDecimal(parts, 5);
                decimal electricCost = ReadDecimal(parts, 6);
                decimal totalCost = ReadDecimal(parts, 7);
                VehicleUsage usage = CalculateUsage(vehicle, count);

                if (parts.Length < 8)
                {
                    gasLiters = usage.GasLiters;
                    electricCost = usage.ElectricCost;
                    totalCost = usage.TotalCost;
                }

                DateTime createdAt;
                if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdAt))
                    createdAt = DateTime.Now;

                SqlDatabaseService.ExecuteNonQuery(
                    @"INSERT INTO RouteStats
                      (CreatedAt, Username, Method, OrderCount, Distance, Vehicle, GasLiters, GasCost, ElectricCost, TotalCost)
                      VALUES
                      (@createdAt, @username, @method, @count, 0, @vehicle, @gasLiters, @gasCost, @electricCost, @totalCost)",
                    new SQLiteParameter("@createdAt", createdAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SQLiteParameter("@username", parts[1]),
                    new SQLiteParameter("@method", parts[2]),
                    new SQLiteParameter("@count", count),
                    new SQLiteParameter("@vehicle", vehicle),
                    new SQLiteParameter("@gasLiters", gasLiters),
                    new SQLiteParameter("@gasCost", gasLiters * GetVehicleSettings("Forklift").FuelPricePerLiter),
                    new SQLiteParameter("@electricCost", electricCost),
                    new SQLiteParameter("@totalCost", totalCost)
                );
            }

            SqlDatabaseService.SetSetting("LegacyRouteStatsImported", "1");
        }

        private static void EnsureStore()
        {
            string directory = Path.GetDirectoryName(StorePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(StorePath))
                File.WriteAllText(StorePath, "", Encoding.UTF8);
        }
    }

    internal class RouteStat
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Username { get; set; }
        public string Method { get; set; }
        public int OrderCount { get; set; }
        public decimal Distance { get; set; }
        public string Vehicle { get; set; }
        public decimal GasLiters { get; set; }
        public decimal GasCost { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime Timestamp
        {
            get
            {
                DateTime.TryParseExact(Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp);
                return timestamp == DateTime.MinValue ? DateTime.Now : timestamp;
            }
        }
    }

    internal class VehicleUsage
    {
        public decimal GasLiters { get; set; }
        public decimal GasCost { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal TotalCost
        {
            get { return GasCost + ElectricCost; }
        }
    }

    internal class MonthlyUsage
    {
        public decimal GasLiters { get; set; }
        public decimal GasCost { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal TotalCost { get; set; }
        public int OrderCount { get; set; }
    }

    internal class VehicleSettings
    {
        public string Vehicle { get; set; }
        public decimal FuelPricePerLiter { get; set; } = 66m;
        public decimal LitersPerHour { get; set; } = 5m;
        public decimal ChargeCostPerHour { get; set; } = 0.5m;
        public decimal OrdersPerHour { get; set; } = 60m;
    }
}
