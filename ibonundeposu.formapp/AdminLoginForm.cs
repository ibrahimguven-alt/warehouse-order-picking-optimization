using System.Drawing;
using System.Windows.Forms;

namespace ibonundeposu.formapp
{
    internal class AdminLoginForm : Form
    {
        private readonly Label lblTitle = new Label();
        private readonly Label lblUsername = new Label();
        private readonly Label lblPassword = new Label();
        private readonly TextBox txtUsername = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly Button btnLogin = new Button();

        public AdminLoginForm()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void InitializeComponent()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(360, 230);
            BackColor = Color.FromArgb(240, 243, 247);
            Font = new Font("Segoe UI", 9F);

            lblTitle.Location = new Point(30, 24);
            lblTitle.Size = new Size(300, 30);
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 45, 61);

            lblUsername.Location = new Point(34, 75);
            lblUsername.Size = new Size(100, 22);
            txtUsername.Location = new Point(145, 72);
            txtUsername.Size = new Size(170, 24);

            lblPassword.Location = new Point(34, 115);
            lblPassword.Size = new Size(100, 22);
            txtPassword.Location = new Point(145, 112);
            txtPassword.Size = new Size(170, 24);
            txtPassword.UseSystemPasswordChar = true;

            btnLogin.Location = new Point(145, 158);
            btnLogin.Size = new Size(100, 32);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.BackColor = Color.FromArgb(25, 118, 210);
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnLogin.Click += (s, e) => Login();

            Controls.AddRange(new Control[]
            {
                lblTitle,
                lblUsername,
                txtUsername,
                lblPassword,
                txtPassword,
                btnLogin
            });

            AcceptButton = btnLogin;
        }

        private void ApplyLanguage()
        {
            Text = AppText.T("adminLoginTitle");
            lblTitle.Text = AppText.T("adminLoginTitle");
            lblUsername.Text = AppText.T("username");
            lblPassword.Text = AppText.T("password");
            btnLogin.Text = AppText.T("login");
        }

        private void Login()
        {
            if (!AuthService.ValidateAdmin(txtUsername.Text, txtPassword.Text))
            {
                MessageBox.Show(AppText.T("invalidLogin"), AppText.T("adminLogin"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();

            using (AdminForm adminForm = new AdminForm())
                adminForm.ShowDialog(this);

            Close();
        }
    }
}
