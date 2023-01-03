<template>
  <div class="flex flex-col">
    <div class="min-h-[50vh]">
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
    rootXPath: {
      type: String,
      default: "/html/body",
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
      async handler() {
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
      this.data = this.schema
        .map((property) => {
          let calculatedXPath = property.xpath.replace(
            "/html/body",
            this.rootXPath
          );
          var elements = xpathService.evaluateXPath(document, calculatedXPath);
          return { property, values: elements.map((x) => x.innerText) };
        })
        .toMap(
          (x) => x.property.name,
          (x) => x.values
        );
    },

    suggest() {
      let results = [];

      for (const property of this.schema) {
        let calculatedXPath = property.xpath.replace(
              "/html/body",
              this.rootXPath
            );

        const matchedXpathIndexes = [...calculatedXPath.matchAll(/\[.*?\]/g)];
        var propertySuggestions = matchedXpathIndexes
          .map((match) => {
            let before = calculatedXPath.substring(0, match.index);
            let after = calculatedXPath.substring(
              match.index + match[0].length,
              calculatedXPath.length
            );
            return before + after;
          })
          .map((suggestedXpath) => {
            return {
              xpath: suggestedXpath,
              elements: xpathService.evaluateXPath(document, suggestedXpath),
              property: property,
              originalElements: xpathService.evaluateXPath(
                document,
                calculatedXPath
              ),
            };
          });

        results.push(...propertySuggestions);
      }

      this.suggestions = results
        .filter((x) => x.elements.length > 0)
        .uniqueBy((x) => x.xpath)
        .filter((x) => !x.elements.compare(x.originalElements));
    },

    highlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.highlight(elements, "!bg-indigo-500");
    },

    unhighlightSuggestionHandler(suggestion) {
      let elements = xpathService.evaluateXPath(document, suggestion.xpath);
      highlightService.unhighlight(elements, "!bg-indigo-500");
    },

    applySuggestionHandler(suggestion) {
      this.$emit("suggested", suggestion.property, suggestion.xpath);
    },
  },
};
</script>
