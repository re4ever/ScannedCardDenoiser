using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScannedCardDenoiser
{
    // 큰 이미지를 창에 맞게 축소해 보여주고, 확대 슬라이더로 배율을 조절하는 프리뷰 창.
    // compare(원본)를 넘기면 결과 위에 원본을 겹쳐, 세로 분할선을 드래그해 원본|결과를 비교한다.
    public class PreviewForm : Form
    {
        private readonly Panel panel;
        private readonly PictureBox pic;
        private readonly TrackBar zoom;
        private readonly Label zoomLabel;
        private readonly Bitmap bmp;
        private readonly Size imgSize;

        private readonly Bitmap compareBmp;   // 분할 비교용 원본(null이면 단일 이미지 창)
        private double splitFrac = 0.5;       // 분할선 위치(표시 폭 대비 0~1)
        private bool splitDrag = false;

        public PreviewForm(string title, Bitmap image, Bitmap compare = null)
        {
            bmp = image;
            imgSize = image.Size;
            compareBmp = compare;

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
            Button btn100 = new Button { Text = "100%", AutoSize = true, Location = new Point(680, 7) };
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
            FormClosed += (s, e) =>
            {
                if (bmp != null) bmp.Dispose();
                if (compareBmp != null) compareBmp.Dispose();
            };

            if (compareBmp != null)
            {
                // 분할 비교: 분할선을 잡고 드래그(혹은 클릭)해서 좌우 비율을 바꾼다.
                pic.Cursor = Cursors.VSplit;
                pic.Paint += Pic_PaintSplit;
                pic.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { splitDrag = true; UpdateSplit(e.X); } };
                pic.MouseMove += (s, e) => { if (splitDrag) UpdateSplit(e.X); };
                pic.MouseUp += (s, e) => splitDrag = false;
            }
        }

        private void UpdateSplit(int x)
        {
            splitFrac = Math.Max(0.0, Math.Min(1.0, (double)x / Math.Max(1, pic.Width)));
            pic.Invalidate();
        }

        // 분할선 왼쪽 = 원본, 오른쪽 = 결과. 두 이미지는 각각 표시 크기에 맞게 늘려 겹친다.
        private void Pic_PaintSplit(object sender, PaintEventArgs e)
        {
            int w = pic.Width, h = pic.Height;
            int split = (int)Math.Round(w * splitFrac);

            if (split > 0)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                e.Graphics.DrawImage(compareBmp,
                    new Rectangle(0, 0, split, h),
                    new RectangleF(0, 0, (float)(compareBmp.Width * splitFrac), compareBmp.Height),
                    GraphicsUnit.Pixel);
            }

            using (Pen div = new Pen(Color.Yellow, 2f))
                e.Graphics.DrawLine(div, split, 0, split, h);

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, Color.Black)))
            using (SolidBrush tx = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(bg, 4, 4, 44, 20);
                e.Graphics.DrawString("원본", Font, tx, 8, 7);
                e.Graphics.FillRectangle(bg, w - 48, 4, 44, 20);
                e.Graphics.DrawString("결과", Font, tx, w - 44, 7);
            }
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
