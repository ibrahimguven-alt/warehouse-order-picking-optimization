using System;
using System.Drawing;
using System.Windows.Forms;

namespace ibonundeposu.formapp
{
    internal class LoginForm : Form
    {
        private readonly Label lblTitle = new Label();
        private readonly Label lblLanguage = new Label();
        private readonly ComboBox cmbLanguage = new ComboBox();
        private readonly Label lblUsername = new Label();
        private readonly Label lblPassword = new Label();
        private readonly TextBox txtUsername = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly Button btnLogin = new Button();
        private readonly Button btnAdmin = new Button();

        public LoginForm()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void InitializeComponent()
        {
            Text = AppText.T("appLoginTitle");
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(430, 310);
            BackColor = Color.FromArgb(240, 243, 247);
            Font = new Font("Segoe UI", 9F);

            lblTitle.Location = new Point(36, 24);
            lblTitle.Size = new Size(360, 34);
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 45, 61);

            lblLanguage.Location = new Point(38, 72);
            lblLanguage.Size = new Size(90, 22);

            cmbLanguage.Location = new Point(150, 69);
            cmbLanguage.Size = new Size(190, 24);
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Items.AddRange(new object[]
            {
                "English",
                "\u4e2d\u6587",
                "Turkce",
                "Espanol",
                "\u10e5\u10d0\u10e0\u10d7\u10e3\u10da\u10d8"
            });
            cmbLanguage.SelectedIndex = 0;
            cmbLanguage.SelectedIndexChanged += (s, e) =>
            {
                string[] languages = { "en", "zh", "tr", "es", "ka" };
                AppText.CurrentLanguage = languages[cmbLanguage.SelectedIndex];
                ApplyLanguage();
            };

            lblUsername.Location = new Point(38, 115);
            lblUsername.Size = new Size(100, 22);
            txtUsername.Location = new Point(150, 112);
            txtUsername.Size = new Size(190, 24);

            lblPassword.Location = new Point(38, 155);
            lblPassword.Size = new Size(100, 22);
            txtPassword.Location = new Point(150, 152);
            txtPassword.Size = new Size(190, 24);
            txtPassword.UseSystemPasswordChar = true;

            btnLogin.Location = new Point(150, 198);
            btnLogin.Size = new Size(90, 32);
            btnLogin.Click += (s, e) => Login();

            btnAdmin.Location = new Point(250, 198);
            btnAdmin.Size = new Size(120, 32);
            btnAdmin.Click += (s, e) =>
            {
                using (AdminLoginForm adminLogin = new AdminLoginForm())
                    adminLogin.ShowDialog(this);
            };

            StyleButton(btnLogin, Color.FromArgb(25, 118, 210));
            StyleButton(btnAdmin, Color.FromArgb(31, 45, 61));

            Controls.AddRange(new Control[]
            {
                lblTitle,
                lblLanguage,
                cmbLanguage,
                lblUsername,
                txtUsername,
                lblPassword,
                txtPassword,
                btnLogin,
                btnAdmin
            });

            AcceptButton = btnLogin;
        }

        private void ApplyLanguage()
        {
            Text = AppText.T("appLoginTitle");
            lblTitle.Text = AppText.T("appLoginTitle");
            lblLanguage.Text = AppText.T("language");
            lblUsername.Text = AppText.T("username");
            lblPassword.Text = AppText.T("password");
            btnLogin.Text = AppText.T("login");
            btnAdmin.Text = AppText.T("adminLogin");
        }

        private void Login()
        {
            if (!AuthService.ValidateUser(txtUsername.Text, txtPassword.Text))
            {
                MessageBox.Show(AppText.T("invalidLogin"), AppText.T("login"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();

            using (Form1 main = new Form1(txtUsername.Text.Trim()))
                main.ShowDialog(this);

            Close();
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
