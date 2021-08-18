using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Muse.Goi2.Entity;

namespace BuffKit.InfoPanels
{
    public static class ShipStatsPanel
    {

        private static GameObject _obOriginal;
        private static GameObject _obPanel;

        private static TextMeshProUGUI _lHull, _lArmor, _lMass, _lAcc, _lSpeed, _lTurnAcc, _lTurnSpeed, _lVertAcc, _lVertSpeed;
        private static TextMeshProUGUI _lRebuildHits;
        //private static TextMeshProUGUI _lThrust, _lTorque, _lLiftForce;

        private static Dictionary<int, Dictionary<GameType, Dictionary<string, float>>> _shipDataDict;

        public static void SetShip(ShipModel model)
        {
            var data = _shipDataDict[model.Id][NetworkedPlayer.Local.GameType];

            _lHull.text = String.Format("{0:0.###}", data["hull"]);
            _lArmor.text = String.Format("{0:0.###}", data["armor"]);

            _lRebuildHits.text = String.Format("{0:0}", data["rebuild"]);

            _lMass.text = String.Format("{0:0.} tonnes", data["mass"] / 1e3f);
            _lAcc.text = String.Format("{0:0.###} m/s²", data["forward acceleration"]);
            _lSpeed.text = String.Format("{0:0.###} m/s", data["forward speed"]);
            _lTurnAcc.text = String.Format("{0:0.###} °/s²", data["turning acceleration"]);
            _lTurnSpeed.text = String.Format("{0:0.###} °/s", data["turning speed"]);
            _lVertAcc.text = String.Format("{0:0.###} m/s²", data["vertical acceleration"]);
            _lVertSpeed.text = String.Format("{0:0.###} m/s", data["vertical speed"]);
            //_lThrust.text = String.Format("{0:0.###} kN", data["thrust"]/1e3);
            //_lTorque.text = String.Format("{0:0.###} MNm", data["torque"]/1e6);
            //_lLiftForce.text = String.Format("{0:0.###} kN", data["lift"]/1e3);
        }

        private static void BuildPanel(Transform parent)
        {
            _obPanel = UI.Builder.BuildPanel(parent);
            _obPanel.name = "Custom Ship Stats";
            var le = _obPanel.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rt = _obPanel.GetComponent<RectTransform>();
            rt.pivot = new Vector2(1, 0);
            rt.offsetMin = new Vector2(0, 40);
            rt.offsetMax = new Vector2(0, 0);
            var vlg = _obPanel.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(6, 6, 5, 5);
            var csf = _obPanel.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildRow(_obPanel.transform, "Hull", out _lHull);
            BuildRow(_obPanel.transform, "Armor", out _lArmor);
            BuildRow(_obPanel.transform, "Required Rebuild", out _lRebuildHits);
            BuildRow(_obPanel.transform, "Mass", out _lMass);
            BuildRow(_obPanel.transform, "Acceleration", out _lAcc);
            BuildRow(_obPanel.transform, "Speed", out _lSpeed);
            BuildRow(_obPanel.transform, "Turning Acceleration", out _lTurnAcc);
            BuildRow(_obPanel.transform, "Turning Speed", out _lTurnSpeed);
            BuildRow(_obPanel.transform, "Vertical Acceleration", out _lVertAcc);
            BuildRow(_obPanel.transform, "Vertical Speed", out _lVertSpeed);
            //BuildRow(_obPanel.transform, "Thrust", out _lThrust);
            //BuildRow(_obPanel.transform, "Torque", out _lTorque);
            //BuildRow(_obPanel.transform, "Lift Force", out _lLiftForce);
        }
        private static void BuildRow(Transform parent, string name, out TextMeshProUGUI label)
        {
            var row = new GameObject($"row {name}");
            row.transform.SetParent(parent, false);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleRight;

            TextMeshProUGUI tmp;
            UI.Builder.BuildLabel(row.transform, out tmp, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 13).name = "label text";
            tmp.text = name;

            var spacer = new GameObject("spacer");
            spacer.transform.SetParent(row.transform, false);
            var le = spacer.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 20;

            UI.Builder.BuildLabel(row.transform, out label, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 13).name = "label info";
        }

