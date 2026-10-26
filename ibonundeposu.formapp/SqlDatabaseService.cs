using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ibonundeposu.formapp
{
    internal static class SqlDatabaseService
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ibonundeposu.formapp"
        );

        private static readonly string DatabasePath = Path.Combine(AppFolder, "warehouse-route-planning.sqlite");
        private static bool initialized;

        public static SQLiteConnection OpenConnection()
        {
            EnsureDatabase();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();
            return connection;
        }

        public static void ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                return command.ExecuteScalar();
            }
        }

        public static DataTable ExecuteTable(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                DataTable table = new DataTable();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    adapter.Fill(table);

                return table;
            }
        }

        public static string GetSetting(string key, string defaultValue = "")
        {
            object value = ExecuteScalar(
                "SELECT SettingValue FROM AppSettings WHERE SettingKey = @key",
                new SQLiteParameter("@key", key)
            );

            return value == null || value == DBNull.Value ? defaultValue : value.ToString();
        }

        public static void SetSetting(string key, string value)
        {
            ExecuteNonQuery(
                @"INSERT OR REPLACE INTO AppSettings (SettingKey, SettingValue)
                  VALUES (@key, @value)",
                new SQLiteParameter("@key", key),
                new SQLiteParameter("@value", value ?? "")
            );
        }

        public static void EnsureDatabase()
        {
            if (initialized)
                return;

            Directory.CreateDirectory(AppFolder);

            if (!File.Exists(DatabasePath))
                SQLiteConnection.CreateFile(DatabasePath);

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;"))
            {
                connection.Open();
                ExecuteSchema(connection);
            }

            initialized = true;
        }

        private static void ExecuteSchema(SQLiteConnection connection)
        {
            string schema = @"
CREATE TABLE IF NOT EXISTS Users
(
    Username TEXT NOT NULL PRIMARY KEY,
    Salt TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    UserRole TEXT NOT NULL DEFAULT 'Employee',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS RouteStats
(
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    CreatedAt TEXT NOT NULL,
    Username TEXT NOT NULL,
    Method TEXT NOT NULL,
    OrderCount INTEGER NOT NULL,
    Distance REAL NOT NULL DEFAULT 0,
    Vehicle TEXT NOT NULL,
    GasLiters REAL NOT NULL DEFAULT 0,
    GasCost REAL NOT NULL DEFAULT 0,
    ElectricCost REAL NOT NULL DEFAULT 0,
    TotalCost REAL NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS VehicleSettings
(
    Vehicle TEXT NOT NULL PRIMARY KEY,
    FuelPricePerLiter REAL NOT NULL DEFAULT 66,
    LitersPerHour REAL NOT NULL DEFAULT 5,
    ChargeCostPerHour REAL NOT NULL DEFAULT 0.5,
    OrdersPerHour REAL NOT NULL DEFAULT 60
);

INSERT OR IGNORE INTO VehicleSettings (Vehicle, FuelPricePerLiter, LitersPerHour, ChargeCostPerHour, OrdersPerHour)
VALUES ('Human', 66, 0, 0, 60);
INSERT OR IGNORE INTO VehicleSettings (Vehicle, FuelPricePerLiter, LitersPerHour, ChargeCostPerHour, OrdersPerHour)
VALUES ('Forklift', 66, 5, 0, 60);
INSERT OR IGNORE INTO VehicleSettings (Vehicle, FuelPricePerLiter, LitersPerHour, ChargeCostPerHour, OrdersPerHour)
VALUES ('Robot', 66, 0, 0.5, 60);

CREATE TABLE IF NOT EXISTS AppSettings
(
    SettingKey TEXT NOT NULL PRIMARY KEY,
    SettingValue TEXT NOT NULL
);";

            using (SQLiteCommand command = new SQLiteCommand(schema, connection))
                command.ExecuteNonQuery();
        }
    }
}
