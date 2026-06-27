namespace ScannedCardDenoiser
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.BTN_OpenFile = new System.Windows.Forms.Button();
            this.TB_Source = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BTN_OpenFolder = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TB_Target = new System.Windows.Forms.TextBox();
            this.BTN_TargetFolder = new System.Windows.Forms.Button();
            this.BTN_Execute = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.RB_NoneAdjust = new System.Windows.Forms.RadioButton();
            this.TB_AdjThreshold = new System.Windows.Forms.TextBox();
            this.RB_ManualAdjust = new System.Windows.Forms.RadioButton();
            this.RB_AutoAdjust = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.BTN_AutoLevelDefault = new System.Windows.Forms.Button();
            this.BTN_DenoiseDefault = new System.Windows.Forms.Button();
            this.TB_DenoiseSSize = new System.Windows.Forms.TextBox();
            this.TB_DenoiseTSize = new System.Windows.Forms.TextBox();
            this.TB_DenoiseHColor = new System.Windows.Forms.TextBox();
            this.TB_DenoiseH = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.TB_AutoLevelMax = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.TB_AutoLevelMin = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CB_DenoiseColor = new System.Windows.Forms.CheckBox();
            this.CB_AutoLevel = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.CB_CornerRounding = new System.Windows.Forms.CheckBox();
            this.CB_EdgeLine = new System.Windows.Forms.CheckBox();
            this.RB_ResizeTradingCard = new System.Windows.Forms.RadioButton();
            this.RB_ResizeCarddass = new System.Windows.Forms.RadioButton();
            this.RB_Resize4x6 = new System.Windows.Forms.RadioButton();
            this.RB_ResizeCustom = new System.Windows.Forms.RadioButton();
            this.label13 = new System.Windows.Forms.Label();
            this.TB_ResizeH = new System.Windows.Forms.TextBox();
            this.TB_ResizeW = new System.Windows.Forms.TextBox();
            this.CB_ChangeSize = new System.Windows.Forms.CheckBox();
            this.CB_Clip = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.TB_ClipLeft = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.TB_ClipBottom = new System.Windows.Forms.TextBox();
            this.TB_CornerRounding = new System.Windows.Forms.TextBox();
            this.TB_ClipRight = new System.Windows.Forms.TextBox();
            this.TB_ClipTop = new System.Windows.Forms.TextBox();
            this.PB_Progress = new System.Windows.Forms.ProgressBar();
            this.Label_Progress = new System.Windows.Forms.Label();
            this.BTN_Abort = new System.Windows.Forms.Button();
            this.CB_SubFolder = new System.Windows.Forms.CheckBox();
            this.BTN_ShowPreview = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BTN_waifu2x = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.CB_Waifu2xTTA = new System.Windows.Forms.CheckBox();
            this.RB_WaifuPhoto = new System.Windows.Forms.RadioButton();
            this.RB_WaifuAnime = new System.Windows.Forms.RadioButton();
            this.RB_WaifuCunet = new System.Windows.Forms.RadioButton();
            this.CB_waifu2x = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.RB_WaifuHighest = new System.Windows.Forms.RadioButton();
            this.RB_WaifuHigh = new System.Windows.Forms.RadioButton();
            this.RB_WaifuMed = new System.Windows.Forms.RadioButton();
            this.RB_WaifuLow = new System.Windows.Forms.RadioButton();
            this.Label_FileName = new System.Windows.Forms.Label();
            this.CB_Overwrite = new System.Windows.Forms.CheckBox();
            this.TB_Brightness = new System.Windows.Forms.TrackBar();
            this.lbl_BrightnessVal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TB_Brightness)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "이미지|*.jpg;*.png;*.bmp";
            this.openFileDialog.Multiselect = true;
            // 
            // BTN_OpenFile
            // 
            this.BTN_OpenFile.Location = new System.Drawing.Point(648, 32);
            this.BTN_OpenFile.Name = "BTN_OpenFile";
            this.BTN_OpenFile.Size = new System.Drawing.Size(43, 23);
            this.BTN_OpenFile.TabIndex = 0;
            this.BTN_OpenFile.Text = "File";
            this.BTN_OpenFile.UseVisualStyleBackColor = true;
            this.BTN_OpenFile.Click += new System.EventHandler(this.BTN_OpenFile_Click);
            // 
            // TB_Source
            // 
            this.TB_Source.Enabled = false;
            this.TB_Source.Location = new System.Drawing.Point(124, 32);
            this.TB_Source.Name = "TB_Source";
            this.TB_Source.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.TB_Source.Size = new System.Drawing.Size(518, 21);
            this.TB_Source.TabIndex = 1;
            this.TB_Source.TextChanged += new System.EventHandler(this.TB_Source_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "원본 파일 및 폴더";
            // 
            // BTN_OpenFolder
            // 
            this.BTN_OpenFolder.Location = new System.Drawing.Point(697, 32);
            this.BTN_OpenFolder.Name = "BTN_OpenFolder";
            this.BTN_OpenFolder.Size = new System.Drawing.Size(53, 23);
            this.BTN_OpenFolder.TabIndex = 3;
            this.BTN_OpenFolder.Text = "Folder";
            this.BTN_OpenFolder.UseVisualStyleBackColor = true;
            this.BTN_OpenFolder.Click += new System.EventHandler(this.BTN_OpenFolder_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "저장 위치";
            // 
            // TB_Target
            // 
            this.TB_Target.Enabled = false;
            this.TB_Target.Location = new System.Drawing.Point(124, 78);
            this.TB_Target.Name = "TB_Target";
            this.TB_Target.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.TB_Target.Size = new System.Drawing.Size(518, 21);
            this.TB_Target.TabIndex = 5;
            this.TB_Target.TextChanged += new System.EventHandler(this.TB_Target_TextChanged);
            // 
            // BTN_TargetFolder
            // 
            this.BTN_TargetFolder.Location = new System.Drawing.Point(649, 78);
            this.BTN_TargetFolder.Name = "BTN_TargetFolder";
            this.BTN_TargetFolder.Size = new System.Drawing.Size(101, 21);
            this.BTN_TargetFolder.TabIndex = 6;
            this.BTN_TargetFolder.Text = "Folder";
            this.BTN_TargetFolder.UseVisualStyleBackColor = true;
            this.BTN_TargetFolder.Click += new System.EventHandler(this.BTN_TargetFolder_Click);
            // 
            // BTN_Execute
            // 
            this.BTN_Execute.Enabled = false;
            this.BTN_Execute.Location = new System.Drawing.Point(574, 437);
            this.BTN_Execute.Name = "BTN_Execute";
            this.BTN_Execute.Size = new System.Drawing.Size(104, 54);
            this.BTN_Execute.TabIndex = 7;
            this.BTN_Execute.Text = "실행";
            this.BTN_Execute.UseVisualStyleBackColor = true;
            this.BTN_Execute.Click += new System.EventHandler(this.BTN_Execute_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TB_Brightness);
            this.groupBox1.Controls.Add(this.lbl_BrightnessVal);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.BTN_AutoLevelDefault);
            this.groupBox1.Controls.Add(this.BTN_DenoiseDefault);
            this.groupBox1.Controls.Add(this.TB_DenoiseSSize);
            this.groupBox1.Controls.Add(this.TB_DenoiseTSize);
            this.groupBox1.Controls.Add(this.TB_DenoiseHColor);
            this.groupBox1.Controls.Add(this.TB_DenoiseH);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.TB_AutoLevelMax);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.TB_AutoLevelMin);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.CB_DenoiseColor);
            this.groupBox1.Controls.Add(this.CB_AutoLevel);
            this.groupBox1.Location = new System.Drawing.Point(14, 105);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(218, 355);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "이미지 보정";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.RB_NoneAdjust);
            this.groupBox5.Controls.Add(this.TB_AdjThreshold);
            this.groupBox5.Controls.Add(this.RB_ManualAdjust);
            this.groupBox5.Controls.Add(this.RB_AutoAdjust);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Location = new System.Drawing.Point(6, 197);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(206, 83);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Image Area Adjustment";
            // 
            // RB_NoneAdjust
            // 
            this.RB_NoneAdjust.AutoSize = true;
            this.RB_NoneAdjust.Location = new System.Drawing.Point(7, 16);
            this.RB_NoneAdjust.Name = "RB_NoneAdjust";
            this.RB_NoneAdjust.Size = new System.Drawing.Size(39, 16);
            this.RB_NoneAdjust.TabIndex = 13;
            this.RB_NoneAdjust.Text = "No";
            this.RB_NoneAdjust.UseVisualStyleBackColor = true;
            // 
            // TB_AdjThreshold
            // 
            this.TB_AdjThreshold.Location = new System.Drawing.Point(162, 17);
            this.TB_AdjThreshold.Name = "TB_AdjThreshold";
            this.TB_AdjThreshold.Size = new System.Drawing.Size(41, 21);
            this.TB_AdjThreshold.TabIndex = 11;
            this.TB_AdjThreshold.Text = "50";
            this.TB_AdjThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_AdjThreshold.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // RB_ManualAdjust
            // 
            this.RB_ManualAdjust.AutoSize = true;
            this.RB_ManualAdjust.Location = new System.Drawing.Point(7, 62);
            this.RB_ManualAdjust.Name = "RB_ManualAdjust";
            this.RB_ManualAdjust.Size = new System.Drawing.Size(153, 16);
            this.RB_ManualAdjust.TabIndex = 12;
            this.RB_ManualAdjust.Text = "Manual (Experimental)";
            this.RB_ManualAdjust.UseVisualStyleBackColor = true;
            // 
            // RB_AutoAdjust
            // 
            this.RB_AutoAdjust.AutoSize = true;
            this.RB_AutoAdjust.Checked = true;
            this.RB_AutoAdjust.Location = new System.Drawing.Point(7, 39);
            this.RB_AutoAdjust.Name = "RB_AutoAdjust";
            this.RB_AutoAdjust.Size = new System.Drawing.Size(48, 16);
            this.RB_AutoAdjust.TabIndex = 11;
            this.RB_AutoAdjust.TabStop = true;
            this.RB_AutoAdjust.Text = "Auto";
            this.RB_AutoAdjust.UseVisualStyleBackColor = true;
            this.RB_AutoAdjust.CheckedChanged += new System.EventHandler(this.RB_AutoAdjust_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label14.Location = new System.Drawing.Point(95, 21);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(61, 11);
            this.label14.TabIndex = 10;
            this.label14.Text = "Threshold";
            // 
            // BTN_AutoLevelDefault
            // 
            this.BTN_AutoLevelDefault.Location = new System.Drawing.Point(13, 58);
            this.BTN_AutoLevelDefault.Name = "BTN_AutoLevelDefault";
            this.BTN_AutoLevelDefault.Size = new System.Drawing.Size(52, 23);
            this.BTN_AutoLevelDefault.TabIndex = 8;
            this.BTN_AutoLevelDefault.Text = "Default";
            this.BTN_AutoLevelDefault.UseVisualStyleBackColor = true;
            this.BTN_AutoLevelDefault.Click += new System.EventHandler(this.BTN_AutoLevelDefault_Click);
            // 
            // BTN_DenoiseDefault
            // 
            this.BTN_DenoiseDefault.Location = new System.Drawing.Point(11, 162);
            this.BTN_DenoiseDefault.Name = "BTN_DenoiseDefault";
            this.BTN_DenoiseDefault.Size = new System.Drawing.Size(52, 23);
            this.BTN_DenoiseDefault.TabIndex = 8;
            this.BTN_DenoiseDefault.Text = "Default";
            this.BTN_DenoiseDefault.UseVisualStyleBackColor = true;
            this.BTN_DenoiseDefault.Click += new System.EventHandler(this.BTN_DenoiseDefault_Click);
            // 
            // TB_DenoiseSSize
            // 
            this.TB_DenoiseSSize.Location = new System.Drawing.Point(147, 165);
            this.TB_DenoiseSSize.Name = "TB_DenoiseSSize";
            this.TB_DenoiseSSize.Size = new System.Drawing.Size(45, 21);
            this.TB_DenoiseSSize.TabIndex = 7;
            this.TB_DenoiseSSize.Text = "3";
            this.TB_DenoiseSSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_DenoiseSSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_DenoiseTSize
            // 
            this.TB_DenoiseTSize.Location = new System.Drawing.Point(147, 144);
            this.TB_DenoiseTSize.Name = "TB_DenoiseTSize";
            this.TB_DenoiseTSize.Size = new System.Drawing.Size(45, 21);
            this.TB_DenoiseTSize.TabIndex = 7;
            this.TB_DenoiseTSize.Text = "21";
            this.TB_DenoiseTSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_DenoiseTSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_DenoiseHColor
            // 
            this.TB_DenoiseHColor.Location = new System.Drawing.Point(171, 121);
            this.TB_DenoiseHColor.Name = "TB_DenoiseHColor";
            this.TB_DenoiseHColor.Size = new System.Drawing.Size(21, 21);
            this.TB_DenoiseHColor.TabIndex = 7;
            this.TB_DenoiseHColor.Text = "3";
            this.TB_DenoiseHColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_DenoiseHColor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_DenoiseH
            // 
            this.TB_DenoiseH.Location = new System.Drawing.Point(93, 121);
            this.TB_DenoiseH.Name = "TB_DenoiseH";
            this.TB_DenoiseH.Size = new System.Drawing.Size(21, 21);
            this.TB_DenoiseH.TabIndex = 7;
            this.TB_DenoiseH.Text = "10";
            this.TB_DenoiseH.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_DenoiseH.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(69, 168);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 12);
            this.label8.TabIndex = 6;
            this.label8.Text = "Search Size";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(57, 147);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "Template Size";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(123, 124);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "hColor";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(75, 124);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(12, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "h";
            // 
            // TB_AutoLevelMax
            // 
            this.TB_AutoLevelMax.Location = new System.Drawing.Point(156, 54);
            this.TB_AutoLevelMax.Name = "TB_AutoLevelMax";
            this.TB_AutoLevelMax.Size = new System.Drawing.Size(36, 21);
            this.TB_AutoLevelMax.TabIndex = 5;
            this.TB_AutoLevelMax.Text = "0";
            this.TB_AutoLevelMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_AutoLevelMax.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("굴림", 9F);
            this.label17.Location = new System.Drawing.Point(194, 57);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(15, 12);
            this.label17.TabIndex = 2;
            this.label17.Text = "%";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("굴림", 9F);
            this.label16.Location = new System.Drawing.Point(194, 39);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(15, 12);
            this.label16.TabIndex = 2;
            this.label16.Text = "%";
            // 
            // TB_AutoLevelMin
            // 
            this.TB_AutoLevelMin.Location = new System.Drawing.Point(156, 36);
            this.TB_AutoLevelMin.Name = "TB_AutoLevelMin";
            this.TB_AutoLevelMin.Size = new System.Drawing.Size(36, 21);
            this.TB_AutoLevelMin.TabIndex = 4;
            this.TB_AutoLevelMin.Text = "5";
            this.TB_AutoLevelMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_AutoLevelMin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("굴림", 9F);
            this.label4.Location = new System.Drawing.Point(124, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "Max";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 9F);
            this.label3.Location = new System.Drawing.Point(124, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Min";
            // 
            // CB_DenoiseColor
            // 
            this.CB_DenoiseColor.AutoSize = true;
            this.CB_DenoiseColor.Checked = true;
            this.CB_DenoiseColor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_DenoiseColor.Location = new System.Drawing.Point(6, 99);
            this.CB_DenoiseColor.Name = "CB_DenoiseColor";
            this.CB_DenoiseColor.Size = new System.Drawing.Size(173, 16);
            this.CB_DenoiseColor.TabIndex = 1;
            this.CB_DenoiseColor.Text = "Non-local Means Denoise";
            this.CB_DenoiseColor.UseVisualStyleBackColor = true;
            this.CB_DenoiseColor.CheckedChanged += new System.EventHandler(this.CB_DenoiseColor_CheckedChanged);
            // 
            // CB_AutoLevel
            // 
            this.CB_AutoLevel.AutoSize = true;
            this.CB_AutoLevel.Checked = true;
            this.CB_AutoLevel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_AutoLevel.Location = new System.Drawing.Point(6, 20);
            this.CB_AutoLevel.Name = "CB_AutoLevel";
            this.CB_AutoLevel.Size = new System.Drawing.Size(122, 16);
            this.CB_AutoLevel.TabIndex = 0;
            this.CB_AutoLevel.Text = "Auto Level Image";
            this.CB_AutoLevel.UseVisualStyleBackColor = true;
            this.CB_AutoLevel.CheckedChanged += new System.EventHandler(this.CB_AutoLevel_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.CB_CornerRounding);
            this.groupBox2.Controls.Add(this.CB_EdgeLine);
            this.groupBox2.Controls.Add(this.RB_ResizeTradingCard);
            this.groupBox2.Controls.Add(this.RB_ResizeCarddass);
            this.groupBox2.Controls.Add(this.RB_Resize4x6);
            this.groupBox2.Controls.Add(this.RB_ResizeCustom);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.TB_ResizeH);
            this.groupBox2.Controls.Add(this.TB_ResizeW);
            this.groupBox2.Controls.Add(this.CB_ChangeSize);
            this.groupBox2.Controls.Add(this.CB_Clip);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.TB_ClipLeft);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.TB_ClipBottom);
            this.groupBox2.Controls.Add(this.TB_CornerRounding);
            this.groupBox2.Controls.Add(this.TB_ClipRight);
            this.groupBox2.Controls.Add(this.TB_ClipTop);
            this.groupBox2.Location = new System.Drawing.Point(249, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(218, 303);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "이미지 편집";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label19.Location = new System.Drawing.Point(159, 41);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(35, 22);
            this.label19.TabIndex = 9;
            this.label19.Text = "Short\r\nside";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label18.Location = new System.Drawing.Point(15, 41);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(32, 22);
            this.label18.TabIndex = 9;
            this.label18.Text = "Long\r\nside";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CB_CornerRounding
            // 
            this.CB_CornerRounding.AutoSize = true;
            this.CB_CornerRounding.Checked = true;
            this.CB_CornerRounding.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_CornerRounding.Location = new System.Drawing.Point(6, 282);
            this.CB_CornerRounding.Name = "CB_CornerRounding";
            this.CB_CornerRounding.Size = new System.Drawing.Size(88, 16);
            this.CB_CornerRounding.TabIndex = 8;
            this.CB_CornerRounding.Text = "코너 라운딩";
            this.CB_CornerRounding.UseVisualStyleBackColor = true;
            this.CB_CornerRounding.CheckedChanged += new System.EventHandler(this.CB_CornerRounding_CheckedChanged);
            // 
            // CB_EdgeLine
            // 
            this.CB_EdgeLine.AutoSize = true;
            this.CB_EdgeLine.Checked = true;
            this.CB_EdgeLine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_EdgeLine.Location = new System.Drawing.Point(7, 259);
            this.CB_EdgeLine.Name = "CB_EdgeLine";
            this.CB_EdgeLine.Size = new System.Drawing.Size(100, 16);
            this.CB_EdgeLine.TabIndex = 8;
            this.CB_EdgeLine.Text = "테두리 그리기";
            this.CB_EdgeLine.UseVisualStyleBackColor = true;
            // 
            // RB_ResizeTradingCard
            // 
            this.RB_ResizeTradingCard.AutoSize = true;
            this.RB_ResizeTradingCard.Enabled = false;
            this.RB_ResizeTradingCard.Location = new System.Drawing.Point(34, 135);
            this.RB_ResizeTradingCard.Name = "RB_ResizeTradingCard";
            this.RB_ResizeTradingCard.Size = new System.Drawing.Size(154, 16);
            this.RB_ResizeTradingCard.TabIndex = 7;
            this.RB_ResizeTradingCard.Text = "TradingCard (8.9 x 6.3)";
            this.RB_ResizeTradingCard.UseVisualStyleBackColor = true;
            this.RB_ResizeTradingCard.CheckedChanged += new System.EventHandler(this.RB_ResizeTradingCard_CheckedChanged);
            // 
            // RB_ResizeCarddass
            // 
            this.RB_ResizeCarddass.AutoSize = true;
            this.RB_ResizeCarddass.Checked = true;
            this.RB_ResizeCarddass.Enabled = false;
            this.RB_ResizeCarddass.Location = new System.Drawing.Point(34, 113);
            this.RB_ResizeCarddass.Name = "RB_ResizeCarddass";
            this.RB_ResizeCarddass.Size = new System.Drawing.Size(139, 16);
            this.RB_ResizeCarddass.TabIndex = 7;
            this.RB_ResizeCarddass.TabStop = true;
            this.RB_ResizeCarddass.Text = "Carddass (8.6 x 5.9)";
            this.RB_ResizeCarddass.UseVisualStyleBackColor = true;
            this.RB_ResizeCarddass.CheckedChanged += new System.EventHandler(this.RB_ResizeCarddass_CheckedChanged);
            // 
            // RB_Resize4x6
            // 
            this.RB_Resize4x6.AutoSize = true;
            this.RB_Resize4x6.Enabled = false;
            this.RB_Resize4x6.Location = new System.Drawing.Point(34, 91);
            this.RB_Resize4x6.Name = "RB_Resize4x6";
            this.RB_Resize4x6.Size = new System.Drawing.Size(175, 16);
            this.RB_Resize4x6.TabIndex = 7;
            this.RB_Resize4x6.Text = "절지(A1, A2,... / B1, B2,...)";
            this.RB_Resize4x6.UseVisualStyleBackColor = true;
            this.RB_Resize4x6.CheckedChanged += new System.EventHandler(this.RB_Resize4x6_CheckedChanged);
            // 
            // RB_ResizeCustom
            // 
            this.RB_ResizeCustom.AutoSize = true;
            this.RB_ResizeCustom.Enabled = false;
            this.RB_ResizeCustom.Location = new System.Drawing.Point(34, 69);
            this.RB_ResizeCustom.Name = "RB_ResizeCustom";
            this.RB_ResizeCustom.Size = new System.Drawing.Size(67, 16);
            this.RB_ResizeCustom.TabIndex = 7;
            this.RB_ResizeCustom.Text = "Custom";
            this.RB_ResizeCustom.UseVisualStyleBackColor = true;
            this.RB_ResizeCustom.CheckedChanged += new System.EventHandler(this.RB_ResizeCustom_CheckedChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(98, 47);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(15, 12);
            this.label13.TabIndex = 6;
            this.label13.Text = "×";
            // 
            // TB_ResizeH
            // 
            this.TB_ResizeH.Enabled = false;
            this.TB_ResizeH.Location = new System.Drawing.Point(115, 42);
            this.TB_ResizeH.Name = "TB_ResizeH";
            this.TB_ResizeH.Size = new System.Drawing.Size(37, 21);
            this.TB_ResizeH.TabIndex = 5;
            this.TB_ResizeH.Text = "590";
            this.TB_ResizeH.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_ResizeW
            // 
            this.TB_ResizeW.Enabled = false;
            this.TB_ResizeW.Location = new System.Drawing.Point(53, 42);
            this.TB_ResizeW.Name = "TB_ResizeW";
            this.TB_ResizeW.Size = new System.Drawing.Size(41, 21);
            this.TB_ResizeW.TabIndex = 5;
            this.TB_ResizeW.Text = "860";
            this.TB_ResizeW.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_ResizeW.TextChanged += new System.EventHandler(this.TB_ResizeW_TextChanged);
            this.TB_ResizeW.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // CB_ChangeSize
            // 
            this.CB_ChangeSize.AutoSize = true;
            this.CB_ChangeSize.Location = new System.Drawing.Point(6, 20);
            this.CB_ChangeSize.Name = "CB_ChangeSize";
            this.CB_ChangeSize.Size = new System.Drawing.Size(88, 16);
            this.CB_ChangeSize.TabIndex = 0;
            this.CB_ChangeSize.Text = "사이즈 변경";
            this.CB_ChangeSize.UseVisualStyleBackColor = true;
            this.CB_ChangeSize.CheckedChanged += new System.EventHandler(this.CB_ChangeSize_CheckedChanged);
            // 
            // CB_Clip
            // 
            this.CB_Clip.AutoSize = true;
            this.CB_Clip.Checked = true;
            this.CB_Clip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_Clip.Location = new System.Drawing.Point(6, 164);
            this.CB_Clip.Name = "CB_Clip";
            this.CB_Clip.Size = new System.Drawing.Size(100, 16);
            this.CB_Clip.TabIndex = 0;
            this.CB_Clip.Text = "모서리 자르기";
            this.CB_Clip.UseVisualStyleBackColor = true;
            this.CB_Clip.CheckedChanged += new System.EventHandler(this.CB_Clip_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("굴림", 9F);
            this.label15.Location = new System.Drawing.Point(164, 280);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(15, 12);
            this.label15.TabIndex = 2;
            this.label15.Text = "%";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("굴림", 9F);
            this.label10.Location = new System.Drawing.Point(6, 213);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(25, 12);
            this.label10.TabIndex = 2;
            this.label10.Text = "Left";
            // 
            // TB_ClipLeft
            // 
            this.TB_ClipLeft.Location = new System.Drawing.Point(38, 204);
            this.TB_ClipLeft.Name = "TB_ClipLeft";
            this.TB_ClipLeft.Size = new System.Drawing.Size(30, 21);
            this.TB_ClipLeft.TabIndex = 4;
            this.TB_ClipLeft.Text = "2";
            this.TB_ClipLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_ClipLeft.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("굴림", 9F);
            this.label12.Location = new System.Drawing.Point(51, 237);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 12);
            this.label12.TabIndex = 2;
            this.label12.Text = "Bottom";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("굴림", 9F);
            this.label11.Location = new System.Drawing.Point(129, 211);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(33, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "Right";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("굴림", 9F);
            this.label9.Location = new System.Drawing.Point(68, 189);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(27, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "Top";
            // 
            // TB_ClipBottom
            // 
            this.TB_ClipBottom.Location = new System.Drawing.Point(100, 228);
            this.TB_ClipBottom.Name = "TB_ClipBottom";
            this.TB_ClipBottom.Size = new System.Drawing.Size(30, 21);
            this.TB_ClipBottom.TabIndex = 4;
            this.TB_ClipBottom.Text = "2";
            this.TB_ClipBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_ClipBottom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_CornerRounding
            // 
            this.TB_CornerRounding.Location = new System.Drawing.Point(131, 277);
            this.TB_CornerRounding.Name = "TB_CornerRounding";
            this.TB_CornerRounding.Size = new System.Drawing.Size(30, 21);
            this.TB_CornerRounding.TabIndex = 4;
            this.TB_CornerRounding.Text = "4";
            this.TB_CornerRounding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_CornerRounding.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_ClipRight
            // 
            this.TB_ClipRight.Location = new System.Drawing.Point(161, 204);
            this.TB_ClipRight.Name = "TB_ClipRight";
            this.TB_ClipRight.Size = new System.Drawing.Size(30, 21);
            this.TB_ClipRight.TabIndex = 4;
            this.TB_ClipRight.Text = "2";
            this.TB_ClipRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_ClipRight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // TB_ClipTop
            // 
            this.TB_ClipTop.Location = new System.Drawing.Point(100, 180);
            this.TB_ClipTop.Name = "TB_ClipTop";
            this.TB_ClipTop.Size = new System.Drawing.Size(30, 21);
            this.TB_ClipTop.TabIndex = 4;
            this.TB_ClipTop.Text = "2";
            this.TB_ClipTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TB_ClipTop.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_OnlyDigit_KeyPressed);
            // 
            // PB_Progress
            // 
            this.PB_Progress.Enabled = false;
            this.PB_Progress.Location = new System.Drawing.Point(14, 474);
            this.PB_Progress.Name = "PB_Progress";
            this.PB_Progress.Size = new System.Drawing.Size(554, 14);
            this.PB_Progress.TabIndex = 10;
            // 
            // Label_Progress
            // 
            this.Label_Progress.AutoSize = true;
            this.Label_Progress.BackColor = System.Drawing.Color.Transparent;
            this.Label_Progress.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Label_Progress.Location = new System.Drawing.Point(408, 461);
            this.Label_Progress.Name = "Label_Progress";
            this.Label_Progress.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label_Progress.Size = new System.Drawing.Size(165, 11);
            this.Label_Progress.TabIndex = 11;
            this.Label_Progress.Text = "000 / 1111 (0000.0s / 0000.0s)";
            this.Label_Progress.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.Label_Progress.Visible = false;
            // 
            // BTN_Abort
            // 
            this.BTN_Abort.Enabled = false;
            this.BTN_Abort.Location = new System.Drawing.Point(684, 437);
            this.BTN_Abort.Name = "BTN_Abort";
            this.BTN_Abort.Size = new System.Drawing.Size(66, 54);
            this.BTN_Abort.TabIndex = 7;
            this.BTN_Abort.Text = "취소";
            this.BTN_Abort.UseVisualStyleBackColor = true;
            this.BTN_Abort.Click += new System.EventHandler(this.BTN_Abort_Click);
            // 
            // CB_SubFolder
            // 
            this.CB_SubFolder.AutoSize = true;
            this.CB_SubFolder.Location = new System.Drawing.Point(673, 59);
            this.CB_SubFolder.Name = "CB_SubFolder";
            this.CB_SubFolder.Size = new System.Drawing.Size(77, 16);
            this.CB_SubFolder.TabIndex = 12;
            this.CB_SubFolder.Text = "Subfolder";
            this.CB_SubFolder.UseVisualStyleBackColor = true;
            // 
            // BTN_ShowPreview
            // 
            this.BTN_ShowPreview.Enabled = false;
            this.BTN_ShowPreview.Location = new System.Drawing.Point(574, 396);
            this.BTN_ShowPreview.Name = "BTN_ShowPreview";
            this.BTN_ShowPreview.Size = new System.Drawing.Size(176, 28);
            this.BTN_ShowPreview.TabIndex = 13;
            this.BTN_ShowPreview.Text = "Show Preview";
            this.BTN_ShowPreview.UseVisualStyleBackColor = true;
            this.BTN_ShowPreview.Click += new System.EventHandler(this.BTN_ShowPreview_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BTN_waifu2x);
            this.groupBox3.Controls.Add(this.groupBox6);
            this.groupBox3.Controls.Add(this.CB_waifu2x);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Location = new System.Drawing.Point(486, 105);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(264, 223);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "waifu2x 디노이저";
            // 
            // BTN_waifu2x
            // 
            this.BTN_waifu2x.Location = new System.Drawing.Point(7, 39);
            this.BTN_waifu2x.Name = "BTN_waifu2x";
            this.BTN_waifu2x.Size = new System.Drawing.Size(251, 23);
            this.BTN_waifu2x.TabIndex = 4;
            this.BTN_waifu2x.Text = "waifu2x_ncnn_vulkan.exe 연결 필요";
            this.BTN_waifu2x.UseVisualStyleBackColor = true;
            this.BTN_waifu2x.Click += new System.EventHandler(this.BTN_waifu2x_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.CB_Waifu2xTTA);
            this.groupBox6.Controls.Add(this.RB_WaifuPhoto);
            this.groupBox6.Controls.Add(this.RB_WaifuAnime);
            this.groupBox6.Controls.Add(this.RB_WaifuCunet);
            this.groupBox6.Location = new System.Drawing.Point(7, 76);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(251, 84);
            this.groupBox6.TabIndex = 3;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Model";
            // 
            // CB_Waifu2xTTA
            // 
            this.CB_Waifu2xTTA.AutoSize = true;
            this.CB_Waifu2xTTA.Checked = true;
            this.CB_Waifu2xTTA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_Waifu2xTTA.Location = new System.Drawing.Point(156, 63);
            this.CB_Waifu2xTTA.Name = "CB_Waifu2xTTA";
            this.CB_Waifu2xTTA.Size = new System.Drawing.Size(88, 16);
            this.CB_Waifu2xTTA.TabIndex = 1;
            this.CB_Waifu2xTTA.Text = "TTA 활성화";
            this.CB_Waifu2xTTA.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuPhoto
            // 
            this.RB_WaifuPhoto.AutoSize = true;
            this.RB_WaifuPhoto.Location = new System.Drawing.Point(6, 38);
            this.RB_WaifuPhoto.Name = "RB_WaifuPhoto";
            this.RB_WaifuPhoto.Size = new System.Drawing.Size(105, 16);
            this.RB_WaifuPhoto.TabIndex = 0;
            this.RB_WaifuPhoto.Text = "upconv7 photo";
            this.RB_WaifuPhoto.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuAnime
            // 
            this.RB_WaifuAnime.AutoSize = true;
            this.RB_WaifuAnime.Checked = true;
            this.RB_WaifuAnime.Location = new System.Drawing.Point(6, 16);
            this.RB_WaifuAnime.Name = "RB_WaifuAnime";
            this.RB_WaifuAnime.Size = new System.Drawing.Size(109, 16);
            this.RB_WaifuAnime.TabIndex = 0;
            this.RB_WaifuAnime.TabStop = true;
            this.RB_WaifuAnime.Text = "upconv7 anime";
            this.RB_WaifuAnime.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuCunet
            // 
            this.RB_WaifuCunet.AutoSize = true;
            this.RB_WaifuCunet.Location = new System.Drawing.Point(6, 62);
            this.RB_WaifuCunet.Name = "RB_WaifuCunet";
            this.RB_WaifuCunet.Size = new System.Drawing.Size(54, 16);
            this.RB_WaifuCunet.TabIndex = 0;
            this.RB_WaifuCunet.Text = "cunet";
            this.RB_WaifuCunet.UseVisualStyleBackColor = true;
            // 
            // CB_waifu2x
            // 
            this.CB_waifu2x.AutoSize = true;
            this.CB_waifu2x.Checked = true;
            this.CB_waifu2x.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_waifu2x.Location = new System.Drawing.Point(7, 19);
            this.CB_waifu2x.Name = "CB_waifu2x";
            this.CB_waifu2x.Size = new System.Drawing.Size(97, 16);
            this.CB_waifu2x.TabIndex = 2;
            this.CB_waifu2x.Text = "Use waifu2x ";
            this.CB_waifu2x.UseVisualStyleBackColor = true;
            this.CB_waifu2x.CheckedChanged += new System.EventHandler(this.CB_waifu2x_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.RB_WaifuHighest);
            this.groupBox4.Controls.Add(this.RB_WaifuHigh);
            this.groupBox4.Controls.Add(this.RB_WaifuMed);
            this.groupBox4.Controls.Add(this.RB_WaifuLow);
            this.groupBox4.Location = new System.Drawing.Point(7, 167);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(251, 48);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Denoise Level";
            // 
            // RB_WaifuHighest
            // 
            this.RB_WaifuHighest.AutoSize = true;
            this.RB_WaifuHighest.Checked = true;
            this.RB_WaifuHighest.Location = new System.Drawing.Point(180, 20);
            this.RB_WaifuHighest.Name = "RB_WaifuHighest";
            this.RB_WaifuHighest.Size = new System.Drawing.Size(65, 16);
            this.RB_WaifuHighest.TabIndex = 0;
            this.RB_WaifuHighest.TabStop = true;
            this.RB_WaifuHighest.Text = "Highest";
            this.RB_WaifuHighest.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuHigh
            // 
            this.RB_WaifuHigh.AutoSize = true;
            this.RB_WaifuHigh.Location = new System.Drawing.Point(126, 20);
            this.RB_WaifuHigh.Name = "RB_WaifuHigh";
            this.RB_WaifuHigh.Size = new System.Drawing.Size(48, 16);
            this.RB_WaifuHigh.TabIndex = 0;
            this.RB_WaifuHigh.Text = "High";
            this.RB_WaifuHigh.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuMed
            // 
            this.RB_WaifuMed.AutoSize = true;
            this.RB_WaifuMed.Location = new System.Drawing.Point(54, 20);
            this.RB_WaifuMed.Name = "RB_WaifuMed";
            this.RB_WaifuMed.Size = new System.Drawing.Size(69, 16);
            this.RB_WaifuMed.TabIndex = 0;
            this.RB_WaifuMed.Text = "Medium";
            this.RB_WaifuMed.UseVisualStyleBackColor = true;
            // 
            // RB_WaifuLow
            // 
            this.RB_WaifuLow.AutoSize = true;
            this.RB_WaifuLow.Location = new System.Drawing.Point(6, 20);
            this.RB_WaifuLow.Name = "RB_WaifuLow";
            this.RB_WaifuLow.Size = new System.Drawing.Size(47, 16);
            this.RB_WaifuLow.TabIndex = 0;
            this.RB_WaifuLow.Text = "Low";
            this.RB_WaifuLow.UseVisualStyleBackColor = true;
            // 
            // Label_FileName
            // 
            this.Label_FileName.AutoSize = true;
            this.Label_FileName.Location = new System.Drawing.Point(13, 456);
            this.Label_FileName.Name = "Label_FileName";
            this.Label_FileName.Size = new System.Drawing.Size(108, 12);
            this.Label_FileName.TabIndex = 15;
            this.Label_FileName.Text = "filename.extention";
            this.Label_FileName.Visible = false;
            // 
            // CB_Overwrite
            // 
            this.CB_Overwrite.AutoSize = true;
            this.CB_Overwrite.Location = new System.Drawing.Point(574, 59);
            this.CB_Overwrite.Name = "CB_Overwrite";
            this.CB_Overwrite.Size = new System.Drawing.Size(77, 16);
            this.CB_Overwrite.TabIndex = 16;
            this.CB_Overwrite.Text = "Overwrite";
            this.CB_Overwrite.UseVisualStyleBackColor = true;
            //
            // lbl_BrightnessVal
            //
            this.lbl_BrightnessVal.AutoSize = true;
            this.lbl_BrightnessVal.Location = new System.Drawing.Point(6, 289);
            this.lbl_BrightnessVal.Name = "lbl_BrightnessVal";
            this.lbl_BrightnessVal.Size = new System.Drawing.Size(120, 12);
            this.lbl_BrightnessVal.Text = "Brightness : 0";
            //
            // TB_Brightness
            //
            this.TB_Brightness.Location = new System.Drawing.Point(6, 305);
            this.TB_Brightness.Maximum = 100;
            this.TB_Brightness.Minimum = -100;
            this.TB_Brightness.Name = "TB_Brightness";
            this.TB_Brightness.Size = new System.Drawing.Size(200, 45);
            this.TB_Brightness.TabIndex = 0;
            this.TB_Brightness.TickFrequency = 20;
            this.TB_Brightness.Value = 0;
            this.TB_Brightness.Scroll += new System.EventHandler(this.TB_Brightness_Scroll);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.CB_Overwrite);
            this.Controls.Add(this.Label_FileName);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.BTN_ShowPreview);
            this.Controls.Add(this.CB_SubFolder);
            this.Controls.Add(this.Label_Progress);
            this.Controls.Add(this.PB_Progress);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.BTN_Abort);
            this.Controls.Add(this.BTN_Execute);
            this.Controls.Add(this.BTN_TargetFolder);
            this.Controls.Add(this.TB_Target);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BTN_OpenFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TB_Source);
            this.Controls.Add(this.BTN_OpenFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Scanned Card Denoiser";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TB_Brightness)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar TB_Brightness;
        private System.Windows.Forms.Label lbl_BrightnessVal;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button BTN_OpenFile;
        private System.Windows.Forms.TextBox TB_Source;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTN_OpenFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TB_Target;
        private System.Windows.Forms.Button BTN_TargetFolder;
        private System.Windows.Forms.Button BTN_Execute;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox CB_DenoiseColor;
        private System.Windows.Forms.CheckBox CB_AutoLevel;
        private System.Windows.Forms.TextBox TB_AutoLevelMin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TB_DenoiseSSize;
        private System.Windows.Forms.TextBox TB_DenoiseTSize;
        private System.Windows.Forms.TextBox TB_DenoiseHColor;
        private System.Windows.Forms.TextBox TB_DenoiseH;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TB_AutoLevelMax;
        private System.Windows.Forms.Button BTN_AutoLevelDefault;
        private System.Windows.Forms.Button BTN_DenoiseDefault;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox CB_Clip;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TB_ClipLeft;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox TB_ClipBottom;
        private System.Windows.Forms.TextBox TB_ClipRight;
        private System.Windows.Forms.TextBox TB_ClipTop;
        private System.Windows.Forms.CheckBox CB_ChangeSize;
        private System.Windows.Forms.RadioButton RB_ResizeTradingCard;
        private System.Windows.Forms.RadioButton RB_ResizeCarddass;
        private System.Windows.Forms.RadioButton RB_Resize4x6;
        private System.Windows.Forms.RadioButton RB_ResizeCustom;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox TB_ResizeH;
        private System.Windows.Forms.TextBox TB_ResizeW;
        private System.Windows.Forms.CheckBox CB_CornerRounding;
        private System.Windows.Forms.CheckBox CB_EdgeLine;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox TB_CornerRounding;
        private System.Windows.Forms.ProgressBar PB_Progress;
        private System.Windows.Forms.Label Label_Progress;
        private System.Windows.Forms.Button BTN_Abort;
        private System.Windows.Forms.CheckBox CB_SubFolder;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button BTN_ShowPreview;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton RB_WaifuHighest;
        private System.Windows.Forms.RadioButton RB_WaifuHigh;
        private System.Windows.Forms.RadioButton RB_WaifuMed;
        private System.Windows.Forms.RadioButton RB_WaifuLow;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton RB_WaifuAnime;
        private System.Windows.Forms.RadioButton RB_WaifuCunet;
        private System.Windows.Forms.CheckBox CB_waifu2x;
        private System.Windows.Forms.RadioButton RB_WaifuPhoto;
        private System.Windows.Forms.Button BTN_waifu2x;
        private System.Windows.Forms.TextBox TB_AdjThreshold;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label Label_FileName;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox CB_Waifu2xTTA;
        private System.Windows.Forms.CheckBox CB_Overwrite;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton RB_ManualAdjust;
        private System.Windows.Forms.RadioButton RB_AutoAdjust;
        private System.Windows.Forms.RadioButton RB_NoneAdjust;
    }
}

