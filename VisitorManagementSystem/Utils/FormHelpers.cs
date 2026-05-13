namespace VisitorManagementSystem.Utils;

public static class FormHelpers
{
    public static void ShowError(Exception ex)
    {
        MessageBox.Show(
            "Operation failed. If this is an Azure SQL operation, check the connection string, password, and firewall rules.\n\n" + ex.Message,
            "Visitor Management System",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    public static TableLayoutPanel FormTable()
    {
        return new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
    }

    public static string? PromptForReason(IWin32Window owner, string title, string visitorName)
    {
        using var form = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(440, 250),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ModernTheme.Background
        };

        var label = new Label
        {
            Text = $"Reason for blacklisting {visitorName}:",
            AutoSize = false,
            Size = new Size(380, 36),
            Location = new Point(22, 22),
            Font = ModernTheme.BodyFont,
            ForeColor = ModernTheme.Text
        };
        var reason = new TextBox
        {
            Multiline = true,
            Size = new Size(380, 82),
            Location = new Point(22, 64),
            Font = ModernTheme.BodyFont
        };
        var ok = ModernTheme.DangerButton("Add to Blacklist");
        ok.Location = new Point(198, 160);
        ok.Width = 140;
        ok.DialogResult = DialogResult.OK;
        var cancel = ModernTheme.SecondaryButton("Cancel");
        cancel.Location = new Point(86, 160);
        cancel.Width = 96;
        cancel.DialogResult = DialogResult.Cancel;

        form.Controls.AddRange(new Control[] { label, reason, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        if (form.ShowDialog(owner) != DialogResult.OK)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(reason.Text) ? null : reason.Text.Trim();
    }
}
