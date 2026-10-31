using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.External
{
    public interface IPdfGenerator
    {
        byte[] GenerateQuotationPdf(Quotation quotation, Client client);
    }

    /// <summary>
    /// Branded quotation PDF built with QuestPDF (community licence).
    /// </summary>
    public class PdfGenerator : IPdfGenerator
    {
        private const string BrandNavy = "#0A1F44";
        private const string BrandCyan = "#00B4D8";

        public byte[] GenerateQuotationPdf(Quotation quotation, Client client)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Element(header => BuildHeader(header, quotation));
                    page.Content().Element(content => BuildContent(content, quotation, client));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Verichat Quality Projects - Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private static void BuildHeader(IContainer container, Quotation quotation)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("VERICHAT").FontSize(24).Bold().FontColor(BrandNavy);
                    col.Item().Text("MEDICAL PROCUREMENT").FontSize(8).FontColor(BrandCyan);
                });

                row.ConstantItem(190).AlignRight().Column(col =>
                {
                    col.Item().Text("QUOTATION").FontSize(12).Bold().FontColor(BrandNavy);
                    col.Item().Text(quotation.QuoteReference).FontSize(10);
                    col.Item().Text(quotation.DateGenerated.ToString("dd MMM yyyy"));
                });
            });
        }

        private static void BuildContent(IContainer container, Quotation quotation, Client client)
        {
            container.PaddingVertical(15).Column(col =>
            {
                col.Item().Row(infoRow =>
                {
                    infoRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text("BILL TO").FontSize(8).SemiBold().FontColor(BrandCyan);
                        c.Item().Text(client.ClientName).FontSize(11).Bold();
                        c.Item().Text(client.Address ?? "");
                        c.Item().Text(client.ContactEmail ?? "");
                        c.Item().Text(client.ContactPhone ?? "");
                    });

                    infoRow.ConstantItem(180).Column(c =>
                    {
                        c.Item().AlignRight().Text("VALID UNTIL").FontSize(8).SemiBold().FontColor(BrandCyan);
                        c.Item().AlignRight().Text(quotation.ValidUntil.ToString("dd MMM yyyy")).FontSize(10);
                    });
                });

                col.Item().PaddingTop(15);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(95);
                        columns.ConstantColumn(95);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(BrandNavy).Padding(6).Text("#").FontColor(Colors.White).Bold();
                        header.Cell().Background(BrandNavy).Padding(6).Text("DESCRIPTION").FontColor(Colors.White).Bold();
                        header.Cell().Background(BrandNavy).Padding(6).Text("SKU").FontColor(Colors.White).Bold();
                        header.Cell().Background(BrandNavy).Padding(6).Text("QTY").FontColor(Colors.White).Bold();
                        header.Cell().Background(BrandNavy).Padding(6).AlignRight().Text("UNIT PRICE").FontColor(Colors.White).Bold();
                        header.Cell().Background(BrandNavy).Padding(6).AlignRight().Text("TOTAL").FontColor(Colors.White).Bold();
                    });

                    var index = 1;
                    foreach (var line in quotation.LineItems)
                    {
                        var position = index++;
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(position.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(line.ProductName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(line.SKU).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(line.Quantity.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(Format(line.UnitPrice));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(Format(line.LineTotal));
                    }
                });

                col.Item().PaddingTop(20).AlignRight().Width(250).Column(totals =>
                {
                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal");
                        r.ConstantItem(120).AlignRight().Text(Format(quotation.Subtotal));
                    });
                    totals.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text("VAT (" + (quotation.VatRate * 100).ToString("0") + "%)");
                        r.ConstantItem(120).AlignRight().Text(Format(quotation.VatAmount));
                    });
                    totals.Item().PaddingTop(8).BorderTop(2).BorderColor(BrandNavy).PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL DUE").Bold().FontSize(12).FontColor(BrandNavy);
                        r.ConstantItem(120).AlignRight().Text(Format(quotation.GrandTotal)).Bold().FontSize(12).FontColor(BrandNavy);
                    });
                });

                col.Item().PaddingTop(25).Column(terms =>
                {
                    terms.Item().Text("TERMS & CONDITIONS").FontSize(8).SemiBold().FontColor(BrandCyan);
                    terms.Item().PaddingTop(4).Text(quotation.Terms).FontSize(9).Italic();
                });
            });
        }

        private static string Format(decimal value) => "R " + value.ToString("N2");
    }
}
