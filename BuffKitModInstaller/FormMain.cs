using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BuffKitModInstaller
{
    public partial class FormMain : Form
    {
        static readonly HttpClient _httpClient = new();
        // Version without the 4th number.
        static readonly string _applicationVersion = Regex.Replace(Application.ProductVersion, @"\.\d+$", "");
        static readonly string _gameFileName = "GunsOfIcarusOnline.exe";
        static readonly string _modRelativeFilePath = @"BepInEx\plugins\BuffKit\BuffKit.dll";
        static readonly string _modRelativeDirectory = Path.GetDirectoryName(_modRelativeFilePath);
        // Source: https://github.com/DrPitLazarus/goi-mods/blob/gh-pages/docs/buffkit-mod-installer-versions.json
        static readonly string _installerVersionsUrl = "https://drpitlazarus.github.io/goi-mods/buffkit-mod-installer-versions.json";
        // Source: https://github.com/DrPitLazarus/goi-mods/blob/gh-pages/docs/buffkit-versions.json
        static readonly string _versionsUrl = "https://drpitlazarus.github.io/goi-mods/buffkit-versions.json";
        static readonly string _githubUrl = "https://github.com/DrPitLazarus/buffkit";
        static string _gameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Guns of Icarus Online";
        public static ModVersion[] ModVersions = [];

        public struct ModVersion
        {
            public string version { get; set; }
            public string downloadUrl { get; set; }
            public string releaseUrl { get; set; }
        }

        static string ModFullFilePath => Path.Combine(_gameDirectory, _modRelativeFilePath);
        static string ModFullDirectory => Path.GetDirectoryName(ModFullFilePath);
        static bool GameIsRunning => Process.GetProcessesByName(_gameFileName.Replace(".exe", "")).Length > 0;

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

        public FormMain()
        {
            InitializeComponent();
            Text = $"{Application.ProductName} {_applicationVersion}";
        }

        async void FormMain_Shown(object sender, EventArgs e)
        {
            // Don't focus on any control on startup.
            ActiveControl = null;
            await CheckForInstallerUpdate();
            ValidateGameDirectory();
            await GetModVersions();
            UpdateState();
        }

        /// <summary>
        /// Checks if there is a newer version of the installer. If fails, show a warning message.
        /// </summary>
        async Task CheckForInstallerUpdate()
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(_installerVersionsUrl);
                response.EnsureSuccessStatusCode();
                var jsonString = (await response.Content.ReadAsStringAsync()).ToString().Trim();
                var versions = JsonSerializer.Deserialize<ModVersion[]>(jsonString);
                var latestVersion = versions[0];
                if (!VersionIsOutdated(_applicationVersion, latestVersion.version))
                {
                    return;
                }
                var message = $"Latest Version: {latestVersion.version}\n" +
                    $"Current Version: {_applicationVersion}\n\n" +
                    $"Press OK to open the release page in your web browser and exit.\n" +
                    $"Press Cancel to continue without updating.";
                var result = MessageBox.Show(message, $"{Application.ProductName} Update Available", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.OK)
                {
                    Process.Start(latestVersion.releaseUrl);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                var message = $"Failed to check for installer update. Check the GitHub link for the latest information.\n\n{ex.Message}";
                MessageBox.Show(message, $"{Application.ProductName} Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Ensures the game directory is valid before continuing. If not, prompt to select directory or exit the program.
        /// </summary>
        void ValidateGameDirectory()
        {
            while (!File.Exists(Path.Combine(_gameDirectory, _gameFileName)))
            {
                var message = $"Did not find {_gameFileName} in:\n" +
                $"{_gameDirectory}\n\n" +
                $"Press OK to manually select your game directory.\n" +
                $"Press Cancel to exit.";
                var result = MessageBox.Show(message, $"{Application.ProductName} Game Directory", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
                if (result == DialogResult.OK)
                {
                    using var ofd = new OpenFileDialog()
                    {
                        Filter = $"{_gameFileName}|{_gameFileName}",
                        Title = $"Select {_gameFileName}",
                    };
                    var ofdResult = ofd.ShowDialog();
                    if (ofdResult == DialogResult.OK)
                    {
                        _gameDirectory = Path.GetDirectoryName(ofd.FileName);
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                    ofd.Dispose();
                }
            }
        }

        /// <summary>
        /// Get mod versions json. If fails to fetch, the program exits. Updates UI.
        /// </summary>
        async Task GetModVersions()
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(_versionsUrl);
                response.EnsureSuccessStatusCode();
                var jsonString = (await response.Content.ReadAsStringAsync()).ToString().Trim();
                ModVersions = JsonSerializer.Deserialize<ModVersion[]>(jsonString);
                labelLatestVersion.Text = ModVersions[0].version;
            }
            catch (Exception ex)
            {
                var message = $"Failed to get mod versions. The program will now close.\n\n{ex.Message}";
                MessageBox.Show(message, $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Updates UI state based on game directory file checks.
        /// </summary>
        void UpdateState()
        {
            if (InvokeRequired) { Invoke(UpdateState); return; }
            linkLabelOpenGameDir.Text = _gameDirectory;
            linkLabelOpenGameDir.Links[0].Enabled = Directory.Exists(_gameDirectory);
            linkLabelOpenModDir.Text = _modRelativeDirectory;
            linkLabelOpenModDir.Links[0].Enabled = Directory.Exists(ModFullDirectory);
            buttonUninstallKeepSettings.Enabled = File.Exists(ModFullFilePath);
            buttonUninstallClean.Enabled = Directory.Exists(ModFullDirectory);

            // Version checks.
            // If the mod file does not exist, update UI to say not installed.
            if (!File.Exists(ModFullFilePath))
            {
                labelVersionStatus.Text = "Not installed.";
                labelCurrentVersion.Text = "N/A";
                return;
            }
            var fileInfo = FileVersionInfo.GetVersionInfo(ModFullFilePath);
            labelCurrentVersion.Text = $"{fileInfo.FileMajorPart}.{fileInfo.FileMinorPart}.{fileInfo.FileBuildPart}";
            if (VersionIsOutdated(labelCurrentVersion.Text, labelLatestVersion.Text))
            {
                labelVersionStatus.Text = "Update available!";
            }
            else
            {
                labelVersionStatus.Text = "You're up to date!";
            }
        }

        /// <summary>
        /// Returns true if version1 is less than version2.
        /// </summary>
        /// <param name="version1">Current</param>
        /// <param name="version2">Latest</param>
        static bool VersionIsOutdated(string version1, string version2)
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2) < 0;
        }

        /// <summary>
        /// Thread-safe call to set status label text.
        /// </summary>
        /// <param name="status"></param>
        void SetStatusLabelText(string status)
        {
            if (InvokeRequired) { Invoke(SetStatusLabelText, status); return; }
            labelStatusLog.Text = status;
        }

        /// <summary>
        /// Thread-safe call to set the enabled property on the action buttons. Used to prevent unwanted calls while processing. 
        /// </summary>
        /// <param name="enabled"></param>
        void SetActionControlsEnabled(bool enabled)
        {
            if (InvokeRequired) { Invoke(SetActionControlsEnabled, enabled); return; }
            buttonInstall.Enabled = enabled;
            buttonInstallOlderVersion.Enabled = enabled;
            buttonUninstallKeepSettings.Enabled = enabled;
            buttonUninstallClean.Enabled = enabled;
        }

        #region Actions
        /// <summary>
        /// Download and extract the zip to the game directory. Game must not be running. Updates UI.
        /// </summary>
        public async void InstallFromUrl(string url)
        {
            SetActionControlsEnabled(false);
            if (GameIsRunning)
            {
                SetStatusLabelText("ERROR: Game is running, please close it!");
                SetActionControlsEnabled(true);
                return;
            }
            SetStatusLabelText("Downloading...");
            try
            {
                // Temporary file location to download the zip.
                var tempZipFile = Path.GetTempFileName();
                // Download the zip.
                using var stream = await _httpClient.GetStreamAsync(url);
                using var file = File.OpenWrite(tempZipFile);
                await stream.CopyToAsync(file);
                stream.Dispose();
                file.Dispose();

                SetStatusLabelText("Extracting...");
                // Ensure the mod directory exists.
                Directory.CreateDirectory(_gameDirectory);
                using var openZip = ZipFile.OpenRead(tempZipFile);
                // Extract all files in zip to game directory.
                foreach (var entry in openZip.Entries)
                {
                    var filePath = Path.Combine(_gameDirectory, entry.FullName);
                    // Ensure extract directory exists.
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    // Extract file with overwrite.
                    entry.ExtractToFile(filePath, true);
                }
                openZip.Dispose();
                File.Delete(tempZipFile);
                SetStatusLabelText("SUCCESS: Installed!");
            }
            catch (Exception ex)
            {
                var message = "ERROR: Failed to run Install Mod";
                MessageBox.Show($"{message}\n\n{ex}", $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatusLabelText(message);
            }
            SetActionControlsEnabled(true);
            UpdateState();
        }

        /// <summary>
        /// Delete only the mod file dll. Game must not be running. Updates UI.
        /// </summary>
        void UninstallAndKeepSettings()
        {
            SetActionControlsEnabled(false);
            if (GameIsRunning)
            {
                SetStatusLabelText("ERROR: Game is running, please close it!");
                SetActionControlsEnabled(true);
                return;
            }
            SetStatusLabelText("Deleting mod file only...");
            try
            {
                File.Delete(ModFullFilePath);
                SetStatusLabelText("SUCCESS: Uninstalled and kept settings!");
            }
            catch (Exception ex)
            {
                var message = "ERROR: Failed to run Uninstall and Keep Settings!";
                MessageBox.Show($"{message}\n\n{ex}", $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatusLabelText(message);
            }
            SetActionControlsEnabled(true);
            UpdateState();
        }

        /// <summary>
        /// Delete the ENTIRE mod directory. Confirm with user before calling. Game must not be running. Updates UI.
        /// </summary>
        void UninstallClean()
        {
            SetActionControlsEnabled(false);
            if (GameIsRunning)
            {
                SetStatusLabelText("ERROR: Game is running, please close it!");
                SetActionControlsEnabled(true);
                return;
            }
            SetStatusLabelText("Deleting entire mod directory...");
            try
            {
                Directory.Delete(ModFullDirectory, true);
                SetStatusLabelText("SUCCESS: Uninstalled cleanly!");
            }
            catch (Exception ex)
            {
                var message = "ERROR: Failed to run Clean Uninstall!";
                MessageBox.Show($"{message}\n\n{ex}", $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatusLabelText(message);
            }
            SetActionControlsEnabled(true);
            UpdateState();
        }
        #endregion

        #region EventHandlers
        void linkLabelOpenGameDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(_gameDirectory);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void linkLabelOpenModDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(ModFullDirectory);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void linkLabelGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(_githubUrl);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        async void buttonInstall_Click(object sender, EventArgs e)
        {
            try
            {
                var url = ModVersions[0].downloadUrl;
                await Task.Run(() => InstallFromUrl(url));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Application.ProductName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonInstallOtherVersion_Click(object sender, EventArgs e)
        {
            var formVersionSelect = new FormVersionSelect(this);
            formVersionSelect.ShowDialog();
        }

        async void buttonUninstallKeepSettings_Click(object sender, EventArgs e)
        {
            await Task.Run(UninstallAndKeepSettings);
        }

        async void buttonUninstallClean_Click(object sender, EventArgs e)
        {
            var message = $"This will permanently delete the mod directory and all its contents!!! This includes mod settings!\n" +
                $"ARE YOU SURE???\n\n" +
                $"If you'd like to make a backup, copy the folder to somewhere NOT IN the plugins directory.\n\n" +
                $"{ModFullDirectory}";
            var result = MessageBox.Show(message, "Confirm Clean Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                await Task.Run(UninstallClean);
            }
        }
        #endregion
    }
}
