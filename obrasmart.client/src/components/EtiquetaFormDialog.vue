<template>
  <Dialog v-model:visible="isVisible"
          header="Nueva Etiqueta"
          modal
          :style="{ width: '300px' }"
          :closable="!guardando"
          @hide="resetForm">
    <div class="flex flex-column gap-3 mt-2">

      <div class="flex flex-column gap-2">
        <label for="nombreEtiqueta" class="font-bold text-sm">Nombre <span class="text-red-500">*</span></label>
        <InputText id="nombreEtiqueta" v-model="nuevaEtiqueta.nombre" maxlength="50" autofocus />
      </div>

      <div class="flex flex-column gap-2">
        <label for="colorEtiqueta" class="font-bold text-sm">Color</label>
        <input type="color" id="colorEtiqueta" v-model="nuevaEtiqueta.colorHex" class="w-full h-2rem border-round cursor-pointer" style="border: 1px solid #cbd5e1;" />
      </div>

      <Message v-if="errorLocal" severity="error" :closable="false" class="mt-2 mb-0 p-2 text-sm">
        {{ errorLocal }}
      </Message>

      <div class="flex justify-content-end gap-2 mt-3">
        <Button label="Cancelar" text severity="secondary" size="small" @click="isVisible = false" :disabled="guardando" />
        <Button label="Crear" size="small" @click="guardar" :loading="guardando" :disabled="!nuevaEtiqueta.nombre.trim()" />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue';
  import { insumoService } from '../services/insumoService';
  import Dialog from 'primevue/dialog';
  import InputText from 'primevue/inputtext';
  import Button from 'primevue/button';
  import Message from 'primevue/message';

  const props = defineProps({
    visible: {
      type: Boolean,
      required: true
    }
  });

  const emit = defineEmits<{
    (e: 'update:visible', value: boolean): void;
    // Emitimos el ID de la etiqueta creada y su data parcial para actualizar las listas visuales
    (e: 'etiqueta-creada', payload: { id: string, nombre: string, colorHex: string }): void;
  }>();

  const isVisible = computed({
    get: () => props.visible,
    set: (value) => emit('update:visible', value)
  });

  const guardando = ref(false);
  const errorLocal = ref('');

  const nuevaEtiqueta = ref({
    nombre: '',
    colorHex: '#3b82f6'
  });

  const resetForm = () => {
    nuevaEtiqueta.value = { nombre: '', colorHex: '#3b82f6' };
    errorLocal.value = '';
  };

  const guardar = async () => {
    if (!nuevaEtiqueta.value.nombre.trim()) return;

    guardando.value = true;
    errorLocal.value = '';

    try {
      const payload = {
        nombre: nuevaEtiqueta.value.nombre.trim(),
        colorHex: nuevaEtiqueta.value.colorHex,
        esPlantilla: false
      };

      const nuevoId = await insumoService.crearEtiqueta(payload);

      emit('etiqueta-creada', { id: nuevoId, ...payload });
      isVisible.value = false;
    } catch (err: any) {
      errorLocal.value = err.message;
    } finally {
      guardando.value = false;
    }
  };
</script>
