using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace ibonundeposu.formapp
{
    internal class AdminForm : Form
    {
        private readonly Label lblTitle = new Label();
        private readonly Label lblUsers = new Label();
        private readonly Label lblStats = new Label();
        private readonly Label lblMonthlyUsage = new Label();
        private readonly Label lblEmployeesOfMonth = new Label();
        private readonly Label lblVehicleSettings = new Label();
        private readonly Label lblHistory = new Label();
        private readonly Label lblFuelPrice = new Label();
        private readonly Label lblForkliftLiters = new Label();
        private readonly Label lblRobotCharge = new Label();
        private readonly ListBox lstUsers = new ListBox();
        private readonly ListBox lstStats = new ListBox();
        private readonly ListBox lstEmployeesOfMonth = new ListBox();
        private readonly DataGridView gridHistory = new DataGridView();
        private readonly Panel pnlMonthlyUsage = new Panel();
        private readonly Label lblUsername = new Label();
        private readonly Label lblPassword = new Label();
        private readonly Label lblRepeat = new Label();
        private readonly Label lblRole = new Label();
        private readonly ComboBox cmbRole = new ComboBox();
        private readonly TextBox txtUsername = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtRepeatPassword = new TextBox();
        private readonly TextBox txtFuelPrice = new TextBox();
        private readonly TextBox txtForkliftLiters = new TextBox();
        private readonly TextBox txtRobotCharge = new TextBox();
        private readonly Button btnAdd = new Button();
        private readonly Button btnDelete = new Button();
        private readonly Button btnSaveVehicleSettings = new Button();
        private readonly Button btnExportCsv = new Button();
        private readonly Button btnPrintPdf = new Button();
        private readonly Button btnClose = new Button();

        public AdminForm()
        {
            InitializeComponent();
            ApplyLanguage();
            RefreshUsers();
            RefreshStats();
        }

        private void InitializeComponent()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(1060, 700);
            BackColor = Color.FromArgb(240, 243, 247);
            Font = new Font("Segoe UI", 9F);

            lblTitle.Location = new Point(30, 24);
            lblTitle.Size = new Size(1000, 30);
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 45, 61);

            lblUsers.Location = new Point(34, 75);
            lblUsers.Size = new Size(220, 22);
            lstUsers.Location = new Point(34, 100);
            lstUsers.Size = new Size(250, 120);

            lblStats.Location = new Point(550, 75);
            lblStats.Size = new Size(480, 22);
            lstStats.Location = new Point(550, 100);
            lstStats.Size = new Size(500, 120);

            lblMonthlyUsage.Location = new Point(34, 300);
            lblMonthlyUsage.Size = new Size(250, 22);
            pnlMonthlyUsage.Location = new Point(34, 325);
            pnlMonthlyUsage.Size = new Size(360, 82);
            pnlMonthlyUsage.BackColor = Color.White;
            pnlMonthlyUsage.BorderStyle = BorderStyle.FixedSingle;
            pnlMonthlyUsage.Paint += (s, e) => DrawMonthlyUsage(e.Graphics);

            lblEmployeesOfMonth.Location = new Point(420, 300);
            lblEmployeesOfMonth.Size = new Size(260, 22);
            lstEmployeesOfMonth.Location = new Point(420, 325);
            lstEmployeesOfMonth.Size = new Size(630, 82);

            lblHistory.Location = new Point(34, 425);
            lblHistory.Size = new Size(400, 22);
            lblHistory.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblHistory.ForeColor = Color.FromArgb(31, 45, 61);

            gridHistory.Location = new Point(34, 450);
            gridHistory.Size = new Size(1020, 150);
            gridHistory.ReadOnly = true;
            gridHistory.AllowUserToAddRows = false;
            gridHistory.AllowUserToDeleteRows = false;
            gridHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridHistory.BackgroundColor = Color.White;
            gridHistory.BorderStyle = BorderStyle.FixedSingle;

            lblUsername.Location = new Point(320, 95);
            lblUsername.Size = new Size(130, 22);
            txtUsername.Location = new Point(455, 92);
            txtUsername.Size = new Size(125, 24);

            lblPassword.Location = new Point(320, 132);
            lblPassword.Size = new Size(130, 22);
            txtPassword.Location = new Point(455, 129);
            txtPassword.Size = new Size(125, 24);
            txtPassword.UseSystemPasswordChar = true;

            lblRepeat.Location = new Point(320, 169);
            lblRepeat.Size = new Size(130, 22);
            txtRepeatPassword.Location = new Point(455, 166);
            txtRepeatPassword.Size = new Size(125, 24);
            txtRepeatPassword.UseSystemPasswordChar = true;

            lblRole.Location = new Point(320, 206);
            lblRole.Size = new Size(130, 22);
            cmbRole.Location = new Point(455, 203);
            cmbRole.Size = new Size(125, 24);
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            btnAdd.Location = new Point(320, 254);
            btnAdd.Size = new Size(125, 30);
            btnAdd.Click += (s, e) => AddUser();

            btnDelete.Location = new Point(455, 254);
            btnDelete.Size = new Size(125, 30);
            btnDelete.Click += (s, e) => DeleteUser();

            lblVehicleSettings.Location = new Point(34, 620);
            lblVehicleSettings.Size = new Size(150, 22);

            lblFuelPrice.Location = new Point(190, 620);
            lblFuelPrice.Size = new Size(90, 22);
            txtFuelPrice.Location = new Point(190, 645);
            txtFuelPrice.Size = new Size(90, 24);
            lblForkliftLiters.Location = new Point(295, 620);
            lblForkliftLiters.Size = new Size(100, 22);
            txtForkliftLiters.Location = new Point(295, 645);
            txtForkliftLiters.Size = new Size(90, 24);
            lblRobotCharge.Location = new Point(400, 620);
            lblRobotCharge.Size = new Size(100, 22);
            txtRobotCharge.Location = new Point(400, 645);
            txtRobotCharge.Size = new Size(90, 24);

            btnSaveVehicleSettings.Location = new Point(510, 642);
            btnSaveVehicleSettings.Size = new Size(145, 32);
            btnSaveVehicleSettings.Click += (s, e) => SaveVehicleSettings();

            btnExportCsv.Location = new Point(670, 642);
            btnExportCsv.Size = new Size(130, 32);
            btnExportCsv.Click += (s, e) => ExportCsv();

            btnPrintPdf.Location = new Point(815, 642);
            btnPrintPdf.Size = new Size(130, 32);
            btnPrintPdf.Click += (s, e) => PrintReport();

            btnClose.Location = new Point(954, 642);
            btnClose.Size = new Size(100, 32);
            btnClose.Click += (s, e) => Close();

            StyleButton(btnAdd, Color.FromArgb(25, 118, 210));
            StyleButton(btnDelete, Color.FromArgb(198, 40, 40));
            StyleButton(btnSaveVehicleSettings, Color.FromArgb(25, 118, 210));
            StyleButton(btnExportCsv, Color.FromArgb(46, 125, 50));
            StyleButton(btnPrintPdf, Color.FromArgb(31, 45, 61));
            StyleButton(btnClose, Color.FromArgb(31, 45, 61));

            Controls.AddRange(new Control[]
            {
                lblTitle,
                lblUsers,
                lstUsers,
                lblStats,
                lstStats,
                lblHistory,
                gridHistory,
                lblMonthlyUsage,
                pnlMonthlyUsage,
                lblEmployeesOfMonth,
                lstEmployeesOfMonth,
                lblVehicleSettings,
                lblFuelPrice,
                txtFuelPrice,
                lblForkliftLiters,
                txtForkliftLiters,
                lblRobotCharge,
                txtRobotCharge,
                lblUsername,
                txtUsername,
                lblPassword,
                txtPassword,
                lblRepeat,
                txtRepeatPassword,
                lblRole,
                cmbRole,
                btnAdd,
                btnDelete,
                btnSaveVehicleSettings,
                btnExportCsv,
                btnPrintPdf,
                btnClose
            });
        }

        private void ApplyLanguage()
        {
            Text = AppText.T("adminPanelTitle");
            lblTitle.Text = AppText.T("adminPanelTitle");
            lblUsers.Text = AppText.T("users");
            lblStats.Text = AppText.T("orderStats");
            lblMonthlyUsage.Text = AppText.T("monthlyUsage");
            lblEmployeesOfMonth.Text = AppText.T("employeesOfMonth");
            lblVehicleSettings.Text = AppText.T("vehicleSettings");
            lblHistory.Text = AppText.T("history");
            lblFuelPrice.Text = AppText.T("fuelPrice");
            lblForkliftLiters.Text = AppText.T("forkliftUsage");
            lblRobotCharge.Text = AppText.T("robotCharge");
            lblUsername.Text = AppText.T("newUsername");
            lblPassword.Text = AppText.T("newPassword");
            lblRepeat.Text = AppText.T("repeatPassword");
            lblRole.Text = AppText.T("role");
            int selectedRole = cmbRole.SelectedIndex < 0 ? 0 : cmbRole.SelectedIndex;
            cmbRole.Items.Clear();
            cmbRole.Items.AddRange(new object[] { AppText.T("employee"), AppText.T("viewer") });
            cmbRole.SelectedIndex = selectedRole >= 0 && selectedRole < cmbRole.Items.Count ? selectedRole : 0;
            btnAdd.Text = AppText.T("addUser");
            btnDelete.Text = AppText.T("deleteUser");
            btnSaveVehicleSettings.Text = AppText.T("saveSettings");
            btnExportCsv.Text = AppText.T("exportCsv");
            btnPrintPdf.Text = AppText.T("printPdf");
            btnClose.Text = AppText.T("close");
        }

        private void AddUser()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtRepeatPassword.Text))
            {
                MessageBox.Show(AppText.T("emptyFields"));
                return;
            }

            if (txtPassword.Text != txtRepeatPassword.Text)
            {
                MessageBox.Show(AppText.T("passwordMismatch"));
                return;
            }

            if (AuthService.UserExists(txtUsername.Text))
            {
                MessageBox.Show(AppText.T("userExists"));
                return;
            }

            string role = cmbRole.SelectedIndex == 1 ? "Viewer" : "Employee";
            AuthService.AddUser(txtUsername.Text, txtPassword.Text, role);
            txtUsername.Clear();
            txtPassword.Clear();
            txtRepeatPassword.Clear();
            RefreshUsers();
            RefreshStats();
            MessageBox.Show(AppText.T("userAdded"));
        }

        private void DeleteUser()
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show(AppText.T("selectUser"));
                return;
            }

            AuthService.UserInfo selectedUser = lstUsers.SelectedItem as AuthService.UserInfo;
            string username = selectedUser == null ? lstUsers.SelectedItem.ToString() : selectedUser.Username;

            if (username == "admin")
            {
                MessageBox.Show(AppText.T("adminCannotDelete"));
                return;
            }

            DialogResult result = MessageBox.Show(
                AppText.T("confirmDelete"),
                AppText.T("deleteUser"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            AuthService.DeleteUser(username);
            RefreshUsers();
            RefreshStats();
            MessageBox.Show(AppText.T("userDeleted"));
        }

        private void RefreshUsers()
        {
            lstUsers.Items.Clear();

            foreach (AuthService.UserInfo user in AuthService.GetUsers())
                lstUsers.Items.Add(user);

            LoadVehicleSettings();
        }

        private void RefreshStats()
        {
            lstStats.Items.Clear();

            foreach (string line in StatsService.SummaryLines())
                lstStats.Items.Add(line);

            gridHistory.DataSource = StatsService.Load()
                .Select(s => new
                {
                    Date = s.Date,
                    User = s.Username,
                    Role = AuthService.GetUsers().FirstOrDefault(u => u.Username == s.Username)?.Role ?? "Admin",
                    Method = s.Method,
                    Orders = s.OrderCount,
                    Vehicle = s.Vehicle,
                    Distance = s.Distance,
                    FuelL = StatsService.FormatDecimal(s.GasLiters),
                    FuelTL = StatsService.FormatDecimal(s.GasCost),
                    ElectricTL = StatsService.FormatDecimal(s.ElectricCost),
                    TotalTL = StatsService.FormatDecimal(s.TotalCost)
                })
                .ToList();
            ApplyHistoryHeaders();

            lstEmployeesOfMonth.Items.Clear();
            List<string> employees = StatsService.EmployeeOfMonthLines();
            if (employees.Count == 0)
                lstEmployeesOfMonth.Items.Add(AppText.T("noStats"));
            else
            {
                foreach (string line in employees)
                    lstEmployeesOfMonth.Items.Add(line);
            }

            pnlMonthlyUsage.Invalidate();
        }

        private void ApplyHistoryHeaders()
        {
            SetColumnHeader("Date", AppText.T("colDate"));
            SetColumnHeader("User", AppText.T("colUser"));
            SetColumnHeader("Role", AppText.T("colRole"));
            SetColumnHeader("Method", AppText.T("colMethod"));
            SetColumnHeader("Orders", AppText.T("colOrders"));
            SetColumnHeader("Vehicle", AppText.T("colVehicle"));
            SetColumnHeader("Distance", AppText.T("colDistance"));
            SetColumnHeader("FuelL", AppText.T("colFuelL"));
            SetColumnHeader("FuelTL", AppText.T("colFuelTL"));
            SetColumnHeader("ElectricTL", AppText.T("colElectricTL"));
            SetColumnHeader("TotalTL", AppText.T("colTotalTL"));
        }

        private void SetColumnHeader(string columnName, string headerText)
        {
            if (gridHistory.Columns.Contains(columnName))
                gridHistory.Columns[columnName].HeaderText = headerText;
        }

        private void DrawMonthlyUsage(Graphics graphics)
        {
            graphics.Clear(Color.White);
            MonthlyUsage usage = StatsService.GetCurrentMonthUsage();

            Rectangle fuelBar = new Rectangle(95, 25, GetBarWidth(usage.GasCost, usage.TotalCost), 18);
            Rectangle electricBar = new Rectangle(95, 58, GetBarWidth(usage.ElectricCost, usage.TotalCost), 18);

            using (Brush textBrush = new SolidBrush(Color.FromArgb(31, 45, 61)))
            using (Brush fuelBrush = new SolidBrush(Color.FromArgb(198, 40, 40)))
            using (Brush electricBrush = new SolidBrush(Color.FromArgb(25, 118, 210)))
            using (Pen borderPen = new Pen(Color.FromArgb(210, 216, 224)))
            {
                graphics.DrawString(AppText.T("fuel"), Font, textBrush, 12, 24);
                graphics.DrawString(AppText.T("electric"), Font, textBrush, 12, 57);

                graphics.DrawRectangle(borderPen, 95, 25, 160, 18);
                graphics.DrawRectangle(borderPen, 95, 58, 160, 18);
                graphics.FillRectangle(fuelBrush, fuelBar);
                graphics.FillRectangle(electricBrush, electricBar);

                graphics.DrawString(
                    StatsService.FormatDecimal(usage.GasLiters) + " L / " + StatsService.FormatMoney(usage.GasCost),
                    Font,
                    textBrush,
                    265,
                    24);
                graphics.DrawString(
                    StatsService.FormatMoney(usage.ElectricCost),
                    Font,
                    textBrush,
                    265,
                    57);
            }
        }

        private int GetBarWidth(decimal value, decimal total)
        {
            if (total <= 0m || value <= 0m)
                return 0;

            return Math.Max(2, Math.Min(160, (int)Math.Round((value / total) * 160m)));
        }

        private void LoadVehicleSettings()
        {
            VehicleSettings forklift = StatsService.GetVehicleSettings("Forklift");
            VehicleSettings robot = StatsService.GetVehicleSettings("Robot");
            txtFuelPrice.Text = StatsService.FormatDecimal(forklift.FuelPricePerLiter);
            txtForkliftLiters.Text = StatsService.FormatDecimal(forklift.LitersPerHour);
            txtRobotCharge.Text = StatsService.FormatDecimal(robot.ChargeCostPerHour);
        }

        private void SaveVehicleSettings()
        {
            if (!decimal.TryParse(txtFuelPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal fuelPrice) ||
                !decimal.TryParse(txtForkliftLiters.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal forkliftLiters) ||
                !decimal.TryParse(txtRobotCharge.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal robotCharge))
            {
                MessageBox.Show(AppText.T("validNumericSettings"));
                return;
            }

            StatsService.SaveVehicleSettings(
                new VehicleSettings { FuelPricePerLiter = fuelPrice, LitersPerHour = forkliftLiters, OrdersPerHour = 60m },
                new VehicleSettings { ChargeCostPerHour = robotCharge, OrdersPerHour = 60m }
            );
            RefreshStats();
            MessageBox.Show(AppText.T("settingsSaved"));
        }

        private void ExportCsv()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV file|*.csv",
                FileName = "warehouse-route-report-" + DateTime.Now.ToString("yyyy-MM") + ".csv"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            List<RouteStat> stats = StatsService.Load();
            List<string> lines = new List<string>
            {
                string.Join(",", new[]
                {
                    AppText.T("colDate"),
                    AppText.T("colUser"),
                    AppText.T("colMethod"),
                    AppText.T("colOrders"),
                    AppText.T("colVehicle"),
                    AppText.T("colDistance"),
                    AppText.T("colFuelL"),
                    AppText.T("colFuelTL"),
                    AppText.T("colElectricTL"),
                    AppText.T("colTotalTL")
                })
            };

            foreach (RouteStat stat in stats)
            {
                lines.Add(string.Join(",", new[]
                {
                    Csv(stat.Date),
                    Csv(stat.Username),
                    Csv(stat.Method),
                    stat.OrderCount.ToString(),
                    Csv(stat.Vehicle),
                    StatsService.FormatDecimal(stat.Distance),
                    StatsService.FormatDecimal(stat.GasLiters),
                    StatsService.FormatDecimal(stat.GasCost),
                    StatsService.FormatDecimal(stat.ElectricCost),
                    StatsService.FormatDecimal(stat.TotalCost)
                }));
            }

            File.WriteAllLines(dialog.FileName, lines);
            MessageBox.Show(AppText.T("reportExported"));
        }

        private string Csv(string value)
        {
            value = value ?? "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void PrintReport()
        {
            List<string> lines = StatsService.SummaryLines();
            if (lines.Count == 0)
                lines.Add(AppText.T("noStats"));

            PrintDocument document = new PrintDocument();
            document.DocumentName = AppText.T("reportTitle");
            document.PrintPage += (s, e) =>
            {
                float y = 40;
                e.Graphics.DrawString(AppText.T("reportTitle"), new Font("Segoe UI", 14F, FontStyle.Bold), Brushes.Black, 40, y);
                y += 35;

                foreach (string line in lines.Take(35))
                {
                    e.Graphics.DrawString(line, Font, Brushes.Black, 40, y);
                    y += 20;
                }
            };

            using (PrintDialog dialog = new PrintDialog { Document = document, UseEXDialog = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    document.Print();
            }
        }

        private void StyleButton(Button button, Color color)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        }
    }
}
