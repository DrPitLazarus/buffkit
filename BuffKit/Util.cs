using Muse.Common;
using MuseBase.Multiplayer.Unity;
using UnityEngine;
using Muse.Goi2.Entity.Vo;

namespace BuffKit
{
    public static class Util
    {
        public static bool HasModPrivilege(MatchLobbyView mlv)
        {
            return mlv.Moderated && NetworkedPlayer.Local.Privilege >= UserPrivilege.Referee ||
                   NetworkedPlayer.Local.Privilege.IsModerator();
        }
        
        public static void TrySendMessage(string message, string channel = "match")
        {
            MuseWorldClient.Instance.ChatHandler.TrySendMessage(message, channel);
        }

        public static string GetHierarchyPath(this Transform t)
        {
            string s = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                s = t.name + "/" + s;
            }
            return s;
        }

        public static ShipViewObject GetShipVO(this MatchLobbyView mlv, string crewId)
        {
            foreach (var csvo in mlv.CrewShips)
                if (csvo.CrewId == crewId) return ShipPreview.GetShipVO(csvo);
            return null;
        }
    }
}