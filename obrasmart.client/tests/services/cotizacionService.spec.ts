import { beforeEach, describe, expect, it, vi } from 'vitest';
import { cotizacionService } from '../../src/services/cotizacionService';
import { apiClient } from '../../src/utils/apiClient';

vi.mock('../../src/utils/apiClient', () => ({
  apiClient: vi.fn(),
  manejarErrorHttp: vi.fn()
}));

describe('cotizacionService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('CompartirPdf_ConWebShareDisponible_DebeCompartirArchivoPdf', async () => {
    // Arrange
    const cotizacionId = '11111111-1111-1111-1111-111111111111';
    const numeroCotizacion = 'COT-25';

    const pdfBlob = new Blob(
      ['%PDF-1.7 contenido de prueba'],
      { type: 'application/pdf' }
    );

    const responseMock = {
      ok: true,
      blob: vi.fn().mockResolvedValue(pdfBlob)
    } as unknown as Response;

    vi.mocked(apiClient).mockResolvedValue(responseMock);

    const canShareMock = vi.fn().mockReturnValue(true);
    const shareMock = vi.fn().mockResolvedValue(undefined);

    Object.defineProperty(navigator, 'canShare', {
      configurable: true,
      value: canShareMock
    });

    Object.defineProperty(navigator, 'share', {
      configurable: true,
      value: shareMock
    });

    // Act
    await cotizacionService.compartirPdf(
      cotizacionId,
      numeroCotizacion
    );

    // Assert
    expect(apiClient).toHaveBeenCalledTimes(1);

    expect(apiClient).toHaveBeenCalledWith(
      `/Cotizaciones/${cotizacionId}/pdf?incluirRecursos=false`
    );

    expect(canShareMock).toHaveBeenCalledTimes(1);
    expect(shareMock).toHaveBeenCalledTimes(1);

    const [sharePayload] = shareMock.mock.calls[0]! as [{
      title: string;
      text: string;
      files: File[];
    }];

    expect(sharePayload.title).toBe(
      `Cotización ${numeroCotizacion}`
    );

    expect(sharePayload.text).toBe(
      'Adjunto la cotización solicitada.'
    );

    expect(sharePayload.files).toHaveLength(1);

    const archivoCompartido = sharePayload.files[0]!;

    expect(archivoCompartido).toBeInstanceOf(File);

    expect(archivoCompartido.name).toBe(
      `Cotizacion-${numeroCotizacion}.pdf`
    );

    expect(archivoCompartido.type).toBe(
      'application/pdf'
    );

    expect(archivoCompartido.size).toBeGreaterThan(0);
  });

  it('CompartirPdf_SinWebShareDisponible_DebeDescargarPdfComoFallback', async () => {
    // Arrange
    const cotizacionId = '22222222-2222-2222-2222-222222222222';
    const numeroCotizacion = 'COT-26';

    const pdfBlob = new Blob(
      ['%PDF-1.7 contenido de prueba fallback'],
      { type: 'application/pdf' }
    );

    const responseMock = {
      ok: true,
      blob: vi.fn().mockResolvedValue(pdfBlob)
    } as unknown as Response;

    vi.mocked(apiClient).mockResolvedValue(responseMock);

    // Simula un navegador que no permite compartir archivos.
    const canShareMock = vi.fn().mockReturnValue(false);
    const shareMock = vi.fn().mockResolvedValue(undefined);

    Object.defineProperty(navigator, 'canShare', {
      configurable: true,
      value: canShareMock
    });

    Object.defineProperty(navigator, 'share', {
      configurable: true,
      value: shareMock
    });

    // Simula la creación de una URL temporal para el Blob.
    const createObjectURLMock = vi.fn().mockReturnValue(
      'blob:http://localhost/pdf-prueba'
    );

    const revokeObjectURLMock = vi.fn();

    Object.defineProperty(window.URL, 'createObjectURL', {
      configurable: true,
      value: createObjectURLMock
    });

    Object.defineProperty(window.URL, 'revokeObjectURL', {
      configurable: true,
      value: revokeObjectURLMock
    });

    /*
     * Capturamos el enlace sobre el que el servicio ejecuta click().
     * Así podemos comprobar nombre y URL incluso después de que
     * el servicio lo elimine del DOM.
     */
    let enlaceDescarga: HTMLAnchorElement | undefined;

    const clickMock = vi
      .spyOn(HTMLAnchorElement.prototype, 'click')
      .mockImplementation(function (this: HTMLAnchorElement) {
        enlaceDescarga = this;
      });

    // Act
    await cotizacionService.compartirPdf(
      cotizacionId,
      numeroCotizacion
    );

    // Assert API
    expect(apiClient).toHaveBeenCalledTimes(1);

    expect(apiClient).toHaveBeenCalledWith(
      `/Cotizaciones/${cotizacionId}/pdf?incluirRecursos=false`
    );

    // Comprueba que se evaluó Web Share.
    expect(canShareMock).toHaveBeenCalledTimes(1);

    // Al no existir soporte, navigator.share no debe ejecutarse.
    expect(shareMock).not.toHaveBeenCalled();

    // Debe generar una URL temporal para descargar el Blob.
    expect(createObjectURLMock).toHaveBeenCalledTimes(1);
    expect(createObjectURLMock).toHaveBeenCalledWith(pdfBlob);

    // Debe ejecutar realmente la descarga.
    expect(clickMock).toHaveBeenCalledTimes(1);

    expect(enlaceDescarga).toBeDefined();

    expect(enlaceDescarga!.download).toBe(
      `Cotizacion-${numeroCotizacion}.pdf`
    );

    expect(enlaceDescarga!.href).toBe(
      'blob:http://localhost/pdf-prueba'
    );

    // La URL temporal debe liberarse después de descargar.
    expect(revokeObjectURLMock).toHaveBeenCalledTimes(1);

    expect(revokeObjectURLMock).toHaveBeenCalledWith(
      'blob:http://localhost/pdf-prueba'
    );
  });

});
