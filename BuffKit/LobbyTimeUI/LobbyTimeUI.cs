using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.LobbyTimeUI;

[HarmonyPatch]
internal class LobbyTimeUI : MonoBehaviour
{
    private static bool _firstMainMenuState = true;
    private static bool _enabled = true;
    private static GameObject _mainObject;
    private static TextMeshProUGUI _textMeshProUGUI;
    private static Coroutine _coroutine;
    private static DateTime _joinedLobbyTime;
    private static readonly int _fontSize = 13;
    private static readonly string _textFormatTime = "<mspace=2em>{0:00}</mspace>:<mspace=2em>{1:00}</mspace>";
    private static readonly WaitForSeconds _waitForSeconds = new(0.1f);

    /// <summary>
    /// Feature initialization. Create settings.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        if (!_firstMainMenuState) return;
        _firstMainMenuState = false;
        Settings.Settings.Instance.AddEntry("lobby time ui", "lobby time ui display", v =>
        {
            _enabled = v;
            if (UIMatchLobby.Instance.Activated)
                _mainObject?.SetActive(v);
        },
        _enabled);

        if (_mainObject == null)
        {
            MuseLog.Info("GameObject created!");
            _mainObject = CreateUi();
            _mainObject.SetActive(false);
        }
    }

    private static GameObject CreateUi()
    {
        var parentObject = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Lobby/Lobby Main Panel/Team Group/Subnav Button Group");
        var mainObject = new GameObject("LobbyTimeUI");
        mainObject.transform.SetParent(parentObject.transform, false);
        var layoutElement = mainObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 50f;
        var textMesh = mainObject.AddComponent<TextMeshProUGUI>();
        textMesh.font = Resources.FontPenumbraHalfSerifStd;
        textMesh.text = "<space=2em>00</mspace>:<mspace=2em>00</mspace>";
        textMesh.fontSize = _fontSize;
        _textMeshProUGUI = textMesh;
        mainObject.transform.SetSiblingIndex(5); // 5 is after Ship Loadout button.
        mainObject.AddComponent<LobbyTimeUI>();
        return mainObject;
    }

    /// <summary>
    /// Updates and sets the text for the UI.
    /// </summary>
    private static void UpdateUI()
    {
        var elapsedTime = DateTime.Now - _joinedLobbyTime;
        var newText = _textFormatTime.F([elapsedTime.Minutes, elapsedTime.Seconds]);

        if (elapsedTime.Hours > 0)
        {
            newText = $"<mspace=2em>{elapsedTime.Hours}</mspace>:{newText}";
        }
        _textMeshProUGUI?.text = newText;
    }

    private void OnEnable()
    {
        // Set alignment here since it doesn't seem to stick when created.
        _textMeshProUGUI?.alignment = TextAlignmentOptions.Capline;
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
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), nameof(UIManager.UINewMatchLobbyState.Enter))]
    [HarmonyPostfix]
    private static void UIManager_UINewMatchLobbyState_Enter()
    {
        if (!_enabled) return;
        _mainObject?.SetActive(true);
    }

    /// <summary>
    /// Deactivate feature.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), nameof(UIManager.UINewMatchLobbyState.Exit))]
    [HarmonyPostfix]
    private static void UIManager_UINewMatchLobbyState_Exit()
    {
        _mainObject?.SetActive(false);
    }

    /// <summary>
    /// Set lobby join time.
    /// </summary>
    [HarmonyPatch(typeof(MatchLobbyView), nameof(MatchLobbyView.Start))]
    [HarmonyPostfix]
    private static void MatchLobbyView_Start()
    {
        _joinedLobbyTime = DateTime.Now;
    }

    /// <summary>
    /// Reset lobby join time.
    /// </summary>
    [HarmonyPatch(typeof(MatchLobbyView), nameof(MatchLobbyView.OnDisable))]
    [HarmonyPostfix]
    private static void MatchLobbyView_OnDisable()
    {
        _joinedLobbyTime = DateTime.MinValue;
    }
}