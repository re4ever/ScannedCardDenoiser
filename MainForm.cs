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
        WrongProperty,
        ErrorWaifu2x
    }
    public partial class MainForm : Form
    {
        private Thread th;
        private volatile bool abortRequested;
        string waifu2xExePath;
        string[] sourceFiles;
        private readonly ToolTip toolTip1 = new ToolTip();

        public MainForm()
        {
            InitializeComponent();

            waifu2xExePath = Path.Combine(Environment.CurrentDirectory, "waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan.exe");
            if (!File.Exists(waifu2xExePath))
            {
                waifu2xExePath = null;
            }
            else
            {
                BTN_waifu2x.Text = waifu2xExePath;
                BTN_waifu2x.Enabled = false;
            }
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

            // waifu2x
            toolTip1.SetToolTip(CB_waifu2x, "처리 후 waifu2x로 업스케일/노이즈 제거를 추가로 실행합니다.");
            toolTip1.SetToolTip(BTN_waifu2x, "waifu2x-ncnn-vulkan.exe 실행 파일 위치를 지정합니다.");
            toolTip1.SetToolTip(CB_Waifu2xTTA, "TTA 모드: 품질이 조금 오르지만 처리 시간이 크게 늘어납니다.");
            toolTip1.SetToolTip(RB_WaifuCunet, "CUNet 모델(일반적으로 일러스트/카드에 무난).");
            toolTip1.SetToolTip(RB_WaifuAnime, "애니메이션 스타일용 모델.");
            toolTip1.SetToolTip(RB_WaifuPhoto, "사진용 모델.");
            toolTip1.SetToolTip(RB_WaifuLow, "노이즈 제거 레벨: 낮음.");
            toolTip1.SetToolTip(RB_WaifuMed, "노이즈 제거 레벨: 중간.");
            toolTip1.SetToolTip(RB_WaifuHigh, "노이즈 제거 레벨: 높음.");
            toolTip1.SetToolTip(RB_WaifuHighest, "노이즈 제거 레벨: 최고.");

            // 실행/미리보기
            toolTip1.SetToolTip(BTN_Execute, "설정한 옵션으로 전체 처리를 시작합니다.");
            toolTip1.SetToolTip(BTN_Abort, "진행 중인 처리를 중단합니다.");
            toolTip1.SetToolTip(BTN_ShowPreview, "현재 원본과 옵션으로 한 장을 처리해 미리보기 창에 보여줍니다.");
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
                // waifu2x
                "Waifu2x=" + CB_waifu2x.Checked,
                "Waifu2xTTA=" + CB_Waifu2xTTA.Checked,
                "WaifuModel=" + (RB_WaifuCunet.Checked ? "Cunet" : RB_WaifuAnime.Checked ? "Anime" : "Photo"),
                "WaifuLevel=" + (RB_WaifuLow.Checked ? "Low" : RB_WaifuMed.Checked ? "Med" : RB_WaifuHigh.Checked ? "High" : "Highest"),
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

            setBool("Waifu2x", CB_waifu2x);
            setBool("Waifu2xTTA", CB_Waifu2xTTA);
            if (map.TryGetValue("WaifuModel", out string waifuModel))
            {
                RB_WaifuCunet.Checked = waifuModel == "Cunet";
                RB_WaifuAnime.Checked = waifuModel == "Anime";
                RB_WaifuPhoto.Checked = waifuModel == "Photo";
            }
            if (map.TryGetValue("WaifuLevel", out string waifuLevel))
            {
                RB_WaifuLow.Checked = waifuLevel == "Low";
                RB_WaifuMed.Checked = waifuLevel == "Med";
                RB_WaifuHigh.Checked = waifuLevel == "High";
                RB_WaifuHighest.Checked = waifuLevel == "Highest";
            }

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
            if (CB_waifu2x.Checked && waifu2xExePath == null)
            {
                if (!LinkWaifu2X())
                    return;
            }

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

            // Manual 보정도 폴더(다중 파일)에서 사용 가능하다. 각 파일마다 순서대로 다이얼로그가 뜬다.

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

            PipelineContext ctx = BuildPipelineContext(filepath);

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

            if (CB_waifu2x.Checked)
            {
                Process ps = new Process();

                ps.StartInfo.FileName = waifu2xExePath;
                ps.StartInfo.CreateNoWindow = true;
                ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                string modelPath = " -m ";
                if (RB_WaifuAnime.Checked)
                    modelPath += "models-upconv_7_anime_style_art_rgb";
                else if (RB_WaifuPhoto.Checked)
                    modelPath += "models-upconv_7_photo";
                else if (RB_WaifuCunet.Checked)
                    modelPath += "models-cunet";

                string denoiseLevel = " -n ";
                if (RB_WaifuLow.Checked)
                    denoiseLevel += "0";
                else if (RB_WaifuMed.Checked)
                    denoiseLevel += "1";
                else if (RB_WaifuHigh.Checked)
                    denoiseLevel += "2";
                else if (RB_WaifuHighest.Checked)
                    denoiseLevel += "3";

                ps.StartInfo.Arguments = "-i " + newFileName + " -o " + newFileName + modelPath + denoiseLevel + " -s 2";

                if (CB_Waifu2xTTA.Checked)
                    ps.StartInfo.Arguments += " -x";

                ps.Start();
                ps.WaitForExit();

                if (File.Exists(newFileName))
                {
                    Image image = Image.FromFile(newFileName);
                    System.Drawing.Size imageSize = image.Size;
                    if(imageSize.Width != Result.Width)
                        imageSize.Width = Result.Width;
                    if (imageSize.Height != Result.Height)
                        imageSize.Height = Result.Height;

                    Bitmap newImage = new Bitmap(image, imageSize);
                    image.Dispose();

                    newImage.Save(newFileName, ImageFormat.Png);
                    newImage.Dispose();
                }
                else
                {
                    ShowWarning(EErrorType.ErrorWaifu2x);
                }

                ps.Dispose();
            }

            Result.Dispose();
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

            if (CB_waifu2x.Checked)
            {
                if (waifu2xExePath == null)
                {
                    if (!LinkWaifu2X())
                    {
                        ShowWarning();
                        return;
                    }
                }

                string tmpFileName = "PreviewTemp.png";

                Cv2.ImWrite("PreviewTemp.png", tmpImage); 

                Process ps = new Process();
                ps.StartInfo.FileName = waifu2xExePath;
                ps.StartInfo.CreateNoWindow = true;
                ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                string modelPath = " -m ";
                if (RB_WaifuAnime.Checked)
                {
                    modelPath += "models-upconv_7_anime_style_art_rgb";
                    Title += "[waifu2x : upconv7_anim / ";
                }
                else if (RB_WaifuPhoto.Checked)
                {
                    modelPath += "models-upconv_7_photo";
                    Title += "[waifu2x : upconv7_photo / ";
                }
                else if (RB_WaifuCunet.Checked)
                {
                    modelPath += "models-cunet";
                    Title += "[waifu2x : upconv7_cunet / ";
                }
                 
                string denoiseLevel = " -n ";
                if (RB_WaifuLow.Checked)
                {
                    denoiseLevel += "0";
                    Title += "Denoise Low]";
                }
                else if (RB_WaifuMed.Checked)
                {
                    denoiseLevel += "1";
                    Title += "Denoise Medium]";
                }
                else if (RB_WaifuHigh.Checked)
                {
                    denoiseLevel += "2";
                    Title += "Denoise High]";
                }
                else if (RB_WaifuHighest.Checked)
                {
                    denoiseLevel += "3";
                    Title += "Denoise Highest]";
                }

                ps.StartInfo.Arguments = "-i " + tmpFileName + " -o " + tmpFileName + modelPath + denoiseLevel + " -s 2";

                ps.Start();
                ps.WaitForExit();

                if (File.Exists(tmpFileName))
                {
                    Image image = Image.FromFile(tmpFileName);
                    System.Drawing.Size imageSize = image.Size;
                    if (imageSize.Width != tmpImage.Width)
                        imageSize.Width = tmpImage.Width;
                    if (imageSize.Height != tmpImage.Height)
                        imageSize.Height = tmpImage.Height;

                    Bitmap newImage = new Bitmap(image, imageSize);
                    image.Dispose();

                    newImage.Save(tmpFileName, ImageFormat.Png);
                    newImage.Dispose();

                    tmpImage = Cv2.ImRead(tmpFileName);

                    File.Delete(tmpFileName);
                }
                else
                {
                    ShowWarning(EErrorType.ErrorWaifu2x);
                }

                ps.Dispose();
            }

            // 원본(좌)과 결과(우)를 각각 별도 창으로 띄우고, 화면 작업영역을 좌/우로 나눠 나란히 배치한다.
            System.Drawing.Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int halfW = wa.Width / 2;
            using (Mat original = Cv2.ImRead(TB_Source.Text))
            {
                ShowPreviewWindow("Original (Before)", original, new System.Drawing.Rectangle(wa.Left, wa.Top, halfW, wa.Height));
            }
            ShowPreviewWindow("Result (After)  " + Title, tmpImage, new System.Drawing.Rectangle(wa.Left + halfW, wa.Top, wa.Width - halfW, wa.Height));
            tmpImage.Dispose();
        }

        private void CB_waifu2x_CheckedChanged(object sender, EventArgs e)
        {
            RB_WaifuAnime.Enabled = CB_waifu2x.Checked;
            RB_WaifuPhoto.Enabled = CB_waifu2x.Checked;
            RB_WaifuCunet.Enabled = CB_waifu2x.Checked;
            RB_WaifuLow.Enabled = CB_waifu2x.Checked;
            RB_WaifuMed.Enabled = CB_waifu2x.Checked;
            RB_WaifuHigh.Enabled = CB_waifu2x.Checked;
            RB_WaifuHighest.Enabled = CB_waifu2x.Checked;
            CB_Waifu2xTTA.Enabled = CB_waifu2x.Checked;
        }

        private void BTN_waifu2x_Click(object sender, EventArgs e)
        {
            LinkWaifu2X();
        }

        private bool LinkWaifu2X()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Filter = "waifu2x exe file|waifu2x-ncnn-vulkan.exe";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                waifu2xExePath = openFileDialog1.FileName;
                BTN_waifu2x.Text = waifu2xExePath;
                return true;
            }

            return false;
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
                case EErrorType.ErrorWaifu2x: System.Windows.Forms.MessageBox.Show("waifu2x 실행 실패", "경고"); break;
            }
            
        }

        private void RB_AutoAdjust_CheckedChanged(object sender, EventArgs e)
        {
            // Threshold는 Auto 크롭과 Manual 초기 4포인트 검출에 모두 쓰이므로 두 모드에서 활성화.
            TB_AdjThreshold.Enabled = RB_AutoAdjust.Checked || RB_ManualAdjust.Checked;
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

        // bounds가 비어있지 않으면 그 위치/크기로 창을 배치한다(좌/우 나란히 비교용).
        private void ShowPreviewWindow(string title, Mat mat, System.Drawing.Rectangle bounds)
        {
            byte[] buf;
            Cv2.ImEncode(".png", mat, out buf);
            Bitmap bmp;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buf))
            using (Bitmap tmp = new Bitmap(ms))
                bmp = new Bitmap(tmp);
            PreviewForm pf = new PreviewForm(title, bmp);
            if (bounds != System.Drawing.Rectangle.Empty)
            {
                pf.StartPosition = FormStartPosition.Manual;
                pf.Bounds = bounds;
            }
            pf.Show();
        }
    }
}
