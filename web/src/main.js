import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router';

require('@/services/extensions.js')
require('@/assets/style.css')


import App from '@/App.vue'
import Home from '@/pages/Home.vue'
import Viewer from '@/pages/Viewer.vue'
import SchemaEditor from '@/pages/SchemaEditor.vue'
import DataEditor from '@/pages/DataEditor.vue'

const routes = [
  { path: '/', component: Home },
  { path: '/viewer', component: Viewer },
  { path: '/schema-editor', component: SchemaEditor },
  { path: '/data-editor', component: DataEditor },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

createApp(App).use(router).mount('#app')
