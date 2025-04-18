namespace BuffKitModInstaller
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.labelLatestVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelCurrentVersion = new System.Windows.Forms.Label();
            this.labelVersionStatus = new System.Windows.Forms.Label();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.buttonUninstallKeepSettings = new System.Windows.Forms.Button();
            this.buttonUninstallClean = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.linkLabelGitHub = new System.Windows.Forms.LinkLabel();
            this.label12 = new System.Windows.Forms.Label();
            this.linkLabelOpenGameDir = new System.Windows.Forms.LinkLabel();
            this.linkLabelOpenModDir = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonInstallOlderVersion = new System.Windows.Forms.Button();
            this.labelStatusLog = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.label1.Location = new System.Drawing.Point(0, 60);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Latest Version:";
            // 
            // labelLatestVersion
            // 
            this.labelLatestVersion.BackColor = System.Drawing.Color.Transparent;
            this.labelLatestVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLatestVersion.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.labelLatestVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.labelLatestVersion.Location = new System.Drawing.Point(166, 60);
            this.labelLatestVersion.Margin = new System.Windows.Forms.Padding(0);
            this.labelLatestVersion.Name = "labelLatestVersion";
            this.labelLatestVersion.Size = new System.Drawing.Size(166, 30);
            this.labelLatestVersion.TabIndex = 1;
            this.labelLatestVersion.Text = "0000.0.0";
            this.labelLatestVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 30);
            this.label3.TabIndex = 5;
            this.label3.Text = "Game Directory:";
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.label5.Location = new System.Drawing.Point(0, 90);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 30);
            this.label5.TabIndex = 4;
            this.label5.Text = "Current Version:";
            // 
            // labelCurrentVersion
            // 
            this.labelCurrentVersion.BackColor = System.Drawing.Color.Transparent;
            this.labelCurrentVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCurrentVersion.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.labelCurrentVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.labelCurrentVersion.Location = new System.Drawing.Point(166, 90);
            this.labelCurrentVersion.Margin = new System.Windows.Forms.Padding(0);
            this.labelCurrentVersion.Name = "labelCurrentVersion";
            this.labelCurrentVersion.Size = new System.Drawing.Size(166, 30);
            this.labelCurrentVersion.TabIndex = 5;
            this.labelCurrentVersion.Text = "0000.0.0";
            this.labelCurrentVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelVersionStatus
            // 
            this.labelVersionStatus.BackColor = System.Drawing.Color.Transparent;
            this.labelVersionStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelVersionStatus.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.labelVersionStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.labelVersionStatus.Location = new System.Drawing.Point(332, 90);
            this.labelVersionStatus.Margin = new System.Windows.Forms.Padding(0);
            this.labelVersionStatus.Name = "labelVersionStatus";
            this.labelVersionStatus.Size = new System.Drawing.Size(334, 30);
            this.labelVersionStatus.TabIndex = 6;
            this.labelVersionStatus.Text = "N/A";
            this.labelVersionStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonInstall
            // 
            this.buttonInstall.AutoSize = true;
            this.buttonInstall.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonInstall.BackColor = System.Drawing.Color.Transparent;
            this.buttonInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonInstall.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstall.FlatAppearance.BorderSize = 2;
            this.buttonInstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(123)))), ((int)(((byte)(62)))));
            this.buttonInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInstall.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInstall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.buttonInstall.Location = new System.Drawing.Point(3, 39);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.buttonInstall.Size = new System.Drawing.Size(327, 35);
            this.buttonInstall.TabIndex = 0;
            this.buttonInstall.Text = "Install Latest Version";
            this.buttonInstall.UseVisualStyleBackColor = false;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // buttonUninstallKeepSettings
            // 
            this.buttonUninstallKeepSettings.AutoSize = true;
            this.buttonUninstallKeepSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonUninstallKeepSettings.BackColor = System.Drawing.Color.Transparent;
            this.buttonUninstallKeepSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonUninstallKeepSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonUninstallKeepSettings.FlatAppearance.BorderSize = 2;
            this.buttonUninstallKeepSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(123)))), ((int)(((byte)(62)))));
            this.buttonUninstallKeepSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonUninstallKeepSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUninstallKeepSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUninstallKeepSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.buttonUninstallKeepSettings.Location = new System.Drawing.Point(336, 39);
            this.buttonUninstallKeepSettings.Name = "buttonUninstallKeepSettings";
            this.buttonUninstallKeepSettings.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.buttonUninstallKeepSettings.Size = new System.Drawing.Size(327, 35);
            this.buttonUninstallKeepSettings.TabIndex = 1;
            this.buttonUninstallKeepSettings.Text = "Uninstall and Keep Settings";
            this.buttonUninstallKeepSettings.UseVisualStyleBackColor = false;
            this.buttonUninstallKeepSettings.Click += new System.EventHandler(this.buttonUninstallKeepSettings_Click);
            // 
            // buttonUninstallClean
            // 
            this.buttonUninstallClean.AutoSize = true;
            this.buttonUninstallClean.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonUninstallClean.BackColor = System.Drawing.Color.Transparent;
            this.buttonUninstallClean.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonUninstallClean.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonUninstallClean.FlatAppearance.BorderSize = 2;
            this.buttonUninstallClean.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(123)))), ((int)(((byte)(62)))));
            this.buttonUninstallClean.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonUninstallClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUninstallClean.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUninstallClean.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.buttonUninstallClean.Location = new System.Drawing.Point(336, 80);
            this.buttonUninstallClean.Name = "buttonUninstallClean";
            this.buttonUninstallClean.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.buttonUninstallClean.Size = new System.Drawing.Size(327, 35);
            this.buttonUninstallClean.TabIndex = 3;
            this.buttonUninstallClean.Text = "Clean Uninstall...";
            this.buttonUninstallClean.UseVisualStyleBackColor = false;
            this.buttonUninstallClean.Click += new System.EventHandler(this.buttonUninstallClean_Click);
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.label10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.label10.Location = new System.Drawing.Point(0, 30);
            this.label10.Margin = new System.Windows.Forms.Padding(0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(166, 30);
            this.label10.TabIndex = 12;
            this.label10.Text = "Mod Directory:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.SetColumnSpan(this.label14, 2);
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.label14.Location = new System.Drawing.Point(0, 15);
            this.label14.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(666, 21);
            this.label14.TabIndex = 18;
            this.label14.Text = "Choose an action, there is no confirm prompt:";
            // 
            // linkLabelGitHub
            // 
            this.linkLabelGitHub.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(164)))), ((int)(((byte)(24)))));
            this.linkLabelGitHub.AutoSize = true;
            this.linkLabelGitHub.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.linkLabelGitHub.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelGitHub.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(104)))));
            this.linkLabelGitHub.Location = new System.Drawing.Point(339, 17);
            this.linkLabelGitHub.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelGitHub.Name = "linkLabelGitHub";
            this.linkLabelGitHub.Size = new System.Drawing.Size(48, 17);
            this.linkLabelGitHub.TabIndex = 0;
            this.linkLabelGitHub.TabStop = true;
            this.linkLabelGitHub.Text = "GitHub";
            this.toolTip1.SetToolTip(this.linkLabelGitHub, "View source code on GitHub in your web browser");
            this.linkLabelGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGitHub_LinkClicked);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.Transparent;
            this.label12.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.label12.Location = new System.Drawing.Point(0, 0);
            this.label12.Margin = new System.Windows.Forms.Padding(0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(339, 34);
            this.label12.TabIndex = 0;
            this.label12.Text = "BuffKit Mod by Trgk, Ightrril, and Dr. Pit Lazarus\r\nBuffKit Mod Installer for Gun" +
    "s of Icarus by Dr. Pit Lazarus";
            // 
            // linkLabelOpenGameDir
            // 
            this.linkLabelOpenGameDir.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(164)))), ((int)(((byte)(24)))));
            this.linkLabelOpenGameDir.AutoEllipsis = true;
            this.tableLayoutPanel2.SetColumnSpan(this.linkLabelOpenGameDir, 2);
            this.linkLabelOpenGameDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelOpenGameDir.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.linkLabelOpenGameDir.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelOpenGameDir.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(104)))));
            this.linkLabelOpenGameDir.Location = new System.Drawing.Point(166, 0);
            this.linkLabelOpenGameDir.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelOpenGameDir.Name = "linkLabelOpenGameDir";
            this.linkLabelOpenGameDir.Size = new System.Drawing.Size(500, 30);
            this.linkLabelOpenGameDir.TabIndex = 0;
            this.linkLabelOpenGameDir.TabStop = true;
            this.linkLabelOpenGameDir.Text = "C:\\";
            this.linkLabelOpenGameDir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabelOpenGameDir, "Browse directory in File Explorer");
            this.linkLabelOpenGameDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenGameDir_LinkClicked);
            // 
            // linkLabelOpenModDir
            // 
            this.linkLabelOpenModDir.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(164)))), ((int)(((byte)(24)))));
            this.tableLayoutPanel2.SetColumnSpan(this.linkLabelOpenModDir, 2);
            this.linkLabelOpenModDir.DisabledLinkColor = System.Drawing.Color.Maroon;
            this.linkLabelOpenModDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelOpenModDir.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.linkLabelOpenModDir.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelOpenModDir.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(104)))));
            this.linkLabelOpenModDir.Location = new System.Drawing.Point(166, 30);
            this.linkLabelOpenModDir.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelOpenModDir.Name = "linkLabelOpenModDir";
            this.linkLabelOpenModDir.Size = new System.Drawing.Size(500, 30);
            this.linkLabelOpenModDir.TabIndex = 1;
            this.linkLabelOpenModDir.TabStop = true;
            this.linkLabelOpenModDir.Text = "BepInEx\\plugins\\BuffKit";
            this.linkLabelOpenModDir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabelOpenModDir, "Browse directory in File Explorer");
            this.linkLabelOpenModDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenModDir_LinkClicked);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonInstallOlderVersion, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonUninstallClean, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonInstall, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonUninstallKeepSettings, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 120);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(666, 152);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.label8.Location = new System.Drawing.Point(333, 118);
            this.label8.Margin = new System.Windows.Forms.Padding(0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(333, 34);
            this.label8.TabIndex = 10;
            this.label8.Text = "Uninstall will only remove the mod. It does not remove the mod loader BepInEx.";
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Transparent;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.label11.Location = new System.Drawing.Point(0, 118);
            this.label11.Margin = new System.Windows.Forms.Padding(0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(333, 34);
            this.label11.TabIndex = 14;
            this.label11.Text = "Install doesn\'t reset mod settings. If downgrading, make a backup of the mod dire" +
    "ctory not in the plugins directory.";
            // 
            // buttonInstallOlderVersion
            // 
            this.buttonInstallOlderVersion.AutoSize = true;
            this.buttonInstallOlderVersion.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonInstallOlderVersion.BackColor = System.Drawing.Color.Transparent;
            this.buttonInstallOlderVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonInstallOlderVersion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstallOlderVersion.FlatAppearance.BorderSize = 2;
            this.buttonInstallOlderVersion.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(123)))), ((int)(((byte)(62)))));
            this.buttonInstallOlderVersion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.buttonInstallOlderVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInstallOlderVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInstallOlderVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(194)))), ((int)(((byte)(145)))));
            this.buttonInstallOlderVersion.Location = new System.Drawing.Point(3, 80);
            this.buttonInstallOlderVersion.Name = "buttonInstallOlderVersion";
            this.buttonInstallOlderVersion.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.buttonInstallOlderVersion.Size = new System.Drawing.Size(327, 35);
            this.buttonInstallOlderVersion.TabIndex = 2;
            this.buttonInstallOlderVersion.Text = "Install Other Version...";
            this.buttonInstallOlderVersion.UseVisualStyleBackColor = false;
            this.buttonInstallOlderVersion.Click += new System.EventHandler(this.buttonInstallOtherVersion_Click);
            // 
            // labelStatusLog
            // 
            this.labelStatusLog.BackColor = System.Drawing.Color.Transparent;
            this.labelStatusLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatusLog.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.labelStatusLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(153)))), ((int)(((byte)(79)))));
            this.labelStatusLog.Location = new System.Drawing.Point(0, 272);
            this.labelStatusLog.Margin = new System.Windows.Forms.Padding(0);
            this.labelStatusLog.Name = "labelStatusLog";
            this.labelStatusLog.Padding = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.labelStatusLog.Size = new System.Drawing.Size(666, 60);
            this.labelStatusLog.TabIndex = 22;
            this.labelStatusLog.Text = "STATUS: Ready";
            this.labelStatusLog.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.linkLabelOpenGameDir, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.linkLabelOpenModDir, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelLatestVersion, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.labelCurrentVersion, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.labelVersionStatus, 2, 3);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(666, 120);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel2);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Controls.Add(this.labelStatusLog);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(666, 349);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.linkLabelGitHub);
            this.panel1.Location = new System.Drawing.Point(9, 358);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(666, 34);
            this.panel1.TabIndex = 3;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(684, 401);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BuffKit Mod Installer";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelLatestVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelCurrentVersion;
        private System.Windows.Forms.Label labelVersionStatus;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.Button buttonUninstallKeepSettings;
        private System.Windows.Forms.Button buttonUninstallClean;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.LinkLabel linkLabelGitHub;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.LinkLabel linkLabelOpenGameDir;
        private System.Windows.Forms.LinkLabel linkLabelOpenModDir;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonInstallOlderVersion;
        private System.Windows.Forms.Label labelStatusLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
    }
}

