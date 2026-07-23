<template>
  <div class="flex flex-column gap-4 pb-4" style="height: calc(100vh - 120px);">

    <div class="surface-card p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">
      <!-- Cabecera -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-3 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-indigo-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-file-invoice text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-xl font-bold">Gestión de Presupuestos</h2>
            <span class="text-500 text-sm hidden md:block">Crea, duplica y administra tus cotizaciones comerciales</span>
          </div>
        </div>

        <div class="flex gap-2 w-full md:w-auto justify-content-end flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Búsqueda' : 'Buscar'"
                  severity="secondary"
                  outlined
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo Presupuesto" icon="pi pi-plus" @click="abrirFormulario()" />
        </div>
      </div>

      <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="mb-3">
        {{ errorGlobal }}
      </Message>

      <!-- Tabla -->
      <DataTable :value="presupuestos"
                 :loading="cargando"
                 v-model:filters="filtros"
                 :globalFilterFields="['nombreProyecto', 'clienteNombre']"
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
            <div class="w-full md:w-20rem">
              <InputText v-model="filtros['global'].value"
                         placeholder="Buscar por proyecto o cliente..."
                         class="w-full" />
            </div>

            <div class="w-full md:w-15rem">
              <Select v-model="filtros['estado'].value"
                      :options="estadosPermitidos"
                      placeholder="Filtrar por Estado"
                      showClear
                      class="w-full" />
            </div>
          </div>
        </template>

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron presupuestos. Presiona "Nuevo Presupuesto" para comenzar.
          </div>
        </template>

        <Column field="nombreProyecto" header="Proyecto" :sortable="true" class="font-bold">
          <template #body="slotProps">
            <div class="flex align-items-center gap-2">
              {{ slotProps.data.nombreProyecto }}
              <span v-if="slotProps.data.esPlantilla" class="text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1" v-tooltip.top="'Plantilla del Sistema'">
                Plantilla
              </span>
            </div>
          </template>
        </Column>
        <Column field="clienteNombre" header="Cliente" :sortable="true"></Column>
        <Column field="fechaCreacion" header="Fecha" :sortable="true">
          <template #body="slotProps">
            {{ formatearFecha(slotProps.data.fechaCreacion) }}
          </template>
        </Column>

        <Column field="estado" header="Estado" :sortable="true">
          <template #body="slotProps">
            <span :class="obtenerClaseEstado(slotProps.data.estado)">
              {{ slotProps.data.estado }}
            </span>
          </template>
        </Column>

        <Column header="Subtotal" :sortable="true" sortField="subtotal">
          <template #body="slotProps">
            <span>
              $ {{ formatearMoneda(slotProps.data.subtotal) }}
            </span>
          </template>
        </Column>

        <Column header="IVA" :sortable="true" sortField="montoIva">
          <template #body="slotProps">
            <span class="text-500">
              $ {{ formatearMoneda(slotProps.data.montoIva) }}
            </span>
          </template>
        </Column>

        <Column header="Total" :sortable="true" sortField="total">
          <template #body="slotProps">
            <span class="font-bold text-green-600">
              $ {{ formatearMoneda(slotProps.data.total) }}
            </span>
          </template>
        </Column>

        <Column header="Acciones" :exportable="false" style="min-width: 12rem" alignFrozen="right" frozen>
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded severity="info"
                      @click="abrirFormulario(slotProps.data.id)"
                      :disabled="slotProps.data.esPlantilla"
                      v-tooltip.top="slotProps.data.esPlantilla ? 'Las plantillas no se pueden editar' : 'Editar'" />

              <Button icon="pi pi-copy" outlined rounded severity="secondary"
                      @click="duplicarPresupuesto(slotProps.data.id)"
                      v-tooltip.top="'Duplicar'" />

              <Button icon="pi pi-trash" outlined rounded severity="danger"
                      @click="confirmarEliminacion(slotProps.data)"
                      :disabled="slotProps.data.esPlantilla || slotProps.data.estado !== 'Borrador'"
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
  import { presupuestoService } from '../../services/presupuestoService';
  import type { IPresupuesto } from '../../interfaces/IPresupuesto';

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

  const presupuestos = ref<IPresupuesto[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');

  const filtros = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    estado: { value: null, matchMode: FilterMatchMode.EQUALS }
  });
  const estadosPermitidos = ref(['Borrador', 'Emitido', 'Aprobado', 'Rechazado']);
  const mostrarFiltros = ref(window.innerHeight > 500);

  onMounted(() => {
    cargarPresupuestos();
  });

  const cargarPresupuestos = async () => {
    cargando.value = true;
    errorGlobal.value = '';
    try {
      presupuestos.value = await presupuestoService.obtenerTodos();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      cargando.value = false;
    }
  };

  const abrirFormulario = (id?: string) => {
    if (id) router.push(`/presupuestos/editar/${id}`);
    else router.push('/presupuestos/crear');
  };

  const duplicarPresupuesto = (id: string) => {
    router.push({ path: '/presupuestos/crear', query: { cloneId: id } });
  };

  const confirmarEliminacion = (presupuesto: IPresupuesto) => {
    confirm.require({
      message: `¿Estás seguro de eliminar el presupuesto "${presupuesto.nombreProyecto}"?`,
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: { label: 'Cancelar', severity: 'secondary', outlined: true },
      acceptProps: { label: 'Eliminar', severity: 'danger' },
      accept: async () => {
        try {
          cargando.value = true;
          await presupuestoService.eliminar(presupuesto.id!);
          await cargarPresupuestos();
        } catch (err: any) {
          errorGlobal.value = err.message;
          cargando.value = false;
        }
      }
    });
  };

  const formatearMoneda = (valor?: number) => {
    if (valor === undefined) return '0';
    return valor.toLocaleString('es-CL');
  };

  const formatearFecha = (fechaStr?: string) => {
    if (!fechaStr) return '';
    return new Date(fechaStr).toLocaleDateString('es-CL');
  };

  const obtenerClaseEstado = (estado?: string) => {
    switch (estado) {
      case 'Borrador': return 'text-xs font-bold text-gray-600 bg-gray-100 border-round px-2 py-1';
      case 'Emitido': return 'text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1';
      case 'Aprobado': return 'text-xs font-bold text-green-600 bg-green-50 border-round px-2 py-1';
      case 'Rechazado': return 'text-xs font-bold text-red-600 bg-red-50 border-round px-2 py-1';
      default: return 'text-xs font-bold text-500 bg-100 border-round px-2 py-1';
    }
  };
</script>

<style scoped>
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
