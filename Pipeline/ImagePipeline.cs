using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // 파이프라인 단계 간 공유되는 설정/상태 스냅샷.
    // Process 시작 시 폼 컨트롤 값을 한 번 읽어 채운다(단계는 이 값만 참조).
    class PipelineContext
    {
        public MainForm Form;
        public string FilePath;

        // 영역 보정(Image Area Adjustment)
        public bool Manual;
        public bool Auto;
        public int AdjThresholdOffset;

        // 리사이즈
        public bool ChangeSize;
        public bool ResizePercent;
        public int ResizeW, ResizeH, ResizePct;

        // 클립(모서리 자르기) — 비활성 시 0으로 채워짐
        public int ClipTop, ClipBottom, ClipLeft, ClipRight;

        // 코너 라운딩 / 테두리
        public bool CornerRounding, EdgeLine;
        public int CornerPercent;

        // 밝기
        public bool BrightnessEnabled;
        public int Brightness;

        // 노이즈 제거
        public bool Denoise;
        public int DenoiseH, DenoiseHColor, DenoiseT, DenoiseS;

        // 오토 레벨
        public bool AutoLevel;
        public int AutoLevelMin, AutoLevelMax;

        // 샤프닝
        public bool Sharpen;
        public int SharpenAmount;

        // ResizeStep이 계산해 ClipStep이 사용하는 값
        public OpenCvSharp.Size PaddedSize;   // 리사이즈 목표(클립 여백 포함)
        public int CircleRadius;              // 코너 라운딩 반경(px)
        public bool Horizontal;               // 가로 방향 여부
    }

    // 이미지 처리 파이프라인의 한 단계.
    // - input을 받아 결과 Mat을 반환한다.
    // - 비활성/변경 없음이면 input을 그대로 반환한다(동일 인스턴스).
    // - 새 Mat을 반환하면 오케스트레이터가 이전 input을 해제한다(단계에서 input을 직접 해제하지 말 것).
    // - 실패(로드 실패, 잘못된 크기 등) 시 null을 반환해 파이프라인을 중단한다.
    interface IImageStep
    {
        Mat Apply(Mat input, PipelineContext ctx);
    }
}
