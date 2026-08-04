<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4 h-full max-w-50rem mx-auto">

    <!-- Cabecera y Acciones -->
    <div class="surface-card p-3 md:p-4 shadow-1 border-round flex flex-column md:flex-row justify-content-between md:align-items-center gap-3">
      <div class="flex align-items-center gap-3">
        <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
          <i class="pi pi-building text-xl"></i>
        </div>
        <div>
          <h2 class="m-0 text-900 text-lg md:text-xl font-bold">Configuración Comercial</h2>
          <span class="text-500 text-sm">Personaliza los datos base para tus cotizaciones</span>
        </div>
      </div>
      <Button label="Guardar Cambios" icon="pi pi-save" @click="guardar" :loading="guardando" class="w-full md:w-auto flex-shrink-0" />
    </div>

    <!-- Mensaje de Feedback -->
    <Message v-if="mensaje" :severity="mensaje.tipo" :closable="true" @close="mensaje = null" class="m-0">
      {{ mensaje.texto }}
    </Message>

    <!-- Tarjeta de Formulario -->
    <div class="surface-card p-3 md:p-4 shadow-1 border-round">
      <div class="grid formgrid p-fluid">

        <!-- Logo -->
        <div class="field col-12 mb-4 border-bottom-1 surface-border pb-4">
          <label class="font-bold block mb-3 text-700">Logo de la Empresa</label>
          <div class="flex flex-column sm:flex-row align-items-center gap-4">
            <!-- Preview del Logo -->
            <div class="border-1 surface-border border-round flex align-items-center justify-content-center surface-50 overflow-hidden"
                 style="width: 120px; height: 120px;">
              <img v-if="logoPreview" :src="logoPreview" alt="Logo Empresa" class="w-full h-full" style="object-fit: contain;" />
              <i v-else class="pi pi-image text-4xl text-400"></i>
            </div>

            <!-- Uploader -->
            <div class="flex flex-column gap-2 text-center sm:text-left">
              <!-- Usamos un FileUpload básico auto-procesado en local para la vista previa -->
              <FileUpload mode="basic" name="logo" accept="image/*" :maxFileSize="2000000"
                          chooseLabel="Seleccionar Imagen" customUpload @uploader="onLogoSeleccionado" auto
                          class="p-button-outlined" />
              <small class="text-500">Formato JPG o PNG. Tamaño máximo 2MB.</small>
            </div>
          </div>
        </div>

        <!-- Razón Social -->
        <div class="field col-12">
          <label for="razonSocial" class="font-bold block mb-2 text-700">Razón Social o Nombre <span class="text-red-500">*</span></label>
          <InputText id="razonSocial" v-model="formulario.razonSocial" placeholder="Ej: Construcciones y Remodelaciones SpA" class="w-full" />
        </div>

        <!-- IVA -->
        <div class="field col-12 md:col-6">
          <label for="iva" class="font-bold block mb-2 text-700">Porcentaje de IVA (%) <span class="text-red-500">*</span></label>
          <InputNumber id="iva" v-model="formulario.porcentajeIva" :min="0" :max="100" suffix=" %" class="w-full" />
        </div>

        <!-- Validez -->
        <div class="field col-12 md:col-6">
          <label for="validez" class="font-bold block mb-2 text-700">Validez de Cotización <span class="text-red-500">*</span></label>
          <InputNumber id="validez" v-model="formulario.diasValidez" :min="1" :max="365" suffix=" días" class="w-full" />
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  // Aquí importarías tu servicio, por ejemplo:
  // import { configuracionService } from '../../services/configuracionService';
  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Message from 'primevue/message';
  import FileUpload from 'primevue/fileupload';

  interface IConfiguracionComercial {
    razonSocial: string;
    porcentajeIva: number;
    diasValidez: number;
    logoBase64: string | null;
  }

  const guardando = ref(false);
  const mensaje = ref<{ tipo: string, texto: string } | null>(null);
  const logoPreview = ref<string | null>(null);

  const formulario = ref<IConfiguracionComercial>({
    razonSocial: '',
    porcentajeIva: 19, // Valor por defecto
    diasValidez: 15,   // Valor por defecto
    logoBase64: null
  });

  onMounted(async () => {
    await cargarConfiguracion();
  });

  const cargarConfiguracion = async () => {
    try {
      // const config = await configuracionService.obtener();
      // Simulamos la respuesta del backend por ahora:
      const config = {
        razonSocial: '',
        porcentajeIva: 19,
        diasValidez: 15,
        logoBase64: null
      };

      if (config) {
        formulario.value = { ...config };
        logoPreview.value = config.logoBase64;
      }
    } catch (error: any) {
      mostrarMensaje('error', 'Error al cargar la configuración: ' + error.message);
    }
  };

  // Procesa la imagen seleccionada y la convierte a Base64 para guardarla fácilmente
  const onLogoSeleccionado = async (event: any) => {
    const file = event.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      const base64String = reader.result as string;
      logoPreview.value = base64String;
      formulario.value.logoBase64 = base64String; // Listo para enviar al backend en el DTO
    };
    reader.onerror = (error) => {
      mostrarMensaje('error', 'Error al procesar la imagen.');
    };
  };

  const guardar = async () => {
    mensaje.value = null;

    if (!formulario.value.razonSocial.trim()) {
      mostrarMensaje('error', 'La razón social es obligatoria.');
      return;
    }

    guardando.value = true;
    try {
      // await configuracionService.guardar(formulario.value);

      // Simulación de guardado exitoso
      await new Promise(resolve => setTimeout(resolve, 800));
      mostrarMensaje('success', 'Configuración comercial guardada con éxito.');
    } catch (error: any) {
      mostrarMensaje('error', 'Error al guardar: ' + error.message);
    } finally {
      guardando.value = false;
    }
  };

  const mostrarMensaje = (tipo: string, texto: string) => {
    mensaje.value = { tipo, texto };
    if (tipo === 'success') {
      setTimeout(() => { mensaje.value = null; }, 3000);
    }
  };
</script>
