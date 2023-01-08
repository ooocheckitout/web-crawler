<template>
  <iframe ref="viewer" :src="url" class="w-full h-full" frameborder="0"></iframe>
</template>

<script>
import highlightService from "@/services/highlight";
import helperService from "@/services/helper";

export default {
  props: {
    url: {
      type: String,
      default: null,
    },
    highlightElements: {
      type: Array,
      default: [],
    },
    previewElements: {
      type: Array,
      default: [],
    },
  },

  data() {
    return {
      lastSelectedElement: null,
    };
  },

  computed: {
    viewerUrl() {
      return `/#/viewer?url=${this.url}`;
    },
  },

  watch: {
    highlightElements: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        const highlightClass = "!bg-red-500";
        highlightService.unhighlight(previous, highlightClass);
        highlightService.highlight(current, highlightClass);
      },
    },

    previewElements: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        const highlightClass = "!bg-indigo-500";
        highlightService.unhighlight(previous, highlightClass);
        highlightService.highlight(current, highlightClass);
      },
    },
  },

  methods: {
    highlightHandler(event) {
      let { target, relatedTarget } = event;

      const highlightClass = "!bg-sky-500";
      highlightService.unhighlight(relatedTarget, highlightClass);
      highlightService.highlight(target, highlightClass);
    },

    unhighlightHandler(event) {
      let { target } = event;

      const highlightClass = "!bg-sky-500";
      highlightService.unhighlight(target, highlightClass);
    },

    selectHandler(event) {
      let { target } = event;

      if (this.lastSelectedElement == target) return;

      this.lastSelectedElement = target;

      this.$emit("selected", this.lastSelectedElement);
    },

    receiveMessage(event) {
      if (event.data.type != "viewer.loaded" || !this.$refs.viewer) return;

      console.log("loaded", event.data);
      this.$emit("loaded", this.$refs.viewer.contentWindow.document, event.data.viewerXpath);
    },
  },

  async mounted() {
    window.addEventListener("message", this.receiveMessage);

    let iframeWindow = this.$refs.viewer.contentWindow;
    iframeWindow.addEventListener("mouseover", this.highlightHandler);
    iframeWindow.addEventListener("mouseout", this.unhighlightHandler);
    iframeWindow.addEventListener("click", this.selectHandler);

    await helperService.waitEvent(this.$refs.viewer, "load")

    let initialLength = this.$refs.viewer.contentWindow.document.styleSheets.length
    await helperService.waitAction(() => {
      return this.$refs.viewer.contentWindow.document.styleSheets.length > initialLength
    })

    let viewerDocument = this.$refs.viewer.contentWindow.document
    let currentLastStylesheet = document.styleSheets[document.styleSheets.length - 1]
    let iframeLastStylesheet = viewerDocument.styleSheets[viewerDocument.styleSheets.length - 1]

    for (const [index, cssRule] of Array.from(currentLastStylesheet.cssRules).entries()) {
      iframeLastStylesheet.insertRule(cssRule.cssText, index)
    }

    console.log(`transfered ${currentLastStylesheet.cssRules.length} rules`);
  },

  beforeDestroy() {
    window.removeEventListener("message", this.receiveMessage);

    let iframeWindow = this.$refs.viewer.contentWindow;
    iframeWindow.removeEventListener("mouseover", this.highlightHandler);
    iframeWindow.removeEventListener("mouseout", this.unhighlightHandler);
    iframeWindow.removeEventListener("click", this.selectHandler);
  },
};
</script>
