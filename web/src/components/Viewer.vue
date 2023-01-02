<template lang="">
    <div v-html="html" 
    @mousemove="highlightHandler"
    @click="selectHandler" ></div>
</template>
<script>
import xpathService from "../services/xpath";
import highlightService from "../services/highlight";

export default {
  props: {
    html: {
      type: String,
      default: "",
    },
    schema: {
      type: Array,
      default: [],
    },
  },
  data() {
    return {
      lastHighlightedElement: null,
      lastSelectedElement: null,
    };
  },
  watch: {
    schema: {
      async handler(schema, oldSchema) {
        // ensure html is completely loaded to the dom
        this.$nextTick(() => {
          if (oldSchema) {
            for (const property of oldSchema) {
              let elements = xpathService.evaluateXPath(
                document,
                property.suggestedXpath ?? property.xpath
              );
              console.log("unhighlight", elements);
              highlightService.unhighlight(elements, "bg-red-500");
            }
          }

          for (const property of schema) {
            let elements = xpathService.evaluateXPath(
              document,
              property.suggestedXpath ?? property.xpath
            );
            highlightService.highlight(elements, "bg-red-500");
          }
        });
      },
      immediate: true,
      deep: true,
    },
  },
  methods: {
    selectHandler(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      if (currentElement == this.lastSelectedElement) return;

      const highlightClass = "bg-red-500";
      highlightService.highlight(currentElement, highlightClass);

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
      highlightService.unhighlight(this.lastHighlightedElement, highlightClass);

      this.lastHighlightedElement = currentElement;
      highlightService.highlight(this.lastHighlightedElement, highlightClass);
    },
  },
};
</script>