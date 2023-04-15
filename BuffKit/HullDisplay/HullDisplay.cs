using System.Linq;
using Muse.Goi2.Entity;

namespace BuffKit.HullDisplay
{
    public static class HullDisplay
    {

        private static int squidHull = 0;
        private static int galleonHull = 0;

        public static void Initialize()
        {
            // Squid ID 13
            squidHull = GetHullFromShipId(13);
            // Galleon ID 14
            galleonHull = GetHullFromShipId(14);

            System.Console.WriteLine("Squid hull : " + squidHull + ", Galleon hull : " + galleonHull);
        }

        private static int GetHullFromShipId(int shipId)
        {
            var model = CachedRepository.Instance.Get<ShipModel>(shipId);
            var shipHull = (from part in model.Parts
                            where part.Type == ShipStaticPartType.RIGGING
                            select part).Sum((ShipStaticPartEntity part) => part.ArmorNum);
            return shipHull;
        }
    }
}
