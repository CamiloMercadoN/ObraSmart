import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';

import PrimeVue from 'primevue/config';
import ConfirmationService from 'primevue/confirmationservice';
import Aura from '@primeuix/themes/aura';
import Tooltip from 'primevue/tooltip';
import ToastService from 'primevue/toastservice';

import 'primeicons/primeicons.css';
import './assets/main.css';

const app = createApp(App);
const pinia = createPinia();

app.directive('tooltip', Tooltip);

app.use(pinia);
app.use(router);
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.app-dark',
      cssLayer: {
        name: 'primevue',
        order: 'primevue, primeflex, utilidades'
      }
    }
  }
});
app.use(ConfirmationService);
app.use(ToastService);

app.mount('#app');

(window as any).pwaPrompt = null;
window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault(); // Oculta el banner nativo del móvil
  (window as any).pwaPrompt = e; // Lo guardamos globalmente
  window.dispatchEvent(new Event('pwa-ready')); // Avisamos a Vue que está listo
});
