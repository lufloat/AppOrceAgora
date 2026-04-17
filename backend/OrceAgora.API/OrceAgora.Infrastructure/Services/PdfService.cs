using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OrceAgora.Application.DTOs.Budgets;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;

namespace OrceAgora.Infrastructure.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateBudgetPdf(BudgetDto budget, User professional)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var brand = professional.BrandColor ?? "#1A56DB";
        var culture = new System.Globalization.CultureInfo("pt-BR");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // ── HEADER ──────────────────────────────────────────────
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        // Logo + dados da empresa
                        row.RelativeItem().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(professional.LogoUrl) &&
                                professional.LogoUrl.StartsWith("data:image"))
                            {
                                try
                                {
                                    var base64 = professional.LogoUrl
                                        .Substring(professional.LogoUrl.IndexOf(',') + 1);
                                    var bytes = Convert.FromBase64String(base64);
                                    col.Item().Height(52).Image(bytes).FitArea();
                                    col.Item().PaddingTop(6);
                                }
                                catch { }
                            }

                            col.Item().Text(professional.CompanyName ?? professional.Name)
                                .FontSize(18).Bold().FontColor(brand);

                            if (!string.IsNullOrWhiteSpace(professional.Phone))
                                col.Item().PaddingTop(2)
                                    .Text($"Tel: {professional.Phone}")
                                    .FontSize(9).FontColor("#64748B");

                            if (!string.IsNullOrWhiteSpace(professional.Address))
                                col.Item().Text(professional.Address)
                                    .FontSize(9).FontColor("#64748B");
                        });

                        // Número e datas
                        row.ConstantItem(170).AlignRight().Column(col =>
                        {
                            col.Item().Text($"ORÇAMENTO #{budget.Number:D4}")
                                .FontSize(15).Bold().FontColor("#1E293B");
                            col.Item().PaddingTop(6)
                                .Text($"Emitido em {budget.CreatedAt:dd/MM/yyyy}")
                                .FontSize(9).FontColor("#64748B");
                            col.Item().Text($"Válido até {budget.ExpiresAt:dd/MM/yyyy}")
                                .FontSize(9).FontColor("#DC2626");
                        });
                    });

                    header.Item().PaddingTop(10)
                        .LineHorizontal(2).LineColor(brand);
                });

                // ── CONTENT ─────────────────────────────────────────────
                page.Content().PaddingVertical(16).Column(col =>
                {
                    // Bloco do cliente
                    if (budget.Client is not null)
                    {
                        col.Item().Border(1).BorderColor("#E2E8F0").Padding(12).Row(row =>
                        {
                            row.ConstantItem(4).Background(brand);
                            row.RelativeItem().PaddingLeft(10).Column(c =>
                            {
                                c.Item().Text("Cliente")
                                    .FontSize(8).Bold().FontColor("#94A3B8");
                                c.Item().PaddingTop(3)
                                    .Text(budget.Client.Name)
                                    .FontSize(13).Bold().FontColor("#1E293B");
                                if (!string.IsNullOrWhiteSpace(budget.Client.Phone))
                                    c.Item().PaddingTop(2)
                                        .Text($"Tel: {budget.Client.Phone}")
                                        .FontSize(9).FontColor("#64748B");
                                if (!string.IsNullOrWhiteSpace(budget.Client.Email))
                                    c.Item().Text(budget.Client.Email)
                                        .FontSize(9).FontColor("#64748B");
                                if (!string.IsNullOrWhiteSpace(budget.Client.Address))
                                    c.Item().Text(budget.Client.Address)
                                        .FontSize(9).FontColor("#64748B");
                            });
                        });

                        col.Item().PaddingTop(16);
                    }

                    // Tabela de itens
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(5);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(brand).Padding(8)
                                .Text("Descrição").FontColor("#FFFFFF").Bold().FontSize(9);
                            h.Cell().Background(brand).Padding(8).AlignRight()
                                .Text("Qtd").FontColor("#FFFFFF").Bold().FontSize(9);
                            h.Cell().Background(brand).Padding(8).AlignRight()
                                .Text("Valor unit.").FontColor("#FFFFFF").Bold().FontSize(9);
                            h.Cell().Background(brand).Padding(8).AlignRight()
                                .Text("Total").FontColor("#FFFFFF").Bold().FontSize(9);
                        });

                        var regularItems = budget.Items.Where(i => !i.IsLabor).ToList();
                        var laborItems = budget.Items.Where(i => i.IsLabor).ToList();

                        int idx = 0;

                        void RenderRow(BudgetItemDto item)
                        {
                            var bg = idx % 2 == 0 ? "#FFFFFF" : "#F8FAFC";
                            idx++;

                            table.Cell().Background(bg).Padding(8).Column(c =>
                            {
                                c.Item().Text(item.Name).FontSize(10);
                                if (!string.IsNullOrWhiteSpace(item.LaborType))
                                {
                                    var tipo = item.LaborType switch
                                    {
                                        "hour" => "por hora",
                                        "day" => "por dia",
                                        _ => "valor total"
                                    };
                                    c.Item().Text($"Mão de obra · {tipo}")
                                        .FontSize(8).FontColor("#94A3B8");
                                }
                            });
                            table.Cell().Background(bg).Padding(8).AlignRight()
                                .Text(item.Qty.ToString("G")).FontSize(10);
                            table.Cell().Background(bg).Padding(8).AlignRight()
                                .Text(item.UnitPrice.ToString("C2", culture)).FontSize(10);
                            table.Cell().Background(bg).Padding(8).AlignRight()
                                .Text(item.Total.ToString("C2", culture)).FontSize(10);
                        }

                        foreach (var item in regularItems) RenderRow(item);

                        if (laborItems.Any())
                        {
                            table.Cell().ColumnSpan(4)
                                .Background("#F1F5F9").Padding(6)
                                .Text("Mão de obra")
                                .FontSize(9).Bold().FontColor("#475569");

                            foreach (var item in laborItems) RenderRow(item);
                        }
                    });

                    col.Item().PaddingTop(12);

                    // Totais
                    col.Item().AlignRight().Width(240).Column(totals =>
                    {
                        void TotalsRow(string label, string value,
                            bool highlight = false, bool isRed = false)
                        {
                            totals.Item().PaddingVertical(3).Row(r =>
                            {
                                var labelText = r.RelativeItem().Text(label)
                                    .FontSize(highlight ? 11 : 9)
                                    .FontColor(highlight ? brand
                                        : isRed ? "#DC2626" : "#64748B");
                                if (highlight) labelText.Bold();

                                var valueText = r.ConstantItem(100).AlignRight().Text(value)
                                    .FontSize(highlight ? 11 : 9)
                                    .FontColor(highlight ? brand
                                        : isRed ? "#DC2626" : "#1E293B");
                                if (highlight) valueText.Bold();
                            });
                        }

                        TotalsRow("Subtotal", budget.Subtotal.ToString("C2", culture));

                        if (budget.DiscountAmount > 0)
                        {
                            var label = budget.DiscountType == "percent"
                                ? $"Desconto ({budget.DiscountValue}%)"
                                : "Desconto (R$)";
                            TotalsRow(label,
                                $"- {budget.DiscountAmount.ToString("C2", culture)}",
                                isRed: true);
                        }

                        if (budget.Extras > 0)
                            TotalsRow(
                                budget.ExtrasDescription ?? "Extras",
                                budget.Extras.ToString("C2", culture));

                        totals.Item().PaddingTop(4).PaddingBottom(4)
                            .LineHorizontal(1).LineColor("#E2E8F0");

                        TotalsRow("TOTAL",
                            budget.Total.ToString("C2", culture), highlight: true);
                    });

                    // Observações
                    if (!string.IsNullOrWhiteSpace(budget.Notes))
                    {
                        col.Item().PaddingTop(20)
                            .BorderLeft(3).BorderColor(brand)
                            .PaddingLeft(10).Column(obs =>
                            {
                                obs.Item().Text("Observações")
                                    .FontSize(9).Bold().FontColor("#475569");
                                obs.Item().PaddingTop(4).Text(budget.Notes)
                                    .FontSize(9).FontColor("#475569");
                            });
                    }

                    // Formas de pagamento
                    if (!string.IsNullOrWhiteSpace(budget.PaymentMethods))
                    {
                        col.Item().PaddingTop(12)
                            .BorderLeft(3).BorderColor("#CBD5E1")
                            .PaddingLeft(10).Column(pay =>
                            {
                                pay.Item().Text("Formas de pagamento")
                                    .FontSize(9).Bold().FontColor("#475569");
                                pay.Item().PaddingTop(4).Text(budget.PaymentMethods)
                                    .FontSize(9).FontColor("#475569");
                            });
                    }
                });

                // ── FOOTER ──────────────────────────────────────────────
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor("#E2E8F0");
                    footer.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem()
// Atualize a linha do footer:
                        .Text("Gerado por StimServ · stimserv.com.br")          
                            .FontSize(8).FontColor("#94A3B8");
                        row.RelativeItem().AlignRight()
                            .Text($"Válido por {budget.ValidityDays} dias")
                            .FontSize(8).FontColor("#94A3B8");
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}