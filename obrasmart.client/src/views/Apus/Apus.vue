<template>
  <div class="flex flex-column gap-4 pb-4" style="height: calc(100vh - 120px);">

    <div class="surface-card p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">
      <!-- Cabecera y Acciones -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-3 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-primary text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-book text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-xl font-bold">Catálogo de APUs</h2>
            <span class="text-500 text-sm hidden md:block">Gestiona tus análisis de precios unitarios base</span>
          </div>
        </div>

        <div class="flex gap-2 w-full md:w-auto justify-content-end flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Filtros' : 'Filtros'"
                  severity="secondary"
                  outlined
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo APU" icon="pi pi-plus" @click="abrirFormulario()" />
        </div>
      </div>

      <!-- Mensaje de Error -->
      <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="mb-3">
        {{ errorGlobal }}
      </Message>

      <!-- Tabla de APUs -->
      <DataTable :value="apus"
                 :loading="cargando"
                 v-model:filters="filtros"
                 :globalFilterFields="['nombre']"
                 responsiveLayout="scroll"
                 stripedRows
                 class="p-datatable-sm flex flex-column flex-grow-1"
                 style="min-height: 0;"
                 scrollable
                 scrollHeight="flex"
                 :virtualScroll="true"
                 :rows="25">

        <template #header>
          <div v-if="mostrarFiltros" class="flex flex-column md:flex-row justify-content-between gap-3 transition-duration-200">
            <div class="w-full md:w-25rem">
              <InputText v-model="filtros['global'].value"
                         placeholder="Buscar por nombre..."
                         class="w-full" />
            </div>

            <div class="flex flex-column md:flex-row gap-2 w-full md:w-auto">
              <Select v-model="filtros['etiquetasIds'].value"
                      :options="catalogoEtiquetas"
                      optionLabel="nombre"
                      optionValue="id"
                      placeholder="Filtrar por Etiqueta"
                      showClear
                      class="w-full md:w-15rem" />
            </div>
          </div>
        </template>

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron estructuras APU. Crea la primera para comenzar.
          </div>
        </template>

        <Column field="nombre" header="Nombre" :sortable="true" class="font-bold"></Column>

        <Column field="unidadMedidaNombre" header="Unidad" :sortable="true"></Column>

        <Column header="Costo Calculado" :sortable="true" sortField="costoTotalCalculado">
          <template #body="slotProps">
            <span class="font-bold text-green-600">
              $ {{ formatearMoneda(slotProps.data.costoTotalCalculado) }}
            </span>
          </template>
        </Column>

        <Column header="Etiquetas" style="min-width: 12rem">
          <template #body="slotProps">
            <div class="flex flex-wrap gap-1">
              <span v-for="tagId in slotProps.data.etiquetasIds"
                    :key="tagId"
                    class="text-xs px-2 py-1 border-round font-semibold"
                    :style="obtenerEstiloEtiqueta(tagId)">
                {{ obtenerNombreEtiqueta(tagId) }}
              </span>
            </div>
          </template>
        </Column>

        <Column field="esPlantilla" header="Tipo">
          <template #body="slotProps">
            <span v-if="slotProps.data.esPlantilla" class="text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1">
              Plantilla Sistema
            </span>
            <span v-else class="text-xs font-bold text-green-600 bg-green-50 border-round px-2 py-1">
              Personalizado
            </span>
          </template>
        </Column>

        <Column header="Acciones" :exportable="false" style="min-width: 12rem" alignFrozen="right" frozen>
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded severity="info" class="mr-2"
                      @click="abrirFormulario(slotProps.data.id)"
                      :disabled="slotProps.data.esPlantilla"
                      v-tooltip.top="'Editar'" />

              <Button icon="pi pi-refresh" outlined rounded severity="secondary" class="mr-2"
                      @click="recalcularCosto(slotProps.data)"
                      :disabled="slotProps.data.esPlantilla || recargandoId === slotProps.data.id"
                      :loading="recargandoId === slotProps.data.id"
                      v-tooltip.top="'Recalcular costo con precios actuales'" />

              <Button icon="pi pi-trash" outlined rounded severity="danger"
                      @click="confirmarEliminacion(slotProps.data)"
                      :disabled="slotProps.data.esPlantilla"
                      v-tooltip.top="'Eliminar'" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { apuService } from '../../services/apuService';
  import { insumoService } from '../../services/insumoService';
  import type { IEstructuraAPU } from '../../interfaces/IApu';
  import type { IEtiqueta } from '../../interfaces/IInsumo';

  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import { useConfirm } from 'primevue/useconfirm';
  import ConfirmDialog from 'primevue/confirmdialog';
  import { FilterMatchMode } from "@primevue/core/api";

  const router = useRouter();
  const confirm = useConfirm();

  // Estado de la Vista
  const apus = ref<IEstructuraAPU[]>([]);
  const catalogoEtiquetas = ref<IEtiqueta[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');
  const recargandoId = ref<string | null>(null);

  // Configuración de Filtros
  const filtros = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    etiquetasIds: { value: null, matchMode: FilterMatchMode.CONTAINS }
  });
  const mostrarFiltros = ref(window.innerHeight > 500);

  onMounted(async () => {
    await cargarEtiquetas();
    await cargarApus();
  });

  const cargarEtiquetas = async () => {
    try {
      catalogoEtiquetas.value = await insumoService.obtenerEtiquetas();
    } catch (err) {
      console.error("No se pudieron pre-cargar las etiquetas", err);
    }
  };

  const cargarApus = async () => {
    cargando.value = true;
    errorGlobal.value = '';
    try {
      apus.value = await apuService.obtenerTodos();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      cargando.value = false;
    }
  };

  const abrirFormulario = (id?: string) => {
    if (id) {
      router.push(`/apus/editar/${id}`);
    } else {
      router.push('/apus/crear');
    }
  };

  const recalcularCosto = async (apu: IEstructuraAPU) => {
    recargandoId.value = apu.id;
    errorGlobal.value = '';
    try {
      await apuService.recalcularCosto(apu.id);
      await cargarApus();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      recargandoId.value = null;
    }
  };

  const confirmarEliminacion = (apu: IEstructuraAPU) => {
    confirm.require({
      message: `¿Estás seguro de eliminar el APU "${apu.nombre}"? Esta acción no se puede deshacer.`,
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
          await apuService.eliminar(apu.id);
          await cargarApus();
        } catch (err: any) {
          errorGlobal.value = err.message;
          cargando.value = false; // Se detiene aquí si hay error para no ocultar la tabla infinitamente
        }
      }
    });
  };

  // Funciones de utilidad visual
  const formatearMoneda = (valor: number) => {
    return valor.toLocaleString('es-CL');
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
  /* Ocultar el título principal solo cuando la altura de la pantalla es crítica (ej. móviles en horizontal) para priorizar la tabla */
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
