namespace ScannedCardDenoiser
{
    partial class CamScanForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamScanForm));
            this.BTN_Confirm = new System.Windows.Forms.Button();
            this.PIC_Main = new System.Windows.Forms.PictureBox();
            this.TopLeft = new System.Windows.Forms.PictureBox();
            this.TopRight = new System.Windows.Forms.PictureBox();
            this.TopCenter = new System.Windows.Forms.PictureBox();
            this.BottomLeft = new System.Windows.Forms.PictureBox();
            this.BottomRight = new System.Windows.Forms.PictureBox();
            this.BottomCenter = new System.Windows.Forms.PictureBox();
            this.CenterLeft = new System.Windows.Forms.PictureBox();
            this.CenterRight = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PIC_Main)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopCenter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomCenter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterRight)).BeginInit();
            this.SuspendLayout();
            // 
            // BTN_Confirm
            // 
            this.BTN_Confirm.Location = new System.Drawing.Point(585, 658);
            this.BTN_Confirm.Name = "BTN_Confirm";
            this.BTN_Confirm.Size = new System.Drawing.Size(67, 36);
            this.BTN_Confirm.TabIndex = 0;
            this.BTN_Confirm.Text = "완료";
            this.BTN_Confirm.UseVisualStyleBackColor = true;
            this.BTN_Confirm.Click += new System.EventHandler(this.BTN_Confirm_Click);
            // 
            // PIC_Main
            // 
            this.PIC_Main.Location = new System.Drawing.Point(12, 12);
            this.PIC_Main.Name = "PIC_Main";
            this.PIC_Main.Size = new System.Drawing.Size(640, 640);
            this.PIC_Main.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PIC_Main.TabIndex = 1;
            this.PIC_Main.TabStop = false;
            this.PIC_Main.Paint += new System.Windows.Forms.PaintEventHandler(this.PIC_Main_Paint);
            this.PIC_Main.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PIC_Main_MouseDown);
            this.PIC_Main.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PIC_Main_MouseMove);
            this.PIC_Main.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PIC_Main_MouseUp);
            // 
            // TopLeft
            // 
            this.TopLeft.BackColor = System.Drawing.Color.Transparent;
            this.TopLeft.Image = ((System.Drawing.Image)(resources.GetObject("TopLeft.Image")));
            this.TopLeft.InitialImage = ((System.Drawing.Image)(resources.GetObject("TopLeft.InitialImage")));
            this.TopLeft.Location = new System.Drawing.Point(2, 2);
            this.TopLeft.Name = "TopLeft";
            this.TopLeft.Size = new System.Drawing.Size(20, 20);
            this.TopLeft.TabIndex = 2;
            this.TopLeft.TabStop = false;
            this.TopLeft.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.TopLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopLeftMouseDown);
            this.TopLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopLeftMouseMove);
            this.TopLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopLeftMouseUp);
            // 
            // TopRight
            // 
            this.TopRight.BackColor = System.Drawing.Color.Transparent;
            this.TopRight.Image = ((System.Drawing.Image)(resources.GetObject("TopRight.Image")));
            this.TopRight.InitialImage = ((System.Drawing.Image)(resources.GetObject("TopRight.InitialImage")));
            this.TopRight.Location = new System.Drawing.Point(642, 2);
            this.TopRight.Name = "TopRight";
            this.TopRight.Size = new System.Drawing.Size(20, 20);
            this.TopRight.TabIndex = 2;
            this.TopRight.TabStop = false;
            this.TopRight.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.TopRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopRightMouseDown);
            this.TopRight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopRightMouseMove);
            this.TopRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopRightMouseUp);
            // 
            // TopCenter
            // 
            this.TopCenter.BackColor = System.Drawing.Color.Transparent;
            this.TopCenter.Image = ((System.Drawing.Image)(resources.GetObject("TopCenter.Image")));
            this.TopCenter.InitialImage = ((System.Drawing.Image)(resources.GetObject("TopCenter.InitialImage")));
            this.TopCenter.Location = new System.Drawing.Point(322, 2);
            this.TopCenter.Name = "TopCenter";
            this.TopCenter.Size = new System.Drawing.Size(20, 20);
            this.TopCenter.TabIndex = 2;
            this.TopCenter.TabStop = false;
            this.TopCenter.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.TopCenter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopCenterMouseDown);
            this.TopCenter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopCenterMouseMove);
            this.TopCenter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopCenterMouseUp);
            // 
            // BottomLeft
            // 
            this.BottomLeft.BackColor = System.Drawing.Color.Transparent;
            this.BottomLeft.Image = ((System.Drawing.Image)(resources.GetObject("BottomLeft.Image")));
            this.BottomLeft.InitialImage = ((System.Drawing.Image)(resources.GetObject("BottomLeft.InitialImage")));
            this.BottomLeft.Location = new System.Drawing.Point(2, 642);
            this.BottomLeft.Name = "BottomLeft";
            this.BottomLeft.Size = new System.Drawing.Size(20, 20);
            this.BottomLeft.TabIndex = 2;
            this.BottomLeft.TabStop = false;
            this.BottomLeft.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.BottomLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BottomLeftMouseDown);
            this.BottomLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BottomLeftMouseMove);
            this.BottomLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BottomLeftMouseUp);
            // 
            // BottomRight
            // 
            this.BottomRight.BackColor = System.Drawing.Color.Transparent;
            this.BottomRight.Image = ((System.Drawing.Image)(resources.GetObject("BottomRight.Image")));
            this.BottomRight.InitialImage = ((System.Drawing.Image)(resources.GetObject("BottomRight.InitialImage")));
            this.BottomRight.Location = new System.Drawing.Point(642, 642);
            this.BottomRight.Name = "BottomRight";
            this.BottomRight.Size = new System.Drawing.Size(20, 20);
            this.BottomRight.TabIndex = 2;
            this.BottomRight.TabStop = false;
            this.BottomRight.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.BottomRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BottomRightMouseDown);
            this.BottomRight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BottomRightMouseMove);
            this.BottomRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BottomRightMouseUp);
            // 
            // BottomCenter
            // 
            this.BottomCenter.BackColor = System.Drawing.Color.Transparent;
            this.BottomCenter.Image = ((System.Drawing.Image)(resources.GetObject("BottomCenter.Image")));
            this.BottomCenter.InitialImage = ((System.Drawing.Image)(resources.GetObject("BottomCenter.InitialImage")));
            this.BottomCenter.Location = new System.Drawing.Point(322, 642);
            this.BottomCenter.Name = "BottomCenter";
            this.BottomCenter.Size = new System.Drawing.Size(20, 20);
            this.BottomCenter.TabIndex = 2;
            this.BottomCenter.TabStop = false;
            this.BottomCenter.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.BottomCenter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BottomCenterMouseDown);
            this.BottomCenter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BottomCenterMouseMove);
            this.BottomCenter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BottomCenterMouseUp);
            // 
            // CenterLeft
            // 
            this.CenterLeft.BackColor = System.Drawing.Color.Transparent;
            this.CenterLeft.Image = ((System.Drawing.Image)(resources.GetObject("CenterLeft.Image")));
            this.CenterLeft.InitialImage = ((System.Drawing.Image)(resources.GetObject("CenterLeft.InitialImage")));
            this.CenterLeft.Location = new System.Drawing.Point(2, 322);
            this.CenterLeft.Name = "CenterLeft";
            this.CenterLeft.Size = new System.Drawing.Size(20, 20);
            this.CenterLeft.TabIndex = 2;
            this.CenterLeft.TabStop = false;
            this.CenterLeft.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.CenterLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CenterLeftMouseDown);
            this.CenterLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CenterLeftMouseMove);
            this.CenterLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CenterLeftMouseUp);
            // 
            // CenterRight
            // 
            this.CenterRight.BackColor = System.Drawing.Color.Transparent;
            this.CenterRight.Image = ((System.Drawing.Image)(resources.GetObject("CenterRight.Image")));
            this.CenterRight.InitialImage = ((System.Drawing.Image)(resources.GetObject("CenterRight.InitialImage")));
            this.CenterRight.Location = new System.Drawing.Point(642, 322);
            this.CenterRight.Name = "CenterRight";
            this.CenterRight.Size = new System.Drawing.Size(20, 20);
            this.CenterRight.TabIndex = 2;
            this.CenterRight.TabStop = false;
            this.CenterRight.LocationChanged += new System.EventHandler(this.PointLocationChanged);
            this.CenterRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CenterRightMouseDown);
            this.CenterRight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CenterRightMouseMove);
            this.CenterRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CenterRightMouseUp);
            // 
            // CamScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 699);
            this.Controls.Add(this.CenterRight);
            this.Controls.Add(this.TopRight);
            this.Controls.Add(this.TopCenter);
            this.Controls.Add(this.BottomCenter);
            this.Controls.Add(this.BottomRight);
            this.Controls.Add(this.CenterLeft);
            this.Controls.Add(this.BottomLeft);
            this.Controls.Add(this.TopLeft);
            this.Controls.Add(this.BTN_Confirm);
            this.Controls.Add(this.PIC_Main);
            this.Name = "CamScanForm";
            this.Text = "CamScanForm";
            ((System.ComponentModel.ISupportInitialize)(this.PIC_Main)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopCenter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomCenter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterRight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTN_Confirm;
        private System.Windows.Forms.PictureBox PIC_Main;
        private System.Windows.Forms.PictureBox TopLeft;
        private System.Windows.Forms.PictureBox TopRight;
        private System.Windows.Forms.PictureBox TopCenter;
        private System.Windows.Forms.PictureBox BottomLeft;
        private System.Windows.Forms.PictureBox BottomRight;
        private System.Windows.Forms.PictureBox BottomCenter;
        private System.Windows.Forms.PictureBox CenterLeft;
        private System.Windows.Forms.PictureBox CenterRight;
    }
}