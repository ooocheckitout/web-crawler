<template>
  <div>
    <div class="flex flex-row">
      <div class="w-1/2 h-screen border p-4">
        <p class="text-lg font-bold">Schema</p>
        <DynamicTable :objects="properties" :key="properties" />
        <p class="text-lg font-bold">Suggestions</p>
        <DynamicTable
          :objects="suggestions"
          :key="suggestions"
          @itemMouseOver="highlightSuggestionHandler"
          @itemMouseOut="unhighlightSuggestionHandler"
          @itemClick="applySuggestionHandler"
        />
        <p class="text-lg font-bold">Data</p>
        <DynamicTable :objects="datas" :key="datas" />
      </div>
      <div class="w-1/2 border p-4">
        <IFrameViewer
          :url="viewerUrl"
          :previewElements="suggestedElements"
          :highlightElements="selectedElements"
          @selected="selectHandler"
          @loaded="loadedHandler"
        />
      </div>
    </div>
  </div>
</template>

<script>
import DynamicTable from "@/components/DynamicTable.vue";

import IFrameViewer from "@/components/IFrameViewer.vue";
import xpathService from "@/services/xpath";

export default {
  components: {
    IFrameViewer,
    DynamicTable,
  },

  data() {
    return {
      viewerUrl: null,
      contextDocument: null,
      selectedElements: [],
      suggestedElements: [],
      properties: [],
      suggestions: [],
      datas: [],
    };
  },

  watch: {
    properties: {
      deep: true,
      handler() {
        this.updateProperties();
      },
    },
  },

  methods: {
    applySuggestionHandler(suggestion) {
      suggestion.property.xpath = suggestion.suggestedXpath;
    },

    highlightSuggestionHandler(suggestion) {
      this.suggestedElements.push(...suggestion.suggestedElements);
    },

    unhighlightSuggestionHandler(suggestion) {
      this.suggestedElements = this.suggestedElements.filter(x => !suggestion.suggestedElements.includes(x));
    },

    suggestXpaths(originalXpath) {
      const matchedXpathIndexes = [...originalXpath.matchAll(/\[.*?\]/g)];

      var suggestedXpaths = matchedXpathIndexes.map(match => {
        let before = originalXpath.substring(0, match.index);
        let after = originalXpath.substring(match.index + match[0].length, originalXpath.length);
        return before + after;
      });

      return suggestedXpaths.uniqueBy(x => x);
    },

    updateProperties() {
      let suggestions = [];
      let datas = [];

      for (const property of this.properties) {
        let originalXpath = property.xpath;
        let originalElements = xpathService.evaluateXPath(this.contextDocument, originalXpath);

        // selected elements
        this.selectedElements.push(originalElements);

        // suggestions
        var propertySuggestions = this.suggestXpaths(originalXpath).map(suggestedXpath => {
          return {
            property,
            originalElements,
            suggestedXpath,
            suggestedElements: xpathService.evaluateXPath(this.contextDocument, suggestedXpath),
          };
        });

        suggestions.push(...propertySuggestions);

        // data
        datas.push({
          property: property.name,
          values: originalElements.map(x => x.innerText),
        });
      }

      this.suggestions = suggestions
        .filter(x => x.suggestedElements.length > 0)
        .uniqueBy(x => x.suggestedXpath)
        .filter(x => !x.suggestedElements.compare(x.originalElements));

      this.datas = datas;
    },

    selectHandler(element) {
      let xpath = xpathService.getElementXPath(element);

      let property = {
        name: `Property-${this.properties.length + 1}`,
        xpath,
      };
      this.properties.push(property);

      this.properties = JSON.parse(JSON.stringify(this.properties))

    },

    loadedHandler(contextDocument) {
      this.contextDocument = contextDocument;
      this.updateProperties();
    },
  },

  created() {
    this.viewerUrl = this.$route.query.viewerUrl;
  },
};
</script>
