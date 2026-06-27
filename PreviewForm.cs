using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScannedCardDenoiser
{
    // 큰 이미지를 창에 맞게 축소해 보여주고, 확대 슬라이더로 배율을 조절하는 프리뷰 창.
    public class PreviewForm : Form
    {
        private readonly Panel panel;
        private readonly PictureBox pic;
        private readonly TrackBar zoom;
        private readonly Label zoomLabel;
        private readonly Bitmap bmp;
        private readonly Size imgSize;

        public PreviewForm(string title, Bitmap image)
        {
            bmp = image;
            imgSize = image.Size;

            Text = title;
            ClientSize = new Size(900, 720);
            StartPosition = FormStartPosition.Manual;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 38 };
            Label lbl = new Label { Text = "확대", AutoSize = true, Location = new Point(8, 11) };
            zoom = new TrackBar
            {
                Minimum = 5,
                Maximum = 400,
                TickFrequency = 50,
                SmallChange = 5,
                LargeChange = 25,
                Width = 480,
                Location = new Point(44, 4)
            };
            zoomLabel = new Label { AutoSize = true, Location = new Point(534, 11), Text = "100%" };
            Button btnFit = new Button { Text = "맞춤", AutoSize = true, Location = new Point(600, 7) };
            Button btn100 = new Button { Text = "100%", AutoSize = true, Location = new Point(650, 7) };
            top.Controls.Add(lbl);
            top.Controls.Add(zoom);
            top.Controls.Add(zoomLabel);
            top.Controls.Add(btnFit);
            top.Controls.Add(btn100);

            panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(40, 40, 40) };
            pic = new PictureBox { SizeMode = PictureBoxSizeMode.StretchImage, Image = bmp, Location = new Point(0, 0) };
            panel.Controls.Add(pic);

            Controls.Add(panel);
            Controls.Add(top);

            zoom.Scroll += (s, e) => ApplyZoom();
            btnFit.Click += (s, e) => FitZoom();
            btn100.Click += (s, e) => { zoom.Value = 100; ApplyZoom(); };
            // 마우스 휠로 확대/축소
            panel.MouseWheel += (s, e) =>
            {
                int nz = zoom.Value + (e.Delta > 0 ? 10 : -10);
                zoom.Value = Math.Max(zoom.Minimum, Math.Min(zoom.Maximum, nz));
                ApplyZoom();
            };
            Shown += (s, e) => FitZoom();
            FormClosed += (s, e) => { if (bmp != null) bmp.Dispose(); };
        }

        private void FitZoom()
        {
            double fit = Math.Min((double)panel.ClientSize.Width / imgSize.Width,
                                  (double)panel.ClientSize.Height / imgSize.Height);
            int z = (int)Math.Round(fit * 100);
            z = Math.Max(zoom.Minimum, Math.Min(zoom.Maximum, z));
            zoom.Value = z;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            int z = zoom.Value;
            pic.Size = new Size(Math.Max(1, imgSize.Width * z / 100), Math.Max(1, imgSize.Height * z / 100));
            zoomLabel.Text = z + "%";
        }
    }
}
