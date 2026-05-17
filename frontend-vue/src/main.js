import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { useAuthStore } from './store/auth'
import './assets/base.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// Initialize auth state
const authStore = useAuthStore()
authStore.initialize()

app.mount('#app')
