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
            // 이전 실행에서 저장한 입력 경로 복원
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SourcePath))
                TB_Source.Text = Properties.Settings.Default.SourcePath;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.TargetPath))
                TB_Target.Text = Properties.Settings.Default.TargetPath;

            UpdateTBResize();
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

            TB_ResizeW.Enabled = CB_ChangeSize.Checked;
            TB_ResizeH.Enabled = CB_ChangeSize.Checked && RB_ResizeCustom.Checked;

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

            // 이번 실행에 입력한 경로를 다음 실행을 위해 저장
            Properties.Settings.Default.SourcePath = TB_Source.Text;
            Properties.Settings.Default.TargetPath = TB_Target.Text;
            Properties.Settings.Default.Save();

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

            if (!CB_Overwrite.Checked)
            {
                List<string> newFiles = new List<string>();
                foreach (string filepath in files)
                {
                    string dstDirectory = TB_Target.Text + filepath.Substring(TB_Source.Text.Length);
                    if (!bSelectedFiles)
                        dstDirectory = dstDirectory.Remove(dstDirectory.LastIndexOf('\\'));

                    string newFileName = dstDirectory + '\\' + filepath.Split('\\').Last().Split('.')[0] + ".png";
                    if (!File.Exists(newFileName))
                    {
                        newFiles.Add(filepath);
                    }
                }
                files = newFiles.ToArray();
            }

            // Manual 보정은 파일마다 다이얼로그를 띄우므로 단일 파일에서만 안전하게 사용한다.
            if (RB_ManualAdjust.Checked && files.Length > 1)
            {
                this.Invoke(new Action(() =>
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Manual 보정은 단일 파일에서만 사용할 수 있습니다.\n원본으로 파일 하나만 선택하세요.",
                        "Manual 모드");
                    BTN_Execute.Enabled = true;
                    BTN_Abort.Enabled = false;
                }));
                return;
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

                string dstDirectory = TB_Target.Text + filepath.Substring(TB_Source.Text.Length);
                if (!bSelectedFiles)
                    dstDirectory = dstDirectory.Remove(dstDirectory.LastIndexOf('\\'));

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

        private Mat Process(string filepath)
        {
            if (!File.Exists(filepath))
                return null;

            Mat image = null;
            if (RB_ManualAdjust.Checked)
            {
                int desireLong = CB_ChangeSize.Checked ? Convert.ToInt32(TB_ResizeW.Text) : 0;
                int desireShort = CB_ChangeSize.Checked ? Convert.ToInt32(TB_ResizeH.Text) : 0;

                // 다이얼로그는 반드시 UI 스레드에서 띄운다(배치 실행은 백그라운드 스레드이므로 Invoke).
                Mat manualResult = null;
                this.Invoke(new Action(() =>
                {
                    using (CamScanForm camScanForm = new CamScanForm(filepath, desireLong, desireShort))
                    {
                        if (camScanForm.ShowDialog() == DialogResult.OK)
                        {
                            manualResult = camScanForm.result;   // 결과 Mat 소유권 이전(복사 불필요)
                            camScanForm.result = null;
                        }
                        else
                        {
                            camScanForm.result?.Dispose(); // 취소 시 로드한 원본 해제
                        }
                    }
                }));
                image = manualResult;
            }
            else
            {
                image = Cv2.ImRead(filepath);
            }
            
            if (image == null)
                return null;

            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2RGBA);
            OpenCvSharp.Size imageSize = new OpenCvSharp.Size(image.Cols, image.Rows);

            if (RB_AutoAdjust.Checked)
            {                
                // 카드 테두리(상/하/좌/우 4변) 에지점에 로버스트 직선 피팅(Huber)을 적용해
                // 기울기를 산출하고 그만큼 회전 보정한다. 어두운 배경 카드에서도 안정적이다.
                Cv2.CvtColor(image, image, ColorConversionCodes.RGBA2RGB);
#if DEBUG
                string newFileName = filepath.Split('\\').Last().Split('.')[0];
#endif
                double rotAngle = ComputeDeskewAngle(image);
                if (rotAngle != 0)
                {
                    Mat RotMat = Cv2.GetRotationMatrix2D(new Point2f(imageSize.Width * 0.5f, imageSize.Height * 0.5f), rotAngle, 1.0);
                    Cv2.WarpAffine(image, image, RotMat, new OpenCvSharp.Size(imageSize.Width, imageSize.Height), InterpolationFlags.Linear, BorderTypes.Replicate);
                    RotMat.Dispose();
                }

                int Top = imageSize.Height; int Bottom = 0; int Left = imageSize.Width; int Right = 0;

                int W = imageSize.Width;
                int H = imageSize.Height;

                // 비닐 슬리브의 빛 산란으로 카드 주위에 밝은 헤일로가 생긴다. 에지가 아니라 밝기로 판단해
                // 밝은 비닐/배경을 지나 '어두운 실제 카드'가 시작하는 경계를 찾는다(Otsu로 카드=255 마스크).
                Mat gray = new Mat();
                Mat darkMask = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.RGB2GRAY);
                Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
                Cv2.Threshold(gray, darkMask, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
                gray.Dispose();

                // 각 행/열에서 처음/마지막으로 '어두워지는'(카드 영역 시작) 위치를 히스토그램으로 모은다
                int[] leftHist = new int[W];
                int[] rightHist = new int[W];
                int[] topHist = new int[H];
                int[] bottomHist = new int[H];

                unsafe
                {
                    byte* data = (byte*)darkMask.Data.ToPointer();

                    // 행별: 처음/마지막으로 어두워지는 열 (좌/우 경계 후보)
                    for (int y = 0; y < H; ++y)
                    {
                        byte* row = data + y * W;
                        int l = -1;
                        for (int x = 0; x < W; ++x) if (row[x] != 0) { l = x; break; }
                        if (l < 0)
                            continue;
                        int r = W - 1;
                        for (int x = W - 1; x >= 0; --x) if (row[x] != 0) { r = x; break; }
                        leftHist[l]++;
                        rightHist[r]++;
                    }

                    // 열별: 처음/마지막으로 어두워지는 행 (상/하 경계 후보)
                    for (int x = 0; x < W; ++x)
                    {
                        int t = -1;
                        for (int y = 0; y < H; ++y) if (data[y * W + x] != 0) { t = y; break; }
                        if (t < 0)
                            continue;
                        int b = H - 1;
                        for (int y = H - 1; y >= 0; --y) if (data[y * W + x] != 0) { b = y; break; }
                        topHist[t]++;
                        bottomHist[b]++;
                    }
                }
                darkMask.Dispose();

                // 비닐 헤일로 안쪽에서 '어두운 카드'가 지배적으로 시작하는 위치를 경계로 삼는다.
                Left = FindDominantBound(leftHist, true, W);
                Right = FindDominantBound(rightHist, false, 0);
                Top = FindDominantBound(topHist, true, H);
                Bottom = FindDominantBound(bottomHist, false, 0);

#if DEBUG
                Mat RectImage = new Mat();
                Cv2.CopyTo(image, RectImage);

                RectImage.Rectangle(new OpenCvSharp.Point(Left, Top), new OpenCvSharp.Point(Right, Bottom), new Scalar(255, 0, 255), 3);

                Cv2.ImShow("Rect", RectImage);
                Cv2.WaitKey(0);

                Cv2.ImWrite(Path.Combine(Environment.CurrentDirectory, newFileName + "_Rect.png"), RectImage);
                RectImage.Dispose();
#endif

                // 어두운 카드 영역을 못 찾으면(빈/단색 이미지) 크롭하지 않고 원본 유지
                if (Right > Left && Bottom > Top)
                {
                    Mat cropped = image.GetRectSubPix(new OpenCvSharp.Size(Right - Left, Bottom - Top), new OpenCvSharp.Point((Right + Left) * 0.5f, (Bottom + Top) * 0.5f));
                    image.Dispose();
                    image = cropped;
                }

                Cv2.CvtColor(image, image, ColorConversionCodes.RGB2RGBA);
                imageSize = new OpenCvSharp.Size(image.Cols, image.Rows);
            }
            
            bool bHorizontal = imageSize.Width > imageSize.Height;

            int X = CB_ChangeSize.Checked ? (bHorizontal ? Convert.ToInt32(TB_ResizeW.Text) : Convert.ToInt32(TB_ResizeH.Text)) : imageSize.Width;
            int Y = CB_ChangeSize.Checked ? (bHorizontal ? Convert.ToInt32(TB_ResizeH.Text) : Convert.ToInt32(TB_ResizeW.Text)) : imageSize.Height;

            if (X * Y == 0)
                return null;

            if (X * Y > 268435456)
                return null;

            OpenCvSharp.Size size = new OpenCvSharp.Size(X, Y);

            int ClipTop = 0, ClipBottom = 0, ClipLeft = 0, ClipRight = 0;
            if (CB_Clip.Checked)
            {
                ClipTop = Convert.ToInt32(TB_ClipTop.Text);
                ClipBottom = Convert.ToInt32(TB_ClipBottom.Text);
                ClipLeft = Convert.ToInt32(TB_ClipLeft.Text);
                ClipRight = Convert.ToInt32(TB_ClipRight.Text);
            }

            int circleRadius = 0;
            if (CB_CornerRounding.Checked)
            {
                circleRadius = (int)((double)(bHorizontal ? size.Width : size.Height) * 0.01 * Convert.ToInt32(TB_CornerRounding.Text));
            }

            size.Width += ClipLeft + ClipRight;
            size.Height += ClipTop + ClipBottom;

            Mat Original = new Mat();
            Cv2.Resize(image, Original, size);

            Mat After = new Mat();
            if (CB_DenoiseColor.Checked)
            {
                Cv2.FastNlMeansDenoisingColored(
                    Original,
                    After,
                    Convert.ToInt32(TB_DenoiseH.Text),
                    Convert.ToInt32(TB_DenoiseHColor.Text),
                    Convert.ToInt32(TB_DenoiseTSize.Text),
                    Convert.ToInt32(TB_DenoiseSSize.Text)
                );
            }
            else
            {
                Cv2.CopyTo(Original, After);
            }

            Mat AutoLevelSrc = new Mat();
            if (CB_AutoLevel.Checked)
            {
                AutoLevelSrc = AutoLevel(After, Convert.ToInt32(TB_AutoLevelMin.Text), Convert.ToInt32(TB_AutoLevelMax.Text));
            }
            else
            {
                Cv2.CopyTo(After, AutoLevelSrc);
            }

            // 밝기 조정(최종 톤 단계). 알파(4번째 채널)는 0으로 두어 변경하지 않는다.
            int brightness = TB_Brightness.Value;
            if (brightness > 0)
                Cv2.Add(AutoLevelSrc, new Scalar(brightness, brightness, brightness, 0), AutoLevelSrc);
            else if (brightness < 0)
                Cv2.Subtract(AutoLevelSrc, new Scalar(-brightness, -brightness, -brightness, 0), AutoLevelSrc);

            OpenCvSharp.Size ResultSize = new OpenCvSharp.Size(size.Width - ClipLeft - ClipRight, size.Height - ClipTop - ClipBottom);
            Mat Result = new Mat(ResultSize, MatType.CV_8UC4);

            IntPtr pSrcBitmap = Original.Data;
            IntPtr pDstBitmap = Result.Data;

            bool bCornerRounding = CB_CornerRounding.Checked;
            bool bEdgeLine = CB_EdgeLine.Checked;

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
                        uint temp = AutoLevelSrc.At<uint>(HeightIdx, WidthIdx);
                        *pDstPixel = temp | 0xff000000;
                    }
                }
            }

            image.Dispose();
            After.Dispose();
            Original.Dispose();
            AutoLevelSrc.Dispose();

            return Result;
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

        private void GetMinMaxHist(ref Mat Hist, int totalPixel, double LowCut, double HighCut, out int Min, out int Max)
        {
            Min = 0;
            Max = 0;

            float sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += Hist.At<float>(i);
                if (sum >= totalPixel * LowCut * 0.01)
                {
                    Min = i;
                    break;
                }
            }

            sum = 0;
            for (int i = 255; i >= 0; i--)
            {
                sum = sum + Hist.At<float>(i);
                if (sum >= totalPixel * HighCut * 0.01)
                {
                    Max = i;
                    break;
                }
            }
        }

        private void AutoLevelChannelLUT(out byte TableValue, int IndexValue, int Min, int Max)
        {
            if (IndexValue < Min)
            {
                TableValue = 0;
            }
            else
            {
                if (IndexValue > Max || Max == Min)
                {
                    TableValue = 255;
                }
                else
                {
                    float temp = (float)(IndexValue - Min) / (Max - Min);
                    TableValue = (byte)(temp * 255);
                }
            }
        }

        private Mat AutoLevel(Mat src, double LowCut, double HighCut)
        {     
            int rows = src.Rows;
            int cols = src.Cols;
            int totalPixel = rows * cols;

            byte[] Pixel = new byte[256 * 4];

            Mat[] rgb;
            Cv2.Split(src, out rgb);
            Mat HistBlue = new Mat();
            Mat HistGreen = new Mat();
            Mat HistRed = new Mat();

            int[] hdims = { 256 };
            Rangef[] ranges = { new Rangef(0, 255) };

            CLAHE clahe = Cv2.CreateCLAHE();
            clahe.Apply(rgb[2], rgb[2]);
            clahe.Apply(rgb[1], rgb[1]);
            clahe.Apply(rgb[0], rgb[0]);

            Cv2.CalcHist(new Mat[] { rgb[2] }, new int[] { 0 }, null, HistRed, 1, hdims, ranges);
            Cv2.CalcHist(new Mat[] { rgb[1] }, new int[] { 0 }, null, HistGreen, 1, hdims, ranges);
            Cv2.CalcHist(new Mat[] { rgb[0] }, new int[] { 0 }, null, HistBlue, 1, hdims, ranges);

            int MinBlue = 0, MaxBlue = 255;            
            int MinGreen = 0, MaxGreen = 255;
            int MinRed = 0, MaxRed = 255;

            //Blue Channel
            GetMinMaxHist(ref HistBlue, totalPixel, LowCut, HighCut, out MinBlue, out MaxBlue);
            //Green channel
            GetMinMaxHist(ref HistGreen, totalPixel, LowCut, HighCut, out MinGreen, out MaxGreen);
            //Red channel
            GetMinMaxHist(ref HistRed, totalPixel, LowCut, HighCut, out MinRed, out MaxRed);

            for (int i = 0; i < 256; i++)
            {
                int PixelIndexBase = i * 4;
                Pixel[PixelIndexBase + 3] = 255;

                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 2], i, MinBlue, MaxBlue);
                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 1], i, MinGreen, MaxGreen);
                AutoLevelChannelLUT(out Pixel[PixelIndexBase + 0], i, MinRed, MaxRed);
            }

            Mat dst = new Mat();
            Mat TMP = Mat.FromPixelData(1, 256, MatType.CV_8UC4, Pixel);

            Cv2.LUT(src, TMP, dst);

            rgb[0].Dispose();
            rgb[1].Dispose();
            rgb[2].Dispose();
            HistBlue.Dispose();
            HistGreen.Dispose();
            HistRed.Dispose();
            TMP.Dispose();
            clahe.Dispose();

            return dst;
        }

        // 카드 테두리 4변의 에지점에 로버스트 직선 피팅을 적용해 기울기(도)를 구한다.
        // 반환값은 보정각(=GetRotationMatrix2D에 그대로 넣어 회전하면 수평이 됨). 실패 시 0.
        private unsafe double ComputeDeskewAngle(Mat rgb)
        {
            int W = rgb.Cols, H = rgb.Rows;

            // 채널별 Canny 에지를 OR로 합쳐 에지 마스크 생성 (여백 검출과 동일)
            Mat[] chans;
            Cv2.Split(rgb, out chans);
            Mat edge = new Mat(new OpenCvSharp.Size(W, H), MatType.CV_8UC1, Scalar.All(0));
            foreach (Mat c in chans)
            {
                Cv2.GaussianBlur(c, c, new OpenCvSharp.Size(5, 5), 0);
                Cv2.Canny(c, c, 100, 200);
                Cv2.Threshold(c, c, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                Cv2.BitwiseOr(edge, c, edge);
                c.Dispose();
            }

            byte* d = (byte*)edge.Data.ToPointer();

            // 강건 경계(L/T/R/B) — 여백 검출과 동일 방식
            int[] lh = new int[W], rh = new int[W], th = new int[H], bh = new int[H];
            for (int y = 0; y < H; ++y)
            {
                int l = -1; for (int x = 0; x < W; ++x) if (d[y * W + x] != 0) { l = x; break; }
                if (l < 0) continue;
                int r = W - 1; for (int x = W - 1; x >= 0; --x) if (d[y * W + x] != 0) { r = x; break; }
                lh[l]++; rh[r]++;
            }
            for (int x = 0; x < W; ++x)
            {
                int t = -1; for (int y = 0; y < H; ++y) if (d[y * W + x] != 0) { t = y; break; }
                if (t < 0) continue;
                int b = H - 1; for (int y = H - 1; y >= 0; --y) if (d[y * W + x] != 0) { b = y; break; }
                th[t]++; bh[b]++;
            }
            int rs = Math.Max(5, H / 100), cs = Math.Max(5, W / 100);
            int L = FindRobustBound(lh, true, rs, W), R = FindRobustBound(rh, false, rs, 0);
            int T = FindRobustBound(th, true, cs, H), B = FindRobustBound(bh, false, cs, 0);
            if (!(R > L && B > T)) { edge.Dispose(); return 0; }

            // 4변의 경계점 수집 (둥근 모서리 제외, 경계 근처 밴드만)
            int cw = R - L, chh = B - T;
            int skipH = (int)(cw * 0.12), skipV = (int)(chh * 0.12);
            int bandH = Math.Max(25, chh / 8), bandV = Math.Max(25, cw / 8);
            var top = new List<Point2f>(); var bot = new List<Point2f>();
            var lft = new List<Point2f>(); var rgt = new List<Point2f>();
            for (int x = L + skipH; x <= R - skipH; x += 3)
            {
                for (int y = 0; y < H; ++y) if (d[y * W + x] != 0) { if (y <= T + bandH) top.Add(new Point2f(x, y)); break; }
                for (int y = H - 1; y >= 0; --y) if (d[y * W + x] != 0) { if (y >= B - bandH) bot.Add(new Point2f(x, y)); break; }
            }
            for (int y = T + skipV; y <= B - skipV; y += 3)
            {
                for (int x = 0; x < W; ++x) if (d[y * W + x] != 0) { if (x <= L + bandV) lft.Add(new Point2f(x, y)); break; }
                for (int x = W - 1; x >= 0; --x) if (d[y * W + x] != 0) { if (x >= R - bandV) rgt.Add(new Point2f(x, y)); break; }
            }
            edge.Dispose();

            // 각 변의 기울기를 모아 중앙값을 취한다 (이상치 변 배제)
            List<double> angles = new List<double>();
            AddEdgeAngle(top, true, angles);
            AddEdgeAngle(bot, true, angles);
            AddEdgeAngle(lft, false, angles);
            AddEdgeAngle(rgt, false, angles);
            if (angles.Count == 0)
                return 0;
            angles.Sort();
            int m = angles.Count;
            double median = (m % 2 == 1) ? angles[m / 2] : (angles[m / 2 - 1] + angles[m / 2]) / 2;
            return median;
        }

        // 한 변의 에지점에 Huber FitLine을 적용해 기울기(도)를 angles에 추가.
        // horizontal=true: 상/하변(수평), false: 좌/우변(수직). 부호는 보정각 기준으로 통일.
        private void AddEdgeAngle(List<Point2f> pts, bool horizontal, List<double> angles)
        {
            if (pts.Count < 30)
                return;
            Line2D ln = Cv2.FitLine(pts, DistanceTypes.Huber, 0, 0.01, 0.01);
            double vx = ln.Vx, vy = ln.Vy;
            double a;
            if (horizontal)
            {
                if (vx < 0) { vx = -vx; vy = -vy; }
                a = Math.Atan2(vy, vx) * 180 / Math.PI;
            }
            else
            {
                if (vy < 0) { vx = -vx; vy = -vy; }
                a = Math.Atan2(vy, vx) * 180 / Math.PI - 90;
            }
            if (Math.Abs(a) < 20)
                angles.Add(a);
        }

        // 히스토그램 피크의 일정 비율(25%) 이상인 첫 위치를 경계로 반환(지배적 에지 탐지).
        // 카드 바깥의 옅은 에지는 피크 대비 작아 건너뛴다. 에지가 없으면 fallback 반환.
        // 카드 경계 탐지: ① 최외곽 에지(누적 소임계) B1 → ② B1에서 여백창(치수의 8%) 안의
        // 최강 에지 위치 Pm → ③ Pm 클러스터(Pm 지지의 40% 이상)의 바깥 끝을 경계로.
        // 카드 바로 위의 스캐너 띠는 창 안의 더 강한 카드 에지로 보정되고, 멀리 떨어진 반대편
        // 흔적은 창 밖이라 무시되어 과크롭(폭/높이 붕괴) 없이 흰 여백을 제거한다.
        private int FindDominantBound(int[] hist, bool fromStart, int fallback)
        {
            int len = hist.Length;
            int smallThr = Math.Max(5, len / 100);

            // ① 최외곽 에지
            int B1 = -1, csum = 0;
            if (fromStart)
            {
                for (int i = 0; i < len; ++i) { csum += hist[i]; if (csum >= smallThr) { B1 = i; break; } }
            }
            else
            {
                for (int i = len - 1; i >= 0; --i) { csum += hist[i]; if (csum >= smallThr) { B1 = i; break; } }
            }
            if (B1 < 0)
                return fallback;

            // ② 여백창 안의 최강 위치
            int maxMargin = Math.Max(40, (int)(len * 0.08));
            int Pm = B1, pmVal = 0;
            if (fromStart)
            {
                for (int i = B1; i <= Math.Min(len - 1, B1 + maxMargin); ++i) if (hist[i] > pmVal) { pmVal = hist[i]; Pm = i; }
            }
            else
            {
                for (int i = B1; i >= Math.Max(0, B1 - maxMargin); --i) if (hist[i] > pmVal) { pmVal = hist[i]; Pm = i; }
            }

            // ③ Pm 클러스터의 바깥 끝
            int clusterThr = Math.Max(8, (int)(pmVal * 0.4));
            int b = Pm;
            if (fromStart) { while (b - 1 >= 0 && hist[b - 1] >= clusterThr) b--; }
            else { while (b + 1 < len && hist[b + 1] >= clusterThr) b++; }
            return b;
        }

        // 최외곽 에지 위치 히스토그램을 한쪽 끝에서 누적하여 임계값을 처음 넘는 인덱스를 경계로 반환.
        // 임계값에 못 미치면(에지 없음) fallback을 반환해 크롭이 일어나지 않게 한다.
        private int FindRobustBound(int[] hist, bool fromStart, int threshold, int fallback)
        {
            int sum = 0;
            if (fromStart)
            {
                for (int i = 0; i < hist.Length; ++i)
                {
                    sum += hist[i];
                    if (sum >= threshold)
                        return i;
                }
            }
            else
            {
                for (int i = hist.Length - 1; i >= 0; --i)
                {
                    sum += hist[i];
                    if (sum >= threshold)
                        return i;
                }
            }
            return fallback;
        }

        private bool IsOutside(int WidthIdx, int HeightIdx, OpenCvSharp.Size size, int ClipTop, int ClipBottom, int ClipLeft, int ClipRight, int circleRadius)
        {
            return (WidthIdx < circleRadius && HeightIdx < circleRadius && Math.Sqrt((WidthIdx - circleRadius - ClipLeft) * (WidthIdx - circleRadius - ClipLeft) + (HeightIdx - circleRadius - ClipTop) * (HeightIdx - circleRadius - ClipTop)) > circleRadius) ||
                         (WidthIdx > size.Width - circleRadius && HeightIdx < circleRadius && Math.Sqrt((WidthIdx - size.Width + circleRadius + ClipRight) * (WidthIdx - size.Width + circleRadius + ClipRight) + (HeightIdx - circleRadius - ClipTop) * (HeightIdx - circleRadius - ClipTop)) > circleRadius) ||
                         (WidthIdx < circleRadius && HeightIdx > size.Height - circleRadius && Math.Sqrt((WidthIdx - circleRadius - ClipLeft) * (WidthIdx - circleRadius - ClipLeft) + (HeightIdx - size.Height + circleRadius + ClipBottom) * (HeightIdx - size.Height + circleRadius + ClipBottom)) > circleRadius) ||
                         (WidthIdx > size.Width - circleRadius && HeightIdx > size.Height - circleRadius && Math.Sqrt((WidthIdx - size.Width + circleRadius + ClipRight) * (WidthIdx - size.Width + circleRadius + ClipRight) + (HeightIdx - size.Height + circleRadius + ClipBottom) * (HeightIdx - size.Height + circleRadius + ClipBottom)) > circleRadius);
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
            if (TB_Brightness.Value != 0)
            {
                Title += "[Brightness:" + TB_Brightness.Value + "]";
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

            using (Mat original = Cv2.ImRead(TB_Source.Text))
            {
                ShowPreviewWindow("Original", original);
            }
            ShowPreviewWindow(Title, tmpImage);
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
            TB_AdjThreshold.Enabled = RB_AutoAdjust.Checked;
        }

        private void TB_Brightness_Scroll(object sender, EventArgs e)
        {
            lbl_BrightnessVal.Text = "Brightness : " + TB_Brightness.Value.ToString();
        }

        // Mat을 확대 슬라이더가 있는 프리뷰 창으로 표시(큰 이미지는 창에 맞게 축소).
        private void ShowPreviewWindow(string title, Mat mat)
        {
            byte[] buf;
            Cv2.ImEncode(".png", mat, out buf);
            Bitmap bmp;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buf))
            using (Bitmap tmp = new Bitmap(ms))
                bmp = new Bitmap(tmp);
            PreviewForm pf = new PreviewForm(title, bmp);
            pf.Show();
        }
    }
}
