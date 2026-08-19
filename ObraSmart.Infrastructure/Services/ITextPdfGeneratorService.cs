using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font; 
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Options;
using ObraSmart.Application.Common;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Infrastructure.Options;

namespace ObraSmart.Infrastructure.Services
{
    public class ITextPdfGeneratorService(IOptions<StorageOptions> _storageOptions) : IPdfGeneratorService
    {
        public Task<Result<byte[]>> GenerarCotizacionPdfAsync(Cotizacion cotizacion, bool incluirRecursos)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                PdfFont fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                document.SetFont(fontNormal);

                var emisor = cotizacion.Presupuesto?.Usuario;

                // HEADER CON LOGO
                Table headerTable = new Table(2).UseAllAvailableWidth();

                Cell logoCell = new Cell().SetBorder(Border.NO_BORDER);
                if (emisor != null && !string.IsNullOrWhiteSpace(emisor.LogoUrl))
                {
                    // Limpiamos la ruta relativa y la combinamos con el BasePath configurado
                    string rutaSinSlash = emisor.LogoUrl.TrimStart('/');
                    string logoPath = Path.Combine(_storageOptions.Value.BasePath, rutaSinSlash);

                    if (File.Exists(logoPath))
                    {
                        var imgData = ImageDataFactory.Create(logoPath);
                        var logo = new Image(imgData).SetMaxHeight(60).SetMaxWidth(150);
                        logoCell.Add(logo);
                    }
                }
                headerTable.AddCell(logoCell);

                // Datos del Emisor al lado derecho
                Cell emisorCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
                if (emisor != null)
                {
                    emisorCell.Add(new Paragraph(emisor.RazonSocial ?? "Trabajador Independiente").SetFontSize(14).SetFont(fontBold));
                    emisorCell.Add(new Paragraph($"RUT: {emisor.Rut}"));
                    emisorCell.Add(new Paragraph($"Tel: {emisor.Telefono} | Email: {emisor.Correo}"));
                }
                headerTable.AddCell(emisorCell);

                document.Add(headerTable);
                document.Add(new Paragraph("\n"));

                // Título y Fechas
                document.Add(new Paragraph("COTIZACIÓN DE SERVICIOS")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SetFont(fontBold));

                document.Add(new Paragraph($"N° Cotización: {cotizacion.NumeroCotizacion}"));
                document.Add(new Paragraph($"Fecha de Emisión: {cotizacion.FechaEmision:dd/MM/yyyy}"));
                document.Add(new Paragraph($"Válida hasta: {cotizacion.FechaVencimiento:dd/MM/yyyy}"));
                document.Add(new Paragraph("\n"));

                // Datos del Cliente
                var cliente = cotizacion.Presupuesto?.Cliente;
                if (cliente != null)
                {
                    document.Add(new Paragraph("Datos del Cliente").SetFont(fontBold));
                    document.Add(new Paragraph($"Nombre o Razón Social: {cliente.Nombre}"));
                    document.Add(new Paragraph($"RUT: {cliente.Rut}"));
                    if (!string.IsNullOrEmpty(cliente.Direccion)) document.Add(new Paragraph($"Dirección: {cliente.Direccion}, {cliente?.Ciudad?.Nombre}"));
                }
                document.Add(new Paragraph("\n"));

                // Detalle del Presupuesto (Tabla)
                var table = new Table(new float[] { 4, 1, 1, 2, 2 }).UseAllAvailableWidth();

                table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(fontBold)).SetTextAlignment(TextAlignment.LEFT));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Cant.").SetFont(fontBold)).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Unidad").SetFont(fontBold)).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio Unit.").SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT));

                if (cotizacion.Presupuesto?.Items != null)
                {
                    foreach (var item in cotizacion.Presupuesto.Items)
                    {
                        string unidad = item.UnidadMedida?.Nombre ?? "";

                        table.AddCell(new Cell().Add(new Paragraph(item.Descripcion)));
                        table.AddCell(new Cell().Add(new Paragraph(item.CantidadItem.ToString("0.##"))).SetTextAlignment(TextAlignment.CENTER));
                        table.AddCell(new Cell().Add(new Paragraph(unidad)).SetTextAlignment(TextAlignment.CENTER));
                        table.AddCell(new Cell().Add(new Paragraph($"${item.PrecioUnitarioCalculado:N0}"))).SetTextAlignment(TextAlignment.RIGHT);
                        table.AddCell(new Cell().Add(new Paragraph($"${item.Subtotal:N0}"))).SetTextAlignment(TextAlignment.RIGHT);

                        // Desglose de Insumos (Recursos)
                        if (incluirRecursos && item.Recursos.Any())
                        {
                            var listaRecursos = new iText.Layout.Element.List()
                                .SetSymbolIndent(10)
                                .SetFontSize(10)
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY);

                            foreach (var rec in item.Recursos)
                            {
                                string unidadRecurso = rec.UnidadMedida?.Nombre ?? "";
                                listaRecursos.Add(new ListItem($"{rec.Cantidad:0.##} {unidadRecurso} de {rec.DescripcionCongelada} (${rec.PrecioUnitarioCongelado:N0})"));
                            }

                            // ATENCIÓN: colspan cambiado a 5 para abarcar toda la fila nueva
                            var recursosCell = new Cell(1, 5).SetBorder(Border.NO_BORDER).SetPaddingLeft(15);
                            recursosCell.Add(listaRecursos);
                            table.AddCell(recursosCell);
                        }
                    }
                }
                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Consolidado de Totales
                if (cotizacion.Presupuesto != null)
                {
                    document.Add(new Paragraph($"Subtotal: ${cotizacion.Presupuesto.Subtotal:N0}").SetTextAlignment(TextAlignment.RIGHT));
                    document.Add(new Paragraph($"IVA: ${cotizacion.Presupuesto.MontoIva:N0}").SetTextAlignment(TextAlignment.RIGHT));
                    document.Add(new Paragraph($"Total: ${cotizacion.Presupuesto.Total:N0}").SetTextAlignment(TextAlignment.RIGHT).SetFontSize(14).SetFont(fontBold));
                }

                document.Close();
                return Task.FromResult(Result<byte[]>.Success(memoryStream.ToArray()));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result<byte[]>.Failure($"Error al generar el PDF: {ex.Message}", "PDF_ERROR"));
            }
        }
    }
}
