<template>
  <iframe ref="viewer" :src="viewerUrl" class="w-full h-full" frameborder="0"></iframe>
</template>

<script>
import highlightService from "@/services/highlight";

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
      isLoaded: false,
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
        if (!previous) previous = [];
        if (!current) current = [];

        for (const element of previous) {
          const highlightClass = "!bg-red-500";
          highlightService.unhighlight(element, highlightClass);
        }

        for (const element of current) {
          const highlightClass = "!bg-red-500";
          highlightService.highlight(element, highlightClass);
        }
      },
    },
    previewElements: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        if (!previous) previous = [];
        if (!current) current = [];

        for (const element of previous) {
          const highlightClass = "!bg-indigo-500";
          highlightService.unhighlight(element, highlightClass);
        }

        for (const element of current) {
          const highlightClass = "!bg-indigo-500";
          highlightService.highlight(element, highlightClass);
        }
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
      if (event.data.type != "viewer.loaded") return;

      if (!this.$refs.viewer) return;

      this.isLoaded = event.data.isLoaded;

      console.log(event.data);
      this.$emit("loaded", this.$refs.viewer.contentWindow.document);
    },
  },

  mounted() {
    window.addEventListener("message", this.receiveMessage);

    let iframeWindow = this.$refs.viewer.contentWindow;
    iframeWindow.addEventListener("mouseover", this.highlightHandler);
    iframeWindow.addEventListener("mouseout", this.unhighlightHandler);
    iframeWindow.addEventListener("click", this.selectHandler);
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
