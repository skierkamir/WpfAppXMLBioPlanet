using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WpfAppXMLBioPlanet.Models;

namespace WpfAppXMLBioPlanet.Services
{
    public static class XmlService
    {
        /// <summary>
        /// Wczytuje produkty z pliku XML (UTF-8 lub Windows-1250) i zwraca listę ProduktModel.
        /// </summary>
        public static List<ProduktModel> WczytajProdukty(string sciezka)
        {
            // Pobieramy surowe bajty z pliku
            byte[] bytes = File.ReadAllBytes(sciezka);

            // Zamieniamy bajty na string z wykrytym kodowaniem
            string xmlContent = WczytajXmlContent(bytes);

            // Wczytujemy produkty ze stringa
            return WczytajProduktyZString(xmlContent);
        }

        /// <summary>
        /// Wczytuje produkty z XML w formie string (UTF-8 lub Windows-1250)
        /// </summary>
        public static List<ProduktModel> WczytajProduktyZString(string xmlContent)
        {
            var produkty = new List<ProduktModel>();

            XDocument doc;
            using (var stringReader = new StringReader(xmlContent))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                doc = XDocument.Load(xmlReader);
            }

            // Każdy element <produkty> i filtr po producencie
            foreach (var node in doc.Descendants("produkty")
                                     .Where(p =>
                                     {
                                         var producent = p.Element("Producent")?.Value ?? "";
                                         return producent.IndexOf("ŚWIEŻE", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                producent.IndexOf("warzywa", StringComparison.OrdinalIgnoreCase) >= 0;
                                     }))
            {
                string kod = node.Element("kod_kreskowy")?.Value ?? "";
                string nazwa = node.Element("nazwa")?.Value ?? "";
                string kraj = node.Element("KrajPochodzeniaSkladnikow")?.Value ?? "";

                // Przestawianie OPAKOWANIA ZBIORCZE na koniec
                if (nazwa.StartsWith("OPAKOWANIE ZBIORCZE (kg) - "))
                    nazwa = nazwa.Replace("OPAKOWANIE ZBIORCZE (kg) - ", "") + " - OPAKOWANIE ZBIORCZE (kg)";
                else if (nazwa.StartsWith("OPAKOWANIE ZBIORCZE (szt) - "))
                    nazwa = nazwa.Replace("OPAKOWANIE ZBIORCZE (szt) - ", "") + " - OPAKOWANIE ZBIORCZE (szt)";

                var produkt = new ProduktModel
                {
                    Ean = kod,
                    Nazwa = nazwa,
                    Kraj = kraj,
                    Status = "",
                    Zmiana = "",
                    NowyProdukt = false,
                    Wycofany = false,
                    NazwaBazowa = PobierzNazwaBazowa(nazwa)
                };

                produkty.Add(produkt);
            }

            return produkty;
        }

        /// <summary>
        /// Zamienia bajty XML na string z poprawnym kodowaniem (UTF-8 lub Windows-1250)
        /// </summary>
        private static string WczytajXmlContent(byte[] bytes)
        {
            // Najpierw UTF-8
            string text = Encoding.UTF8.GetString(bytes);

            // Jeśli pojawiły się znaki zastępcze, próbujemy Windows-1250
            if (text.Contains("�"))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                text = Encoding.GetEncoding(1250).GetString(bytes);
            }

            return text;
        }
        

        /// <summary>
        /// Wyciąga bazową nazwę produktu do grupowania.
        /// Bierze wszystko od początku do słowa BIO, po przestawieniu "OPAKOWANIE ZBIORCZE".
        /// </summary>
        private static string PobierzNazwaBazowa(string nazwa)
        {
            if (string.IsNullOrWhiteSpace(nazwa))
                return string.Empty;

            // Przestawianie OPAKOWANIA ZBIORCZE na koniec
            if (nazwa.StartsWith("OPAKOWANIE ZBIORCZE (kg) - "))
                nazwa = nazwa.Replace("OPAKOWANIE ZBIORCZE (kg) - ", "") + " - OPAKOWANIE ZBIORCZE (kg)";
            else if (nazwa.StartsWith("OPAKOWANIE ZBIORCZE (szt) - "))
                nazwa = nazwa.Replace("OPAKOWANIE ZBIORCZE (szt) - ", "") + " - OPAKOWANIE ZBIORCZE (szt)";

            // Wyciągnięcie części od początku do "(około"
            int idx = nazwa.IndexOf("(około", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                return nazwa.Substring(0, idx + 0).Trim(); // +6 wlicza "(około"
            else
                return nazwa;
        }
    }
}
