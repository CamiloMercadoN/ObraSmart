<template>
  <div class="flex flex-column gap-4 pb-4">

    <!-- =====================================================
         ENCABEZADO
         ===================================================== -->
    <div class="flex align-items-center gap-3">
      <div class="bg-blue-500 text-white border-round-lg flex align-items-center justify-content-center flex-shrink-0"
           style="width: 3.5rem; height: 3.5rem;">
        <i class="pi pi-th-large text-2xl"></i>
      </div>
      <div class="flex flex-column">
        <h1 class="m-0 app-text text-2xl font-bold">
          Dashboard
        </h1>
        <span class="app-text-muted text-sm">
          Bienvenido de vuelta, {{ nombreUsuario }}
        </span>
      </div>
    </div>


    <!-- =====================================================
         NUEVO PRESUPUESTO
         ===================================================== -->
    <div class="flex justify-content-start">
      <Button label="NUEVO PRESUPUESTO"
              icon="pi pi-plus"
              size="large"
              class="w-full md:w-6 lg:w-3 font-bold"
              @click="irANuevoPresupuesto" />
    </div>


    <!-- =====================================================
         INDICADORES (Con skeleton/spinner básico si carga)
         ===================================================== -->
    <div class="grid m-0">

      <!-- PRESUPUESTOS ACTIVOS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">
        <div class="app-card p-3 flex align-items-center gap-3 h-full">
          <div class="bg-green-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-file-edit text-2xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="app-text-muted text-sm mb-1">Presupuestos Activos</span>
            <span class="app-text text-2xl font-bold">
              <i v-if="cargando" class="pi pi-spin pi-spinner text-lg text-primary"></i>
              <template v-else>
                {{ stats.activos }}
              </template>
            </span>
          </div>
        </div>
      </div>

      <!-- APUS CREADAS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">
        <div class="app-card p-3 flex align-items-center gap-3 h-full">
          <div class="bg-orange-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-book text-2xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="app-text-muted text-sm mb-1">APUs Creadas</span>
            <span class="app-text text-2xl font-bold">
              <i v-if="cargando" class="pi pi-spin pi-spinner text-lg text-primary"></i>
              <template v-else>
                {{ stats.apus }}
              </template>
            </span>
          </div>
        </div>
      </div>

      <!-- COTIZACIONES ENVIADAS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">
        <div class="app-card p-3 flex align-items-center gap-3 h-full">
          <div class="bg-blue-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-send text-2xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="app-text-muted text-sm mb-1">Cotizaciones Enviadas</span>
            <span class="app-text text-2xl font-bold">
              <i v-if="cargando" class="pi pi-spin pi-spinner text-lg text-primary"></i>
              <template v-else>
                {{ stats.enviadas }}
              </template>
            </span>
          </div>
        </div>
      </div>

      <!-- PENDIENTES -->
      <div class="col-12 md:col-6 lg:col-3 p-2">
        <div class="app-card p-3 flex align-items-center gap-3 h-full">
          <div class="bg-yellow-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-clock text-2xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="app-text-muted text-sm mb-1">Cotizaciones Pendientes (Borrador)</span>
            <span class="app-text text-2xl font-bold">
              <i v-if="cargando" class="pi pi-spin pi-spinner text-lg text-primary"></i>
              <template v-else>
                {{ stats.pendientes }}
              </template>
            </span>
          </div>
        </div>
      </div>

    </div>


    <!-- =====================================================
         PANELES INFERIORES
         ===================================================== -->
    <div class="grid m-0">

      <!-- PROYECTOS RECIENTES -->
      <div class="col-12 lg:col-6 p-2">
        <Card class="app-panel h-full">
          <template #title>
            <div class="app-text text-xl font-bold">
              Proyectos Recientes
            </div>
          </template>

          <template #content>
            <div v-if="cargando" class="flex justify-content-center p-4">
              <i class="pi pi-spin pi-spinner text-3xl text-primary"></i>
            </div>

            <div v-else-if="proyectosRecientes.length === 0" class="text-center p-4 app-text-muted border-dashed border-1 app-border-color border-round">
              Aún no has creado ningún proyecto.
            </div>

            <div v-else class="flex flex-column gap-3">
              <div v-for="proyecto in proyectosRecientes"
                   :key="proyecto.id"
                   class="app-subcard p-3 flex justify-content-between align-items-center gap-3">

                <div class="flex flex-column gap-1">
                  <span class="app-text font-bold">
                    {{ proyecto.titulo }}
                  </span>
                  <span class="app-text-muted text-sm">
                    Cliente: {{ proyecto.cliente }}
                  </span>
                </div>

                <div class="flex flex-column align-items-end gap-2 flex-shrink-0">
                  <span class="app-text-muted text-sm">
                    {{ formatearFecha(proyecto.fecha) }}
                  </span>
                  <Tag :value="proyecto.estado"
                       :severity="obtenerSeveridadEstado(proyecto.estado)"
                       rounded />
                </div>
              </div>
            </div>
          </template>
        </Card>
      </div>


      <!-- ACCESO RÁPIDO (Enlazado con MainLayout) -->
      <div class="col-12 lg:col-6 p-2">
        <Card class="app-panel h-full">
          <template #title>
            <div class="app-text text-xl font-bold">
              Acceso Rápido
            </div>
          </template>

          <template #content>
            <div class="flex flex-column gap-3">
              <Button label="PRESUPUESTOS"
                      icon="pi pi-file-edit"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/presupuestos')" />

              <Button label="COTIZACIONES"
                      icon="pi pi-send"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/cotizaciones')" />

              <Button label="CLIENTES"
                      icon="pi pi-users"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/clientes')" />

              <Button label="INSUMOS Y PRECIOS"
                      icon="pi pi-box"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/insumos')" />

              <Button label="CATÁLOGO APUS"
                      icon="pi pi-book"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/apus')" />

              <Button label="CONFIGURACIÓN"
                      icon="pi pi-cog"
                      outlined
                      class="w-full justify-content-start"
                      @click="irA('/configuracion')" />
            </div>
          </template>
        </Card>
      </div>

    </div>

  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useAuthStore } from '../../stores/authStore';
  import { dashboardService } from '../../services/dashboardService';

  import Button from 'primevue/button';
  import Card from 'primevue/card';
  import Tag from 'primevue/tag';

  const authStore = useAuthStore();
  const router = useRouter();

  const nombreUsuario = computed(() => authStore.usuarioNombre);
  const cargando = ref(true);

  const stats = ref({
    activos: 0,
    enviadas: 0,
    apus: 0,
    pendientes: 0
  });

  const proyectosRecientes = ref<any[]>([]);

  onMounted(async () => {
    try {
      const data = await dashboardService.obtenerResumen();
      if (data) {
        stats.value = {
          activos: data.presupuestosActivos,
          enviadas: data.cotizacionesEnviadas,
          apus: data.apusCreadas,
          pendientes: data.pendientes
        };
        proyectosRecientes.value = data.proyectosRecientes || [];
      }
    } catch (error) {
      console.error("Error al cargar el dashboard", error);
    } finally {
      cargando.value = false;
    }
  });

  // Formateador de fechas configurado para Chile
  const formatearFecha = (fechaStr: string) => {
    if (!fechaStr) return '';
    return new Date(fechaStr).toLocaleDateString('es-CL', { timeZone: 'America/Santiago' });
  };

  // Mapeo dinámico de severidades de PrimeVue según el estado
  const obtenerSeveridadEstado = (estado: string) => {
    switch (estado) {
      case 'Borrador': return 'secondary';
      case 'Emitido': return 'info';
      case 'Emitida': return 'info';
      case 'Aceptada': return 'success';
      case 'Aprobado': return 'success';
      case 'Rechazado': return 'danger';
      case 'Rechazada': return 'danger';
      case 'Vencida': return 'warn';
      default: return 'secondary';
    }
  };

  // Navegación
  const irANuevoPresupuesto = () => {
    router.push('/presupuestos/crear');
  };

  const irA = (ruta: string) => {
    router.push(ruta);
  };
</script>
