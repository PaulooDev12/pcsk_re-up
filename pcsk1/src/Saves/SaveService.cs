#nullable enable
using System;
using System.IO;
using System.Text.Json;

namespace pcsk1.src.Saves
{
    public sealed class SaveService
    {
        private readonly string _saveDirectory;
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
        };

        public SaveService()
        {
            string appData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
                );

            _saveDirectory = Path.Combine(
                appData,
                "pcks1",
                "saves"
                );

            Directory.CreateDirectory(_saveDirectory);
        }

        private string GetSavePath(int slot)
        {
            if(slot < 1 || slot > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));

            }
            return Path.Combine(_saveDirectory, $"slot{slot}.json");
        }

        public bool Exists(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public void Save(SaveData data, int slot)
        {
            if (data is null) throw new ArgumentException("data não pode ser null");

            string path = GetSavePath(slot);
            string tempPath = path + ".tmp";

            string json = JsonSerializer.Serialize(
                data,
                _options
                );
            File.WriteAllText(tempPath,json);
            File.Move(tempPath, path, overwrite: true);

        }
        public SaveData? Load(int slot)
        {
            string path = GetSavePath(slot);

            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);

            SaveData? data = JsonSerializer.Deserialize<SaveData>(
                json,
                _options
                );

            if (data is null) throw new ArgumentException("Save inválido ou nulo");

            if (data.Version != 1) throw new InvalidDataException($"Versão de save incopátivel {data.Version}");

            if(data.Flags is null || data.TalkedNpcs is null)
            {
                throw new InvalidDataException("Estrutura de save incompleta");
            }
            return data;
        }

        public void Delete(int slot = 1)
        {
            string path = GetSavePath(slot);

            if (File.Exists(path)) 
                File.Delete(path);
        }
    }
}
