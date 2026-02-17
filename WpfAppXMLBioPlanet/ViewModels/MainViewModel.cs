using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using WpfAppXMLBioPlanet;

namespace WpfAppXMLBioPlanet.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const string SettingsFile = "lastPath.txt";

        public ObservableCollection<ProduktModel> Produkty { get; } = new();

        public MainViewModel()
        {
        }

        // ===============================
        // WCZYTYWANIE XML
        // ===============================

        [RelayCommand]
        private void WczytajXml()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki XML (*.xml)|*.xml",
                InitialDirectory = LoadLastPath()
            };

            if (dialog.ShowDialog() != true)
                return;

            SaveLastPath(Path.GetDirectoryName(dialog.FileName)!);

            XDocument doc = XDocument.Load(dialog.FileName);

            var noweProdukty = doc.Descendants("Produkt")
                .Select(x => new ProduktModel
                {
                    Ean = x.Element("EAN")?.Value ?? "",
                    Nazwa = x.Element("Nazwa")?.Value ?? "",
                    Kraj = x.Element("Kraj")?.Value ?? ""
                })
                .ToList();

            foreach (var nowy in noweProdukty)
            {
                var istniejący = Produkty
                    .FirstOrDefault(p => p.Nazwa == nowy.Nazwa);

                if (istniejący != null)
                {
                    if (istniejący.Kraj != nowy.Kraj)
                    {
                        istniejący.Zmiana = "Zmieniono";
                    }
                    else
                    {
                        istniejący.Zmiana = "";
                    }

                    istniejący.Kraj = nowy.Kraj;
                }
                else
                {
                    nowy.Zmiana = "";
                    Produkty.Add(nowy);
                }
            }
        }

        // ===============================
        // RESET
        // ===============================

        [RelayCommand]
        private void Reset()
        {
            var dialog = new PasswordDialog();
            if (dialog.ShowDialog() == true)
            {
                if (dialog.Password == "admin")
                {
                    Produkty.Clear();
                    MessageBox.Show("Dane zostały wyczyszczone.");
                }
                else
                {
                    MessageBox.Show("Nieprawidłowe hasło.");
                }
            }
        }

        // ===============================
        // ZAPIS / ODCZYT OSTATNIEJ ŚCIEŻKI
        // ===============================

        private void SaveLastPath(string path)
        {
            File.WriteAllText(SettingsFile, path);
        }

        private string LoadLastPath()
        {
            if (File.Exists(SettingsFile))
                return File.ReadAllText(SettingsFile);

            return Directory.GetCurrentDirectory();
        }
    }
}
