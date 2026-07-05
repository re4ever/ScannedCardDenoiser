using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 카드 4모서리 검출(공용): Auto(AreaAdjustStep)와 Manual(CamScanForm)이 공유한다.
    // deskew(기울기 보정) → 어두운 카드 마스크 → 각 변 64샘플 로버스트 직선 → 교점 4모서리 → 원본 좌표로 역회전.
    static class CardCornerDetector
    {
        // raw BGR에서 카드 4모서리를 '원본(회전 전) 좌표'(TL,TR,BR,BL)로 검출. 성공 시 true.
        // 검출 실패 시 deskew된 전체 프레임(크롭 없음)으로 폴백. showDebug=true면 DEBUG에서 단계별 프리뷰.
        public static bool DetectOriginalCorners(Mat bgr, int thresholdOffset, bool showDebug, out Point2f[] cornersOrig)
        {
            cornersOrig = null;
            int W = bgr.Cols, H = bgr.Rows;
            if (W < 8 || H < 8)
                return false;

            double rotAngle = ComputeDeskewAngle(bgr);

            Mat rot = new Mat();
            Mat rotMat = Cv2.GetRotationMatrix2D(new Point2f(W * 0.5f, H * 0.5f), rotAngle, 1.0);
            if (rotAngle != 0)
                Cv2.WarpAffine(bgr, rot, rotMat, new Size(W, H), InterpolationFlags.Linear, BorderTypes.Replicate);
            else
                bgr.CopyTo(rot);
#if DEBUG
            if (showDebug) DbgShow("1) Deskew", rot);
#endif

            // 어두운 카드 마스크: Auto Level(흑점 50%) → Otsu(+offset) → morphology → 255 클로징(×50).
            Point[] quad = null;
            using (Mat enh = ApplyAutoLevel(rot, 50, 0))
            using (Mat gray = new Mat())
            using (Mat mask = new Mat())
            {
#if DEBUG
                if (showDebug) DbgShow("2) Auto Level (50/0)", enh);
#endif
                Cv2.CvtColor(enh, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new Size(5, 5), 0);
                double tO = Cv2.Threshold(gray, mask, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
                if (thresholdOffset != 0)
                {
                    double t = Math.Max(0, Math.Min(255, tO + thresholdOffset));
                    Cv2.Threshold(gray, mask, t, 255, ThresholdTypes.BinaryInv);
                }
#if DEBUG
                if (showDebug) DbgShow("3) Dark mask (Otsu+offset)", mask);
#endif
                int mk = Math.Max(3, Math.Min(W, H) / 200);
                if (mk % 2 == 0) mk++;
                using (Mat se = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(mk, mk)))
                {
                    Cv2.MorphologyEx(mask, mask, MorphTypes.Open, se);
                    Cv2.MorphologyEx(mask, mask, MorphTypes.Close, se);
                }
#if DEBUG
                if (showDebug) DbgShow("4) Morphology (open/close)", mask);
#endif
                // 카드 내부 구멍 메움(십자 커널 클로징 ×50)으로 실루엣을 solid하게.
                using (Mat cross = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3)))
                {
                    Cv2.Dilate(mask, mask, cross, null, 50);
                    Cv2.Erode(mask, mask, cross, null, 50);
                }
#if DEBUG
                if (showDebug) DbgShow("5) Close 255 holes (x50)", mask);
#endif
                // 각 변 64샘플 → 로버스트 직선 → 교점 4모서리(회전 프레임 좌표).
                ProjectionQuad(mask, showDebug, out quad);
            }

#if DEBUG
            if (showDebug)
            {
                using (Mat boxImg = rot.Clone())
                {
                    if (quad != null)
                    {
                        Cv2.Polylines(boxImg, new[] { quad }, true, new Scalar(0, 0, 255), 3);
                        foreach (Point p in quad) Cv2.Circle(boxImg, p, 6, new Scalar(0, 255, 255), -1);
                    }
                    DbgShow("7) Box quad (result)", boxImg);
                }
                Cv2.DestroyAllWindows();
            }
