using HarmonyLib;
using MuseBase.Multiplayer;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace BuffKit.UpdateChecker
{
    [HarmonyPatch]
    public class UpdateChecker : MonoBehaviour
    {
        private static readonly string _serverVersionUrl = "https://drpitlazarus.github.io/goi-mods/buffkit-version";
        private static readonly string _latestReleasePageUrl = "https://github.com/DrPitLazarus/buffkit/releases/latest";
        private static readonly string _chatCommandToOpenDownloadPage = "/buffkit update";
        private static bool _firstMainMenuState = true;
        private static UpdateChecker _instance;

        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Initialize()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            _instance = BuffKit.GameObject.AddComponent<UpdateChecker>();
        }

        private void Start()
        {
            StartCoroutine(CheckForUpdates());
        }

        /// <summary>
        /// Log current version in chat and check for updates. If outdated, log an update available message in chat.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckForUpdates()
        {
            var currentVersion = PluginInfo.PLUGIN_VERSION;
            MuseLog.Info($"Current BuffKit version: {currentVersion}.");
            Util.SendConsoleChatMessage($"BuffKit {currentVersion} loaded.");

            // Using Unity's version since WebClient/HttpWebRequest fails to connect to https URLs.
            var request = UnityWebRequest.Get(_serverVersionUrl);
            yield return request.Send(); // Wait for the request to complete without blocking.
            if (request.isError || request.responseCode != 200)
            {
                MuseLog.Info($"Failed to check for update. Error: {request.error}");
                yield break; // Early return.
            }

            var versionFromServer = request.downloadHandler.text.Trim();
            MuseLog.Info($"Latest BuffKit version: {versionFromServer}.");

            var isOutdated = new Version(currentVersion).CompareTo(new Version(versionFromServer)) < 0; // -1 means the server version is newer.
            if (!isOutdated)
            {
                yield break; // Early return.
            }
            var message = $"BuffKit {versionFromServer} is available. Type \"{_chatCommandToOpenDownloadPage}\" to open download page.";
            Util.SendConsoleChatMessage(message);
        }

        /// <summary>
        /// Prefix patch to prevent sending chat command as a message and open the download page.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(MessageClient), nameof(MessageClient.TrySendMessage))]
        [HarmonyPrefix]
        private static bool HandleChatCommandToOpenDownloadPage(string msg)
        {
            if (msg.ToLower().Trim() != _chatCommandToOpenDownloadPage) return true; // Allow other messages to be sent normally.
            Application.OpenURL(_latestReleasePageUrl);
            return false; // Prevent the message from being sent to the server.
        }
    }
}