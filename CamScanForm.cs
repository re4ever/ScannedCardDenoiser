using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    public partial class CamScanForm : Form
    {
        public Mat result { get; set; } = null;
        public OpenCvSharp.Size resultSize { get; set; }
        public System.Drawing.Point min { get; set; }
        public System.Drawing.Point max { get; set; }

        // 초기 4포인트 검출 시 사용하는 Otsu 오프셋(Threshold 컨트롤 값). Auto와 동일한 의미.
        private readonly int adjThresholdOffset;

        public CamScanForm(string filepath, int desireLong, int desireShort, int adjThresholdOffset = 0)
        {
            this.adjThresholdOffset = adjThresholdOffset;
            result = Cv2.ImRead(filepath);
            InitializeComponent();

            int srcW, srcH;
            using (Image Src = Image.FromFile(filepath))
            {
                srcW = Src.Width; srcH = Src.Height;
                if (srcW > 640 || srcH > 640)
                {
                    float ratio = Math.Min(640.0f / srcW, 640.0f / srcH);
                    PIC_Main.Image = new Bitmap(Src, new System.Drawing.Size((int)(srcW * ratio + 0.5), (int)(srcH * ratio + 0.5)));
                }
                else
                {
                    PIC_Main.Image = new Bitmap(Src);
                }
            }
            // 폼 종료 시 표시용 비트맵 해제 (GDI 누수 방지)
            this.FormClosed += (s, ev) => { if (PIC_Main.Image != null) { PIC_Main.Image.Dispose(); PIC_Main.Image = null; } };

            min = new System.Drawing.Point(PIC_Main.Location.X + (640 - PIC_Main.Image.Size.Width) / 2 - TopLeft.Size.Width / 2, PIC_Main.Location.Y + (640 - PIC_Main.Image.Size.Height) / 2 - TopLeft.Size.Height / 2);
            max = new System.Drawing.Point(min.X + PIC_Main.Image.Size.Width, min.Y + PIC_Main.Image.Size.Height);

            if (desireLong != 0 && desireShort != 0 && srcH < srcW)
            {
                resultSize = new OpenCvSharp.Size(desireLong, desireShort);
            }
            else if (desireLong != 0 && desireShort != 0 && srcH > srcW)
            {
                resultSize = new OpenCvSharp.Size(desireShort, desireLong);
            }
            else
            {
                resultSize = new OpenCvSharp.Size(srcW, srcH);
            }

            System.Drawing.Point basePoint = GetBasePoint();
            basePoint.X -= TopLeft.Size.Width / 2;
            basePoint.Y -= TopLeft.Size.Height / 2;

            // Auto 검출(기울기 deskew 포함)한 카드 4모서리를 초기 포인트 위치로 사용(실패 시 전체 이미지). (표시 좌표 기준)
            System.Drawing.PointF qTL, qTR, qBL, qBR;
            DetectCardQuad(out qTL, out qTR, out qBL, out qBR);

            TopLeft.Location = new System.Drawing.Point(basePoint.X + (int)Math.Round(qTL.X), basePoint.Y + (int)Math.Round(qTL.Y));
            TopRight.Location = new System.Drawing.Point(basePoint.X + (int)Math.Round(qTR.X), basePoint.Y + (int)Math.Round(qTR.Y));
            BottomLeft.Location = new System.Drawing.Point(basePoint.X + (int)Math.Round(qBL.X), basePoint.Y + (int)Math.Round(qBL.Y));
            BottomRight.Location = new System.Drawing.Point(basePoint.X + (int)Math.Round(qBR.X), basePoint.Y + (int)Math.Round(qBR.Y));

            TopCenter.Location = new System.Drawing.Point((TopLeft.Location.X + TopRight.Location.X) / 2, (TopLeft.Location.Y + TopRight.Location.Y) / 2);
            BottomCenter.Location = new System.Drawing.Point((BottomLeft.Location.X + BottomRight.Location.X) / 2, (BottomLeft.Location.Y + BottomRight.Location.Y) / 2);
            CenterLeft.Location = new System.Drawing.Point((TopLeft.Location.X + BottomLeft.Location.X) / 2, (TopLeft.Location.Y + BottomLeft.Location.Y) / 2);
            CenterRight.Location = new System.Drawing.Point((TopRight.Location.X + BottomRight.Location.X) / 2, (TopRight.Location.Y + BottomRight.Location.Y) / 2);

            // 포인트 컨트롤은 숨기고(투명 박스 잔상 제거) PIC_Main 위에 마커를 직접 그린다.
            // 드래그/히트테스트도 PIC_Main에서 처리한다.
            foreach (PictureBox pb in new[] { TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, CenterLeft, CenterRight })
                pb.Visible = false;

            // 돋보기 갱신 시 깜빡임 감소 (PictureBox 더블버퍼링)
            typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(PIC_Main, true);
        }

        System.Drawing.Point GetBasePoint()
        {
            System.Drawing.Point basePoint = new System.Drawing.Point(PIC_Main.Location.X, PIC_Main.Location.Y);
            basePoint.X += (PIC_Main.ClientSize.Width - PIC_Main.Image.Size.Width) / 2;
            basePoint.Y += (PIC_Main.ClientSize.Height - PIC_Main.Image.Size.Height) / 2;

            return basePoint;
        }

        // Auto와 '완전히 동일한' 검출기(CardCornerDetector)로 전체 해상도 원본에서 카드 4모서리를
        // (원본 좌표로) 구하고, 표시 좌표로 스케일한다. → Auto 크롭과 Manual 초기 위치가 일치한다.
        private void DetectCardQuad(out System.Drawing.PointF tl, out System.Drawing.PointF tr, out System.Drawing.PointF bl, out System.Drawing.PointF br)
        {
            int Wd = PIC_Main.Image.Width, Hd = PIC_Main.Image.Height;   // 표시 크기
            tl = new System.Drawing.PointF(0, 0);
            tr = new System.Drawing.PointF(Wd, 0);
            bl = new System.Drawing.PointF(0, Hd);
            br = new System.Drawing.PointF(Wd, Hd);

            int Wf = result.Cols, Hf = result.Rows;                       // 전체 해상도
            if (Wf < 1 || Hf < 1)
                return;

            // Manual에서는 단계별 DEBUG 프리뷰를 띄우지 않는다(showDebug: false).
            Point2f[] cor;
            if (!CardCornerDetector.DetectOriginalCorners(result, adjThresholdOffset, false, out cor))
                return;

            // 전체 해상도 → 표시 좌표로 스케일 (cor 순서: TL, TR, BR, BL)
            double sx = (double)Wd / Wf, sy = (double)Hd / Hf;
            tl = new System.Drawing.PointF((float)(cor[0].X * sx), (float)(cor[0].Y * sy));
            tr = new System.Drawing.PointF((float)(cor[1].X * sx), (float)(cor[1].Y * sy));
            br = new System.Drawing.PointF((float)(cor[2].X * sx), (float)(cor[2].Y * sy));
            bl = new System.Drawing.PointF((float)(cor[3].X * sx), (float)(cor[3].Y * sy));
        }

        private void BTN_Confirm_Click(object sender, EventArgs e)
        {
            System.Drawing.Point basePoint = GetBasePoint();
            basePoint.X -= TopLeft.Size.Width / 2;
            basePoint.Y -= TopLeft.Size.Height / 2;

            float Scale = (float)result.Width / PIC_Main.Image.Size.Width;
            float Scaley = (float)result.Height / PIC_Main.Image.Size.Height;

            Mat TransformedTL = new Mat();
            Mat TransformedTR = new Mat();
            Mat TransformedBL = new Mat();
            Mat TransformedBR = new Mat();

            System.Drawing.Point CenterCenterLocation = new System.Drawing.Point((TopCenter.Location.X + BottomCenter.Location.X + CenterLeft.Location.X + CenterRight.Location.X) / 4, (TopCenter.Location.Y + BottomCenter.Location.Y + CenterLeft.Location.Y + CenterRight.Location.Y) / 4);

            List<OpenCvSharp.Vec2f> dstPoints = new List<OpenCvSharp.Vec2f>();
            dstPoints.Add(new OpenCvSharp.Vec2f(0, 0));
            dstPoints.Add(new OpenCvSharp.Vec2f(resultSize.Width / 2.0f, 0));
            dstPoints.Add(new OpenCvSharp.Vec2f(0, resultSize.Height / 2.0f));
            dstPoints.Add(new OpenCvSharp.Vec2f(resultSize.Width / 2.0f, resultSize.Height / 2.0f));

            OpenCvSharp.Size partsSize = new OpenCvSharp.Size(resultSize.Width / 2.0, resultSize.Height / 2.0);

            //TopLeft
            {
                List<OpenCvSharp.Vec2f> srcPoints = new List<OpenCvSharp.Vec2f>();
                srcPoints.Add(GetPointsCv(Scale, TopLeft.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, TopCenter.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, CenterLeft.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, CenterCenterLocation, basePoint));

                Mat perspectiveTransform = Cv2.GetPerspectiveTransform(InputArray.Create(srcPoints), InputArray.Create(dstPoints));
                
                Cv2.WarpPerspective(result, TransformedTL, perspectiveTransform, partsSize);
                perspectiveTransform.Dispose();
            }
            //TopRight
            {
                List<OpenCvSharp.Vec2f> srcPoints = new List<OpenCvSharp.Vec2f>();
                srcPoints.Add(GetPointsCv(Scale, TopCenter.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, TopRight.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, CenterCenterLocation, basePoint)); 
                srcPoints.Add(GetPointsCv(Scale, CenterRight.Location, basePoint));

                Mat perspectiveTransform = Cv2.GetPerspectiveTransform(InputArray.Create(srcPoints), InputArray.Create(dstPoints));

                Cv2.WarpPerspective(result, TransformedTR, perspectiveTransform, partsSize);
                perspectiveTransform.Dispose();
            }
            //BottomLeft
            {
                List<OpenCvSharp.Vec2f> srcPoints = new List<OpenCvSharp.Vec2f>();
                srcPoints.Add(GetPointsCv(Scale, CenterLeft.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, CenterCenterLocation, basePoint));
                srcPoints.Add(GetPointsCv(Scale, BottomLeft.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, BottomCenter.Location, basePoint));

                Mat perspectiveTransform = Cv2.GetPerspectiveTransform(InputArray.Create(srcPoints), InputArray.Create(dstPoints));

                Cv2.WarpPerspective(result, TransformedBL, perspectiveTransform, partsSize);
                perspectiveTransform.Dispose();
            }
            //BottomRight
            {
                List<OpenCvSharp.Vec2f> srcPoints = new List<OpenCvSharp.Vec2f>();
                srcPoints.Add(GetPointsCv(Scale, CenterCenterLocation, basePoint));
                srcPoints.Add(GetPointsCv(Scale, CenterRight.Location, basePoint));
                srcPoints.Add(GetPointsCv(Scale, BottomCenter.Location, basePoint)); 
                srcPoints.Add(GetPointsCv(Scale, BottomRight.Location, basePoint));                

                Mat perspectiveTransform = Cv2.GetPerspectiveTransform(InputArray.Create(srcPoints), InputArray.Create(dstPoints));

                Cv2.WarpPerspective(result, TransformedBR, perspectiveTransform, partsSize);
                perspectiveTransform.Dispose();
            }

            Mat Left = new Mat();
            Mat Right = new Mat();
            Cv2.VConcat(TransformedTL, TransformedBL, Left);
            Cv2.VConcat(TransformedTR, TransformedBR, Right);
            // 원본(result)은 그대로 두고 결과는 별도 Mat에 만든다(미리보기 No 시 재편집 위해).
            Mat finalResult = new Mat();
            Cv2.HConcat(Left, Right, finalResult);

            TransformedTL.Dispose();
            TransformedTR.Dispose();
            TransformedBL.Dispose();
            TransformedBR.Dispose();
            Left.Dispose();
            Right.Dispose();

            // 확정 단계(미리보기/확인) 없이 바로 결과로 확정한다.
            result.Dispose();        // 원본 해제
            result = finalResult;    // 결과로 교체
            DialogResult = DialogResult.OK;
            this.Close();
        }

        OpenCvSharp.Vec2f GetPointsCv(float Scale, System.Drawing.Point Base, System.Drawing.Point ToSub)
        {
            return new OpenCvSharp.Vec2f(Scale * (Base.X - ToSub.X), Scale * (Base.Y - ToSub.Y));
        }

        System.Drawing.Point SubPoints(System.Drawing.Point Base, System.Drawing.Point ToSub)
        {
            return new System.Drawing.Point(Base.X - ToSub.X, Base.Y - ToSub.Y);
        }

        private void DrawLines(PaintEventArgs e)
        {
            System.Drawing.Point basePoint = PIC_Main.Location;
            basePoint.X -= TopLeft.Size.Width / 2;
            basePoint.Y -= TopLeft.Size.Height / 2;

            Graphics g = e.Graphics;
            DrawGuide(g, basePoint, 1f);

            // 포인트 마커를 이미지 위에 직접 그린다(배경이 투명하게 보임).
            DrawMarker(g, basePoint, TopLeft, true);
            DrawMarker(g, basePoint, TopRight, true);
            DrawMarker(g, basePoint, BottomLeft, true);
            DrawMarker(g, basePoint, BottomRight, true);
            DrawMarker(g, basePoint, TopCenter, false);
            DrawMarker(g, basePoint, BottomCenter, false);
            DrawMarker(g, basePoint, CenterLeft, false);
            DrawMarker(g, basePoint, CenterRight, false);
        }

        // 사각형 가이드 라인(외곽 8 + 십자 2)을 그린다. lineWidth로 굵기 조절(돋보기 안에서는 1/배율).
        private void DrawGuide(Graphics g, System.Drawing.Point basePoint, float lineWidth)
        {
            using (Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), lineWidth))
            {
                g.DrawLine(pen, SubPoints(TopLeft.Location, basePoint), SubPoints(TopCenter.Location, basePoint));
                g.DrawLine(pen, SubPoints(TopCenter.Location, basePoint), SubPoints(TopRight.Location, basePoint));
                g.DrawLine(pen, SubPoints(TopLeft.Location, basePoint), SubPoints(CenterLeft.Location, basePoint));
                g.DrawLine(pen, SubPoints(CenterLeft.Location, basePoint), SubPoints(BottomLeft.Location, basePoint));
                g.DrawLine(pen, SubPoints(BottomLeft.Location, basePoint), SubPoints(BottomCenter.Location, basePoint));
                g.DrawLine(pen, SubPoints(BottomCenter.Location, basePoint), SubPoints(BottomRight.Location, basePoint));
                g.DrawLine(pen, SubPoints(BottomRight.Location, basePoint), SubPoints(CenterRight.Location, basePoint));
                g.DrawLine(pen, SubPoints(CenterRight.Location, basePoint), SubPoints(TopRight.Location, basePoint));
            }
            using (Pen pen2 = new Pen(Color.FromArgb(128, 255, 0, 0), lineWidth))
            {
                g.DrawLine(pen2, SubPoints(TopCenter.Location, basePoint), SubPoints(BottomCenter.Location, basePoint));
                g.DrawLine(pen2, SubPoints(CenterLeft.Location, basePoint), SubPoints(CenterRight.Location, basePoint));
            }
        }

        private void DrawMarker(Graphics g, System.Drawing.Point basePoint, PictureBox pb, bool corner)
        {
            System.Drawing.Point c = SubPoints(pb.Location, basePoint);
            int r = corner ? 7 : 5;
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(110, corner ? Color.Red : Color.Gold)))
            using (Pen ring = new Pen(corner ? Color.Red : Color.Gold, 2f))
            using (SolidBrush dot = new SolidBrush(Color.White))
            {
                g.FillEllipse(fill, c.X - r, c.Y - r, 2 * r, 2 * r);
                g.DrawEllipse(ring, c.X - r, c.Y - r, 2 * r, 2 * r);
                g.FillEllipse(dot, c.X - 2, c.Y - 2, 4, 4);
            }
        }

        // 드래그 중인 포인트 주변을 원형으로 확대해 보여주는 돋보기
        private void DrawMagnifier(PaintEventArgs e)
        {
            if (!dragging || activePoint == null || PIC_Main.Image == null)
                return;

            Image img = PIC_Main.Image;
            int imgOffX = (PIC_Main.Width - img.Width) / 2;
            int imgOffY = (PIC_Main.Height - img.Height) / 2;
            int acx = activePoint.Location.X + activePoint.Width / 2 - PIC_Main.Location.X;
            int acy = activePoint.Location.Y + activePoint.Height / 2 - PIC_Main.Location.Y;

            int R = 72;
            float Z = 2.5f;
            int lcx = acx;
            int lcy = acy - R - 50;
            if (lcy - R < 2)
                lcy = acy + R + 50;
            lcx = Math.Max(R + 2, Math.Min(PIC_Main.Width - R - 2, lcx));
            lcy = Math.Max(R + 2, Math.Min(PIC_Main.Height - R - 2, lcy));

            Graphics g = e.Graphics;
            System.Drawing.Drawing2D.GraphicsState st = g.Save();
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddEllipse(lcx - R, lcy - R, 2 * R, 2 * R);
                g.SetClip(path);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.TranslateTransform(lcx, lcy);
                g.ScaleTransform(Z, Z);
                g.TranslateTransform(-acx, -acy);
                g.DrawImage(img, imgOffX, imgOffY, img.Width, img.Height);

                // 확대 화면에도 가이드 라인을 함께 그린다(배율만큼 얇게 → 화면상 ~1px).
                System.Drawing.Point gbase = PIC_Main.Location;
                gbase.X -= TopLeft.Size.Width / 2;
                gbase.Y -= TopLeft.Size.Height / 2;
                DrawGuide(g, gbase, 1f / Z);
            }
            g.Restore(st);

            using (Pen outer = new Pen(Color.Black, 3f))
                g.DrawEllipse(outer, lcx - R, lcy - R, 2 * R, 2 * R);
            using (Pen inner = new Pen(Color.White, 1.5f))
                g.DrawEllipse(inner, lcx - R, lcy - R, 2 * R, 2 * R);
            using (Pen cross = new Pen(Color.FromArgb(220, 255, 0, 0), 1.5f))
            {
                g.DrawLine(cross, lcx - 12, lcy, lcx + 12, lcy);
                g.DrawLine(cross, lcx, lcy - 12, lcx, lcy + 12);
            }
        }

        private void PIC_Main_Paint(object sender, PaintEventArgs e)
        {
            DrawLines(e);
            DrawMagnifier(e);
        }

        // ── PIC_Main 위에서 포인트를 직접 잡아 드래그 (포인트 컨트롤은 숨김) ──
        private bool dragging = false;
        private PictureBox activePoint = null;

        private PictureBox HitTest(System.Drawing.Point ePicMain)
        {
            System.Drawing.Point f = this.PointToClient(PIC_Main.PointToScreen(ePicMain));
            PictureBox best = null;
            double bestD = 28;
            foreach (PictureBox pb in new[] { TopLeft, TopRight, BottomLeft, BottomRight, TopCenter, BottomCenter, CenterLeft, CenterRight })
            {
                double cx = pb.Location.X + pb.Width / 2.0;
                double cy = pb.Location.Y + pb.Height / 2.0;
                double d = Math.Sqrt((f.X - cx) * (f.X - cx) + (f.Y - cy) * (f.Y - cy));
                if (d < bestD) { bestD = d; best = pb; }
            }
            return best;
        }

        private void PIC_Main_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox hit = HitTest(e.Location);
            if (hit == null)
                return;
            activePoint = hit;
            dragging = true;
            if (hit == TopLeft) TopLeftMouseDown(PIC_Main, e);
            else if (hit == TopRight) TopRightMouseDown(PIC_Main, e);
            else if (hit == BottomLeft) BottomLeftMouseDown(PIC_Main, e);
            else if (hit == BottomRight) BottomRightMouseDown(PIC_Main, e);
            else if (hit == TopCenter) TopCenterMouseDown(PIC_Main, e);
            else if (hit == BottomCenter) BottomCenterMouseDown(PIC_Main, e);
            else if (hit == CenterLeft) CenterLeftMouseDown(PIC_Main, e);
            else if (hit == CenterRight) CenterRightMouseDown(PIC_Main, e);
            PIC_Main.Invalidate();
        }

        private void PIC_Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging)
                return;
            TopLeftMouseMove(PIC_Main, e); TopRightMouseMove(PIC_Main, e);
            BottomLeftMouseMove(PIC_Main, e); BottomRightMouseMove(PIC_Main, e);
            TopCenterMouseMove(PIC_Main, e); BottomCenterMouseMove(PIC_Main, e);
            CenterLeftMouseMove(PIC_Main, e); CenterRightMouseMove(PIC_Main, e);
            PIC_Main.Invalidate();
        }

        private void PIC_Main_MouseUp(object sender, MouseEventArgs e)
        {
            if (!dragging)
                return;
            TopLeftMouseUp(PIC_Main, e); TopRightMouseUp(PIC_Main, e);
            BottomLeftMouseUp(PIC_Main, e); BottomRightMouseUp(PIC_Main, e);
            TopCenterMouseUp(PIC_Main, e); BottomCenterMouseUp(PIC_Main, e);
            CenterLeftMouseUp(PIC_Main, e); CenterRightMouseUp(PIC_Main, e);
            dragging = false;
            activePoint = null;
            PIC_Main.Invalidate();
        }

        private void PointLocationChanged(object sender, EventArgs e)
        {
            // 포인트가 이미지 영역을 벗어나도 허용한다(경계 클램프 없음).
        }

        private void MovePoint(ref PictureBox PointBox, object sender, MouseEventArgs e)
        {
            System.Drawing.Point ClientPos = this.PointToClient(((Control)sender).PointToScreen(e.Location));

            ClientPos.X -= TopLeft.Size.Width / 2;
            ClientPos.Y -= TopLeft.Size.Height / 2;

            PointBox.Location = ClientPos;
        }

        private bool bTopLeftMoveStart = false;
        private void TopLeftMouseDown(object sender, MouseEventArgs e)
        {
            bTopLeftMoveStart = true;            
        }

        private void TopLeftMouseUp(object sender, MouseEventArgs e)
        {
            bTopLeftMoveStart = false;
        }

        private void TopLeftMouseMove(object sender, MouseEventArgs e)
        {
            if (bTopLeftMoveStart)
            {
                MovePoint(ref TopLeft, sender, e);
                MoveCenterPoint(ref TopCenter, TopLeft.Location, TopRight.Location);
                MoveCenterPoint(ref CenterLeft, TopLeft.Location, BottomLeft.Location);

                PIC_Main.Invalidate();
            }
        }

        private bool bTopRightMoveStart = false;
        private void TopRightMouseDown(object sender, MouseEventArgs e)
        {
            bTopRightMoveStart = true;
        }

        private void TopRightMouseUp(object sender, MouseEventArgs e)
        {
            bTopRightMoveStart = false;
        }

        private void TopRightMouseMove(object sender, MouseEventArgs e)
        {
            if (bTopRightMoveStart)
            {
                MovePoint(ref TopRight, sender, e);
                MoveCenterPoint(ref TopCenter, TopLeft.Location, TopRight.Location);
                MoveCenterPoint(ref CenterRight, TopRight.Location, BottomRight.Location);

                PIC_Main.Invalidate();
            }
        }

        private bool bBottomLeftMoveStart = false;
        private void BottomLeftMouseDown(object sender, MouseEventArgs e)
        {
            bBottomLeftMoveStart = true;
        }

        private void BottomLeftMouseUp(object sender, MouseEventArgs e)
        {
            bBottomLeftMoveStart = false;
        }

        private void BottomLeftMouseMove(object sender, MouseEventArgs e)
        {
            if (bBottomLeftMoveStart)
            {
                MovePoint(ref BottomLeft, sender, e);
                MoveCenterPoint(ref BottomCenter, BottomLeft.Location, BottomRight.Location);
                MoveCenterPoint(ref CenterLeft, TopLeft.Location, BottomLeft.Location);

                PIC_Main.Invalidate();
            }
        }

        private bool bBottomRightMoveStart = false;
        private void BottomRightMouseDown(object sender, MouseEventArgs e)
        {
            bBottomRightMoveStart = true;
        }

        private void BottomRightMouseUp(object sender, MouseEventArgs e)
        {
            bBottomRightMoveStart = false;
        }

        private void BottomRightMouseMove(object sender, MouseEventArgs e)
        {
            if (bBottomRightMoveStart)
            {
                MovePoint(ref BottomRight, sender, e);
                MoveCenterPoint(ref BottomCenter, BottomLeft.Location, BottomRight.Location);
                MoveCenterPoint(ref CenterRight, TopRight.Location, BottomRight.Location);

                PIC_Main.Invalidate();
            }
        }

        private void MoveCenterPoint(ref PictureBox PointBox, System.Drawing.Point p1, System.Drawing.Point p2)
        {
            PointBox.Location = new System.Drawing.Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        private void MoveCenterPoint(ref PictureBox PointBox, object sender, MouseEventArgs e, System.Drawing.Point p1, System.Drawing.Point p2)
        {
            System.Drawing.Point Center = new System.Drawing.Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            Vec2f bidirection = new Vec2f(p1.Y - p2.Y, p2.X - p1.X);

            System.Drawing.Point ClientPos = this.PointToClient(((Control)sender).PointToScreen(e.Location));

            ClientPos.X -= TopLeft.Size.Width / 2;
            ClientPos.Y -= TopLeft.Size.Height / 2;

            if (bidirection.Item0 == 0)
            {
                ClientPos.X = Center.X;
            }
            else
            {
                float gradient = bidirection.Item1 / bidirection.Item0;

                if (-1 < gradient && gradient < 1)
                    ClientPos.Y = (int)(gradient * (ClientPos.X - Center.X)) + Center.Y;
                else
                    ClientPos.X = (int)(1 / gradient * (ClientPos.Y - Center.Y)) + Center.X;
                
            }

            PointBox.Location = ClientPos;
        }
        private void MoveNeighborPoint(ref PictureBox PointBox0, object sender, MouseEventArgs e, System.Drawing.Point BasePoint1, System.Drawing.Point BasePoint2, ref PictureBox PointBox1, ref PictureBox PointBox2)
        {
            System.Drawing.Point OldPos = PointBox0.Location;
            MoveCenterPoint(ref PointBox0, sender, e, BasePoint1, BasePoint2);

            System.Drawing.Point Delta = new System.Drawing.Point(PointBox0.Location.X - OldPos.X, PointBox0.Location.Y - OldPos.Y);

            PointBox1.Location = new System.Drawing.Point(PointBox1.Location.X + Delta.X, PointBox1.Location.Y + Delta.Y);
            PointBox2.Location = new System.Drawing.Point(PointBox2.Location.X + Delta.X, PointBox2.Location.Y + Delta.Y);
        }

        private bool bTopLineMove = false;
        private void TopCenterMouseDown(object sender, MouseEventArgs e)
        {
            // 중앙점은 양옆 코너와 함께 이동(변 전체 이동).
            bTopLineMove = true;
            BaseP1 = TopLeft.Location;
            BaseP2 = TopRight.Location;
        }

        private void TopCenterMouseUp(object sender, MouseEventArgs e)
        {
            bTopLineMove = false;
        }

        private void TopCenterMouseMove(object sender, MouseEventArgs e)
        {
            if (bTopLineMove)
            {
                MoveNeighborPoint(ref TopCenter, sender, e, BaseP1, BaseP2, ref TopLeft, ref TopRight);
                MoveCenterPoint(ref CenterLeft, TopLeft.Location, BottomLeft.Location);
                MoveCenterPoint(ref CenterRight, TopRight.Location, BottomRight.Location);
                PIC_Main.Invalidate();
            }
        }

        private bool bBottomLineMove = false;
        private void BottomCenterMouseDown(object sender, MouseEventArgs e)
        {
            bBottomLineMove = true;
            BaseP1 = BottomLeft.Location;
            BaseP2 = BottomRight.Location;
        }

        private void BottomCenterMouseUp(object sender, MouseEventArgs e)
        {
            bBottomLineMove = false;
        }

        private void BottomCenterMouseMove(object sender, MouseEventArgs e)
        {
            if (bBottomLineMove)
            {
                MoveNeighborPoint(ref BottomCenter, sender, e, BaseP1, BaseP2, ref BottomLeft, ref BottomRight);
                MoveCenterPoint(ref CenterLeft, TopLeft.Location, BottomLeft.Location);
                MoveCenterPoint(ref CenterRight, TopRight.Location, BottomRight.Location);
                PIC_Main.Invalidate();
            }
        }

        private bool bLeftLineMove = false;
        private void CenterLeftMouseDown(object sender, MouseEventArgs e)
        {
            bLeftLineMove = true;
            BaseP1 = TopLeft.Location;
            BaseP2 = BottomLeft.Location;
        }

        private void CenterLeftMouseUp(object sender, MouseEventArgs e)
        {
            bLeftLineMove = false;
        }

        private void CenterLeftMouseMove(object sender, MouseEventArgs e)
        {
            if (bLeftLineMove)
            {
                MoveNeighborPoint(ref CenterLeft, sender, e, BaseP1, BaseP2, ref TopLeft, ref BottomLeft);
                MoveCenterPoint(ref TopCenter, TopLeft.Location, TopRight.Location);
                MoveCenterPoint(ref BottomCenter, BottomLeft.Location, BottomRight.Location);
                PIC_Main.Invalidate();
            }
        }

        private bool bRightLineMove = false;
        private System.Drawing.Point BaseP1 = new System.Drawing.Point();
        private System.Drawing.Point BaseP2 = new System.Drawing.Point();
        private void CenterRightMouseDown(object sender, MouseEventArgs e)
        {
            bRightLineMove = true;
            BaseP1 = TopRight.Location;
            BaseP2 = BottomRight.Location;
        }

        private void CenterRightMouseUp(object sender, MouseEventArgs e)
        {
            bRightLineMove = false;
        }

        private void CenterRightMouseMove(object sender, MouseEventArgs e)
        {
            if(bRightLineMove)
            {
                MoveNeighborPoint(ref CenterRight, sender, e, BaseP1, BaseP2, ref TopRight, ref BottomRight);
                MoveCenterPoint(ref TopCenter,TopLeft.Location, TopRight.Location);
                MoveCenterPoint(ref BottomCenter, BottomLeft.Location, BottomRight.Location);
                PIC_Main.Invalidate();
            }
        }
    }
}

