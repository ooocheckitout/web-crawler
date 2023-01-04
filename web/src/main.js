import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router';

require('@/services/extensions.js')
require('@/assets/style.css')


import App from '@/App.vue'
import Main from '@/components/legacy/Main.vue'
import Home from '@/pages/Home.vue'
import Viewer from '@/pages/Viewer.vue'

const routes = [
  { path: '/main', component: Main },
  { path: '/', component: Home },
  { path: '/viewer', component: Viewer },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

createApp(App).use(router).mount('#app')
