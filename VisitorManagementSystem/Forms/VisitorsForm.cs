using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class VisitorsForm : Form
{
    private readonly VisitorService _visitorService;
    private readonly ImageService _imageService;
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly TextBox _identity = ModernTheme.TextBox();
    private readonly TextBox _name = ModernTheme.TextBox();
    private readonly PictureBox _photo = ModernTheme.PictureBox();
    private readonly DataGridView _grid = ModernTheme.Grid();
    private int _selectedId;
    private string? _photoPath;
    private List<Visitor> _loadedVisitors = new();
    private List<VisitorListItem> _visitorRows = new();
    private Dictionary<string, Visitor> _visitorLookup = new();

    public VisitorsForm(VisitorService visitorService, ImageService imageService)
    {
        _visitorService = visitorService;
        _imageService = imageService;
        Text = "Visitors";
        BackColor = ModernTheme.Background;
        BuildUi();
        ConfigureGrid();
        Load += async (_, _) => await LoadVisitorsAsync();
    }

    private void BuildUi()
    {
        var left = ModernTheme.Card();
        left.Dock = DockStyle.Left;
        left.Width = 330;
        Controls.Add(left);

        var title = ModernTheme.Label("Visitor Details", 16, true);
        title.Location = new Point(16, 14);
        left.Controls.Add(title);
        AddField(left, "Identity Number", _identity, 62);
        AddField(left, "Full Name", _name, 128);
        _photo.Location = new Point(18, 210);
        left.Controls.Add(_photo);

        var take = ModernTheme.SecondaryButton("Take Photo");
        take.Location = new Point(18, 350);
        take.Width = 128;
        take.Click += (_, _) => { _photoPath = _imageService.TakePhoto(this) ?? _photoPath; ImageService.LoadInto(_photo, _photoPath); };
        left.Controls.Add(take);

        var upload = ModernTheme.SecondaryButton("Upload Photo");
        upload.Location = new Point(156, 350);
        upload.Width = 128;
        upload.Click += (_, _) => { _photoPath = _imageService.UploadPhoto(this) ?? _photoPath; ImageService.LoadInto(_photo, _photoPath); };
        left.Controls.Add(upload);

        var save = ModernTheme.PrimaryButton("Save Visitor");
        save.Location = new Point(18, 405);
        save.Width = 266;
        save.Click += async (_, _) => await SaveAsync();
        left.Controls.Add(save);

        var delete = ModernTheme.DangerButton("Delete");
        delete.Location = new Point(18, 455);
        delete.Width = 126;
        delete.Click += async (_, _) => await DeleteAsync();
        left.Controls.Add(delete);

        var clear = ModernTheme.SecondaryButton("Clear");
        clear.Location = new Point(158, 455);
        clear.Width = 126;
        clear.Click += (_, _) => Clear();
        left.Controls.Add(clear);

        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        Controls.Add(main);
        Controls.SetChildIndex(main, 0);
        var header = new Panel { Dock = DockStyle.Top, Height = 58 };
        main.Controls.Add(_grid);
        main.Controls.Add(header);
        header.Controls.Add(new Label { Text = "Visitors", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, AutoSize = true, Location = new Point(0, 8) });
        _search.Width = 320;
        _search.Location = new Point(250, 12);
        _search.PlaceholderText = "Search visitors or identity number";
        _search.TextChanged += async (_, _) => await LoadVisitorsAsync();
        header.Controls.Add(_search);
        _grid.CellClick += Grid_CellClick;
    }

    private static void AddField(Control parent, string label, TextBox box, int top)
    {
        var lbl = ModernTheme.Label(label);
        lbl.Location = new Point(18, top);
        box.Location = new Point(18, top + 24);
        box.Width = 266;
        parent.Controls.Add(lbl);
        parent.Controls.Add(box);
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitorId", DataPropertyName = "VisitorId", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Visitor No", DataPropertyName = "ReservationNumber", FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = "FullName", FillWeight = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Identity Number", DataPropertyName = "IdentityNumber", FillWeight = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Times Came", DataPropertyName = "VisitCount", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created At", DataPropertyName = "CreatedAt", FillWeight = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yy hh:mm tt" } });
    }

    private async Task LoadVisitorsAsync()
    {
        try
        {
            _loadedVisitors = await _visitorService.GetVisitorsAsync(_search.Text);
            _visitorRows = await _visitorService.GetVisitorListItemsAsync(_search.Text);
            _visitorLookup = await _visitorService.GetVisitorLookupAsync();
            _grid.DataSource = _visitorRows;
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        _selectedId = (int)_grid.Rows[e.RowIndex].Cells["VisitorId"].Value;
        var visitor = _loadedVisitors.First(v => v.VisitorId == _selectedId);
        _identity.Text = visitor.IdentityNumber;
        _name.Text = visitor.FullName;
        _photoPath = visitor.PhotoPath;
        ImageService.LoadInto(_photo, _photoPath);
    }

    private async Task SaveAsync()
    {
        try
        {
            if (_visitorLookup.TryGetValue(_identity.Text.Trim(), out var existing) && existing.VisitorId != _selectedId)
            {
                _selectedId = existing.VisitorId;
                _photoPath = existing.PhotoPath;
            }

            var visitor = new Visitor { VisitorId = _selectedId, IdentityNumber = _identity.Text.Trim(), FullName = _name.Text.Trim(), PhotoPath = _photoPath };
            var result = await _visitorService.SaveAsync(visitor);
            MessageBox.Show(result.Message);
            if (result.Success)
            {
                Clear();
                await LoadVisitorsAsync();
            }
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task DeleteAsync()
    {
        if (_selectedId == 0) return;
        try
        {
            await _visitorService.DeleteAsync(_selectedId);
            Clear();
            await LoadVisitorsAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private void Clear()
    {
        _selectedId = 0;
        _identity.Clear();
        _name.Clear();
        _photoPath = null;
        ImageService.LoadInto(_photo, null);
    }
}
