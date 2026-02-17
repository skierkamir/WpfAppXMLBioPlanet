using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using WpfAppXMLBioPlanet;

namespace WpfAppXMLBioPlanet.Services
{
    public class XmlProductService
    {
        public List<ProduktModel> WczytajProdukty(string path)
        {
            XDocument doc;
            
            try 
            {
                doc = XDocument.Load(path);
            }
            catch
            {
                var xml = File.ReadAllText(path, System.Text.Encoding.GetEncoding(1250));
                doc = XDocument.Parse(xml);
            }

            return doc.Descendants("produkty")
                .Where(p => (string)p.Element("Producent") != null &&
                            p.Element("Producent").Value.Contains("ŚWIEŻE"))
                .Select(p => new ProduktModel
                {
                    EAN = (string)p.Element("kod_kreskowy") ?? "",
                    Nazwa = (string)p.Element("nazwa") ?? "",
                    Kraj = (string)p.Element("KrajPochodzeniaSkladnikow") ?? ""
                })
                .ToList();
        }
    }
}