#endif
            rot.Dispose();

            // 검출된 4모서리(회전 프레임) → 원본 좌표로 역회전. 실패 시 전체 프레임(=deskew만).
            Point2f[] cr;
            if (quad != null)
                cr = new[] { new Point2f(quad[0].X, quad[0].Y), new Point2f(quad[1].X, quad[1].Y), new Point2f(quad[2].X, quad[2].Y), new Point2f(quad[3].X, quad[3].Y) };
            else
                cr = new[] { new Point2f(0, 0), new Point2f(W - 1, 0), new Point2f(W - 1, H - 1), new Point2f(0, H - 1) };

            cornersOrig = new Point2f[4];
            if (rotAngle != 0)
            {
                using (Mat Minv = new Mat())
                {
                    Cv2.InvertAffineTransform(rotMat, Minv);
                    double a = Minv.At<double>(0, 0), b = Minv.At<double>(0, 1), tx = Minv.At<double>(0, 2);
                    double c = Minv.At<double>(1, 0), d = Minv.At<double>(1, 1), ty = Minv.At<double>(1, 2);
                    for (int i = 0; i < 4; i++)
                        cornersOrig[i] = new Point2f((float)(a * cr[i].X + b * cr[i].Y + tx), (float)(c * cr[i].X + d * cr[i].Y + ty));
                }
            }
            else
            {
                for (int i = 0; i < 4; i++) cornersOrig[i] = cr[i];
            }
            rotMat.Dispose();
            return true;
        }

        // 두 점 사이 거리(원근 보정 출력 크기 산출용).
        public static double Dist(Point2f a, Point2f b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // 어두운 마스크에서 각 변을 64개 샘플로 검출해 로버스트(Huber) 직선을 피팅하고,
        // 인접 직선의 교점 4개(TL,TR,BR,BL)를 카드 4모서리로 반환한다.
        private static unsafe bool ProjectionQuad(Mat mask, bool showDebug, out Point[] quad)
        {
            quad = null;
            int W = mask.Cols, H = mask.Rows;
            byte* d = (byte*)mask.Data.ToPointer(); int step = (int)mask.Step();

            // 마스크(카드) 대략 경계
            int L0 = W, T0 = H, R0 = -1, B0 = -1;
            for (int y = 0; y < H; ++y)
            {
                byte* row = d + y * step;
                for (int x = 0; x < W; ++x)
                    if (row[x] != 0) { if (x < L0) L0 = x; if (x > R0) R0 = x; if (y < T0) T0 = y; if (y > B0) B0 = y; }
            }
            if (R0 <= L0 || B0 <= T0) return false;
            int cw = R0 - L0, ch = B0 - T0;
            int skipX = (int)(cw * 0.05), skipY = (int)(ch * 0.05);
            int x0 = L0 + skipX, x1 = R0 - skipX, y0 = T0 + skipY, y1 = B0 - skipY;
            if (x1 <= x0 || y1 <= y0) return false;

            var top = new List<Point2f>(); var bot = new List<Point2f>();
            var lft = new List<Point2f>(); var rgt = new List<Point2f>();
            const int N = 64;
            for (int i = 0; i < N; ++i)
            {
                int x = x0 + (int)((long)(x1 - x0) * i / (N - 1));
                int yt = -1; for (int y = 0; y < H; ++y) if (d[y * step + x] != 0) { yt = y; break; }
                int yb = -1; for (int y = H - 1; y >= 0; --y) if (d[y * step + x] != 0) { yb = y; break; }
                if (yt >= 0) top.Add(new Point2f(x, yt));
                if (yb >= 0) bot.Add(new Point2f(x, yb));

                int yv = y0 + (int)((long)(y1 - y0) * i / (N - 1));
                int xl = -1; for (int x2 = 0; x2 < W; ++x2) if (d[yv * step + x2] != 0) { xl = x2; break; }
                int xr = -1; for (int x2 = W - 1; x2 >= 0; --x2) if (d[yv * step + x2] != 0) { xr = x2; break; }
                if (xl >= 0) lft.Add(new Point2f(xl, yv));
                if (xr >= 0) rgt.Add(new Point2f(xr, yv));
            }
            if (top.Count < 10 || bot.Count < 10 || lft.Count < 10 || rgt.Count < 10) return false;

            Line2D lt = Cv2.FitLine(top, DistanceTypes.Huber, 0, 0.01, 0.01);
            Line2D lb = Cv2.FitLine(bot, DistanceTypes.Huber, 0, 0.01, 0.01);
            Line2D ll = Cv2.FitLine(lft, DistanceTypes.Huber, 0, 0.01, 0.01);
            Line2D lr = Cv2.FitLine(rgt, DistanceTypes.Huber, 0, 0.01, 0.01);

            Point2f tl = Inter(lt, ll), tr = Inter(lt, lr), br = Inter(lb, lr), bl = Inter(lb, ll);
            quad = new[] { P(tl), P(tr), P(br), P(bl) };

#if DEBUG
            if (showDebug)
            {
                using (Mat vis = new Mat())
                {
                    Cv2.CvtColor(mask, vis, ColorConversionCodes.GRAY2BGR);
                    Scalar cT = new Scalar(0, 0, 255), cB = new Scalar(0, 200, 0), cL = new Scalar(255, 128, 0), cR = new Scalar(0, 165, 255);
                    foreach (Point2f p in top) Cv2.Circle(vis, P(p), 2, cT, -1);
                    foreach (Point2f p in bot) Cv2.Circle(vis, P(p), 2, cB, -1);
                    foreach (Point2f p in lft) Cv2.Circle(vis, P(p), 2, cL, -1);
                    foreach (Point2f p in rgt) Cv2.Circle(vis, P(p), 2, cR, -1);
                    DrawLong(vis, lt, cT); DrawLong(vis, lb, cB); DrawLong(vis, ll, cL); DrawLong(vis, lr, cR);
                    foreach (Point c in quad) Cv2.Circle(vis, c, 6, new Scalar(255, 0, 255), -1);
                    DbgShow("6) 64-sample probes + fitted lines", vis);
                }
            }
#endif
            return true;
        }

        // 두 직선(Line2D: Vx,Vy,X1,Y1)의 교점.
        private static Point2f Inter(Line2D a, Line2D b)
        {
            double dx1 = a.Vx, dy1 = a.Vy, x1 = a.X1, y1 = a.Y1;
            double dx2 = b.Vx, dy2 = b.Vy, x2 = b.X1, y2 = b.Y1;
            double denom = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(denom) < 1e-9) return new Point2f((float)x1, (float)y1);
            double t = ((x2 - x1) * dy2 - (y2 - y1) * dx2) / denom;
            return new Point2f((float)(x1 + t * dx1), (float)(y1 + t * dy1));
        }

        private static Point P(Point2f p) { return new Point((int)Math.Round(p.X), (int)Math.Round(p.Y)); }

        // Auto Level(흑점=lowCutPct, 백점: highCutPct=0이면 255)을 채널별로 적용.
        private static Mat ApplyAutoLevel(Mat bgr, double lowCutPct, double highCutPct)
        {
            Mat[] ch;
            Cv2.Split(bgr, out ch);
            int total = bgr.Rows * bgr.Cols;
            using (CLAHE clahe = Cv2.CreateCLAHE())
                foreach (Mat c in ch) clahe.Apply(c, c);
            foreach (Mat c in ch)
            {
                int mn = 0, mx = 255;
                using (Mat hist = new Mat())
                {
                    Cv2.CalcHist(new[] { c }, new[] { 0 }, null, hist, 1, new[] { 256 }, new[] { new Rangef(0, 256) });
                    float sum = 0;
                    for (int i = 0; i < 256; i++) { sum += hist.At<float>(i); if (sum >= total * lowCutPct * 0.01) { mn = i; break; } }
                    sum = 0;
                    for (int i = 255; i >= 0; i--) { sum += hist.At<float>(i); if (sum >= total * highCutPct * 0.01) { mx = i; break; } }
                }
                byte[] lut = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    if (i <= mn) lut[i] = 0;
                    else if (i >= mx || mx <= mn) lut[i] = 255;
                    else lut[i] = (byte)((float)(i - mn) / (mx - mn) * 255);
                }
                using (Mat lm = Mat.FromPixelData(1, 256, MatType.CV_8UC1, lut))
                    Cv2.LUT(c, lm, c);
            }
            Mat outp = new Mat();
            Cv2.Merge(ch, outp);
            foreach (Mat c in ch) c.Dispose();
            return outp;
        }

        // 카드 테두리 4변의 에지점에 로버스트 직선 피팅을 적용해 기울기(도)를 구한다. 실패 시 0.
        private static unsafe double ComputeDeskewAngle(Mat bgr)
        {
            int W = bgr.Cols, H = bgr.Rows;

            Mat[] chans;
            Cv2.Split(bgr, out chans);
            Mat edge = new Mat(new Size(W, H), MatType.CV_8UC1, Scalar.All(0));
            foreach (Mat c in chans)
            {
                Cv2.GaussianBlur(c, c, new Size(5, 5), 0);
                Cv2.Canny(c, c, 100, 200);
                Cv2.Threshold(c, c, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                Cv2.BitwiseOr(edge, c, edge);
                c.Dispose();
            }

            byte* d = (byte*)edge.Data.ToPointer();

            int[] lh = new int[W], rh = new int[W], th = new int[H], bh = new int[H];
            for (int y = 0; y < H; ++y)
            {
                int l = -1; for (int x = 0; x < W; ++x) if (d[y * W + x] != 0) { l = x; break; }
                if (l < 0) continue;
                int r = W - 1; for (int x = W - 1; x >= 0; --x) if (d[y * W + x] != 0) { r = x; break; }
                lh[l]++; rh[r]++;
            }
            for (int x = 0; x < W; ++x)
            {
                int t = -1; for (int y = 0; y < H; ++y) if (d[y * W + x] != 0) { t = y; break; }
                if (t < 0) continue;
                int b = H - 1; for (int y = H - 1; y >= 0; --y) if (d[y * W + x] != 0) { b = y; break; }
                th[t]++; bh[b]++;
            }
            int rs = Math.Max(5, H / 100), cs = Math.Max(5, W / 100);
            int L = FindRobustBound(lh, true, rs, W), R = FindRobustBound(rh, false, rs, 0);
            int T = FindRobustBound(th, true, cs, H), B = FindRobustBound(bh, false, cs, 0);
            if (!(R > L && B > T)) { edge.Dispose(); return 0; }

            int cw = R - L, chh = B - T;
            int skipH = (int)(cw * 0.12), skipV = (int)(chh * 0.12);
            int bandH = Math.Max(25, chh / 8), bandV = Math.Max(25, cw / 8);
            var top = new List<Point2f>(); var bot = new List<Point2f>();
            var lft = new List<Point2f>(); var rgt = new List<Point2f>();
            for (int x = L + skipH; x <= R - skipH; x += 3)
            {
                for (int y = 0; y < H; ++y) if (d[y * W + x] != 0) { if (y <= T + bandH) top.Add(new Point2f(x, y)); break; }
                for (int y = H - 1; y >= 0; --y) if (d[y * W + x] != 0) { if (y >= B - bandH) bot.Add(new Point2f(x, y)); break; }
            }
            for (int y = T + skipV; y <= B - skipV; y += 3)
            {
                for (int x = 0; x < W; ++x) if (d[y * W + x] != 0) { if (x <= L + bandV) lft.Add(new Point2f(x, y)); break; }
                for (int x = W - 1; x >= 0; --x) if (d[y * W + x] != 0) { if (x >= R - bandV) rgt.Add(new Point2f(x, y)); break; }
            }
            edge.Dispose();

            List<double> angles = new List<double>();
            AddEdgeAngle(top, true, angles);
            AddEdgeAngle(bot, true, angles);
            AddEdgeAngle(lft, false, angles);
            AddEdgeAngle(rgt, false, angles);
            if (angles.Count == 0)
                return 0;
            angles.Sort();
            int m = angles.Count;
            return (m % 2 == 1) ? angles[m / 2] : (angles[m / 2 - 1] + angles[m / 2]) / 2;
        }

        private static void AddEdgeAngle(List<Point2f> pts, bool horizontal, List<double> angles)
        {
            if (pts.Count < 30)
                return;
            Line2D ln = Cv2.FitLine(pts, DistanceTypes.Huber, 0, 0.01, 0.01);
            double vx = ln.Vx, vy = ln.Vy;
            double a;
            if (horizontal)
            {
                if (vx < 0) { vx = -vx; vy = -vy; }
                a = Math.Atan2(vy, vx) * 180 / Math.PI;
            }
            else
            {
                if (vy < 0) { vx = -vx; vy = -vy; }
                a = Math.Atan2(vy, vx) * 180 / Math.PI - 90;
            }
            if (Math.Abs(a) < 20)
                angles.Add(a);
        }

        private static int FindRobustBound(int[] hist, bool fromStart, int threshold, int fallback)
        {
            int sum = 0;
            if (fromStart)
            {
                for (int i = 0; i < hist.Length; ++i) { sum += hist[i]; if (sum >= threshold) return i; }
            }
            else
            {
                for (int i = hist.Length - 1; i >= 0; --i) { sum += hist[i]; if (sum >= threshold) return i; }
            }
            return fallback;
        }

#if DEBUG
        // DEBUG 전용: 직선(Line2D)을 이미지 밖까지 길게 그린다.
        private static void DrawLong(Mat img, Line2D l, Scalar col)
        {
            double t = Math.Max(img.Cols, img.Rows) * 2.0;
            Point a = new Point((int)Math.Round(l.X1 - t * l.Vx), (int)Math.Round(l.Y1 - t * l.Vy));
            Point b = new Point((int)Math.Round(l.X1 + t * l.Vx), (int)Math.Round(l.Y1 + t * l.Vy));
            Cv2.Line(img, a, b, col, 2);
        }

        // DEBUG 전용: 단계 이미지를 축소해 창으로 보여주고 키 입력을 기다린다.
        internal static void DbgShow(string title, Mat m)
        {
            using (Mat disp = new Mat())
            {
                double s = 760.0 / Math.Max(m.Cols, m.Rows);
                if (s < 1.0) Cv2.Resize(m, disp, new Size(Math.Max(1, (int)(m.Cols * s)), Math.Max(1, (int)(m.Rows * s))));
                else m.CopyTo(disp);
                Cv2.ImShow(title, disp);
            }
            Cv2.WaitKey(0);
        }
#endif
    }
}
