<template>
  <div class="flex flex-column gap-4 pb-4">

    <!-- Cabecera -->
    <div class="flex align-items-center gap-3">
      <div class="bg-indigo-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
        <i class="pi pi-chart-line text-xl"></i>
      </div>
      <div>
        <h2 class="m-0 text-900 text-xl font-bold">Actualización de Precios Base</h2>
        <span class="text-500 text-sm">Gestiona reajustes masivos por lotes o mediante importación de archivos</span>
      </div>
    </div>

    <!-- Panel de Resultados (Global) -->
    <Message v-if="resultadoProceso" severity="info" :closable="true" @close="resultadoProceso = null" class="m-0">
      <div class="flex flex-column gap-2">
        <div class="font-bold">Proceso Finalizado</div>
        <div>Registros procesados: <strong>{{ resultadoProceso.procesados }}</strong></div>
        <div>Registros actualizados exitosamente: <strong>{{ resultadoProceso.actualizados }}</strong></div>

        <div v-if="resultadoProceso.detalleErrores.length > 0" class="mt-2 p-2 bg-yellow-50 text-yellow-900 border-round text-sm" style="max-height: 150px; overflow-y: auto;">
          <div class="font-bold mb-1">Advertencias y Errores:</div>
          <div v-for="(err, index) in resultadoProceso.detalleErrores" :key="index">
            - {{ err }}
          </div>
        </div>
      </div>
    </Message>

    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="m-0">
      {{ errorGlobal }}
    </Message>

    <div class="grid">
      <!-- Reajuste por Lote -->
      <div class="col-12 md:col-6">
        <div class="surface-card p-4 shadow-1 border-round h-full flex flex-column gap-4">
          <div>
            <h3 class="m-0 text-lg font-bold text-900 mb-1">Reajuste por Filtros</h3>
            <span class="text-500 text-sm">Aplica un porcentaje o monto fijo a un grupo específico de insumos.</span>
          </div>

          <div class="flex flex-column gap-3">
            <div class="flex flex-column gap-2">
              <label class="font-bold text-sm">1. Selecciona el Tipo de Insumo (Opcional)</label>
              <Select v-model="loteData.tipoInsumo" :options="tiposInsumo" placeholder="Todos los tipos" showClear class="w-full" />
            </div>

            <div class="flex flex-column gap-2">
              <label class="font-bold text-sm">2. Selecciona la Etiqueta (Opcional)</label>
              <Select v-model="loteData.etiquetaId" :options="etiquetas" optionLabel="nombre" optionValue="id" placeholder="Todas las etiquetas" showClear class="w-full" />
            </div>

            <div class="flex flex-column gap-2">
              <label class="font-bold text-sm">3. Tipo de Ajuste</label>
              <Select v-model="loteData.esPorcentaje" :options="tiposAjuste" optionLabel="label" optionValue="value" class="w-full" />
            </div>

            <div class="flex flex-column gap-2">
              <label class="font-bold text-sm">4. Valor a aplicar (Usa negativo para descuentos)</label>
              <InputNumber v-model="loteData.valor"
                           :prefix="!loteData.esPorcentaje ? '$ ' : ''"
                           :suffix="loteData.esPorcentaje ? ' %' : ''"
                           :minFractionDigits="0"
                           :maxFractionDigits="2"
                           class="w-full" />
            </div>
          </div>

          <div class="mt-auto pt-4 flex justify-content-end">
            <Button label="Aplicar Reajuste Masivo" icon="pi pi-check" @click="ejecutarReajuste" :loading="cargandoLote" :disabled="loteData.valor === 0" />
          </div>
        </div>
      </div>

      <!-- Importación CSV -->
      <div class="col-12 md:col-6">
        <div class="surface-card p-4 shadow-1 border-round h-full flex flex-column gap-4">
          <div>
            <h3 class="m-0 text-lg font-bold text-900 mb-1">Importación desde CSV</h3>
            <span class="text-500 text-sm">Sube un archivo con los precios actualizados por tu proveedor.</span>
          </div>

          <div class="bg-blue-50 text-blue-900 p-3 border-round text-sm flex flex-column gap-2">
            <div class="flex gap-2">
              <i class="pi pi-info-circle mt-1"></i>
              <div>
                <strong>Paso 1:</strong> Descarga la plantilla oficial. Este archivo contiene todos tus insumos actuales.<br>
                <strong>Paso 2:</strong> Ábrelo en Excel, modifica la columna <code>NuevoPrecio</code> y guárdalo.<br>
                <strong>Paso 3:</strong> Sube el archivo modificado aquí.
              </div>
            </div>
            <div class="mt-2">
              <Button label="Descargar Plantilla CSV" icon="pi pi-download" size="small" @click="insumoPrecioService.descargarPlantilla()" />
            </div>
          </div>

          <div class="border-2 border-dashed border-300 border-round p-4 flex flex-column align-items-center justify-content-center gap-3" style="min-height: 200px;">
            <i class="pi pi-cloud-upload text-4xl text-500"></i>

            <div class="text-center">
              <span class="block text-900 font-bold mb-1">{{ archivoSeleccionado ? archivoSeleccionado.name : 'Ningún archivo seleccionado' }}</span>
              <span v-if="archivoSeleccionado" class="text-500 text-sm">{{ (archivoSeleccionado.size / 1024).toFixed(2) }} KB</span>
            </div>

            <div class="flex gap-2">
              <!-- Botón oculto nativo -->
              <input type="file" ref="fileInputRef" accept=".csv" class="hidden" @change="manejarSeleccionArchivo" />

              <Button v-if="!archivoSeleccionado" label="Explorar Archivo" icon="pi pi-search" outlined severity="secondary" @click="activarInputArchivo" />
              <Button v-else label="Cambiar" icon="pi pi-refresh" outlined severity="secondary" @click="activarInputArchivo" />
              <Button v-if="archivoSeleccionado" icon="pi pi-times" outlined severity="danger" @click="limpiarArchivo" />
            </div>
          </div>

          <div class="mt-auto pt-4 flex justify-content-end">
            <Button label="Procesar Archivo CSV" icon="pi pi-upload" severity="success" @click="ejecutarImportacionCsv" :loading="cargandoCsv" :disabled="!archivoSeleccionado" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { insumoPrecioService, type IReajusteLote, type IResumenProcesamiento } from '../../services/insumoPrecioService';
  import { insumoService } from '../../services/insumoService';
  import type { IEtiqueta } from '../../interfaces/IInsumo';

  import Select from 'primevue/select';
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import Message from 'primevue/message';

  // Estado de catálogos para filtros
  const tiposInsumo = ref(['Material', 'Mano de Obra', 'Equipo']);
  const etiquetas = ref<IEtiqueta[]>([]);

  // Estado General
  const resultadoProceso = ref<IResumenProcesamiento | null>(null);
  const errorGlobal = ref('');

  // Estado Lote
  const cargandoLote = ref(false);
  const loteData = ref<IReajusteLote>({
    tipoInsumo: null,
    etiquetaId: null,
    esPorcentaje: true,
    valor: 0
  });

  const tiposAjuste = ref([
    { label: 'Porcentaje (%)', value: true },
    { label: 'Monto Fijo ($)', value: false }
  ]);

  // Estado CSV
  const fileInputRef = ref<HTMLInputElement | null>(null);
  const archivoSeleccionado = ref<File | null>(null);
  const cargandoCsv = ref(false);

  onMounted(async () => {
    try {
      etiquetas.value = await insumoService.obtenerEtiquetas();
    } catch (err) {
      console.error("No se pudieron pre-cargar las etiquetas", err);
    }
  });

  // Lógica Lote
  const ejecutarReajuste = async () => {
    if (loteData.value.valor === 0) return;

    cargandoLote.value = true;
    resultadoProceso.value = null;
    errorGlobal.value = '';

    try {
      const resumen = await insumoPrecioService.reajustarLote(loteData.value);
      resultadoProceso.value = resumen;
      loteData.value.valor = 0; // Resetear valor por seguridad
    } catch (err: any) {
      errorGlobal.value = err.message || 'Ocurrió un error al procesar el lote.';
    } finally {
      cargandoLote.value = false;
    }
  };

  // Lógica CSV
  const activarInputArchivo = () => {
    fileInputRef.value?.click();
  };

  const manejarSeleccionArchivo = (event: Event) => {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];

    if (file) {
      if (file.name.toLowerCase().endsWith('.csv')) {
        archivoSeleccionado.value = file;
      } else {
        errorGlobal.value = 'Por favor, selecciona un archivo CSV válido.';
        limpiarArchivo();
      }
    }
  };

  const limpiarArchivo = () => {
    archivoSeleccionado.value = null;
    if (fileInputRef.value) {
      fileInputRef.value.value = '';
    }
  };

  const ejecutarImportacionCsv = async () => {
    if (!archivoSeleccionado.value) return;

    cargandoCsv.value = true;
    resultadoProceso.value = null;
    errorGlobal.value = '';

    try {
      const resumen = await insumoPrecioService.importarCsv(archivoSeleccionado.value);
      resultadoProceso.value = resumen;
      limpiarArchivo();
    } catch (err: any) {
      errorGlobal.value = err.message || 'Ocurrió un error al importar el archivo CSV.';
    } finally {
      cargandoCsv.value = false;
    }
  };
</script>
