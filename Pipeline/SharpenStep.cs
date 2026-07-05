using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 6) 샤프닝(언샤프 마스크). 가우시안 블러로 만든 저주파를 빼서 에지를 강조한다.
    // Amount(%)가 클수록 강해진다. 알파(4번째 채널)는 균일(255)하므로 영향 없음. 제자리 수정 후 반환.
    class SharpenStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            if (ctx.Sharpen && ctx.SharpenAmount > 0)
            {
                double amount = ctx.SharpenAmount / 100.0;
                using (Mat blurred = new Mat())
                {
                    Cv2.GaussianBlur(input, blurred, new OpenCvSharp.Size(0, 0), 3);
                    Cv2.AddWeighted(input, 1.0 + amount, blurred, -amount, 0, input);
                }
            }
            return input;
        }
    }
}
