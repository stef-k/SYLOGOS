using ClosedXML.Excel;

namespace SYLOGOS.Util
{
    public static class ExportService
    {
        public static void ExportToExcel<T>(
            IEnumerable<T> data,
            string title,
            string filePath,
            Dictionary<string, Func<T, object?>> columns)
        {
            using XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet sheet = workbook.Worksheets.Add("ΕΞΑΓΩΓΗ");

            int colIndex = 1;
            foreach (string header in columns.Keys)
            {
                sheet.Cell(1, colIndex).Value = header;
                sheet.Cell(1, colIndex).Style.Font.SetBold();
                colIndex++;
            }

            int rowIndex = 2;
            foreach (T? item in data)
            {
                colIndex = 1;
                foreach (Func<T, object?> extractor in columns.Values)
                {
                    object? value = extractor(item);
                    sheet.Cell(rowIndex, colIndex).Value = value switch
                    {
                        null => "",
                        DateTime dt => dt,
                        int i => i,
                        decimal d => d,
                        double dbl => dbl,
                        _ => value.ToString() ?? ""
                    };
                    colIndex++;
                }
                rowIndex++;
            }

            sheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

    }
}
