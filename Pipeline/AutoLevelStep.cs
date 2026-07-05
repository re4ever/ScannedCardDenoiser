using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 5) 오토 레벨. CLAHE 적용 후 채널별 히스토그램을 클리핑(Min/Max %)해 대비를 늘린다.
    // 비활성 시 입력 그대로 반환, 활성 시 새 Mat 반환.
    class AutoLevelStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            if (!ctx.AutoLevel)
                return input;
            return AutoLevel(input, ctx.AutoLevelMin, ctx.AutoLevelMax);
        }

        private void GetMinMaxHist(ref Mat Hist, int totalPixel, double LowCut, double HighCut, out int Min, out int Max)
        {
            Min = 0;
            Max = 0;

            float sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += Hist.At<float>(i);
                if (sum >= totalPixel * LowCut * 0.01)
                {
                    Min = i;
                    break;
                }
            }

            sum = 0;
            for (int i = 255; i >= 0; i--)
            {
                sum = sum + Hist.At<float>(i);
                if (sum >= totalPixel * HighCut * 0.01)
                {
                    Max = i;
                    break;
                }
            }
        }

        private void AutoLevelChannelLUT(out byte TableValue, int IndexValue, int Min, int Max)
        {
            if (IndexValue < Min)
            {
                TableValue = 0;
            }
            else
            {
                if (IndexValue > Max || Max == Min)
                {
                    TableValue = 255;
                }
                else
                {
                    float temp = (float)(IndexValue - Min) / (Max - Min);
                    TableValue = (byte)(temp * 255);
                }
            }
        }

        private Mat AutoLevel(Mat src, double LowCut, double HighCut)
        {
            int rows = src.Rows;
            int cols = src.Cols;
            int totalPixel = rows * cols;

            byte[] Pixel = new byte[256 * 4];

            Mat[] rgb;
            Cv2.Split(src, out rgb);
            Mat HistBlue = new Mat();
            Mat HistGreen = new Mat();
            Mat HistRed = new Mat();

            int[] hdims = { 256 };
            Rangef[] ranges = { new Rangef(0, 255) };

            CLAHE clahe = Cv2.CreateCLAHE();
            clahe.Apply(rgb[2], rgb[2]);
            clahe.Apply(rgb[1], rgb[1]);
            clahe.Apply(rgb[0], rgb[0]);

            Cv2.CalcHist(new Mat[] { rgb[2] }, new int[] { 0 }, null, HistRed, 1, hdims, ranges);
            Cv2.CalcHist(new Mat[] { rgb[1] }, new int[] { 0 }, null, HistGreen, 1, hdims, ranges);
            Cv2.CalcHist(new Mat[] { rgb[0] }, new int[] { 0 }, null, HistBlue, 1, hdims, ranges);

            int MinBlue = 0, MaxBlue = 255;
            int MinGreen = 0, MaxGreen = 255;
            int MinRed = 0, MaxRed = 255;

            //Blue Channel
            GetMinMaxHist(ref HistBlue, totalPixel, LowCut, HighCut, out MinBlue, out MaxBlue);
            //Green channel
            GetMinMaxHist(ref HistGreen, totalPixel, LowCut, HighCut, out MinGreen, out MaxGreen);
            //Red channel
            GetMinMaxHist(ref HistRed, totalPixel, LowCut, HighCut, out MinRed, out MaxRed);

            for (int i = 0; i < 256; i++)
            {
                int PixelIndexBase = i * 4;
                Pixel[PixelIndexBase + 3] = 255;

                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 2], i, MinBlue, MaxBlue);
                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 1], i, MinGreen, MaxGreen);
                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 0], i, MinRed, MaxRed);
            }

            Mat dst = new Mat();
            Mat TMP = Mat.FromPixelData(1, 256, MatType.CV_8UC4, Pixel);

            Cv2.LUT(src, TMP, dst);

            rgb[0].Dispose();
            rgb[1].Dispose();
            rgb[2].Dispose();
            if (rgb.Length > 3)
                rgb[3].Dispose();
            HistBlue.Dispose();
            HistGreen.Dispose();
            HistRed.Dispose();
            TMP.Dispose();
            clahe.Dispose();

            return dst;
        }
    }
}
