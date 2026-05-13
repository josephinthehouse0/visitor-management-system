using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class VisitorExitForm : Form
{
    private readonly VisitService _visitService;
    private readonly DurationService _durationService;
    private readonly DataMaskingService _maskingService;
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly DataGridView _grid = ModernTheme.Grid();
    private List<Visit> _visits = new();

    public VisitorExitForm(VisitService visitService, DurationService durationService, DataMaskingService maskingService)
    {
        _visitService = visitService;
        _durationService = durationService;
        _maskingService = maskingService;
        Text = "Visitor Exit";
        BackColor = ModernTheme.Background;
        BuildUi();
        Load += async (_, _) => await LoadVisitsAsync();
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Visitor Exit", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, AutoSize = true, Location = new Point(6, 10) });
        _search.Width = 300;
        _search.PlaceholderText = "Search active visitors";
        _search.Location = new Point(240, 16);
        _search.TextChanged += async (_, _) => await LoadVisitsAsync();
        top.Controls.Add(_search);
        var signOut = ModernTheme.PrimaryButton("Sign Out Selected");
        signOut.Location = new Point(560, 14);
        signOut.Width = 170;
        signOut.Click += async (_, _) => await SignOutAsync();
        top.Controls.Add(signOut);
        Controls.Add(_grid);
        Controls.Add(top);
    }

    private async Task LoadVisitsAsync()
    {
        try
        {
            _visits = await _visitService.GetActiveVisitsAsync(_search.Text);
            _grid.DataSource = _visits.Select(v => new
            {
                v.VisitId,
                Visitor = v.Visitor.FullName,
                Identity = _maskingService.MaskIdentityNumber(v.Visitor.IdentityNumber),
                Resident = v.Resident.FullName,
                v.EntryTime,
                Duration = _durationService.FormatDuration((int)(DateTime.Now - v.EntryTime).TotalMinutes)
            }).ToList();
            _grid.Columns["VisitId"].Visible = false;
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
}
