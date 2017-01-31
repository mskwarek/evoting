namespace Proxy
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.configButton = new System.Windows.Forms.Button();
            this.startProxyButton = new System.Windows.Forms.Button();
            this.connectElectionAuthorityButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // configButton
            // 
            this.configButton.Location = new System.Drawing.Point(19, 12);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(107, 35);
            this.configButton.TabIndex = 1;
            this.configButton.Text = "Load Configuration";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // startProxyButton
            // 
            this.startProxyButton.Enabled = false;
            this.startProxyButton.Location = new System.Drawing.Point(18, 53);
            this.startProxyButton.Name = "startProxyButton";
            this.startProxyButton.Size = new System.Drawing.Size(108, 35);
            this.startProxyButton.TabIndex = 2;
            this.startProxyButton.Text = "Start Proxy";
            this.startProxyButton.UseVisualStyleBackColor = true;
            this.startProxyButton.Click += new System.EventHandler(this.startProxyButton_Click);
            // 
            // connectElectionAuthorityButton
            // 
            this.connectElectionAuthorityButton.Enabled = false;
            this.connectElectionAuthorityButton.Location = new System.Drawing.Point(19, 95);
            this.connectElectionAuthorityButton.Name = "connectElectionAuthorityButton";
            this.connectElectionAuthorityButton.Size = new System.Drawing.Size(107, 35);
            this.connectElectionAuthorityButton.TabIndex = 3;
            this.connectElectionAuthorityButton.Text = "Connect to Election Authority";
            this.connectElectionAuthorityButton.UseVisualStyleBackColor = true;
            this.connectElectionAuthorityButton.Click += new System.EventHandler(this.connectElectionAuthorityButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(144, 144);
            this.Controls.Add(this.connectElectionAuthorityButton);
            this.Controls.Add(this.startProxyButton);
            this.Controls.Add(this.configButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Proxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button configButton;
        private System.Windows.Forms.Button startProxyButton;
        private System.Windows.Forms.Button connectElectionAuthorityButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}