        private static void SetEnabled(bool v)
        {
            _obPanel.SetActive(v);
            _obOriginal.SetActive(!v);
        }

        public static void Initialize()
        {
            _obOriginal = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Ship Stats").gameObject;
            _obOriginal.SetActive(false);
            BuildPanel(_obOriginal.transform.parent);

            _shipDataDict = new Dictionary<int, Dictionary<GameType, Dictionary<string, float>>>();

            foreach (var id in Util.Util.ShipIds)
            {
                var _currentShipDataDict = new Dictionary<GameType, Dictionary<string, float>>();

                var model = CachedRepository.Instance.Get<ShipModel>(id);
                foreach (var gameType in new List<GameType> { GameType.Skirmish, GameType.Coop })
                {
                    var armor = (from part in model.Parts
                                 where part.Type == ShipStaticPartType.RIGGING
                                 select part).Sum((ShipStaticPartEntity part) => part.MaxHealth);
                    var rebuild = Mathf.Max(1, (float)Math.Floor((3f + armor * 0.017f) * 3f));
                    var hull = (from part in model.Parts
                                where part.Type == ShipStaticPartType.RIGGING
                                select part).Sum((ShipStaticPartEntity part) => part.ArmorNum);
                    var mass = model.GetHullWeight(gameType);
                    var totalThrust = 0f;
                    var totalTorque = 0f;
                    foreach (var keyValuePair in model.GetEngines(gameType))
                    {
                        totalThrust += keyValuePair.Value.MaxSpeed;
                        totalTorque += keyValuePair.Value.MaxSpeed * Math.Abs(model.Slots[keyValuePair.Key].Position.X);
                    }
                    var lift = model.GetLift(gameType);
                    var forwardAcceleration = totalThrust / mass;
                    var forwardSpeed = Mathf.Sqrt(totalThrust / (222f * model.GetDrag(gameType)));
                    var radialAcceleration = totalTorque / model.GetMomentOfInertia(gameType);
                    var turnAcceleration = radialAcceleration * 57.29578f;
                    var turnSpeed = Mathf.Sqrt(totalTorque * 57.29578f / (444444f * model.GetAngularDrag(gameType)));
                    var verticalAcceleration = lift / mass;
                    var verticalSpeed = Mathf.Sqrt(lift / (4444f * model.GetVerticalDrag(gameType)));

                    var _currentGametypeShipDataDict = new Dictionary<string, float>();
                    _currentGametypeShipDataDict.Add("armor", armor);
                    _currentGametypeShipDataDict.Add("hull", hull);
                    _currentGametypeShipDataDict.Add("mass", mass);
                    _currentGametypeShipDataDict.Add("forward acceleration", forwardAcceleration);
                    _currentGametypeShipDataDict.Add("forward speed", forwardSpeed);
                    _currentGametypeShipDataDict.Add("turning acceleration", turnAcceleration);
                    _currentGametypeShipDataDict.Add("turning speed", turnSpeed);
                    _currentGametypeShipDataDict.Add("vertical acceleration", verticalAcceleration);
                    _currentGametypeShipDataDict.Add("vertical speed", verticalSpeed);
                    _currentGametypeShipDataDict.Add("thrust", totalThrust);
                    _currentGametypeShipDataDict.Add("torque", totalTorque);
                    _currentGametypeShipDataDict.Add("lift", lift);

                    _currentGametypeShipDataDict.Add("rebuild", rebuild);

                    _currentShipDataDict.Add(gameType, _currentGametypeShipDataDict);
                }
                _shipDataDict.Add(id, _currentShipDataDict);
            }

            Settings.Settings.Instance.AddEntry("detailed ship stat panel", SetEnabled, true);
        }
    }
}
