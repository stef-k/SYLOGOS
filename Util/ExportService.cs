using ClosedXML.Excel;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SYLOGOS.Models;

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
            IXLWorksheet sheet = workbook.Worksheets.Add("Export");

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

        public static void ExportMembershipReceipt(Member member, Membership membership, string filePath)
        {
            ReceiptDocument document = new ReceiptDocument(member, membership);
            document.GeneratePdf(filePath);
        }

        private class ReceiptDocument : IDocument
        {
            private readonly Member _member;
            private readonly Membership _membership;

            public ReceiptDocument(Member member, Membership membership)
            {
                _member = member;
                _membership = membership;
            }

            public DocumentMetadata GetMetadata()
            {
                return DocumentMetadata.Default;
            }

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Element(header =>
                    {
                        header.AlignCenter().Text("SYLOGOS Membership Receipt").Bold().FontSize(16);
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                        col.Item().Text($"Member #: {_member.MemberNumber}");
                        col.Item().Text($"Name: {_member.FullName}");
                        col.Item().Text($"City: {_member.City}");
                        col.Item().Text($"Year: {_membership.Year}");
                        col.Item().Text($"Amount: {_membership.Amount:C2}");
                    });

                    page.Footer().Element(footer =>
                    {
                        footer.AlignCenter().Text("Thank you for your support.");
                    });
                });
            }
        }
    }
}
