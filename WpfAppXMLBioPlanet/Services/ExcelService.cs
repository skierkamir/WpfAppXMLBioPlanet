using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WpfAppXMLBioPlanet;
using WpfAppXMLBioPlanet;

namespace WpfAppXMLBioPlanet.Services
{
    public static class ExcelService
    {
        public static void Zapisz(ObservableCollection<ProduktModel> produkty, string sciezka)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Produkty");

            ws.Cell(1, 1).Value = "EAN";
            ws.Cell(1, 2).Value = "Nazwa";
            ws.Cell(1, 3).Value = "Kraj";
            ws.Cell(1, 4).Value = "Zmiana kraju";
            ws.Cell(1, 5).Value = "Status";

            int row = 2;
            foreach (var p in produkty)
            {
                var cell = ws.Cell(row, 1);
                cell.Value = p.EAN ?? "";
                cell.Style.NumberFormat.Format = "@";

                ws.Cell(row, 2).Value = p.Nazwa ?? "";
                ws.Cell(row, 3).Value = p.Kraj ?? "";
                ws.Cell(row, 4).Value = p.Zmiana ?? "";

                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(sciezka);
        }
        public static Dictionary<string, string> WczytajPoprzednieKraje(string sciezka)
        {
            var kraje = new Dictionary<string, string>();

            if (!File.Exists(sciezka))
                return kraje;

            using var workbook = new XLWorkbook(sciezka);
            var ws = workbook.Worksheet(1);

            var rows = ws.RowsUsed().Skip(1); // pomijamy nagłówek

            foreach (var row in rows)
            {
                string ean = row.Cell(1).GetValue<string>()?.Trim() ?? "";
                string kraj = row.Cell(3).GetValue<string>()?.Trim() ?? "";


                if (!kraje.ContainsKey(ean))
                    kraje.Add(ean, kraj);
            }

            return kraje;
        }

    }
}