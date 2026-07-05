using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 4) 컬러 노이즈 제거(Non-local Means). 비활성 시 입력 그대로 반환.
    class DenoiseStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            if (!ctx.Denoise)
                return input;

            Mat after = new Mat();
            Cv2.FastNlMeansDenoisingColored(
                input,
                after,
                ctx.DenoiseH,
                ctx.DenoiseHColor,
                ctx.DenoiseT,
                ctx.DenoiseS);
            return after;
        }
    }
}
