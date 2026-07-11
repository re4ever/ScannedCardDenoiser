using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Collections.Generic;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Path = System.IO.Path;

namespace ScannedCardDenoiser
{
    enum EErrorType
    {
        WrongProperty
    }
    public partial class MainForm : Form
    {
        private Thread th;

        // Manual 배치 실행에서 1단계(포인트 수집)가 채우는 파일별 코너 포인트 맵.
        // null이면(미리보기 등) LoadStep이 다이얼로그를 띄워 그 자리에서 받는다.
        private Dictionary<string, ManualQuadPoints> manualPointsMap = null;
        private volatile bool abortRequested;
        string[] sourceFiles;
        private readonly ToolTip toolTip1 = new ToolTip();

        // ── 결과 프리뷰 상태 ──
        private Bitmap previewFullBmp = null;       // 결과 전체 해상도(클릭 시 1:1 크롭용)
        private Bitmap previewOrigFullBmp = null;   // 보정 전(기하만 적용) 전체 해상도 — 줌 분할 비교용
        private Bitmap previewResultThumb = null;   // 결과 썸네일(최대 300, 비율 유지)
        private Bitmap zoomOrigCrop = null;         // 줌 뷰의 원본 크롭(결과 크롭과 같은 영역)
        private double zoomSplitFrac = 0.5;         // 줌 뷰 분할선 위치(이미지 폭 대비 0~1)
        private bool zoomSplitDrag = false;
        private string previewMessage = null;       // 썸네일에 표시할 안내 문구(null이면 없음)
        private bool previewBusy = false;           // 갱신 중(스피너 표시)
        private int previewSpinnerAngle = 0;
        // 줌 뷰가 보고 있는 위치(결과 이미지 대비 상대 좌표 0~1). X<0이면 선택 안 됨.
        // 상대 좌표로 기억해서 결과 크기가 바뀌어도 같은 지점을 다시 크롭한다.
        private PointF previewZoomCenter = new PointF(-1f, -1f);
        private System.Drawing.Point previewHover = new System.Drawing.Point(-1, -1);
        private readonly System.Windows.Forms.Timer previewRefreshTimer = new System.Windows.Forms.Timer { Interval = 600 };  // 값 변경 디바운스
        private readonly System.Windows.Forms.Timer previewSpinnerTimer = new System.Windows.Forms.Timer { Interval = 80 };
        private Thread previewThread = null;
        private volatile bool previewRerun = false; // 갱신 중 값이 또 바뀌면 끝나고 한 번 더

        // Manual 미리보기용 코너 포인트('포인트 지정' 버튼으로 설정, 원본 이미지가 바뀌면 초기화)
        private ManualQuadPoints previewManualPoints = null;
        private string previewPointsFile = null;

