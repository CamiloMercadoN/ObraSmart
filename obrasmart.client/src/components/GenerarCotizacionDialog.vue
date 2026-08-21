<template>
  <Dialog :visible="visible" @update:visible="$emit('update:visible', $event)" modal header="Generar Cotización" :style="{ width: '90vw', maxWidth: '450px' }">
    <div class="flex flex-column gap-3 pt-2">
      <Message v-if="errorCotizacion" severity="error" :closable="true" @close="errorCotizacion = ''">
        {{ errorCotizacion }}
      </Message>
      <Message v-if="tieneVigentes" severity="warn" :closable="false">
        Ya existe una cotización vigente para este presupuesto. Generar una nueva no anulará la anterior automáticamente.
      </Message>
      <div class="field">
        <label class="font-bold app-text mb-2 block">Fecha de Vencimiento *</label>
        <DatePicker v-model="fechaVencimiento" dateFormat="dd/mm/yy" class="w-full" :minDate="new Date()" appendTo="body" />
      </div>

      <div class="field">
        <label class="font-bold app-text mb-2 block">Número de Cotización (Opcional)</label>
        <InputNumber v-model="numeroCotizacionOpcional" placeholder="Ej: 1025" class="w-full" :min="1" />
        <small class="app-text-muted mt-1 block">Déjalo en blanco para usar el siguiente número automático.</small>
      </div>
    </div>

    <template #footer>
      <div class="flex gap-2 w-full">
        <Button label="Cancelar" severity="secondary" outlined @click="cerrarDialogo" class="flex-1" />
        <Button label="Generar" icon="pi pi-check" @click="confirmarGeneracion" :loading="generando" class="flex-1" />
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import { cotizacionService } from '../services/cotizacionService';
  import { configuracionService } from '../services/configuracionService';

  import Dialog from 'primevue/dialog';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import DatePicker from 'primevue/datepicker';
  import InputNumber from 'primevue/inputnumber';

  const props = defineProps<{
    visible: boolean;
    presupuestoId: string;
  }>();

  const emit = defineEmits<{
    (e: 'update:visible', value: boolean): void;
    (e: 'generada'): void;
  }>();

  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const generando = ref(false);
  const errorCotizacion = ref('');
  const fechaVencimiento = ref<Date>(new Date());
  const numeroCotizacionOpcional = ref<number | null>(null);
  const diasValidezDefecto = ref(15);
  const tieneVigentes = ref(false);

  watch(() => props.visible, async (nuevoValor) => {
    if (nuevoValor) {
      numeroCotizacionOpcional.value = null;
      errorCotizacion.value = '';
      tieneVigentes.value = false;

      // CARGAR CONFIGURACIÓN AL ABRIR EL MODAL
      try {
        const config = await configuracionService.obtener();
        if (config && config.diasValidez) {
          diasValidezDefecto.value = Number(config.diasValidez);
        }
      } catch (error) {
        console.warn('No se pudo cargar la configuración comercial. Usando validez por defecto.');
      }

      // CALCULAR FECHA DE VENCIMIENTO
      const fecha = new Date();
      fecha.setDate(fecha.getDate() + diasValidezDefecto.value);
      fechaVencimiento.value = fecha;

      // EVALUACIÓN DE COTIZACIONES VIGENTES O EN CURSO
      try {
        const historial = await cotizacionService.obtenerTodas();
        const hoy = new Date();

        tieneVigentes.value = historial.some(c => {
          if (c.presupuestoId !== props.presupuestoId) return false;

          // Si ya está Aceptada o hay un Borrador pendiente, lanzamos alerta
          if (c.estado === 'Aceptada' || c.estado === 'Borrador') return true;

          // Si está Emitida, validamos que aún no venza
          if (c.estado === 'Emitida' && new Date(c.fechaVencimiento) >= hoy) return true;

          return false;
        });
      } catch (e) { }
    }
  }, { immediate: true });

  const cerrarDialogo = () => {
    emit('update:visible', false);
  };

  const confirmarGeneracion = async () => {
    if (!props.presupuestoId) return;

    generando.value = true;
    errorCotizacion.value = '';

    try {
      const request = {
        presupuestoId: props.presupuestoId,
        fechaVencimiento: fechaVencimiento.value.toISOString(),
        numeroCotizacionPersonalizado: numeroCotizacionOpcional.value
      };

      const nuevaCotizacion = await cotizacionService.crear(request);

      cerrarDialogo();
      emit('generada');

      toast.add({
        severity: 'success',
        summary: 'Cotización Generada',
        detail: `La cotización ${nuevaCotizacion.numeroCotizacion} fue creada con éxito.`,
        life: 4000
      });

      setTimeout(() => {
        confirm.require({
          message: `¿Deseas descargar o compartir la cotización ${nuevaCotizacion.numeroCotizacion} ahora mismo?`,
          header: 'Siguiente paso',
          icon: 'pi pi-send',
          acceptLabel: 'Compartir PDF',
          rejectLabel: 'Ir al Gestor',
          accept: async () => {
            confirm.close();
            await cotizacionService.compartirPdf(nuevaCotizacion.id, nuevaCotizacion.numeroCotizacion, false);
            router.push('/cotizaciones');
          },
          reject: () => {
            confirm.close();
            router.push('/cotizaciones');
          }
        });
      }, 300);

    } catch (err: any) {
      errorCotizacion.value = err.message;
    } finally {
      generando.value = false;
    }
  };
</script>
