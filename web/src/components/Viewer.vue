<template lang="">
    <div v-html="html" 
    @mousemove="highlightHandler"
    @click="selectHandler" ></div>
</template>
<script>
export default {
  props: {
    url: {
      type: String,
      default: "https://tailwindcss.com",
    },
  },
  watch: {
    url: {
      async handler() {
        let response = await fetch(this.url);
        this.html = await response.text();
      },
      immediate: true,
    },
  },
  data() {
    return {
      html: null,
      lastHighlightedElement: null,
      lastSelectedElement: null,
    };
  },
  methods: {
    selectHandler(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      if (currentElement == this.lastSelectedElement) return;

      const highlightClass = "bg-red-500";
      this.highlight(currentElement, highlightClass);

      this.lastSelectedElement = currentElement;
      this.$emit("selected", this.lastSelectedElement);
    },

    highlightHandler(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      if (currentElement == this.lastHighlightedElement) return;

      const highlightClass = "bg-sky-500";
      this.unhighlight(this.lastHighlightedElement, highlightClass);

      this.lastHighlightedElement = currentElement;
      this.highlight(this.lastHighlightedElement, highlightClass);
    },

    highlight(elements, highlightClass) {
      if (!elements) return;

      if (!Array.isArray(elements)) elements = [elements];

      for (const element of elements) {
        if (element.classList?.contains(highlightClass)) continue;

        element.classList.add(highlightClass);
      }
    },

    unhighlight(elements, highlightClass) {
      if (!elements) return;

      if (!Array.isArray(elements)) elements = [elements];

      for (const element of elements) {
        if (!element.classList?.contains(highlightClass)) continue;

        element.classList.remove(highlightClass);
      }
    },
  },
};
</script>