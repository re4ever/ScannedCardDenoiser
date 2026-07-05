using System;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 2) Image Area Adjustment(Auto): CardCornerDetector로 카드 4모서리를 검출해 원근(사다리꼴) 보정 크롭.
    // Manual(CamScanForm)과 동일한 검출 로직을 공유한다. Auto가 아니면 입력 그대로 반환. 입력/출력 RGBA.
    class AreaAdjustStep : IImageStep
    {
        public Mat Apply(Mat image, PipelineContext ctx)
        {
            if (!ctx.Auto)
                return image;

            Cv2.CvtColor(image, image, ColorConversionCodes.RGBA2RGB);   // 톤 반영 원본(회전하지 않음)

            // 검출은 '원본(raw)'에서 수행해 Manual 초기 위치와 정확히 일치시킨다.
            Mat detSrc = Cv2.ImRead(ctx.FilePath, ImreadModes.Color);
            bool ownDet = detSrc != null && !detSrc.Empty() && detSrc.Cols == image.Cols && detSrc.Rows == image.Rows;
            if (!ownDet) { detSrc?.Dispose(); detSrc = image; }   // 폴백: 톤 이미지로 검출

            Point2f[] cor;
            bool ok = CardCornerDetector.DetectOriginalCorners(detSrc, ctx.AdjThresholdOffset, true, out cor);
            if (ownDet) detSrc.Dispose();

            Mat outImg;
            if (ok)
            {
                // 4모서리 → 직사각형으로 원근 보정. 출력 크기는 마주보는 변의 최대 길이로 산출.
                Point2f tl = cor[0], tr = cor[1], br = cor[2], bl = cor[3];
                int outW = (int)Math.Round(Math.Max(CardCornerDetector.Dist(tl, tr), CardCornerDetector.Dist(bl, br)));
                int outH = (int)Math.Round(Math.Max(CardCornerDetector.Dist(tl, bl), CardCornerDetector.Dist(tr, br)));
                if (outW < 1) outW = 1;
                if (outH < 1) outH = 1;
                Point2f[] src = { tl, tr, br, bl };
                Point2f[] dst =
                {
                    new Point2f(0, 0), new Point2f(outW - 1, 0),
                    new Point2f(outW - 1, outH - 1), new Point2f(0, outH - 1)
                };
                outImg = new Mat();
                using (Mat PM = Cv2.GetPerspectiveTransform(src, dst))
                    Cv2.WarpPerspective(image, outImg, PM, new OpenCvSharp.Size(outW, outH));
                // 원본(image=input)은 오케스트레이터가 해제한다. 여기서 해제하지 않는다.
            }
            else
            {
                outImg = image;   // 검출 실패 → 크롭하지 않고 원본 유지
            }

#if DEBUG
            CardCornerDetector.DbgShow("5) Cropped result", outImg);
            Cv2.DestroyAllWindows();
#endif

            Cv2.CvtColor(outImg, outImg, ColorConversionCodes.RGB2RGBA);
            return outImg;
        }
    }
}
