<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4 h-full">

    <div class="surface-card p-3 md:p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">

      <!-- Cabecera Responsiva -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-box text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-lg md:text-xl font-bold">Catálogo de Insumos</h2>
            <span class="text-500 text-sm hidden md:block">Administra los materiales, mano de obra y equipos</span>
          </div>
        </div>

        <div class="flex flex-column sm:flex-row gap-2 w-full md:w-auto flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Filtros' : 'Filtros'"
                  severity="secondary"
                  outlined
                  class="w-full sm:w-auto"
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo Insumo" icon="pi pi-plus" class="w-full sm:w-auto" @click="abrirNuevo" />
        </div>
      </div>

      <!-- Mensaje de Error -->
      <Message v-if="globalError" severity="error" :closable="true" @close="globalError = ''" class="mb-3">
        {{ globalError }}
      </Message>

      <!-- Zona de Filtros -->
      <div v-if="mostrarFiltros" class="flex flex-column md:flex-row gap-3 mb-4 p-3 surface-100 border-round">

        <!-- Buscador con flex-1 y IconField -->
        <div class="flex-1 w-full">
          <IconField class="w-full">
            <InputIcon class="pi pi-search" />
            <InputText v-model="filtroTexto" placeholder="Buscar por descripción..." class="w-full" />
          </IconField>
        </div>

        <div class="flex flex-column sm:flex-row gap-3 w-full md:w-auto flex-shrink-0">
          <div class="w-full sm:w-15rem">
            <Select v-model="filtroTipo"
                    :options="tiposInsumo"
                    placeholder="Filtrar por Tipo"
                    showClear
                    class="w-full" />
          </div>

          <div class="w-full sm:w-15rem">
            <Select v-model="filtroEtiqueta"
                    :options="catalogoEtiquetas"
                    optionLabel="nombre"
                    optionValue="id"
                    placeholder="Filtrar por Etiqueta"
                    showClear
                    class="w-full" />
          </div>
        </div>
      </div>

      <!-- Vista de Tarjetas (DataView) -->
      <div class="flex-grow-1 overflow-auto">

        <div v-if="cargando" class="flex justify-content-center align-items-center p-5">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>

        <div v-else-if="insumosFiltrados.length === 0" class="text-center p-5 text-500 border-round surface-100 border-1 surface-border border-dashed">
          No se encontraron insumos que coincidan con la búsqueda.
        </div>

        <div v-else class="grid">
          <div v-for="insumo in insumosFiltrados" :key="insumo.id" class="col-12 md:col-6 lg:col-4 xl:col-3">

            <!-- Tarjeta Individual -->
            <div class="surface-card border-1 surface-border border-round shadow-1 flex flex-column h-full">

              <!-- Header Tarjeta -->
              <div class="flex justify-content-between align-items-start p-3 border-bottom-1 surface-border surface-50 border-round-top">
                <span class="text-900 font-bold text-lg line-height-2 mb-1 pr-2" style="word-break: break-word;">
                  {{ insumo.descripcion }}
                </span>
                <div class="flex-shrink-0 mt-1">
                  <Tag v-if="insumo.esPlantilla" value="Plantilla" severity="info" class="text-xs" v-tooltip.top="'Plantilla del Sistema'" />
                  <Tag v-else value="Usuario" severity="success" class="text-xs" />
                </div>
              </div>

              <!-- Body Tarjeta -->
              <div class="p-3 flex-grow-1 flex flex-column gap-3 justify-content-between">

                <div class="flex justify-content-between align-items-center">
                  <span class="text-500 text-sm">Tipo:</span>
                  <span :class="OBTENER_CLASE_TIPO_INSUMO(insumo.tipoInsumo)">
                    {{ insumo.tipoInsumo }}
                  </span>
                </div>

                <div class="flex justify-content-between align-items-center">
                  <span class="text-500 text-sm">Unidad:</span>
                  <span class="text-700 text-sm font-semibold">{{ insumo.unidadMedidaNombre }}</span>
                </div>

                <!-- Etiquetas -->
                <div v-if="insumo.etiquetasIds && insumo.etiquetasIds.length > 0">
                  <span class="text-500 text-sm block mb-2">Etiquetas:</span>
                  <div class="flex flex-wrap gap-1">
                    <span v-for="tagId in insumo.etiquetasIds"
                          :key="tagId"
                          class="text-xs px-2 py-1 border-round font-semibold"
                          :style="obtenerEstiloEtiqueta(tagId)">
                      {{ obtenerNombreEtiqueta(tagId) }}
                    </span>
                  </div>
                </div>

                <!-- Precio Referencia -->
                <div class="flex justify-content-between align-items-center pt-3 border-top-1 surface-border mt-auto">
                  <span class="text-700 font-bold text-sm">Precio Referencia:</span>
                  <span class="text-blue-600 font-bold text-xl">{{ formatMonedaChilena(insumo.precioReferencia) }}</span>
                </div>

              </div>

              <!-- Footer Tarjeta (Acciones) -->
              <div class="p-3 border-top-1 surface-border flex gap-2 justify-content-end surface-50 border-round-bottom">
                <Button icon="pi pi-pencil" outlined rounded severity="info"
                        @click="abrirEditar(insumo)"
                        :disabled="insumo.esPlantilla"
                        v-tooltip.top="insumo.esPlantilla ? 'Las plantillas no se pueden editar' : 'Editar'" />

                <Button icon="pi pi-trash" outlined rounded severity="danger"
                        @click="confirmarEliminacion(insumo.id!)"
                        :disabled="insumo.esPlantilla"
                        v-tooltip.top="insumo.esPlantilla ? 'Las plantillas no se pueden eliminar' : 'Eliminar'" />
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Formulario Insumo -->
    <InsumoFormDialog v-model:visible="mostrarDialogo"
                      :insumoData="insumoSeleccionado"
                      :loading="guardando"
                      :error="errorDialogo"
                      @guardar="procesarGuardado" />

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import type { IInsumo, IEtiqueta } from '../../interfaces/IInsumo';
  import { insumoService } from '../../services/insumoService';
  import { TIPOS_INSUMO, OBTENER_CLASE_TIPO_INSUMO } from '../../utils/constantes';

  import { useConfirm } from "primevue/useconfirm";
  import ConfirmDialog from 'primevue/confirmdialog';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Select from 'primevue/select';
  import InputText from 'primevue/inputtext';
  import Tag from 'primevue/tag';
  import IconField from 'primevue/iconfield';
  import InputIcon from 'primevue/inputicon';
  import InsumoFormDialog from '../../components/InsumoFormDialog.vue';

  // Estado de la Vista
  const insumos = ref<IInsumo[]>([]);
  const catalogoEtiquetas = ref<IEtiqueta[]>([]);
  const cargando = ref(false);
  const globalError = ref('');

  // Filtros Locales (Reemplazan al filterMode de DataTable)
  const filtroTexto = ref('');
  const filtroTipo = ref<string | null>(null);
  const filtroEtiqueta = ref<string | null>(null);
  const tiposInsumo = ref(TIPOS_INSUMO);
  const mostrarFiltros = ref(window.innerWidth > 768);

  // Estado del Modal
  const mostrarDialogo = ref(false);
  const insumoSeleccionado = ref<IInsumo | null>(null);
  const guardando = ref(false);
  const errorDialogo = ref('');

  const confirm = useConfirm();

  onMounted(async () => {
    await cargarEtiquetas();
    await cargarInsumos();
  });

  const cargarEtiquetas = async () => {
    try {
      catalogoEtiquetas.value = await insumoService.obtenerEtiquetas();
    } catch (err) {
      console.error("No se pudieron pre-cargar las etiquetas", err);
    }
  };

  const cargarInsumos = async () => {
    cargando.value = true;
    globalError.value = '';
    try {
      insumos.value = await insumoService.obtenerTodos();
    } catch (err: any) {
      globalError.value = err.message;
    } finally {
      cargando.value = false;
    }
  };

  // Computada para el buscador responsivo por texto, tipo y etiquetas
  const insumosFiltrados = computed(() => {
    return insumos.value.filter(insumo => {
      let coincideTexto = true;
      let coincideTipo = true;
      let coincideEtiqueta = true;

      if (filtroTexto.value) {
        coincideTexto = insumo.descripcion.toLowerCase().includes(filtroTexto.value.toLowerCase());
      }

      if (filtroTipo.value) {
        coincideTipo = insumo.tipoInsumo === filtroTipo.value;
      }

      if (filtroEtiqueta.value) {
        coincideEtiqueta = insumo.etiquetasIds?.includes(filtroEtiqueta.value) ?? false;
      }

      return coincideTexto && coincideTipo && coincideEtiqueta;
    });
  });

  const abrirNuevo = () => {
    insumoSeleccionado.value = null;
    errorDialogo.value = '';
    mostrarDialogo.value = true;
  };

  const abrirEditar = (insumo: IInsumo) => {
    insumoSeleccionado.value = { ...insumo };
    errorDialogo.value = '';
    mostrarDialogo.value = true;
  };

  const procesarGuardado = async (payload: IInsumo) => {
    guardando.value = true;
    errorDialogo.value = '';

    try {
      if (payload.id) {
        await insumoService.actualizar(payload.id, payload);
      } else {
        await insumoService.crear(payload);
      }
      mostrarDialogo.value = false;
      await cargarEtiquetas();
      await cargarInsumos();
    } catch (err: any) {
      errorDialogo.value = err.message;
    } finally {
      guardando.value = false;
    }
  };

  const confirmarEliminacion = async (id: string) => {
    confirm.require({
      message: '¿Estás seguro de que deseas eliminar este insumo? Esta acción no se puede deshacer.',
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: {
        label: 'Cancelar',
        severity: 'secondary',
        outlined: true
      },
      acceptProps: {
        label: 'Eliminar',
        severity: 'danger'
      },
      accept: async () => {
        try {
          cargando.value = true;
          globalError.value = '';
          await insumoService.eliminar(id);
          await cargarInsumos();
        } catch (err: any) {
          globalError.value = err.message;
        } finally {
          cargando.value = false;
        }
      }
    });
  };

  // Funciones de utilidad visual
  const formatMonedaChilena = (valor: number) => {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(valor);
  };

  const obtenerNombreEtiqueta = (id: string): string => {
    const tag = catalogoEtiquetas.value.find(e => e.id === id);
    return tag ? tag.nombre : 'Etiqueta';
  };

  const obtenerEstiloEtiqueta = (id: string) => {
    const tag = catalogoEtiquetas.value.find(e => e.id === id);
    if (!tag) return { backgroundColor: '#e2e8f0', color: '#475569' };

    return {
      backgroundColor: `${tag.colorHex}20`,
      color: tag.colorHex,
      border: `1px solid ${tag.colorHex}`
    };
  };
</script>

<style scoped>
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
