import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import { useRoute, useRouter } from 'vue-router';
import PresupuestoForm from '@/views/presupuestos/PresupuestoForm.vue';
import type { IPresupuesto } from '@/interfaces/IPresupuesto';

// Evita solicitudes HTTP reales.
vi.mock('@/services/presupuestoService', () => ({
  presupuestoService: {
    obtenerPorId: vi.fn(),
    crear: vi.fn(),
    actualizar: vi.fn()
  }
}));

vi.mock('@/services/clienteService', () => ({
  clienteService: {
    obtenerTodos: vi.fn(() => Promise.resolve([]))
  }
}));

vi.mock('@/services/apuService', () => ({
  apuService: {
    obtenerTodos: vi.fn(() => Promise.resolve([])),
    obtenerPorId: vi.fn()
  }
}));

vi.mock('@/services/insumoService', () => ({
  insumoService: {
    obtenerEtiquetas: vi.fn(() => Promise.resolve([])),
    obtenerTodos: vi.fn(() => Promise.resolve([])),
    obtenerUnidadesMedida: vi.fn(() => Promise.resolve([]))
  }
}));

vi.mock('vue-router', () => ({
  useRoute: vi.fn(),
  useRouter: vi.fn(() => ({
    push: vi.fn()
  }))
}));

describe('PresupuestoForm.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Guardar_EnModoClonacion_DebeCrearNuevoPresupuestoSinIdsOriginales', async () => {
    // Arrange
    const presupuestoOriginalId = 'presupuesto-original-123';

    (useRoute as any).mockReturnValue({
      params: {},
      query: {
        cloneId: presupuestoOriginalId
      }
    });

    const { presupuestoService } = await import(
      '@/services/presupuestoService'
    );

    const presupuestoOriginal: IPresupuesto = {
      id: presupuestoOriginalId,
      clienteId: 'cliente-123',
      clienteNombre: 'Cliente de prueba',
      nombreProyecto: 'Instalación sanitaria vivienda',
      fechaCreacion: '2026-08-05T10:00:00Z',
      estado: 'Borrador',
      subtotal: 11000,
      montoIva: 2090,
      total: 13090,
      esPlantilla: false,
      items: [
        {
          id: 'item-original-1',
          estructuraAPUOrigenId: 'apu-origen-123',
          descripcion: 'Instalación de lavaplatos',
          cantidadItem: 2,
          precioUnitarioCalculado: 4000,
          subtotal: 8000,
          unidadMedidaId: 1,
          recursos: [
            {
              id: 'recurso-original-1',
              tipoInsumo: 'Material',
              descripcionCongelada: 'Tubería PPR 20 mm',
              cantidad: 2,
              precioUnitarioCongelado: 1000,
              costoTotalRecurso: 2000,
              unidadMedidaId: 1
            },
            {
              id: 'recurso-original-2',
              tipoInsumo: 'ManoObra',
              descripcionCongelada: 'Maestro gasfíter',
              cantidad: 1,
              precioUnitarioCongelado: 2000,
              costoTotalRecurso: 2000,
              unidadMedidaId: 1
            }
          ]
        },
        {
          id: 'item-original-2',
          estructuraAPUOrigenId: null,
          descripcion: 'Prueba de funcionamiento',
          cantidadItem: 1,
          precioUnitarioCalculado: 3000,
          subtotal: 3000,
          unidadMedidaId: 1,
          recursos: [
            {
              id: 'recurso-original-3',
              tipoInsumo: 'Equipo',
              descripcionCongelada: 'Equipo de prueba',
              cantidad: 1,
              precioUnitarioCongelado: 3000,
              costoTotalRecurso: 3000,
              unidadMedidaId: 1
            }
          ]
        }
      ]
    };

    /*
     * Entregamos una copia para evitar que las transformaciones
     * realizadas por el componente modifiquen el objeto utilizado
     * como referencia en la prueba.
     */
    (presupuestoService.obtenerPorId as any).mockResolvedValue(
      JSON.parse(JSON.stringify(presupuestoOriginal))
    );

    /*
     * Las comprobaciones del payload se realizan exactamente en el
     * momento en que el componente llama al servicio de creación.
     *
     * Esto es importante porque, una vez creado el presupuesto,
     * PresupuestoForm asigna al formulario el nuevo ID retornado
     * por el backend.
     */
    (presupuestoService.crear as any).mockImplementationOnce(
      async (presupuesto: IPresupuesto) => {
        // El nuevo presupuesto no debe reutilizar el ID original.
        expect(presupuesto.id).toBeUndefined();

        expect(presupuesto.nombreProyecto)
          .toBe('Instalación sanitaria vivienda (Copia)');

        expect(presupuesto.clienteId)
          .toBe('cliente-123');

        expect(presupuesto.items)
          .toHaveLength(2);

        // Ningún ítem debe conservar su identificador original.
        expect(
          presupuesto.items.every(
            item => item.id === undefined
          )
        ).toBe(true);

        // Ningún recurso debe conservar su identificador original.
        expect(
          presupuesto.items.every(
            item => item.recursos.every(
              recurso => recurso.id === undefined
            )
          )
        ).toBe(true);

        const itemInstalacion = presupuesto.items.find(
          item =>
            item.descripcion === 'Instalación de lavaplatos'
        );

        expect(itemInstalacion).toBeDefined();

        /*
         * Se conserva la referencia funcional al APU de origen,
         * pero no el ID del ítem perteneciente al presupuesto original.
         */
        expect(itemInstalacion!.estructuraAPUOrigenId)
          .toBe('apu-origen-123');

        expect(itemInstalacion!.cantidadItem)
          .toBe(2);

        expect(itemInstalacion!.recursos)
          .toHaveLength(2);

        const material = itemInstalacion!.recursos.find(
          recurso =>
            recurso.descripcionCongelada ===
            'Tubería PPR 20 mm'
        );

        expect(material).toBeDefined();
        expect(material!.id).toBeUndefined();
        expect(material!.tipoInsumo).toBe('Material');
        expect(material!.cantidad).toBe(2);
        expect(material!.precioUnitarioCongelado).toBe(1000);

        const manoObra = itemInstalacion!.recursos.find(
          recurso =>
            recurso.descripcionCongelada ===
            'Maestro gasfíter'
        );

        expect(manoObra).toBeDefined();
        expect(manoObra!.id).toBeUndefined();
        expect(manoObra!.cantidad).toBe(1);
        expect(manoObra!.precioUnitarioCongelado).toBe(2000);

        /*
         * Comprobación explícita de independencia:
         * ninguno de los IDs originales debe estar presente
         * en el payload enviado al backend.
         */
        const payloadSerializado = JSON.stringify(presupuesto);

        expect(payloadSerializado)
          .not.toContain('presupuesto-original-123');

        expect(payloadSerializado)
          .not.toContain('item-original-1');

        expect(payloadSerializado)
          .not.toContain('item-original-2');

        expect(payloadSerializado)
          .not.toContain('recurso-original-1');

        expect(payloadSerializado)
          .not.toContain('recurso-original-2');

        expect(payloadSerializado)
          .not.toContain('recurso-original-3');

        /*
         * Simulamos el ID que devuelve el backend después
         * de crear correctamente el nuevo presupuesto.
         */
        return 'presupuesto-copia-456';
      }
    );

    const wrapper = mount(PresupuestoForm, {
      global: {
        stubs: {
          /*
           * Botón simplificado que permite disparar realmente
           * el evento click utilizado por PresupuestoForm.
           */
          Button: {
            props: ['label'],
            emits: ['click'],
            template: `
              <button
                type="button"
                :data-label="label"
                @click="$emit('click')">
                {{ label }}
              </button>
            `
          },
          Message: true,
          InputText: true,
          Select: true,
          InputNumber: true,
          Dialog: true,
          GenerarCotizacionDialog: true
        }
      }
    });

    // Resuelve la carga de catálogos y del presupuesto original.
    await flushPromises();
    await wrapper.vm.$nextTick();

    // Control previo: debe cargar el presupuesto indicado por cloneId.
    expect(presupuestoService.obtenerPorId)
      .toHaveBeenCalledTimes(1);

    expect(presupuestoService.obtenerPorId)
      .toHaveBeenCalledWith(presupuestoOriginalId);

    /*
     * Aunque se cargó un presupuesto existente, la pantalla
     * debe funcionar como creación y no como edición.
     */
    expect(wrapper.text()).toContain('Nuevo Presupuesto');
    expect(wrapper.text()).not.toContain('Editar Presupuesto');

    // Act
    const botonGuardar = wrapper.get(
      '[data-label="Guardar"]'
    );

    await botonGuardar.trigger('click');
    await flushPromises();

    // Assert
    expect(presupuestoService.crear)
      .toHaveBeenCalledTimes(1);

    expect(presupuestoService.actualizar)
      .not.toHaveBeenCalled();
  });

  it('GuardarYCotizar_ConPresupuestoNuevo_DebeCrearYMostrarDialogoConNuevoId', async () => {
    // Arrange
    const nuevoPresupuestoId = 'presupuesto-nuevo-789';
    const routerPushMock = vi.fn();

    (useRoute as any).mockReturnValue({
      params: {},
      query: {}
    });

    (useRouter as any).mockReturnValue({
      push: routerPushMock
    });

    const { presupuestoService } = await import(
      '@/services/presupuestoService'
    );

    /*
     * Verificamos el estado del presupuesto exactamente
     * cuando se envía al servicio de creación.
     */
    (presupuestoService.crear as any).mockImplementationOnce(
      async (presupuesto: IPresupuesto) => {
        expect(presupuesto.id).toBeUndefined();

        expect(presupuesto.nombreProyecto)
          .toBe('Instalación sanitaria nueva');

        expect(presupuesto.clienteId)
          .toBe('cliente-123');

        expect(presupuesto.items)
          .toHaveLength(1);

        return nuevoPresupuestoId;
      }
    );

    /*
     * jsdom no implementa scrollIntoView.
     * El componente lo utiliza después de agregar un ítem manual.
     */
    const scrollIntoViewMock = vi.fn();

    Object.defineProperty(
      HTMLElement.prototype,
      'scrollIntoView',
      {
        configurable: true,
        value: scrollIntoViewMock
      }
    );

    const wrapper = mount(PresupuestoForm, {
      global: {
        stubs: {
          Button: {
            props: ['label', 'disabled'],
            emits: ['click'],
            template: `
            <button
              type="button"
              :data-label="label"
              :disabled="disabled"
              @click="$emit('click')">
              {{ label }}
            </button>
          `
          },

          /*
           * Input simplificado con soporte real de v-model.
           */
          InputText: {
            props: ['modelValue'],
            emits: ['update:modelValue'],
            template: `
            <input
              v-bind="$attrs"
              :value="modelValue ?? ''"
              @input="$emit(
                'update:modelValue',
                $event.target.value
              )"
            />
          `
          },

          /*
           * Para esta prueba basta tratar Select como un input.
           * Nos interesa modificar formulario.clienteId.
           */
          Select: {
            props: ['modelValue'],
            emits: ['update:modelValue'],
            template: `
            <input
              v-bind="$attrs"
              :value="modelValue ?? ''"
              @input="$emit(
                'update:modelValue',
                $event.target.value
              )"
            />
          `
          },

          InputNumber: true,
          Message: true,
          Dialog: true,

          /*
           * Este stub nos permite comprobar que el diálogo
           * recibe el ID retornado por presupuestoService.crear().
           */
          GenerarCotizacionDialog: {
            props: ['visible', 'presupuestoId'],
            emits: ['update:visible'],
            template: `
            <div
              v-if="visible"
              data-testid="generar-cotizacion-dialog"
              :data-presupuesto-id="presupuestoId">
            </div>
          `
          }
        }
      }
    });

    await flushPromises();
    await wrapper.vm.$nextTick();

    /*
     * Control inicial:
     * estamos creando un presupuesto completamente nuevo.
     */
    expect(wrapper.text())
      .toContain('Nuevo Presupuesto');

    expect(wrapper.text())
      .not.toContain('Editar Presupuesto');

    expect(presupuestoService.obtenerPorId)
      .not.toHaveBeenCalled();

    // Ingresamos nombre del proyecto.
    await wrapper.get('#proyecto')
      .setValue('Instalación sanitaria nueva');

    // Asignamos cliente.
    await wrapper.get('#cliente')
      .setValue('cliente-123');

    await wrapper.vm.$nextTick();

    /*
     * Agregamos el mínimo requerido para que guardar()
     * considere válido el presupuesto.
     */
    const botonNuevoItem = wrapper.get(
      '[data-label="Nuevo Ítem Ad-Hoc"]'
    );

    await botonNuevoItem.trigger('click');

    await flushPromises();
    await wrapper.vm.$nextTick();

    /*
     * El botón debe estar habilitado porque ya existe
     * un cliente seleccionado.
     */
    const botonGuardarYCotizar = wrapper.get(
      '[data-label="Guardar y Cotizar"]'
    );

    expect(
      botonGuardarYCotizar.attributes('disabled')
    ).toBeUndefined();

    // Act
    await botonGuardarYCotizar.trigger('click');

    await flushPromises();
    await wrapper.vm.$nextTick();

    // Assert

    /*
     * Como es un presupuesto nuevo, debe utilizar crear()
     * y nunca actualizar().
     */
    expect(presupuestoService.crear)
      .toHaveBeenCalledTimes(1);

    expect(presupuestoService.actualizar)
      .not.toHaveBeenCalled();

    /*
     * Guardar y Cotizar no debe regresar al listado.
     */
    expect(routerPushMock)
      .not.toHaveBeenCalled();

    /*
     * Después de crear el presupuesto debe aparecer
     * GenerarCotizacionDialog.
     */
    const dialogo = wrapper.get(
      '[data-testid="generar-cotizacion-dialog"]'
    );

    /*
     * La parte fundamental:
     * el diálogo debe recibir exactamente el ID que
     * presupuestoService.crear() acaba de devolver.
     */
    expect(
      dialogo.attributes('data-presupuesto-id')
    ).toBe(nuevoPresupuestoId);
  });
});
