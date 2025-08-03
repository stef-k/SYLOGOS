using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SYLOGOS.Models;

namespace SYLOGOS.Util
{
    /// <summary>
    /// Generates a fixed‑size (10 × 6 cm) member card PDF.
    /// Front and back faces are placed side‑by‑side with a slim 3–5 mm spacer so
    /// the sheet can be printed, folded, and glued to form a single two‑sided card.
    /// </summary>
    public class MemberCardDocument : IDocument
    {
        private readonly Member _member;
        private readonly AppSetting _settings;

        public MemberCardDocument(Member member, AppSetting settings)
        {
            _member = member;
            _settings = settings;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            // 10 cm × 6 cm in points (1 mm ≈ 2.835 pt)
            const float CardWidth = 283f;  // 100 mm
            const float CardHeight = 170f;  //  60 mm
            const float Spacer = 11f;   //  ≈ 3.9 mm gap between faces
            const float PageMargin = 20f;   // frame around entire layout
            const float OuterPad = 5f;    // halo around the two‑card row
            const float CardPad = 12f;   // internal padding inside each card

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(PageMargin);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(12));

                page.Content()
                    .Padding(OuterPad)
                    .Row(row =>
                    {
                        row.ConstantItem(CardWidth).Element(c => DrawFront(c, CardPad, CardHeight));
                        row.ConstantItem(Spacer); // fold‑line gap
                        row.ConstantItem(CardWidth).Element(c => DrawBack(c, CardPad, CardHeight));
                    });
            });
        }

        // ───────────────────────── Front face ─────────────────────────
        private void DrawFront(IContainer container, float pad, float height)
        {
            container
                .Border(1)
                .Padding(pad)
                .MinHeight(height)
                .Column(col =>
                {
                    col.Spacing(2);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            if (_settings.ClubLogo != null)
                            {
                                left.Item().AlignCenter().Height(80).PaddingBottom(10).Image(_settings.ClubLogo, ImageScaling.FitHeight);
                            }

                            left.Item().AlignCenter()
                                .Text(_settings.ClubName ?? string.Empty)
                                .Bold().FontSize(12);
                        });
                    });

                    col.Item().AlignCenter().Text("ΚΑΡΤΑ ΜΕΛΟΥΣ").Bold().FontSize(14);
                    col.Item().PaddingBottom(10).AlignCenter().Text($"Αριθμός Μέλους: {_member.MemberNumber}").Bold().FontSize(14);

                    col.Item().AlignCenter().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(10));
                        if (!string.IsNullOrWhiteSpace(_settings.Address))
                        {
                            txt.Line(_settings.Address);
                        }

                        if (!string.IsNullOrWhiteSpace(_settings.Phone))
                        {
                            txt.Line(_settings.Phone);
                        }

                        if (!string.IsNullOrWhiteSpace(_settings.Email))
                        {
                            txt.Line(_settings.Email);
                        }
                    });
                });
        }

        // ───────────────────────── Back face ──────────────────────────
        private void DrawBack(IContainer container, float pad, float height)
        {
            container
                .Border(1)
                .Padding(pad)
                .MinHeight(height)
                .Column(col =>
                {
                    void Label(string text)
                    {
                        col.Item().PaddingTop(4).Text(text).Bold();
                    }

                    col.Item().Text("Ονοματεπώνυμο:").Bold();
                    col.Item().Text(_member.FullName);

                    if (!string.IsNullOrWhiteSpace(_member.SpouseFullName))
                    {
                        Label("Σύζυγος:");
                        col.Item().Text(_member.SpouseFullName);
                    }

                    if (!string.IsNullOrWhiteSpace(_member.Email))
                    {
                        Label("Email:");
                        col.Item().Text(_member.Email);
                    }

                    if (!string.IsNullOrWhiteSpace(_member.MemberPhone))
                    {
                        Label("Τηλέφωνο:");
                        col.Item().Text(_member.MemberPhone);
                    }

                    if (!string.IsNullOrWhiteSpace(_member.SpousePhone))
                    {
                        Label("Τηλ. Συζύγου:");
                        col.Item().Text(_member.SpousePhone);
                    }
                });
        }
    }
}
