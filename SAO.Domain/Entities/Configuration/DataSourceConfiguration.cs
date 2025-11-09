using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities.Configuration
{
    public sealed class DataSourceConfiguration
    {
        
        public string Name { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public bool IsEnabled { get; init; }

        public int Priority { get; init; }

        public Dictionary<string, string> Settings { get; init; } = new();

        private DataSourceConfiguration() { }

        public static DataSourceConfiguration Create(
            string name,
            string type,
            bool isEnabled = true,
            int priority = 0,
            Dictionary<string, string>? settings = null)
        {
            return new DataSourceConfiguration
            {
                Name = name,
                Type = type,
                IsEnabled = isEnabled,
                Priority = priority,
                Settings = settings ?? new Dictionary<string, string>()
            };
        }

        public string? GetSetting(string key)
        {
            return Settings.TryGetValue(key, out var value) ? value : null;
        }

        public string GetSettingOrDefault(string key, string defaultValue)
        {
            return Settings.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
