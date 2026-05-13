using AForge.Video.DirectShow;
using VisitorManagementSystem.Forms;

namespace VisitorManagementSystem.Services;

public sealed class ImageService
{
    private readonly string _photoFolder;

    public ImageService()
    {
        _photoFolder = Path.Combine(AppContext.BaseDirectory, "VisitorPhotos");
        Directory.CreateDirectory(_photoFolder);
    }

    public string? UploadPhoto(IWin32Window owner)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Upload Visitor Photo",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog(owner) != DialogResult.OK)
        {
            return null;
        }

        var extension = Path.GetExtension(dialog.FileName);
        var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(_photoFolder, fileName);
        File.Copy(dialog.FileName, destination, overwrite: false);
        return destination;
    }

    public string? TakePhoto(IWin32Window owner)
    {
        try
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (devices.Count == 0)
            {
                return UploadPhoto(owner);
            }

            using var capture = new WebcamCaptureForm(devices);
            if (capture.ShowDialog(owner) != DialogResult.OK || capture.CapturedImage is null)
            {
                return null;
            }

            return SaveBitmap(capture.CapturedImage);
        }
        catch
        {
            return UploadPhoto(owner);
        }
    }

    private string SaveBitmap(Bitmap bitmap)
    {
        var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}.jpg";
        var destination = Path.Combine(_photoFolder, fileName);
        bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Jpeg);
        return destination;
    }

    public static void LoadInto(PictureBox pictureBox, string? path)
    {
        pictureBox.Image?.Dispose();
        pictureBox.Image = null;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            pictureBox.Image = Image.FromStream(stream);
        }
    }
}
