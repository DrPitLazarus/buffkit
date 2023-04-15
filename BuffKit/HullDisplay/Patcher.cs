using HarmonyLib;

namespace BuffKit.HullDisplay
{
    [HarmonyPatch(typeof(UIShipHealthIndicatorBar), "SetHealthBar")]
    class UILoadingLobbyState_Exit
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate { HullDisplay.Initialize(); };

                _firstPrepare = false;
            }
        }

        private static void Postfix(UIShipHealthIndicatorBar __instance, float value)
        {
            // TODO: 
            //  Create display label
            //  Ensure display label is only visible when it should be (tie to UIShipHealthIndicatorBar visibility??)
            //  Make display label show current ship hull in whatever unit
            //  Make settings menu for hull display
            //      Options:
            //          No display (default)            display is disabled
            //          Ship units                      units are based on the current ship, basically just numerical version of bar
            //          Squids                          units are based on Squid hull (450 as of time of writing this)
            //          Galleons                        units are based on Galleon hull (1750 as of time of writing this)
            var ship = NetworkedPlayer.Local.CurrentShip;
            var hull = ship.ActiveHull;
            System.Console.WriteLine("Armor: " + hull.Health + ", Hull: " + hull.CoreHealth);
        }
    }
}
