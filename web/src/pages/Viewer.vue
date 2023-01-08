<template>
  <div ref="viewer" v-html="html"></div>
</template>

<script>
import xpathService from "@/services/xpath";
import helperService from "@/services/helper";

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
    window.parent.postMessage({
      type: "viewer.loaded",
      isLoaded: true,
      viewerXpath: xpathService.getElementXPath(this.$refs.viewer),
    });
  },

  async updated() {
    await helperService.waitStylesheetsAsync(document)
    helperService.removeHideElements(document)
  },
};
</script>

<style scoped>
div :deep() a {
  pointer-events: none;
}
</style>
