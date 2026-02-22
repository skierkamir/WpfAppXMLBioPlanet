using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using WpfAppXMLBioPlanet.Models;
using WpfAppXMLBioPlanet.Services;

namespace WpfAppXMLBioPlanet.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const string CacheFilePath = "cache.json";
        private readonly ApiSettings apiSettings;

        public MainViewModel()
        {
            // Wczytanie ustawień API
            apiSettings = ApiSettingsService.Load();
            ApiAddress = apiSettings.ApiAddress;
            ApiKey = apiSettings.ApiKey;
            ApiAddress2 = apiSettings.ApiAddress2; // 🔹 dodatkowe API magazynowe

            // Wczytanie cache produktów
            WczytajCache();
        }

        // =========================
        // Properties (Observable)
        // =========================
        [ObservableProperty] private ObservableCollection<ProduktModel> produkty = new();
        [ObservableProperty] private bool filtrNowy = true;
        [ObservableProperty] private bool filtrWycofany = true;
        [ObservableProperty] private bool filtrZmiana = true;
        [ObservableProperty] private bool filtrReszta = true;
        [ObservableProperty] private string sciezkaDoExcela = "Produkty.xlsx";
        [ObservableProperty] private string apiAddress;
        [ObservableProperty] private string apiKey;
        [ObservableProperty] private string apiAddress2; // 🔹 API magazynowe

        private ObservableCollection<ProduktModel> WszystkieProdukty = new();

        // =========================
        // Cache produktów
        // =========================
        private void ZapiszCache()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(WszystkieProdukty, options);
                File.WriteAllText(CacheFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Błąd podczas zapisu cache: " + ex.Message);
            }
        }

        private void WczytajCache()
        {
            try
            {
                if (!File.Exists(CacheFilePath)) return;

                string json = File.ReadAllText(CacheFilePath, Encoding.UTF8);
                var lista = JsonSerializer.Deserialize<List<ProduktModel>>(json);
                if (lista != null)
                {
                    WszystkieProdukty = new ObservableCollection<ProduktModel>(lista);
                    FiltrujProdukty();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Błąd podczas wczytywania cache: " + ex.Message);
            }
        }

        // =========================
        // Zapis ustawień API
        // =========================
        [RelayCommand]
        private void ZapiszUstawieniaApi()
        {
            apiSettings.ApiAddress = ApiAddress;
            apiSettings.ApiKey = ApiKey;
            apiSettings.ApiAddress2 = ApiAddress2; // 🔹 zapis API magazynowe
            ApiSettingsService.Save(apiSettings);

            System.Windows.MessageBox.Show("Ustawienia API zapisane", "Info",
                                           System.Windows.MessageBoxButton.OK,
                                           System.Windows.MessageBoxImage.Information);
        }

        // =========================
        // Wczytaj XML z pliku
        // =========================
        [RelayCommand]
        private void WczytajXml()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Pliki XML (*.xml)|*.xml",
                    InitialDirectory = File.Exists("lastfolder.txt")
                        ? File.ReadAllText("lastfolder.txt")
                        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dlg.ShowDialog() != true) return;

                string sciezkaXml = dlg.FileName;
                var folder = Path.GetDirectoryName(sciezkaXml);
                if (!string.IsNullOrEmpty(folder)) File.WriteAllText("lastfolder.txt", folder);

                var noweProdukty = XmlService.WczytajProdukty(sciezkaXml);
                if (noweProdukty == null || !noweProdukty.Any())
                {
                    System.Windows.MessageBox.Show("Plik nie zawiera produktów lub ma nieprawidłową strukturę.",
                                                   "Informacja", System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                PrzetworzNoweProdukty(noweProdukty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Błąd podczas wczytywania pliku:\n\n" + ex.Message,
                                               "Błąd", System.Windows.MessageBoxButton.OK,
                                               System.Windows.MessageBoxImage.Error);
            }
        }

        // =========================
        // Wczytaj XML z API + API magazynowe
        // =========================
        [RelayCommand]
        private async Task WczytajApi()
        {
            if (string.IsNullOrWhiteSpace(ApiAddress) ||
                string.IsNullOrWhiteSpace(ApiKey) ||
                string.IsNullOrWhiteSpace(ApiAddress2))
            {
                System.Windows.MessageBox.Show(
                    "Brak ustawień API. Uzupełnij ustawienia przed pobraniem XML z API.",
                    "Info",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            try
            {
                using HttpClient client = new();

                // 🔹 pobranie podstawowego XML
                string fullUrl = $"{ApiAddress.TrimEnd('/')}/{ApiKey.Trim()}";
                var response = await client.GetAsync(fullUrl);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Windows.MessageBox.Show($"Błąd HTTP {(int)response.StatusCode}\n\n{responseText}");
                    return;
                }

                File.WriteAllText("api_debug.txt", responseText, Encoding.UTF8);

                if (!responseText.TrimStart().StartsWith("<"))
                {
                    System.Windows.MessageBox.Show("API nie zwróciło poprawnego XML.\n\n" +
                                                   responseText.Substring(0, Math.Min(300, responseText.Length)));
                    return;
                }

                var noweProdukty = XmlService.WczytajProduktyZString(responseText);

                // 🔹 pobranie stanów magazynowych (drugi API)
                var stanyMagazynowe = await PobierzStanyMagazynowe();

                // 🔹 połączenie po EAN
                foreach (var produkt in noweProdukty)
                {
                    if (stanyMagazynowe.TryGetValue(produkt.Ean, out int qty))
                        produkt.StanMagazynowy = qty; // 🔹 nowa właściwość ProduktModel
                    else
                        produkt.StanMagazynowy = 0;
                }

                PrzetworzNoweProdukty(noweProdukty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Błąd API: " + ex.Message);
            }
        }

        // =========================
        // Pobranie stanów magazynowych z drugiego API
        // =========================
        private async Task<Dictionary<string, int>> PobierzStanyMagazynowe()
        {
            var wynik = new Dictionary<string, int>();
            try
            {
                using HttpClient client = new();
                string fullUrl = $"{ApiAddress2.TrimEnd('/')}/{ApiKey.Trim()}";
                var response = await client.GetAsync(fullUrl);
                var text = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return wynik;

                var xml = XDocument.Parse(text);

                foreach (var product in xml.Descendants("product"))
                {
                    string ean = product.Element("ean")?.Value.Trim() ?? "";
                    string qtyStr = product.Element("qty")?.Value.Trim() ?? "0";

                    if (int.TryParse(qtyStr, out int qty))
                        wynik[ean] = qty;
                }
            }
            catch
            {
                // ignorujemy błędy, zwracamy pusty słownik
            }

            return wynik;
        }

        // =========================
        // Reset danych
        // =========================
        [RelayCommand]
        private void Reset()
        {
            var password = Microsoft.VisualBasic.Interaction.InputBox(
                "Podaj hasło aby zresetować dane", "Reset danych");

            if (password != "admin") return;

            Produkty.Clear();
            WszystkieProdukty.Clear();

            if (File.Exists("lastfolder.txt")) File.Delete("lastfolder.txt");
            if (File.Exists(SciezkaDoExcela)) File.Delete(SciezkaDoExcela);
            if (File.Exists(CacheFilePath)) File.Delete(CacheFilePath);
        }

        // =========================
        // Metoda przetwarzająca produkty
        // =========================
        private void PrzetworzNoweProdukty(List<ProduktModel> noweProdukty)
        {
            foreach (var nowy in noweProdukty)
            {
                var istniejący = WszystkieProdukty.FirstOrDefault(p => p.Ean == nowy.Ean);

                if (istniejący != null)
                {
                    nowy.Zmiana = istniejący.Kraj != nowy.Kraj
                        ? $"Zmieniono z: {istniejący.Kraj} na {nowy.Kraj}"
                        : string.Empty;
                    nowy.Wycofany = istniejący.Wycofany;
                    nowy.NowyProdukt = false;
                    WszystkieProdukty.Remove(istniejący);
                }
                else
                {
                    nowy.NowyProdukt = true;
                    nowy.Wycofany = false;
                    nowy.Zmiana = string.Empty;
                }

                WszystkieProdukty.Add(nowy);
            }

            var eanyNowych = noweProdukty.Select(p => p.Ean).ToHashSet();
            foreach (var stary in WszystkieProdukty.Where(p => !eanyNowych.Contains(p.Ean)).ToList())
            {
                stary.Wycofany = true;
                stary.NowyProdukt = false;
                stary.Zmiana = $"Wycofany w dniu {DateTime.Now:yyyy-MM-dd}";
            }

            Produkty = new ObservableCollection<ProduktModel>(WszystkieProdukty.OrderBy(p => p.Nazwa));

            ExcelService.Zapisz(Produkty, SciezkaDoExcela);
            ZapiszCache();
        }

        // =========================
        // Filtracja produktów
        // =========================
        private void FiltrujProdukty()
        {
            var lista = WszystkieProdukty.Where(p =>
                (FiltrNowy && p.NowyProdukt) ||
                (FiltrWycofany && p.Wycofany) ||
                (FiltrZmiana && !string.IsNullOrEmpty(p.Zmiana)) ||
                (FiltrReszta && !p.NowyProdukt && !p.Wycofany && string.IsNullOrEmpty(p.Zmiana))
            );

            Produkty = new ObservableCollection<ProduktModel>(lista.OrderBy(p => p.Nazwa));
        }

        [RelayCommand]
        private void OtworzUstawieniaApi()
        {
            var settingsWindow = new WpfAppXMLBioPlanet.Views.ApiSettingsWindow(ApiAddress, ApiKey, ApiAddress2)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (settingsWindow.ShowDialog() == true)
            {
                ApiAddress = settingsWindow.ApiAddress;
                ApiKey = settingsWindow.ApiKey;
                ApiAddress2 = settingsWindow.ApiAddress2;

                apiSettings.ApiAddress = ApiAddress;
                apiSettings.ApiKey = ApiKey;
                apiSettings.ApiAddress2 = ApiAddress2;
                //ApiSettingsService.Save(apiSettings);
                ApiSettingsService.Save(new ApiSettings
                {
                    ApiAddress = ApiAddress,
                    ApiKey = ApiKey,
                    ApiAddress2 = ApiAddress2
                });
            }
        }

        partial void OnFiltrNowyChanged(bool value) => FiltrujProdukty();
        partial void OnFiltrWycofanyChanged(bool value) => FiltrujProdukty();
        partial void OnFiltrZmianaChanged(bool value) => FiltrujProdukty();
        partial void OnFiltrResztaChanged(bool value) => FiltrujProdukty();
    }
}
