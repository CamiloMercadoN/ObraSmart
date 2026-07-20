<template>
  <div class="flex flex-column gap-4 pb-4">

    <!-- Cabecera -->
    <div class="flex justify-content-between align-items-center">
      <div class="flex align-items-center gap-3">
        <div class="bg-primary text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
          <i class="pi pi-book text-xl"></i>
        </div>
        <div>
          <h2 class="m-0 text-900 text-xl font-bold">Catálogo de APUs</h2>
          <span class="text-500 text-sm">Gestiona tus análisis de precios unitarios base</span>
        </div>
      </div>
      <Button label="Nuevo APU" icon="pi pi-plus" @click="abrirFormulario()" />
    </div>

    <!-- Mensaje de Error -->
    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="m-0">
      {{ errorGlobal }}
    </Message>

    <!-- Tabla de APUs -->
    <div class="surface-card p-4 shadow-1 border-round">
      <DataTable :value="apus" :loading="cargando" responsiveLayout="scroll" :paginator="true" :rows="10">

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron estructuras APU. Crea la primera para comenzar.
          </div>
        </template>

        <Column field="nombre" header="Nombre" :sortable="true"></Column>
        <Column field="unidadMedidaNombre" header="Unidad" :sortable="true"></Column>

        <Column header="Costo Calculado" :sortable="true" sortField="costoTotalCalculado">
          <template #body="slotProps">
            <span class="font-bold text-green-600">
              $ {{ formatearMoneda(slotProps.data.costoTotalCalculado) }}
            </span>
          </template>
        </Column>

        <Column field="esPlantilla" header="Tipo">
          <template #body="slotProps">
            <Tag :severity="slotProps.data.esPlantilla ? 'info' : 'success'"
                 :value="slotProps.data.esPlantilla ? 'Plantilla Sistema' : 'Personalizado'" />
          </template>
        </Column>

        <Column header="Acciones" :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded class="mr-2"
                      @click="abrirFormulario(slotProps.data.id)"
                      :disabled="slotProps.data.esPlantilla"
                      v-tooltip.top="'Editar'" />

              <Button icon="pi pi-refresh" outlined rounded severity="info" class="mr-2"
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
  import type { IEstructuraAPU } from '../../interfaces/IApu';

  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Button from 'primevue/button';
  import Tag from 'primevue/tag';
  import Message from 'primevue/message';
  import { useConfirm } from 'primevue/useconfirm';
  import ConfirmDialog from 'primevue/confirmdialog';

  const router = useRouter();
  const confirm = useConfirm();

  const apus = ref<IEstructuraAPU[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');
  const recargandoId = ref<string | null>(null);

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

  onMounted(() => {
    cargarApus();
  });

  const formatearMoneda = (valor: number) => {
    return valor.toLocaleString('es-CL');
  };

  const abrirFormulario = (id?: string) => {
    if (id) {
      // Redirigir a vista de edición
      router.push(`/apus/editar/${id}`);
    } else {
      // Redirigir a vista de creación
      router.push('/apus/crear');
    }
  };

  const recalcularCosto = async (apu: IEstructuraAPU) => {
    recargandoId.value = apu.id;
    errorGlobal.value = '';
    try {
      await apuService.recalcularCosto(apu.id);
      await cargarApus(); // Recargar la lista para ver el nuevo precio
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
          await eliminarApu(apu.id);
        } catch (err: any) {
          errorGlobal.value = err.message;
        } finally {
          cargando.value = false;
        }
      }
    });
  };

  const eliminarApu = async (id: string) => {
    errorGlobal.value = '';
    try {
      await apuService.eliminar(id);
      await cargarApus();
    } catch (error: any) {
      errorGlobal.value = error.message;
    }
  };
</script>
