using HorseFeederAvalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HorseFeederAvalonia.Services
{
    public class DataPersistenceService
    {
        private readonly string configFile;

        public DataPersistenceService()
        {
            configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HorseFeederConfig.json");
        }

        public List<SlotConfiguration> GetConfigurations()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    var configFileJsonString = File.ReadAllText(configFile);
                    var slotConfigurations = JsonSerializer.Deserialize<List<SlotConfiguration>>(configFileJsonString);
                    return slotConfigurations;
                }
            }
            catch
            {
                return new List<SlotConfiguration>();
            }
            return new List<SlotConfiguration>();
        }

        public void SaveConfiguration(SlotConfiguration slotConfiguration)
        {
            var configs = GetConfigurations();
            var config = configs.SingleOrDefault(s => s.Slot == slotConfiguration.Slot);

            if (config is null)
            {
                config = new SlotConfiguration
                {
                    Slot = slotConfiguration.Slot,
                    ExpirationDate = slotConfiguration.ExpirationDate,
                    RepetitionFrequency = slotConfiguration.RepetitionFrequency
                };
                configs.Add(config);
            }
            else
            {
                config.ExpirationDate = slotConfiguration.ExpirationDate;
                config.RepetitionFrequency = slotConfiguration.RepetitionFrequency;
            }

            var updatedConfigsJsonString = JsonSerializer.Serialize(configs);
            File.WriteAllText(configFile, updatedConfigsJsonString);
        }
    }
}
