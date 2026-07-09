<template>
  <div class="flex flex-column gap-4 pb-4">

    <div class="surface-card p-4 shadow-1 border-round">
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3">
          <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center" style="width: 3rem; height: 3rem;">
            <i class="pi pi-users text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-xl font-bold">Directorio de Clientes</h2>
            <span class="text-500 text-sm">Gestiona tus contactos para los presupuestos</span>
          </div>
        </div>

        <Button label="Nuevo Cliente" icon="pi pi-plus" @click="abrirNuevo" class="w-full md:w-auto" />
      </div>

      <Message v-if="globalError" severity="error" :closable="true" @close="globalError = ''" class="mb-3">
        {{ globalError }}
      </Message>

      <DataTable :value="clientes"
                 :loading="cargando"
                 responsiveLayout="scroll"
                 stripedRows
                 class="p-datatable-sm">

        <template #empty>
          <div class="text-center p-4 text-500">
            No se encontraron clientes registrados. Presiona "Nuevo Cliente" para comenzar.
          </div>
        </template>

        <Column field="nombre" header="Nombre / Razón Social" class="font-bold"></Column>
        <Column field="rut" header="RUT"></Column>
        <Column field="correo" header="Correo"></Column>
        <Column field="telefono" header="Teléfono"></Column>

        <Column header="Acciones" :exportable="false" style="min-width: 8rem" alignFrozen="right" frozen>
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button icon="pi pi-pencil" outlined rounded severity="info" @click="abrirEditar(slotProps.data)" />
              <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmarEliminacion(slotProps.data.id)" />
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
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import ClienteFormDialog from '../../components/ClienteFormDialog.vue';

  // Estado de la Vista
  const clientes = ref<ICliente[]>([]);
  const cargando = ref(false);
  const globalError = ref('');

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
      await cargarClientes(); // Recargar la grilla
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
          try {
            await clienteService.eliminar(id);
            await cargarClientes();
          } catch (err: any) {
            globalError.value = err.message;
          } finally {
            cargando.value = false;
          }
        } catch (error) {
          globalError.value = "Error al eliminar cliente";
          console.error("Error al eliminar cliente", error);
        }
      }
    });
  };
</script>
