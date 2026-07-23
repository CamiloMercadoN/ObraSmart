<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4 h-full">

    <div class="surface-card p-3 md:p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">

      <!-- Cabecera Responsiva -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-users text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-lg md:text-xl font-bold">Directorio de Clientes</h2>
            <span class="text-500 text-sm hidden md:block">Gestiona tus contactos para los presupuestos</span>
          </div>
        </div>

        <div class="flex flex-column sm:flex-row gap-2 w-full md:w-auto flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Búsqueda' : 'Buscar'"
                  severity="secondary"
                  outlined
                  class="w-full sm:w-auto"
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo Cliente" icon="pi pi-plus" class="w-full sm:w-auto" @click="abrirNuevo" />
        </div>
      </div>

      <!-- Mensaje de Error -->
      <Message v-if="globalError" severity="error" :closable="true" @close="globalError = ''" class="mb-3">
        {{ globalError }}
      </Message>

      <!-- Zona de Filtros -->
      <div v-if="mostrarFiltros" class="flex mb-4 p-3 surface-100 border-round">
        <!-- Buscador con flex-1 y IconField -->
        <div class="w-full">
          <IconField class="w-full">
            <InputIcon class="pi pi-search" />
            <InputText v-model="filtroTexto" placeholder="Buscar por Nombre, RUT, Correo o Teléfono..." class="w-full" />
          </IconField>
        </div>
      </div>

      <!-- Vista de Tarjetas (DataView) -->
      <div class="flex-grow-1 overflow-auto">

        <div v-if="cargando" class="flex justify-content-center align-items-center p-5">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>

        <div v-else-if="clientesFiltrados.length === 0" class="text-center p-5 text-500 border-round surface-100 border-1 surface-border border-dashed">
          No se encontraron clientes registrados o que coincidan con la búsqueda.
        </div>

        <div v-else class="grid">
          <div v-for="cliente in clientesFiltrados" :key="cliente.id" class="col-12 md:col-6 lg:col-4 xl:col-3">

            <!-- Tarjeta Individual -->
            <div class="surface-card border-1 surface-border border-round shadow-1 flex flex-column h-full">

              <!-- Header Tarjeta -->
              <div class="p-3 border-bottom-1 surface-border surface-50 border-round-top">
                <span class="text-900 font-bold text-lg line-height-2" style="word-break: break-word;">
                  {{ cliente.nombre }}
                </span>
              </div>

              <!-- Body Tarjeta -->
              <div class="p-3 flex-grow-1 flex flex-column gap-3 justify-content-center">

                <div class="flex align-items-center gap-3">
                  <i class="pi pi-id-card text-500 text-lg"></i>
                  <span class="text-700 font-semibold">{{ cliente.rut || 'Sin RUT' }}</span>
                </div>

                <div class="flex align-items-center gap-3">
                  <i class="pi pi-envelope text-500 text-lg"></i>
                  <span class="text-700 text-sm overflow-hidden text-overflow-ellipsis" :title="cliente.correo">
                    {{ cliente.correo || 'Sin correo' }}
                  </span>
                </div>

                <div class="flex align-items-center gap-3">
                  <i class="pi pi-phone text-500 text-lg"></i>
                  <span class="text-700">{{ cliente.telefono || 'Sin teléfono' }}</span>
                </div>

              </div>

              <!-- Footer Tarjeta (Acciones) -->
              <div class="p-3 border-top-1 surface-border flex gap-2 justify-content-end surface-50 border-round-bottom">
                <Button icon="pi pi-pencil" outlined rounded severity="info"
                        @click="abrirEditar(cliente)"
                        v-tooltip.top="'Editar Cliente'" />

                <Button icon="pi pi-trash" outlined rounded severity="danger"
                        @click="confirmarEliminacion(cliente.id!)"
                        v-tooltip.top="'Eliminar Cliente'" />
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Formulario Cliente -->
    <ClienteFormDialog v-model:visible="mostrarDialogo"
                       :clienteData="clienteSeleccionado"
                       :loading="guardando"
                       :error="errorDialogo"
                       @guardar="procesarGuardado" />

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import type { ICliente } from '../../interfaces/ICliente';
  import { clienteService } from '../../services/clienteService';

  import { useConfirm } from "primevue/useconfirm";
  import ConfirmDialog from 'primevue/confirmdialog';
  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import Message from 'primevue/message';
  import IconField from 'primevue/iconfield';
  import InputIcon from 'primevue/inputicon';
  import ClienteFormDialog from '../../components/ClienteFormDialog.vue';

  // Estado de la Vista
  const clientes = ref<ICliente[]>([]);
  const cargando = ref(false);
  const globalError = ref('');

  // Filtros Locales
  const filtroTexto = ref('');
  const mostrarFiltros = ref(window.innerWidth > 768);

  // Estado del Modal
  const mostrarDialogo = ref(false);
  const clienteSeleccionado = ref<ICliente | null>(null);
  const guardando = ref(false);
  const errorDialogo = ref('');

  const confirm = useConfirm();

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

  // Computada para el buscador responsivo
  const clientesFiltrados = computed(() => {
    if (!filtroTexto.value) return clientes.value;

    const busqueda = filtroTexto.value.toLowerCase();

    return clientes.value.filter(c => {
      return (
        (c.nombre?.toLowerCase().includes(busqueda)) ||
        (c.rut?.toLowerCase().includes(busqueda)) ||
        (c.correo?.toLowerCase().includes(busqueda)) ||
        (c.telefono?.toLowerCase().includes(busqueda))
      );
    });
  });

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
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
