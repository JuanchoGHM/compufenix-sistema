using System;
using System.IO;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Compufenix.Business;
using Compufenix.Models;
using Document = QuestPDF.Fluent.Document;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Compufenix.UI;

// Exporta UN ticket (con su cliente, equipo, repuestos, historial y costos) a PDF o Excel
public static class ExportadorTicket
{
    private const string Naranja = "#EA580C";
    private const string Azul = "#1E3A8A";
    private const string Gris = "#64748B";
    private const string FondoSuave = "#F8FAFC";
    private const string Borde = "#E2E8F0";
    private const string FormatoMoneda = "\u20a1#,##0.00";

    // Todo lo que necesitan los dos formatos, leído una sola vez de la base de datos
    private sealed class DatosTicket
    {
        public Ticket Ticket { get; init; } = null!;
        public List<MovimientoInventario> Repuestos { get; init; } = new();
        public List<HistorialEstadoTicket> Historial { get; init; } = new();
    }

    private static DatosTicket Cargar(int idTicket)
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioTickets(db);

        return new DatosTicket
        {
            Ticket = servicio.ObtenerPorId(idTicket),
            Repuestos = servicio.ObtenerRepuestosUsados(idTicket).OrderBy(m => m.Fecha).ToList(),
            Historial = servicio.ObtenerHistorial(idTicket)
        };
    }

    // ---------- Textos que se repiten en los dos formatos ----------

    private static string TextoEquipo(Equipo? e) =>
        e == null ? "" : string.Join(" ", new[] { e.Tipo, e.Marca, e.Modelo }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string Valor(string? texto) => string.IsNullOrWhiteSpace(texto) ? "—" : texto.Trim();

    private static (string Fondo, string Texto) ColoresEstado(EstadoTicket estado) => estado switch
    {
        EstadoTicket.Cancelado => ("#FEE2E2", "#991B1B"),
        EstadoTicket.Entregado => ("#DCFCE7", "#166534"),
        EstadoTicket.Reparado => ("#DCFCE7", "#166534"),
        EstadoTicket.EsperandoRepuesto => ("#FEF3C7", "#92400E"),
        _ => ("#DBEAFE", "#1E40AF")
    };

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
            return null; // Sin logo, el archivo se genera igual
        }
    }

    // =====================================================================
    //  PDF
    // =====================================================================

    public static void ExportarPdf(int idTicket, string ruta)
    {
        var datos = Cargar(idTicket);
        var t = datos.Ticket;
        var cliente = t.Equipo?.Cliente;
        var equipo = t.Equipo;
        byte[]? logo = CargarLogo();

        var (fondoEstado, textoEstado) = ColoresEstado(t.Estado);
        decimal costoRepuestos = t.CostoTotal - t.CostoManoObra;

        Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(36);
                pagina.DefaultTextStyle(x => x.FontSize(10).FontColor("#0F172A"));

                // ----- Encabezado -----
                pagina.Header().Column(encabezado =>
                {
                    encabezado.Item().Row(fila =>
                    {
                        if (logo != null)
                        {
                            fila.ConstantColumn(50).Height(50).Image(logo).FitArea();
                        }

                        fila.RelativeColumn().PaddingLeft(12).AlignMiddle().Column(titulo =>
                        {
                            titulo.Item().Text("COMPUFENIX").FontSize(20).Bold().FontColor(Naranja);
                            titulo.Item().Text("Ticket de servicio técnico").FontSize(10).FontColor(Gris);
                        });

                        fila.ConstantColumn(170).AlignMiddle().Column(numero =>
                        {
                            numero.Item().AlignRight().Text($"Ticket #{t.IdTicket}").FontSize(18).Bold().FontColor(Azul);
                            numero.Item().AlignRight().Text($"Ingreso: {t.FechaIngreso:dd/MM/yyyy}").FontSize(9).FontColor(Gris);
                        });
                    });

                    encabezado.Item().PaddingTop(10).LineHorizontal(2).LineColor(Naranja);
                });

                // ----- Contenido -----
                pagina.Content().PaddingTop(16).Column(col =>
                {
                    col.Spacing(14);

                    // Estado
                    col.Item().Background(fondoEstado).PaddingVertical(9).PaddingHorizontal(14).Row(fila =>
                    {
                        fila.RelativeColumn().AlignMiddle()
                            .Text("ESTADO DEL TICKET").FontSize(8).Bold().FontColor(textoEstado);
                        fila.ConstantColumn(220).AlignRight()
                            .Text(Textos.Mostrar(t.Estado)).FontSize(13).Bold().FontColor(textoEstado);
                    });

                    // Cliente y equipo, lado a lado
                    col.Item().Row(fila =>
                    {
                        fila.RelativeColumn().PaddingRight(7).Element(c => Tarjeta(c, "Cliente", new[]
                        {
                            ("Nombre", Valor(cliente?.Nombre)),
                            ("Teléfono", Valor(cliente?.Telefono)),
                            ("Correo", Valor(cliente?.Correo)),
                            ("Dirección", Valor(cliente?.Direccion))
                        }));

                        fila.RelativeColumn().PaddingLeft(7).Element(c => Tarjeta(c, "Equipo", new[]
                        {
                            ("Tipo", Valor(equipo?.Tipo)),
                            ("Marca y modelo", Valor(string.Join(" ", new[] { equipo?.Marca, equipo?.Modelo }
                                .Where(x => !string.IsNullOrWhiteSpace(x))))),
                            ("Número de serie", Valor(equipo?.NumeroSerie)),
                            ("Técnico asignado", t.Tecnico?.Nombre ?? "Sin asignar")
                        }));
                    });

                    // Diagnóstico
                    col.Item().Column(bloque =>
                    {
                        bloque.Item().Element(c => TituloSeccion(c, "Diagnóstico"));
                        bloque.Item().Background(FondoSuave).Border(1).BorderColor(Borde).Padding(12)
                            .Text(string.IsNullOrWhiteSpace(t.Diagnostico) ? "Sin diagnóstico registrado." : t.Diagnostico.Trim());
                    });

                    // Repuestos utilizados
                    col.Item().Column(bloque =>
                    {
                        bloque.Item().Element(c => TituloSeccion(c, "Repuestos utilizados"));

                        if (datos.Repuestos.Count == 0)
                        {
                            bloque.Item().Text("No se usaron repuestos en este ticket.").FontColor(Gris);
                            return;
                        }

                        bloque.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.ConstantColumn(45);
                                columnas.RelativeColumn(3.2f);
                                columnas.RelativeColumn(1.4f);
                                columnas.RelativeColumn(1.4f);
                                columnas.RelativeColumn(1.4f);
                            });

                            tabla.Header(encabezado =>
                            {
                                string[] titulos = { "Cant.", "Producto", "Fecha", "Precio unit.", "Subtotal" };
                                foreach (var titulo in titulos)
                                {
                                    encabezado.Cell().Background(Azul).Padding(6)
                                        .Text(titulo).FontColor(Colors.White).SemiBold().FontSize(9);
                                }
                            });

                            int indice = 0;
                            foreach (var m in datos.Repuestos)
                            {
                                var fondo = indice % 2 == 0 ? "#FFFFFF" : "#F1F5F9";
                                decimal precio = m.Producto?.PrecioUnitario ?? 0;

                                tabla.Cell().Background(fondo).Padding(6).Text(m.Cantidad.ToString());
                                tabla.Cell().Background(fondo).Padding(6).Text(m.Producto?.Nombre ?? "");
                                tabla.Cell().Background(fondo).Padding(6).Text(m.Fecha.ToString("dd/MM/yyyy"));
                                tabla.Cell().Background(fondo).Padding(6).Text(precio.ToString("C"));
                                tabla.Cell().Background(fondo).Padding(6).Text((precio * m.Cantidad).ToString("C")).SemiBold();
                                indice++;
                            }
                        });
                    });

                    // Historial de estados
                    col.Item().Column(bloque =>
                    {
                        bloque.Item().Element(c => TituloSeccion(c, "Historial de estados"));

                        bloque.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2.4f);
                            });

                            tabla.Header(encabezado =>
                            {
                                string[] titulos = { "Estado", "Fecha y hora", "Registrado por" };
                                foreach (var titulo in titulos)
                                {
                                    encabezado.Cell().Background(Azul).Padding(6)
                                        .Text(titulo).FontColor(Colors.White).SemiBold().FontSize(9);
                                }
                            });

                            int indice = 0;
                            foreach (var h in datos.Historial)
                            {
                                var fondo = indice % 2 == 0 ? "#FFFFFF" : "#F1F5F9";
                                var (fondoH, textoH) = ColoresEstado(h.Estado);

                                tabla.Cell().Background(fondo).Padding(4).AlignLeft().Element(c =>
                                    c.Background(fondoH).PaddingVertical(2).PaddingHorizontal(8)
                                     .Text(Textos.Mostrar(h.Estado)).FontSize(9).SemiBold().FontColor(textoH));
                                tabla.Cell().Background(fondo).Padding(6).Text(h.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                tabla.Cell().Background(fondo).Padding(6).Text(h.Usuario?.Nombre ?? "—");
                                indice++;
                            }
                        });
                    });

                    // Resumen de costos (alineado a la derecha)
                    col.Item().AlignRight().Width(270).Column(costos =>
                    {
                        costos.Item().Element(c => FilaCosto(c, "Repuestos", costoRepuestos.ToString("C")));
                        costos.Item().Element(c => FilaCosto(c, "Mano de obra", t.CostoManoObra.ToString("C")));
                        costos.Item().PaddingTop(4).Background(Naranja).Padding(10).Row(fila =>
                        {
                            fila.RelativeColumn().AlignMiddle()
                                .Text("COSTO TOTAL").Bold().FontSize(10).FontColor(Colors.White);
                            fila.ConstantColumn(120).AlignRight()
                                .Text(t.CostoTotal.ToString("C")).Bold().FontSize(15).FontColor(Colors.White);
                        });
                    });
                });

                // ----- Pie de página -----
                pagina.Footer().PaddingTop(8).BorderTop(1).BorderColor(Borde)
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Gris))
                    .Row(fila =>
                    {
                        fila.RelativeColumn().Text($"COMPUFENIX · Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                        fila.ConstantColumn(100).AlignRight().Text(texto =>
                        {
                            texto.Span("Página ");
                            texto.CurrentPageNumber();
                            texto.Span(" de ");
                            texto.TotalPages();
                        });
                    });
            });
        }).GeneratePdf(ruta);
    }

    private static void TituloSeccion(IContainer contenedor, string titulo)
    {
        contenedor.PaddingBottom(6).Text(titulo.ToUpper()).FontSize(10).Bold().FontColor(Azul);
    }

    private static void Tarjeta(IContainer contenedor, string titulo, (string Etiqueta, string Valor)[] campos)
    {
        contenedor.Background(FondoSuave).Border(1).BorderColor(Borde).Padding(12).Column(col =>
        {
            col.Item().PaddingBottom(8).Text(titulo.ToUpper()).FontSize(9).Bold().FontColor(Naranja);

            foreach (var (etiqueta, valor) in campos)
            {
                col.Item().PaddingBottom(6).Column(campo =>
                {
                    campo.Item().Text(etiqueta).FontSize(8).FontColor(Gris);
                    campo.Item().Text(valor).FontSize(10.5f).SemiBold();
                });
            }
        });
    }

    private static void FilaCosto(IContainer contenedor, string etiqueta, string monto)
    {
        contenedor.PaddingVertical(4).PaddingHorizontal(10).BorderBottom(1).BorderColor(Borde).Row(fila =>
        {
            fila.RelativeColumn().Text(etiqueta).FontColor(Gris);
            fila.ConstantColumn(120).AlignRight().Text(monto).SemiBold();
        });
    }

    // =====================================================================
    //  EXCEL
    // =====================================================================

    public static void ExportarExcel(int idTicket, string ruta)
    {
        var datos = Cargar(idTicket);
        var t = datos.Ticket;
        var cliente = t.Equipo?.Cliente;
        var equipo = t.Equipo;
        decimal costoRepuestos = t.CostoTotal - t.CostoManoObra;

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add($"Ticket {t.IdTicket}");
        hoja.ShowGridLines = false;
        hoja.Style.Font.FontName = "Calibri";
        hoja.Style.Font.FontSize = 11;

        // Columnas: A y G son márgenes; B..F llevan el contenido
        hoja.Column(1).Width = 2;
        hoja.Column(2).Width = 18;
        hoja.Column(3).Width = 30;
        hoja.Column(4).Width = 18;
        hoja.Column(5).Width = 20;
        hoja.Column(6).Width = 20;
        hoja.Column(7).Width = 2;

        // ----- Banner -----
        hoja.Row(1).Height = 8;
        hoja.Row(2).Height = 38;
        hoja.Row(3).Height = 24;

        var logoCelda = hoja.Range("B2:B3").Merge();
        logoCelda.Style.Fill.BackgroundColor = XLColor.White;

        var titulo = hoja.Range("C2:F2").Merge();
        hoja.Cell("C2").Value = "COMPUFENIX";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 22;
        titulo.Style.Font.FontColor = XLColor.White;
        titulo.Style.Fill.BackgroundColor = XLColor.FromHtml(Naranja);
        titulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        titulo.Style.Alignment.Indent = 1;

        var subtitulo = hoja.Range("C3:F3").Merge();
        hoja.Cell("C3").Value = $"Ticket de servicio técnico  ·  N.° {t.IdTicket}";
        subtitulo.Style.Font.FontSize = 12;
        subtitulo.Style.Font.FontColor = XLColor.White;
        subtitulo.Style.Fill.BackgroundColor = XLColor.FromHtml(Azul);
        subtitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        subtitulo.Style.Alignment.Indent = 1;

        var logo = CargarLogo();
        if (logo != null)
        {
            try
            {
                var flujo = new MemoryStream(logo);
                hoja.AddPicture(flujo).WithSize(50, 50).MoveTo(hoja.Cell("B2"), 8, 4);
            }
            catch
            {
                // Si el logo falla, el archivo se genera igual sin imagen
            }
        }

        int fila = 5;

        // ----- Datos del ticket -----
        fila = Seccion(hoja, fila, "Datos del ticket");

        var (fondoEstado, textoEstado) = ColoresEstado(t.Estado);
        Par(hoja, fila, 2, 3, 3, "Estado", Textos.Mostrar(t.Estado));
        hoja.Cell(fila, 3).Style.Fill.BackgroundColor = XLColor.FromHtml(fondoEstado);
        hoja.Cell(fila, 3).Style.Font.FontColor = XLColor.FromHtml(textoEstado);
        hoja.Cell(fila, 3).Style.Font.Bold = true;
        Par(hoja, fila, 4, 5, 6, "Fecha de ingreso", t.FechaIngreso.ToString("dd/MM/yyyy"));
        fila++;

        Par(hoja, fila, 2, 3, 3, "Técnico", t.Tecnico?.Nombre ?? "Sin asignar");
        Par(hoja, fila, 4, 5, 6, "N.° de ticket", t.IdTicket.ToString());
        fila += 2;

        // ----- Cliente -----
        fila = Seccion(hoja, fila, "Cliente");
        Par(hoja, fila, 2, 3, 3, "Nombre", Valor(cliente?.Nombre));
        Par(hoja, fila, 4, 5, 6, "Teléfono", Valor(cliente?.Telefono));
        fila++;
        Par(hoja, fila, 2, 3, 3, "Correo", Valor(cliente?.Correo));
        Par(hoja, fila, 4, 5, 6, "Dirección", Valor(cliente?.Direccion));
        fila += 2;

        // ----- Equipo -----
        fila = Seccion(hoja, fila, "Equipo");
        Par(hoja, fila, 2, 3, 3, "Tipo", Valor(equipo?.Tipo));
        Par(hoja, fila, 4, 5, 6, "Marca", Valor(equipo?.Marca));
        fila++;
        Par(hoja, fila, 2, 3, 3, "Modelo", Valor(equipo?.Modelo));
        Par(hoja, fila, 4, 5, 6, "N.° de serie", Valor(equipo?.NumeroSerie));
        fila += 2;

        // ----- Diagnóstico -----
        fila = Seccion(hoja, fila, "Diagnóstico");
        string diagnostico = string.IsNullOrWhiteSpace(t.Diagnostico) ? "Sin diagnóstico registrado." : t.Diagnostico.Trim();
        var cajaDiagnostico = hoja.Range(fila, 2, fila, 6).Merge();
        hoja.Cell(fila, 2).Value = diagnostico;
        cajaDiagnostico.Style.Alignment.WrapText = true;
        cajaDiagnostico.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        cajaDiagnostico.Style.Alignment.Indent = 1;
        cajaDiagnostico.Style.Fill.BackgroundColor = XLColor.FromHtml(FondoSuave);
        cajaDiagnostico.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cajaDiagnostico.Style.Border.OutsideBorderColor = XLColor.FromHtml(Borde);
        int lineas = diagnostico.Split('\n').Sum(l => (int)Math.Ceiling(Math.Max(1, l.Length) / 95.0));
        hoja.Row(fila).Height = Math.Max(36, lineas * 16 + 10);
        fila += 2;

        // ----- Repuestos -----
        fila = Seccion(hoja, fila, "Repuestos utilizados");
        if (datos.Repuestos.Count == 0)
        {
            var vacio = hoja.Range(fila, 2, fila, 6).Merge();
            hoja.Cell(fila, 2).Value = "No se usaron repuestos en este ticket.";
            vacio.Style.Font.Italic = true;
            vacio.Style.Font.FontColor = XLColor.FromHtml(Gris);
            vacio.Style.Alignment.Indent = 1;
            fila++;
        }
        else
        {
            EncabezadoTabla(hoja, fila, new[] { "Cantidad", "Producto", "Fecha", "Precio unit.", "Subtotal" });
            fila++;

            int indice = 0;
            foreach (var m in datos.Repuestos)
            {
                decimal precio = m.Producto?.PrecioUnitario ?? 0;

                hoja.Cell(fila, 2).Value = m.Cantidad;
                hoja.Cell(fila, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Cell(fila, 3).Value = m.Producto?.Nombre ?? "";
                hoja.Cell(fila, 4).Value = m.Fecha.ToString("dd/MM/yyyy");
                hoja.Cell(fila, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Cell(fila, 5).Value = precio;
                hoja.Cell(fila, 5).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Cell(fila, 6).Value = precio * m.Cantidad;
                hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Cell(fila, 6).Style.Font.Bold = true;

                EstiloFilaTabla(hoja, fila, indice);
                indice++;
                fila++;
            }
        }
        fila++;

        // ----- Historial -----
        fila = Seccion(hoja, fila, "Historial de estados");
        EncabezadoTabla(hoja, fila, new[] { "Estado", "Fecha y hora", "Registrado por", "", "" });
        hoja.Range(fila, 4, fila, 6).Merge();
        fila++;

        int i2 = 0;
        foreach (var h in datos.Historial)
        {
            var (fondoH, textoH) = ColoresEstado(h.Estado);

            hoja.Cell(fila, 2).Value = Textos.Mostrar(h.Estado);
            hoja.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.FromHtml(fondoH);
            hoja.Cell(fila, 2).Style.Font.FontColor = XLColor.FromHtml(textoH);
            hoja.Cell(fila, 2).Style.Font.Bold = true;
            hoja.Cell(fila, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            hoja.Cell(fila, 3).Value = h.Fecha.ToString("dd/MM/yyyy HH:mm");
            hoja.Cell(fila, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            hoja.Range(fila, 4, fila, 6).Merge();
            hoja.Cell(fila, 4).Value = h.Usuario?.Nombre ?? "—";
            hoja.Cell(fila, 4).Style.Alignment.Indent = 1;

            // Solo el fondo alterno de las celdas que no son la pastilla de estado
            if (i2 % 2 == 1)
            {
                hoja.Cell(fila, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
                hoja.Range(fila, 4, fila, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            }
            hoja.Range(fila, 2, fila, 6).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
            hoja.Range(fila, 2, fila, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            hoja.Row(fila).Height = 20;
            i2++;
            fila++;
        }
        fila++;

        // ----- Resumen de costos -----
        fila = Seccion(hoja, fila, "Resumen de costos");
        FilaCostoExcel(hoja, fila, "Repuestos", costoRepuestos);
        fila++;
        FilaCostoExcel(hoja, fila, "Mano de obra", t.CostoManoObra);
        fila++;

        var etiquetaTotal = hoja.Range(fila, 4, fila, 5).Merge();
        hoja.Cell(fila, 4).Value = "COSTO TOTAL";
        etiquetaTotal.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        var celdaTotal = hoja.Cell(fila, 6);
        celdaTotal.Value = t.CostoTotal;
        celdaTotal.Style.NumberFormat.Format = FormatoMoneda;
        var filaTotal = hoja.Range(fila, 4, fila, 6);
        filaTotal.Style.Font.Bold = true;
        filaTotal.Style.Font.FontSize = 14;
        filaTotal.Style.Font.FontColor = XLColor.White;
        filaTotal.Style.Fill.BackgroundColor = XLColor.FromHtml(Naranja);
        filaTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        hoja.Row(fila).Height = 28;
        fila += 2;

        // ----- Pie -----
        var pie = hoja.Range(fila, 2, fila, 6).Merge();
        hoja.Cell(fila, 2).Value = $"COMPUFENIX · Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
        pie.Style.Font.FontSize = 9;
        pie.Style.Font.Italic = true;
        pie.Style.Font.FontColor = XLColor.FromHtml(Gris);
        pie.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // ----- Impresión: una hoja A4 de ancho -----
        hoja.PageSetup.PaperSize = XLPaperSize.A4Paper;
        hoja.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        hoja.PageSetup.FitToPages(1, 0);
        hoja.PageSetup.CenterHorizontally = true;

        libro.SaveAs(ruta);
    }

    // Título de sección con línea naranja debajo; devuelve la siguiente fila libre
    private static int Seccion(IXLWorksheet hoja, int fila, string titulo)
    {
        var rango = hoja.Range(fila, 2, fila, 6).Merge();
        hoja.Cell(fila, 2).Value = titulo.ToUpper();
        rango.Style.Font.Bold = true;
        rango.Style.Font.FontSize = 11;
        rango.Style.Font.FontColor = XLColor.FromHtml(Azul);
        rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        rango.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        rango.Style.Border.BottomBorderColor = XLColor.FromHtml(Naranja);
        hoja.Row(fila).Height = 24;
        return fila + 1;
    }

    // Etiqueta (gris) + valor, en la fila indicada
    private static void Par(IXLWorksheet hoja, int fila, int colEtiqueta, int colValor, int colValorFin,
        string etiqueta, string valor)
    {
        var celdaEtiqueta = hoja.Cell(fila, colEtiqueta);
        celdaEtiqueta.Value = etiqueta;
        celdaEtiqueta.Style.Font.Bold = true;
        celdaEtiqueta.Style.Font.FontColor = XLColor.FromHtml(Gris);
        celdaEtiqueta.Style.Fill.BackgroundColor = XLColor.FromHtml(FondoSuave);
        celdaEtiqueta.Style.Alignment.Indent = 1;

        var rangoValor = colValorFin > colValor
            ? hoja.Range(fila, colValor, fila, colValorFin).Merge()
            : hoja.Range(fila, colValor, fila, colValor);
        hoja.Cell(fila, colValor).Value = string.IsNullOrWhiteSpace(valor) ? "—" : valor;
        rangoValor.Style.Alignment.Indent = 1;
        rangoValor.Style.Alignment.WrapText = true;

        var fila2 = hoja.Range(fila, colEtiqueta, fila, colValorFin);
        fila2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        fila2.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        fila2.Style.Border.BottomBorderColor = XLColor.FromHtml(Borde);
        hoja.Row(fila).Height = 22;
    }

    private static void EncabezadoTabla(IXLWorksheet hoja, int fila, string[] titulos)
    {
        for (int i = 0; i < titulos.Length; i++)
        {
            hoja.Cell(fila, 2 + i).Value = titulos[i];
        }

        var rango = hoja.Range(fila, 2, fila, 6);
        rango.Style.Font.Bold = true;
        rango.Style.Font.FontColor = XLColor.White;
        rango.Style.Fill.BackgroundColor = XLColor.FromHtml(Azul);
        rango.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        hoja.Row(fila).Height = 22;
    }

    private static void EstiloFilaTabla(IXLWorksheet hoja, int fila, int indice)
    {
        var rango = hoja.Range(fila, 2, fila, 6);
        if (indice % 2 == 1) rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
        rango.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        hoja.Row(fila).Height = 20;
    }

    private static void FilaCostoExcel(IXLWorksheet hoja, int fila, string etiqueta, decimal monto)
    {
        var rangoEtiqueta = hoja.Range(fila, 4, fila, 5).Merge();
        hoja.Cell(fila, 4).Value = etiqueta;
        rangoEtiqueta.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        rangoEtiqueta.Style.Font.FontColor = XLColor.FromHtml(Gris);

        hoja.Cell(fila, 6).Value = monto;
        hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoMoneda;
        hoja.Cell(fila, 6).Style.Font.Bold = true;

        hoja.Range(fila, 4, fila, 6).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        hoja.Row(fila).Height = 20;
    }
}