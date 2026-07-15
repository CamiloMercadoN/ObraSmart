<template>
  <div class="flex flex-column gap-4 pb-4">

    <div class="surface-card p-4 shadow-1 border-round">
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3">
          <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center" style="width: 3rem; height: 3rem;">
            <i class="pi pi-box text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-xl font-bold">Catálogo de Insumos</h2>
            <span class="text-500 text-sm">Administra los materiales, mano de obra y equipos</span>
          </div>
        </div>

        <button-prime label="Nuevo Insumo" icon="pi pi-plus" @click="abrirNuevo" class="w-full md:w-auto p-button" />
      </div>

      <Message v-if="globalError" severity="error" :closable="true" @close="globalError = ''" class="mb-3">
        {{ globalError }}
      </Message>

      <DataTable :value="insumos"
                 :loading="cargando"
                 responsiveLayout="scroll"
                 stripedRows
                 class="p-datatable-sm">

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron insumos registrados. Presiona "Nuevo Insumo" para comenzar.
          </div>
        </template>

        <Column field="descripcion" header="Descripción" class="font-bold"></Column>

        <Column field="tipoInsumo" header="Tipo">
          <template #body="slotProps">
            <span :class="obtenerBadgeClaseTipo(slotProps.data.tipoInsumo)">
              {{ slotProps.data.tipoInsumo }}
            </span>
          </template>
        </Column>

        <Column field="precioReferencia" header="Precio Referencia">
          <template #body="slotProps">
            {{ formatMonedaChilena(slotProps.data.precioReferencia) }}
          </template>
        </Column>

        <Column field="unidadMedidaNombre" header="Unidad"></Column>

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

        <Column field="esPlantilla" header="Origen">
          <template #body="slotProps">
            <span v-if="slotProps.data.esPlantilla" class="text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1">
              Plantilla Sistema
            </span>
            <span v-else class="text-xs font-bold text-green-600 bg-green-50 border-round px-2 py-1">
              Usuario
            </span>
          </template>
        </Column>

        <Column header="Acciones" :exportable="false" style="min-width: 8rem" alignFrozen="right" frozen>
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded severity="info" @click="abrirEditar(slotProps.data)" :disabled="slotProps.data.esPlantilla" />
              <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmarEliminacion(slotProps.data.id)" :disabled="slotProps.data.esPlantilla" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <InsumoFormDialog v-model:visible="mostrarDialogo"
                      :insumoData="insumoSeleccionado"
                      :loading="guardando"
                      :error="errorDialogo"
                      @guardar="procesarGuardado" />

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import type { IInsumo, IEtiqueta } from '../../interfaces/IInsumo';
  import { insumoService } from '../../services/insumoService';

  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import { useConfirm } from "primevue/useconfirm";
  import ConfirmDialog from 'primevue/confirmdialog';
  import Button from 'primevue/button';

  import ButtonPrime from 'primevue/button';
  import Message from 'primevue/message';
  import InsumoFormDialog from '../../components/InsumoFormDialog.vue';

  // Estado de la Vista
  const insumos = ref<IInsumo[]>([]);
  const catalogoEtiquetas = ref<IEtiqueta[]>([]);
  const cargando = ref(false);
  const globalError = ref('');

  // Estado del Modal
  const mostrarDialogo = ref(false);
  const insumoSeleccionado = ref<IInsumo | null>(null);
  const guardando = ref(false);
  const errorDialogo = ref('');

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
      await cargarInsumos();
    } catch (err: any) {
      errorDialogo.value = err.message;
    } finally {
      guardando.value = false;
    }
  };

  const confirm = useConfirm();

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

  const obtenerBadgeClaseTipo = (tipo: string) => {
    switch (tipo) {
      case 'Material': return 'text-xs font-bold text-orange-600 bg-orange-50 border-round px-2 py-1';
      case 'Mano de Obra': return 'text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1';
      case 'Equipo': return 'text-xs font-bold text-purple-600 bg-purple-50 border-round px-2 py-1';
      default: return 'text-xs font-bold text-500 bg-100 border-round px-2 py-1';
    }
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
