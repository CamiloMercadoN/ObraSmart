import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import { useRoute } from 'vue-router';
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
     * Entregamos una copia para que el componente pueda eliminar
     * identificadores sin modificar la constante usada por la prueba.
     */
    (presupuestoService.obtenerPorId as any).mockResolvedValue(
      JSON.parse(JSON.stringify(presupuestoOriginal))
    );

    (presupuestoService.crear as any).mockResolvedValue(
      'presupuesto-copia-456'
    );

    const wrapper = mount(PresupuestoForm, {
      global: {
        stubs: {
          /*
           * Se utiliza un botón simplificado real para poder disparar
           * el evento click del botón Guardar.
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
          Dialog: true
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
     * Aunque se cargó un presupuesto existente, la pantalla debe
     * funcionar como creación y no como edición.
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

    const presupuestoEnviado = (
      presupuestoService.crear as any
    ).mock.calls[0][0] as IPresupuesto;

    // El nuevo presupuesto no debe reutilizar el ID del original.
    expect(presupuestoEnviado.id).toBeUndefined();

    expect(presupuestoEnviado.nombreProyecto)
      .toBe('Instalación sanitaria vivienda (Copia)');

    expect(presupuestoEnviado.clienteId)
      .toBe('cliente-123');

    expect(presupuestoEnviado.items)
      .toHaveLength(2);

    // Ningún ítem debe conservar su identificador original.
    expect(
      presupuestoEnviado.items.every(
        item => item.id === undefined
      )
    ).toBe(true);

    // Ningún recurso debe conservar su identificador original.
    expect(
      presupuestoEnviado.items.every(
        item => item.recursos.every(
          recurso => recurso.id === undefined
        )
      )
    ).toBe(true);

    const itemInstalacion = presupuestoEnviado.items.find(
      item =>
        item.descripcion === 'Instalación de lavaplatos'
    );

    expect(itemInstalacion).toBeDefined();

    /*
     * Se conserva la referencia funcional al APU de origen,
     * pero no el ID del ítem del presupuesto anterior.
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
     * ninguno de los IDs originales aparece en el payload.
     */
    const payloadSerializado =
      JSON.stringify(presupuestoEnviado);

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
  });
});
