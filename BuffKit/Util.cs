using Muse.Common;
using MuseBase.Multiplayer.Unity;

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
    }
}