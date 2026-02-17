using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

using WpfAppXMLBioPlanet;

namespace WpfAppXMLBioPlanet.Services
{
    public static class XmlService
    {
        public static List<ProduktModel> WczytajProdukty(string sciezka)
        {
            var produkty = new List<ProduktModel>();

            XDocument doc;

            try
            {
                // Najpierw próbujemy normalnie (UTF-8 / deklarowane w XML)
                doc = XDocument.Load(sciezka);
            }
            catch
            {
                // Jeśli się wywali – próbujemy Windows-1250
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                string xmlContent = File.ReadAllText(sciezka, Encoding.GetEncoding(1250));

                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader);

                doc = XDocument.Load(xmlReader);
            }

            foreach (var node in doc.Descendants("produkty")
                                    .Where(p =>
                                    {
                                        var producent = p.Element("Producent")?.Value;
                                        return producent != null &&
                                               producent.Contains("ŚWIEŻE", StringComparison.OrdinalIgnoreCase);
                                    }))
            {

                string kod = node.Element("kod_kreskowy")?.Value ?? "";
                string nazwa = node.Element("nazwa")?.Value ?? "";
                string kraj = node.Element("KrajPochodzeniaSkladnikow")?.Value ?? "";

                produkty.Add(new ProduktModel
                {
                    EAN = kod,
                    Nazwa = nazwa,
                    Kraj = kraj,
                    Status = "",
                    Zmiana = ""
                });
            }

            return produkty;
        }

    }
}