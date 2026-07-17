using HarmonyLib;
using LitJson;
using MuseBase.Multiplayer;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace BuffKit.UpdateChecker
{
    [HarmonyPatch]
    public class UpdateChecker : MonoBehaviour
    {
        private static readonly string _chatCommandToOpenDownloadPage = "/buffkit update";
        private static readonly string _gitHubReleasesUrl = "https://api.github.com/repos/drpitlazarus/buffkit/releases";
        private static readonly string _gitHubReleasesTag = "SCS";
        private static readonly string _chatCommandToOpenGitHubPage = "/buffkit github";
        private static readonly string _gitHubUrl = "https://github.com/DrPitLazarus/buffkit";
        private static bool _firstMainMenuState = true;
        private static UpdateChecker _instance;
        private static string _latestReleasePageUrl;

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
        private IEnumerator CheckForUpdates()
        {
            var currentVersion = PluginInfo.PLUGIN_VERSION;
            MuseLog.Info($"Current BuffKit version: {currentVersion}.");
            Util.SendConsoleChatMessage($"BuffKit {currentVersion} loaded.");

            var request = UnityWebRequest.Get(_gitHubReleasesUrl);
            yield return request.Send(); // Wait for the request to complete without blocking.
            if (request.isError || request.responseCode != 200)
            {
                MuseLog.Info($"Failed to check for update. Error: {request.error}");
                yield break; // Early return.
            }

            try
            {
                var releasesJson = JsonMapper.ToObject(request.downloadHandler.text);
                var foundLatestRelease = false;
                var serverVersionString = "";
                foreach (JsonData release in releasesJson)
                {
                    if (release["tag_name"].ToString().StartsWith(_gitHubReleasesTag))
                    {
                        foundLatestRelease = true;
                        serverVersionString = release["tag_name"].ToString().Replace(_gitHubReleasesTag, "");
                        var parts = serverVersionString.Split('.').Length;
                        if (parts == 1)
                            serverVersionString += ".0.0";
                        else if (parts == 2)
                            serverVersionString += ".0";
                        MuseLog.Info($"Latest {PluginInfo.PLUGIN_NAME} version: {serverVersionString}.");
                        _latestReleasePageUrl = release["html_url"].ToString();
                        break;
                    }
                }

                if (!foundLatestRelease)
                {
                    throw new Exception($"Did not find a release with the expected tag format ({_gitHubReleasesTag}<version>).");
                }

                var isOutdated = new Version(currentVersion).CompareTo(new Version(serverVersionString)) < 0; // -1 means the server version is newer.
                if (!isOutdated)
                {
                    MuseLog.Info("No update available.");
                    yield break; // Early return.
                }
                var message = $"{PluginInfo.PLUGIN_NAME} {serverVersionString} is available. Type \"{_chatCommandToOpenDownloadPage}\" to open download page.";
                Util.SendConsoleChatMessage(message);
            }
            catch (Exception ex)
            {
                MuseLog.Info($"Failed to check for update. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Prefix patch to prevent sending chat command as a message and run actions.
        /// </summary>
        [HarmonyPatch(typeof(MessageClient), nameof(MessageClient.TrySendMessage))]
        [HarmonyPrefix]
        private static bool HandleChatCommandToOpenDownloadPage(string msg)
        {
            var preparedMsg = msg.ToLower().Trim();
            if (preparedMsg == _chatCommandToOpenDownloadPage)
            {
                Application.OpenURL(_latestReleasePageUrl);
                return false; // Prevent the message from being sent to the server.
            }
            if (preparedMsg == _chatCommandToOpenGitHubPage)
            {
                Application.OpenURL(_gitHubUrl);
                return false;
            }
            return true; // Allow other messages to be sent normally.
        }
    }
}