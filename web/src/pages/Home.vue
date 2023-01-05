<template>
  <div class="flex flex-col p-4 space-2">
    <input type="text" v-model="url" class="p-2 border-2 rounded">
    <router-link to="/data-editor" class="text-lg font-bold">Data Editor</router-link>
    <router-link :to="schemaEditorUrl" class="text-lg font-bold">Schema Editor</router-link>
    <router-link :to="viewerUrl" class="text-lg font-bold">Viewer</router-link>
  </div>
</template>

<script>
export default {
  data() {
    return {
      url: "https://tailwindcss.com/docs/customizing-colors",
    };
  },
  computed: {
    viewerUrl() {
      return `/viewer?url=${this.url}`;
    },
    schemaEditorUrl() {
      return `/schema-editor?viewerUrl=${this.url}`;
    },
  },
};
</script>
