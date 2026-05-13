using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class ResidentsForm : Form
{
    private readonly ResidentService _residentService;
    private readonly ImageService _imageService;
    private readonly TextBox _search = ModernTheme.TextBox();
    private readonly TextBox _identity = ModernTheme.TextBox();
    private readonly TextBox _name = ModernTheme.TextBox();
    private readonly TextBox _room = ModernTheme.TextBox();
    private readonly PictureBox _photo = ModernTheme.PictureBox();
    private readonly DataGridView _grid = ModernTheme.Grid();
    private List<Resident> _residents = new();
    private int _selectedId;
    private string? _photoPath;

    public ResidentsForm(ResidentService residentService, ImageService imageService)
    {
        _residentService = residentService;
        _imageService = imageService;
        Text = "Residents";
        BackColor = ModernTheme.Background;
        BuildUi();
        Load += async (_, _) => await LoadResidentsAsync();
    }

    private void BuildUi()
    {
        var left = ModernTheme.Card();
        left.Dock = DockStyle.Left;
        left.Width = 330;
        Controls.Add(left);
        left.Controls.Add(new Label { Text = "Resident Details", Font = ModernTheme.HeadingFont, ForeColor = ModernTheme.Text, Location = new Point(18, 18), AutoSize = true });
        AddField(left, "Identity Number", _identity, 62);
        AddField(left, "Full Name", _name, 128);
        AddField(left, "Room / House Number", _room, 194);
        _photo.Location = new Point(18, 270);
        left.Controls.Add(_photo);
        var upload = ModernTheme.SecondaryButton("Upload Photo");
        upload.Location = new Point(158, 270);
        upload.Width = 126;
        upload.Click += (_, _) => { _photoPath = _imageService.UploadPhoto(this) ?? _photoPath; ImageService.LoadInto(_photo, _photoPath); };
        left.Controls.Add(upload);
        var save = ModernTheme.PrimaryButton("Save");
        save.Location = new Point(18, 420);
        save.Width = 126;
        save.Click += async (_, _) => await SaveAsync();
        left.Controls.Add(save);
        var delete = ModernTheme.DangerButton("Delete");
        delete.Location = new Point(158, 420);
        delete.Width = 126;
        delete.Click += async (_, _) => await DeleteAsync();
        left.Controls.Add(delete);

        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        Controls.Add(main);
        Controls.SetChildIndex(main, 0);
        var top = new Panel { Dock = DockStyle.Top, Height = 58 };
        top.Controls.Add(new Label { Text = "Residents", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, AutoSize = true, Location = new Point(0, 8) });
        _search.PlaceholderText = "Search residents";
        _search.Location = new Point(230, 12);
        _search.Width = 300;
        _search.TextChanged += async (_, _) => await LoadResidentsAsync();
        top.Controls.Add(_search);
        main.Controls.Add(_grid);
        main.Controls.Add(top);
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

    private async Task LoadResidentsAsync()
    {
        try
        {
            _residents = await _residentService.GetResidentsAsync(_search.Text);
            _grid.DataSource = _residents.Select(r => new { r.ResidentId, r.FullName, r.IdentityNumber, r.RoomNumber, r.IsActive }).ToList();
            _grid.Columns["ResidentId"].Visible = false;
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        _selectedId = (int)_grid.Rows[e.RowIndex].Cells["ResidentId"].Value;
        var resident = _residents.First(r => r.ResidentId == _selectedId);
        _identity.Text = resident.IdentityNumber;
        _name.Text = resident.FullName;
        _room.Text = resident.RoomNumber;
        _photoPath = resident.PhotoPath;
        ImageService.LoadInto(_photo, _photoPath);
    }

    private async Task SaveAsync()
    {
        try
        {
            var result = await _residentService.SaveAsync(new Resident
            {
                ResidentId = _selectedId,
                IdentityNumber = _identity.Text.Trim(),
                FullName = _name.Text.Trim(),
                RoomNumber = _room.Text.Trim(),
                PhotoPath = _photoPath
            });
            MessageBox.Show(result.Message);
            if (result.Success) await LoadResidentsAsync();
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
            await _residentService.DeleteAsync(_selectedId);
            _selectedId = 0;
            await LoadResidentsAsync();
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }
}
