using System;
using System.IO;
using System.Text.Json;

namespace GameOverlay.Core
{
    public static class ConfigStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static OverlayConfig LoadOrCreate(string path)
        {
            OverlayConfig config = Read(path).Normalize();
            Save(path, config);
            return config;
        }

        public static void Save(string path, OverlayConfig config)
        {
            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(config.Normalize(), Options);
                File.WriteAllText(path, json);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static OverlayConfig Read(string path)
        {
            if (!File.Exists(path))
            {
                return OverlayConfig.CreateDefault();
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<OverlayConfig>(json, Options) ?? OverlayConfig.CreateDefault();
            }
            catch (JsonException)
            {
                return OverlayConfig.CreateDefault();
            }
            catch (IOException)
            {
                return OverlayConfig.CreateDefault();
            }
            catch (UnauthorizedAccessException)
            {
                return OverlayConfig.CreateDefault();
            }
        }
    }
}
