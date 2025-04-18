namespace BuffKitModInstaller
{
    partial class FormVersionSelect
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVersionSelect));
            this.comboBoxVersions = new System.Windows.Forms.ComboBox();
            this.linkLabelReleasePage = new System.Windows.Forms.LinkLabel();
            this.buttonInstallThisVersion = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxVersions
            // 
            this.comboBoxVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxVersions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVersions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxVersions.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxVersions.FormattingEnabled = true;
            this.comboBoxVersions.Location = new System.Drawing.Point(3, 3);
            this.comboBoxVersions.Name = "comboBoxVersions";
            this.comboBoxVersions.Size = new System.Drawing.Size(149, 33);
            this.comboBoxVersions.TabIndex = 0;
            // 
            // linkLabelReleasePage
            // 
            this.linkLabelReleasePage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(164)))), ((int)(((byte)(24)))));
            this.linkLabelReleasePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelReleasePage.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.linkLabelReleasePage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelReleasePage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(104)))));
            this.linkLabelReleasePage.Location = new System.Drawing.Point(155, 0);
            this.linkLabelReleasePage.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelReleasePage.Name = "linkLabelReleasePage";
            this.linkLabelReleasePage.Size = new System.Drawing.Size(155, 39);
            this.linkLabelReleasePage.TabIndex = 1;
            this.linkLabelReleasePage.TabStop = true;
            this.linkLabelReleasePage.Text = "View Release Page";
            this.linkLabelReleasePage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.linkLabelReleasePage, "View GitHub release page for this version in your web browser");
            this.linkLabelReleasePage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelReleasePage_LinkClicked);
            // 
            // buttonInstallThisVersion
            // 
            this.buttonInstallThisVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInstallThisVersion.AutoSize = true;
            this.buttonInstallThisVersion.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonInstallThisVersion.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.SetColumnSpan(this.buttonInstallThisVersion, 2);
            this.buttonInstallThisVersion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstallThisVersion.FlatAppearance.BorderSize = 2;
            this.buttonInstallThisVersion.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(123)))), ((int)(((byte)(62)))));
            this.buttonInstallThisVersion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstallThisVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInstallThisVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInstallThisVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.buttonInstallThisVersion.Location = new System.Drawing.Point(3, 49);
            this.buttonInstallThisVersion.Name = "buttonInstallThisVersion";
            this.buttonInstallThisVersion.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.buttonInstallThisVersion.Size = new System.Drawing.Size(304, 35);
            this.buttonInstallThisVersion.TabIndex = 2;
            this.buttonInstallThisVersion.Text = "Install This Version";
            this.buttonInstallThisVersion.UseVisualStyleBackColor = false;
            this.buttonInstallThisVersion.Click += new System.EventHandler(this.buttonInstallThisVersion_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.buttonInstallThisVersion, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelReleasePage, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxVersions, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(310, 87);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // FormVersionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(334, 111);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormVersionSelect";
            this.Text = "Select Mod Version to Install";
            this.Load += new System.EventHandler(this.FormVersionSelect_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxVersions;
        private System.Windows.Forms.LinkLabel linkLabelReleasePage;
        private System.Windows.Forms.Button buttonInstallThisVersion;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}