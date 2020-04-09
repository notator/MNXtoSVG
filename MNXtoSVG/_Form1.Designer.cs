namespace MNXtoSVG
{
    partial class _Form1
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
            this.compileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // compileButton
            // 
            this.compileButton.Font = new System.Drawing.Font("Verdana", 12F);
            this.compileButton.Location = new System.Drawing.Point(67, 12);
            this.compileButton.Name = "compileButton";
            this.compileButton.Size = new System.Drawing.Size(110, 52);
            this.compileButton.TabIndex = 0;
            this.compileButton.Text = "go";
            this.compileButton.UseVisualStyleBackColor = true;
            this.compileButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 77);
            this.Controls.Add(this.compileButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MNXtoSVG";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button compileButton;
    }
}

