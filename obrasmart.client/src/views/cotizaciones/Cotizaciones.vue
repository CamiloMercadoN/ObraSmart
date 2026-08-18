<template>
  <div class="app-panel p-3 md:p-4 flex flex-column flex-grow-1">

    <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
      <div class="flex align-items-center gap-3 titulo-mantenedor">
        <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
          <i class="pi pi-send text-xl"></i>
        </div>
        <div>
          <h2 class="m-0 app-text text-lg md:text-xl font-bold">Gestión de Cotizaciones</h2>
          <span class="app-text-muted text-sm hidden md:block">Administra el ciclo comercial y comparte tus propuestas</span>
        </div>
      </div>

      <div class="flex gap-2 w-full md:w-auto flex-shrink-0">
        <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                :label="mostrarFiltros ? 'Ocultar' : 'Filtros'"
                severity="secondary" outlined class="w-full sm:w-auto"
                @click="mostrarFiltros = !mostrarFiltros" />
      </div>
    </div>

    <Message v-if="errorGlobal" severity="error" :closable="true" class="mb-3" @close="errorGlobal = ''">{{ errorGlobal }}</Message>

    <!-- Zona de Filtros -->
    <div v-if="mostrarFiltros" class="app-subcard flex flex-column md:flex-row gap-3 mb-4 p-3">
      <IconField class="flex-1 w-full">
        <InputIcon class="pi pi-search" />
        <InputText v-model="filtroTexto" placeholder="Buscar por N° o Cliente..." class="w-full" />
      </IconField>
      <Select v-model="filtroEstado" :options="estadosPermitidos" placeholder="Estado" showClear class="w-full md:w-15rem" />
    </div>

    <!-- Lista de Cotizaciones -->
    <div class="flex-grow-1 overflow-auto">
      <div v-if="cargando" class="flex justify-content-center p-5"><i class="pi pi-spin pi-spinner text-4xl text-primary"></i></div>
      <div v-else-if="cotizacionesFiltradas.length === 0" class="app-subcard app-text-muted text-center p-5 border-1 app-border-color border-dashed">
        No se encontraron cotizaciones. Ve a Presupuestos para generar una.
      </div>

      <div v-else class="grid">
        <div v-for="cot in cotizacionesFiltradas" :key="cot.id" class="col-12 md:col-6 lg:col-4">
          <div class="app-card flex flex-column h-full">

            <div class="app-surface-subtle flex justify-content-between align-items-start p-3 border-bottom-1 app-border-color border-round-top">
              <div class="flex flex-column pr-2">
                <span class="app-text font-bold text-lg mb-1">{{ cot.numeroCotizacion }} - {{ cot.nombreProyecto }}</span>
                <span class="app-text-muted text-sm"><i class="pi pi-user text-xs"></i> {{ cot.clienteNombre }}</span>
                <span class="app-text-muted text-sm mt-1"><i class="pi pi-calendar text-xs"></i> Emisión: {{ formatearFecha(cot.fechaEmision) }}</span>
              </div>
              <Tag :value="cot.estado" :severity="obtenerSeveridad(cot.estado)" />
            </div>

            <div class="p-3 flex-grow-1 flex flex-column gap-2">
              <div class="flex justify-content-between">
                <span class="app-text-muted text-sm">Vencimiento:</span>
                <span class="app-text text-sm font-semibold" :class="{'text-red-500': cot.estado === 'Vencida'}">{{ formatearFecha(cot.fechaVencimiento) }}</span>
              </div>
            </div>

            <div class="app-surface-subtle p-3 border-top-1 app-border-color flex gap-2 justify-content-end border-round-bottom">
              <Button icon="pi pi-sync" outlined rounded severity="info" v-tooltip.top="'Cambiar Estado'" @click="abrirModalEstado(cot)" />
              <Button icon="pi pi-eye" outlined rounded severity="primary" v-tooltip.top="'Ver Cotización'" @click="abrirVistaPrevia(cot)" />
              <Button icon="pi pi-trash" outlined rounded severity="danger" v-tooltip.top="'Eliminar'" v-if="cot.estado === 'Borrador'" @click="confirmarEliminacion(cot)" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Cambio de Estado -->
    <Dialog v-model:visible="mostrarModalEstado" modal header="Actualizar Cotización" :style="{ width: '90vw', maxWidth: '400px' }">
      <div v-if="cotizacionEnEdicion" class="flex flex-column gap-3 pt-2">
        <div class="field">
          <label class="font-bold app-text mb-2 block">Estado Actual</label>
          <Select v-model="nuevoEstado" :options="estadosPermitidos" class="w-full" :disabled="cotizacionEnEdicion.estado === 'Aceptada' || cotizacionEnEdicion.estado === 'Rechazada'" />
        </div>
        <div class="field" v-if="nuevoEstado === 'Vencida' || cotizacionEnEdicion.estado === 'Vencida'">
          <label class="font-bold app-text mb-2 block">Extender Vigencia (Opcional)</label>
          <DatePicker v-model="nuevaVigencia" dateFormat="dd/mm/yy" class="w-full" :minDate="new Date()" />
        </div>
      </div>
      <template #footer>
        <Button label="Guardar" icon="pi pi-check" @click="guardarEstado" :loading="guardandoEstado" />
      </template>
    </Dialog>

    <VistaPreviaCotizacion v-if="cotizacionSeleccionada"
                           v-model:visible="mostrarVistaPrevia"
                           :cotizacion="cotizacionSeleccionada"
                           @recargar="cargarCotizaciones"/>

  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import { cotizacionService } from '../../services/cotizacionService';
  import type { ICotizacion } from '../../interfaces/ICotizacion';
  import VistaPreviaCotizacion from '../../components/VistaPreviaCotizacion.vue';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import Tag from 'primevue/tag';
  import IconField from 'primevue/iconfield';
  import InputIcon from 'primevue/inputicon';
  import Dialog from 'primevue/dialog';
  import DatePicker from 'primevue/datepicker';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';

  const cotizaciones = ref<ICotizacion[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');
  const filtroTexto = ref('');
  const filtroEstado = ref<string | null>(null);
  const estadosPermitidos = ref(['Borrador', 'Emitida', 'Aceptada', 'Rechazada', 'Vencida']);
  const mostrarFiltros = ref(window.innerWidth > 768);

  const mostrarModalEstado = ref(false);
  const guardandoEstado = ref(false);
  const cotizacionEnEdicion = ref<ICotizacion | null>(null);
  const nuevoEstado = ref('');
  const nuevaVigencia = ref<Date | null>(null);

  const confirm = useConfirm();
  const toast = useToast();

  onMounted(async () => {
    await cargarCotizaciones();
  });

  const cargarCotizaciones = async () => {
    cargando.value = true;
    try {
      cotizaciones.value = await cotizacionService.obtenerTodas();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      cargando.value = false;
    }
  };

  const cotizacionesFiltradas = computed(() => {
    return cotizaciones.value.filter(c => {
      let coincideTexto = true;
      let coincideEstado = true;

      if (filtroTexto.value) {
        const termino = filtroTexto.value.toLowerCase();
        coincideTexto =
          c.numeroCotizacion.toLowerCase().includes(termino) ||
          (c.clienteNombre?.toLowerCase().includes(termino) ?? false) ||
          (c.nombreProyecto?.toLowerCase().includes(termino) ?? false);
      }

      if (filtroEstado.value) {
        coincideEstado = c.estado === filtroEstado.value;
      }

      return coincideTexto && coincideEstado;
    });
  });

  const abrirModalEstado = (cotizacion: ICotizacion) => {
    cotizacionEnEdicion.value = cotizacion;
    nuevoEstado.value = cotizacion.estado;
    nuevaVigencia.value = null;
    mostrarModalEstado.value = true;
  };

  const guardarEstado = async () => {
    if (!cotizacionEnEdicion.value) return;
    guardandoEstado.value = true;
    try {
      if (nuevoEstado.value !== cotizacionEnEdicion.value.estado) {
        await cotizacionService.actualizarEstado(cotizacionEnEdicion.value.id, { nuevoEstado: nuevoEstado.value });
      }
      if (nuevaVigencia.value) {
        await cotizacionService.renovarVigencia(cotizacionEnEdicion.value.id, { nuevaFechaVencimiento: nuevaVigencia.value.toISOString() });
      }
      mostrarModalEstado.value = false;
      await cargarCotizaciones();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      guardandoEstado.value = false;
    }
  };

  const descargar = async (cotizacion: ICotizacion) => {
    try {
      await cotizacionService.descargarPdf(cotizacion.id, cotizacion.numeroCotizacion);
    } catch (error: any) {
      errorGlobal.value = error.message;
    }
  };

  const compartir = async (cotizacion: ICotizacion) => {
    try {
      await cotizacionService.compartirPdf(cotizacion.id, cotizacion.numeroCotizacion);
    } catch (error: any) {
      errorGlobal.value = error.message;
    }
  };

  const formatearFecha = (fechaStr: string) => {
    return new Date(fechaStr).toLocaleDateString('es-CL', { timeZone: 'America/Santiago' });
  };

  const obtenerSeveridad = (estado: string) => {
    switch (estado) {
      case 'Borrador': return 'secondary';
      case 'Emitida': return 'info';
      case 'Aceptada': return 'success';
      case 'Rechazada': return 'danger';
      case 'Vencida': return 'warn';
      default: return 'secondary';
    }
  };
  const mostrarVistaPrevia = ref(false);
  const cotizacionSeleccionada = ref<ICotizacion | null>(null);

  const abrirVistaPrevia = (cotizacion: ICotizacion) => {
    cotizacionSeleccionada.value = cotizacion;
    mostrarVistaPrevia.value = true;
  };

  const confirmarEliminacion = (cotizacion: ICotizacion) => {
    confirm.require({
      message: `¿Estás seguro de eliminar la cotización ${cotizacion.numeroCotizacion}?`,
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: { label: 'Cancelar', severity: 'secondary', outlined: true },
      acceptProps: { label: 'Eliminar', severity: 'danger' },
      accept: async () => {
        try {
          cargando.value = true;
          await cotizacionService.eliminar(cotizacion.id);
          await cargarCotizaciones();
          toast.add({ severity: 'success', summary: 'Eliminada', detail: 'Cotización eliminada correctamente.', life: 3000 });
        } catch (error: any) {
          errorGlobal.value = error.message;
        } finally {
          cargando.value = false;
        }
      }
    });
  };

</script>
