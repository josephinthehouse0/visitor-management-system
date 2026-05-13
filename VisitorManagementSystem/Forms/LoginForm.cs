using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class LoginForm : Form
{
    private readonly AuthService _authService;
    private readonly AppState _appState;
    private readonly TextBox _username = ModernTheme.TextBox();
    private readonly TextBox _password = ModernTheme.TextBox();

    public LoginForm(AuthService authService, AppState appState)
    {
        _authService = authService;
        _appState = appState;
        Text = "VisitorManagementSystem - Login";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(920, 560);
        BackColor = ModernTheme.Background;
        BuildUi();
    }

    private void BuildUi()
    {
        var hero = new Panel { Dock = DockStyle.Left, Width = 380, BackColor = ModernTheme.Sidebar, Padding = new Padding(36) };
        hero.Controls.Add(new Label
        {
            Text = "Visitor\nManagement\nSystem",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(36, 90)
        });
        hero.Controls.Add(new Label
        {
            Text = "University reception and security desk dashboard",
            ForeColor = Color.FromArgb(191, 219, 254),
            Font = new Font("Segoe UI", 11),
            AutoSize = false,
            Size = new Size(280, 70),
            Location = new Point(40, 300)
        });

        var card = ModernTheme.Card();
        card.Size = new Size(380, 330);
        card.Location = new Point(470, 95);
        card.Anchor = AnchorStyles.None;

        var title = ModernTheme.Label("Employee Login", 20, true);
        title.Location = new Point(26, 22);
        card.Controls.Add(title);

        var usernameLabel = ModernTheme.Label("Username");
        usernameLabel.Location = new Point(28, 82);
        card.Controls.Add(usernameLabel);
        _username.Location = new Point(28, 110);
        _username.Width = 310;
        _username.Text = "admin";
        card.Controls.Add(_username);

        var passwordLabel = ModernTheme.Label("Password");
        passwordLabel.Location = new Point(28, 150);
        card.Controls.Add(passwordLabel);
        _password.Location = new Point(28, 178);
        _password.Width = 310;
        _password.UseSystemPasswordChar = true;
        _password.Text = "Admin123!";
        card.Controls.Add(_password);

        var loginButton = ModernTheme.PrimaryButton("Login");
        loginButton.Location = new Point(28, 238);
        loginButton.Width = 310;
        loginButton.Click += async (_, _) => await LoginAsync();
        card.Controls.Add(loginButton);

        AcceptButton = loginButton;
        Controls.Add(card);
        Controls.Add(hero);
    }

    private async Task LoginAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_password.Text))
            {
                MessageBox.Show("Username and password are required.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var employee = await _authService.LoginAsync(_username.Text.Trim(), _password.Text);
            if (employee is null)
            {
                MessageBox.Show("Invalid username or password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _appState.CurrentEmployee = employee;
            _appState.Log($"Login: {employee.Username}");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }
}
