using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class BlacklistForm : Form
{
    private readonly VisitorService _visitorService;
    private readonly BlacklistService _blacklistService;
    private readonly DataMaskingService _maskingService;
    private readonly TextBox _identity = ModernTheme.TextBox();
    private readonly TextBox _reason = ModernTheme.TextBox();
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly DataGridView _grid = ModernTheme.Grid();

    public BlacklistForm(VisitorService visitorService, BlacklistService blacklistService, DataMaskingService maskingService)
    {
        _visitorService = visitorService;
        _blacklistService = blacklistService;
        _maskingService = maskingService;
        Text = "Blacklist";
        BackColor = ModernTheme.Background;
        BuildUi();
        Load += async (_, _) => await LoadEntriesAsync();
    }

    private void BuildUi()
    {
        var top = ModernTheme.Card();
        top.Dock = DockStyle.Top;
        top.Height = 170;
        top.Controls.Add(new Label { Text = "Blacklist Management", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, Location = new Point(18, 14), AutoSize = true });
        AddField(top, "Visitor Identity Number", _identity, 64, 18);
        AddField(top, "Reason", _reason, 64, 260);
        _reason.Width = 340;
        var add = ModernTheme.DangerButton("Add to Blacklist");
        add.Location = new Point(620, 88);
        add.Width = 160;
        add.Click += async (_, _) => await AddAsync();
        top.Controls.Add(add);
        Controls.Add(top);

        var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        _search.PlaceholderText = "Search blacklist";
        _search.Dock = DockStyle.Top;
        _search.TextChanged += async (_, _) => await LoadEntriesAsync();
        gridPanel.Controls.Add(_search);
        gridPanel.Controls.Add(_grid);
        Controls.Add(gridPanel);
        Controls.SetChildIndex(gridPanel, 0);
        _grid.CellDoubleClick += async (_, _) => await RemoveSelectedAsync();
    }

    private static void AddField(Control parent, string label, TextBox box, int top, int left)
    {
        var lbl = ModernTheme.Label(label);
        lbl.Location = new Point(left, top);
        box.Location = new Point(left, top + 24);
        box.Width = 220;
        parent.Controls.Add(lbl);
        parent.Controls.Add(box);
    }

    private async Task LoadEntriesAsync()
    {
        try
        {
            var entries = await _blacklistService.GetEntriesAsync(_search.Text);
            _grid.DataSource = entries.Select(b => new
            {
                b.BlacklistEntryId,
                Visitor = b.Visitor.FullName,
                IdentityNumber = _maskingService.MaskIdentityNumber(b.Visitor.IdentityNumber),
                b.Reason,
                b.AddedAt,
                Status = b.IsActive ? "Active" : "Removed"
            }).ToList();
            _grid.Columns["BlacklistEntryId"].Visible = false;
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task AddAsync()
    {
        try
        {
            var visitor = await _visitorService.FindByIdentityAsync(_identity.Text.Trim());
            if (visitor is null)
            {
                MessageBox.Show("Visitor must be registered before adding to blacklist.");
                return;
            }

            var result = await _blacklistService.AddAsync(visitor, _reason.Text);
            MessageBox.Show(result.Message);
            await LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task RemoveSelectedAsync()
    {
        if (_grid.CurrentRow is null) return;
        try
        {
            var id = (int)_grid.CurrentRow.Cells["BlacklistEntryId"].Value;
            await _blacklistService.RemoveAsync(id);
            await LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }
}
