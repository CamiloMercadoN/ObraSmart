<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4">

    <!-- Cabecera y Acciones Responsivas -->
    <div class="flex flex-column md:flex-row md:align-items-center justify-content-between gap-3">
      <div class="flex align-items-center gap-2">
        <Button icon="pi pi-arrow-left" text rounded @click="volver" class="p-0 flex-shrink-0" style="width: 2.5rem; height: 2.5rem;" />
        <h2 class="m-0 app-text text-lg md:text-xl font-bold line-height-2">
          {{ esEdicion ? 'Editar Estructura APU' : 'Nueva Estructura APU' }}
        </h2>
      </div>
      <div class="flex gap-2 w-full md:w-auto">
        <Button label="Cancelar" outlined severity="secondary" @click="volver" class="flex-1 md:flex-none" />
        <Button label="Guardar APU" icon="pi pi-save" @click="guardar" :loading="guardando" class="flex-1 md:flex-none" />
      </div>
    </div>


    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="m-0">
      {{ errorGlobal }}
    </Message>

    <!-- Datos Generales -->
    <div class="app-panel p-3 md:p-4">
      <h3 class="mt-0 mb-3 app-text border-bottom-1 app-border-color pb-2 text-lg">Datos Generales</h3>

      <div class="grid formgrid p-fluid">
        <div class="field col-12 md:col-5">
          <label for="nombre" class="font-bold block mb-2 app-text">Nombre del APU <span class="text-red-500">*</span></label>
          <InputText id="nombre" v-model="formulario.nombre" placeholder="Ej: Hormigón H30" class="w-full" />
        </div>

        <div class="field col-12 md:col-3">
          <label for="unidad" class="font-bold block mb-2 app-text">Unidad de Medida <span class="text-red-500">*</span></label>
          <Select id="unidad" v-model="formulario.unidadMedidaId" :options="unidades"
                  optionLabel="nombre" optionValue="id" placeholder="Seleccione..." class="w-full" />
        </div>

        <div class="field col-12 md:col-4">
          <label for="etiquetas" class="font-bold block mb-2 app-text">Clasificación</label>
          <div class="flex gap-2 align-items-stretch">
            <MultiSelect id="etiquetas" v-model="formulario.etiquetasIds" :options="etiquetas"
                         optionLabel="nombre" optionValue="id" placeholder="Seleccione etiquetas"
                         display="chip" class="flex-grow-1" style="min-width: 0;" />
            <Button icon="pi pi-plus" severity="secondary" @click="mostrarDialogoEtiqueta = true" v-tooltip.top="'Nueva Etiqueta'" class="flex-shrink-0" />
          </div>
        </div>
      </div>
    </div>

    <!-- Receta (Componentes) -->
    <div class="app-panel p-3 md:p-4">
      <div class="flex flex-column lg:flex-row justify-content-between align-items-start lg:align-items-center mb-4 border-bottom-1 app-border-color pb-3 gap-3">
        <h3 class="m-0 app-text text-lg">Desglose de Insumos (Receta)</h3>
        <div class="text-xl font-bold text-primary app-subcard p-2">
          Costo Directo Total: $ {{ formatearMoneda(costoTotalApu) }}
        </div>
      </div>

      <!-- Buscador y Agregador Responsivo -->
      <div class="app-subcard border-1 app-border-color p-3 mb-4 flex flex-column md:flex-row gap-3 md:align-items-end">

        <!-- Contenedor del Buscador de Insumos -->
        <div class="flex-grow-1 w-full">
          <label class="font-bold text-sm block mb-2 app-text">Buscar Insumo / Equipo / Mano de Obra</label>

          <div class="flex flex-column sm:flex-row gap-2">
            <Select v-model="insumoSeleccionado"
                    :options="insumosCatalogo"
                    filter
                    optionLabel="descripcion"
                    placeholder="Escriba para buscar..."
                    class="w-full flex-grow-1"
                    appendTo="body">
              <template #value="slotProps">
                <div v-if="slotProps.value" class="flex align-items-center gap-2 text-overflow-ellipsis overflow-hidden">
                  <Tag :value="slotProps.value.tipoInsumo" severity="info" class="hidden sm:inline-flex" />
                  <span class="white-space-nowrap text-overflow-ellipsis overflow-hidden">
                    {{ slotProps.value.descripcion }}
                    <!-- Usamos directamente la propiedad que viene del backend -->
                    <span class="app-text-muted font-semibold">({{ slotProps.value.unidadMedidaNombre || 'S/U' }})</span>
                  </span>
                </div>
                <span v-else>{{ slotProps.placeholder }}</span>
              </template>
              <template #option="slotProps">
                <div class="flex flex-column w-full" style="max-width: 85vw;">
                  <span class="font-bold white-space-normal text-sm line-height-2 app-text" style="word-break: break-word;">{{ slotProps.option.descripcion }}</span>
                  <span class="text-sm app-text-muted mt-1 flex align-items-center flex-wrap gap-2">
                    <Tag :value="slotProps.option.tipoInsumo" severity="info" style="font-size: 0.6rem; padding: 2px 4px;" />
                    <!-- Usamos directamente la propiedad que viene del backend -->
                    <span class="font-semibold app-text-muted">{{ slotProps.option.unidadMedidaNombre || 'S/U' }}</span>
                    <span>|</span>
                    <span>Precio Ref: $ {{ formatearMoneda(slotProps.option.precioReferencia) }}</span>
                  </span>
                </div>
              </template>
            </Select>

            <Button icon="pi pi-plus"
                    severity="success"
                    @click="abrirModalInsumo"
                    class="w-full sm:w-auto flex-shrink-0"
                    label="Nuevo Insumo"
                    v-tooltip.top="'Crear nuevo insumo en el sistema'" />
          </div>
        </div>

        <!-- Contenedor de Cantidad y Agregar -->
        <div class="flex flex-column sm:flex-row gap-3 w-full md:w-auto">
          <div class="w-full sm:w-15rem">
            <label class="font-bold text-sm block mb-2 app-text">Cantidad / Rendimiento</label>
            <InputNumber v-model="cantidadInput" :minFractionDigits="2" :maxFractionDigits="4" class="w-full" inputClass="w-full" />
          </div>

          <Button label="Agregar a la Receta" icon="pi pi-check" @click="agregarComponente"
                  class="w-full sm:w-auto"
                  :disabled="!insumoSeleccionado || !cantidadInput || cantidadInput <= 0" />
        </div>
      </div>

      <!-- Estado Vacío -->
      <div v-if="componentesVisuales.length === 0" class="text-center p-4 app-text-muted app-subcard border-1 app-border-color border-dashed">
        No hay insumos agregados a esta receta. Búscalos arriba y añádelos.
      </div>

      <!-- Tarjetas de Componentes (Reemplazo del DataTable) -->
      <div v-else class="grid">
        <div v-for="(componente, index) in componentesVisuales" :key="index" class="col-12 lg:col-6 xl:col-4">
          <div class="app-card p-3 relative h-full flex flex-column">

            <Button icon="pi pi-times" severity="danger" text rounded class="absolute top-0 right-0 mt-1 mr-1" @click="eliminarComponente(index)" />

            <div class="flex flex-column gap-2 mt-2 flex-grow-1 pr-4">
              <span class="text-xs font-semibold app-text-muted mb-1">
                <Tag :value="componente.tipoInsumo" severity="secondary" class="mr-1" />
              </span>
              <span class="font-bold app-text line-height-2" style="word-break: break-word;">{{ componente.descripcionInsumo }}</span>
            </div>

            <div class="grid formgrid p-fluid mt-3">
              <div class="field col-6">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Precio Ref.</label>
                <div class="app-subcard p-2 text-sm font-semibold app-text text-center">
                  $ {{ formatearMoneda(componente.precioUnitarioReferencia) }}
                </div>
              </div>
              <div class="field col-6">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Cantidad <span class="app-text-muted font-normal">({{ componente.unidadMedidaNombre }})</span></label>
                <!-- Ojo aquí: Vinculado al v-model para edición rápida desde la tarjeta -->
                <InputNumber v-model="componente.cantidad" :minFractionDigits="0" :maxFractionDigits="4" class="w-full" inputClass="w-full text-center" />
              </div>
            </div>

            <div class="col-12 mt-auto p-0">
              <div class="flex justify-content-between align-items-center p-2 app-subcard w-full border-1 app-border-color">
                <span class="text-sm font-bold app-text">Subtotal:</span>
                <span class="font-bold text-green-600 text-lg">$ {{ formatearMoneda(componente.precioUnitarioReferencia * componente.cantidad) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <InsumoFormDialog v-model:visible="modalInsumoVisible"
                      :insumoData=null
                      :loading="guardandoInsumo"
                      :error="errorModalInsumo"
                      @guardar="guardarNuevoInsumo" />

    <EtiquetaFormDialog v-model:visible="mostrarDialogoEtiqueta"
                        @etiqueta-creada="onEtiquetaCreada" />

  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { apuService } from '../../services/apuService';
  import { insumoService } from '../../services/insumoService';
  import type { IEstructuraAPUUpsert, IComponenteAPU } from '../../interfaces/IApu';
  import type { IInsumo, IUnidadMedida, IEtiqueta } from '../../interfaces/IInsumo';
  import InsumoFormDialog from '../../components/InsumoFormDialog.vue';
  import EtiquetaFormDialog from '../../components/EtiquetaFormDialog.vue'

  interface IComponenteVisual extends IComponenteAPU {
    unidadMedidaNombre?: string;
  }

  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import MultiSelect from 'primevue/multiselect';
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import Tag from 'primevue/tag';
  import Message from 'primevue/message';

  const route = useRoute();
  const router = useRouter();

  const esEdicion = computed(() => !!route.params.id && !route.query.cloneId);
  const guardando = ref(false);
  const errorGlobal = ref('');

  // Catálogos base
  const unidades = ref<IUnidadMedida[]>([]);
  const etiquetas = ref<IEtiqueta[]>([]);
  const insumosCatalogo = ref<IInsumo[]>([]);

  // Estado del formulario
  const formulario = ref<IEstructuraAPUUpsert>({
    nombre: '',
    unidadMedidaId: 0,
    etiquetasIds: [],
    componentes: []
  });

  // Estado visual de la tabla
  const componentesVisuales = ref<IComponenteVisual[]>([]);

  // Estado del agregador
  const insumoSeleccionado = ref<IInsumo | null>(null);
  const cantidadInput = ref<number | null>(null);

  // Estado de insumo
  const modalInsumoVisible = ref(false);
  const guardandoInsumo = ref(false);
  const errorModalInsumo = ref('');

  // Cálculo en tiempo real del costo del APU
  const costoTotalApu = computed(() => {
    return componentesVisuales.value.reduce((total, comp) => {
      return total + (comp.precioUnitarioReferencia * comp.cantidad);
    }, 0);
  });

  onMounted(async () => {
    await cargarCatalogos();
    if (route.params.id) {
      await cargarApuExistente(route.params.id as string, false);
    } else if (route.query.cloneId) {
      await cargarApuExistente(route.query.cloneId as string, true);
    }
  });

  const cargarCatalogos = async () => {
    try {
      const [unds, etqs, insms] = await Promise.all([
        insumoService.obtenerUnidadesMedida(),
        insumoService.obtenerEtiquetas(),
        insumoService.obtenerTodos()
      ]);
      unidades.value = unds;
      etiquetas.value = etqs;
      insumosCatalogo.value = insms
    } catch (error: any) {
      errorGlobal.value = "Error al cargar catálogos: " + error.message;
    }
  };

const cargarApuExistente = async (id: string, esClonacion: boolean) => {
    try {
      const apu = await apuService.obtenerPorId(id);

      if (esClonacion) {
        formulario.value.nombre = `${apu.nombre} (Copia)`;
      } else {
        formulario.value.nombre = apu.nombre;
      }

      formulario.value.unidadMedidaId = apu.unidadMedidaId;
      formulario.value.etiquetasIds = apu.etiquetasIds;


      // Enriquecemos la lista visual cruzando con el catálogo de insumos y unidades
      componentesVisuales.value = apu.componentes.map(comp => {
        const insumoRef = insumosCatalogo.value.find(i => i.id === comp.insumoId);
        const unidadRef = unidades.value.find(u => u.id === insumoRef?.unidadMedidaId);

        return {
          ...comp,
          unidadMedidaNombre: unidadRef ? unidadRef.nombre : 'S/U'
        };
      });
    } catch (error: any) {
      errorGlobal.value = error.message;
    }
  };

  const agregarComponente = () => {
    const insumo = insumoSeleccionado.value;
    const cantidad = cantidadInput.value;

    if (!insumo || !cantidad || cantidad <= 0) return;

    const indexExistente = componentesVisuales.value.findIndex(c => c.insumoId === insumo.id);

    // Buscamos el nombre de la unidad del insumo seleccionado
    const unidadRef = unidades.value.find(u => u.id === insumo.unidadMedidaId);

    if (indexExistente >= 0) {
      const componenteExistente = componentesVisuales.value[indexExistente];
      if (componenteExistente) {
        componenteExistente.cantidad += cantidad;
        componenteExistente.subtotal = componenteExistente.cantidad * componenteExistente.precioUnitarioReferencia;
      }
    } else {
      componentesVisuales.value.push({
        insumoId: insumo.id ?? "",
        descripcionInsumo: insumo.descripcion,
        tipoInsumo: insumo.tipoInsumo,
        precioUnitarioReferencia: insumo.precioReferencia,
        cantidad: cantidad,
        subtotal: insumo.precioReferencia * cantidad,
        unidadMedidaNombre: unidadRef ? unidadRef.nombre : 'S/U'
      });
    }

    insumoSeleccionado.value = null;
    cantidadInput.value = null;
  };

  const eliminarComponente = (index: number) => {
    componentesVisuales.value.splice(index, 1);
  };

  const guardar = async () => {
    errorGlobal.value = '';

    if (!formulario.value.nombre || formulario.value.unidadMedidaId === 0) {
      errorGlobal.value = 'Por favor, completa el nombre y la unidad de medida.';
      return;
    }
    if (componentesVisuales.value.length === 0) {
      errorGlobal.value = 'El APU debe contener al menos un insumo en su receta.';
      return;
    }

    // Mapeo final: Convertir la vista visual al DTO estricto
    formulario.value.componentes = componentesVisuales.value.map(c => ({
      insumoId: c.insumoId,
      cantidad: c.cantidad
    }));

    guardando.value = true;
    try {
      if (esEdicion.value) {
        await apuService.actualizar(route.params.id as string, formulario.value);
      } else {
        await apuService.crear(formulario.value);
      }
      router.push('/apus');
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      guardando.value = false;
    }
  };

  const volver = () => {
    router.push('/apus');
  };

  const formatearMoneda = (valor: number) => {
    return valor.toLocaleString('es-CL');
  };

  const abrirModalInsumo = () => {
    errorModalInsumo.value = '';
    modalInsumoVisible.value = true;
  };

  const guardarNuevoInsumo = async (payload: IInsumo) => {
    errorModalInsumo.value = '';
    guardandoInsumo.value = true;

    try {
      // Enviamos a la API a crear el insumo usando el payload
      const insumoIdCreado = await insumoService.crear(payload);

      // Recargamos la lista de catálogo
      const insumosActualizados = await insumoService.obtenerTodos();
      insumosCatalogo.value = insumosActualizados;

      // Auto-seleccionamos el insumo
      const insumoRecienCreado = insumosCatalogo.value.find(i => i.id === insumoIdCreado);
      if (insumoRecienCreado) {
        insumoSeleccionado.value = insumoRecienCreado;
      }

      modalInsumoVisible.value = false;
    } catch (error: any) {
      errorModalInsumo.value = error.message;
    } finally {
      guardandoInsumo.value = false;
    }
  };

  // --- Estado para Nueva Etiqueta ---
  const mostrarDialogoEtiqueta = ref(false);

  const onEtiquetaCreada = async (tagCreada: { id: string, nombre: string, colorHex: string }) => {
    // Recargamos la lista completa de etiquetas para asegurar consistencia
    etiquetas.value = await insumoService.obtenerEtiquetas();

    // Auto-seleccionar la etiqueta
    if (!formulario.value.etiquetasIds) formulario.value.etiquetasIds = [];
    formulario.value.etiquetasIds.push(tagCreada.id);
  };

</script>
