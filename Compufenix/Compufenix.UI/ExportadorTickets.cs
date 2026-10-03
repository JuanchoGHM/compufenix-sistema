using System;
using System.IO;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Compufenix.Models;
using Document = QuestPDF.Fluent.Document;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Compufenix.UI;

public static class ExportadorTickets
{
    public static void ExportarExcel(List<Ticket> tickets, string ruta)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Tickets");

        string[] encabezados = { "N.° Ticket", "Cliente", "Equipo", "Técnico", "Estado", "Fecha ingreso", "Costo total" };

        // Título de la empresa (fila 1)
        hoja.Range(1, 1, 1, encabezados.Length).Merge();
        hoja.Cell(1, 1).Value = "COMPUFENIX - Tickets de servicio";
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        hoja.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EA580C");
        hoja.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        hoja.Row(1).Height = 24;

        // Encabezados de columnas (fila 2)
        for (int i = 0; i < encabezados.Length; i++)
        {
            var celda = hoja.Cell(2, i + 1);
            celda.Value = encabezados[i];
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int fila = 3;
        foreach (var t in tickets)
        {
            hoja.Cell(fila, 1).Value = t.IdTicket;
            hoja.Cell(fila, 2).Value = t.Equipo?.Cliente?.Nombre ?? "";
            hoja.Cell(fila, 3).Value = t.Equipo?.Tipo ?? "";
            hoja.Cell(fila, 4).Value = t.Tecnico?.Nombre ?? "Sin asignar";
            hoja.Cell(fila, 5).Value = Textos.Mostrar(t.Estado);
            hoja.Cell(fila, 6).Value = t.FechaIngreso;
            hoja.Cell(fila, 6).Style.DateFormat.Format = "dd/MM/yyyy";
            hoja.Cell(fila, 7).Value = t.CostoTotal;
            hoja.Cell(fila, 7).Style.NumberFormat.Format = "\u20a1#,##0.00";

            if (fila % 2 == 0)
            {
                hoja.Range(fila, 1, fila, encabezados.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            }

            fila++;
        }

        // Fila de totales
        hoja.Range(fila, 1, fila, encabezados.Length - 1).Merge();
        hoja.Cell(fila, 1).Value = "Total";
        hoja.Cell(fila, 1).Style.Font.Bold = true;
        hoja.Cell(fila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        hoja.Cell(fila, encabezados.Length).Value = tickets.Sum(t => t.CostoTotal);
        hoja.Cell(fila, encabezados.Length).Style.NumberFormat.Format = "\u20a1#,##0.00";
        hoja.Cell(fila, encabezados.Length).Style.Font.Bold = true;

        var rangoDatos = hoja.Range(2, 1, fila, encabezados.Length);
        rangoDatos.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        rangoDatos.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        hoja.SheetView.FreezeRows(2);
        hoja.Columns().AdjustToContents();
        libro.SaveAs(ruta);
    }

    public static void ExportarPdf(List<Ticket> tickets, string ruta)
    {
        byte[]? logoBytes = CargarLogo();

        Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(30);

                pagina.Header().Row(fila =>
                {
                    if (logoBytes != null)
                    {
                        fila.ConstantColumn(45).Height(45).Image(logoBytes).FitArea();
                    }
                    fila.RelativeColumn().AlignMiddle().PaddingLeft(12).Column(columna =>
                    {
                        columna.Item().Text("COMPUFENIX").FontSize(18).Bold().FontColor("#EA580C");
                        columna.Item().Text("Tickets de servicio").FontSize(12).FontColor(Colors.Grey.Darken1);
                    });
                });

                pagina.Content().PaddingTop(15).Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.ConstantColumn(40);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(1.5f);
                        columnas.RelativeColumn(1.5f);
                    });

                    tabla.Header(encabezado =>
                    {
                        string[] titulos = { "N.°", "Cliente", "Equipo", "Técnico", "Estado", "Ingreso", "Costo" };
                        foreach (var titulo in titulos)
                        {
                            encabezado.Cell().Background("#1E3A8A").Padding(6)
                                .Text(titulo).FontColor(Colors.White).SemiBold();
                        }
                    });

                    int indice = 0;
                    foreach (var t in tickets)
                    {
                        var fondo = indice % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        tabla.Cell().Background(fondo).Padding(6).Text(t.IdTicket.ToString());
                        tabla.Cell().Background(fondo).Padding(6).Text(t.Equipo?.Cliente?.Nombre ?? "");
                        tabla.Cell().Background(fondo).Padding(6).Text(t.Equipo?.Tipo ?? "");
                        tabla.Cell().Background(fondo).Padding(6).Text(t.Tecnico?.Nombre ?? "Sin asignar");
                        tabla.Cell().Background(fondo).Padding(6).Text(Textos.Mostrar(t.Estado));
                        tabla.Cell().Background(fondo).Padding(6).Text(t.FechaIngreso.ToString("dd/MM/yyyy"));
                        tabla.Cell().Background(fondo).Padding(6).Text(t.CostoTotal.ToString("C"));

                        indice++;
                    }

                    tabla.Cell().ColumnSpan(6).Background("#F1F5F9").Padding(6)
                        .AlignRight().Text("Total:").SemiBold();
                    tabla.Cell().Background("#F1F5F9").Padding(6)
                        .Text(tickets.Sum(t => t.CostoTotal).ToString("C")).SemiBold();
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado el ");
                    texto.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        }).GeneratePdf(ruta);
    }

    private static byte[]? CargarLogo()
    {
        try
        {
            var recurso = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/logo.png"));
            if (recurso == null) return null;

            using var memoria = new MemoryStream();
            recurso.Stream.CopyTo(memoria);
            return memoria.ToArray();
        }
        catch
        {
            return null; // Si no se encuentra el logo, el PDF se genera igual, sin imagen
        }
    }
}