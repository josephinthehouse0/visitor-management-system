using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class ActiveVisitorsForm : Form
{
    private readonly VisitService _visitService;
    private readonly BlacklistService _blacklistService;
    private readonly DurationService _durationService;
    private readonly DataMaskingService _maskingService;
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly DataGridView _grid = ModernTheme.Grid();
    private List<Visit> _visits = new();

    public ActiveVisitorsForm(VisitService visitService, BlacklistService blacklistService, DurationService durationService, DataMaskingService maskingService)
    {
        _visitService = visitService;
        _blacklistService = blacklistService;
        _durationService = durationService;
        _maskingService = maskingService;
        Text = "Active Visitors";
        BackColor = ModernTheme.Background;
        BuildUi();
        ConfigureGrid();
        Load += async (_, _) => await LoadVisitsAsync();
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Active Visitors", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, AutoSize = true, Location = new Point(6, 10) });
        _search.Width = 300;
        _search.PlaceholderText = "Search by name, identity, resident";
        _search.Location = new Point(280, 16);
        _search.TextChanged += async (_, _) => await LoadVisitsAsync();
        top.Controls.Add(_search);
        var refresh = ModernTheme.SecondaryButton("Refresh");
        refresh.Location = new Point(600, 14);
        refresh.Width = 110;
        refresh.Click += async (_, _) => await LoadVisitsAsync();
        top.Controls.Add(refresh);
        var signOut = ModernTheme.PrimaryButton("Sign Out");
        signOut.Location = new Point(724, 14);
        signOut.Width = 110;
        signOut.Click += async (_, _) => await SignOutAsync();
        top.Controls.Add(signOut);
        var blacklist = ModernTheme.DangerButton("Blacklist Visitor");
        blacklist.Location = new Point(848, 14);
        blacklist.Width = 150;
        blacklist.Click += async (_, _) => await BlacklistSelectedAsync();
        top.Controls.Add(blacklist);
        Controls.Add(_grid);
        Controls.Add(top);
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitId", DataPropertyName = "VisitId", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Visit No", DataPropertyName = "VisitNumber", FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Photo", DataPropertyName = "Photo", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Visitor", DataPropertyName = "Visitor", FillWeight = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Identity", DataPropertyName = "IdentityNumber", FillWeight = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Resident", DataPropertyName = "Resident", FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Entry Time", DataPropertyName = "EntryTime", FillWeight = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yy hh:mm tt" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Duration", DataPropertyName = "CurrentDuration", FillWeight = 100 });
    }

    private async Task LoadVisitsAsync()
    {
        try
        {
            _visits = await _visitService.GetActiveVisitsAsync(_search.Text);
            _grid.DataSource = _visits.Select(v => new
            {
                v.VisitId,
                VisitNumber = $"VISIT-{v.VisitId:000000}",
                Photo = string.IsNullOrWhiteSpace(v.Visitor.PhotoPath) ? "No photo" : "Saved",
                Visitor = v.Visitor.FullName,
                IdentityNumber = _maskingService.MaskIdentityNumber(v.Visitor.IdentityNumber),
                Resident = v.Resident.FullName,
                v.EntryTime,
                CurrentDuration = _durationService.FormatDuration((int)(DateTime.Now - v.EntryTime).TotalMinutes)
            }).ToList();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task SignOutAsync()
    {
        if (_grid.CurrentRow is null) return;
        try
        {
            var visitId = (int)_grid.CurrentRow.Cells["VisitId"].Value;
            var result = await _visitService.SignOutAsync(visitId);
            MessageBox.Show(result.Message);
            await LoadVisitsAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task BlacklistSelectedAsync()
    {
        if (_grid.CurrentRow is null) return;
        try
        {
            var visitId = (int)_grid.CurrentRow.Cells["VisitId"].Value;
            var visit = _visits.First(v => v.VisitId == visitId);
            var reason = FormHelpers.PromptForReason(this, "Blacklist Visitor", visit.Visitor.FullName);
            if (reason is null)
            {
                return;
            }

            var result = await _blacklistService.AddAsync(visit.Visitor, reason);
            MessageBox.Show(result.Message, "Blacklist", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }
}
