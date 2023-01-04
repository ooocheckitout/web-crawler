<template>
  <div class="flex flex-col space-2 p-4">
    <input type="text" v-model="url" class="border-2 rounded p-2">
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
