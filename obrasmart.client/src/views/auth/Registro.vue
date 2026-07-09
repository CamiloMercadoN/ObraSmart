<template>
  <div class="flex justify-content-center align-items-center min-h-screen bg-surface-100 py-4">
    <Card class="w-full shadow-2 mx-3" style="max-width: 450px; padding: 1rem;">
      <template #title>
        <div class="text-center">
          <h2 class="mt-0 mb-1">ObraSmart</h2>
          <p class="text-color-secondary text-base mt-0 mb-3">Crear Nueva Cuenta</p>
        </div>
      </template>
      <template #content>
        <form @submit.prevent="handleRegistro">

          <div class="flex flex-column gap-2 mb-4">
            <label for="rut">RUT</label>
            <InputText id="rut"
                       v-model="formulario.rut"
                       @input="handleInputRut"
                       placeholder="12345678-9"
                       :invalid="!esRutValido && formulario.rut.length > 7"
                       class="w-full"
                       required />
            <small v-if="!esRutValido && formulario.rut.length > 7" class="text-red-500 text-sm">El RUT ingresado no es válido.</small>
          </div>

          <div class="flex flex-column gap-2 mb-4">
            <label for="razonSocial">Razón Social</label>
            <InputText id="razonSocial"
                       v-model="formulario.razonSocial"
                       class="w-full"
                       required />
          </div>

          <div class="flex flex-column gap-2 mb-4">
            <label for="correo">Correo Electrónico</label>
            <InputText id="correo"
                       v-model="formulario.correo"
                       type="email"
                       :invalid="!esCorreoValido"
                       placeholder="ejemplo@dominio.com"
                       class="w-full"
                       required />
            <small v-if="!esCorreoValido && formulario.correo.length > 0" class="text-red-500 text-sm">El formato del correo es inválido.</small>
          </div>

          <div class="flex flex-column gap-2 mb-4">
            <label for="password">Contraseña</label>
            <Password id="password"
                      v-model="formulario.password"
                      toggleMask
                      required
                      class="w-full"
                      inputClass="w-full"
                      promptLabel="Ingresa una contraseña"
                      weakLabel="Débil"
                      mediumLabel="Media"
                      strongLabel="Fuerte" />
          </div>

          <div class="flex flex-column gap-2 mb-4">
            <label for="confirmPassword">Confirmar Contraseña</label>
            <Password id="confirmPassword"
                      v-model="formulario.confirmPassword"
                      :feedback="false"
                      toggleMask
                      :invalid="formulario.password !== formulario.confirmPassword && formulario.confirmPassword.length > 0"
                      class="w-full"
                      inputClass="w-full"
                      required />
            <small v-if="formulario.password !== formulario.confirmPassword && formulario.confirmPassword.length > 0" class="text-red-500 text-sm">Las contraseñas no coinciden.</small>
          </div>

          <Message v-if="error" severity="error" :closable="false" class="mb-3">
            {{ error }}
          </Message>

          <Message v-if="success" severity="success" :closable="false" class="mb-3">
            Registro exitoso. Redirigiendo al login...
          </Message>

          <Button type="submit"
                  label="Registrarse"
                  :loading="loading"
                  :disabled="!formularioValido"
                  class="w-full mt-2" />

          <div class="text-center mt-4">
            <router-link to="/login" class="text-primary no-underline hover:underline text-sm transition-colors transition-duration-200">
              ¿Ya tienes cuenta? Inicia sesión aquí
            </router-link>
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { authService } from '../../services/authService';
  import { validarRutChileno, formatearRut } from '../../utils/rutHelper';
  import { validarCorreo } from '../../utils/emailHelper';
  import type { IRegistroUsuario } from '../../interfaces/IRegistroUsuario'; // <-- Importación de la interfaz

  import Card from 'primevue/card';
  import InputText from 'primevue/inputtext';
  import Password from 'primevue/password';
  import Button from 'primevue/button';
  import Message from 'primevue/message';

  const router = useRouter();

  const formulario = ref<IRegistroUsuario>({
    rut: '',
    razonSocial: '',
    correo: '',
    password: '',
    confirmPassword: ''
  });

  const error = ref('');
  const success = ref(false);
  const loading = ref(false);

  const handleInputRut = () => {
    formulario.value.rut = formatearRut(formulario.value.rut);
  };

  const esRutValido = computed(() => validarRutChileno(formulario.value.rut));

  const esCorreoValido = computed(() => {
    if (formulario.value.correo.length === 0) return true;
    return validarCorreo(formulario.value.correo);
  });

  const formularioValido = computed(() => {
    return esRutValido.value &&
      formulario.value.correo.length > 0 &&
      validarCorreo(formulario.value.correo) &&
      formulario.value.password.length >= 6 &&
      formulario.value.password === formulario.value.confirmPassword &&
      formulario.value.razonSocial.length > 0;
  });

  const handleRegistro = async () => {
    if (!formularioValido.value) return;

    error.value = '';
    loading.value = true;

    try {
      await authService.registro({
        rut: formulario.value.rut.replace('-', ''),
        correo: formulario.value.correo,
        password: formulario.value.password,
        razonSocial: formulario.value.razonSocial
      });

      success.value = true;
      setTimeout(() => {
        router.push('/login');
      }, 2000);
    } catch (err: any) {
      error.value = err.message || 'Ocurrió un error al intentar registrarse.';
    } finally {
      loading.value = false;
    }
  };
</script>
