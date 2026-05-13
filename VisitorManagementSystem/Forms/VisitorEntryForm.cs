using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class VisitorEntryForm : Form
{
    private readonly VisitorService _visitorService;
    private readonly ResidentService _residentService;
    private readonly VisitService _visitService;
    private readonly ImageService _imageService;
    private readonly TextBox _identity = ModernTheme.TextBox();
    private readonly TextBox _name = ModernTheme.TextBox();
    private readonly TextBox _reason = ModernTheme.TextBox();
    private readonly ComboBox _residents = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = ModernTheme.BodyFont, Height = 32 };
    private readonly PictureBox _photo = ModernTheme.PictureBox();
    private Visitor? _visitor;
    private string? _photoPath;

    public VisitorEntryForm(VisitorService visitorService, ResidentService residentService, VisitService visitService, ImageService imageService)
    {
        _visitorService = visitorService;
        _residentService = residentService;
        _visitService = visitService;
        _imageService = imageService;
        Text = "Sign In Visitor";
        BackColor = ModernTheme.Background;
        BuildUi();
        Load += async (_, _) => await LoadResidentsAsync();
    }

    private void BuildUi()
    {
        var card = ModernTheme.Card();
        card.Dock = DockStyle.Top;
        card.Height = 520;
        Controls.Add(card);
        card.Controls.Add(new Label { Text = "Sign In Visitor", Font = ModernTheme.TitleFont, ForeColor = ModernTheme.Text, Location = new Point(20, 18), AutoSize = true });
        AddField(card, "Identity Number", _identity, 82);
        _identity.TextChanged += async (_, _) => await TryLoadVisitorAsync();
        AddField(card, "Full Name", _name, 150);

        _photo.Location = new Point(370, 88);
        card.Controls.Add(_photo);
        var take = ModernTheme.SecondaryButton("Take Photo");
        take.Location = new Point(370, 230);
        take.Width = 128;
        take.Click += (_, _) => { _photoPath = _imageService.TakePhoto(this) ?? _photoPath; ImageService.LoadInto(_photo, _photoPath); };
        card.Controls.Add(take);
        var upload = ModernTheme.SecondaryButton("Upload Photo");
        upload.Location = new Point(508, 230);
        upload.Width = 128;
        upload.Click += (_, _) => { _photoPath = _imageService.UploadPhoto(this) ?? _photoPath; ImageService.LoadInto(_photo, _photoPath); };
        card.Controls.Add(upload);

        var residentLabel = ModernTheme.Label("Resident / Person Being Visited");
        residentLabel.Location = new Point(20, 220);
        card.Controls.Add(residentLabel);
        _residents.Location = new Point(20, 248);
        _residents.Width = 310;
        card.Controls.Add(_residents);

        AddField(card, "Visit Reason", _reason, 292);
        _reason.Width = 616;

        var signIn = ModernTheme.PrimaryButton("Sign In");
        signIn.Location = new Point(20, 380);
        signIn.Width = 180;
        signIn.Click += async (_, _) => await SignInAsync();
        card.Controls.Add(signIn);
    }

    private static void AddField(Control parent, string label, TextBox box, int top)
    {
        var lbl = ModernTheme.Label(label);
        lbl.Location = new Point(20, top);
        box.Location = new Point(20, top + 26);
        box.Width = 310;
        parent.Controls.Add(lbl);
        parent.Controls.Add(box);
    }

    private async Task LoadResidentsAsync()
    {
        try
        {
            _residents.DataSource = await _residentService.GetResidentsAsync();
            _residents.DisplayMember = nameof(Resident.FullName);
            _residents.ValueMember = nameof(Resident.ResidentId);
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task TryLoadVisitorAsync()
    {
        if (_identity.Text.Trim().Length != 11) return;
        try
        {
            _visitor = await _visitorService.FindByIdentityAsync(_identity.Text.Trim());
            if (_visitor is not null)
            {
                _name.Text = _visitor.FullName;
                _photoPath = _visitor.PhotoPath;
                ImageService.LoadInto(_photo, _photoPath);
            }
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }

    private async Task SignInAsync()
    {
        try
        {
            _visitor ??= new Visitor { IdentityNumber = _identity.Text.Trim(), FullName = _name.Text.Trim(), PhotoPath = _photoPath };
            if (_visitor.VisitorId == 0)
            {
                var saved = await _visitorService.SaveAsync(_visitor);
                if (!saved.Success || saved.Visitor is null)
                {
                    MessageBox.Show(saved.Message, "Visitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _visitor = saved.Visitor;
            }

            if (_residents.SelectedValue is not int residentId)
            {
                MessageBox.Show("Please select the resident being visited.", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = await _visitService.SignInAsync(_visitor, residentId, _reason.Text);
            MessageBox.Show(result.Message, "Sign In", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (result.Success)
            {
                _identity.Clear();
                _name.Clear();
                _reason.Clear();
                _visitor = null;
                _photoPath = null;
                ImageService.LoadInto(_photo, null);
            }
        }
        catch (Exception ex)
        {
            FormHelpers.ShowError(ex);
        }
    }
}
