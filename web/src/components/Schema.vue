<template>
  <div>
    <p>Schema</p>
    <pre>{{ schema }}</pre>
    <p>Suggestions</p>
    <pre>{{ suggestions.map((x) => x.xpath) }}</pre>
    <div v-for="(item, index) in suggestions" :key="index">
      <input
        type="button"
        :value="index"
        @mouseenter="highlightSuggestionHandler(item)"
        @mouseleave="unhighlightSuggestionHandler(item)"
        @click="applySuggestionHandler(item)"
        class="w-full h-full p-2 border-2 border-indigo-600 border-solid rounded "
      />
    </div>
  </div>
</template>

<script>
import xpathService from "../services/xpath";
import highlightService from "../services/highlight";

export default {
  props: {
    schema: {
      type: Array,
      default: [],
    },
  },
  data() {
    return {
      suggestions: [],
    };
  },
  watch: {
    schema: {
      async handler(schema) {
        // ensure html is completely loaded to the dom
        this.$nextTick(() => {
          let suggestions = [];

          for (const property of schema) {
            let propertySuggestions = this.suggest(property);
            
            if (property.suggestedXpath) {
              for (const suggestion of propertySuggestions) {
                this.unhighlightSuggestionHandler(suggestion);
              }

              continue;
            }

            suggestions.push(...propertySuggestions);
          }

          console.log("suggestions", suggestions);
          this.suggestions = suggestions;
        });
      },
      immediate: true,
      deep: true,
    },
  },
  methods: {
    highlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.highlight(elements, "bg-cyan-500");
    },
    unhighlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.unhighlight(elements, "bg-cyan-500");
    },
    applySuggestionHandler(suggestion) {
      this.$emit("suggested", suggestion.property, suggestion.xpath);
    },

    suggest(property) {
      const matchedXpathIndexes = [...property.xpath.matchAll(/\[.*?\]/g)];
      var suggestions = matchedXpathIndexes
        .map((match) => {
          let before = property.xpath.substring(0, match.index);
          let after = property.xpath.substring(
            match.index + match[0].length,
            property.xpath.length
          );
          return before + after;
        })
        .map((xpath) => {
          return {
            xpath,
            elements: xpathService.evaluateXPath(document, xpath),
            property: property,
          };
        })
        .filter((x) => x.elements.length > 1);

      return suggestions;
    },
  },
};
</script>
