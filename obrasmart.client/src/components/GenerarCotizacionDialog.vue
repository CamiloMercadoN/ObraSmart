<template>
  <Dialog :visible="visible" @update:visible="$emit('update:visible', $event)" modal header="Generar Cotización" :style="{ width: '90vw', maxWidth: '450px' }">
    <div class="flex flex-column gap-3 pt-2">
      <Message v-if="errorCotizacion" severity="error" :closable="true" @close="errorCotizacion = ''">
        {{ errorCotizacion }}
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
  import { ref, watch, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
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

  const generando = ref(false);
  const errorCotizacion = ref('');
  const fechaVencimiento = ref<Date>(new Date());
  const numeroCotizacionOpcional = ref<number | null>(null);

  const diasValidezDefecto = ref(15); // Fallback inicial

  onMounted(async () => {
    try {
      const config = await configuracionService.obtener();
      if (config && config.diasValidez) {
        diasValidezDefecto.value = config.diasValidez ?? 15;
      }
    } catch (error) {
      console.warn('No se pudo cargar la configuración comercial para la validez por defecto.');
    }
  });

  // Resetear valores cada vez que se abre el modal, usando el valor configurado
  watch(() => props.visible, (nuevoValor) => {
    if (nuevoValor) {
      const fecha = new Date();
      fecha.setDate(fecha.getDate() + diasValidezDefecto.value);

      fechaVencimiento.value = fecha;
      numeroCotizacionOpcional.value = null;
      errorCotizacion.value = '';
    }
  });

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

      confirm.require({
        message: `La cotización ${nuevaCotizacion.numeroCotizacion} fue generada exitosamente. ¿Deseas compartirla ahora?`,
        header: 'Cotización Generada',
        icon: 'pi pi-check-circle',
        acceptLabel: 'Compartir PDF',
        rejectLabel: 'Ir al Gestor',
        accept: async () => {
          await cotizacionService.compartirPdf(nuevaCotizacion.id, nuevaCotizacion.numeroCotizacion);
          router.push('/cotizaciones');
        },
        reject: () => {
          router.push('/cotizaciones');
        }
      });

    } catch (err: any) {
      errorCotizacion.value = err.message;
    } finally {
      generando.value = false;
    }
  };
</script>
