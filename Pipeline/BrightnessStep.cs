using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 3) 밝기 조정. 알파(4번째 채널)는 0으로 두어 변경하지 않는다. 입력을 제자리에서 수정해 반환.
    class BrightnessStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            if (!ctx.BrightnessEnabled)
                return input;

            int brightness = ctx.Brightness;
            if (brightness > 0)
                Cv2.Add(input, new Scalar(brightness, brightness, brightness, 0), input);
            else if (brightness < 0)
                Cv2.Subtract(input, new Scalar(-brightness, -brightness, -brightness, 0), input);
            return input;
        }
    }
}
