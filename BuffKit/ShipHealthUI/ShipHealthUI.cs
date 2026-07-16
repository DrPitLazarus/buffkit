using BuffKit.Settings;
using HarmonyLib;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.ShipHealthUI;

[HarmonyPatch]
internal class ShipHealthUI : MonoBehaviour
{
    private static bool _firstMainMenuState = true;
    private static readonly int _fontSize = 12;
    private static GameObject _mainObject;
    private static TextMeshProUGUI _textMeshProUGUI;
    private static ShipHealthDisplayOption _shipHealthDisplayOption = ShipHealthDisplayOption.Disabled;
    private static readonly string[] _displayOptionTextFormat =
    [
        "",
        "<mspace=2em>{0:0}/{1:0}</mspace> (<mspace=2em>{2:0}</mspace>%)",
        "<mspace=2em>{0:0}/{1:0}",
        "<mspace=2em>{2:0}</mspace>%"
    ];
    private static string _selectedTextFormat => _displayOptionTextFormat[(int)_shipHealthDisplayOption];
    private static Ship _currentShip => NetworkedPlayer.Local?.CurrentShip;
    private static Hull _currentShipHull => NetworkedPlayer.Local?.CurrentShip?.ActiveHull;

    private enum ShipHealthDisplayOption
    {
        Disabled,
        [Description("Number and Percent")]
        NumberAndPercent,
        Number,
        Percent,
    }

    /// <summary>
    /// Feature initialization. Create settings.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        if (!_firstMainMenuState) return;
        _firstMainMenuState = false;
        Settings.Settings.Instance.AddEntry("ship health ui", "ship health ui display",
            v => { _shipHealthDisplayOption = (ShipHealthDisplayOption)v.SelectedValue; },
            new EnumString(typeof(ShipHealthDisplayOption), (int)_shipHealthDisplayOption));

        if (_mainObject == null)
        {
            MuseLog.Info("GameObject created!");
            _mainObject = CreateUi();
            _mainObject.SetActive(false);
        }
    }

    private static GameObject CreateUi()
    {
        var parentObject = GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Ship Health Display/Health Bar");
        var mainObject = new GameObject("ShipHealthUI");
        mainObject.transform.SetParent(parentObject.transform, false);
        var rectTransform = mainObject.AddComponent<RectTransform>();
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, -19, 0);
        var textMesh = mainObject.AddComponent<TextMeshProUGUI>();
        textMesh.font = Resources.FontPenumbraHalfSerifStd;
        textMesh.text = "<mspace=2em>9999/9999";
        textMesh.fontSize = _fontSize;
        _textMeshProUGUI = textMesh;
        mainObject.AddComponent<ShipHealthUI>();
        return mainObject;
    }

    // Old update method, moved to event based updates instead of every frame.
    //private void LateUpdate()
    //{
    //    UpdateUI();
    //}

    /// <summary>
    /// Any time a ship adds a hull, check if the calling ship is the player's and update the UI.
    /// Sometimes does not work... It is always something...
    /// </summary>
    //[HarmonyPatch(typeof(Ship), nameof(Ship.AddHull))]
    //[HarmonyPostfix]
    //private static void Ship_AddHull(Ship __instance)
    //{
    //    var playerShip = _currentShip;
    //    if (playerShip == null) return;
    //    var callingShipIsPlayers = ReferenceEquals(__instance, playerShip);
    //    if (!callingShipIsPlayers) return;
    //    UpdateUI();
    //}

    /// <summary>
    /// Any time a player's parent is changed, update the UI. Change occurs on ship death and spawn.
    /// Should run less often than polling frequently.
    /// </summary>
    [HarmonyPatch(typeof(NetworkedPlayer), nameof(NetworkedPlayer.OnParentChange))]
    [HarmonyPostfix]
    private static void NetworkedPlayer_OnParentChange()
    {
        UpdateUI();
    }

    /// <summary>
    /// Any time a hull is remotely updated, check if the calling hull is the player's and update the UI.
    /// </summary>
    [HarmonyPatch(typeof(Hull), nameof(Hull.OnRemoteUpdate))]
    [HarmonyPostfix]
    private static void Hull_OnRemoteUpdate(Hull __instance)
    {
        var playerHull = _currentShipHull;
        if (playerHull == null) return;
        var callingHullIsPlayers = ReferenceEquals(__instance, playerHull);
        if (!callingHullIsPlayers) return;
        UpdateUI();
    }

    /// <summary>
    /// Clear UI on ship death so it does not briefly appear on next spawn.
    /// </summary>
    [HarmonyPatch(typeof(NetworkedPlayer), nameof(NetworkedPlayer.OnShipDeath))]
    [HarmonyPostfix]
    private static void NetworkedPlayer_OnShipDeath()
    {
        _textMeshProUGUI.text = "";
    }

    /// <summary>
    /// Updates and sets the text for the UI.
    /// </summary>
    private static void UpdateUI()
    {
        var hull = _currentShipHull;
        if (hull == null) return;
        var newText = _selectedTextFormat.F([hull.CoreHealth, hull.MaxCoreHealth, hull.PercentCoreHealth]);
        if (newText != _textMeshProUGUI.text) _textMeshProUGUI.text = newText;
    }

    /// <summary>
    /// Activate feature.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UIMatchBlockState), nameof(UIManager.UIMatchBlockState.Exit))] // Normal match start.
    [HarmonyPatch(typeof(UIManager.UILoadingBlockState), nameof(UIManager.UILoadingBlockState.Exit))] // Join running match.
    [HarmonyPostfix]
    private static void UIManager_UIMatchBlockState_Exit()
    {
        if (_shipHealthDisplayOption == ShipHealthDisplayOption.Disabled) return;
        var shouldBeEnabled = Util.MissionIsNotPvP();
        MuseLog.Info($"shouldBeEnabled: {shouldBeEnabled}");
        if (!shouldBeEnabled) return;
        MuseLog.Info($"Activating with display option: {_shipHealthDisplayOption}.");
        _mainObject.SetActive(true);
    }

    /// <summary>
    /// Deactivate feature.
    /// </summary>
    [HarmonyPatch(typeof(Mission), nameof(Mission.OnDisable))]
    [HarmonyPostfix]
    private static void Mission_OnDisable()
    {
        if (_mainObject != null && _mainObject.activeSelf) _mainObject.SetActive(false);
    }
}