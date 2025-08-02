using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SYLOGOS.Models;
using System.Globalization;

namespace SYLOGOS.Util
{
    public class ReceiptDocument : IDocument
    {
        private readonly Member _member;
        private readonly Membership _membership;
        private readonly AppSetting _settings;

        public ReceiptDocument(Member member, Membership membership, AppSetting settings)
        {
            _member = member;
            _membership = membership;
            _settings = settings;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            CultureInfo elGR = new("el-GR");

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(12));

                page.Content().Height(421).Column(col =>
                {
                    col.Spacing(8);

                    // ─────────────── Header: Logo + Club Info + Receipt Info ───────────────
                    col.Item().Row(row =>
                    {
                        row.Spacing(10);

                        row.RelativeItem(2).Column(left =>
                        {
                            if (_settings.ClubLogo != null)
                            {
                                left.Item().Height(40).Image(_settings.ClubLogo, ImageScaling.FitHeight);
                            }

                            left.Item().Text(_settings.ClubName ?? "").Bold();
                            if (!string.IsNullOrWhiteSpace(_settings.Address))
                            {
                                left.Item().Text(_settings.Address);
                            }

                            if (!string.IsNullOrWhiteSpace(_settings.Email))
                            {
                                left.Item().Text(_settings.Email);
                            }

                            if (!string.IsNullOrWhiteSpace(_settings.Website))
                            {
                                left.Item().Text(_settings.Website);
                            }

                            if (!string.IsNullOrWhiteSpace(_settings.Phone))
                            {
                                left.Item().Text(_settings.Phone);
                            }
                        });

                        row.RelativeItem(1).Column(right =>
                        {
                            right.Item().AlignRight().Text($"Ημερομηνία: {DateTime.Now:dd/MM/yyyy}");
                            right.Item().AlignRight().Text(txt =>
                            {
                                txt.Span("Αριθμ. Απόδειξης").SemiBold();
                                txt.Span($" {_membership.ReceiptYear} / {_membership.ReceiptNumber}").Bold();
                            });
                        });
                    });

                    // ─────────────── Title and Amount ───────────────
                    col.Item().AlignCenter().Text("ΑΠΟΔΕΙΞΗ ΕΙΣΠΡΑΞΗΣ").Bold().FontSize(14);

                    string amount = _membership.Amount.ToString("C2", elGR);
                    col.Item().AlignCenter().Text($"ΕΥΡΩ: {amount}").Bold();

                    // ─────────────── Recipient Info ───────────────
                    string amountWords = ExportHelper.ToGreekAmountText(_membership.Amount);
                    col.Item().Text(txt =>
                    {
                        txt.Span("Βεβαιώνεται η λήψη του ποσού των ");
                        txt.Span($"{amountWords} ({amount})").Bold();
                        txt.Span(", από τον/την ");
                        txt.Span(_member.FullName);
                        txt.Span(", ");
                        txt.Span(_member.Address ?? "");
                        txt.Span(", ");
                        txt.Span(_member.City ?? "");
                        txt.Span(".");
                    });

                    col.Item().Text(txt =>
                    {
                        txt.Span("Αιτιολογία είσπραξης: Η συνδρομή ως μέλος στο σύλλογο ");
                        txt.Span(_settings.ClubName ?? "").Bold();
                        txt.Span(", για το έτος ");
                        txt.Span(_membership.ReceiptYear?.ToString() ?? "").Bold();
                    });

                    // ─────────────── Signature Row ───────────────
                    col.Item().PaddingTop(20).Row(sig =>
                    {
                        sig.RelativeItem().AlignCenter().Text("ΓΙΑ ΤΗΝ ΕΙΣΠΡΑΞΗ").Bold();
                        sig.RelativeItem().AlignCenter().Text("ΓΙΑ ΤΗΝ ΠΛΗΡΩΜΗ").Bold();
                    });
                });
            });
        }

    }
}
