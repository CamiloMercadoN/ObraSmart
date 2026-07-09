<template>
  <Dialog v-model:visible="isVisible"
          :header="modoEdicion ? 'Editar Cliente' : 'Nuevo Cliente'"
          modal
          :style="{ width: '90vw', maxWidth: '600px' }"
          :closable="!loading"
          @hide="resetForm">

    <form @submit.prevent="handleSubmit" class="flex flex-column gap-3 mt-2">

      <div class="flex flex-column gap-2">
        <label for="nombre" class="font-bold">Nombre o Razón Social <span class="text-red-500">*</span></label>
        <InputText id="nombre"
                   v-model="formulario.nombre"
                   required
                   class="w-full"
                   maxlength="150" />
      </div>

      <div class="flex flex-column gap-2">
        <label for="rut" class="font-bold">RUT</label>
        <InputText id="rut"
                   v-model="formulario.rut"
                   @input="handleInputRut"
                   placeholder="12345678-9"
                   :invalid="!esRutValido && formulario.rut.length > 7"
                   class="w-full"
                   maxlength="12" />
        <small v-if="!esRutValido && formulario.rut.length > 7" class="text-red-500 text-sm">El RUT ingresado no es válido.</small>
      </div>

      <div class="flex flex-column gap-2">
        <label for="correo" class="font-bold">Correo Electrónico</label>
        <InputText id="correo"
                   v-model="formulario.correo"
                   type="email"
                   :invalid="!esCorreoValido"
                   placeholder="ejemplo@dominio.com"
                   class="w-full" />
        <small v-if="!esCorreoValido && formulario.correo.length > 0" class="text-red-500 text-sm">El formato del correo es inválido.</small>
      </div>

      <div class="grid m-0 p-0">
        <div class="col-12 md:col-6 p-0 md:pr-2 flex flex-column gap-2">
          <label for="telefono" class="font-bold">Teléfono</label>
          <InputText id="telefono"
                     v-model="formulario.telefono"
                     placeholder="+56912345678"
                     class="w-full"
                     maxlength="20" />
        </div>
      </div>

      <div class="grid m-0 p-0">
        <div class="col-12 md:col-6 p-0 md:pr-2 flex flex-column gap-2">
          <label for="region" class="font-bold">Región</label>
          <Select id="region"
                    v-model="regionSeleccionada"
                    :options="regiones"
                    optionLabel="nombre"
                    optionValue="id"
                    placeholder="Seleccione Región"
                    class="w-full"
                    :disabled="cargandoTerritorios"
                    @change="onRegionChange"/>
        </div>
        <div class="col-12 md:col-6 p-0 md:pl-2 flex flex-column gap-2 mt-3 md:mt-0">
          <label for="comuna" class="font-bold">Comuna</label>
          <Select id="comuna"
                    v-model="formulario.ciudadId"
                    :options="comunas"
                    optionLabel="nombre"
                    optionValue="id"
                    placeholder="Seleccione Comuna"
                    :disabled="!regionSeleccionada || cargandoTerritorios"
                    class="w-full" />
        </div>
      </div>

      <div class="flex flex-column gap-2">
        <label for="direccion" class="font-bold">Dirección (Calle y Número)</label>
        <InputText id="direccion"
                   v-model="formulario.direccion"
                   class="w-full"
                   maxlength="250" />
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
</template>

<script setup lang="ts">
  import { ref, computed, watch, type PropType } from 'vue';
  import type { ICliente } from '../interfaces/ICliente';
  import Dialog from 'primevue/dialog';
  import InputText from 'primevue/inputtext';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Select from 'primevue/select';
  import { validarRutChileno, formatearRut } from '../utils/rutHelper';
  import { validarCorreo } from '../utils/emailHelper';
  import { territorioService, type ITerritorio } from '../services/territorioService';

  const props = defineProps({
    visible: {
      type: Boolean,
      required: true
    },
    clienteData: {
      type: Object as PropType<ICliente | null>,
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
    (e: 'guardar', payload: ICliente): void;
  }>();

  const isVisible = computed({
    get: () => props.visible,
    set: (value) => emit('update:visible', value)
  });

  const modoEdicion = computed(() => !!props.clienteData?.id);

  // Estado de Territorios
  const regiones = ref<ITerritorio[]>([]);
  const comunas = ref<ITerritorio[]>([]);
  const regionSeleccionada = ref<number | null>(null);
  const cargandoTerritorios = ref(false);

  const formulario = ref<ICliente>({
    id: null,
    nombre: '',
    rut: '',
    correo: '',
    telefono: '',
    direccion: '',
    ciudadId: null 
  });

  // Cargar regiones iniciales
  const cargarRegiones = async () => {
    if (regiones.value.length > 0) return; // Evita llamadas redundantes
    try {
      cargandoTerritorios.value = true;
      regiones.value = await territorioService.getRegiones();
    } catch (err) {
      console.error("Error al cargar regiones", err);
    } finally {
      cargandoTerritorios.value = false;
    }
  };

  watch(() => props.visible, async (newVal) => {
    if (newVal) {
      await cargarRegiones();

      if (props.clienteData) {
        formulario.value = { ...props.clienteData };
        if (props.clienteData.regionId) {
          regionSeleccionada.value = props.clienteData.regionId;
        } else {
          regionSeleccionada.value = null;
          comunas.value = [];
        }
      } else {
        resetForm();
      }
    }
  });

  // Observador para cargar comunas cuando cambia la región
  watch(regionSeleccionada, async (newVal) => {
    if (newVal) {
      try {
        cargandoTerritorios.value = true;
        comunas.value = await territorioService.getCiudadesPorRegion(newVal);
      } catch (err) {
        console.error("Error al cargar comunas", err);
      } finally {
        cargandoTerritorios.value = false;
      }
    } else {
      comunas.value = [];
    }
  });

  const onRegionChange = () => {
    formulario.value.ciudadId = null;
  };

  const handleInputRut = () => {
    formulario.value.rut = formatearRut(formulario.value.rut);
  };

  const esRutValido = computed(() => {
    if (!formulario.value.rut) return true;
    return validarRutChileno(formulario.value.rut);
  });

  const esCorreoValido = computed(() => {
    if (!formulario.value.correo) return true;
    return validarCorreo(formulario.value.correo);
  });

  const formularioValido = computed(() => {
    return formulario.value.nombre.trim().length > 0 &&
      esRutValido.value &&
      esCorreoValido.value;
  });

  const resetForm = () => {
    formulario.value = {
      id: null,
      nombre: '',
      rut: '',
      correo: '',
      telefono: '',
      direccion: '',
      ciudadId: null
    };
    regionSeleccionada.value = null;
    comunas.value = [];
  };

  const handleSubmit = () => {
    if (!formularioValido.value) return;
    emit('guardar', { ...formulario.value });
  };
</script>
