using System;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 8) 클립(모서리 자르기) + 코너 라운딩 + 테두리 그리기 → 최종 RGBA 결과 생성.
    // 리사이즈된 입력(ctx.PaddedSize 크기)에서 클립 여백을 잘라내며 픽셀별로 결과를 채운다.
    class ClipStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            OpenCvSharp.Size size = ctx.PaddedSize;
            int ClipTop = ctx.ClipTop, ClipBottom = ctx.ClipBottom, ClipLeft = ctx.ClipLeft, ClipRight = ctx.ClipRight;
            int circleRadius = ctx.CircleRadius;
            bool bCornerRounding = ctx.CornerRounding;
            bool bEdgeLine = ctx.EdgeLine;

            OpenCvSharp.Size ResultSize = new OpenCvSharp.Size(size.Width - ClipLeft - ClipRight, size.Height - ClipTop - ClipBottom);
            Mat Result = new Mat(ResultSize, MatType.CV_8UC4);

            IntPtr pSrcBitmap = input.Data;
            IntPtr pDstBitmap = Result.Data;

            for (int i = 0, j = 0; i < size.Width * size.Height; ++i)
            {
                int HeightIdx = i / size.Width;
                int WidthIdx = i % size.Width;
                if (HeightIdx < ClipTop || HeightIdx > (size.Height - ClipBottom - 1) || WidthIdx < ClipLeft || WidthIdx > (size.Width - ClipRight - 1))
                    continue;

                unsafe
                {
                    IntPtr currentSrcPixel = IntPtr.Add(pSrcBitmap, i * 4);
                    IntPtr currentDstPixel = IntPtr.Add(pDstBitmap, j++ * 4);

                    uint* pSrcPixel = (uint*)currentSrcPixel.ToPointer();
                    uint* pDstPixel = (uint*)currentDstPixel.ToPointer();

                    if (bCornerRounding && IsOutside(WidthIdx, HeightIdx, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius))
                    {
                        *pDstPixel = 0x00000000;

                        if (bEdgeLine)
                        {
                            if (!IsOutside(WidthIdx - 1, HeightIdx, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                            !IsOutside(WidthIdx + 1, HeightIdx, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                            !IsOutside(WidthIdx, HeightIdx - 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                            !IsOutside(WidthIdx, HeightIdx + 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius))
                            {
                                *pDstPixel = 0xff000000;
                            }
                            else if (!IsOutside(WidthIdx - 1, HeightIdx - 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                                !IsOutside(WidthIdx - 1, HeightIdx + 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                                !IsOutside(WidthIdx + 1, HeightIdx - 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius) ||
                                !IsOutside(WidthIdx + 1, HeightIdx + 1, size, ClipTop, ClipBottom, ClipLeft, ClipRight, circleRadius))
                            {
                                *pDstPixel = 0xff000000;
                            }
                        }
                    }
                    else if (bEdgeLine && ((HeightIdx == ClipTop || HeightIdx == (size.Height - ClipBottom - 1) || WidthIdx == ClipLeft || WidthIdx == (size.Width - ClipRight - 1))))
                    {
                        *pDstPixel = 0xff000000;
                    }
                    else
                    {
                        uint temp = input.At<uint>(HeightIdx, WidthIdx);
                        *pDstPixel = temp | 0xff000000;
                    }
                }
            }

            return Result;
        }

        private bool IsOutside(int WidthIdx, int HeightIdx, OpenCvSharp.Size size, int ClipTop, int ClipBottom, int ClipLeft, int ClipRight, int circleRadius)
        {
            return (WidthIdx < circleRadius && HeightIdx < circleRadius && Math.Sqrt((WidthIdx - circleRadius - ClipLeft) * (WidthIdx - circleRadius - ClipLeft) + (HeightIdx - circleRadius - ClipTop) * (HeightIdx - circleRadius - ClipTop)) > circleRadius) ||
                     (WidthIdx > size.Width - circleRadius && HeightIdx < circleRadius && Math.Sqrt((WidthIdx - size.Width + circleRadius + ClipRight) * (WidthIdx - size.Width + circleRadius + ClipRight) + (HeightIdx - circleRadius - ClipTop) * (HeightIdx - circleRadius - ClipTop)) > circleRadius) ||
                     (WidthIdx < circleRadius && HeightIdx > size.Height - circleRadius && Math.Sqrt((WidthIdx - circleRadius - ClipLeft) * (WidthIdx - circleRadius - ClipLeft) + (HeightIdx - size.Height + circleRadius + ClipBottom) * (HeightIdx - size.Height + circleRadius + ClipBottom)) > circleRadius) ||
                     (WidthIdx > size.Width - circleRadius && HeightIdx > size.Height - circleRadius && Math.Sqrt((WidthIdx - size.Width + circleRadius + ClipRight) * (WidthIdx - size.Width + circleRadius + ClipRight) + (HeightIdx - size.Height + circleRadius + ClipBottom) * (HeightIdx - size.Height + circleRadius + ClipBottom)) > circleRadius);
        }
    }
}
