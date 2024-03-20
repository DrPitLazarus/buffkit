using HarmonyLib;
using MuseBase.Multiplayer;
using MuseBase.Multiplayer.Unity;
using UnityEngine.Networking;
using Version = System.Version;

namespace BuffKit.UpdateChecker
{
    [HarmonyPatch]
    public class UpdateChecker
    {
        private static bool _firstMainMenuState = true;

        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), "Enter")]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            CheckForUpdates();
        }

        private static void CheckForUpdates()
        {
            var currentVersion = PluginInfo.PLUGIN_VERSION;
            MuseLog.Info("Current BuffKit version: " + currentVersion);
            MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console($"BuffKit {currentVersion} loaded."));

            // Using unity's version, webclient/httpwebrequest fails to connect to https urls.
            var request = UnityWebRequest.Get("https://drpitlazarus.github.io/goi-mods/buffkit-version");
            request.Send();
            while (!request.isDone) { } // Wait for request to finish. Didn't want to deal with coroutines.
            if (request.isError || request.responseCode != 200)
            {
                MuseLog.Info("Failed to check for update.");
                return;
            }

            var versionFromServer = request.downloadHandler.text.Trim();
            MuseLog.Info("Latest BuffKit version: " + versionFromServer);

            if (!(new Version(currentVersion).CompareTo(new Version(versionFromServer)) < 0)) return; // -1 means the server version is newer.
            var message = $"BuffKit {versionFromServer} is available. Check the #buffkit channel in the COG discord server to download.";
            MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console(message));
        }
    }
}