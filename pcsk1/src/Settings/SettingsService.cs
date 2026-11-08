#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;


namespace pcsk1.src.Settings
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private readonly string _defaultsPath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private sealed class SettingsFile
        {
            public int Width { get; set; } = 1280;
            public int Height { get; set; } = 720;
            public bool Fullscreen { get; set; }
            public float MasterVolume { get; set; } = 1f;
        }

        public SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string directory = Path.Combine(
             appData,
            "pcsk1"
            );

            Directory.CreateDirectory(directory);


            if (directory == null)
                throw new InvalidOperationException("Caminho de settings inválido.");

            _settingsPath = Path.Combine(
                directory,
                "settings.json"
            );

            _defaultsPath = Path.Combine(
                AppContext.BaseDirectory,
                "Content",
                "Settings",
                "defaults.json"
            );
        }

        public GameSettings Load()
        {
            string path = Path.Exists(_settingsPath)
                ? _settingsPath
                : _defaultsPath;
           
            try
            {
                if (!File.Exists(path))
                    return new GameSettings();

                string json = File.ReadAllText(path);

                SettingsFile? file = JsonSerializer.Deserialize<SettingsFile>(json, _jsonOptions);

                if (file == null) throw new InvalidDataException("Arquivo de configurações vazio");

                return FromFile(file);
            }
            catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException ||
            ex is ArgumentException ||
            ex is NotSupportedException)
            {
                Debug.WriteLine(
                    $"Falha ao carregar configurações: {ex}"
                );

                // Retorna valores seguros sem sobrescrever
                // automaticamente o arquivo com problema.
                return new GameSettings();
            }
        }

        public void Save(GameSettings settings)
        {
            if (settings == null) throw new ArgumentException("as configurações não podem ser nulas");

            SettingsFile file = ToFile(settings);
            string json = JsonSerializer.Serialize(file, _jsonOptions);
            string tempPath = _settingsPath + ".tmp";

            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _settingsPath, overwrite: true);
        }
        private static SettingsFile ToFile(GameSettings settings)
        {
            return new SettingsFile
            {
                Width = settings.Width,
                Height = settings.Height,
                MasterVolume = settings.MasterVolume,
                Fullscreen = settings.Fullscreen
            };
        }

        private static GameSettings FromFile(SettingsFile file)
        {
            GameSettings settings = new();

            settings.SetResolution(
                file.Width,
                file.Height
                );
            settings.SetVolume(file.MasterVolume);
            settings.SetFullscreen(file.Fullscreen);

            return settings;
        }
    }
}
