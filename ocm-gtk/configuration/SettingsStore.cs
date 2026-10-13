using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ocmgtk
{
    internal sealed class SettingsStore
    {
        private readonly string filePath;
        private readonly Dictionary<string, JsonElement> values = new Dictionary<string, JsonElement>();

        public SettingsStore(string settingsPath = null)
        {
            filePath = settingsPath ?? GetDefaultSettingsPath();
            Load();
        }

        public object Get(string key, object defaultValue)
        {
            if (!values.TryGetValue(key, out JsonElement value))
                return defaultValue;

            try
            {
                if (defaultValue is bool)
                    return value.GetBoolean();
                if (defaultValue is int)
                    return value.GetInt32();
                if (defaultValue is double)
                    return value.GetDouble();
                if (defaultValue is string)
                    return value.GetString() ?? defaultValue;
            }
            catch (InvalidOperationException)
            {
                return defaultValue;
            }
            catch (FormatException)
            {
                return defaultValue;
            }

            return defaultValue;
        }

        public void Set(string key, object value)
        {
            values[key] = JsonSerializer.SerializeToElement(value, value?.GetType() ?? typeof(object));
            Save();
        }

        public void Unset(string key)
        {
            if (values.Remove(key))
                Save();
        }

        private void Load()
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                Dictionary<string, JsonElement> loaded = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(filePath));
                if (loaded != null)
                {
                    foreach (KeyValuePair<string, JsonElement> item in loaded)
                        values[item.Key] = item.Value;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                values.Clear();
            }
        }

        private void Save()
        {
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            string temporaryPath = filePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(values));
            File.Move(temporaryPath, filePath, true);
        }

        private static string GetDefaultSettingsPath()
        {
            string configDirectory = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (string.IsNullOrWhiteSpace(configDirectory) || !Path.IsPathRooted(configDirectory))
                configDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(configDirectory))
                configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

            return Path.Combine(configDirectory, "opencache-manager", "settings.json");
        }
    }
}
