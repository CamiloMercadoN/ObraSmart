<template>
  <div class="flex flex-column gap-4 pb-4">

    <!-- Header -->
    <div class="flex align-items-center justify-content-between">
      <div class="flex align-items-center gap-3">
        <Button icon="pi pi-arrow-left" text rounded @click="volver" />
        <h2 class="m-0 text-900 text-xl font-bold">
          {{ esEdicion ? 'Editar Presupuesto' : 'Nuevo Presupuesto' }}
        </h2>
      </div>
      <div class="flex gap-2">
        <Button label="Cancelar" severity="secondary" outlined @click="volver" />
        <Button label="Guardar" icon="pi pi-save" @click="guardar" :loading="guardando" />
      </div>
    </div>

    <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="m-0">
      {{ errorGlobal }}
    </Message>

    <div class="grid formgrid p-fluid">
      <!-- Datos Generales (Maestro) -->
      <div class="col-12">
        <div class="surface-card p-4 shadow-1 border-round">
          <h3 class="mt-0 mb-4 text-900 border-bottom-1 border-300 pb-2">Datos Generales</h3>
          <div class="grid formgrid">
            <div class="field col-12 md:col-6">
              <label for="proyecto" class="font-bold">Nombre del Proyecto <span class="text-red-500">*</span></label>
              <InputText id="proyecto" v-model="formulario.nombreProyecto" placeholder="Ej: Remodelación Oficina Central" />
            </div>
            <div class="field col-12 md:col-6">
              <label for="cliente" class="font-bold">Cliente</label>
              <Select id="cliente" v-model="formulario.clienteId" :options="clientes"
                      optionLabel="nombre" optionValue="id" placeholder="Seleccione un cliente" showClear />
            </div>
          </div>
        </div>
      </div>

      <!-- Detalle (Ítems APU) -->
      <div class="col-12">
        <div class="surface-card p-4 shadow-1 border-round">
          <div class="flex justify-content-between align-items-center mb-4 border-bottom-1 border-300 pb-2">
            <h3 class="m-0 text-900">Detalle del Presupuesto</h3>
            <div class="flex gap-2">
              <Button label="Nuevo Ítem Ad-Hoc" icon="pi pi-plus" size="small" severity="secondary" outlined @click="agregarItemManual" />
              <Button label="Agregar del Catálogo APU" icon="pi pi-search" size="small" @click="mostrarModalCatalog = true" />
            </div>
          </div>

          <DataTable :value="formulario.items" responsiveLayout="scroll" class="p-datatable-sm" stripedRows>
            <template #empty>
              <div class="text-center p-4 text-500 border-round surface-100">
                Aún no hay ítems en este presupuesto. Agrega una estructura APU desde el catálogo o crea una nueva.
              </div>
            </template>

            <Column header="Descripción del Ítem">
              <template #body="slotProps">
                <InputText v-model="slotProps.data.descripcion" class="w-full" />
              </template>
            </Column>

            <Column header="Unidad">
              <template #body="slotProps">
                <Select v-model="slotProps.data.unidadMedidaId" :options="unidadesCatalogo"
                        optionLabel="nombre" optionValue="id" class="w-full md:w-8rem" />
              </template>
            </Column>

            <Column header="Cantidad">
              <template #body="slotProps">
                <!-- Reactividad delegada al watcher profundo -->
                <InputNumber v-model="slotProps.data.cantidadItem" :minFractionDigits="0" :maxFractionDigits="2"
                             class="w-8rem" />
              </template>
            </Column>

            <Column header="P. Unitario">
              <template #body="slotProps">
                $ {{ formatearMoneda(slotProps.data.precioUnitarioCalculado) }}
              </template>
            </Column>

            <Column header="Subtotal">
              <template #body="slotProps">
                <span class="font-bold text-primary">$ {{ formatearMoneda(slotProps.data.cantidadItem * (slotProps.data.precioUnitarioCalculado || 0)) }}</span>
              </template>
            </Column>

            <Column header="Acciones" style="width: 8rem">
              <template #body="slotProps">
                <div class="flex gap-2">
                  <Button icon="pi pi-list" severity="info" text rounded @click="abrirDetalleRecursos(slotProps.index)" v-tooltip.top="'Gestionar Insumos'" />
                  <Button icon="pi pi-trash" severity="danger" text rounded @click="eliminarItem(slotProps.index)" />
                </div>
              </template>
            </Column>
          </DataTable>

          <div class="flex justify-content-end mt-4">
            <div class="w-full md:w-20rem surface-100 p-3 border-round">
              <div class="flex justify-content-between mb-2">
                <span class="text-700">Subtotal Neto:</span>
                <span class="font-bold text-xl text-green-600">$ {{ formatearMoneda(totalCalculadoFrontend) }}</span>
              </div>
              <div class="text-xs text-500 text-right">
                * El cálculo final (incluyendo IVA) se procesará al guardar.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Catálogo APU -->
    <Dialog v-model:visible="mostrarModalCatalog" modal header="Seleccionar Estructura APU" :style="{ width: '65vw' }" :breakpoints="{ '960px': '85vw', '641px': '100vw' }">
      <div class="flex flex-column md:flex-row gap-3 mb-3">
        <InputText v-model="filtrosApu['global'].value" placeholder="Buscar APU por nombre..." class="w-full" />
        <Select v-model="filtrosApu['etiquetasIds'].value" :options="catalogoEtiquetas" optionLabel="nombre" optionValue="id" placeholder="Filtrar por Etiqueta" showClear class="w-full md:w-15rem" />
      </div>

      <DataTable :value="apusCatalogo" :loading="cargandoApus" v-model:filters="filtrosApu" :globalFilterFields="['nombre']" paginator :rows="5" class="p-datatable-sm" selectionMode="single" @rowSelect="seleccionarApu">
        <Column field="nombre" header="Nombre" class="font-bold"></Column>
        <Column field="unidadMedidaNombre" header="Unidad"></Column>
        <Column header="Costo Unitario">
          <template #body="slotProps">
            <span class="font-semibold text-green-600">$ {{ formatearMoneda(slotProps.data.costoTotalCalculado) }}</span>
          </template>
        </Column>
        <Column header="Etiquetas" style="min-width: 12rem">
          <template #body="slotProps">
            <div class="flex flex-wrap gap-1">
              <span v-for="tagId in slotProps.data.etiquetasIds" :key="tagId" class="text-xs px-2 py-1 border-round font-semibold" :style="obtenerEstiloEtiqueta(tagId)">
                {{ obtenerNombreEtiqueta(tagId) }}
              </span>
            </div>
          </template>
        </Column>
      </DataTable>
    </Dialog>

    <!-- Modal para Gestionar los Insumos de un Ítem Ad-Hoc -->
    <Dialog v-model:visible="mostrarModalRecursos" modal :header="'Recursos del Ítem'" :style="{ width: '70vw' }" :breakpoints="{ '960px': '85vw', '641px': '100vw' }">
      <div class="flex gap-2 mb-3">
        <Select v-model="insumoTemporal" :options="insumosCatalogo" optionLabel="descripcion" placeholder="Selecciona un insumo para agregar..." filter class="w-full" />
        <Button icon="pi pi-plus" @click="agregarRecursoAlItem" :disabled="!insumoTemporal" />
      </div>

      <DataTable v-if="itemEnEdicion" :value="itemEnEdicion.recursos" class="p-datatable-sm" stripedRows>
        <template #empty>
          <div class="text-center p-3 text-500">Este ítem no tiene insumos. Agrega uno para calcular su precio.</div>
        </template>

        <Column header="Insumo">
          <template #body="slotProps">
            <InputText v-model="slotProps.data.descripcionCongelada" class="w-full" />
          </template>
        </Column>

        <Column header="Unidad">
          <template #body="slotProps">
            <Select v-model="slotProps.data.unidadMedidaId" :options="unidadesCatalogo"
                    optionLabel="nombre" optionValue="id" class="w-full md:w-8rem" />
          </template>
        </Column>

        <Column header="Cantidad">
          <template #body="slotProps">
            <!-- Reactividad delegada al watcher profundo -->
            <InputNumber v-model="slotProps.data.cantidad" :minFractionDigits="0" :maxFractionDigits="4" class="w-6rem" />
          </template>
        </Column>
        <Column header="Precio Referencia">
          <template #body="slotProps">
            <!-- Reactividad delegada al watcher profundo -->
            <InputNumber v-model="slotProps.data.precioUnitarioCongelado" mode="currency" currency="CLP" locale="es-CL" class="w-8rem" />
          </template>
        </Column>
        <Column header="Costo Total">
          <template #body="slotProps">
            <span class="font-bold">$ {{ formatearMoneda(slotProps.data.cantidad * slotProps.data.precioUnitarioCongelado) }}</span>
          </template>
        </Column>
        <Column header="" style="width: 4rem">
          <template #body="slotProps">
            <Button icon="pi pi-times" severity="danger" text rounded @click="eliminarRecurso(slotProps.index)" />
          </template>
        </Column>
      </DataTable>
      <template #footer>
        <Button label="Cerrar" icon="pi pi-check" @click="mostrarModalRecursos = false" />
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { presupuestoService } from '../../services/presupuestoService';
  import { clienteService } from '../../services/clienteService';
  import { apuService } from '../../services/apuService';
  import { insumoService } from '../../services/insumoService';
  import type { IPresupuesto, IItemPresupuesto, IRecursoItemPresupuesto } from '../../interfaces/IPresupuesto';
  import type { ICliente } from '../../interfaces/ICliente';
  import type { IEstructuraAPU } from '../../interfaces/IApu';
  import type { IEtiqueta, IInsumo, IUnidadMedida } from '../../interfaces/IInsumo';
  import { FilterMatchMode } from "@primevue/core/api";

  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import InputNumber from 'primevue/inputnumber';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
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
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    etiquetasIds: { value: null, matchMode: FilterMatchMode.CONTAINS }
  });

  const mostrarModalRecursos = ref(false);
  const itemEnEdicionIndex = ref<number | null>(null);
  const itemEnEdicion = computed(() => itemEnEdicionIndex.value !== null ? formulario.value.items[itemEnEdicionIndex.value] : null);
  const insumoTemporal = ref<IInsumo | null>(null);
  const totalCalculadoFrontend = ref(0);

  const formulario = ref<IPresupuesto>({
    nombreProyecto: '',
    clienteId: null,
    items: []
  });

  // Motor de cálculo reactivo
  // Vigila cualquier cambio profundo (deep) en el arreglo de ítems o sus recursos
  watch(() => formulario.value.items, (nuevosItems) => {
    let totalMonto = 0;

    if (nuevosItems && nuevosItems.length > 0) {
      nuevosItems.forEach(item => {
        // 1. Recalcular el costo unitario del ítem sumando sus recursos
        let costoUnitarioItem = 0;

        if (item.recursos && item.recursos.length > 0) {
          item.recursos.forEach(r => {
            // El costo es Cantidad del insumo * Precio negociado/congelado
            costoUnitarioItem += (r.cantidad * r.precioUnitarioCongelado);
          });
        }

        // Actualizamos el costo unitario del ítem en tiempo real
        item.precioUnitarioCalculado = costoUnitarioItem;

        // 2. Sumar al Subtotal General del presupuesto (Cantidad del ítem * Costo Unitario)
        totalMonto += (item.cantidadItem * item.precioUnitarioCalculado);
      });
    }

    // Actualizamos la variable visual del total
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

      // Saneamiento estricto de IDs para clonación
      if (esClonacion) {
        delete data.id;
        data.nombreProyecto = `${data.nombreProyecto} (Copia)`;

        if (data.items && data.items.length > 0) {
          data.items.forEach(i => {
            delete i.id;

            if (i.recursos && i.recursos.length > 0) {
              i.recursos.forEach(r => {
                delete r.id;
              });
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

  const agregarItemManual = () => {
    formulario.value.items.push({
      descripcion: 'Nuevo Ítem Personalizado',
      cantidadItem: 1,
      unidadMedidaId: 1,
      precioUnitarioCalculado: 0,
      recursos: []
    });
  };

  const seleccionarApu = async (event: any) => {
    const apuSeleccionado = event.data as IEstructuraAPU;
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

  // Utilidades visuales
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
