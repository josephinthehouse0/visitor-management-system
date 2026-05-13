namespace VisitorManagementSystem.Utils;

public static class ModernTheme
{
    public static readonly Color Background = Color.FromArgb(246, 248, 252);
    public static readonly Color Surface = Color.White;
    public static readonly Color Sidebar = Color.FromArgb(23, 37, 84);
    public static readonly Color SidebarActive = Color.FromArgb(37, 99, 235);
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color Accent = Color.FromArgb(20, 184, 166);
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);
    public static readonly Color Text = Color.FromArgb(15, 23, 42);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Font TitleFont = new("Segoe UI", 20, FontStyle.Bold);
    public static readonly Font HeadingFont = new("Segoe UI", 13, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 10);

    public static Button PrimaryButton(string text) => StyledButton(text, Primary);
    public static Button SecondaryButton(string text) => StyledButton(text, Color.FromArgb(71, 85, 105));
    public static Button DangerButton(string text) => StyledButton(text, Danger);

    public static Button StyledButton(string text, Color color)
    {
        var button = new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Height = 38,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = ControlPaint.Light(color, .15f);
        button.MouseLeave += (_, _) => button.BackColor = color;
        return button;
    }

    public static Label Label(string text, int size = 10, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = bold ? Text : Muted,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        Margin = new Padding(0, 6, 0, 4)
    };

    public static TextBox TextBox() => new()
    {
        Font = BodyFont,
        Height = 32,
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 0, 0, 8)
    };

    public static Panel Card()
    {
        return new Panel
        {
            BackColor = Surface,
            Padding = new Padding(18),
            Margin = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            BackgroundColor = Surface,
            BorderStyle = BorderStyle.None,
            Font = BodyFont,
            EnableHeadersVisualStyles = false,
            ColumnHeadersVisible = true,
            ColumnHeadersHeight = 38,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 232, 240);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Text;
        grid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
        grid.RowTemplate.Height = 34;
        return grid;
    }

    public static PictureBox PictureBox() => new()
    {
        Size = new Size(128, 128),
        BackColor = Color.FromArgb(226, 232, 240),
        SizeMode = PictureBoxSizeMode.Zoom,
        BorderStyle = BorderStyle.FixedSingle
    };
}
