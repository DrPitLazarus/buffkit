using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BuffKitModInstaller
{
    public partial class FormVersionSelect : Form
    {
        readonly FormMain _formReference;
        FormMain.ModVersion SelectedModVersionObject => FormMain.ModVersions[comboBoxVersions.SelectedIndex];

        public FormVersionSelect(FormMain formReference)
        {
            InitializeComponent();
            _formReference = formReference;
        }

        #region DarkTitleBar
        // Dark title bar: https://stackoverflow.com/a/64927217
        [DllImport("DwmApi")] // System.Runtime.InteropServices
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, [1], 4) != 0)
                DwmSetWindowAttribute(Handle, 20, [1], 4);
        }
        #endregion

        void FormVersionSelect_Load(object sender, EventArgs e)
        {
            comboBoxVersions.BeginUpdate();
            foreach (var modVersion in FormMain.ModVersions)
            {
                comboBoxVersions.Items.Add(modVersion.version);
            }
            comboBoxVersions.EndUpdate();
            comboBoxVersions.SelectedIndex = 0;
        }

        void linkLabelReleasePage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(SelectedModVersionObject.releaseUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void buttonInstallThisVersion_Click(object sender, EventArgs e)
        {
            var downloadUrl = SelectedModVersionObject.downloadUrl;
            Task.Run(() => _formReference.InstallFromUrl(downloadUrl));
            Close();
        }
    }
}
