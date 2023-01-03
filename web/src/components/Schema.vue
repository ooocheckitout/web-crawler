<template>
  <div class="flex flex-col">
    <div class="min-h-[50vh]">
      <p>Schema</p>
      <pre>{{ schema }}</pre>
      <p>Suggestions</p>
      <pre>{{ suggestions.map(x => x.xpath) }}</pre>
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

    <div>
      <p>Data</p>
      <pre>{{ data }}</pre>
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
      data: {},
    };
  },
  watch: {
    schema: {
      async handler(schema) {
        // ensure html is completely loaded to the dom
        this.$nextTick(() => {
          this.updateData();
          this.suggest();
        });
      },
      immediate: true,
      deep: true,
    },
  },
  methods: {
    updateData() {
      let results = [];

      for (const property of this.schema) {
        var elements = xpathService.evaluateXPath(document, property.xpath);
        results.push({ property, values: elements.map((x) => x.innerText) });
      }

      this.data = results.reduce((map, val) => {
        map[val.property.name] = val.values;
        return map;
      }, {});
    },
    suggest() {
      let results = [];

      for (const property of this.schema) {
        // /html/body/div[1]/h1 -> /html/body/div/h1
        const matchedXpathIndexes = [...property.xpath.matchAll(/\[.*?\]/g)];
        var propertySuggestions = matchedXpathIndexes
          .map((match) => {
            let before = property.xpath.substring(0, match.index);
            let after = property.xpath.substring(
              match.index + match[0].length,
              property.xpath.length
            );
            return before + after;
          })
          .map((suggestedXpath) => {
            return {
              xpath: suggestedXpath,
              elements: xpathService.evaluateXPath(document, suggestedXpath),
              property: property,
            };
          });

        results.push(propertySuggestions);
      }

      this.suggestions = results
        .flatMap((x) => x)
        .filter((x) => x.elements.length > 0)
        .map(x => x);
    },

    highlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.highlight(elements, "!bg-cyan-500");
    },

    unhighlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.unhighlight(elements, "!bg-cyan-500");
    },

    applySuggestionHandler(suggestion) {
      this.$emit("suggested", suggestion.property, suggestion.xpath);
    },
  },
};
</script>
