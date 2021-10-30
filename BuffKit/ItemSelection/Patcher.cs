using System;
using System.Collections.Generic;
using HarmonyLib;

namespace BuffKit.ItemSelection
{
    [HarmonyPatch(typeof(UISelectionWindow), "Show")]
    class UISelectionWindow_Show
    {
        private static bool _useCustom = true;
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate
                {
                    UICustomItemSelectionWindow.Initialize();
                    Settings.Settings.Instance.AddEntry<bool>("item selection", "item selection panel", delegate (bool v)
                    {
                        _useCustom = v;
                        if (v) UICustomItemSelectionWindow.Instance.SetEnabled();
                        else UICustomItemSelectionWindow.Instance.TryCancel();
                    }, _useCustom);
                };
                _firstPrepare = false;
            }
        }
        private static bool Prefix(string subhead, IEnumerable<SelectionElement> choices, Action<SelectionElement> chooseCallback)
        {
            if (!_useCustom) return true;
            UICustomItemSelectionWindow.Instance.ShowItems(subhead, choices, chooseCallback);
            return false;
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "TryHideOverlay")]
    class UIPageFrame_TryHideOverlay
    {
        private static void Postfix(ref bool __result)
        {
            __result = __result || UICustomItemSelectionWindow.Instance.TryCancel();
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "HideAllElements")]
    class UIPageFrame_HideAllElements
    {
        private static void Postfix()
        {
            UICustomItemSelectionWindow.Instance?.TryCancel();
        }
    }
}
