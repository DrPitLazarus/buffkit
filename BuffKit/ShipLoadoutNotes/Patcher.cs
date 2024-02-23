using BuffKit.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.ShipLoadoutNotes
{
    [HarmonyPatch]
    public class ShipLoadoutNotes
    {
        private static readonly string _name = "Ship Loadout Notes";
        private static readonly string _announceButtonText = "Announce to Crew";
        private static TMP_InputField _inputField;

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "SetActiveShip")]
        private static void Postfix()
        {
            var recommendedLoadoutObj = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/");
            var shipLoadoutNotesObj = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/" + _name);
            if (shipLoadoutNotesObj == null)
            {
                var spacer = new GameObject("Spacer");
                spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 3); // Add a 3px high spacer.
                spacer.transform.SetParent(recommendedLoadoutObj.transform, false);
                BuildPanel(recommendedLoadoutObj.transform);
                // Update the recommended loadout obj to put the new ship loadout notes in the right place.
                LayoutRebuilder.MarkLayoutForRebuild(recommendedLoadoutObj.GetComponent<RectTransform>());
            }

        }

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "UpdateCamera")]
        private static bool Prefix()
        {
            // Prevent ship preview camera from moving when the text box is focused.
            return !_inputField?.isFocused ?? true;
        }

        private static void BuildPanel(Transform parent)
        {
            var panelObj = new GameObject(_name);
            panelObj.transform.SetParent(parent, false);
            var le = panelObj.AddComponent<LayoutElement>();
            le.preferredHeight = 130;
            var vlg = panelObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(5, 0, 5, 5);
            vlg.spacing = 3;
            vlg.childAlignment = TextAnchor.UpperRight;
            var csf = panelObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Panel title
            var panelLabelObj = Builder.BuildLabel(panelObj.transform, _name, TextAnchor.MiddleRight, 13);

            // Text box
            var inputObj = Builder.BuildInputField(panelObj.transform);
            _inputField = inputObj.GetComponent<TMP_InputField>();
            _inputField.fontAsset = UI.Resources.FontGaldeanoRegular;
            _inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
            _inputField.enabled = false; // Needed to make text input work when drawn for the first time.
            _inputField.enabled = true;

            // Button
            var annouceToCrewButton = Builder.BuildButton(panelObj.transform, DoNothing, _announceButtonText, fontSize: 13);
            var annouceToCrewButtonLe = annouceToCrewButton.AddComponent<LayoutElement>();
            annouceToCrewButtonLe.preferredHeight = 30;
            annouceToCrewButtonLe.preferredWidth = 250;
        }

        private static void DoNothing() { }
    }
}