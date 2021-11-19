using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BuffKit.KothUiFix
{
    public static class KothUiFix
    {
        private static UIControlPointHUD _uiCkHud;
        private static Transform[] _topPips;
        private static float _teamHudDefaultY;
        private static float _pipDefaultY;

        public static void CreateFix()
        {
            var prototype1 =
                GameObject.Find(
                    "Game UI/Match UI/UI HUD Canvas/UI Mission Specific Displays/UI Mission-Specific Displays/UI Mission-Specific Displays/Skirmish Control Point UI/Container/Team 1 Score");
            var prototype2 =
                GameObject.Find(
                    "Game UI/Match UI/UI HUD Canvas/UI Mission Specific Displays/UI Mission-Specific Displays/UI Mission-Specific Displays/Skirmish Control Point UI/Container/Team 2 Score");
            var team3 = Object.Instantiate(prototype1, prototype1.transform.parent);
            team3.transform.SetSiblingIndex(2);
            team3.name = "Team 3 Score";
            var team4 = Object.Instantiate(prototype2, prototype2.transform.parent);
            team4.transform.SetSiblingIndex(3);
            team4.name = "Team 4 Score";

            _uiCkHud = prototype1.transform.parent.parent.GetComponent<UIControlPointHUD>();
            _teamHudDefaultY = prototype1.transform.localPosition.y;
            var newHuds = new UISkirmishHUD.TeamHUD[4];
            Array.Copy(_uiCkHud.teamHuds, newHuds, 2);
            newHuds[2] = new UISkirmishHUD.TeamHUD
            {
                root = team3,
                background = team3.transform.GetChild(0).GetComponent<Image>(),
                scoreLabel = team3.transform.GetChild(1).GetComponent<Text>(),
                myTeamIndicator = team3.transform.GetChild(2).GetComponent<Image>()
            };
            
            newHuds[3] = new UISkirmishHUD.TeamHUD
            {
                root = team4,
                background = team4.transform.GetChild(0).GetComponent<Image>(),
                scoreLabel = team4.transform.GetChild(1).GetComponent<Text>(),
                myTeamIndicator = team4.transform.GetChild(2).GetComponent<Image>()
            };

            _uiCkHud.teamHuds = newHuds;
            _uiCkHud.teamHuds[2].background.color = TeamColors.GetColor(2);
            _uiCkHud.teamHuds[3].background.color = TeamColors.GetColor(3);
            
            _uiCkHud.teamHuds[2].root.gameObject.SetActive(false);
            _uiCkHud.teamHuds[3].root.gameObject.SetActive(false);

            var uicp = GameObject.Find(
                "Game UI/Match UI/UI HUD Canvas/UI Mission Specific Displays/UI Mission-Specific Displays/UI Mission-Specific Displays/Skirmish Control Point UI/Container/UI Control Point");
            
            _topPips = new[]
            {
                uicp.transform.FindChild("Team Zero Ship Pip 1"),
                uicp.transform.FindChild("Team Zero Ship Pip 2"),
                uicp.transform.FindChild("Team Zero Ship Pip 3"),
                uicp.transform.FindChild("Team Zero Ship Pip 4"),
                uicp.transform.FindChild("Team One Ship Pip 1"),
                uicp.transform.FindChild("Team One Ship Pip 2"),
                uicp.transform.FindChild("Team One Ship Pip 3"),
                uicp.transform.FindChild("Team One Ship Pip 4")
            };

            _pipDefaultY = _topPips[0].transform.localPosition.y;
        }

        public static void SetUiToTwoTeams()
        {
            if (!_uiCkHud.teamHuds[3].root.activeSelf) return;
            
            _uiCkHud.teamHuds[2].root.SetActive(false);
            _uiCkHud.teamHuds[3].root.SetActive(false);

            Vector3 temp;
            temp = _uiCkHud.teamHuds[0].root.transform.localPosition;
            temp.y = _teamHudDefaultY;
            _uiCkHud.teamHuds[0].root.transform.localPosition = temp;
            
            temp = _uiCkHud.teamHuds[1].root.transform.localPosition;
            temp.y = _teamHudDefaultY;
            _uiCkHud.teamHuds[1].root.transform.localPosition = temp;
            
            foreach (var pip in _topPips)
            {
                var lp = pip.localPosition;
                lp.y = _pipDefaultY;
                pip.localPosition = lp;
            }
        }

        public static void SetUiToFourTeams()
        {
            if (_uiCkHud.teamHuds[3].root.activeSelf) return;

            _uiCkHud.teamHuds[2].root.SetActive(true);
            _uiCkHud.teamHuds[3].root.SetActive(true);
            //Yes, this is an abomination.
            //At least it's an explicit abomination, right?
            Vector3 temp;

            //Teams 1 and 2 are on the top
            temp = _uiCkHud.teamHuds[0].root.transform.localPosition;
            temp.y =  _teamHudDefaultY + 12.5f;
            _uiCkHud.teamHuds[0].root.transform.localPosition = temp;
            
            temp = _uiCkHud.teamHuds[1].root.transform.localPosition;
            temp.y = _teamHudDefaultY + 12.5f;
            _uiCkHud.teamHuds[1].root.transform.localPosition = temp;
            
            //Teams 3 and 4 are on the bottom
            temp = _uiCkHud.teamHuds[2].root.transform.localPosition;
            temp.y = _teamHudDefaultY - 12.5f;
            _uiCkHud.teamHuds[2].root.transform.localPosition = temp;
            
            temp = _uiCkHud.teamHuds[3].root.transform.localPosition;
            temp.y = _teamHudDefaultY - 12.5f;
            _uiCkHud.teamHuds[3].root.transform.localPosition = temp;
         
            foreach (var pip in _topPips)
            {
                var lp = pip.localPosition;
                lp.y = _teamHudDefaultY + 12.5f;
                pip.localPosition = lp;
            }
        }
    }
}
