﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using MS.WindowsAPICodePack.Internal;



namespace ClipScannedCarddass
{
    public partial class Form1 : Form
    {
        private Thread th;
        string waifu2xExePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void BTN_OpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_Source.Clear();

                TB_Source.Text = openFileDialog.FileName;
            }
        }

        private void BTN_OpenFolder_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

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
            TB_DenoiseH.Text = "3";
            TB_DenoiseHColor.Text = "3";
            TB_DenoiseTSize.Text = "27";
            TB_DenoiseSSize.Text = "21";
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
            BTN_AutoLevelDefault.Enabled = CB_AutoLevel.Checked;
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
            if (waifu2xExePath == null)
            {
                if (!LinkWaifu2X())
                    return;
            }

            BTN_Execute.Enabled = false;
            BTN_Abort.Enabled = true;

            if (th != null)
            {
                th.Abort();
                th = null;
            }

            th = new Thread(Execute);
            th.IsBackground = true;
            th.Start();
        }

        private void BTN_Abort_Click(object sender, EventArgs e)
        {
            if (th != null)
            {
                th.Abort();
                th = null;
            }

            UpdateExecuteButton();
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
                BTN_Execute.Enabled |= true;
                BTN_Abort.Enabled = false;
            }
        }
        
        private void Execute()
        {
            if (!Directory.Exists(TB_Target.Text))
            {
                Directory.CreateDirectory(TB_Target.Text);
            }

            bool bOneFile = false;
            string srcDirectory;
            string[] files;
            if (File.Exists(TB_Source.Text))
            {
                srcDirectory = TB_Source.Text.Remove(TB_Source.Text.LastIndexOf('\\'));
                files = new string[] { TB_Source.Text };
                bOneFile = true;
            }
            else if (Directory.Exists(TB_Source.Text))
            {
                srcDirectory = TB_Source.Text;
                if (CB_SubFolder.Checked)
                    files = Directory.GetFiles(TB_Source.Text, "*.*", SearchOption.AllDirectories);
                else
                    files = Directory.GetFiles(TB_Source.Text, "*.*", SearchOption.TopDirectoryOnly);
            }
            else
            {
                files = new string[] { };
            }

            Label_Progress.Invoke(new Action(() =>
            {
                Label_Progress.Text = "0 / " + files.Length.ToString();
                Label_Progress.Visible = true;
            }));

            PB_Progress.Invoke(new Action(() =>
            {
                PB_Progress.Enabled = true;
            }));

            int DoneCount = 0;
            foreach (string filepath in files)
            {
                string dstDirectory = TB_Target.Text + filepath.Substring(TB_Source.Text.Length);
                if (!bOneFile)
                    dstDirectory = dstDirectory.Remove(dstDirectory.LastIndexOf('\\'));

                if (!Directory.Exists(dstDirectory))
                {
                    Directory.CreateDirectory(dstDirectory);
                }

                Process(filepath, dstDirectory);
                DoneCount++;

                Label_Progress.Invoke(new Action(() =>
                    {
                        Label_Progress.Text = DoneCount.ToString() + " / " + files.Length.ToString();
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

            Image image = Image.FromFile(filepath);
            if (image == null)
                return null;

            System.Drawing.Size imageSize = image.Size;
            bool bHorizontal = imageSize.Width > imageSize.Height;

            int X = CB_ChangeSize.Checked ? Convert.ToInt32(TB_ResizeW.Text) : imageSize.Width;
            int Y = CB_ChangeSize.Checked ? Convert.ToInt32(TB_ResizeH.Text) : imageSize.Height;

            if (X * Y == 0)
                return null;

            if (X * Y > 268435456)
                return null;

            System.Drawing.Size size = new System.Drawing.Size(bHorizontal ? X : Y, bHorizontal ? Y : X);

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

            Bitmap srcBitmap = new Bitmap(image, size);
            image.Dispose();

            Rectangle srcRect = new Rectangle(0, 0, size.Width, size.Height);
            BitmapData srcBitmapData = srcBitmap.LockBits(srcRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            Mat Original = Mat.FromPixelData(srcBitmap.Height, srcBitmap.Width, MatType.CV_8UC4, srcBitmapData.Scan0);

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

            srcBitmap.UnlockBits(srcBitmapData);
            srcBitmap.Dispose();

            Bitmap dstBitmap = new Bitmap(size.Width - ClipLeft - ClipRight, size.Height - ClipTop - ClipBottom, PixelFormat.Format32bppArgb);
            Rectangle dstRect = new Rectangle(0, 0, size.Width - ClipLeft - ClipRight, size.Height - ClipTop - ClipBottom);
            BitmapData dstBitmapData = dstBitmap.LockBits(dstRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            IntPtr pSrcBitmap = srcBitmapData.Scan0;
            IntPtr pDstBitmap = dstBitmapData.Scan0;

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

            Mat Result = new Mat();
            Cv2.CopyTo(Mat.FromPixelData(dstBitmap.Size.Height, dstBitmap.Size.Width, MatType.CV_8UC4, dstBitmapData.Scan0), Result);

            dstBitmap.UnlockBits(dstBitmapData);
            AutoLevelSrc.Dispose();
            dstBitmap.Dispose();

            return Result;
        }

        private void Process(string filepath, string dstDirectory)
        {
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
            Result.Dispose();

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

                ps.Start();
                ps.WaitForExit();

                Image image = Image.FromFile(newFileName);
                System.Drawing.Size imageSize = image.Size;
                imageSize.Width = imageSize.Width / 2;
                imageSize.Height = imageSize.Height / 2;
                Bitmap newImage = new Bitmap(image, imageSize);
                image.Dispose();

                newImage.Save(newFileName, ImageFormat.Png);
            }
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

            MatType type = src.Type();

            int[] hdims = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };

            bool uniform = true;
            bool accumulate = false;
            Cv2.CalcHist(new Mat[] { rgb[2] }, new int[] { 0 }, null, HistRed, 1, hdims, ranges, uniform, accumulate);
            Cv2.CalcHist(new Mat[] { rgb[1] }, new int[] { 0 }, null, HistGreen, 1, hdims, ranges, uniform, accumulate);
            Cv2.CalcHist(new Mat[] { rgb[0] }, new int[] { 0 }, null, HistBlue, 1, hdims, ranges, uniform, accumulate);

            int MinBlue = 0, MaxBlue = 0;
            int MinRed = 0, MaxRed = 0;
            int MinGreen = 0, MaxGreen = 0;

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
            return dst;
        }

        private bool IsOutside(int WidthIdx, int HeightIdx, System.Drawing.Size size, int ClipTop, int ClipBottom, int ClipLeft, int ClipRight, int circleRadius)
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

                Image image = Image.FromFile(tmpFileName);
                System.Drawing.Size imageSize = image.Size;
                imageSize.Width = imageSize.Width / 2;
                imageSize.Height = imageSize.Height / 2;
                Bitmap newImage = new Bitmap(image, imageSize);
                image.Dispose();

                newImage.Save(tmpFileName, ImageFormat.Png);

                tmpImage = Cv2.ImRead(tmpFileName);

                File.Delete(tmpFileName);
            }

            Cv2.ImShow("Original", Cv2.ImRead(TB_Source.Text));
            Cv2.ImShow(Title, tmpImage);
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
        }

        private void button1_Click(object sender, EventArgs e)
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

        private void TB_ResizeW_KeyPressed(object sender, KeyPressEventArgs e)
        {
            //숫자와 백스페이스만 입력
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ResizeH_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ClipTop_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ClipLeft_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ClipRight_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ClipBottom_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_CornerRounding_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_AutoLevelMin_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_AutoLevelMax_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_DenoiseH_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_DenoiseHColor_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_DenoiseTSize_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_DenoiseSSize_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void TB_ResizeW_TextChanged(object sender, EventArgs e)
        {
            UpdateTBResize();
        }

        private void ShowWarning()
        {
            MessageBox.Show("설정 값을 확인하세요.", "경고");
        }
    }
}
