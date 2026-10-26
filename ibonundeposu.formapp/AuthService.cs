using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ibonundeposu.formapp
{
    internal static class AuthService
    {
        private const string AdminUsername = "admin";
        private static readonly string AdminPassword =
            Environment.GetEnvironmentVariable("IBON_ADMIN_PASSWORD");

        private static readonly string StorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ibonundeposu.formapp",
            "users.db"
        );

        public static bool ValidateAdmin(string username, string password)
        {
            return string.Equals(username.Trim(), AdminUsername, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(AdminPassword) && password == AdminPassword;
        }

        public static bool ValidateUser(string username, string password)
        {
            if (ValidateAdmin(username, password))
                return true;

            UserRecord user = FindUser(username);
            return user != null && VerifyPassword(password, user.Salt, user.Hash);
        }

        public static bool UserExists(string username)
        {
            if (string.Equals(username.Trim(), AdminUsername, StringComparison.OrdinalIgnoreCase))
                return true;

            return FindUser(username) != null;
        }

        public static bool AddUser(string username, string password)
        {
            return AddUser(username, password, "Employee");
        }

        public static bool AddUser(string username, string password, string role)
        {
            username = username.Trim();
            role = NormalizeRole(role);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || UserExists(username))
                return false;

            string salt = CreateSalt();
            SqlDatabaseService.ExecuteNonQuery(
                @"INSERT INTO Users (Username, Salt, PasswordHash, UserRole, IsActive)
                  VALUES (@username, @salt, @hash, @role, 1)",
                new SQLiteParameter("@username", username),
                new SQLiteParameter("@salt", salt),
                new SQLiteParameter("@hash", HashPassword(password, salt)),
                new SQLiteParameter("@role", role)
            );
            return true;
        }

        public static bool DeleteUser(string username)
        {
            username = username.Trim();

            if (string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!SqlUserExists(username))
                return false;

            SqlDatabaseService.ExecuteNonQuery(
                "DELETE FROM Users WHERE LOWER(Username) = LOWER(@username)",
                new SQLiteParameter("@username", username)
            );

            return true;
        }

        public static List<UserInfo> GetUsers()
        {
            ImportLegacyUsers();

            List<UserInfo> users = new List<UserInfo>
            {
                new UserInfo { Username = AdminUsername, Role = "Admin" }
            };

            DataTable table = SqlDatabaseService.ExecuteTable(
                "SELECT Username, UserRole FROM Users WHERE IsActive = 1 ORDER BY Username"
            );

            foreach (DataRow row in table.Rows)
            {
                users.Add(new UserInfo
                {
                    Username = row["Username"].ToString(),
                    Role = row["UserRole"].ToString()
                });
            }

            return users;
        }

        public static List<string> GetUsernames()
        {
            return GetUsers().Select(u => u.Username).ToList();
        }

        public static string GetRole(string username)
        {
            if (string.Equals((username ?? "").Trim(), AdminUsername, StringComparison.OrdinalIgnoreCase))
                return "Admin";

            UserRecord user = FindUser(username);
            return user == null ? "Employee" : user.Role;
        }

        private static UserRecord FindUser(string username)
        {
            ImportLegacyUsers();

            DataTable table = SqlDatabaseService.ExecuteTable(
                @"SELECT Username, Salt, PasswordHash, UserRole
                  FROM Users
                  WHERE IsActive = 1 AND LOWER(Username) = LOWER(@username)
                  LIMIT 1",
                new SQLiteParameter("@username", (username ?? "").Trim())
            );

            if (table.Rows.Count == 0)
                return null;

            DataRow row = table.Rows[0];
            return new UserRecord
            {
                Username = row["Username"].ToString(),
                Salt = row["Salt"].ToString(),
                Hash = row["PasswordHash"].ToString(),
                Role = row["UserRole"].ToString()
            };
        }

        private static void ImportLegacyUsers()
        {
            if (SqlDatabaseService.GetSetting("LegacyUsersImported", "0") == "1")
                return;

            EnsureStore();

            foreach (string line in File.ReadAllLines(StorePath, Encoding.UTF8))
            {
                string[] parts = line.Split('|');

                if (parts.Length != 3 || SqlUserExists(parts[0]))
                    continue;

                SqlDatabaseService.ExecuteNonQuery(
                    @"INSERT INTO Users (Username, Salt, PasswordHash, UserRole, IsActive)
                      VALUES (@username, @salt, @hash, 'Employee', 1)",
                    new SQLiteParameter("@username", parts[0]),
                    new SQLiteParameter("@salt", parts[1]),
                    new SQLiteParameter("@hash", parts[2])
                );
            }

            SqlDatabaseService.SetSetting("LegacyUsersImported", "1");
        }

        private static bool SqlUserExists(string username)
        {
            object value = SqlDatabaseService.ExecuteScalar(
                "SELECT COUNT(1) FROM Users WHERE LOWER(Username) = LOWER(@username)",
                new SQLiteParameter("@username", (username ?? "").Trim())
            );

            return Convert.ToInt32(value) > 0;
        }

        private static string CreateSalt()
        {
            byte[] bytes = new byte[16];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(salt + password);
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }

        private static bool VerifyPassword(string password, string salt, string hash)
        {
            return HashPassword(password, salt) == hash;
        }

        private static void EnsureStore()
        {
            string directory = Path.GetDirectoryName(StorePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(StorePath))
                File.WriteAllText(StorePath, "", Encoding.UTF8);
        }

        private static string NormalizeRole(string role)
        {
            string value = (role ?? "").Trim();
            return string.Equals(value, "Viewer", StringComparison.OrdinalIgnoreCase) ? "Viewer" : "Employee";
        }

        public class UserInfo
        {
            public string Username { get; set; }
            public string Role { get; set; }

            public override string ToString()
            {
                return Username + " (" + Role + ")";
            }
        }

        private class UserRecord
        {
            public string Username { get; set; }
            public string Salt { get; set; }
            public string Hash { get; set; }
            public string Role { get; set; }
        }
    }
}
