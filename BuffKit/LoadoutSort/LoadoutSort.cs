using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BuffKit.LoadoutSort
{
    public class LoadoutSort
    {

        [Serializable]
        private struct LoadoutSortData
        {
            public List<int> pilotToolOrder;
            public List<int> gunnerToolOrder;
            public List<int> engineerToolOrder;
            public List<ClassSkillSet> specificSkillSets;
        }

        public static void SaveToFile()
        {
            var data = new LoadoutSortData
            {
                pilotToolOrder = UILoadoutSortPanel.Instance.PilotToolOrder,
                gunnerToolOrder = UILoadoutSortPanel.Instance.GunnerToolOrder,
                engineerToolOrder = UILoadoutSortPanel.Instance.EngineerToolOrder,
                specificSkillSets = UILoadoutSpecificSortPanel.Instance.GetSpecificSkillSets()
            };

            var dataString = JsonConvert.SerializeObject(data, Formatting.Indented);

            var filePath = @"BepInEx\plugins\BuffKit\loadout_order_settings.json";
            var gp = Directory.GetCurrentDirectory();
            var path = Path.Combine(gp, filePath);
            File.WriteAllText(path, dataString);

            MuseLog.Info("Saved loadout settings to file");
        }

        // Returns true if successful, false otherwise
        public static bool LoadFromFile(out List<int> pilotToolOrder, out List<int> gunnerToolOrder, out List<int> engineerToolOrder,
            out List<ClassSkillSet> specificSkillSets)
        {
            pilotToolOrder = new List<int>();
            gunnerToolOrder = new List<int>();
            engineerToolOrder = new List<int>();
            specificSkillSets = new List<ClassSkillSet>();
            try
            {
                var filePath = @"BepInEx\plugins\BuffKit\loadout_order_settings.json";
                var gp = Directory.GetCurrentDirectory();
                var path = Path.Combine(gp, filePath);
                var savedData = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<LoadoutSortData>(savedData);

                pilotToolOrder = data.pilotToolOrder;
                gunnerToolOrder = data.gunnerToolOrder;
                engineerToolOrder = data.engineerToolOrder;
                specificSkillSets = data.specificSkillSets;

                MuseLog.Info("Loaded loadout settings from file");

                return true;
            }
            catch (FileNotFoundException)
            {
                MuseLog.Info("Loadout settings file was not found");
            }
            catch (JsonReaderException e)
            {
                MuseLog.Info($"Failed to read loadout settings file:\n{e.Message}");
            }
            catch (JsonSerializationException e)
            {
                MuseLog.Info($"Failed to deserialise loadout settings file:\n{e.Message}");
            }
            return false;
        }

    }


}
