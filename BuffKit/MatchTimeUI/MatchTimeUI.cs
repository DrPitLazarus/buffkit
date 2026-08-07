using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.MatchTimeUI;

[HarmonyPatch]
internal class MatchTimeUI : MonoBehaviour
{
    private static bool _firstMainMenuState = true;
    private static bool _enabled = true;
    private static GameObject _mainObject;
    private static TextMeshProUGUI _textMeshProUGUI;
    private static Coroutine _coroutine;
    private static RectTransform _crewOrdersContainerRect;
    private static readonly int _fontSize = 16;
    private static readonly Vector2 _crewOrdersOldAnchoredPosition = new(0, 0);
    private static readonly Vector2 _crewOrdersNewAnchoredPosition = new(0, -20);
    private static readonly string _textFormatBoth = "<mspace=2em>{0:00}</mspace>:<mspace=2em>{1:00}</mspace>   <color=yellow><mspace=2em>{2:00}</mspace>:<mspace=2em>{3:00}</mspace></color>";
    private static readonly string _textFormatTime = "<mspace=2em>{0:00}</mspace>:<mspace=2em>{1:00}</mspace>";
    private static readonly string _textFormatModTimer = "<color=yellow><mspace=2em>{0:00}</mspace>:<mspace=2em>{1:00}</mspace></color>";
    private static readonly WaitForSeconds _waitForSeconds = new(0.1f);

    private static TimeSpan _matchElapsedTime => MatchLobbyView.Instance?.ElapsedTime ?? TimeSpan.Zero;
    private static float _matchModTimerSeconds => MatchStateView.Instance?.ModCountdown ?? -1;
    private static bool _matchModTimerActive => _matchModTimerSeconds >= 0f;
    private static bool _enabledOrModTimerActive => _enabled || _matchModTimerActive;

    /// <summary>
    /// Feature initialization. Create settings.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        if (!_firstMainMenuState) return;
        _firstMainMenuState = false;
        Settings.Settings.Instance.AddEntry("match time ui", "match time ui display", v => _enabled = v, _enabled);

        _crewOrdersContainerRect ??= GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Crew Orders Display/Container")?.GetComponent<RectTransform>();

        GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI Compass Display/UI Compass Display")?.GetComponent<UIMatchModCountdownDisplay>()?.enabled = false;

        if (_mainObject == null)
        {
            MuseLog.Info("GameObject created!");
            _mainObject = CreateUi();
            _mainObject.SetActive(false);
        }
    }

    private static GameObject CreateUi()
    {
        var parentObject = GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI Compass Display/UI Compass Display");
        var mainObject = new GameObject("MatchTimeUI");
        mainObject.transform.SetParent(parentObject.transform, false);
        var rectTransform = mainObject.AddComponent<RectTransform>();
        var vector2Center = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = vector2Center;
        rectTransform.anchorMin = vector2Center;
        rectTransform.pivot = new Vector2(0.5f, 1.35f);
        var textMesh = mainObject.AddComponent<TextMeshProUGUI>();
        textMesh.font = Resources.FontPenumbraHalfSerifStd;
        textMesh.text = "<space=2em>00</mspace>:<mspace=2em>00</mspace>";
        textMesh.fontSize = _fontSize;
        _textMeshProUGUI = textMesh;
        mainObject.AddComponent<MatchTimeUI>();
        return mainObject;
    }

    /// <summary>
    /// Updates and sets the text for the UI.
    /// </summary>
    private static void UpdateUI()
    {
        var newText = "";
        if (_enabled && _matchModTimerActive)
        {
            var modTimer = TimeSpan.FromSeconds(_matchModTimerSeconds);
            newText = _textFormatBoth.F([_matchElapsedTime.Minutes, _matchElapsedTime.Seconds, modTimer.Minutes, modTimer.Seconds]);
        }
        else if (_enabled)
        {
            newText = _textFormatTime.F([_matchElapsedTime.Minutes, _matchElapsedTime.Seconds]);
        }
        else if (_matchModTimerActive)
        {
            var modTimer = TimeSpan.FromSeconds(_matchModTimerSeconds);
            newText = _textFormatModTimer.F([modTimer.Minutes, modTimer.Seconds]); ;
        }
        if (_enabled && _matchElapsedTime.Hours > 0)
        {
            newText = $"<mspace=2em>{_matchElapsedTime.Hours}</mspace>:{newText}";
        }
        _textMeshProUGUI?.text = newText;
        _crewOrdersContainerRect?.anchoredPosition = _enabledOrModTimerActive ? _crewOrdersNewAnchoredPosition : _crewOrdersOldAnchoredPosition;
    }

    private void OnEnable()
    {
        // Set alignment here since it doesn't seem to stick when created/on mission start.
        _textMeshProUGUI?.alignment = TextAlignmentOptions.Top;
        _coroutine = StartCoroutine(UpdateUI_Coroutine());
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private IEnumerator UpdateUI_Coroutine()
    {
        while (true)
        {
            UpdateUI();
            yield return _waitForSeconds;
        }
    }

    /// <summary>
    /// Activate feature.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UIMatchBlockState), nameof(UIManager.UIMatchBlockState.Exit))] // Normal match start.
    [HarmonyPatch(typeof(UIManager.UILoadingBlockState), nameof(UIManager.UILoadingBlockState.Exit))] // Join running match.
    [HarmonyPostfix]
    private static void UIManager_UIMatchBlockState_Exit()
    {
        if (!_enabled) return;
        MuseLog.Info($"Activating...");
        if (_mainObject == null)
        {
            MuseLog.Info("GameObject created!");
            _mainObject = CreateUi();
        }
        _textMeshProUGUI?.text = "";
        _mainObject?.SetActive(true);
        UpdateUI();
    }

    /// <summary>
    /// Deactivate feature.
    /// </summary>
    [HarmonyPatch(typeof(Mission), nameof(Mission.OnDisable))]
    [HarmonyPostfix]
    private static void Mission_OnDisable()
    {
        _mainObject?.SetActive(false);
    }
}