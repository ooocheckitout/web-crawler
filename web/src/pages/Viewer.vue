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

  updated() {
    this.$nextTick(() => {
      let externalStylesheets = Array.from(document.styleSheets).filter(x => x.href);
      let rulesThatHideElement = externalStylesheets
        .flatMap(x => Array.from(x.cssRules))
        .filter(x => x.styleMap?.get("display") == "none");

      rulesThatHideElement.forEach(x => {
        let ruleIndex = Array.from(x.parentStyleSheet.cssRules).indexOf(x);
        x.parentStyleSheet.deleteRule(ruleIndex);
      });

      console.log(`removed ${rulesThatHideElement.length} css rules that hide elements`);
    });
  },
};
</script>

<style scoped>
div :deep() a {
  pointer-events: none;
}
</style>
