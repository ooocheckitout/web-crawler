<template>
  <div v-html="html"></div>
</template>
  
<script>
export default {
  data() {
    return {
      url: null,
      html: null,
    };
  },
  async created() {
    this.url = this.$route.query.url;
    let response = await fetch(this.url);
    this.html = await response.text();

    // notify parent page that viewer finished loading
    window.parent.postMessage({ type: "viewer.loaded", isLoaded: true });
  },
};
</script>

<style scoped>
div :deep() * {
  display: block !important;
}
div :deep() a {
  pointer-events: none;
}
</style>