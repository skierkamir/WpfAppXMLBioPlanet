using ClosedXML.Excel;
using System.Collections.ObjectModel;
using WpfAppXMLBioPlanet.Models;
using System.IO;

namespace WpfAppXMLBioPlanet.Services
{
    public static class ExcelService
    {
        public static void Zapisz(ObservableCollection<ProduktModel> produkty, string sciezka)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Produkty");

            // Nagłówki
            ws.Cell(1, 1).Value = "EAN";
            ws.Cell(1, 2).Value = "Nazwa";
            ws.Cell(1, 3).Value = "Kraj";
            ws.Cell(1, 4).Value = "Zmiana";
            ws.Cell(1, 5).Value = "Nowy/Wycofany";

            int row = 2;
            foreach (var p in produkty)
            {
                ws.Cell(row, 1).Value = p.Ean ?? "";
                ws.Cell(row, 2).Value = p.Nazwa ?? "";
                ws.Cell(row, 3).Value = p.Kraj ?? "";
                ws.Cell(row, 4).Value = p.Zmiana ?? "";

                if (p.NowyProdukt) ws.Cell(row, 5).Value = "Nowy produkt";
                else if (p.Wycofany) ws.Cell(row, 5).Value = p.Zmiana;
                else ws.Cell(row, 5).Value = "";

                row++;
            }

            // Autofit wszystkich kolumn
            ws.Columns().AdjustToContents();

            workbook.SaveAs(sciezka);
        }
    }
}
