using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace WpfAppXMLBioPlanet.Services
{
    public class ApiSettings
    {
        public string ApiAddress { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        public string ApiAddress2 { get; set; } = string.Empty; // API magazynowe

    }

    public static class ApiSettingsService
    {
        private static readonly string SettingsFilePath = "ApiSettings.json";

        /// <summary>
        /// Wczytuje ustawienia API z pliku. 
        /// Jeśli plik nie istnieje, tworzy domyślny.
        /// </summary>
        public static ApiSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    var defaultSettings = new ApiSettings();
                    Save(defaultSettings); // tworzymy plik z domyślnymi wartościami
                    return defaultSettings;
                }

                string json = File.ReadAllText(SettingsFilePath, Encoding.UTF8);

                // Deserializacja wyłącznie klasy ApiSettings
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var settings = JsonSerializer.Deserialize<ApiSettings>(json, options);

                // Jeśli deserializacja się nie powiodła, zwracamy domyślny obiekt
                return settings ?? new ApiSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania ustawień API:\n" + ex.Message,
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new ApiSettings();
            }
        }

        /// <summary>
        /// Zapisuje ustawienia API do pliku
        /// </summary>
        public static void Save(ApiSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się zapisać ustawień API:\n" + ex.Message,
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
