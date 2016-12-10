namespace WdCameraViewer
{
    partial class WdCameraViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cameraViewer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.cameraViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraViewer
            // 
            this.cameraViewer.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.cameraViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraViewer.Location = new System.Drawing.Point(0, 0);
            this.cameraViewer.Name = "cameraViewer";
            this.cameraViewer.Size = new System.Drawing.Size(150, 150);
            this.cameraViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.cameraViewer.TabIndex = 0;
            this.cameraViewer.TabStop = false;
            // 
            // WdCameraViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cameraViewer);
            this.Name = "WdCameraViewer";
            ((System.ComponentModel.ISupportInitialize)(this.cameraViewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox cameraViewer;
    }
}
