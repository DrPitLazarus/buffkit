using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BuffKit.KothUiFix
{
    public static class KothUiFix
    {
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

            var uiCkHud = prototype1.transform.parent.parent.GetComponent<UIControlPointHUD>();
            var newHuds = new UISkirmishHUD.TeamHUD[4];
            Array.Copy(uiCkHud.teamHuds, newHuds, 2);
            newHuds[2] = new UISkirmishHUD.TeamHUD
            {
                root = team3,
                background = team3.transform.GetChild(0).GetComponent<Image>(),
                scoreLabel = team3.transform.GetChild(1).GetComponent<Text>(),
                myTeamIndicator = team3.transform.GetChild(2).GetComponent<Image>(),
            };
            
            newHuds[3] = new UISkirmishHUD.TeamHUD
            {
                root = team4,
                background = team4.transform.GetChild(0).GetComponent<Image>(),
                scoreLabel = team4.transform.GetChild(1).GetComponent<Text>(),
                myTeamIndicator = team4.transform.GetChild(2).GetComponent<Image>(),
            };

            uiCkHud.teamHuds = newHuds;
            uiCkHud.teamHuds[2].background.color = TeamColors.GetColor(2);
            uiCkHud.teamHuds[3].background.color = TeamColors.GetColor(3);

            Vector3 temp;
            
            temp = uiCkHud.teamHuds[0].root.transform.localPosition;
            temp.y += 12.5f;
            uiCkHud.teamHuds[0].root.transform.localPosition = temp;
            
            temp = uiCkHud.teamHuds[1].root.transform.localPosition;
            temp.y += 12.5f;
            uiCkHud.teamHuds[1].root.transform.localPosition = temp;
            
            temp = uiCkHud.teamHuds[2].root.transform.localPosition;
            temp.y -= 12.5f;
            uiCkHud.teamHuds[2].root.transform.localPosition = temp;
            
            temp = uiCkHud.teamHuds[3].root.transform.localPosition;
            temp.y -= 12.5f;
            uiCkHud.teamHuds[3].root.transform.localPosition = temp;

            var uicp = GameObject.Find(
                "Game UI/Match UI/UI HUD Canvas/UI Mission Specific Displays/UI Mission-Specific Displays/UI Mission-Specific Displays/Skirmish Control Point UI/Container/UI Control Point");
            var pipsOne = new Transform[4]
            {
                uicp.transform.FindChild("Team Zero Ship Pip 1"),
                uicp.transform.FindChild("Team Zero Ship Pip 2"),
                uicp.transform.FindChild("Team Zero Ship Pip 3"),
                uicp.transform.FindChild("Team Zero Ship Pip 4")
            };
            
            var pipsTwo = new Transform[4]
            {
                uicp.transform.FindChild("Team One Ship Pip 1"),
                uicp.transform.FindChild("Team One Ship Pip 2"),
                uicp.transform.FindChild("Team One Ship Pip 3"),
                uicp.transform.FindChild("Team One Ship Pip 4")
            };

            foreach (var pip in pipsOne)
            {
                var a = pip.localPosition;
                a.y += 12.5f;
                pip.localPosition = a;
            }
            
            foreach (var pip in pipsTwo)
            {
                var a = pip.localPosition;
                a.y += 12.5f;
                pip.localPosition = a;
            }
        }
    }
}
