<template>
  <Dialog :visible="visible" @update:visible="cerrarDialogo" modal :style="{ width: '90vw', maxWidth: '600px' }" class="p-0">
    <template #header>
      <div class="flex align-items-center gap-2">
        <i class="pi pi-file-pdf text-xl text-primary"></i>
        <h3 class="m-0 app-text">Vista Previa de Cotización</h3>
      </div>
    </template>

    <div v-if="cargando" class="flex justify-content-center p-5">
      <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
    </div>

    <div v-else-if="presupuesto" class="flex flex-column gap-3">
      <!-- Switch para Insumos -->
      <div v-if="cotizacion.estado === 'Borrador'" class="flex align-items-center gap-2 bg-indigo-50 p-2 border-round">
        <ToggleSwitch v-model="incluirRecursos" />
        <label class="text-indigo-900 font-semibold text-sm">Mostrar desglose de insumos en el documento</label>
      </div>
      <!-- Botones de Acción -->
      <div class="flex flex-column sm:flex-row gap-2 mb-3">
        <Button label="Descargar PDF" icon="pi pi-download" class="flex-1" @click="descargarPdf" />
        <Button label="Compartir" icon="pi pi-share-alt" severity="success" class="flex-1" @click="compartirPdf" />
      </div>

      <div v-if="urlPdfRenderizado" class="border-round overflow-hidden" style="border: 1px solid #e2e8f0; height: 65vh;">
        <iframe :src="urlPdfRenderizado" class="w-full h-full border-none"></iframe>
      </div>

      <!-- Documento Visual (HTML) -->
      <div v-else class="p-4 border-round bg-white" style="color: #333333; border: 1px solid #e2e8f0;">

        <!-- Header -->
        <div class="flex justify-content-between align-items-start pb-3 mb-3" style="border-bottom: 1px solid #cbd5e1;">
          <div>
            <h2 class="m-0 text-xl font-bold" style="color: #1e293b;">{{ configuracion?.razonSocial || 'Trabajador Independiente' }}</h2>
            <div class="text-sm mt-1" style="color: #64748b;">Cotización N°: {{ cotizacion.numeroCotizacion }}</div>
            <div class="text-sm" style="color: #64748b;">Fecha Emisión: {{ formatearFecha(cotizacion.fechaEmision) }}</div>
            <div class="text-sm font-bold mt-1" :style="cotizacion.estado === 'Vencida' ? 'color: #ef4444;' : 'color: #64748b;'">
              Válida hasta: {{ formatearFecha(cotizacion.fechaVencimiento) }}
            </div>
          </div>
          <div class="text-right" v-if="configuracion?.logoBase64">
            <img :src="configuracion.logoBase64" alt="Logo" style="max-height: 50px; max-width: 120px; object-fit: contain;" />
          </div>
        </div>

        <!-- Cliente -->
        <div class="mb-4">
          <h4 class="m-0 mb-2 font-bold" style="color: #334155;">DATOS DEL CLIENTE</h4>
          <div class="text-sm line-height-2" style="color: #475569;">
            <div>Nombre o Razón Social:{{ presupuesto.clienteNombre }}</div>
            <div v-if="presupuesto.clienteRut">RUT: {{ presupuesto.clienteRut }}</div>
            <div v-if="presupuesto.clienteDireccion">Dirección: {{ presupuesto.clienteDireccion }}</div>
          </div>
        </div>

        <!-- Detalle -->
        <table class="w-full text-sm text-left mb-4" style="border-collapse: collapse; color: #475569;">
          <thead>
            <tr style="border-bottom: 1px solid #cbd5e1; color: #334155;">
              <th class="py-2">Descripción</th>
              <th class="py-2 text-center">Cant.</th>
              <th class="py-2 text-center">Unidad</th>
              <th class="py-2 text-right">P. Unit</th>
              <th class="py-2 text-right">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            <!-- RECORRIDO DE ITEMS E INSUMOS -->
            <template v-for="item in presupuesto.items" :key="item.id">
              <tr style="border-bottom: 1px solid #f1f5f9;">
                <td class="py-2">
                  <div class="font-bold">{{ item.descripcion }}</div>

                  <!-- Desglose HTML de recursos -->
                  <div v-if="incluirRecursos && item.recursos?.length > 0" class="mt-1 text-xs" style="color: #64748b;">
                    <ul class="m-0 pl-3">
                      <li v-for="rec in item.recursos" :key="rec.id">
                        {{ rec.cantidad }} {{ rec.unidadMedidaNombre || '' }} de {{ rec.descripcionCongelada }} (${{ formatearMoneda(rec.precioUnitarioCongelado) }})
                      </li>
                    </ul>
                  </div>
                </td>
                <td class="py-2 text-center align-top">{{ item.cantidadItem }}</td>
                <td class="py-2 text-center align-top">{{ item.unidadMedidaNombre || '-' }}</td>
                <td class="py-2 text-right align-top">${{ formatearMoneda(item.precioUnitarioCalculado) }}</td>
                <td class="py-2 text-right align-top">${{ formatearMoneda(item.subtotal) }}</td>
              </tr>
            </template>
          </tbody>
        </table>

        <!-- Totales -->
        <div class="flex justify-content-end" style="color: #475569;">
          <div class="w-15rem text-right text-sm">
            <div class="flex justify-content-between mb-1">
              <span>Subtotal:</span>
              <span>${{ formatearMoneda(presupuesto.subtotal) }}</span>
            </div>
            <div class="flex justify-content-between mb-1 pb-1" style="border-bottom: 1px solid #cbd5e1;">
              <span>IVA ({{ configuracion?.porcentajeIva || 19 }}%):</span>
              <span>${{ formatearMoneda(presupuesto.montoIva) }}</span>
            </div>
            <div class="flex justify-content-between mt-2 font-bold text-lg" style="color: #1e293b;">
              <span>Total:</span>
              <span>${{ formatearMoneda(presupuesto.total) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
  import { ref, watch, onUnmounted } from 'vue';
  import { presupuestoService } from '../services/presupuestoService';
  import { cotizacionService } from '../services/cotizacionService';
  import { configuracionService } from '../services/configuracionService';
  import type { ICotizacion } from '../interfaces/ICotizacion';
  import type { IPresupuesto } from '../interfaces/IPresupuesto';
  import type { IConfiguracionComercial } from '../interfaces/IConfiguracionComercial';

  import Dialog from 'primevue/dialog';
  import Button from 'primevue/button';
  import ToggleSwitch from 'primevue/toggleswitch';

  const props = defineProps<{
    visible: boolean;
    cotizacion: ICotizacion;
  }>();

  const emit = defineEmits<{
    (e: 'update:visible', value: boolean): void;
    (e: 'recargar'): void;
  }>();

  const cargando = ref(false);
  const incluirRecursos = ref(false);
  const presupuesto = ref<IPresupuesto | null>(null);
  const configuracion = ref<IConfiguracionComercial | null>(null);
  const urlPdfRenderizado = ref<string | null>(null);

  const cargarDatos = async () => {
    cargando.value = true;
    try {
      const [presData, configData] = await Promise.all([
        presupuestoService.obtenerPorId(props.cotizacion.presupuestoId),
        configuracionService.obtener()
      ]);
      presupuesto.value = presData;
      configuracion.value = configData;

      // Si ya está emitida, descarga el archivo físico protegido y lo ponemos en memoria
      if (props.cotizacion.estado !== 'Borrador') {
        const blob = await cotizacionService.obtenerPdfBlob(props.cotizacion.id, incluirRecursos.value);
        urlPdfRenderizado.value = URL.createObjectURL(blob);
      }

    } catch (error) {
    } finally {
      cargando.value = false;
    }
  };

  watch(() => props.visible, async (nuevoValor) => {
    if (nuevoValor && props.cotizacion) {
        incluirRecursos.value = false;

        // Revoca la URL anterior de la memoria del navegador para evitar fugas
        if (urlPdfRenderizado.value) {
          URL.revokeObjectURL(urlPdfRenderizado.value);
        }
        // Resetea a null para que Vue vuelva a mostrar el HTML si es borrador
        urlPdfRenderizado.value = null;
        await cargarDatos();
    }
  }, { immediate: true });

  // Limpieza de memoria para evitar fugas cuando se cierra el modal
  onUnmounted(() => {
    if (urlPdfRenderizado.value) URL.revokeObjectURL(urlPdfRenderizado.value);
  });

  const cerrarDialogo = () => {
    emit('update:visible', false);
  };

const descargarPdf = async () => {
    await cotizacionService.descargarPdf(props.cotizacion.id, props.cotizacion.numeroCotizacion, incluirRecursos.value);
    emit('recargar');
    cerrarDialogo();
  };

  const compartirPdf = async () => {
    await cotizacionService.compartirPdf(props.cotizacion.id, props.cotizacion.numeroCotizacion, incluirRecursos.value);
    emit('recargar');
    cerrarDialogo();
  };

  const formatearMoneda = (valor?: number) => valor === undefined ? '0' : valor.toLocaleString('es-CL');
  const formatearFecha = (fechaStr: string) => new Date(fechaStr).toLocaleDateString('es-CL', { timeZone: 'America/Santiago' });
</script>
