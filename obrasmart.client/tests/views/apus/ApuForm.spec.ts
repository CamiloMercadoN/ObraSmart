import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import { useRoute } from 'vue-router';
import ApuForm from '@/views/apus/ApuForm.vue';

// Mockeamos los servicios para que no hagan peticiones HTTP reales
vi.mock('@/services/apuService', () => ({
  apuService: {
    obtenerPorId: vi.fn()
  }
}));

vi.mock('@/services/insumoService', () => ({
  insumoService: {
    obtenerUnidadesMedida: vi.fn(() => Promise.resolve([])),
    obtenerEtiquetas: vi.fn(() => Promise.resolve([])),
    obtenerTodos: vi.fn(() => Promise.resolve([]))
  }
}));

// Mockeamos Vue Router
vi.mock('vue-router', () => ({
  useRoute: vi.fn(),
  useRouter: vi.fn(() => ({
    push: vi.fn()
  }))
}));

describe('ApuForm.vue', () => {

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Debe inicializar el Costo Directo Total en $ 0 cuando es un APU nuevo', async () => {
    // Arrange: Simulamos que la ruta no tiene un parámetro ID (Creación)
    (useRoute as any).mockReturnValue({
      params: {},
      query: {}
    });

    // Montamos el componente aislando los componentes hijos de PrimeVue
    const wrapper = mount(ApuForm, {
      global: {
        stubs: [
          'Button', 'Message', 'InputText', 'Select', 'MultiSelect',
          'InputNumber', 'Tag', 'InsumoFormDialog', 'EtiquetaFormDialog'
        ],
        directives: {
          tooltip: () => { } // Ignoramos la directiva v-tooltip
        }
      }
    });

    // Esperamos a que se resuelvan las promesas del onMounted
    await flushPromises();

    // Act & Assert: Buscamos el texto dentro del componente
    const htmlRenderizado = wrapper.html();

    // Verificamos la renderización inicial
    expect(htmlRenderizado).toContain('Costo Directo Total: $ 0');
  });

  it('Debe calcular y renderizar el costo total correctamente al cargar un APU existente', async () => {
    // Arrange: Simulamos que estamos editando el APU con ID 123
    (useRoute as any).mockReturnValue({
      params: { id: '123' },
      query: {}
    });

    // Inyectamos un APU falso en el servicio simulado con 2 componentes
    // Costo esperado: (2 * 1500) + (1 * 6500) = 3000 + 6500 = 9500
    const { apuService } = await import('@/services/apuService');
    (apuService.obtenerPorId as any).mockResolvedValue({
      id: '123',
      nombre: 'APU Test',
      unidadMedidaId: 1,
      etiquetasIds: [],
      componentes: [
        {
          insumoId: 'insumo-1',
          descripcionInsumo: 'Tubo PVC 1/2"',
          tipoInsumo: 'Material',
          precioUnitarioReferencia: 1500,
          cantidad: 2
        },
        {
          insumoId: 'insumo-2',
          descripcionInsumo: 'Maestro Gasfíter',
          tipoInsumo: 'Mano de Obra',
          precioUnitarioReferencia: 6500,
          cantidad: 1
        }
      ]
    });

    const wrapper = mount(ApuForm, {
      global: {
        stubs: [
          'Button', 'Message', 'InputText', 'Select', 'MultiSelect',
          'InputNumber', 'Tag', 'InsumoFormDialog', 'EtiquetaFormDialog'
        ],
        directives: {
          tooltip: () => { }
        }
      }
    });

    // Act: Esperamos a que el hook onMounted termine de ejecutar cargarApuExistente()
    await flushPromises();

    // Assert: Validamos que la computada costoTotalApu hizo la suma matemática y se formateó
    const htmlRenderizado = wrapper.html();
    expect(htmlRenderizado).toContain('Costo Directo Total: $ 9.500');
  });
});
