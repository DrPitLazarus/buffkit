using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BuffKit.LoadoutSort
{
    public class LoadoutSort
    {

        public static void SaveToFile(List<int> pilotToolOrder, List<int> gunnerToolOrder, List<int> engineerToolOrder)
        {
            var data = new List<List<int>>();
            data.Add(pilotToolOrder);
            data.Add(gunnerToolOrder);
            data.Add(engineerToolOrder);

            var dataString = JsonConvert.SerializeObject(data, Formatting.Indented);

            var filePath = @"BepInEx\plugins\BuffKit\loadout_order_settings.json";
            var gp = Directory.GetCurrentDirectory();
            var path = Path.Combine(gp, filePath);
            File.WriteAllText(path, dataString);

            MuseLog.Info("Saved settings to file");
        }

        // Returns true if successful, false otherwise
        public static bool LoadFromFile(out List<int> pilotToolOrder, out List<int> gunnerToolOrder, out List<int> engineerToolOrder)
        {
            pilotToolOrder = new List<int>();
            gunnerToolOrder = new List<int>();
            engineerToolOrder = new List<int>();
            try
            {
                var filePath = @"BepInEx\plugins\BuffKit\loadout_order_settings.json";
                var gp = Directory.GetCurrentDirectory();
                var path = Path.Combine(gp, filePath);
                var savedData = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<List<List<int>>>(savedData);

                pilotToolOrder = data[0];
                gunnerToolOrder = data[1];
                engineerToolOrder = data[2];

                return true;
            }
            catch (FileNotFoundException)
            {
                MuseLog.Info("Settings file was not found");
            }
            catch (JsonReaderException e)
            {
                MuseLog.Info($"Failed to read settings file:\n{e.Message}");
            }
            return false;
        }

    }


}
