using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Muse.Networking;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;

namespace BuffKit.TitleSelection
{
    [HarmonyPatch(typeof(UIProfilePanel), "EditTitle")]
    class UIProfilePanel_EditTitle
    {
        private static BepInEx.Logging.ManualLogSource log;
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                log = BepInEx.Logging.Logger.CreateLogSource("title-selection");
                _firstPrepare = false;

                Util.Util.OnGameInitialize += delegate
                {
                    UICustomTitleSelection.Initialize();
                };
            }
        }
        private static bool Prefix(UIProfilePanel __instance, UserProfile ___currentUser, UnityEngine.UI.Text ___titleLabel)
        {
            log.LogInfo("In custom title selection");
            if (___currentUser == null || ___currentUser.Id != NetworkedPlayer.Local.UserId)
            {
                return false;
            }

            CharCustomActions.GetTitles(delegate (List<PlayerTitle> titles)
            {
                titles.Insert(0, new PlayerTitle
                {
                    Id = 0,
                    TitleText = new Muse.Goi2.Entity.Text
                    {
                        En = NetworkedPlayer.Local.GetLevelTitle("En")
                    }
                });

                UICustomTitleSelection.Instance.DisplayMenu(titles, delegate (PlayerTitle newTitle)
                {
                    if (newTitle != null)
                    {
                        CharCustomActions.ChangeTitle(newTitle.Id, delegate (ExtensionResponse resp)
                        {
                            if (resp.Success)
                            {
                                ___currentUser.TitleId = newTitle.Id;
                                ___currentUser.Title = newTitle.TitleText.En;
                                ___titleLabel.text = ___currentUser.Title;
                            }
                        });
                    }
                });
            });

            return false;
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "TryHideOverlay")]
    class UIPageFrame_TryHideOverlay
    {
        private static void Postfix(ref bool __result)
        {
            __result = __result || UICustomTitleSelection.Instance.TryHide();
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "HideAllElements")]
    class UIPageFrame_HideAllElements
    {
        private static void Postfix()
        {
            UICustomTitleSelection.Instance?.TryHide();
        }
    }
}
