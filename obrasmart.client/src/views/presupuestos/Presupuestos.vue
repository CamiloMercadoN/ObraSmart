<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4 h-full">

    <div class="surface-card p-3 md:p-4 shadow-1 border-round flex flex-column flex-grow-1">

      <!-- Cabecera Responsiva -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-indigo-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-file-edit text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-lg md:text-xl font-bold">Gestión de Presupuestos</h2>
            <span class="text-500 text-sm hidden md:block">Crea, duplica y administra tus cotizaciones comerciales</span>
          </div>
        </div>

        <div class="flex flex-column sm:flex-row gap-2 w-full md:w-auto flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Filtros' : 'Filtros'"
                  severity="secondary"
                  outlined
                  class="w-full sm:w-auto"
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo Presupuesto" icon="pi pi-plus" class="w-full sm:w-auto" @click="abrirFormulario()" />
        </div>
      </div>

      <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="mb-3">
        {{ errorGlobal }}
      </Message>

      <!-- Zona de Filtros -->
      <div v-if="mostrarFiltros" class="flex flex-column md:flex-row gap-3 mb-4 p-3 surface-100 border-round">

        <!-- Buscador con flex-1 y IconField -->
        <div class="flex-1 w-full">
          <IconField class="w-full">
            <InputIcon class="pi pi-search" />
            <InputText v-model="filtroTexto" placeholder="Buscar por proyecto o cliente..." class="w-full" />
          </IconField>
        </div>

        <!-- Selector con ancho fijo en escritorio (15rem) -->
        <div class="w-full md:w-15rem flex-shrink-0">
          <Select v-model="filtroEstado" :options="estadosPermitidos" placeholder="Filtrar por Estado" showClear class="w-full" />
        </div>

      </div>

      <!-- Vista de Tarjetas (DataView) -->
      <div class="flex-grow-1 overflow-auto">

        <div v-if="cargando" class="flex justify-content-center align-items-center p-5">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>

        <div v-else-if="presupuestosFiltrados.length === 0" class="text-center p-5 text-500 border-round surface-100 border-1 surface-border border-dashed">
          No se encontraron presupuestos que coincidan con la búsqueda.
        </div>

        <div v-else class="grid">
          <div v-for="presupuesto in presupuestosFiltrados" :key="presupuesto.id" class="col-12 md:col-6 lg:col-4">
            <!-- Tarjeta Individual -->
            <div class="surface-card border-1 surface-border border-round shadow-1 flex flex-column h-full">

              <!-- Header Tarjeta -->
              <div class="flex justify-content-between align-items-start p-3 border-bottom-1 surface-border surface-50 border-round-top">
                <div class="flex flex-column pr-2">
                  <span class="text-900 font-bold text-lg line-height-2 mb-1" style="word-break: break-word;">
                    {{ presupuesto.nombreProyecto }}
                  </span>
                  <span class="text-600 text-sm flex align-items-center gap-1">
                    <i class="pi pi-user text-xs"></i>
                    {{ presupuesto.clienteNombre || 'Sin Cliente Asignado' }}
                  </span>
                </div>
                <div class="flex flex-column align-items-end gap-1 flex-shrink-0">
                  <Tag :value="presupuesto.estado" :severity="obtenerSeveridadEstado(presupuesto.estado)" />
                  <Tag v-if="presupuesto.esPlantilla" value="Plantilla" severity="info" class="text-xs" />
                </div>
              </div>

              <!-- Body Tarjeta -->
              <div class="p-3 flex-grow-1 flex flex-column gap-2 justify-content-center">
                <div class="flex justify-content-between align-items-center">
                  <span class="text-500 text-sm">Fecha Creación:</span>
                  <span class="text-700 text-sm font-semibold">{{ formatearFecha(presupuesto.fechaCreacion) }}</span>
                </div>
                <div class="flex justify-content-between align-items-center">
                  <span class="text-500 text-sm">Subtotal:</span>
                  <span class="text-700 text-sm">$ {{ formatearMoneda(presupuesto.subtotal) }}</span>
                </div>
                <div class="flex justify-content-between align-items-center border-bottom-1 surface-border pb-2">
                  <span class="text-500 text-sm">IVA (19%):</span>
                  <span class="text-700 text-sm">$ {{ formatearMoneda(presupuesto.montoIva) }}</span>
                </div>
                <div class="flex justify-content-between align-items-center pt-1">
                  <span class="text-700 font-bold">Total:</span>
                  <span class="text-green-600 font-bold text-xl">$ {{ formatearMoneda(presupuesto.total) }}</span>
                </div>
              </div>

              <div class="p-3 border-top-1 surface-border flex gap-2 justify-content-end surface-50 border-round-bottom">
                <Button icon="pi pi-pencil" outlined rounded severity="info"
                        @click="abrirFormulario(presupuesto.id)"
                        :disabled="presupuesto.esPlantilla"
                        v-tooltip.top="presupuesto.esPlantilla ? 'Las plantillas no se editan' : 'Editar'" />

                <Button icon="pi pi-copy" outlined rounded severity="secondary"
                        @click="duplicarPresupuesto(presupuesto.id!)"
                        v-tooltip.top="'Duplicar a nuevo'" />

                <Button icon="pi pi-trash" outlined rounded severity="danger"
                        @click="confirmarEliminacion(presupuesto)"
                        :disabled="presupuesto.esPlantilla || presupuesto.estado !== 'Borrador'"
                        v-tooltip.top="'Eliminar'" />
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { presupuestoService } from '../../services/presupuestoService';
  import type { IPresupuesto } from '../../interfaces/IPresupuesto';

  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import Tag from 'primevue/tag';
  import IconField from 'primevue/iconfield';
  import InputIcon from 'primevue/inputicon';
  import { useConfirm } from 'primevue/useconfirm';
  import ConfirmDialog from 'primevue/confirmdialog';

  const router = useRouter();
  const confirm = useConfirm();

  const presupuestos = ref<IPresupuesto[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');

  // Filtros Locales
  const filtroTexto = ref('');
  const filtroEstado = ref<string | null>(null);
  const estadosPermitidos = ref(['Borrador', 'Emitido', 'Aprobado', 'Rechazado']);
  const mostrarFiltros = ref(window.innerWidth > 768);

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

  const presupuestosFiltrados = computed(() => {
    return presupuestos.value.filter(p => {
      let coincideTexto = true;
      let coincideEstado = true;

      if (filtroTexto.value) {
        const busqueda = filtroTexto.value.toLowerCase();
        const proyecto = p.nombreProyecto?.toLowerCase() || '';
        const cliente = p.clienteNombre?.toLowerCase() || '';
        coincideTexto = proyecto.includes(busqueda) || cliente.includes(busqueda);
      }

      if (filtroEstado.value) {
        coincideEstado = p.estado === filtroEstado.value;
      }

      return coincideTexto && coincideEstado;
    });
  });

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
    return new Date(fechaStr).toLocaleDateString('es-CL', { timeZone: 'America/Santiago' });
  };

  const obtenerSeveridadEstado = (estado?: string) => {
    switch (estado) {
      case 'Borrador': return 'secondary';
      case 'Emitido': return 'info';
      case 'Aprobado': return 'success';
      case 'Rechazado': return 'danger';
      default: return 'secondary';
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
