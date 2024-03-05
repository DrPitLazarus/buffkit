using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.ModMatchTimerFix
{
    [HarmonyPatch]
    public class ModMatchTimerFix
    {
        private static bool _enabled = true;
        private static bool _hideMinuteLeadingZero = true;
        private static bool _changeFontColorToWhite = true;
        private static bool _useSerifFont = true;
        private static bool _firstPrepare = true;

        private static readonly Vector2 _originalAnchoredPosition = new(0, -5);
        private static readonly Vector2 _newAnchoredPosition = new(0, -25);
        private static readonly Color _yellowColor = new(0.9103f, 0.9191f, 0.2771f, 1);
        private static readonly Color _whiteColor = new(1, 1, 1, 1);

        private static RectTransform _labelRt;
        private static Text _crewsOrdersText;
        private static Image _crewsOrdersImage;
        private static Font _roboto;
        private static Font _penumbraHalfSerifStdReg;

        private static bool _crewsOrdersActive => _crewsOrdersText.enabled || _crewsOrdersImage.enabled;

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("mod match timer fix", "mod match timer fix", v => _enabled = v, _enabled);
            Settings.Settings.Instance.AddEntry("mod match timer fix", "hide minute leading zero", v => _hideMinuteLeadingZero = v, _hideMinuteLeadingZero);
            Settings.Settings.Instance.AddEntry("mod match timer fix", "change font color to white", v => _changeFontColorToWhite = v, _changeFontColorToWhite);
            Settings.Settings.Instance.AddEntry("mod match timer fix", "use serif font", v => _useSerifFont = v, _useSerifFont);
            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(Mission), "Start")]
        private static void Postfix()
        {
            if (!_enabled) return;
            var labelObjectPath = "/Game UI/Match UI/UI HUD Canvas/UI Compass Display/UI Compass Display/Mod Countdown Label/";
            var labelObject = GameObject.Find(labelObjectPath);

            if (labelObject == null)
            {
                MuseLog.Info("Mod Countdown Label not found!");
                return;
            }

            // Set fields.
            var crewsOrdersObjectPath = "/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Crew Orders Display/Container/";
            _crewsOrdersText = GameObject.Find(crewsOrdersObjectPath + "Orders Text").GetComponent<Text>();
            _crewsOrdersImage = GameObject.Find(crewsOrdersObjectPath + "Crew Order Icon").GetComponent<Image>();

            _labelRt = labelObject.GetComponent<RectTransform>();

            var labelText = labelObject.GetComponent<Text>();
            if (_roboto == null) _roboto = labelText.font;
            _penumbraHalfSerifStdReg = (Font)Resources.FindObjectsOfTypeAll(typeof(Font))
                .ToList()
                .Find(font => font.name == "PenumbraHalfSerifStd Reg");

            MuseLog.Info("Fix applied!");
        }

        [HarmonyPatch(typeof(UIMatchModCountdownDisplay), "Update")]
        [HarmonyPostfix]
        private static void Countdown_Update(UIMatchModCountdownDisplay __instance)
        {
            if (!_enabled) return;

            var msv = MatchStateView.Instance;

            if (msv != null && msv.ModCountdown >= 0f)
            {
                // Move label below captain's orders UI.
                _labelRt.anchoredPosition = _crewsOrdersActive ? _newAnchoredPosition : _originalAnchoredPosition;

                if (_hideMinuteLeadingZero)
                {
                    var timeSpan = TimeSpan.FromSeconds((double)msv.ModCountdown);
                    __instance.label.text = $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
                }

                __instance.label.color = _changeFontColorToWhite ? _whiteColor : _yellowColor;

                __instance.label.font = _useSerifFont ? _penumbraHalfSerifStdReg : _roboto;
            }
        }
    }
}