<template>
  <div class="flex justify-content-center align-items-center min-h-screen bg-surface-100 py-4">
    <Card class="w-full shadow-2 mx-3" style="max-width: 400px; padding: 1rem;">
      <template #title>
        <div class="text-center">
          <h2 class="mt-0 mb-1">ObraSmart</h2>
          <p class="text-color-secondary text-base mt-0 mb-3">Iniciar Sesión</p>
        </div>
      </template>
      <template #content>
        <form @submit.prevent="handleLogin">
          <div class="flex flex-column gap-2 mb-4">
            <label for="correo">Correo Electrónico</label>
            <InputText id="correo"
                       v-model="correo"
                       type="email"
                       required
                       autofocus
                       class="w-full"
                       autocomplete="username" />
          </div>

          <div class="flex flex-column gap-2 mb-4">
            <label for="password">Contraseña</label>
            <!-- Nota: Usamos inputClass="w-full" para estirar el input interno del Password -->
            <Password id="password"
                      v-model="password"
                      :feedback="false"
                      toggleMask
                      required
                      class="w-full"
                      inputClass="w-full"
                      autocomplete="current-password" />
          </div>

          <Message v-if="error" severity="error" :closable="false" class="mb-3">
            {{ error }}
          </Message>

          <Button type="submit"
                  label="Ingresar"
                  :loading="loading"
                  class="w-full mt-2" />

          <div class="text-center mt-4">
            <router-link to="/registro" class="text-primary no-underline hover:underline text-sm transition-colors transition-duration-200">
              ¿No tienes cuenta? Regístrate aquí
            </router-link>
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';
  import { useRouter } from 'vue-router';
  import { authService } from '../../services/authService';

  import Card from 'primevue/card';
  import InputText from 'primevue/inputtext';
  import Password from 'primevue/password';
  import Button from 'primevue/button';
  import Message from 'primevue/message';

  const router = useRouter();
  const correo = ref('');
  const password = ref('');
  const error = ref('');
  const loading = ref(false);

  const handleLogin = async () => {
    error.value = '';
    loading.value = true;

    try {
      await authService.login(correo.value, password.value);
      router.push('/');
    } catch (err: any) {
      error.value = err.message || 'Ocurrió un error al intentar iniciar sesión.';
    } finally {
      loading.value = false;
    }
  };
</script>
