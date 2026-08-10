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
              class="w-full md:w-6 lg:w-3 font-bold" />

    </div>


    <!-- =====================================================
         INDICADORES
         ===================================================== -->
    <div class="grid m-0">

      <!-- PRESUPUESTOS ACTIVOS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">

        <div class="app-card p-3 flex align-items-center gap-3 h-full">

          <div class="bg-green-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-file text-2xl"></i>
          </div>

          <div class="flex flex-column">

            <span class="app-text-muted text-sm mb-1">
              Presupuestos Activos
            </span>

            <span class="app-text text-2xl font-bold">
              {{ stats.activos }}
            </span>

          </div>

        </div>

      </div>


      <!-- COTIZACIONES ENVIADAS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">

        <div class="app-card p-3 flex align-items-center gap-3 h-full">

          <div class="bg-blue-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-chart-line text-2xl"></i>
          </div>

          <div class="flex flex-column">

            <span class="app-text-muted text-sm mb-1">
              Cotizaciones Enviadas
            </span>

            <span class="app-text text-2xl font-bold">
              {{ stats.enviadas }}
            </span>

          </div>

        </div>

      </div>


      <!-- APUS CREADAS -->
      <div class="col-12 md:col-6 lg:col-3 p-2">

        <div class="app-card p-3 flex align-items-center gap-3 h-full">

          <div class="bg-orange-500 text-white border-round-md flex align-items-center justify-content-center flex-shrink-0"
               style="width: 3.5rem; height: 3.5rem;">
            <i class="pi pi-wrench text-2xl"></i>
          </div>

          <div class="flex flex-column">

            <span class="app-text-muted text-sm mb-1">
              APUs Creadas
            </span>

            <span class="app-text text-2xl font-bold">
              {{ stats.apus }}
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

            <span class="app-text-muted text-sm mb-1">
              Pendientes
            </span>

            <span class="app-text text-2xl font-bold">
              {{ stats.pendientes }}
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

            <div class="flex flex-column gap-3">

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
                    {{ proyecto.fecha }}
                  </span>

                  <Tag :value="proyecto.estado"
                       :severity="proyecto.severity"
                       rounded />

                </div>

              </div>

            </div>

          </template>

        </Card>

      </div>


      <!-- ACCESO RÁPIDO -->
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
                      icon="pi pi-file"
                      outlined
                      class="w-full justify-content-start" />

              <Button label="ESTRUCTURAS APU"
                      icon="pi pi-wrench"
                      outlined
                      class="w-full justify-content-start" />

              <Button label="REPOSITORIO"
                      icon="pi pi-folder"
                      outlined
                      class="w-full justify-content-start" />

              <Button label="PRECIOS"
                      icon="pi pi-dollar"
                      outlined
                      class="w-full justify-content-start" />

              <Button label="CONFIGURACIÓN"
                      icon="pi pi-cog"
                      outlined
                      class="w-full justify-content-start" />

            </div>

          </template>

        </Card>

      </div>

    </div>

  </div>
</template>


<script setup lang="ts">
  import { ref, computed } from 'vue';

  import { useAuthStore } from '../../stores/authStore';

  import Button from 'primevue/button';
  import Card from 'primevue/card';
  import Tag from 'primevue/tag';


  const authStore = useAuthStore();

  const nombreUsuario = computed(() => authStore.usuarioNombre);


  const stats = ref({
    activos: 12,
    enviadas: 8,
    apus: 24,
    pendientes: 3
  });


  const proyectosRecientes = ref([
    {
      id: 1,
      titulo: 'Instalación Sistema Agua Caliente',
      cliente: 'Juan Pérez',
      fecha: '2026-05-08',
      estado: 'En proceso',
      severity: 'warn'
    },
    {
      id: 2,
      titulo: 'Reparación Cañerías Baño',
      cliente: 'María González',
      fecha: '2026-05-06',
      estado: 'Cotizado',
      severity: 'info'
    },
    {
      id: 3,
      titulo: 'Cambio Calefont',
      cliente: 'Carlos Rojas',
      fecha: '2026-05-05',
      estado: 'Finalizado',
      severity: 'success'
    }
  ]);
</script>
