using AForge.Video;
using AForge.Video.DirectShow;
using VisitorManagementSystem.Utils;

namespace VisitorManagementSystem.Forms;

public sealed class WebcamCaptureForm : Form
{
    private readonly FilterInfoCollection _devices;
    private VideoCaptureDevice? _videoSource;
    private readonly PictureBox _preview = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
    private Bitmap? _latestFrame;

    public Bitmap? CapturedImage { get; private set; }

    public WebcamCaptureForm(FilterInfoCollection devices)
    {
        _devices = devices;
        Text = "Take Photo";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 560);
        BackColor = ModernTheme.Background;
        BuildUi();
        FormClosing += (_, _) => StopCamera();
        Load += (_, _) => StartCamera();
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(16), BackColor = ModernTheme.Surface };
        top.Controls.Add(new Label
        {
            Text = "Webcam Photo",
            AutoSize = true,
            Font = ModernTheme.HeadingFont,
            ForeColor = ModernTheme.Text,
            Location = new Point(16, 17)
        });
        Controls.Add(_preview);
        Controls.Add(top);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(16), BackColor = ModernTheme.Surface };
        var capture = ModernTheme.PrimaryButton("Capture Photo");
        capture.Width = 150;
        capture.Location = new Point(16, 16);
        capture.Click += (_, _) => CaptureCurrentFrame();
        bottom.Controls.Add(capture);

        var cancel = ModernTheme.SecondaryButton("Cancel");
        cancel.Width = 100;
        cancel.Location = new Point(180, 16);
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        bottom.Controls.Add(cancel);
        Controls.Add(bottom);
    }

    private void StartCamera()
    {
        if (_devices.Count == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        _videoSource = new VideoCaptureDevice(_devices[0].MonikerString);
        _videoSource.NewFrame += OnNewFrame;
        _videoSource.Start();
    }

    private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        var frame = (Bitmap)eventArgs.Frame.Clone();
        BeginInvoke(() =>
        {
            _latestFrame?.Dispose();
            _latestFrame = (Bitmap)frame.Clone();
            _preview.Image?.Dispose();
            _preview.Image = frame;
        });
    }

    private void CaptureCurrentFrame()
    {
        if (_latestFrame is null)
        {
            MessageBox.Show("The camera is still starting. Please try again in a moment.", "Take Photo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        CapturedImage = (Bitmap)_latestFrame.Clone();
        DialogResult = DialogResult.OK;
    }

    private void StopCamera()
    {
        if (_videoSource is { IsRunning: true })
        {
            _videoSource.SignalToStop();
            _videoSource.WaitForStop();
        }

        _latestFrame?.Dispose();
    }
}
