namespace MNX_Main
{
    partial class MNXtoSVGForm1
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
            if(disposing && (components != null))
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
            this.MNXLabel = new System.Windows.Forms.Label();
            this.MNXSelect = new System.Windows.Forms.ComboBox();
            this.PaperWidthLabel = new System.Windows.Forms.Label();
            this.PaperSizeGroupBox = new System.Windows.Forms.GroupBox();
            this.PaperHeightTextBox = new System.Windows.Forms.TextBox();
            this.PaperWidthTextBox = new System.Windows.Forms.TextBox();
            this.PaperHeightLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.MarginsGroupBox = new System.Windows.Forms.GroupBox();
            this.MarginTopPage1TextBox = new System.Windows.Forms.TextBox();
            this.MarginTopPage1Label = new System.Windows.Forms.Label();
            this.MarginBottomLabel = new System.Windows.Forms.Label();
            this.MarginLeftLabel = new System.Windows.Forms.Label();
            this.MarginBottomTextBox = new System.Windows.Forms.TextBox();
            this.MarginLeftTextBox = new System.Windows.Forms.TextBox();
            this.MarginRightTextBox = new System.Windows.Forms.TextBox();
            this.MarginTopOtherPagesTextBox = new System.Windows.Forms.TextBox();
            this.MarginRightLabel = new System.Windows.Forms.Label();
            this.MarginTopOtherPagesLabel = new System.Windows.Forms.Label();
            this.SaveSpeedAndPageSettingsButton = new System.Windows.Forms.Button();
            this.NotationGroupBox = new System.Windows.Forms.GroupBox();
            this.MinimumGapsBetweenSystemsTextBox = new System.Windows.Forms.TextBox();
            this.MinimumGapsBetweenStavesTextBox = new System.Windows.Forms.TextBox();
            this.GapSizeComboBox = new System.Windows.Forms.ComboBox();
            this.StafflineStemStrokeWidthComboBox = new System.Windows.Forms.ComboBox();
            this.MinimumGapsBetweenSystemsLabel = new System.Windows.Forms.Label();
            this.MinimumGapsBetweenStavesLabel = new System.Windows.Forms.Label();
            this.GapSizeLabel = new System.Windows.Forms.Label();
            this.StafflineAndStemStrokeWidthLabel = new System.Windows.Forms.Label();
            this.CrotchetsPerMinuteLabel = new System.Windows.Forms.Label();
            this.CrotchetsPerMinuteTextBox = new System.Windows.Forms.TextBox();
            this.SpeedGroupBox = new System.Windows.Forms.GroupBox();
            this.WriteAllSVGFilesButton = new System.Windows.Forms.Button();
            this.WriteSVGButton = new System.Windows.Forms.Button();
            this.PaperSizeGroupBox.SuspendLayout();
            this.MarginsGroupBox.SuspendLayout();
            this.NotationGroupBox.SuspendLayout();
            this.SpeedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MNXLabel
            // 
            this.MNXLabel.AutoSize = true;
            this.MNXLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MNXLabel.Location = new System.Drawing.Point(144, 30);
            this.MNXLabel.Name = "MNXLabel";
            this.MNXLabel.Size = new System.Drawing.Size(31, 15);
            this.MNXLabel.TabIndex = 1;
            this.MNXLabel.Text = "mnx";
            // 
            // MNXSelect
            // 
            this.MNXSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MNXSelect.FormattingEnabled = true;
            this.MNXSelect.Location = new System.Drawing.Point(176, 27);
            this.MNXSelect.Name = "MNXSelect";
            this.MNXSelect.Size = new System.Drawing.Size(222, 23);
            this.MNXSelect.TabIndex = 2;
            // 
            // PaperWidthLabel
            // 
            this.PaperWidthLabel.AutoSize = true;
            this.PaperWidthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PaperWidthLabel.Location = new System.Drawing.Point(20, 21);
            this.PaperWidthLabel.Name = "PaperWidthLabel";
            this.PaperWidthLabel.Size = new System.Drawing.Size(32, 13);
            this.PaperWidthLabel.TabIndex = 3;
            this.PaperWidthLabel.Text = "width";
            // 
            // PaperSizeGroupBox
            // 
            this.PaperSizeGroupBox.Controls.Add(this.PaperHeightTextBox);
            this.PaperSizeGroupBox.Controls.Add(this.PaperWidthTextBox);
            this.PaperSizeGroupBox.Controls.Add(this.PaperHeightLabel);
            this.PaperSizeGroupBox.Controls.Add(this.PaperWidthLabel);
            this.PaperSizeGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PaperSizeGroupBox.Location = new System.Drawing.Point(34, 86);
            this.PaperSizeGroupBox.Name = "PaperSizeGroupBox";
            this.PaperSizeGroupBox.Size = new System.Drawing.Size(223, 77);
            this.PaperSizeGroupBox.TabIndex = 4;
            this.PaperSizeGroupBox.TabStop = false;
            this.PaperSizeGroupBox.Text = "paper size";
            // 
            // PaperHeightTextBox
            // 
            this.PaperHeightTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PaperHeightTextBox.Location = new System.Drawing.Point(117, 37);
            this.PaperHeightTextBox.Name = "PaperHeightTextBox";
            this.PaperHeightTextBox.Size = new System.Drawing.Size(88, 21);
            this.PaperHeightTextBox.TabIndex = 6;
            // 
            // PaperWidthTextBox
            // 
            this.PaperWidthTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PaperWidthTextBox.Location = new System.Drawing.Point(23, 37);
            this.PaperWidthTextBox.Name = "PaperWidthTextBox";
            this.PaperWidthTextBox.Size = new System.Drawing.Size(88, 21);
            this.PaperWidthTextBox.TabIndex = 5;
            // 
            // PaperHeightLabel
            // 
            this.PaperHeightLabel.AutoSize = true;
            this.PaperHeightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PaperHeightLabel.Location = new System.Drawing.Point(114, 21);
            this.PaperHeightLabel.Name = "PaperHeightLabel";
            this.PaperHeightLabel.Size = new System.Drawing.Size(36, 13);
            this.PaperHeightLabel.TabIndex = 4;
            this.PaperHeightLabel.Text = "height";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(148, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(278, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "All spatial dimensions are in screen pixels (100%)";
            // 
            // MarginsGroupBox
            // 
            this.MarginsGroupBox.Controls.Add(this.MarginTopPage1TextBox);
            this.MarginsGroupBox.Controls.Add(this.MarginTopPage1Label);
            this.MarginsGroupBox.Controls.Add(this.MarginBottomLabel);
            this.MarginsGroupBox.Controls.Add(this.MarginLeftLabel);
            this.MarginsGroupBox.Controls.Add(this.MarginBottomTextBox);
            this.MarginsGroupBox.Controls.Add(this.MarginLeftTextBox);
            this.MarginsGroupBox.Controls.Add(this.MarginRightTextBox);
            this.MarginsGroupBox.Controls.Add(this.MarginTopOtherPagesTextBox);
            this.MarginsGroupBox.Controls.Add(this.MarginRightLabel);
            this.MarginsGroupBox.Controls.Add(this.MarginTopOtherPagesLabel);
            this.MarginsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginsGroupBox.Location = new System.Drawing.Point(34, 169);
            this.MarginsGroupBox.Name = "MarginsGroupBox";
            this.MarginsGroupBox.Size = new System.Drawing.Size(223, 191);
            this.MarginsGroupBox.TabIndex = 7;
            this.MarginsGroupBox.TabStop = false;
            this.MarginsGroupBox.Text = "margins";
            // 
            // MarginTopPage1TextBox
            // 
            this.MarginTopPage1TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginTopPage1TextBox.Location = new System.Drawing.Point(82, 36);
            this.MarginTopPage1TextBox.Name = "MarginTopPage1TextBox";
            this.MarginTopPage1TextBox.Size = new System.Drawing.Size(60, 21);
            this.MarginTopPage1TextBox.TabIndex = 12;
            // 
            // MarginTopPage1Label
            // 
            this.MarginTopPage1Label.AutoSize = true;
            this.MarginTopPage1Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginTopPage1Label.Location = new System.Drawing.Point(77, 20);
            this.MarginTopPage1Label.Name = "MarginTopPage1Label";
            this.MarginTopPage1Label.Size = new System.Drawing.Size(70, 13);
            this.MarginTopPage1Label.TabIndex = 11;
            this.MarginTopPage1Label.Text = "top ( page 1 )";
            // 
            // MarginBottomLabel
            // 
            this.MarginBottomLabel.AutoSize = true;
            this.MarginBottomLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginBottomLabel.Location = new System.Drawing.Point(93, 133);
            this.MarginBottomLabel.Name = "MarginBottomLabel";
            this.MarginBottomLabel.Size = new System.Drawing.Size(39, 13);
            this.MarginBottomLabel.TabIndex = 10;
            this.MarginBottomLabel.Text = "bottom";
            // 
            // MarginLeftLabel
            // 
            this.MarginLeftLabel.AutoSize = true;
            this.MarginLeftLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginLeftLabel.Location = new System.Drawing.Point(42, 94);
            this.MarginLeftLabel.Name = "MarginLeftLabel";
            this.MarginLeftLabel.Size = new System.Drawing.Size(21, 13);
            this.MarginLeftLabel.TabIndex = 9;
            this.MarginLeftLabel.Text = "left";
            // 
            // MarginBottomTextBox
            // 
            this.MarginBottomTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginBottomTextBox.Location = new System.Drawing.Point(82, 148);
            this.MarginBottomTextBox.Name = "MarginBottomTextBox";
            this.MarginBottomTextBox.Size = new System.Drawing.Size(60, 21);
            this.MarginBottomTextBox.TabIndex = 8;
            // 
            // MarginLeftTextBox
            // 
            this.MarginLeftTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginLeftTextBox.Location = new System.Drawing.Point(22, 110);
            this.MarginLeftTextBox.Name = "MarginLeftTextBox";
            this.MarginLeftTextBox.Size = new System.Drawing.Size(60, 21);
            this.MarginLeftTextBox.TabIndex = 7;
            // 
            // MarginRightTextBox
            // 
            this.MarginRightTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginRightTextBox.Location = new System.Drawing.Point(142, 110);
            this.MarginRightTextBox.Name = "MarginRightTextBox";
            this.MarginRightTextBox.Size = new System.Drawing.Size(60, 21);
            this.MarginRightTextBox.TabIndex = 6;
            // 
            // MarginTopOtherPagesTextBox
            // 
            this.MarginTopOtherPagesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginTopOtherPagesTextBox.Location = new System.Drawing.Point(82, 74);
            this.MarginTopOtherPagesTextBox.Name = "MarginTopOtherPagesTextBox";
            this.MarginTopOtherPagesTextBox.Size = new System.Drawing.Size(60, 21);
            this.MarginTopOtherPagesTextBox.TabIndex = 5;
            // 
            // MarginRightLabel
            // 
            this.MarginRightLabel.AutoSize = true;
            this.MarginRightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginRightLabel.Location = new System.Drawing.Point(159, 94);
            this.MarginRightLabel.Name = "MarginRightLabel";
            this.MarginRightLabel.Size = new System.Drawing.Size(27, 13);
            this.MarginRightLabel.TabIndex = 4;
            this.MarginRightLabel.Text = "right";
            // 
            // MarginTopOtherPagesLabel
            // 
            this.MarginTopOtherPagesLabel.AutoSize = true;
            this.MarginTopOtherPagesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarginTopOtherPagesLabel.Location = new System.Drawing.Point(66, 58);
            this.MarginTopOtherPagesLabel.Name = "MarginTopOtherPagesLabel";
            this.MarginTopOtherPagesLabel.Size = new System.Drawing.Size(93, 13);
            this.MarginTopOtherPagesLabel.TabIndex = 3;
            this.MarginTopOtherPagesLabel.Text = "top ( other pages )";
            // 
            // SaveSpeedAndPageSettingsButton
            // 
            this.SaveSpeedAndPageSettingsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveSpeedAndPageSettingsButton.Location = new System.Drawing.Point(193, 372);
            this.SaveSpeedAndPageSettingsButton.Name = "SaveSpeedAndPageSettingsButton";
            this.SaveSpeedAndPageSettingsButton.Size = new System.Drawing.Size(183, 38);
            this.SaveSpeedAndPageSettingsButton.TabIndex = 8;
            this.SaveSpeedAndPageSettingsButton.Text = "Save Speed and Page Settings";
            this.SaveSpeedAndPageSettingsButton.UseVisualStyleBackColor = true;
            // 
            // NotationGroupBox
            // 
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenSystemsTextBox);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenStavesTextBox);
            this.NotationGroupBox.Controls.Add(this.GapSizeComboBox);
            this.NotationGroupBox.Controls.Add(this.StafflineStemStrokeWidthComboBox);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenSystemsLabel);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenStavesLabel);
            this.NotationGroupBox.Controls.Add(this.GapSizeLabel);
            this.NotationGroupBox.Controls.Add(this.StafflineAndStemStrokeWidthLabel);
            this.NotationGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NotationGroupBox.Location = new System.Drawing.Point(279, 86);
            this.NotationGroupBox.Name = "NotationGroupBox";
            this.NotationGroupBox.Size = new System.Drawing.Size(255, 167);
            this.NotationGroupBox.TabIndex = 9;
            this.NotationGroupBox.TabStop = false;
            this.NotationGroupBox.Text = "notation";
            // 
            // MinimumGapsBetweenSystemsTextBox
            // 
            this.MinimumGapsBetweenSystemsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumGapsBetweenSystemsTextBox.Location = new System.Drawing.Point(175, 119);
            this.MinimumGapsBetweenSystemsTextBox.Name = "MinimumGapsBetweenSystemsTextBox";
            this.MinimumGapsBetweenSystemsTextBox.Size = new System.Drawing.Size(58, 21);
            this.MinimumGapsBetweenSystemsTextBox.TabIndex = 14;
            // 
            // MinimumGapsBetweenStavesTextBox
            // 
            this.MinimumGapsBetweenStavesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumGapsBetweenStavesTextBox.Location = new System.Drawing.Point(175, 91);
            this.MinimumGapsBetweenStavesTextBox.Name = "MinimumGapsBetweenStavesTextBox";
            this.MinimumGapsBetweenStavesTextBox.Size = new System.Drawing.Size(58, 21);
            this.MinimumGapsBetweenStavesTextBox.TabIndex = 13;
            // 
            // GapSizeComboBox
            // 
            this.GapSizeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GapSizeComboBox.FormattingEnabled = true;
            this.GapSizeComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "2.5",
            "3",
            "3.5",
            "4",
            "5",
            "6",
            "7",
            "8",
            "10",
            "12",
            "14",
            "16",
            "18",
            "20",
            "24",
            "28"});
            this.GapSizeComboBox.Location = new System.Drawing.Point(175, 61);
            this.GapSizeComboBox.Name = "GapSizeComboBox";
            this.GapSizeComboBox.Size = new System.Drawing.Size(58, 23);
            this.GapSizeComboBox.TabIndex = 12;
            // 
            // StafflineStemStrokeWidthComboBox
            // 
            this.StafflineStemStrokeWidthComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StafflineStemStrokeWidthComboBox.FormattingEnabled = true;
            this.StafflineStemStrokeWidthComboBox.Items.AddRange(new object[] {
            "0.25",
            "0.5",
            "1.0",
            "1.5",
            "2.0"});
            this.StafflineStemStrokeWidthComboBox.Location = new System.Drawing.Point(175, 32);
            this.StafflineStemStrokeWidthComboBox.Name = "StafflineStemStrokeWidthComboBox";
            this.StafflineStemStrokeWidthComboBox.Size = new System.Drawing.Size(58, 23);
            this.StafflineStemStrokeWidthComboBox.TabIndex = 11;
            // 
            // MinimumGapsBetweenSystemsLabel
            // 
            this.MinimumGapsBetweenSystemsLabel.AutoSize = true;
            this.MinimumGapsBetweenSystemsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumGapsBetweenSystemsLabel.Location = new System.Drawing.Point(12, 122);
            this.MinimumGapsBetweenSystemsLabel.Name = "MinimumGapsBetweenSystemsLabel";
            this.MinimumGapsBetweenSystemsLabel.Size = new System.Drawing.Size(157, 13);
            this.MinimumGapsBetweenSystemsLabel.TabIndex = 10;
            this.MinimumGapsBetweenSystemsLabel.Text = "minimum gaps between systems";
            // 
            // MinimumGapsBetweenStavesLabel
            // 
            this.MinimumGapsBetweenStavesLabel.AutoSize = true;
            this.MinimumGapsBetweenStavesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumGapsBetweenStavesLabel.Location = new System.Drawing.Point(18, 94);
            this.MinimumGapsBetweenStavesLabel.Name = "MinimumGapsBetweenStavesLabel";
            this.MinimumGapsBetweenStavesLabel.Size = new System.Drawing.Size(151, 13);
            this.MinimumGapsBetweenStavesLabel.TabIndex = 9;
            this.MinimumGapsBetweenStavesLabel.Text = "minimum gaps between staves";
            // 
            // GapSizeLabel
            // 
            this.GapSizeLabel.AutoSize = true;
            this.GapSizeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GapSizeLabel.Location = new System.Drawing.Point(123, 64);
            this.GapSizeLabel.Name = "GapSizeLabel";
            this.GapSizeLabel.Size = new System.Drawing.Size(46, 13);
            this.GapSizeLabel.TabIndex = 8;
            this.GapSizeLabel.Text = "gap size";
            // 
            // StafflineAndStemStrokeWidthLabel
            // 
            this.StafflineAndStemStrokeWidthLabel.AutoSize = true;
            this.StafflineAndStemStrokeWidthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StafflineAndStemStrokeWidthLabel.Location = new System.Drawing.Point(20, 35);
            this.StafflineAndStemStrokeWidthLabel.Name = "StafflineAndStemStrokeWidthLabel";
            this.StafflineAndStemStrokeWidthLabel.Size = new System.Drawing.Size(149, 13);
            this.StafflineAndStemStrokeWidthLabel.TabIndex = 7;
            this.StafflineAndStemStrokeWidthLabel.Text = "staffline and stem stroke width";
            // 
            // CrotchetsPerMinuteLabel
            // 
            this.CrotchetsPerMinuteLabel.AutoSize = true;
            this.CrotchetsPerMinuteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CrotchetsPerMinuteLabel.Location = new System.Drawing.Point(66, 21);
            this.CrotchetsPerMinuteLabel.Name = "CrotchetsPerMinuteLabel";
            this.CrotchetsPerMinuteLabel.Size = new System.Drawing.Size(103, 13);
            this.CrotchetsPerMinuteLabel.TabIndex = 3;
            this.CrotchetsPerMinuteLabel.Text = "crotchets per minute";
            // 
            // CrotchetsPerMinuteTextBox
            // 
            this.CrotchetsPerMinuteTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CrotchetsPerMinuteTextBox.Location = new System.Drawing.Point(175, 18);
            this.CrotchetsPerMinuteTextBox.Name = "CrotchetsPerMinuteTextBox";
            this.CrotchetsPerMinuteTextBox.Size = new System.Drawing.Size(58, 21);
            this.CrotchetsPerMinuteTextBox.TabIndex = 4;
            // 
            // SpeedGroupBox
            // 
            this.SpeedGroupBox.Controls.Add(this.CrotchetsPerMinuteTextBox);
            this.SpeedGroupBox.Controls.Add(this.CrotchetsPerMinuteLabel);
            this.SpeedGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpeedGroupBox.Location = new System.Drawing.Point(279, 309);
            this.SpeedGroupBox.Name = "SpeedGroupBox";
            this.SpeedGroupBox.Size = new System.Drawing.Size(255, 51);
            this.SpeedGroupBox.TabIndex = 7;
            this.SpeedGroupBox.TabStop = false;
            this.SpeedGroupBox.Text = "speed";
            // 
            // WriteAllSVGFilesButton
            // 
            this.WriteAllSVGFilesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WriteAllSVGFilesButton.Location = new System.Drawing.Point(35, 372);
            this.WriteAllSVGFilesButton.Name = "WriteAllSVGFilesButton";
            this.WriteAllSVGFilesButton.Size = new System.Drawing.Size(129, 38);
            this.WriteAllSVGFilesButton.TabIndex = 10;
            this.WriteAllSVGFilesButton.Text = "Write all SVG files";
            this.WriteAllSVGFilesButton.UseVisualStyleBackColor = true;
            this.WriteAllSVGFilesButton.Click += new System.EventHandler(this.WriteAllSVGFilesButton_Click);
            // 
            // WriteSVGButton
            // 
            this.WriteSVGButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WriteSVGButton.Location = new System.Drawing.Point(405, 372);
            this.WriteSVGButton.Name = "WriteSVGButton";
            this.WriteSVGButton.Size = new System.Drawing.Size(129, 38);
            this.WriteSVGButton.TabIndex = 0;
            this.WriteSVGButton.Text = "Write SVG";
            this.WriteSVGButton.UseVisualStyleBackColor = true;
            this.WriteSVGButton.Click += new System.EventHandler(this.WriteSVGButton_Click);
            // 
            // MNXtoSVGForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(568, 435);
            this.Controls.Add(this.SpeedGroupBox);
            this.Controls.Add(this.WriteAllSVGFilesButton);
            this.Controls.Add(this.NotationGroupBox);
            this.Controls.Add(this.SaveSpeedAndPageSettingsButton);
            this.Controls.Add(this.MarginsGroupBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.PaperSizeGroupBox);
            this.Controls.Add(this.MNXSelect);
            this.Controls.Add(this.MNXLabel);
            this.Controls.Add(this.WriteSVGButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MNXtoSVGForm1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MNXtoSVG";
            this.PaperSizeGroupBox.ResumeLayout(false);
            this.PaperSizeGroupBox.PerformLayout();
            this.MarginsGroupBox.ResumeLayout(false);
            this.MarginsGroupBox.PerformLayout();
            this.NotationGroupBox.ResumeLayout(false);
            this.NotationGroupBox.PerformLayout();
            this.SpeedGroupBox.ResumeLayout(false);
            this.SpeedGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label MNXLabel;
        private System.Windows.Forms.ComboBox MNXSelect;
        private System.Windows.Forms.Label PaperWidthLabel;
        private System.Windows.Forms.GroupBox PaperSizeGroupBox;
        private System.Windows.Forms.TextBox PaperHeightTextBox;
        private System.Windows.Forms.TextBox PaperWidthTextBox;
        private System.Windows.Forms.Label PaperHeightLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox MarginsGroupBox;
        private System.Windows.Forms.TextBox MarginTopPage1TextBox;
        private System.Windows.Forms.Label MarginTopPage1Label;
        private System.Windows.Forms.Label MarginBottomLabel;
        private System.Windows.Forms.Label MarginLeftLabel;
        private System.Windows.Forms.TextBox MarginBottomTextBox;
        private System.Windows.Forms.TextBox MarginLeftTextBox;
        private System.Windows.Forms.TextBox MarginRightTextBox;
        private System.Windows.Forms.TextBox MarginTopOtherPagesTextBox;
        private System.Windows.Forms.Label MarginRightLabel;
        private System.Windows.Forms.Label MarginTopOtherPagesLabel;
        private System.Windows.Forms.Button SaveSpeedAndPageSettingsButton;
        private System.Windows.Forms.GroupBox NotationGroupBox;
        private System.Windows.Forms.TextBox MinimumGapsBetweenSystemsTextBox;
        private System.Windows.Forms.TextBox MinimumGapsBetweenStavesTextBox;
        private System.Windows.Forms.ComboBox GapSizeComboBox;
        private System.Windows.Forms.ComboBox StafflineStemStrokeWidthComboBox;
        private System.Windows.Forms.Label MinimumGapsBetweenSystemsLabel;
        private System.Windows.Forms.Label MinimumGapsBetweenStavesLabel;
        private System.Windows.Forms.Label GapSizeLabel;
        private System.Windows.Forms.Label StafflineAndStemStrokeWidthLabel;
        private System.Windows.Forms.Label CrotchetsPerMinuteLabel;
        private System.Windows.Forms.TextBox CrotchetsPerMinuteTextBox;
        private System.Windows.Forms.GroupBox SpeedGroupBox;
        private System.Windows.Forms.Button WriteAllSVGFilesButton;
        private System.Windows.Forms.Button WriteSVGButton;
    }
}

