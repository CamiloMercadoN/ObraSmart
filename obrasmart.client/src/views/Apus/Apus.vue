<template>
  <div class="flex flex-column gap-3 md:gap-4 pb-4 h-full">

    <div class="surface-card p-3 md:p-4 shadow-1 border-round flex flex-column flex-grow-1 overflow-hidden">

      <!-- Cabecera y Acciones -->
      <div class="flex flex-column md:flex-row justify-content-between md:align-items-center mb-4 gap-3">
        <div class="flex align-items-center gap-3 titulo-mantenedor">
          <div class="bg-primary text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0" style="width: 3rem; height: 3rem;">
            <i class="pi pi-book text-xl"></i>
          </div>
          <div>
            <h2 class="m-0 text-900 text-lg md:text-xl font-bold">Catálogo de APUs</h2>
            <span class="text-500 text-sm hidden md:block">Gestiona tus análisis de precios unitarios base</span>
          </div>
        </div>

        <div class="flex flex-column sm:flex-row gap-2 w-full md:w-auto flex-shrink-0">
          <Button :icon="mostrarFiltros ? 'pi pi-filter-slash' : 'pi pi-filter'"
                  :label="mostrarFiltros ? 'Ocultar Filtros' : 'Filtros'"
                  severity="secondary"
                  outlined
                  class="w-full sm:w-auto"
                  @click="mostrarFiltros = !mostrarFiltros" />
          <Button label="Nuevo APU" icon="pi pi-plus" class="w-full sm:w-auto" @click="abrirFormulario()" />
        </div>
      </div>

      <!-- Mensaje de Error -->
      <Message v-if="errorGlobal" severity="error" :closable="true" @close="errorGlobal = ''" class="mb-3">
        {{ errorGlobal }}
      </Message>

      <!-- Zona de Filtros -->
      <div v-if="mostrarFiltros" class="flex flex-column md:flex-row gap-3 mb-4 p-3 surface-100 border-round">

        <!-- Buscador con flex-1 y IconField -->
        <div class="flex-1 w-full">
          <IconField class="w-full">
            <InputIcon class="pi pi-search" />
            <InputText v-model="filtroTexto" placeholder="Buscar por nombre..." class="w-full" />
          </IconField>
        </div>

        <!-- Selector con ancho fijo en escritorio -->
        <div class="w-full md:w-15rem flex-shrink-0">
          <Select v-model="filtroEtiqueta"
                  :options="catalogoEtiquetas"
                  optionLabel="nombre"
                  optionValue="id"
                  placeholder="Filtrar por Etiqueta"
                  showClear
                  class="w-full" />
        </div>

      </div>

      <!-- Vista de Tarjetas -->
      <div class="flex-grow-1 overflow-auto">

        <div v-if="cargando" class="flex justify-content-center align-items-center p-5">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>

        <div v-else-if="apusFiltrados.length === 0" class="text-center p-5 text-500 border-round surface-100 border-1 surface-border border-dashed">
          No se encontraron estructuras APU. Modifica los filtros o crea la primera para comenzar.
        </div>

        <div v-else class="grid">
          <div v-for="apu in apusFiltrados" :key="apu.id" class="col-12 md:col-6 lg:col-4 xl:col-3">
            <!-- Tarjeta Individual -->
            <div class="surface-card border-1 surface-border border-round shadow-1 flex flex-column h-full">

              <!-- Header Tarjeta -->
              <div class="flex justify-content-between align-items-start p-3 border-bottom-1 surface-border surface-50 border-round-top">
                <span class="text-900 font-bold text-lg line-height-2 mb-1 pr-2" style="word-break: break-word;">
                  {{ apu.nombre }}
                </span>
                <div class="flex-shrink-0 mt-1">
                  <Tag v-if="apu.esPlantilla" value="Plantilla" severity="info" class="text-xs" v-tooltip.top="'Plantilla del Sistema'" />
                  <Tag v-else value="Personalizado" severity="success" class="text-xs" />
                </div>
              </div>

              <!-- Body Tarjeta -->
              <div class="p-3 flex-grow-1 flex flex-column gap-3 justify-content-between">

                <div class="flex justify-content-between align-items-center">
                  <span class="text-500 text-sm">Unidad:</span>
                  <span class="text-700 text-sm font-semibold">{{ apu.unidadMedidaNombre }}</span>
                </div>

                <!-- Etiquetas -->
                <div v-if="apu.etiquetasIds && apu.etiquetasIds.length > 0">
                  <span class="text-500 text-sm block mb-2">Etiquetas:</span>
                  <div class="flex flex-wrap gap-1">
                    <span v-for="tagId in apu.etiquetasIds"
                          :key="tagId"
                          class="text-xs px-2 py-1 border-round font-semibold"
                          :style="obtenerEstiloEtiqueta(tagId)">
                      {{ obtenerNombreEtiqueta(tagId) }}
                    </span>
                  </div>
                </div>

                <!-- Costo Total -->
                <div class="flex justify-content-between align-items-center pt-3 border-top-1 surface-border mt-auto">
                  <span class="text-700 font-bold text-sm">Costo Calculado:</span>
                  <span class="text-green-600 font-bold text-xl">$ {{ formatearMoneda(apu.costoTotalCalculado) }}</span>
                </div>

              </div>

              <!-- Footer Tarjeta (Acciones) -->
              <div class="p-3 border-top-1 surface-border flex gap-2 justify-content-end surface-50 border-round-bottom">
                <Button icon="pi pi-pencil" outlined rounded severity="info"
                        @click="abrirFormulario(apu.id)"
                        :disabled="apu.esPlantilla"
                        v-tooltip.top="apu.esPlantilla ? 'Las plantillas no se pueden editar' : 'Editar'" />

                <Button icon="pi pi-refresh" outlined rounded severity="secondary"
                        @click="recalcularCosto(apu)"
                        :disabled="apu.esPlantilla || recargandoId === apu.id"
                        :loading="recargandoId === apu.id"
                        v-tooltip.top="'Recalcular costo con precios actuales de insumos'" />

                <Button icon="pi pi-trash" outlined rounded severity="danger"
                        @click="confirmarEliminacion(apu)"
                        :disabled="apu.esPlantilla"
                        v-tooltip.top="apu.esPlantilla ? 'Las plantillas no se pueden eliminar' : 'Eliminar'" />
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { apuService } from '../../services/apuService';
  import { insumoService } from '../../services/insumoService';
  import type { IEstructuraAPU } from '../../interfaces/IApu';
  import type { IEtiqueta } from '../../interfaces/IInsumo';

  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import InputText from 'primevue/inputtext';
  import Select from 'primevue/select';
  import Tag from 'primevue/tag';
  import IconField from 'primevue/iconfield';
  import InputIcon from 'primevue/inputicon';
  import { useConfirm } from 'primevue/useconfirm';
  import ConfirmDialog from 'primevue/confirmdialog';

  const router = useRouter();
  const confirm = useConfirm();

  // Estado de la Vista
  const apus = ref<IEstructuraAPU[]>([]);
  const catalogoEtiquetas = ref<IEtiqueta[]>([]);
  const cargando = ref(false);
  const errorGlobal = ref('');
  const recargandoId = ref<string | null>(null);

  // Filtros Locales
  const filtroTexto = ref('');
  const filtroEtiqueta = ref<string | null>(null);
  const mostrarFiltros = ref(window.innerWidth > 768);

  onMounted(async () => {
    await cargarEtiquetas();
    await cargarApus();
  });

  const cargarEtiquetas = async () => {
    try {
      catalogoEtiquetas.value = await insumoService.obtenerEtiquetas();
    } catch (err) {
      console.error("No se pudieron pre-cargar las etiquetas", err);
    }
  };

  const cargarApus = async () => {
    cargando.value = true;
    errorGlobal.value = '';
    try {
      apus.value = await apuService.obtenerTodos();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      cargando.value = false;
    }
  };

  // Computada para el buscador responsivo por texto y etiquetas
  const apusFiltrados = computed(() => {
    return apus.value.filter(apu => {
      let coincideTexto = true;
      let coincideEtiqueta = true;

      if (filtroTexto.value) {
        coincideTexto = apu.nombre.toLowerCase().includes(filtroTexto.value.toLowerCase());
      }

      if (filtroEtiqueta.value) {
        coincideEtiqueta = apu.etiquetasIds?.includes(filtroEtiqueta.value) ?? false;
      }

      return coincideTexto && coincideEtiqueta;
    });
  });

  const abrirFormulario = (id?: string) => {
    if (id) {
      router.push(`/apus/editar/${id}`);
    } else {
      router.push('/apus/crear');
    }
  };

  const recalcularCosto = async (apu: IEstructuraAPU) => {
    if (!apu.id) return;

    recargandoId.value = apu.id;
    errorGlobal.value = '';
    try {
      await apuService.recalcularCosto(apu.id);
      await cargarApus();
    } catch (error: any) {
      errorGlobal.value = error.message;
    } finally {
      recargandoId.value = null;
    }
  };

  const confirmarEliminacion = (apu: IEstructuraAPU) => {
    confirm.require({
      message: `¿Estás seguro de eliminar el APU "${apu.nombre}"? Esta acción no se puede deshacer.`,
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: {
        label: 'Cancelar',
        severity: 'secondary',
        outlined: true
      },
      acceptProps: {
        label: 'Eliminar',
        severity: 'danger'
      },
      accept: async () => {
        try {
          cargando.value = true;
          await apuService.eliminar(apu.id!);
          await cargarApus();
        } catch (err: any) {
          errorGlobal.value = err.message;
          cargando.value = false;
        }
      }
    });
  };

  // Funciones de utilidad visual
  const formatearMoneda = (valor: number) => {
    return valor.toLocaleString('es-CL');
  };

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

<style scoped>
  @media (max-height: 500px) {
    .titulo-mantenedor {
      display: none !important;
    }
  }
</style>
