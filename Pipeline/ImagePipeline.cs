using OpenCvSharp;

namespace ScannedCardDenoiser
{
    // Manual 보정에서 확정한 9개 포인트(전체 해상도 좌표)와 출력 크기.
    // 포인트 수집 단계와 실제 워프(처리) 단계를 분리하기 위해 사용한다.
    // (CamScanForm.cs에 두면 폼 클래스보다 앞선 클래스가 리소스 임베드 이름을 바꿔 크래시하므로 여기에 둔다.)
    public class ManualQuadPoints
    {
        public OpenCvSharp.Vec2f TL, TC, TR, CL, CC, CR, BL, BC, BR;
        public OpenCvSharp.Size ResultSize;
    }

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

        // Manual 배치 실행 시 사전 수집 단계에서 확정한 코너 포인트.
        // null이면 LoadStep이 다이얼로그를 띄워 그 자리에서 받는다(미리보기 등 단건 실행).
        public ManualQuadPoints ManualPoints;

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
