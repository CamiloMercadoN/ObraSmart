<template>
  <div class="flex flex-column gap-4 pb-4">
    <!-- Cabecera y Acciones -->
    <div class="flex justify-content-between align-items-center">
      <div class="flex align-items-center gap-3">
        <Button icon="pi pi-arrow-left" text rounded @click="volver" />
        <div>
          <h2 class="m-0 text-900 text-xl font-bold">
            {{ esEdicion ? 'Editar Estructura APU' : 'Nueva Estructura APU' }}
          </h2>
        </div>
      </div>
      <div class="flex gap-2">
        <Button label="Cancelar" outlined severity="secondary" @click="volver" />
        <Button label="Guardar APU" icon="pi pi-save" @click="guardar" :loading="guardando" />
      </div>
    </div>

    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''">
      {{ errorGlobal }}
    </Message>

    <!-- Datos Generales -->
    <div class="surface-card p-4 shadow-1 border-round">
      <h3 class="mt-0 mb-4 text-900 border-bottom-1 border-300 pb-2">Datos Generales</h3>

      <div class="grid formgrid p-fluid">
        <div class="field col-12 md:col-6">
          <label for="nombre" class="font-bold">Nombre del APU <span class="text-red-500">*</span></label>
          <InputText id="nombre" v-model="formulario.nombre" placeholder="Ej: Hormigón H30" />
        </div>

        <div class="field col-12 md:col-3">
          <label for="unidad" class="font-bold">Unidad de Medida <span class="text-red-500">*</span></label>
          <Select id="unidad" v-model="formulario.unidadMedidaId" :options="unidades"
                  optionLabel="nombre" optionValue="id" placeholder="Seleccione..." />
        </div>

        <div class="field col-12 md:col-3">
          <label for="etiquetas" class="font-bold">Clasificación</label>
          <MultiSelect id="etiquetas" v-model="formulario.etiquetasIds" :options="etiquetas"
                       optionLabel="nombre" optionValue="id" placeholder="Seleccione etiquetas"
                       display="chip" />
        </div>
      </div>
    </div>

    <!-- Receta (Componentes) -->
    <div class="surface-card p-4 shadow-1 border-round">
      <div class="flex justify-content-between align-items-center mb-4 border-bottom-1 border-300 pb-2">
        <h3 class="m-0 text-900">Desglose de Insumos (Receta)</h3>
        <div class="text-xl font-bold text-primary">
          Costo Directo Total: $ {{ formatearMoneda(costoTotalApu) }}
        </div>
      </div>

      <!-- Buscador y Agregador -->
      <div class="surface-ground border-1 surface-border p-3 border-round mb-4 flex flex-column md:flex-row gap-3 align-items-end">

        <!-- Contenedor del Buscador de Insumos -->
        <div class="flex-grow-1 w-full">
          <label class="font-bold text-sm block mb-2">Buscar Insumo / Equipo / Mano de Obra</label>

          <div class="flex gap-2 align-items-stretch">
            <Select v-model="insumoSeleccionado"
                    :options="insumosCatalogo"
                    filter
                    optionLabel="descripcion"
                    placeholder="Escriba para buscar..."
                    class="flex-grow-1"
                    style="min-width: 0;">
              <template #value="slotProps">
                <div v-if="slotProps.value" class="flex align-items-center gap-2 text-overflow-ellipsis overflow-hidden">
                  <Tag :value="slotProps.value.tipoInsumo" severity="info" />
                  <span class="white-space-nowrap text-overflow-ellipsis overflow-hidden">{{ slotProps.value.descripcion }}</span>
                </div>
                <span v-else>{{ slotProps.placeholder }}</span>
              </template>
              <template #option="slotProps">
                <div class="flex flex-column">
                  <span class="font-bold">{{ slotProps.option.descripcion }}</span>
                  <span class="text-sm text-500">
                    {{ slotProps.option.tipoInsumo }} | Precio: $ {{ formatearMoneda(slotProps.option.precioReferencia) }}
                  </span>
                </div>
              </template>
            </Select>

            <Button icon="pi pi-plus"
                    severity="success"
                    @click="abrirModalInsumo"
                    class="flex-shrink-0"
                    v-tooltip.top="'Crear nuevo insumo'" />
          </div>
        </div>

        <!-- Contenedor de Cantidad -->
        <div class="w-full md:w-15rem">
          <label class="font-bold text-sm block mb-2">Cantidad / Rendimiento</label>
          <InputNumber v-model="cantidadInput" :minFractionDigits="2" :maxFractionDigits="4" class="w-full" />
        </div>

        <Button label="Agregar" icon="pi pi-check" @click="agregarComponente"
                :disabled="!insumoSeleccionado || !cantidadInput || cantidadInput <= 0" />
      </div>

      <!-- Tabla de Componentes Agregados -->
      <DataTable :value="componentesVisuales" responsiveLayout="scroll">
        <template #empty>
          <div class="text-center p-3 text-500">No hay insumos agregados a esta receta.</div>
        </template>

        <Column field="tipoInsumo" header="Tipo">
          <template #body="slotProps">
            <Tag :value="slotProps.data.tipoInsumo" severity="secondary" />
          </template>
        </Column>
        <Column field="descripcionInsumo" header="Insumo"></Column>
        <Column header="Precio Unitario (Ref)">
          <template #body="slotProps">
            $ {{ formatearMoneda(slotProps.data.precioUnitarioReferencia) }}
          </template>
        </Column>
        <Column field="cantidad" header="Cantidad"></Column>
        <Column header="Subtotal" style="width: 15%">
          <template #body="slotProps">
            <span class="font-bold">$ {{ formatearMoneda(slotProps.data.precioUnitarioReferencia * slotProps.data.cantidad) }}</span>
          </template>
        </Column>
        <Column style="width: 5%">
          <template #body="slotProps">
            <Button icon="pi pi-trash" text rounded severity="danger"
                    @click="eliminarComponente(slotProps.index)" />
          </template>
        </Column>
      </DataTable>
    </div>
    <InsumoFormDialog v-model:visible="modalInsumoVisible"
                      :insumoData=null
                      :loading="guardandoInsumo"
                      :error="errorModalInsumo"
                      @guardar="guardarNuevoInsumo" />
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

  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import MultiSelect from 'primevue/multiselect';
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Tag from 'primevue/tag';
  import Message from 'primevue/message';

  const route = useRoute();
  const router = useRouter();

  const esEdicion = computed(() => !!route.params.id);
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
  const componentesVisuales = ref<IComponenteAPU[]>([]);

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
    if (esEdicion.value) {
      await cargarApuExistente(route.params.id as string);
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
      // Excluimos las plantillas para que el usuario arme con sus propios insumos
      insumosCatalogo.value = insms.filter(i => !i.esPlantilla);
    } catch (error: any) {
      errorGlobal.value = "Error al cargar catálogos: " + error.message;
    }
  };

  const cargarApuExistente = async (id: string) => {
    try {
      const apu = await apuService.obtenerPorId(id);
      formulario.value.nombre = apu.nombre;
      formulario.value.unidadMedidaId = apu.unidadMedidaId;
      formulario.value.etiquetasIds = apu.etiquetasIds;

      // Cargamos la lista visual con la información rica del backend
      componentesVisuales.value = [...apu.componentes];
    } catch (error: any) {
      errorGlobal.value = error.message;
    }
  };

  const agregarComponente = () => {
    const insumo = insumoSeleccionado.value;
    const cantidad = cantidadInput.value;

    if (!insumo || !cantidad || cantidad <= 0) return;

    const indexExistente = componentesVisuales.value.findIndex(c => c.insumoId === insumo.id);

    if (indexExistente >= 0) {
      const componenteExistente = componentesVisuales.value[indexExistente];

      if (componenteExistente) {
        componenteExistente.cantidad += cantidad;
        // Actualizamos el subtotal de la fila
        componenteExistente.subtotal = componenteExistente.cantidad * componenteExistente.precioUnitarioReferencia;
      }
    } else {
      componentesVisuales.value.push({
        insumoId: insumo.id ?? "",
        descripcionInsumo: insumo.descripcion,
        tipoInsumo: insumo.tipoInsumo,
        precioUnitarioReferencia: insumo.precioReferencia,
        cantidad: cantidad,
        subtotal: insumo.precioReferencia * cantidad
      });
    }

    // Limpiar inputs
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
      insumosCatalogo.value = insumosActualizados.filter(i => !i.esPlantilla);

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

</script>
