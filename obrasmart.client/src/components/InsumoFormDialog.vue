<template>
  <Dialog v-model:visible="isVisible"
          :header="modoEdicion ? 'Editar Insumo' : 'Nuevo Insumo'"
          modal
          :style="{ width: '90vw', maxWidth: '600px' }"
          :closable="!loading"
          @hide="resetForm">

    <form @submit.prevent="handleSubmit" class="flex flex-column gap-3 mt-2">

      <div class="flex flex-column gap-2">
        <label for="descripcion" class="font-bold">Descripción del Insumo <span class="text-red-500">*</span></label>
        <InputText id="descripcion"
                   v-model="formulario.descripcion"
                   required
                   class="w-full"
                   maxlength="200" />
      </div>

      <div class="grid m-0 p-0">
        <div class="col-12 md:col-6 p-0 md:pr-2 flex flex-column gap-2">
          <label for="tipoInsumo" class="font-bold">Tipo de Insumo <span class="text-red-500">*</span></label>
          <Select id="tipoInsumo"
                  v-model="formulario.tipoInsumo"
                  :options="tiposInsumo"
                  placeholder="Seleccione Tipo"
                  class="w-full" />
        </div>

        <div class="col-12 md:col-6 p-0 md:pl-2 flex flex-column gap-2 mt-3 md:mt-0">
          <label for="precioReferencia" class="font-bold">Precio de Referencia <span class="text-red-500">*</span></label>
          <InputNumber id="precioReferencia"
                       v-model="formulario.precioReferencia"
                       mode="currency"
                       currency="CLP"
                       locale="es-CL"
                       :min="0"
                       class="w-full" />
        </div>
      </div>

      <div class="grid m-0 p-0">
        <div class="col-12 md:col-6 p-0 md:pr-2 flex flex-column gap-2">
          <label for="unidadMedida" class="font-bold">Unidad de Medida <span class="text-red-500">*</span></label>
          <Select id="unidadMedida"
                  v-model="formulario.unidadMedidaId"
                  :options="unidades"
                  optionLabel="nombre"
                  optionValue="id"
                  placeholder="Seleccione Unidad"
                  :disabled="cargandoAuxiliares"
                  class="w-full" />
        </div>

        <div class="col-12 md:col-6 p-0 md:pl-2 flex flex-column gap-2 mt-3 md:mt-0">
          <label for="etiquetas" class="font-bold">Etiquetas</label>
          <div class="flex gap-2 align-items-stretch">
            <MultiSelect id="etiquetas"
                         v-model="formulario.etiquetasIds"
                         :options="etiquetas"
                         optionLabel="nombre"
                         optionValue="id"
                         display="chip"
                         placeholder="Seleccione Etiquetas"
                         :disabled="cargandoAuxiliares"
                         class="flex-grow-1"
                         style="min-width: 0;" />
            <Button icon="pi pi-plus"
                          outlined
                          severity="secondary"
                          @click="mostrarDialogoEtiqueta = true"
                          :disabled="cargandoAuxiliares"
                          class="flex-shrink-0" v-tooltip.top="'Crear nueva etiqueta'" />
          </div>
        </div>
      </div>

      <Message v-if="error" severity="error" :closable="false" class="mt-2 mb-0">
        {{ error }}
      </Message>

      <div class="flex justify-content-end gap-2 mt-4">
        <Button label="Cancelar" icon="pi pi-times" text severity="secondary" @click="isVisible = false" :disabled="loading" />
        <Button type="submit" label="Guardar" icon="pi pi-check" :loading="loading" :disabled="!formularioValido" />
      </div>

    </form>
  </Dialog>
  <Dialog v-model:visible="mostrarDialogoEtiqueta"
          header="Nueva Etiqueta"
          modal
          :style="{ width: '300px' }"
          :closable="!guardandoEtiqueta"
          @hide="resetEtiquetaForm">
    <div class="flex flex-column gap-3 mt-2">
      <div class="flex flex-column gap-2">
        <label for="nombreEtiqueta" class="font-bold text-sm">Nombre <span class="text-red-500">*</span></label>
        <InputText id="nombreEtiqueta" v-model="nuevaEtiqueta.nombre" maxlength="50" autofocus />
      </div>
      <div class="flex flex-column gap-2">
        <label for="colorEtiqueta" class="font-bold text-sm">Color</label>
        <input type="color" id="colorEtiqueta" v-model="nuevaEtiqueta.colorHex" class="w-full h-2rem border-round cursor-pointer" style="border: 1px solid #cbd5e1;" />
      </div>
      <Message v-if="errorEtiqueta" severity="error" :closable="false" class="mt-2 mb-0 p-2 text-sm">
        {{ errorEtiqueta }}
      </Message>
      <div class="flex justify-content-end gap-2 mt-3">
        <Button label="Cancelar" text severity="secondary" size="small" @click="mostrarDialogoEtiqueta = false" :disabled="guardandoEtiqueta" />
        <Button label="Crear" size="small" @click="guardarNuevaEtiqueta" :loading="guardandoEtiqueta" :disabled="!nuevaEtiqueta.nombre.trim()" />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
  import { ref, computed, watch, type PropType } from 'vue';
  import type { IInsumo, IUnidadMedida, IEtiqueta } from '../interfaces/IInsumo';
  import { insumoService } from '../services/insumoService';
  import Dialog from 'primevue/dialog';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Select from 'primevue/select';
  import MultiSelect from 'primevue/multiselect';

  const props = defineProps({
    visible: {
      type: Boolean,
      required: true
    },
    insumoData: {
      type: Object as PropType<IInsumo | null>,
      default: () => null
    },
    loading: {
      type: Boolean,
      default: false
    },
    error: {
      type: String,
      default: ''
    }
  });

  const emit = defineEmits<{
    (e: 'update:visible', value: boolean): void;
    (e: 'guardar', payload: IInsumo): void;
  }>();

  const isVisible = computed({
    get: () => props.visible,
    set: (value) => emit('update:visible', value)
  });

  const modoEdicion = computed(() => !!props.insumoData?.id);

  // Opciones estáticas e interactivas
  const tiposInsumo = ref(['Material', 'Mano de Obra', 'Equipo']);
  const unidades = ref<IUnidadMedida[]>([]);
  const etiquetas = ref<IEtiqueta[]>([]);
  const cargandoAuxiliares = ref(false);

  const formulario = ref<IInsumo>({
    id: null,
    tipoInsumo: '',
    descripcion: '',
    precioReferencia: 0,
    unidadMedidaId: null,
    etiquetasIds: []
  });

  const cargarListasAuxiliares = async () => {
    if (unidades.value.length > 0 && etiquetas.value.length > 0) return;
    try {
      cargandoAuxiliares.value = true;
      const [umResponse, tagResponse] = await Promise.all([
        insumoService.obtenerUnidadesMedida(),
        insumoService.obtenerEtiquetas()
      ]);
      unidades.value = umResponse;
      etiquetas.value = tagResponse;
    } catch (err) {
      console.error("Error al cargar datos auxiliares para insumos", err);
    } finally {
      cargandoAuxiliares.value = false;
    }
  };

  watch(() => props.visible, async (newVal) => {
    if (newVal) {
      await cargarListasAuxiliares();

      if (props.insumoData) {
        formulario.value = { ...props.insumoData };
      } else {
        resetForm();
      }
    }
  });

  const formularioValido = computed(() => {
    return formulario.value.descripcion.trim().length > 0 &&
      formulario.value.tipoInsumo !== '' &&
      formulario.value.unidadMedidaId !== null &&
      formulario.value.precioReferencia >= 0;
  });

  const resetForm = () => {
    formulario.value = {
      id: null,
      tipoInsumo: '',
      descripcion: '',
      precioReferencia: 0,
      unidadMedidaId: null,
      etiquetasIds: []
    };
  };

  const handleSubmit = () => {
    if (!formularioValido.value) return;
    emit('guardar', { ...formulario.value });
  };

  // --- Estado para la Creación Rápida de Etiquetas ---
  const mostrarDialogoEtiqueta = ref(false);
  const guardandoEtiqueta = ref(false);
  const errorEtiqueta = ref('');

  const nuevaEtiqueta = ref({
    nombre: '',
    colorHex: '#3b82f6'
  });

  const resetEtiquetaForm = () => {
    nuevaEtiqueta.value = { nombre: '', colorHex: '#3b82f6' };
    errorEtiqueta.value = '';
  };

  const guardarNuevaEtiqueta = async () => {
    if (!nuevaEtiqueta.value.nombre.trim()) return;

    guardandoEtiqueta.value = true;
    errorEtiqueta.value = '';

    try {
      const nuevoId = await insumoService.crearEtiqueta(nuevaEtiqueta.value);

      const tagAgregada = {
        id: nuevoId,
        nombre: nuevaEtiqueta.value.nombre.trim(),
        colorHex: nuevaEtiqueta.value.colorHex,
        esPlantilla: false
      };

      etiquetas.value.push(tagAgregada);
      formulario.value.etiquetasIds.push(nuevoId);
      mostrarDialogoEtiqueta.value = false;
    } catch (err: any) {
      errorEtiqueta.value = err.message;
    } finally {
      guardandoEtiqueta.value = false;
    }
  };

</script>
