using System.Linq;
using Muse.Common;
using Muse.Goi2.Entity;
using static BuffKit.Util;

namespace BuffKit.MapPicker
{
    public static class MapPicker
    {
        public static bool Paint()
        {
            var comparer = new IntArrayEqualityComparer();
            var mlv = MatchLobbyView.Instance;
            var rawMaps = CachedRepository
                .Instance
                .GetBy(
                    (Region r) =>
                        r.Public && r.GameMode.GetGameType() == mlv.Map.GameMode.GetGameType() &&
                        comparer.Equals(r.NonEmptyTeamSize, mlv.Map.NonEmptyTeamSize)
                );

            //If playing on a DM map, only show DM maps except for Batcave
            if (mlv.Map.GameMode == RegionGameMode.TEAM_MELEE)
                rawMaps = rawMaps.Where(m => m.GameMode == RegionGameMode.TEAM_MELEE)
                    .Where(m => !m.Name.Equals("Batcave"));
            //If playing on a VIP DM map, only show VIP DM
            else if (mlv.Map.GameMode == RegionGameMode.TEAM_MELEE_VIP)
                rawMaps = rawMaps.Where(m => m.GameMode == RegionGameMode.TEAM_MELEE_VIP);

            var maps = rawMaps.OrderBy(m => m.GetLocalizedName()).ToArray();

            UINewModalDialog.Select("Select Map", "Current: " + mlv.Map.GetLocalizedName(),
                UINewModalDialog.DropdownSetting.CreateSetting(maps,
                    r => "{0} ({1})".F(r.GetLocalizedName(), r.GameMode.GetString())), delegate(int index)
                {
                    if (index >= 0) LobbyActions.ChangeMap(maps[index].Id);
                });

            return false;
        }
        
    }
}