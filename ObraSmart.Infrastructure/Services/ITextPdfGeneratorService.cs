using ObraSmart.Application.Common;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font; 
using iText.IO.Font.Constants; 

namespace ObraSmart.Infrastructure.Services
{
    public class ITextPdfGeneratorService : IPdfGeneratorService
    {
        public Task<Result<byte[]>> GenerarCotizacionPdfAsync(Cotizacion cotizacion)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Crear las fuentes estándar
                PdfFont fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                document.SetFont(fontNormal); // Fuente por defecto para todo el documento

                // Datos del Emisor (Usuario)
                var emisor = cotizacion.Presupuesto?.Usuario;
                if (emisor != null)
                {
                    document.Add(new Paragraph(emisor.RazonSocial ?? "Trabajador Independiente")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetFontSize(14)
                        .SetFont(fontBold));

                    document.Add(new Paragraph($"RUT: {emisor.Rut}")
                        .SetTextAlignment(TextAlignment.RIGHT));

                    document.Add(new Paragraph($"Teléfono: {emisor.Telefono} | Correo: {emisor.Correo}")
                        .SetTextAlignment(TextAlignment.RIGHT));
                }

                document.Add(new Paragraph("\n"));

                // Título y Fechas
                document.Add(new Paragraph("COTIZACIÓN DE SERVICIOS")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetFont(fontBold));

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

                    if (!string.IsNullOrEmpty(cliente.Direccion))
                        document.Add(new Paragraph($"Dirección: {cliente.Direccion}"));
                }

                document.Add(new Paragraph("\n"));

                // Detalle del Presupuesto (Tabla)
                var table = new Table(new float[] { 4, 1, 2, 2 }).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Cant.").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio Unit.").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(fontBold)));

                if (cotizacion.Presupuesto?.Items != null)
                {
                    foreach (var item in cotizacion.Presupuesto.Items)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(item.Descripcion)));
                        table.AddCell(new Cell().Add(new Paragraph(item.CantidadItem.ToString("0.##"))));
                        table.AddCell(new Cell().Add(new Paragraph($"${item.PrecioUnitarioCalculado:N0}")));
                        table.AddCell(new Cell().Add(new Paragraph($"${item.Subtotal:N0}")));
                    }
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // 6. Consolidado de Totales
                if (cotizacion.Presupuesto != null)
                {
                    document.Add(new Paragraph($"Subtotal: ${cotizacion.Presupuesto.Subtotal:N0}")
                        .SetTextAlignment(TextAlignment.RIGHT));

                    document.Add(new Paragraph($"IVA: ${cotizacion.Presupuesto.MontoIva:N0}")
                        .SetTextAlignment(TextAlignment.RIGHT));

                    document.Add(new Paragraph($"Total: ${cotizacion.Presupuesto.Total:N0}")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetFontSize(14)
                        .SetFont(fontBold));
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
