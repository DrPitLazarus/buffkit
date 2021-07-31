using Muse.Common;
using MuseBase.Multiplayer.Unity;
using UnityEngine;
using Muse.Goi2.Entity.Vo;
using MuseBase.Multiplayer;
using MuseBase.Multiplayer.Photon;

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

		public static void ForceSendMessage(string msg, string channel = "match")
		{
            // Note: This might break for PMs
			if (string.IsNullOrEmpty(msg))
			{
				return;
			}
            MuseWorldClient.Instance.Client.SendChatMessage(msg, channel);
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