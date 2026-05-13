using Microsoft.Extensions.DependencyInjection;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class MainDashboardForm : Form
{
    private readonly VisitService _visitService;
    private readonly BlacklistService _blacklistService;
    private readonly VisitorService _visitorService;
    private readonly ResidentService _residentService;
    private readonly DurationService _durationService;
    private readonly DataMaskingService _maskingService;
    private readonly AppState _appState;
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = ModernTheme.Background, Padding = new Padding(24) };

    public MainDashboardForm(
        VisitService visitService,
        BlacklistService blacklistService,
        VisitorService visitorService,
        ResidentService residentService,
        DurationService durationService,
        DataMaskingService maskingService,
        AppState appState)
    {
        _visitService = visitService;
        _blacklistService = blacklistService;
        _visitorService = visitorService;
        _residentService = residentService;
        _durationService = durationService;
        _maskingService = maskingService;
        _appState = appState;
        Text = "VisitorManagementSystem";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 720);
        BuildUi();
        Load += async (_, _) => await ShowDashboardAsync();
    }

    private void BuildUi()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = ModernTheme.Sidebar, Padding = new Padding(14) };
        var title = new Label
        {
            Text = "VMS",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 74,
            TextAlign = ContentAlignment.MiddleCenter
        };
        sidebar.Controls.Add(title);

        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 18, 0, 0) };
        sidebar.Controls.Add(nav);
        sidebar.Controls.SetChildIndex(nav, 0);

        AddNav(nav, "Dashboard", async () => await ShowDashboardAsync());
        AddNav(nav, "Visitors", () => OpenChild<VisitorsForm>());
        AddNav(nav, "Sign In Visitor", () => OpenChild<VisitorEntryForm>());
        AddNav(nav, "Active Visitors", () => OpenChild<ActiveVisitorsForm>());
        AddNav(nav, "Visit History", () => OpenChild<VisitHistoryForm>());
        AddNav(nav, "Residents", () => OpenChild<ResidentsForm>());
        AddNav(nav, "Blacklist", () => OpenChild<BlacklistForm>());
        AddNav(nav, "Settings", () => OpenChild<SettingsForm>());
        AddNav(nav, "Logout", Close);

        Controls.Add(_content);
        Controls.Add(sidebar);
    }

    private static void AddNav(FlowLayoutPanel nav, string text, Action action)
    {
        var button = ModernTheme.StyledButton(text, ModernTheme.Sidebar);
        button.Width = 190;
        button.Height = 44;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(16, 0, 0, 0);
        button.Margin = new Padding(4, 4, 4, 6);
        button.MouseEnter += (_, _) => button.BackColor = ModernTheme.SidebarActive;
        button.MouseLeave += (_, _) => button.BackColor = ModernTheme.Sidebar;
        button.Click += (_, _) => action();
        nav.Controls.Add(button);
    }

    private async Task ShowDashboardAsync()
    {
        _content.Controls.Clear();
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = ModernTheme.Background,
            Padding = new Padding(0),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _content.Controls.Add(root);

        var heading = new Label
        {
            Text = "Dashboard",
            Font = ModernTheme.TitleFont,
            ForeColor = ModernTheme.Text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(heading, 0, 0);
        root.SetColumnSpan(heading, 2);

        var cards = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        root.Controls.Add(cards, 0, 1);
        root.SetColumnSpan(cards, 2);

        try
        {
            var active = await _visitService.ActiveVisitsCountAsync();
            var today = await _visitService.TodayVisitsCountAsync();
            var blacklist = (await _blacklistService.GetEntriesAsync()).Count(e => e.IsActive);
            var totalVisitors = await _visitorService.TotalVisitorsCountAsync();
            var residents = await _residentService.TotalResidentsCountAsync();
            cards.Controls.Add(DashboardCard("Active Visitors", active.ToString(), "inside now", ModernTheme.Primary));
            cards.Controls.Add(DashboardCard("Today's Visits", today.ToString(), "today", ModernTheme.Accent));
            cards.Controls.Add(DashboardCard("Total Visitors", totalVisitors.ToString(), "registered", Color.FromArgb(99, 102, 241)));
            cards.Controls.Add(DashboardCard("Residents", residents.ToString(), "available", Color.FromArgb(14, 165, 233)));
            cards.Controls.Add(DashboardCard("Blacklist", blacklist.ToString(), "active", ModernTheme.Danger));

            var activePanel = DashboardPanel("Active visitors");
            var activeGrid = ModernTheme.Grid();
            ConfigureDashboardGrid(activeGrid, ("Visit No", "VisitNumber"), ("Visitor", "Visitor"), ("Identity", "IdentityNumber"), ("Resident", "Resident"), ("Duration", "Duration"));
            var activeVisits = await _visitService.GetActiveVisitsAsync();
            activeGrid.DataSource = activeVisits.Take(8).Select(v => new
            {
                VisitNumber = $"VISIT-{v.VisitId:000000}",
                Visitor = v.Visitor.FullName,
                IdentityNumber = _maskingService.MaskIdentityNumber(v.Visitor.IdentityNumber),
                Resident = v.Resident.FullName,
                Duration = _durationService.FormatDuration((int)(DateTime.Now - v.EntryTime).TotalMinutes)
            }).ToList();
            activePanel.Controls.Add(activeGrid);
            root.Controls.Add(activePanel, 0, 2);

            var side = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.Controls.Add(side, 1, 2);

            var historyPanel = DashboardPanel("Recent visit history");
            var historyGrid = ModernTheme.Grid();
            ConfigureDashboardGrid(historyGrid, ("Visitor", "Visitor"), ("Resident", "Resident"), ("Duration", "Duration"));
            var history = await _visitService.GetCompletedVisitsAsync();
            historyGrid.DataSource = history.Take(5).Select(v => new
            {
                Visitor = v.Visitor.FullName,
                Resident = v.Resident.FullName,
                Duration = _durationService.FormatDuration(v.DurationMinutes)
            }).ToList();
            historyPanel.Controls.Add(historyGrid);
            side.Controls.Add(historyPanel, 0, 0);

            var logPanel = DashboardPanel("Operation history");
            var list = new ListBox { Dock = DockStyle.Fill, Font = ModernTheme.BodyFont, BorderStyle = BorderStyle.None };
            var items = _appState.OperationHistory.Take(8).DefaultIfEmpty("No operations yet.").Cast<object>().ToArray();
            list.Items.AddRange(items);
            logPanel.Controls.Add(list);
            side.Controls.Add(logPanel, 0, 1);
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private static Panel DashboardCard(string title, string value, string caption, Color color)
    {
        var card = ModernTheme.Card();
        card.Size = new Size(190, 118);
        card.Controls.Add(new Label { Text = value, ForeColor = color, Font = new Font("Segoe UI", 24, FontStyle.Bold), Location = new Point(22, 10), AutoSize = true });
        card.Controls.Add(new Label { Text = title, ForeColor = ModernTheme.Text, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(22, 58), Size = new Size(145, 20), AutoEllipsis = true });
        card.Controls.Add(new Label { Text = caption, ForeColor = ModernTheme.Muted, Font = new Font("Segoe UI", 8), Location = new Point(22, 82), Size = new Size(145, 18), AutoEllipsis = true });
        return card;
    }

    private static Panel DashboardPanel(string title)
    {
        var panel = ModernTheme.Card();
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(18, 48, 18, 18);
        panel.Controls.Add(new Label
        {
            Text = title,
            Font = ModernTheme.HeadingFont,
            ForeColor = ModernTheme.Text,
            Location = new Point(18, 16),
            AutoSize = true
        });
        return panel;
    }

    private static void ConfigureDashboardGrid(DataGridView grid, params (string Header, string Property)[] columns)
    {
        grid.AutoGenerateColumns = false;
        grid.Columns.Clear();
        foreach (var column in columns)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = column.Header,
                DataPropertyName = column.Property
            });
        }
    }

    private void OpenChild<T>() where T : Form
    {
        _content.Controls.Clear();
        var form = Program.Services.GetRequiredService<T>();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        _content.Controls.Add(form);
        form.Show();
    }
}
