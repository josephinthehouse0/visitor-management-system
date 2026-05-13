using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class VisitHistoryForm : Form
{
    private readonly VisitService _visitService;
    private readonly ResidentService _residentService;
    private readonly VisitorService _visitorService;
    private readonly BlacklistService _blacklistService;
    private readonly DurationService _durationService;
    private readonly DataMaskingService _maskingService;
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Font = ModernTheme.BodyFont };
    private readonly ComboBox _resident = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = ModernTheme.BodyFont };
    private readonly ComboBox _visitor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = ModernTheme.BodyFont };
    private readonly DataGridView _grid = ModernTheme.Grid();
    private List<Visit> _visits = new();

    public VisitHistoryForm(VisitService visitService, ResidentService residentService, VisitorService visitorService, BlacklistService blacklistService, DurationService durationService, DataMaskingService maskingService)
    {
        _visitService = visitService;
        _residentService = residentService;
        _visitorService = visitorService;
        _blacklistService = blacklistService;
        _durationService = durationService;
        _maskingService = maskingService;
        Text = "Visit History";
        BackColor = ModernTheme.Background;
        BuildUi();
        Load += async (_, _) => await LoadFiltersAndVisitsAsync();
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 108, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Visit History", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, AutoSize = true, Location = new Point(6, 8) });
        _search.PlaceholderText = "Search completed visits";
        _search.Location = new Point(6, 62);
        _search.Width = 250;
        _date.Location = new Point(270, 62);
        _date.Width = 130;
        _resident.Location = new Point(414, 62);
        _resident.Width = 180;
        _visitor.Location = new Point(608, 62);
        _visitor.Width = 180;
        var filter = ModernTheme.PrimaryButton("Filter");
        filter.Location = new Point(802, 58);
        filter.Width = 100;
        filter.Click += async (_, _) => await LoadVisitsAsync();
        var blacklist = ModernTheme.DangerButton("Blacklist Visitor");
        blacklist.Location = new Point(916, 58);
        blacklist.Width = 150;
        blacklist.Click += async (_, _) => await BlacklistSelectedAsync();
        top.Controls.AddRange(new Control[] { _search, _date, _resident, _visitor, filter, blacklist });
        Controls.Add(_grid);
        Controls.Add(top);
    }

    private async Task LoadFiltersAndVisitsAsync()
    {
        try
        {
            var residents = await _residentService.GetResidentsAsync();
            residents.Insert(0, new Resident { ResidentId = 0, FullName = "All residents" });
            _resident.DisplayMember = nameof(Resident.FullName);
            _resident.ValueMember = nameof(Resident.ResidentId);
            _resident.DataSource = residents;
            _resident.SelectedIndex = 0;

            var visitors = await _visitorService.GetVisitorsAsync();
            visitors.Insert(0, new Visitor { VisitorId = 0, FullName = "All visitors" });
            _visitor.DisplayMember = nameof(Visitor.FullName);
            _visitor.ValueMember = nameof(Visitor.VisitorId);
            _visitor.DataSource = visitors;
            _visitor.SelectedIndex = 0;
            _date.Checked = false;
            await LoadVisitsAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task LoadVisitsAsync()
    {
        try
        {
            _visits = await _visitService.GetCompletedVisitsAsync(
                _search.Text,
                _date.Checked ? _date.Value.Date : null,
                SelectedId(_resident),
                SelectedId(_visitor));

            _grid.DataSource = _visits.Select(v => new
            {
                v.VisitId,
                Visitor = v.Visitor.FullName,
                IdentityNumber = _maskingService.MaskIdentityNumber(v.Visitor.IdentityNumber),
                Resident = v.Resident.FullName,
                v.EntryTime,
                v.ExitTime,
                Duration = _durationService.FormatDuration(v.DurationMinutes),
                v.VisitReason
            }).ToList();
            _grid.Columns["VisitId"].Visible = false;
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

    private static int? SelectedId(ComboBox comboBox)
    {
        return comboBox.SelectedValue is int value && value > 0 ? value : null;
    }
}
