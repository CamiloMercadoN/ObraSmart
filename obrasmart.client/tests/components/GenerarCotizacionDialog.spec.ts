import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount } from '@vue/test-utils';
import GenerarCotizacionDialog from '@/components/GenerarCotizacionDialog.vue';
import type { ICotizacion } from '@/interfaces/ICotizacion';

vi.mock('@/services/cotizacionService', () => ({
  cotizacionService: {
    crear: vi.fn(),
    obtenerTodas: vi.fn(() => Promise.resolve([])),
    compartirPdf: vi.fn()
  }
}));

vi.mock('@/services/configuracionService', () => ({
  configuracionService: {
    obtener: vi.fn(() => Promise.resolve({
      diasValidez: 15
    }))
  }
}));

vi.mock('vue-router', () => ({
  useRouter: vi.fn(() => ({
    push: vi.fn()
  }))
}));

vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({
    require: vi.fn(),
    close: vi.fn()
  })
}));

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: vi.fn()
  })
}));

describe('GenerarCotizacionDialog.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.clearAllTimers();
    vi.useRealTimers();
  });

  it('Generar_ConDatosValidos_DebeCrearCotizacionConPresupuestoYFechaIndicados', async () => {
    // Arrange
    const presupuestoId =
      'presupuesto-nuevo-789';

    const cotizacionId =
      'cotizacion-nueva-123';

    const fechaVencimiento =
      new Date('2026-08-31T12:00:00.000Z');

    const { cotizacionService } = await import(
      '@/services/cotizacionService'
    );

    const cotizacionCreada: ICotizacion = {
      id: cotizacionId,
      presupuestoId,
      numeroCotizacion: 'COT-27',
      fechaEmision: '2026-08-18T12:00:00.000Z',
      fechaVencimiento: fechaVencimiento.toISOString(),
      estado: 'Borrador',
      archivoPdfUrl: '',
      nombreProyecto: 'Instalación sanitaria nueva',
      clienteNombre: 'Cliente de prueba'
    };

    (cotizacionService.crear as any).mockResolvedValue(
      cotizacionCreada
    );

    const wrapper = mount(
      GenerarCotizacionDialog,
      {
        props: {
          visible: true,
          presupuestoId
        },
        global: {
          stubs: {
            Dialog: {
              template: `
                <div>
                  <slot />
                  <slot name="footer" />
                </div>
              `
            },

            Button: {
              props: ['label', 'loading'],
              emits: ['click'],
              template: `
                <button
                  type="button"
                  :data-label="label"
                  :data-loading="loading"
                  @click="$emit('click')">
                  {{ label }}
                </button>
              `
            },

            DatePicker: {
              name: 'DatePicker',
              props: ['modelValue'],
              emits: ['update:modelValue'],
              template: `
                <div data-testid="fecha-vencimiento"></div>
              `
            },

            InputNumber: true,
            Message: true
          }
        }
      }
    );

    await flushPromises();

    /*
     * Simulamos que el usuario selecciona explícitamente
     * una fecha de vencimiento.
     */
    const datePicker = wrapper.findComponent({
      name: 'DatePicker'
    });

    expect(datePicker.exists()).toBe(true);

    datePicker.vm.$emit(
      'update:modelValue',
      fechaVencimiento
    );

    await wrapper.vm.$nextTick();

    // Act
    const botonGenerar = wrapper.get(
      '[data-label="Generar"]'
    );

    await botonGenerar.trigger('click');

    await flushPromises();

    // Assert
    expect(cotizacionService.crear)
      .toHaveBeenCalledTimes(1);

    expect(cotizacionService.crear)
      .toHaveBeenCalledWith({
        presupuestoId,
        fechaVencimiento:
          fechaVencimiento.toISOString(),
        numeroCotizacionPersonalizado: null
      });

    /*
     * Una creación exitosa debe cerrar el diálogo.
     */
    expect(
      wrapper.emitted('update:visible')
    ).toBeDefined();

    expect(
      wrapper.emitted('update:visible')![0]
    ).toEqual([false]);

    /*
     * También debe informar al componente padre
     * que la cotización fue generada.
     */
    expect(
      wrapper.emitted('generada')
    ).toBeDefined();

    expect(
      wrapper.emitted('generada')
    ).toHaveLength(1);
  });
});
