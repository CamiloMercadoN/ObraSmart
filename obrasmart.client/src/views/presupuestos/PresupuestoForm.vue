<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4">

    <!-- Header Responsivo -->
    <div class="flex flex-column md:flex-row md:align-items-center justify-content-between gap-3">
      <div class="flex align-items-center gap-2">
        <Button icon="pi pi-arrow-left" text rounded @click="volver" class="p-0 flex-shrink-0" style="width: 2.5rem; height: 2.5rem;" />
        <h2 class="m-0 app-text text-lg md:text-xl font-bold line-height-2">
          {{ esEdicion ? 'Editar Presupuesto' : 'Nuevo Presupuesto' }}
        </h2>
      </div>
      <div class="flex gap-2 w-full md:w-auto">
        <Button label="Cancelar" severity="secondary" outlined @click="volver" class="flex-1 md:flex-none" />
        <Button label="Guardar" icon="pi pi-save" @click="guardar" :loading="guardando" class="flex-1 md:flex-none" />
      </div>
    </div>

    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="m-0">
      {{ errorGlobal }}
    </Message>

    <!-- Datos Generales -->
    <div class="app-panel p-3 md:p-4">
      <h3 class="mt-0 mb-3 app-text border-bottom-1 app-border-color pb-2 text-lg">Datos Generales</h3>
      <div class="grid formgrid p-fluid">
        <div class="field col-12 md:col-6">
          <label for="proyecto" class="font-bold block mb-2 app-text">Nombre del Proyecto <span class="text-red-500">*</span></label>
          <InputText id="proyecto" v-model="formulario.nombreProyecto" placeholder="Ej: Remodelación Oficina Central" class="w-full" />
        </div>
        <div class="field col-12 md:col-6">
          <label for="cliente" class="font-bold block mb-2 app-text">Cliente</label>
          <Select id="cliente" v-model="formulario.clienteId" :options="clientes"
                  optionLabel="nombre" optionValue="id" placeholder="Seleccione un cliente" showClear class="w-full" />
        </div>
      </div>
    </div>

    <!-- Detalle (Lista de Tarjetas en Grilla) -->
    <div class="app-panel p-3 md:p-4">
      <div class="flex flex-column lg:flex-row justify-content-between align-items-start lg:align-items-center mb-4 border-bottom-1 app-border-color pb-3 gap-3">
        <h3 class="m-0 app-text text-lg">Detalle del Presupuesto</h3>
        <div class="flex flex-column sm:flex-row gap-2 w-full lg:w-auto">
          <Button label="Nuevo Ítem Ad-Hoc" icon="pi pi-plus" severity="secondary" outlined @click="agregarItemManual" class="w-full sm:w-auto" />
          <Button label="Catálogo APU" icon="pi pi-search" @click="mostrarModalCatalog = true" class="w-full sm:w-auto" />
        </div>
      </div>

      <div v-if="formulario.items.length === 0" class="app-subcard app-text-muted text-center p-4 border-1 app-border-color border-dashed">
        Aún no hay ítems en este presupuesto. Agrega una estructura APU desde el catálogo o crea una nueva.
      </div>

      <!-- Grilla de Ítems -->
      <div v-else class="grid">
        <!-- SE AGREGÓ EL ID DINÁMICO PARA EL SCROLL -->
        <div v-for="(item, index) in formulario.items" :key="index" class="col-12 lg:col-6" :id="'item-tarjeta-' + index">
          <div class="app-card p-3 relative h-full flex flex-column">

            <div class="flex justify-content-between align-items-center mb-3 border-bottom-1 app-border-color pb-2">
              <span class="font-bold app-text">Ítem {{ index + 1 }}</span>
              <Button icon="pi pi-trash" severity="danger" text rounded size="small" @click="eliminarItem(index)" />
            </div>

            <div class="grid formgrid p-fluid flex-grow-1">
              <div class="field col-12">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Descripción</label>
                <InputText v-model="item.descripcion" class="w-full input-descripcion" />
              </div>

              <div class="field col-6">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Unidad</label>
                <Select v-model="item.unidadMedidaId" :options="unidadesCatalogo" optionLabel="nombre" optionValue="id" class="w-full" appendTo="body" />
              </div>

              <div class="field col-6">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Cantidad</label>
                <InputNumber v-model="item.cantidadItem" :minFractionDigits="0" :maxFractionDigits="2" class="w-full" inputClass="w-full" />
              </div>

              <div class="col-12 flex flex-column gap-2 mt-auto">
                <div class="flex justify-content-between align-items-center p-2 app-subcard">
                  <span class="text-sm app-text-muted">P. Unitario:</span>
                  <span class="font-semibold app-text">$ {{ formatearMoneda(item.precioUnitarioCalculado) }}</span>
                </div>
                <div class="flex justify-content-between align-items-center p-2 bg-blue-50 border-round border-1 border-blue-100">
                  <span class="text-sm font-bold text-blue-900">Subtotal Ítem:</span>
                  <span class="font-bold text-blue-700 text-lg">$ {{ formatearMoneda(item.cantidadItem * (item.precioUnitarioCalculado || 0)) }}</span>
                </div>
                <Button label="Gestionar Insumos / Recursos" icon="pi pi-list" severity="info" outlined class="w-full mt-2" @click="abrirDetalleRecursos(index)" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Total del Presupuesto -->
      <div class="flex justify-content-end mt-4">
        <div class="w-full sm:w-20rem app-subcard p-3 border-1 app-border-color">
          <div class="flex justify-content-between mb-2 align-items-center">
            <span class="app-text font-bold">Subtotal Neto:</span>
            <span class="font-bold text-2xl text-green-600">$ {{ formatearMoneda(totalCalculadoFrontend) }}</span>
          </div>
          <div class="text-xs app-text-muted text-right">
            * El cálculo de IVA y Total se procesará al guardar.
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Catálogo APU -->
    <Dialog v-model:visible="mostrarModalCatalog" modal header="Seleccionar Estructura APU" :style="{ width: '90vw', maxWidth: '1000px' }" :breakpoints="{ '960px': '95vw' }">
      <div class="flex flex-column sm:flex-row gap-3 mb-4">
        <InputText v-model="filtrosApu.global.value" placeholder="Buscar APU por nombre..." class="w-full" />
        <Select v-model="filtrosApu.etiquetasIds.value" :options="catalogoEtiquetas" optionLabel="nombre" optionValue="id" placeholder="Filtrar por Etiqueta" showClear class="w-full sm:w-15rem" appendTo="body" />
      </div>

      <div v-if="apusFiltrados.length === 0" class="text-center p-4 app-text-muted app-subcard">
        No se encontraron APUs con esos filtros.
      </div>

      <div class="grid">
        <div v-for="apu in apusFiltrados" :key="apu.id" class="col-12 lg:col-6">
          <div class="app-card p-3 h-full flex flex-column gap-3">
            <div>
              <h4 class="m-0 app-text mb-1">{{ apu.nombre }}</h4>
              <span class="text-sm app-text-muted">Unidad: {{ apu.unidadMedidaNombre }}</span>
            </div>

            <div class="flex flex-wrap gap-1">
              <span v-for="tagId in apu.etiquetasIds" :key="tagId" class="text-xs px-2 py-1 border-round font-semibold" :style="obtenerEstiloEtiqueta(tagId)">
                {{ obtenerNombreEtiqueta(tagId) }}
              </span>
            </div>

            <div class="flex justify-content-between align-items-center mt-auto pt-3 border-top-1 app-border-color">
              <span class="font-bold text-green-600 text-lg">$ {{ formatearMoneda(apu.costoTotalCalculado) }}</span>
              <Button label="Seleccionar" icon="pi pi-check" size="small" @click="seleccionarApu(apu)" />
            </div>
          </div>
        </div>
      </div>
    </Dialog>

    <!-- Modal para Gestionar los Insumos -->
    <Dialog v-model:visible="mostrarModalRecursos" modal :header="'Recursos del Ítem'" :style="{ width: '90vw', maxWidth: '1200px' }" :breakpoints="{ '960px': '95vw' }">
      <div class="flex flex-column sm:flex-row gap-2 mb-4">
        <Select v-model="insumoTemporal" :options="insumosCatalogo" optionLabel="descripcion" placeholder="Buscar insumo para agregar..." filter class="w-full" appendTo="body">
          <template #option="slotProps">
            <div class="white-space-normal text-sm line-height-2" style="word-break: break-word; max-width: 80vw;">
              {{ slotProps.option.descripcion }}
            </div>
          </template>
        </Select>
        <Button icon="pi pi-plus" label="Agregar" @click="agregarRecursoAlItem" :disabled="!insumoTemporal" class="w-full sm:w-auto flex-shrink-0" />
      </div>

      <div v-if="itemEnEdicion?.recursos.length === 0" class="text-center p-4 app-text-muted app-subcard border-1 app-border-color border-dashed">
        Este ítem no tiene insumos. Agrega uno para calcular su precio.
      </div>

      <div v-else class="grid">
        <div v-for="(recurso, index) in itemEnEdicion?.recursos" :key="index" class="col-12 lg:col-6">
          <div class="app-card p-3 relative h-full flex flex-column">

            <Button icon="pi pi-times" severity="danger" text rounded class="absolute top-0 right-0 mt-1 mr-1" @click="eliminarRecurso(index)" />

            <div class="grid formgrid p-fluid mt-2 flex-grow-1">
              <div class="field col-12 md:col-11">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Insumo</label>
                <InputText v-model="recurso.descripcionCongelada" class="w-full" />
              </div>

              <div class="field col-12 sm:col-4">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Unidad</label>
                <Select v-model="recurso.unidadMedidaId" :options="unidadesCatalogo" optionLabel="nombre" optionValue="id" class="w-full" appendTo="body" />
              </div>

              <div class="field col-6 sm:col-4">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Cantidad</label>
                <InputNumber v-model="recurso.cantidad" :minFractionDigits="0" :maxFractionDigits="4" class="w-full" inputClass="w-full" />
              </div>

              <div class="field col-6 sm:col-4">
                <label class="block mb-2 text-sm font-semibold app-text-muted">Precio Ref.</label>
                <InputNumber v-model="recurso.precioUnitarioCongelado" mode="currency" currency="CLP" locale="es-CL" class="w-full" inputClass="w-full" />
              </div>

              <div class="col-12 mt-auto">
                <div class="flex justify-content-between align-items-center p-2 app-subcard w-full">
                  <span class="text-sm font-bold app-text">Costo Total:</span>
                  <span class="font-bold text-green-600 text-lg">$ {{ formatearMoneda(recurso.cantidad * recurso.precioUnitarioCongelado) }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex w-full justify-content-end mt-3">
          <Button label="Cerrar Insumos" icon="pi pi-check" @click="mostrarModalRecursos = false" class="w-full sm:w-auto" />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, watch, nextTick } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { presupuestoService } from '../../services/presupuestoService';
  import { clienteService } from '../../services/clienteService';
  import { apuService } from '../../services/apuService';
  import { insumoService } from '../../services/insumoService';
  import type { IPresupuesto, IItemPresupuesto, IRecursoItemPresupuesto } from '../../interfaces/IPresupuesto';
  import type { ICliente } from '../../interfaces/ICliente';
  import type { IEstructuraAPU } from '../../interfaces/IApu';
  import type { IEtiqueta, IInsumo, IUnidadMedida } from '../../interfaces/IInsumo';

  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import InputNumber from 'primevue/inputnumber';
  import Message from 'primevue/message';
  import Dialog from 'primevue/dialog';

  const route = useRoute();
  const router = useRouter();

  const esEdicion = computed(() => !!route.params.id && !route.query.cloneId);
  const errorGlobal = ref('');
  const guardando = ref(false);

  const clientes = ref<ICliente[]>([]);
  const apusCatalogo = ref<IEstructuraAPU[]>([]);
  const catalogoEtiquetas = ref<IEtiqueta[]>([]);
  const insumosCatalogo = ref<IInsumo[]>([]);
  const unidadesCatalogo = ref<IUnidadMedida[]>([]);
  const cargandoApus = ref(false);

  const mostrarModalCatalog = ref(false);
  const filtrosApu = ref({
    global: { value: '' },
    etiquetasIds: { value: null }
  });

  const mostrarModalRecursos = ref(false);
  const itemEnEdicionIndex = ref<number | null>(null);
  const itemEnEdicion = computed(() => itemEnEdicionIndex.value !== null ? formulario.value.items[itemEnEdicionIndex.value] : null);
  const insumoTemporal = ref<IInsumo | null>(null);
  const totalCalculadoFrontend = ref(0);

  const formulario = ref<IPresupuesto>({
    nombreProyecto: '',
    clienteId: null,
    items: [],
    esPlantilla: false
  });

  const apusFiltrados = computed(() => {
    return apusCatalogo.value.filter(apu => {
      let coincideTexto = true;
      let coincideEtiqueta = true;

      if (filtrosApu.value.global.value) {
        coincideTexto = apu.nombre.toLowerCase().includes(filtrosApu.value.global.value.toLowerCase());
      }

      if (filtrosApu.value.etiquetasIds.value) {
        coincideEtiqueta = apu.etiquetasIds.includes(filtrosApu.value.etiquetasIds.value as string);
      }

      return coincideTexto && coincideEtiqueta;
    });
  });

  watch(() => formulario.value.items, (nuevosItems) => {
    let totalMonto = 0;

    if (nuevosItems && nuevosItems.length > 0) {
      nuevosItems.forEach(item => {
        let costoUnitarioItem = 0;

        if (item.recursos && item.recursos.length > 0) {
          item.recursos.forEach(r => {
            costoUnitarioItem += (r.cantidad * r.precioUnitarioCongelado);
          });
        }

        item.precioUnitarioCalculado = costoUnitarioItem;
        totalMonto += (item.cantidadItem * item.precioUnitarioCalculado);
      });
    }

    totalCalculadoFrontend.value = totalMonto;
  }, { deep: true });

  onMounted(async () => {
    await cargarDiccionarios();

    if (route.params.id) {
      await cargarPresupuesto(route.params.id as string, false);
    } else if (route.query.cloneId) {
      await cargarPresupuesto(route.query.cloneId as string, true);
    }
  });

  const cargarDiccionarios = async () => {
    try {
      const [clientesData, etiquetasData, insumosData, unidadesData] = await Promise.all([
        clienteService.obtenerTodos(),
        insumoService.obtenerEtiquetas(),
        insumoService.obtenerTodos(),
        insumoService.obtenerUnidadesMedida()
      ]);

      clientes.value = clientesData;
      catalogoEtiquetas.value = etiquetasData;
      insumosCatalogo.value = insumosData;
      unidadesCatalogo.value = unidadesData;

      cargandoApus.value = true;
      apusCatalogo.value = await apuService.obtenerTodos();
    } catch (error) {
      console.error("Error cargando dependencias", error);
    } finally {
      cargandoApus.value = false;
    }
  };

  const cargarPresupuesto = async (id: string, esClonacion: boolean) => {
    try {
      const data = await presupuestoService.obtenerPorId(id);

      if (esClonacion) {
        delete data.id;
        data.nombreProyecto = `${data.nombreProyecto} (Copia)`;

        if (data.items && data.items.length > 0) {
          data.items.forEach(i => {
            delete i.id;
            if (i.recursos && i.recursos.length > 0) {
              i.recursos.forEach(r => delete r.id);
            } else {
              i.recursos = [];
            }
          });
        } else {
          data.items = [];
        }
      }

      formulario.value = data;
    } catch (error: any) {
      errorGlobal.value = "Error al cargar el presupuesto: " + error.message;
    }
  };

  const hacerScrollAlNuevoItem = async () => {
    await nextTick(); // Espera a que Vue repinte el DOM
    const indiceNuevo = formulario.value.items.length - 1;
    const elemento = document.getElementById(`item-tarjeta-${indiceNuevo}`);

    if (elemento) {
      elemento.scrollIntoView({ behavior: 'smooth', block: 'center' });

      // Opcional: Hacer auto-focus en el input de descripción para escribir de inmediato
      const inputFocus = elemento.querySelector('.input-descripcion') as HTMLElement;
      if (inputFocus) {
        setTimeout(() => inputFocus.focus(), 300); // Pequeño retraso visual
      }
    }
  };

  const agregarItemManual = async () => {
    formulario.value.items.push({
      descripcion: '', // Dejamos en blanco para que escriba inmediatamente
      cantidadItem: 1,
      unidadMedidaId: 1,
      precioUnitarioCalculado: 0,
      recursos: []
    });

    await hacerScrollAlNuevoItem();
  };

  const seleccionarApu = async (apuSeleccionado: IEstructuraAPU) => {
    try {
      const apuCompleto = await apuService.obtenerPorId(apuSeleccionado.id!);

      formulario.value.items.push({
        estructuraAPUOrigenId: apuCompleto.id,
        descripcion: apuCompleto.nombre,
        cantidadItem: 1,
        unidadMedidaId: apuCompleto.unidadMedidaId,
        precioUnitarioCalculado: apuCompleto.costoTotalCalculado,
        recursos: apuCompleto.componentes!.map(comp => {
          const insumoReal = insumosCatalogo.value.find(i => i.id === comp.insumoId);
          return {
            tipoInsumo: comp.tipoInsumo,
            descripcionCongelada: comp.descripcionInsumo,
            cantidad: comp.cantidad,
            precioUnitarioCongelado: comp.precioUnitarioReferencia,
            unidadMedidaId: insumoReal ? insumoReal.unidadMedidaId : 1
          };
        })
      });

      mostrarModalCatalog.value = false;

      // Al cerrar el modal, le damos tiempo a PrimeVue de ocultar el overlay antes de hacer scroll
      setTimeout(async () => {
        await hacerScrollAlNuevoItem();
      }, 200);

    } catch (error: any) {
      errorGlobal.value = "Error al cargar la receta del APU: " + error.message;
    }
  };

  const eliminarItem = (index: number) => {
    formulario.value.items.splice(index, 1);
  };

  const abrirDetalleRecursos = (index: number) => {
    itemEnEdicionIndex.value = index;
    insumoTemporal.value = null;
    mostrarModalRecursos.value = true;
  };

  const agregarRecursoAlItem = () => {
    if (insumoTemporal.value && itemEnEdicion.value) {
      itemEnEdicion.value.recursos.push({
        tipoInsumo: insumoTemporal.value.tipoInsumo,
        descripcionCongelada: insumoTemporal.value.descripcion,
        cantidad: 1,
        precioUnitarioCongelado: insumoTemporal.value.precioReferencia,
        unidadMedidaId: insumoTemporal.value.unidadMedidaId
      });
      insumoTemporal.value = null;
    }
  };

  const eliminarRecurso = (index: number) => {
    if (itemEnEdicion.value) {
      itemEnEdicion.value.recursos.splice(index, 1);
    }
  };

  const guardar = async () => {
    if (!formulario.value.nombreProyecto || formulario.value.items.length === 0) {
      errorGlobal.value = "El proyecto requiere nombre y al menos un ítem.";
      return;
    }

    guardando.value = true;
    errorGlobal.value = '';

    try {
      if (esEdicion.value) await presupuestoService.actualizar(formulario.value.id!, formulario.value);
      else await presupuestoService.crear(formulario.value);
      volver();
    } catch (err: any) {
      errorGlobal.value = err.message;
    } finally {
      guardando.value = false;
    }
  };

  const volver = () => router.push('/presupuestos');

  const formatearMoneda = (valor?: number) => valor === undefined ? '0' : valor.toLocaleString('es-CL');

  const obtenerNombreEtiqueta = (id: string): string => {
    const tag = catalogoEtiquetas.value.find(e => e.id === id);
    return tag ? tag.nombre : 'Etiqueta';
  };

  const obtenerEstiloEtiqueta = (id: string) => {
    const tag = catalogoEtiquetas.value.find(e => e.id === id);
    if (!tag) return { backgroundColor: '#e2e8f0', color: '#475569' };
    return {
      backgroundColor: `${tag.colorHex}20`,
      color: tag.colorHex,
      border: `1px solid ${tag.colorHex}`
    };
  };
</script>
