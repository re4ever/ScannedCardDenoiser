using System;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 7) 리사이즈. 목표 크기(설정 + 종횡비)에 클립 여백을 더한 크기로 축소/확대한다.
    // 코너 라운딩 반경과 클립 포함 크기를 ctx에 저장해 다음(ClipStep)이 사용한다. 잘못된 크기면 null.
    class ResizeStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            int Wimg = input.Cols, Himg = input.Rows;
            bool bHorizontal = Wimg > Himg;
            ctx.Horizontal = bHorizontal;

            int X, Y;
            if (ctx.ChangeSize && ctx.ResizePercent)
            {
                // 퍼센트 모드: 원본(보정/크롭 후) 크기의 비율로 축소/확대
                double pct = ctx.ResizePct * 0.01;
                X = (int)Math.Round(Wimg * pct);
                Y = (int)Math.Round(Himg * pct);
            }
            else
            {
                X = ctx.ChangeSize ? (bHorizontal ? ctx.ResizeW : ctx.ResizeH) : Wimg;
                Y = ctx.ChangeSize ? (bHorizontal ? ctx.ResizeH : ctx.ResizeW) : Himg;
            }

            if (X * Y == 0)
                return null;
            if (X * Y > 268435456)
                return null;

            OpenCvSharp.Size size = new OpenCvSharp.Size(X, Y);

            // 코너 라운딩 반경은 클립 여백을 더하기 전(목표) 크기 기준으로 계산한다.
            int circleRadius = 0;
            if (ctx.CornerRounding)
                circleRadius = (int)((double)(bHorizontal ? size.Width : size.Height) * 0.01 * ctx.CornerPercent);

            size.Width += ctx.ClipLeft + ctx.ClipRight;
            size.Height += ctx.ClipTop + ctx.ClipBottom;

            ctx.PaddedSize = size;
            ctx.CircleRadius = circleRadius;

            Mat original = new Mat();
            Cv2.Resize(input, original, size);
            return original;
        }
    }
}
