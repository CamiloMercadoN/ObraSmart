<template>
  <div class="flex flex-column gap-4 pb-4" style="height: calc(100vh - 120px);">

    <div class="surface-card p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">
      <!-- Cabecera y Acciones -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-3 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-users text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-xl font-bold">Directorio de Clientes</h2>
            <span class="text-500 text-sm hidden md:block">Gestiona tus contactos para los presupuestos</span>
          </div>
        </div>

        <div class="flex gap-2 w-full md:w-auto justify-content-end flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Búsqueda' : 'Buscar'"
                  severity="secondary"
                  outlined
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo Cliente" icon="pi pi-plus" @click="abrirNuevo" />
        </div>
      </div>

      <!-- Mensaje de Error -->
      <Message v-if="globalError" severity="error" :closable="true" @close="globalError = ''" class="mb-3">
        {{ globalError }}
      </Message>

      <!-- Tabla de Clientes -->
      <DataTable :value="clientes"
                 :loading="cargando"
                 v-model:filters="filtros"
                 :globalFilterFields="['nombre', 'rut', 'correo', 'telefono']"
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
                         placeholder="Buscar por Nombre, RUT, Correo o Teléfono..."
                         class="w-full" />
            </div>
          </div>
        </template>

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron clientes registrados. Presiona "Nuevo Cliente" para comenzar.
          </div>
        </template>

        <Column field="nombre" header="Nombre / Razón Social" :sortable="true" class="font-bold"></Column>
        <Column field="rut" header="RUT" :sortable="true"></Column>
        <Column field="correo" header="Correo" :sortable="true"></Column>
        <Column field="telefono" header="Teléfono"></Column>

        <Column header="Acciones" :exportable="false" style="min-width: 8rem" alignFrozen="right" frozen>
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded severity="info" @click="abrirEditar(slotProps.data)" v-tooltip.top="'Editar'" />
              <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmarEliminacion(slotProps.data.id)" v-tooltip.top="'Eliminar'" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <ClienteFormDialog v-model:visible="mostrarDialogo"
                       :clienteData="clienteSeleccionado"
                       :loading="guardando"
                       :error="errorDialogo"
                       @guardar="procesarGuardado" />

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import type { ICliente } from '../../interfaces/ICliente';
  import { clienteService } from '../../services/clienteService';

  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import { useConfirm } from "primevue/useconfirm";
  import ConfirmDialog from 'primevue/confirmdialog';
  import { FilterMatchMode } from "@primevue/core/api";
  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import Message from 'primevue/message';
  import ClienteFormDialog from '../../components/ClienteFormDialog.vue';

  // Estado de la Vista
  const clientes = ref<ICliente[]>([]);
  const cargando = ref(false);
  const globalError = ref('');

  // --- Configuración de Filtros ---
  const filtros = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
  });
  const mostrarFiltros = ref(window.innerHeight > 500);
  // ------------------------------------

  // Estado del Modal
  const mostrarDialogo = ref(false);
  const clienteSeleccionado = ref<ICliente | null>(null);
  const guardando = ref(false);
  const errorDialogo = ref('');

  onMounted(() => {
    cargarClientes();
  });

  const cargarClientes = async () => {
    cargando.value = true;
    globalError.value = '';
    try {
      clientes.value = await clienteService.obtenerTodos();
    } catch (err: any) {
      globalError.value = err.message;
    } finally {
      cargando.value = false;
    }
  };

  const abrirNuevo = () => {
    clienteSeleccionado.value = null;
    errorDialogo.value = '';
    mostrarDialogo.value = true;
  };

  const abrirEditar = (cliente: ICliente) => {
    clienteSeleccionado.value = { ...cliente };
    errorDialogo.value = '';
    mostrarDialogo.value = true;
  };

  const procesarGuardado = async (payload: ICliente) => {
    guardando.value = true;
    errorDialogo.value = '';

    try {
      if (payload.id) {
        await clienteService.actualizar(payload.id, payload);
      } else {
        await clienteService.crear(payload);
      }
      mostrarDialogo.value = false;
      await cargarClientes();
    } catch (err: any) {
      errorDialogo.value = err.message;
    } finally {
      guardando.value = false;
    }
  };

  const confirm = useConfirm();

  const confirmarEliminacion = async (id: string) => {
    confirm.require({
      message: '¿Estás seguro de que deseas eliminar este cliente? Esta acción no se puede deshacer.',
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
          await clienteService.eliminar(id);
          await cargarClientes();
        } catch (err: any) {
          globalError.value = err.message;
        } finally {
          cargando.value = false;
        }
      }
    });
  };
</script>

<style scoped>
  /* Ocultar el título principal solo cuando la altura de la pantalla es crítica para priorizar la tabla */
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
