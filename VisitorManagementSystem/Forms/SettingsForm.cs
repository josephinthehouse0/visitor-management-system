using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class SettingsForm : Form
{
    public SettingsForm(AppState appState)
    {
        Text = "Settings";
        BackColor = ModernTheme.Background;
        var card = ModernTheme.Card();
        card.Dock = DockStyle.Top;
        card.Height = 260;
        card.Controls.Add(new Label { Text = "Settings", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, Location = new Point(18, 18), AutoSize = true });
        card.Controls.Add(new Label
        {
            Text = "Current employee: " + (appState.CurrentEmployee?.FullName ?? "Unknown") + Environment.NewLine +
                   "Database: Azure SQL configured through appsettings.json" + Environment.NewLine +
                   "Photos: VisitorPhotos folder" + Environment.NewLine +
                   "Webcam: TODO Take Photo integration; Upload Photo is implemented.",
            Font = ModernTheme.BodyFont,
            ForeColor = ModernTheme.Text,
            Location = new Point(22, 78),
            AutoSize = true
        });
        Controls.Add(card);
    }
}
