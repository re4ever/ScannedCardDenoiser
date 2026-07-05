using System;
using System.Windows.Forms;
using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 1) 이미지 로드. Auto/None은 파일을 직접 읽고, Manual은 CamScanForm을 띄워 결과를 받는다.
    // 결과를 RGBA로 변환해 반환한다. 실패 시 null.
    class LoadStep : IImageStep
    {
        public Mat Apply(Mat input, PipelineContext ctx)
        {
            Mat image;
            if (ctx.Manual)
            {
                // 퍼센트 모드는 절대 크기를 지정하지 않고(0) 이후 리사이즈 단계에서 비율로 처리
                bool bSizeByPixel = ctx.ChangeSize && !ctx.ResizePercent;
                int desireLong = bSizeByPixel ? ctx.ResizeW : 0;
                int desireShort = bSizeByPixel ? ctx.ResizeH : 0;

                // 다이얼로그는 반드시 UI 스레드에서 띄운다(배치 실행은 백그라운드 스레드이므로 Invoke).
                Mat manualResult = null;
                ctx.Form.Invoke(new Action(() =>
                {
                    using (CamScanForm camScanForm = new CamScanForm(ctx.FilePath, desireLong, desireShort, ctx.AdjThresholdOffset))
                    {
                        if (camScanForm.ShowDialog() == DialogResult.OK)
                        {
                            manualResult = camScanForm.result;   // 결과 Mat 소유권 이전(복사 불필요)
                            camScanForm.result = null;
                        }
                        else
                        {
                            camScanForm.result?.Dispose();       // 취소 시 로드한 원본 해제
                        }
                    }
                }));
                image = manualResult;
            }
            else
            {
                image = Cv2.ImRead(ctx.FilePath);
            }

            if (image == null)
                return null;

            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2RGBA);
            return image;
        }
    }
}
