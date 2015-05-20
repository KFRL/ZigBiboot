namespace Programmer
{
    partial class MainUI
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
            this.fileNameBox = new System.Windows.Forms.TextBox();
            this.browseBtn = new System.Windows.Forms.Button();
            this.openHexFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.hexFileLabel = new System.Windows.Forms.Label();
            this.UploadBtn = new System.Windows.Forms.Button();
            this.comPortComboBox = new System.Windows.Forms.ComboBox();
            this.comPortLabel = new System.Windows.Forms.Label();
            this.hostZigBeeAddressLbl = new System.Windows.Forms.Label();
            this.hostZigBeeAddressTextBox = new System.Windows.Forms.TextBox();
            this.targetZigBeeAddressLbl = new System.Windows.Forms.Label();
            this.targetZigBeeAddressTextBox = new System.Windows.Forms.TextBox();
            this.updateAddressesBtn = new System.Windows.Forms.Button();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // fileNameBox
            // 
            this.fileNameBox.Location = new System.Drawing.Point(25, 42);
            this.fileNameBox.Name = "fileNameBox";
            this.fileNameBox.Size = new System.Drawing.Size(146, 20);
            this.fileNameBox.TabIndex = 0;
            // 
            // browseBtn
            // 
            this.browseBtn.Location = new System.Drawing.Point(177, 42);
            this.browseBtn.Name = "browseBtn";
            this.browseBtn.Size = new System.Drawing.Size(75, 20);
            this.browseBtn.TabIndex = 1;
            this.browseBtn.Text = "Browse";
            this.browseBtn.UseVisualStyleBackColor = true;
            this.browseBtn.Click += new System.EventHandler(this.browseBtn_Click);
            // 
            // openHexFileDialog
            // 
            this.openHexFileDialog.FileName = "openHexFileDialog";
            this.openHexFileDialog.Filter = "Hex Files (*.hex)|*.hex";
            // 
            // hexFileLabel
            // 
            this.hexFileLabel.AutoSize = true;
            this.hexFileLabel.Location = new System.Drawing.Point(25, 23);
            this.hexFileLabel.Name = "hexFileLabel";
            this.hexFileLabel.Size = new System.Drawing.Size(45, 13);
            this.hexFileLabel.TabIndex = 2;
            this.hexFileLabel.Text = "Hex File";
            // 
            // UploadBtn
            // 
            this.UploadBtn.Location = new System.Drawing.Point(87, 218);
            this.UploadBtn.Name = "UploadBtn";
            this.UploadBtn.Size = new System.Drawing.Size(112, 42);
            this.UploadBtn.TabIndex = 3;
            this.UploadBtn.Text = "Upload";
            this.UploadBtn.UseVisualStyleBackColor = true;
            this.UploadBtn.Click += new System.EventHandler(this.UploadBtn_Click);
            // 
            // comPortComboBox
            // 
            this.comPortComboBox.FormattingEnabled = true;
            this.comPortComboBox.Location = new System.Drawing.Point(25, 84);
            this.comPortComboBox.Name = "comPortComboBox";
            this.comPortComboBox.Size = new System.Drawing.Size(121, 21);
            this.comPortComboBox.TabIndex = 4;
            this.comPortComboBox.SelectedIndexChanged += new System.EventHandler(this.comPortComboBox_SelectedIndexChanged);
            this.comPortComboBox.Click += new System.EventHandler(this.comPortComboBox_Click);
            // 
            // comPortLabel
            // 
            this.comPortLabel.AutoSize = true;
            this.comPortLabel.Location = new System.Drawing.Point(25, 65);
            this.comPortLabel.Name = "comPortLabel";
            this.comPortLabel.Size = new System.Drawing.Size(53, 13);
            this.comPortLabel.TabIndex = 5;
            this.comPortLabel.Text = "COM Port";
            // 
            // hostZigBeeAddressLbl
            // 
            this.hostZigBeeAddressLbl.AutoSize = true;
            this.hostZigBeeAddressLbl.Location = new System.Drawing.Point(22, 108);
            this.hostZigBeeAddressLbl.Name = "hostZigBeeAddressLbl";
            this.hostZigBeeAddressLbl.Size = new System.Drawing.Size(107, 13);
            this.hostZigBeeAddressLbl.TabIndex = 6;
            this.hostZigBeeAddressLbl.Text = "Host ZigBee Address";
            // 
            // hostZigBeeAddressTextBox
            // 
            this.hostZigBeeAddressTextBox.Location = new System.Drawing.Point(25, 125);
            this.hostZigBeeAddressTextBox.Name = "hostZigBeeAddressTextBox";
            this.hostZigBeeAddressTextBox.Size = new System.Drawing.Size(146, 20);
            this.hostZigBeeAddressTextBox.TabIndex = 7;
            this.hostZigBeeAddressTextBox.WordWrap = false;
            // 
            // targetZigBeeAddressLbl
            // 
            this.targetZigBeeAddressLbl.AutoSize = true;
            this.targetZigBeeAddressLbl.Location = new System.Drawing.Point(22, 148);
            this.targetZigBeeAddressLbl.Name = "targetZigBeeAddressLbl";
            this.targetZigBeeAddressLbl.Size = new System.Drawing.Size(116, 13);
            this.targetZigBeeAddressLbl.TabIndex = 8;
            this.targetZigBeeAddressLbl.Text = "Target ZigBee Address";
            // 
            // targetZigBeeAddressTextBox
            // 
            this.targetZigBeeAddressTextBox.Location = new System.Drawing.Point(25, 165);
            this.targetZigBeeAddressTextBox.Name = "targetZigBeeAddressTextBox";
            this.targetZigBeeAddressTextBox.Size = new System.Drawing.Size(146, 20);
            this.targetZigBeeAddressTextBox.TabIndex = 9;
            this.targetZigBeeAddressTextBox.WordWrap = false;
            // 
            // updateAddressesBtn
            // 
            this.updateAddressesBtn.Location = new System.Drawing.Point(177, 136);
            this.updateAddressesBtn.Name = "updateAddressesBtn";
            this.updateAddressesBtn.Size = new System.Drawing.Size(87, 36);
            this.updateAddressesBtn.TabIndex = 10;
            this.updateAddressesBtn.Text = "Update Addresses";
            this.updateAddressesBtn.UseVisualStyleBackColor = true;
            this.updateAddressesBtn.Click += new System.EventHandler(this.updateAddressesBtn_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 289);
            this.Controls.Add(this.updateAddressesBtn);
            this.Controls.Add(this.targetZigBeeAddressTextBox);
            this.Controls.Add(this.targetZigBeeAddressLbl);
            this.Controls.Add(this.hostZigBeeAddressTextBox);
            this.Controls.Add(this.hostZigBeeAddressLbl);
            this.Controls.Add(this.comPortLabel);
            this.Controls.Add(this.comPortComboBox);
            this.Controls.Add(this.UploadBtn);
            this.Controls.Add(this.hexFileLabel);
            this.Controls.Add(this.browseBtn);
            this.Controls.Add(this.fileNameBox);
            this.Name = "MainUI";
            this.Text = "MainUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox fileNameBox;
        private System.Windows.Forms.Button browseBtn;
        private System.Windows.Forms.OpenFileDialog openHexFileDialog;
        private System.Windows.Forms.Label hexFileLabel;
        private System.Windows.Forms.Button UploadBtn;
        private System.Windows.Forms.Label comPortLabel;
        private System.Windows.Forms.ComboBox comPortComboBox;
        private System.Windows.Forms.Label hostZigBeeAddressLbl;
        private System.Windows.Forms.TextBox hostZigBeeAddressTextBox;
        private System.Windows.Forms.Label targetZigBeeAddressLbl;
        private System.Windows.Forms.TextBox targetZigBeeAddressTextBox;
        private System.Windows.Forms.Button updateAddressesBtn;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}