        public MainForm()
        {
            InitializeComponent();

            previewRefreshTimer.Tick += (s, ev) => { previewRefreshTimer.Stop(); StartPreviewRefresh(); };
            previewSpinnerTimer.Tick += (s, ev) => { previewSpinnerAngle = (previewSpinnerAngle + 24) % 360; PIC_ResultPreview.Invalidate(); };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupTooltips();

            // 이전 실행에서 저장한 입력 경로 복원
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SourcePath))
                TB_Source.Text = Properties.Settings.Default.SourcePath;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.TargetPath))
                TB_Target.Text = Properties.Settings.Default.TargetPath;

            // 이전 실행에서 저장한 보정 옵션 복원
            LoadSettings();

            // 창을 닫을 때(실행하지 않더라도) 현재 설정을 저장
            this.FormClosing += (s, args) => SaveSettings();

            UpdateTBResize();

            // 설정 복원이 끝난 뒤에 값 변경 → 프리뷰 자동 갱신 이벤트를 건다(복원 중 갱신 폭주 방지).
            HookPreviewRefreshEvents();
            SchedulePreviewRefresh();
        }

        // 이미지 결과에 영향을 주는 모든 컨트롤(이미지 보정/편집 그룹)의 값 변경 시 프리뷰를 자동 갱신한다.
        private void HookPreviewRefreshEvents()
        {
            void hook(Control c)
            {
                switch (c)
                {
                    case CheckBox cb: cb.CheckedChanged += (s, e) => SchedulePreviewRefresh(); break;
                    case RadioButton rb: rb.CheckedChanged += (s, e) => SchedulePreviewRefresh(); break;
                    case TextBox tb: tb.TextChanged += (s, e) => SchedulePreviewRefresh(); break;
                    case TrackBar tr: tr.ValueChanged += (s, e) => SchedulePreviewRefresh(); break;
                }
                foreach (Control child in c.Controls)
                    hook(child);
            }
            hook(groupBox1);
            hook(groupBox2);
        }

        // 각 컨트롤에 마우스를 올리면 나오는 도움말(툴팁)을 설정한다.
        private void SetupTooltips()
        {
            toolTip1.AutoPopDelay = 30000;
            toolTip1.InitialDelay = 400;
            toolTip1.ReshowDelay = 100;
            toolTip1.ShowAlways = true;

            // 입출력
            toolTip1.SetToolTip(TB_Source, "처리할 원본 이미지/PDF 파일 또는 폴더 경로입니다.");
            toolTip1.SetToolTip(BTN_OpenFile, "원본 이미지 또는 PDF 파일을 선택합니다(여러 개 선택 가능).");
            toolTip1.SetToolTip(BTN_OpenFolder, "원본이 들어있는 폴더를 선택합니다.");
            toolTip1.SetToolTip(TB_Target, "결과 PNG를 저장할 폴더 경로입니다.");
            toolTip1.SetToolTip(BTN_TargetFolder, "결과를 저장할 폴더를 선택합니다.");
            toolTip1.SetToolTip(CB_SubFolder, "원본이 폴더일 때 하위 폴더까지 재귀적으로 처리합니다.");
            toolTip1.SetToolTip(CB_Overwrite, "이미 같은 이름의 결과 파일이 있어도 다시 처리해 덮어씁니다.\n끄면 결과가 있는 파일은 건너뜁니다.");
            toolTip1.SetToolTip(CB_PdfExtract, "PDF의 각 페이지에 든 임베디드 이미지를 원본 폴더에 '페이지번호'로 추출한 뒤 처리합니다.\n끄면 PDF는 건너뜁니다.");

            // 영역 보정(Image Area Adjustment)
            toolTip1.SetToolTip(RB_NoneAdjust, "회전/크롭 없이 원본 영역을 그대로 사용합니다.");
            toolTip1.SetToolTip(RB_AutoAdjust, "카드 테두리를 자동으로 찾아 기울기를 보정(회전)하고 카드 영역만 잘라냅니다.");
            toolTip1.SetToolTip(RB_ManualAdjust, "파일마다 창을 띄워 네 모서리를 직접 지정합니다.\n폴더(다중 파일) 선택 시 각 파일마다 순서대로 창이 뜹니다.");
            toolTip1.SetToolTip(TB_AdjThreshold,
                "카드 검출 임계값(0=기본). 두 가지를 함께 제어합니다.\n" +
                "1) 카드/배경 분리(Otsu 오프셋): 양수=더 밝은 테두리까지 카드로 포함.\n" +
                "2) 엣지라인 검출 민감도: 양수일수록 Canny/Hough 문턱을 높여\n" +
                "   카드 경계 노이즈로 생기는 엣지를 억제합니다(노이즈 심하면 +20~60).");

            // 밝기
            toolTip1.SetToolTip(CB_Brightness, "밝기 조정 사용 여부. 체크하면 아래 트랙바 값만큼 밝기를 가감합니다.");
            toolTip1.SetToolTip(TB_Brightness, "밝기를 -100~+100 범위로 가감합니다(0=변화 없음). 'Brightness' 체크 시 적용.");

            // 노이즈 제거
            toolTip1.SetToolTip(CB_DenoiseColor, "비지역 평균(Non-local Means) 컬러 노이즈 제거를 적용합니다.");
            toolTip1.SetToolTip(TB_DenoiseH, "밝기(휘도) 노이즈 제거 강도 h. 클수록 노이즈는 줄지만 디테일도 뭉갭니다.");
            toolTip1.SetToolTip(TB_DenoiseHColor, "색상 노이즈 제거 강도 hColor. 클수록 색 얼룩이 줄어듭니다.");
            toolTip1.SetToolTip(TB_DenoiseTSize, "템플릿 창 크기(픽셀). 비교에 사용하는 패치 크기입니다.");
            toolTip1.SetToolTip(TB_DenoiseSSize, "검색 창 크기(픽셀). 클수록 품질이 오르지만 매우 느려집니다.");
            toolTip1.SetToolTip(BTN_DenoiseDefault, "노이즈 제거 파라미터를 기본값으로 되돌립니다.");

            // 오토 레벨
            toolTip1.SetToolTip(CB_AutoLevel, "채널별 히스토그램을 늘려 대비를 자동 보정합니다(오토 레벨).");
            toolTip1.SetToolTip(TB_AutoLevelMin, "하위(어두운 쪽) 클리핑 비율(%). 이 비율만큼 어두운 화소를 검정으로 밀어냅니다.");
            toolTip1.SetToolTip(TB_AutoLevelMax, "상위(밝은 쪽) 클리핑 비율(%). 이 비율만큼 밝은 화소를 흰색으로 밀어냅니다.");
            toolTip1.SetToolTip(BTN_AutoLevelDefault, "오토 레벨 값을 기본값으로 되돌립니다.");

            // 샤프닝
            toolTip1.SetToolTip(CB_Sharpen, "언샤프 마스크로 윤곽을 또렷하게 합니다.");
            toolTip1.SetToolTip(TB_Sharpen, "샤프닝 강도(%). 0=끔. 클수록 강해지며 노이즈도 함께 강조될 수 있습니다.");

            // 크기 변경
            toolTip1.SetToolTip(CB_ChangeSize, "결과 이미지의 크기를 바꿉니다. 끄면 원본(크롭 후) 크기를 유지합니다.");
            toolTip1.SetToolTip(RB_ResizeCustom, "긴 변/짧은 변 픽셀을 직접 지정합니다.");
            toolTip1.SetToolTip(RB_Resize4x6, "절지 비율(1:√2)로 짧은 변을 자동 계산합니다.");
            toolTip1.SetToolTip(RB_ResizeCarddass, "카드다스 비율(86:59)로 짧은 변을 자동 계산합니다.");
            toolTip1.SetToolTip(RB_ResizeTradingCard, "트레이딩카드 비율(89:63)로 짧은 변을 자동 계산합니다.");
            toolTip1.SetToolTip(RB_ResizePercent, "원본(크롭 후) 크기의 비율(%)로 축소/확대합니다. 가로·세로 동일 비율.");
            toolTip1.SetToolTip(TB_ResizeW, "긴 변의 목표 픽셀 크기입니다.");
            toolTip1.SetToolTip(TB_ResizeH, "짧은 변의 픽셀 크기입니다(프리셋에서는 자동 계산됨).");
            toolTip1.SetToolTip(TB_ResizePercent, "리사이즈 비율(%). 100=원본 크기 유지.");

            // 모서리 자르기
            toolTip1.SetToolTip(CB_Clip, "결과 가장자리를 지정 픽셀만큼 잘라냅니다(스캔 여백 제거).");
            toolTip1.SetToolTip(TB_ClipTop, "위쪽에서 잘라낼 픽셀 수.");
            toolTip1.SetToolTip(TB_ClipBottom, "아래쪽에서 잘라낼 픽셀 수.");
            toolTip1.SetToolTip(TB_ClipLeft, "왼쪽에서 잘라낼 픽셀 수.");
            toolTip1.SetToolTip(TB_ClipRight, "오른쪽에서 잘라낼 픽셀 수.");

            // 코너 라운딩 / 테두리
            toolTip1.SetToolTip(CB_CornerRounding, "결과 네 모서리를 둥글게 깎고 바깥을 투명하게 만듭니다.");
            toolTip1.SetToolTip(TB_CornerRounding, "라운딩 반경(짧은 변 대비 %).");
            toolTip1.SetToolTip(CB_EdgeLine, "카드 가장자리에 검은 테두리 선을 그립니다.");

            // 실행/미리보기
            toolTip1.SetToolTip(BTN_SetPoints, "Manual 보정에 사용할 코너 포인트를 다이얼로그로 지정합니다.\n원본 이미지가 바뀌면 초기화됩니다.");
            toolTip1.SetToolTip(BTN_Execute, "설정한 옵션으로 전체 처리를 시작합니다.");
            toolTip1.SetToolTip(BTN_Abort, "진행 중인 처리를 중단합니다.");
            toolTip1.SetToolTip(BTN_ShowPreview, "현재 원본과 옵션으로 한 장을 처리해 결과 창을 띄웁니다.\n결과 창에서는 원본|결과가 겹쳐 있고 분할선을 드래그해 비교할 수 있습니다.");
        }

        // 현재 폼의 모든 보정 옵션을 한 문자열로 직렬화해 사용자 설정에 저장한다.
        private void SaveSettings()
        {
            var lines = new List<string>
            {
                // Image Area Adjustment
                "AdjMode=" + (RB_AutoAdjust.Checked ? "Auto" : RB_ManualAdjust.Checked ? "Manual" : "None"),
                "AdjThreshold=" + TB_AdjThreshold.Text,
                // Brightness
                "BrightnessOn=" + CB_Brightness.Checked,
                "Brightness=" + TB_Brightness.Value,
                // Denoise
                "DenoiseColor=" + CB_DenoiseColor.Checked,
                "DenoiseH=" + TB_DenoiseH.Text,
                "DenoiseHColor=" + TB_DenoiseHColor.Text,
                "DenoiseTSize=" + TB_DenoiseTSize.Text,
                "DenoiseSSize=" + TB_DenoiseSSize.Text,
                // AutoLevel
                "AutoLevel=" + CB_AutoLevel.Checked,
                "AutoLevelMin=" + TB_AutoLevelMin.Text,
                "AutoLevelMax=" + TB_AutoLevelMax.Text,
                // Sharpen
                "Sharpen=" + CB_Sharpen.Checked,
                "SharpenAmount=" + TB_Sharpen.Value,
                // 사이즈 변경(ChangeSize/ResizeMode/W/H/Percent)은 의도적으로 저장하지 않음
                // Clip
                "Clip=" + CB_Clip.Checked,
                "ClipTop=" + TB_ClipTop.Text,
                "ClipBottom=" + TB_ClipBottom.Text,
                "ClipLeft=" + TB_ClipLeft.Text,
                "ClipRight=" + TB_ClipRight.Text,
                // Corner / Edge
                "CornerRounding=" + CB_CornerRounding.Checked,
                "CornerRadius=" + TB_CornerRounding.Text,
                "EdgeLine=" + CB_EdgeLine.Checked,
                // misc
                "SubFolder=" + CB_SubFolder.Checked,
                "Overwrite=" + CB_Overwrite.Checked,
                "PdfExtract=" + CB_PdfExtract.Checked,
            };

            Properties.Settings.Default.Preferences = string.Join("\n", lines);
            Properties.Settings.Default.SourcePath = TB_Source.Text;
            Properties.Settings.Default.TargetPath = TB_Target.Text;
            Properties.Settings.Default.Save();
        }

        // 저장된 보정 옵션 문자열을 파싱해 폼 컨트롤에 적용한다.
        private void LoadSettings()
        {
            string pref = Properties.Settings.Default.Preferences;
            if (string.IsNullOrEmpty(pref))
                return;

            var map = new Dictionary<string, string>();
            foreach (var line in pref.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = line.IndexOf('=');
                if (eq <= 0)
                    continue;
                map[line.Substring(0, eq)] = line.Substring(eq + 1);
            }

            // 체크박스/텍스트박스/트랙바를 안전하게 복원하는 보조 함수들
            Action<string, CheckBox> setBool = (k, cb) =>
            {
                if (map.TryGetValue(k, out string v) && bool.TryParse(v, out bool b)) cb.Checked = b;
            };
            // 숫자 텍스트박스: 빈 값은 무시(Convert.ToInt32 예외 방지)
            Action<string, TextBox> setText = (k, tb) =>
            {
                if (map.TryGetValue(k, out string v) && !string.IsNullOrEmpty(v)) tb.Text = v;
            };
            Action<string, TrackBar> setTrack = (k, tbar) =>
            {
                if (map.TryGetValue(k, out string v) && int.TryParse(v, out int n))
                    tbar.Value = Math.Max(tbar.Minimum, Math.Min(tbar.Maximum, n));
            };

            // Image Area Adjustment
            if (map.TryGetValue("AdjMode", out string adjMode))
            {
                RB_AutoAdjust.Checked = adjMode == "Auto";
                RB_ManualAdjust.Checked = adjMode == "Manual";
                RB_NoneAdjust.Checked = adjMode == "None";
            }
            setText("AdjThreshold", TB_AdjThreshold);
            setBool("BrightnessOn", CB_Brightness);
            setTrack("Brightness", TB_Brightness);
            setBool("DenoiseColor", CB_DenoiseColor);
            setText("DenoiseH", TB_DenoiseH);
            setText("DenoiseHColor", TB_DenoiseHColor);
            setText("DenoiseTSize", TB_DenoiseTSize);
            setText("DenoiseSSize", TB_DenoiseSSize);
            setBool("AutoLevel", CB_AutoLevel);
            setText("AutoLevelMin", TB_AutoLevelMin);
            setText("AutoLevelMax", TB_AutoLevelMax);
            setBool("Sharpen", CB_Sharpen);
            setTrack("SharpenAmount", TB_Sharpen);

            // 사이즈 변경(ChangeSize/ResizeMode/W/H/Percent)은 복원하지 않고 매번 기본값 사용

            setBool("Clip", CB_Clip);
            setText("ClipTop", TB_ClipTop);
            setText("ClipBottom", TB_ClipBottom);
            setText("ClipLeft", TB_ClipLeft);
            setText("ClipRight", TB_ClipRight);

            setBool("CornerRounding", CB_CornerRounding);
            setText("CornerRadius", TB_CornerRounding);
            setBool("EdgeLine", CB_EdgeLine);


            setBool("SubFolder", CB_SubFolder);
            setBool("Overwrite", CB_Overwrite);
            setBool("PdfExtract", CB_PdfExtract);

            // 트랙바는 Value를 코드로 바꿔도 Scroll 이벤트가 안 나므로 라벨을 직접 갱신
            lbl_BrightnessVal.Text = TB_Brightness.Value.ToString();
            lbl_SharpenVal.Text = "Sharpen : " + TB_Sharpen.Value.ToString() + "%";
        }

        private void BTN_OpenFile_Click(object sender, EventArgs e)
        {            
            if (TB_Source.Text.Length > 0)
                openFileDialog.InitialDirectory = Path.GetDirectoryName(TB_Source.Text);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_Source.Clear();

                TB_Source.Text = openFileDialog.FileName;
                sourceFiles = openFileDialog.FileNames;
            }
        }

        private void BTN_OpenFolder_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (TB_Source.Text.Length > 0)
                dialog.InitialDirectory = Path.GetDirectoryName(TB_Source.Text);

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TB_Source.Clear();

                TB_Source.Text = dialog.FileName;
            }
        }

        private void BTN_TargetFolder_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (TB_Target.Text.Length > 0)
                dialog.InitialDirectory = Path.GetDirectoryName(TB_Target.Text);

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TB_Target.Clear();

                TB_Target.Text = dialog.FileName;
            }
        }

        private void BTN_AutoLevelDefault_Click(object sender, EventArgs e)
        {
            TB_AutoLevelMax.Text = "0";
            TB_AutoLevelMin.Text = "5";
        }

        private void BTN_DenoiseDefault_Click(object sender, EventArgs e)
        {
            TB_DenoiseH.Text = "10";
            TB_DenoiseHColor.Text = "3";
            TB_DenoiseTSize.Text = "21";
            TB_DenoiseSSize.Text = "3";
        }

        private void CB_AutoLevel_CheckedChanged(object sender, EventArgs e)
        {
            TB_AutoLevelMax.Enabled = CB_AutoLevel.Checked;
            TB_AutoLevelMin.Enabled = CB_AutoLevel.Checked;
            BTN_AutoLevelDefault.Enabled = CB_AutoLevel.Checked;
        }

        private void CB_Sharpen_CheckedChanged(object sender, EventArgs e)
        {
            TB_Sharpen.Enabled = CB_Sharpen.Checked;
        }

        private void CB_Brightness_CheckedChanged(object sender, EventArgs e)
        {
            TB_Brightness.Enabled = CB_Brightness.Checked;
        }

        private void CB_DenoiseColor_CheckedChanged(object sender, EventArgs e)
        {
            TB_DenoiseH.Enabled = CB_DenoiseColor.Checked;
            TB_DenoiseHColor.Enabled = CB_DenoiseColor.Checked;
            TB_DenoiseTSize.Enabled = CB_DenoiseColor.Checked;
            TB_DenoiseSSize.Enabled = CB_DenoiseColor.Checked;
            BTN_DenoiseDefault.Enabled = CB_DenoiseColor.Checked;
        }

        private void CB_Clip_CheckedChanged(object sender, EventArgs e)
        {
            TB_ClipTop.Enabled = CB_Clip.Checked;
            TB_ClipBottom.Enabled = CB_Clip.Checked;
            TB_ClipLeft.Enabled = CB_Clip.Checked;
            TB_ClipRight.Enabled = CB_Clip.Checked;
        }

        private void CB_ChangeSize_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void UpdateTBResize()
        {
            RB_ResizeCustom.Enabled = CB_ChangeSize.Checked;
            RB_Resize4x6.Enabled = CB_ChangeSize.Checked;
            RB_ResizeCarddass.Enabled = CB_ChangeSize.Checked;
            RB_ResizeTradingCard.Enabled = CB_ChangeSize.Checked;
            RB_ResizePercent.Enabled = CB_ChangeSize.Checked;

            bool bPercent = RB_ResizePercent.Checked;

            // 퍼센트 모드: 비율 입력칸만 사용, 픽셀(Long/Short) 입력은 비활성
            TB_ResizePercent.Enabled = CB_ChangeSize.Checked && bPercent;
            TB_ResizeW.Enabled = CB_ChangeSize.Checked && !bPercent;
            TB_ResizeH.Enabled = CB_ChangeSize.Checked && !bPercent && RB_ResizeCustom.Checked;

            if (RB_Resize4x6.Checked)
            {
                TB_ResizeH.Text = Convert.ToString((int)(Convert.ToInt32(TB_ResizeW.Text) / Math.Sqrt(2)));
            }
            else if (RB_ResizeCarddass.Checked)
            {
                TB_ResizeH.Text = Convert.ToString((int)(Convert.ToInt32(TB_ResizeW.Text) * 59 / 86));
            }
            else if (RB_ResizeTradingCard.Checked)
            {
                TB_ResizeH.Text = Convert.ToString((int)(Convert.ToInt32(TB_ResizeW.Text) * 63 / 89));
            }
        }

        private void RB_ResizeCustom_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void RB_ResizePercent_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void RB_Resize4x6_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void RB_ResizeCarddass_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void RB_ResizeTradingCard_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void CB_CornerRounding_CheckedChanged(object sender, EventArgs e)
        {
            TB_CornerRounding.Enabled = CB_CornerRounding.Checked;
        }

        private void BTN_Execute_Click(object sender, EventArgs e)
        {
            // 이번 실행에 입력한 경로와 보정 옵션을 다음 실행을 위해 저장
            SaveSettings();

            BTN_Execute.Enabled = false;
            BTN_Abort.Enabled = true;
            PB_Progress.Value = 0;

            // 이전 작업이 아직 살아있다면 중단 요청 후 종료를 기다린다.
            if (th != null && th.IsAlive)
            {
                abortRequested = true;
                th.Join();
            }

            abortRequested = false;
            th = new Thread(Execute);
            th.IsBackground = true;
            th.Start();
        }

        private void BTN_Abort_Click(object sender, EventArgs e)
        {
            abortRequested = true;
            BTN_Abort.Enabled = false;
        }

        private void TB_Target_TextChanged(object sender, EventArgs e)
        {
            UpdateExecuteButton();
        }

        private void TB_Source_TextChanged(object sender, EventArgs e)
        {
            UpdateExecuteButton();
            UpdatePreviewButton();

            // 원본 이미지가 바뀌면 Manual 코너 포인트를 초기화하고 프리뷰를 다시 만든다.
            previewManualPoints = null;
            previewPointsFile = null;
            SchedulePreviewRefresh();
        }

        // Manual 코너 포인트를 다이얼로그로 지정한다(프리뷰/단건 처리에 사용).
        private void BTN_SetPoints_Click(object sender, EventArgs e)
        {
            string file = GetPreviewSourceFile();
            if (file == null)
            {
                System.Windows.Forms.MessageBox.Show("원본 이미지(파일 또는 폴더)를 먼저 지정하세요.", "포인트 지정");
                return;
            }

            bool bSizeByPixel = CB_ChangeSize.Checked && !RB_ResizePercent.Checked;
            int desireLong = bSizeByPixel ? ParseInt(TB_ResizeW.Text) : 0;
            int desireShort = bSizeByPixel ? ParseInt(TB_ResizeH.Text) : 0;

            using (CamScanForm cam = new CamScanForm(file, desireLong, desireShort, ParseInt(TB_AdjThreshold.Text)))
            {
                if (cam.ShowDialog() == DialogResult.OK)
                {
                    previewManualPoints = cam.QuadPoints;
                    previewPointsFile = file;
                    SchedulePreviewRefresh();
                }
                cam.result?.Dispose();
                cam.result = null;
            }
        }

        private void UpdateExecuteButton()
        {
            BTN_Execute.Enabled = TB_Target.Text.Length > 0 && TB_Source.Text.Length > 0;

            if (!Directory.Exists(TB_Source.Text) && !File.Exists(TB_Source.Text))
                BTN_Execute.Enabled &= false;

            if (th != null)
            {
                BTN_Execute.Enabled = !th.IsAlive;
                BTN_Abort.Enabled = th.IsAlive;
            }
            else
            {
                BTN_Execute.Enabled &= true;
                BTN_Abort.Enabled = false;
            }
        }
        
        private void Execute()
        {
            if (!Directory.Exists(TB_Target.Text))
            {
                Directory.CreateDirectory(TB_Target.Text);
            }

            bool bSelectedFiles = false;
            string[] files;
            if (File.Exists(TB_Source.Text))
            {
                // sourceFiles는 파일 선택 다이얼로그로만 채워진다. 설정에서 경로를 복원하거나
                // 직접 입력한 경우엔 null/불일치이므로 현재 경로 단일 파일로 처리한다.
                files = (sourceFiles != null && sourceFiles.Contains(TB_Source.Text))
                    ? sourceFiles
                    : new string[] { TB_Source.Text };
                bSelectedFiles = true;
            }
            else if (Directory.Exists(TB_Source.Text))
            {
                if (CB_SubFolder.Checked)
                    files = Directory.GetFiles(TB_Source.Text, "*.*", SearchOption.AllDirectories);
                else
                    files = Directory.GetFiles(TB_Source.Text, "*.*", SearchOption.TopDirectoryOnly);
            }
            else
            {
                files = new string[] { };
            }

            // 원본 위치를 미러링해 출력 폴더(dst)를 구하는 규칙(기존 로직과 동일).
            Func<string, string> defaultDst = (filepath) =>
            {
                string d = TB_Target.Text + filepath.Substring(TB_Source.Text.Length);
                if (!bSelectedFiles)
                    d = d.Remove(d.LastIndexOf('\\'));
                return d;
            };

            // PDF 확장: .pdf는 페이지별 임베디드 이미지를 원본 폴더에 '페이지번호.png'로 추출하고,
            // 그 이미지들을 처리 대상으로 삼는다. 각 추출본의 출력 폴더는 원래 PDF 위치를 미러링한다.
            var workFiles = new List<string>();
            var dstMap = new Dictionary<string, string>();
            foreach (string f in files)
            {
                if (f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    if (!CB_PdfExtract.Checked)
                        continue; // 체크 해제 시 PDF는 처리하지 않음

                    string pdfDst = defaultDst(f);
                    foreach (string img in ExtractPdfImages(f))
                    {
                        workFiles.Add(img);
                        dstMap[img] = pdfDst;
                    }
                }
                else
                {
                    workFiles.Add(f);
                }
            }
            files = workFiles.ToArray();

            // 처리 대상별 출력 폴더: PDF 추출본은 매핑값, 그 외에는 기본 규칙.
            Func<string, string> getDst = (filepath) =>
                dstMap.TryGetValue(filepath, out string d) ? d : defaultDst(filepath);

            if (!CB_Overwrite.Checked)
            {
                List<string> newFiles = new List<string>();
                foreach (string filepath in files)
                {
                    string dstDirectory = getDst(filepath);
                    string newFileName = dstDirectory + '\\' + filepath.Split('\\').Last().Split('.')[0] + ".png";
                    if (!File.Exists(newFileName))
                    {
                        newFiles.Add(filepath);
                    }
                }
                files = newFiles.ToArray();
            }

            // Manual 보정: 1단계에서 모든 파일의 코너 포인트를 순서대로 지정받아 저장하고,
            // 2단계(아래 처리 루프)에서 저장된 포인트로 디노이즈 등 파이프라인을 순차 실행한다.
            manualPointsMap = null;
            if (RB_ManualAdjust.Checked)
            {
                bool bSizeByPixel = CB_ChangeSize.Checked && !RB_ResizePercent.Checked;
                int desireLong = bSizeByPixel ? ParseInt(TB_ResizeW.Text) : 0;
                int desireShort = bSizeByPixel ? ParseInt(TB_ResizeH.Text) : 0;
                int thresholdOffset = ParseInt(TB_AdjThreshold.Text);

                var pointsMap = new Dictionary<string, ManualQuadPoints>();
                var confirmedFiles = new List<string>();
                foreach (string filepath in files)
                {
                    if (abortRequested)
                        break;

                    ManualQuadPoints points = null;
                    this.Invoke(new Action(() =>
                    {
                        using (CamScanForm camScanForm = new CamScanForm(filepath, desireLong, desireShort, thresholdOffset))
                        {
                            if (camScanForm.ShowDialog() == DialogResult.OK)
                                points = camScanForm.QuadPoints;
                            camScanForm.result?.Dispose();   // 수집 단계에서는 원본 Mat이 필요 없다
                            camScanForm.result = null;
                        }
                    }));

                    if (points != null)
                    {
                        pointsMap[filepath] = points;
                        confirmedFiles.Add(filepath);
                    }
                    // 취소(X)한 파일은 처리 대상에서 제외
                }
                files = confirmedFiles.ToArray();
                manualPointsMap = pointsMap;
            }

            Label_Progress.Invoke(new Action(() =>
            {
                Label_Progress.Text = "0 / " + files.Length.ToString();
                Label_Progress.Text += " ( 0.0s / 0.0s )";
                Label_Progress.Visible = true;
            }));

            PB_Progress.Invoke(new Action(() =>
            {
                PB_Progress.Enabled = true;
            }));

            int StartTick = Environment.TickCount;
            int DoneCount = 0;
            foreach (string filepath in files)
            {
                if (abortRequested)
                    break;

                string dstDirectory = getDst(filepath);

                if (!Directory.Exists(dstDirectory))
                {
                    Directory.CreateDirectory(dstDirectory);
                }

                Process(filepath, dstDirectory);
                DoneCount++;

                int CurrentTick = Environment.TickCount;
                double ElapsedTime = (CurrentTick - StartTick) * 0.001;
                double ExpectTime = (ElapsedTime / DoneCount) * (files.Length);
                Label_Progress.Invoke(new Action(() =>
                    {
                        Label_Progress.Text = DoneCount.ToString() + " / " + files.Length.ToString();
                        Label_Progress.Text += " ( " + ElapsedTime.ToString("F1") + "s / " + ExpectTime.ToString("F1") + "s )";
                    }
                ));

                PB_Progress.Invoke(new Action(() =>
                    {
                        PB_Progress.Value = DoneCount * 100 / files.Length;
                    }
                ));
            }

            manualPointsMap = null;   // 다음 실행/미리보기에 이전 포인트가 재사용되지 않도록 정리

            BTN_Execute.Invoke(new Action(() =>
            {
                BTN_Execute.Enabled = true;
                BTN_Abort.Enabled = false;
            }));
        }

        // PDF의 각 페이지에 들어있는 임베디드 이미지를 추출해 PDF와 같은 폴더에 '페이지번호.png'로 저장한다.
        // (한 페이지에 이미지가 여러 개면 '페이지번호_순번'). 저장된 파일 경로 목록을 반환한다.
        private List<string> ExtractPdfImages(string pdfPath)
        {
            var saved = new List<string>();
            string dir = Path.GetDirectoryName(pdfPath);

            try
            {
                using (PdfDocument doc = PdfDocument.Open(pdfPath))
                {
                    int pageNumber = 0;
                    foreach (Page page in doc.GetPages())
                    {
                        pageNumber++;
                        var images = page.GetImages().ToList();
                        int idx = 0;
                        foreach (IPdfImage img in images)
                        {
                            idx++;
                            string baseName = images.Count > 1 ? (pageNumber + "_" + idx) : pageNumber.ToString();

                            // PdfPig가 색공간을 해석해 PNG로 변환해주면 그대로 사용.
                            if (img.TryGetPng(out byte[] png) && png != null && png.Length > 0)
                            {
                                string outPath = Path.Combine(dir, baseName + ".png");
                                File.WriteAllBytes(outPath, png);
                                saved.Add(outPath);
                            }
                            else
                            {
                                // PNG 변환 실패 시(주로 JPEG/DCT) 원본 스트림 바이트를 그대로 저장 시도.
                                byte[] raw = img.RawMemory.ToArray();
                                if (raw == null || raw.Length == 0)
                                    continue;
                                string outPath = Path.Combine(dir, baseName + ".jpg");
                                File.WriteAllBytes(outPath, raw);
                                saved.Add(outPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                    System.Windows.Forms.MessageBox.Show(
                        "PDF 이미지 추출 실패: " + Path.GetFileName(pdfPath) + "\n" + ex.Message,
                        "PDF")));
            }

            return saved;
        }

        private Mat Process(string filepath)
        {
            if (!File.Exists(filepath))
                return null;

            return RunPipeline(BuildPipelineContext(filepath));
        }

        private Mat RunPipeline(PipelineContext ctx)
        {
            // Order: Load -> Denoise -> Brightness -> AutoLevel -> Sharpen -> AreaAdjust -> Resize -> Clip
            IImageStep[] steps =
            {
                new LoadStep(),
                new DenoiseStep(),
                new BrightnessStep(),
                new AutoLevelStep(),
                new SharpenStep(),
                new AreaAdjustStep(),
                new ResizeStep(),
                new ClipStep(),
            };

            Mat cur = null;
            foreach (IImageStep step in steps)
            {
                Mat next = step.Apply(cur, ctx);
                if (next != cur && cur != null)
                    cur.Dispose();
                cur = next;
                if (cur == null)
                    return null;
            }
            return cur;
        }

        // Snapshot the form control values the pipeline steps need.
        private PipelineContext BuildPipelineContext(string filepath)
        {
            return new PipelineContext
            {
                Form = this,
                FilePath = filepath,
                Manual = RB_ManualAdjust.Checked,
                Auto = RB_AutoAdjust.Checked,
                AdjThresholdOffset = ParseInt(TB_AdjThreshold.Text),
                ManualPoints = GetManualPointsFor(filepath),
                ChangeSize = CB_ChangeSize.Checked,
                ResizePercent = RB_ResizePercent.Checked,
                ResizeW = ParseInt(TB_ResizeW.Text),
                ResizeH = ParseInt(TB_ResizeH.Text),
                ResizePct = ParseInt(TB_ResizePercent.Text),
                ClipTop = CB_Clip.Checked ? ParseInt(TB_ClipTop.Text) : 0,
                ClipBottom = CB_Clip.Checked ? ParseInt(TB_ClipBottom.Text) : 0,
                ClipLeft = CB_Clip.Checked ? ParseInt(TB_ClipLeft.Text) : 0,
                ClipRight = CB_Clip.Checked ? ParseInt(TB_ClipRight.Text) : 0,
                CornerRounding = CB_CornerRounding.Checked,
                CornerPercent = ParseInt(TB_CornerRounding.Text),
                EdgeLine = CB_EdgeLine.Checked,
                BrightnessEnabled = CB_Brightness.Checked,
                Brightness = TB_Brightness.Value,
                Denoise = CB_DenoiseColor.Checked,
                DenoiseH = ParseInt(TB_DenoiseH.Text),
                DenoiseHColor = ParseInt(TB_DenoiseHColor.Text),
                DenoiseT = ParseInt(TB_DenoiseTSize.Text),
                DenoiseS = ParseInt(TB_DenoiseSSize.Text),
                AutoLevel = CB_AutoLevel.Checked,
                AutoLevelMin = ParseInt(TB_AutoLevelMin.Text),
                AutoLevelMax = ParseInt(TB_AutoLevelMax.Text),
                Sharpen = CB_Sharpen.Checked,
                SharpenAmount = TB_Sharpen.Value,
            };
        }

        private static int ParseInt(string s)
        {
            int.TryParse(s, out int v);
            return v;
        }

        // Manual 코너 포인트 조회: 배치 수집 맵 → '포인트 지정' 버튼으로 설정한 미리보기 포인트 순.
        private ManualQuadPoints GetManualPointsFor(string filepath)
        {
            if (manualPointsMap != null && manualPointsMap.TryGetValue(filepath, out ManualQuadPoints mp))
                return mp;
            if (previewManualPoints != null && string.Equals(previewPointsFile, filepath, StringComparison.OrdinalIgnoreCase))
                return previewManualPoints;
            return null;
        }

        private void Process(string filepath, string dstDirectory)
        {
            Label_FileName.Invoke(new Action(() => 
            {
                Label_FileName.Visible = true;
                Label_FileName.Text = filepath.Split('\\').Last();
            }));
            
            Mat Result = Process(filepath);
            if (Result == null)
            {
                ShowWarning();
                return;
            }

            string newFileName = dstDirectory + '\\' + filepath.Split('\\').Last().Split('.')[0] + ".png";
            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }

            Cv2.ImWrite(newFileName, Result);

            // 방금 처리한 결과를 메인 폼 프리뷰에 반영(UI 스레드에서, Mat 해제 전에 동기 실행)
            this.Invoke(new Action(() => UpdateResultPreview(Result)));

            Result.Dispose();
        }

        // ── 결과 프리뷰 ──
        // 값 변경 시 디바운스(600ms) 후 백그라운드에서 파이프라인을 실행해 프리뷰를 갱신한다.
        private void SchedulePreviewRefresh()
        {
            previewRefreshTimer.Stop();
            previewRefreshTimer.Start();
        }

        // 프리뷰 대상 원본 파일: 파일이면 그 파일, 폴더면 첫 번째 이미지 파일.
        private static readonly string[] PreviewImageExts = { ".jpg", ".jpeg", ".png", ".bmp" };
        private string GetPreviewSourceFile()
        {
            string src = TB_Source.Text;
            if (File.Exists(src))
                return PreviewImageExts.Contains(Path.GetExtension(src).ToLowerInvariant()) ? src : null;
            if (Directory.Exists(src))
                return Directory.GetFiles(src, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => PreviewImageExts.Contains(Path.GetExtension(f).ToLowerInvariant()));
            return null;
        }

        private void SetPreviewMessage(string msg, bool clearImages)
        {
            previewMessage = msg;
            if (clearImages)
            {
                previewResultThumb?.Dispose(); previewResultThumb = null;
                previewFullBmp?.Dispose(); previewFullBmp = null;
                previewOrigFullBmp?.Dispose(); previewOrigFullBmp = null;
                zoomOrigCrop?.Dispose(); zoomOrigCrop = null;
                previewZoomCenter = new PointF(-1f, -1f);
                Image oldZoom = PIC_ResultZoom.Image;
                PIC_ResultZoom.Image = null;
                oldZoom?.Dispose();
            }
            PIC_ResultPreview.Invalidate();
        }

        // 프리뷰 갱신 시작(UI 스레드). 파이프라인은 백그라운드 스레드에서 돌리고 스피너를 표시한다.
        private void StartPreviewRefresh()
        {
            if (th != null && th.IsAlive)
                return;   // 배치 실행 중에는 파일 처리 결과가 프리뷰를 갱신한다

            string file = GetPreviewSourceFile();
            if (file == null)
            {
                SetPreviewMessage(null, true);
                return;
            }

            if (previewThread != null && previewThread.IsAlive)
            {
                previewRerun = true;   // 진행 중이면 끝난 뒤 한 번 더
                return;
            }

            PipelineContext ctx = BuildPipelineContext(file);
            // 줌 분할 비교용 '보정 전' 이미지: 색/노이즈 보정만 끄고 기하(영역/리사이즈/클립)는 동일하게.
            PipelineContext origCtx = BuildPipelineContext(file);
            origCtx.Denoise = false;
            origCtx.BrightnessEnabled = false;
            origCtx.AutoLevel = false;
            origCtx.Sharpen = false;

            // Manual인데 코너 포인트가 아직 없으면 Auto 검출 결과로 미리보기한다(다이얼로그 팝업 방지).
            if (ctx.Manual && ctx.ManualPoints == null)
            {
                ctx.Manual = false; ctx.Auto = true;
                origCtx.Manual = false; origCtx.Auto = true;
            }

            previewMessage = null;
            previewBusy = true;
            previewSpinnerTimer.Start();
            PIC_ResultPreview.Invalidate();

            previewThread = new Thread(() =>
            {
                Mat result = null, orig = null;
                try
                {
                    result = RunPipeline(ctx);
                    if (result != null)
                        orig = RunPipeline(origCtx);
                }
                catch { /* 잘못된 설정 값 등으로 실패할 수 있다 */ }

                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        previewBusy = false;
                        previewSpinnerTimer.Stop();
                        if (result != null)
                        {
                            UpdateResultPreview(result, orig);
                            result.Dispose();
                            orig?.Dispose();
                        }
                        else
                        {
                            SetPreviewMessage("미리보기 생성 실패 (설정 값을 확인하세요)", false);
                        }
                        PIC_ResultPreview.Invalidate();
                        if (previewRerun)
                        {
                            previewRerun = false;
                            SchedulePreviewRefresh();
                        }
                    }));
                }
                catch
                {
                    result?.Dispose();   // 폼이 닫히는 중이면 무시
                    orig?.Dispose();
                }
            });
            previewThread.IsBackground = true;
            previewThread.Start();
        }

        // 처리 결과를 최대 300x300(비율 유지) 썸네일로 만들고,
        // 클릭 줌용 전체 해상도 비트맵(+분할 비교용 보정 전 이미지)을 보관한다. UI 스레드에서 호출할 것.
        private void UpdateResultPreview(Mat result, Mat origResult = null)
        {
            if (result == null || result.Empty())
                return;

            Bitmap full = MatToBitmap(result);

            double scale = Math.Min(1.0, Math.Min(300.0 / full.Width, 300.0 / full.Height));
            int tw = Math.Max(1, (int)Math.Round(full.Width * scale));
            int th = Math.Max(1, (int)Math.Round(full.Height * scale));
            Bitmap thumb = new Bitmap(full, tw, th);

            previewFullBmp?.Dispose();
            previewFullBmp = full;
            previewOrigFullBmp?.Dispose();
            previewOrigFullBmp = (origResult != null && !origResult.Empty()) ? MatToBitmap(origResult) : null;
            previewResultThumb?.Dispose();
            previewResultThumb = thumb;
            previewMessage = null;

            // 줌 뷰는 초기화하지 않고, 보고 있던 위치를 새 결과로 다시 크롭해 함께 갱신한다.
            RefreshZoomView();

            PIC_ResultPreview.Invalidate();
        }

        // Mat(1/3/4채널) → 32bppArgb Bitmap. 파이프라인의 4채널 데이터는 BGRA 순서라 그대로 복사한다.
        private static Bitmap MatToBitmap(Mat m)
        {
            Mat src = m;
            Mat converted = null;
            if (m.Channels() == 3)
            {
                converted = new Mat();
                Cv2.CvtColor(m, converted, ColorConversionCodes.BGR2BGRA);
                src = converted;
            }
            else if (m.Channels() == 1)
            {
                converted = new Mat();
                Cv2.CvtColor(m, converted, ColorConversionCodes.GRAY2BGRA);
                src = converted;
            }

            Bitmap bmp = new Bitmap(src.Cols, src.Rows, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, src.Cols, src.Rows),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int rowBytes = src.Cols * 4;
            byte[] row = new byte[rowBytes];
            for (int y = 0; y < src.Rows; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(src.Ptr(y), row, 0, rowBytes);
                System.Runtime.InteropServices.Marshal.Copy(row, 0, System.IntPtr.Add(bd.Scan0, y * bd.Stride), rowBytes);
            }
            bmp.UnlockBits(bd);
            converted?.Dispose();
            return bmp;
        }

        private void PIC_ResultPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (previewResultThumb == null)
                return;
            previewHover = e.Location;
            PIC_ResultPreview.Invalidate();
        }

        private void PIC_ResultPreview_MouseLeave(object sender, EventArgs e)
        {
            previewHover = new System.Drawing.Point(-1, -1);
            PIC_ResultPreview.Invalidate();
        }

        // 썸네일 그리기: 결과 이미지 + (호버 시) 줌 뷰가 1:1로 보여줄 수 있는 영역만큼의 프레임,
        // 갱신 중에는 중앙에 프로그레스 서클을 표시한다.
        private void PIC_ResultPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (previewResultThumb != null)
            {
                g.DrawImage(previewResultThumb, 0, 0, previewResultThumb.Width, previewResultThumb.Height);

                bool hoverIn = previewHover.X >= 0
                    && previewHover.X < previewResultThumb.Width
                    && previewHover.Y < previewResultThumb.Height;

                if (hoverIn)
                {
                    // 프레임 크기 = PIC_ResultZoom이 실제 해상도(1:1)로 보여줄 수 있는 최대 영역의 썸네일 환산 크기
                    System.Drawing.Size f = GetHoverFrameSize();
                    using (Pen pen = new Pen(Color.Red, 1f))
                        g.DrawRectangle(pen, previewHover.X - f.Width / 2, previewHover.Y - f.Height / 2, f.Width, f.Height);
                }
            }

            if (previewMessage != null)
            {
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (SolidBrush br = new SolidBrush(Color.DimGray))
                    g.DrawString(previewMessage, this.Font, br, PIC_ResultPreview.ClientRectangle, sf);
            }

            if (previewBusy)
            {
                // 갱신 중 프로그레스 서클(스피너)
                System.Drawing.Rectangle rc = PIC_ResultPreview.ClientRectangle;
                int r = 18;
                System.Drawing.Rectangle arc = new System.Drawing.Rectangle(rc.Width / 2 - r, rc.Height / 2 - r, 2 * r, 2 * r);
                System.Drawing.Drawing2D.SmoothingMode oldMode = g.SmoothingMode;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush dim = new SolidBrush(Color.FromArgb(90, Color.White)))
                    g.FillRectangle(dim, rc);
                using (Pen ring = new Pen(Color.FromArgb(60, Color.SteelBlue), 5f))
                    g.DrawEllipse(ring, arc);
                using (Pen head = new Pen(Color.SteelBlue, 5f))
                    g.DrawArc(head, arc, previewSpinnerAngle, 100);
                g.SmoothingMode = oldMode;
            }
        }

        // 호버 프레임 크기: 줌 뷰 크기(실제 해상도 픽셀)를 썸네일 좌표로 환산한다.
        private System.Drawing.Size GetHoverFrameSize()
        {
            if (previewResultThumb == null || previewFullBmp == null)
                return new System.Drawing.Size(10, 10);

            int fw = (int)Math.Round((double)PIC_ResultZoom.ClientSize.Width * previewResultThumb.Width / previewFullBmp.Width);
            int fh = (int)Math.Round((double)PIC_ResultZoom.ClientSize.Height * previewResultThumb.Height / previewFullBmp.Height);
            fw = Math.Max(6, Math.Min(previewResultThumb.Width, fw));
            fh = Math.Max(6, Math.Min(previewResultThumb.Height, fh));
            return new System.Drawing.Size(fw, fh);
        }

        // 클릭: 클릭 지점을 상대 좌표로 기억하고 줌 뷰를 갱신한다.
        // (상대 좌표라 이후 썸네일이 갱신돼도 같은 지점을 계속 보여준다)
        private void PIC_ResultPreview_MouseClick(object sender, MouseEventArgs e)
        {
            Image thumb = previewResultThumb;
            if (thumb == null || previewFullBmp == null)
                return;
            if (e.X >= thumb.Width || e.Y >= thumb.Height)
                return;

            previewZoomCenter = new PointF((float)e.X / thumb.Width, (float)e.Y / thumb.Height);
            RefreshZoomView();
        }

        // 기억해 둔 상대 좌표를 현재 결과(previewFullBmp)에 적용해, 줌 뷰가 담을 수 있는
        // 최대 영역을 실제 해상도(1:1)로 잘라 표시한다.
        private void RefreshZoomView()
        {
            if (previewZoomCenter.X < 0 || previewFullBmp == null)
                return;

            int cw = Math.Min(previewFullBmp.Width, PIC_ResultZoom.ClientSize.Width);
            int ch = Math.Min(previewFullBmp.Height, PIC_ResultZoom.ClientSize.Height);
            int cx = (int)Math.Round(previewZoomCenter.X * previewFullBmp.Width) - cw / 2;
            int cy = (int)Math.Round(previewZoomCenter.Y * previewFullBmp.Height) - ch / 2;
            cx = Math.Max(0, Math.Min(previewFullBmp.Width - cw, cx));
            cy = Math.Max(0, Math.Min(previewFullBmp.Height - ch, cy));

            Bitmap crop = new Bitmap(cw, ch, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(crop))
                g.DrawImage(previewFullBmp,
                    new System.Drawing.Rectangle(0, 0, cw, ch),
                    new System.Drawing.Rectangle(cx, cy, cw, ch),
                    GraphicsUnit.Pixel);

            // 분할 비교용 원본 크롭(같은 영역). 크기가 결과와 다르면(설정 불일치) 비교를 끈다.
            zoomOrigCrop?.Dispose();
            zoomOrigCrop = null;
            if (previewOrigFullBmp != null
                && previewOrigFullBmp.Width == previewFullBmp.Width
                && previewOrigFullBmp.Height == previewFullBmp.Height)
            {
                zoomOrigCrop = new Bitmap(cw, ch, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(zoomOrigCrop))
                    g.DrawImage(previewOrigFullBmp,
                        new System.Drawing.Rectangle(0, 0, cw, ch),
                        new System.Drawing.Rectangle(cx, cy, cw, ch),
                        GraphicsUnit.Pixel);
            }
            PIC_ResultZoom.Cursor = zoomOrigCrop != null ? Cursors.VSplit : Cursors.Default;

            Image oldZoom = PIC_ResultZoom.Image;
            PIC_ResultZoom.Image = crop;
            oldZoom?.Dispose();
            PIC_ResultZoom.Invalidate();
        }

        // 줌 뷰 분할 비교: 분할선 왼쪽 = 보정 전(기하만 적용), 오른쪽 = 결과. 드래그로 위치 조절.
        private void PIC_ResultZoom_Paint(object sender, PaintEventArgs e)
        {
            Image img = PIC_ResultZoom.Image;
            if (img == null || zoomOrigCrop == null)
                return;

            int offX = (PIC_ResultZoom.ClientSize.Width - img.Width) / 2;
            int offY = (PIC_ResultZoom.ClientSize.Height - img.Height) / 2;
            int split = Math.Max(0, Math.Min(img.Width, (int)Math.Round(img.Width * zoomSplitFrac)));

            if (split > 0)
                e.Graphics.DrawImage(zoomOrigCrop,
                    new System.Drawing.Rectangle(offX, offY, split, img.Height),
                    new System.Drawing.Rectangle(0, 0, split, img.Height),
                    GraphicsUnit.Pixel);

            using (Pen div = new Pen(Color.Yellow, 1.5f))
                e.Graphics.DrawLine(div, offX + split, offY, offX + split, offY + img.Height);
        }

        private void PIC_ResultZoom_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && zoomOrigCrop != null)
            {
                zoomSplitDrag = true;
                UpdateZoomSplit(e.X);
            }
        }

        private void PIC_ResultZoom_MouseMove(object sender, MouseEventArgs e)
        {
            if (zoomSplitDrag)
                UpdateZoomSplit(e.X);
        }

        private void PIC_ResultZoom_MouseUp(object sender, MouseEventArgs e)
        {
            zoomSplitDrag = false;
        }

        private void UpdateZoomSplit(int x)
        {
            Image img = PIC_ResultZoom.Image;
            if (img == null)
                return;
            int offX = (PIC_ResultZoom.ClientSize.Width - img.Width) / 2;
            zoomSplitFrac = Math.Max(0.0, Math.Min(1.0, (double)(x - offX) / Math.Max(1, img.Width)));
            PIC_ResultZoom.Invalidate();
        }

        private void UpdatePreviewButton()
        {
            BTN_ShowPreview.Enabled = false;

            if (TB_Source.Text.Length > 0)
            {
                if (File.Exists(TB_Source.Text))
                    BTN_ShowPreview.Enabled = true;
            }
        }

        private void BTN_ShowPreview_Click(object sender, EventArgs e)
        {
            // Manual인데 코너 포인트가 없으면 결과/비교용 원본 두 번 모두 다이얼로그가 뜨게 되므로
            // '포인트 지정' 버튼으로 먼저 지정하도록 안내한다.
            if (RB_ManualAdjust.Checked && GetManualPointsFor(TB_Source.Text) == null)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Manual 보정은 '포인트 지정' 버튼으로 코너 포인트를 먼저 지정하세요.", "Show Result");
                return;
            }

            string Title = "";
            if (CB_ChangeSize.Checked)
            {
                Title += "[Resize:" + TB_ResizeW.Text + "x" + TB_ResizeH.Text + "]";
            }
            if (CB_Clip.Checked)
            {
                Title += "[Clip:T" + TB_ClipTop.Text + "B" + TB_ClipBottom.Text + "L" + TB_ClipLeft.Text + "R" + TB_ClipRight.Text + "]";
            }
            if (CB_EdgeLine.Checked)
            {
                Title += "[Edge Line]";
            }
            if (CB_CornerRounding.Checked)
            {
                Title += "[Corner:" + TB_CornerRounding.Text + "]";
            }
            if (CB_AutoLevel.Checked)
            {
                Title += "[AutoLevel:" + TB_AutoLevelMin.Text + "_" + TB_AutoLevelMax.Text + "]";
            }
            if (CB_DenoiseColor.Checked)
            {
                Title += "[Denoise:" + TB_DenoiseH.Text + "_" + TB_DenoiseHColor.Text + "_" + TB_DenoiseTSize.Text + "_" + TB_DenoiseSSize.Text + "]";
            }
            if (RB_AutoAdjust.Checked)
            {
                Title += "[AutoAdjust:" + TB_AdjThreshold.Text + "]";
            }
            if (CB_Brightness.Checked && TB_Brightness.Value != 0)
            {
                Title += "[Brightness:" + TB_Brightness.Value + "]";
            }
            if (CB_Sharpen.Checked && TB_Sharpen.Value > 0)
            {
                Title += "[Sharpen:" + TB_Sharpen.Value + "]";
            }

            Mat tmpImage = Process(TB_Source.Text);
            if(tmpImage == null)
            {
                ShowWarning();
                return;
            }                

            // 원본과 결과를 한 창에 겹쳐 놓고 분할 슬라이더(드래그)로 비교한다.
            // 비교용 '원본'은 색/노이즈 보정 없이 기하(영역 보정 + 리사이즈 + 모서리 크롭)만 적용해
            // 결과와 픽셀 위치가 정확히 대응되도록 만든다. 썸네일/줌 프리뷰에도 함께 반영한다.
            Bitmap origBmp = null;
            {
                PipelineContext origCtx = BuildPipelineContext(TB_Source.Text);
                origCtx.Denoise = false;
                origCtx.BrightnessEnabled = false;
                origCtx.AutoLevel = false;
                origCtx.Sharpen = false;
                Mat origMat = RunPipeline(origCtx);
                UpdateResultPreview(tmpImage, origMat);
                if (origMat != null)
                {
                    origBmp = MatToBitmap(origMat);
                    origMat.Dispose();
                }
            }
            System.Drawing.Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(
                wa.Left + wa.Width / 8, wa.Top, wa.Width * 3 / 4, wa.Height);
            ShowPreviewWindow("Result (원본 | 결과)  " + Title, tmpImage, bounds, origBmp);
            tmpImage.Dispose();
        }

        private void TB_OnlyDigit_KeyPressed(object sender, KeyPressEventArgs e)
        {
            //숫자와 백스페이스만 입력
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ResizeW_TextChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void ShowWarning(EErrorType Err = EErrorType.WrongProperty)
        {
            switch(Err)
            {
                case EErrorType.WrongProperty: System.Windows.Forms.MessageBox.Show("설정 값을 확인하세요.", "경고"); break;
            }
            
        }

        private void RB_AutoAdjust_CheckedChanged(object sender, EventArgs e)
        {
            // Threshold는 Auto 크롭과 Manual 초기 4포인트 검출에 모두 쓰이므로 두 모드에서 활성화.
            TB_AdjThreshold.Enabled = RB_AutoAdjust.Checked || RB_ManualAdjust.Checked;
            BTN_SetPoints.Enabled = RB_ManualAdjust.Checked;
        }

        private void TB_Brightness_Scroll(object sender, EventArgs e)
        {
            lbl_BrightnessVal.Text = TB_Brightness.Value.ToString();
        }

        private void TB_Sharpen_Scroll(object sender, EventArgs e)
        {
            lbl_SharpenVal.Text = "Sharpen : " + TB_Sharpen.Value.ToString() + "%";
        }

        // Mat을 확대 슬라이더가 있는 프리뷰 창으로 표시(큰 이미지는 창에 맞게 축소).
        private void ShowPreviewWindow(string title, Mat mat)
        {
            ShowPreviewWindow(title, mat, System.Drawing.Rectangle.Empty);
        }

        // bounds가 비어있지 않으면 그 위치/크기로 창을 배치한다.
        // compare가 있으면 원본|결과를 겹쳐 분할 슬라이더로 비교하는 창이 된다(compare 소유권은 창으로 이전).
        private void ShowPreviewWindow(string title, Mat mat, System.Drawing.Rectangle bounds, Bitmap compare = null)
        {
            byte[] buf;
            Cv2.ImEncode(".png", mat, out buf);
            Bitmap bmp;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buf))
            using (Bitmap tmp = new Bitmap(ms))
                bmp = new Bitmap(tmp);
            PreviewForm pf = new PreviewForm(title, bmp, compare);
            if (bounds != System.Drawing.Rectangle.Empty)
            {
                pf.StartPosition = FormStartPosition.Manual;
                pf.Bounds = bounds;
            }
            pf.Show();
        }
    }
}
