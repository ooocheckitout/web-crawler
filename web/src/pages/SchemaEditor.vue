<template>
  <div>
    <div class="flex flex-row">
      <div class="w-1/2 p-4 border">
        <p class="text-lg font-bold">Schema</p>
        <DynamicTable :objects="properties" :key="isLoaded" />
        <p class="text-lg font-bold">Suggestions</p>
        <DynamicTable
          :objects="suggestions"
          :columns="['property.name', 'suggestedXpath', 'suggestedElements.length']"
          @itemMouseOver="highlightSuggestionHandler"
          @itemMouseOut="unhighlightSuggestionHandler"
          @itemClick="applySuggestionHandler"
        />
        <p class="text-lg font-bold">Data</p>
        <DynamicTable :objects="datas" :columns="['*', 'values.length']" />
      </div>
      <div class="w-1/2 p-4 border">
        <IFrameViewer
          id="viewer"
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
      isLoaded: true,
      viewerUrl: null,
      contextDocument: null,
      selectedElements: [],
      suggestedElements: [],
      properties: [
        {
          name: "Username",
          xpath: "/html/body/div/div/div/div[1]/blz-section/div/div[1]/div/h1",
        },
        {
          name: "Category",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div[1]/select/option",
        },
        {
          name: "GameMode",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[1]/blz-button",
        },
        {
          name: "Platform",
          xpath: "/html/body/div/div/div/div[1]/blz-section/div/div[3]/blz-button",
        },
        {
          name: "QuickPlay_Hero",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div/div/div/div[2]/div[1]",
        },
        {
          name: "QuickPlay_Statistic",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div/div/div/div[2]/div[2]",
        },
        {
          name: "CompetitivePlay_Hero",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[3]/div/div/div/div[2]/div[1]",
        },
        {
          name: "CompetitivePlay_Statistic",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[3]/div/div/div/div[2]/div[2]",
        },
      ],
      suggestions: [],
      datas: [],
    };
  },

  watch: {
    properties: {
      deep: true,
      handler(current, previous) {
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

    selectHandler(element) {
      let xpath = xpathService.getElementTreeXPath(element);

      let property = {
        name: `Property-${this.properties.length + 1}`,
        xpath,
      };
      this.properties.push(property);
    },

    updateProperties() {
      this.suggestions = [];
      this.datas = [];

      for (const property of this.properties) {
        let originalXpath = property.xpath;
        let originalElements = xpathService.evaluateXPath(this.contextDocument, originalXpath);

        // selected elements
        this.selectedElements.push(originalElements);

        // child suggestions
        let childElements = originalElements.filter(x => x.children.length > 0).flatMap(x => Array.from(x));
        let childElementsXpaths = childElements.map(x => xpathService.getElementXPath(x));
        let childrenSuggestedXpaths = childElementsXpaths.flatMap(x => this.suggestXpaths(x));

        // xpath suggestions
        let originalSuggestedXpaths = this.suggestXpaths(originalXpath);
        for (const suggestedXpath of originalSuggestedXpaths.concat(childrenSuggestedXpaths)) {
          if (this.suggestions.find(x => x.suggestedXpath == suggestedXpath)) continue;

          let suggestedElements = xpathService.evaluateXPath(this.contextDocument, suggestedXpath);
          if (suggestedElements.length == 0 || originalElements.compare(suggestedElements)) continue;

          this.suggestions.push({ property, originalElements, suggestedXpath, suggestedElements });
        }

        // data
        let values = originalElements.map(x => x.innerText);
        this.datas.push({
          property: property.name,
          values: values.length == 1 ? values[0] : values,
        });
      }

      console.log("suggestions", this.suggestions);
      console.log("datas", this.datas);
    },

    suggestXpaths(originalXpath) {
      const matchedXpathIndexes = [...originalXpath.matchAll(/\[.*?\]/g)];

      var suggestedXpaths = matchedXpathIndexes.map(match => {
        let before = originalXpath.substring(0, match.index);
        let after = originalXpath.substring(match.index + match[0].length, originalXpath.length);
        return before + after;
      });

      return suggestedXpaths;
    },

    loadedHandler(contextDocument) {
      this.contextDocument = contextDocument;
      this.isLoaded = true;
      this.updateProperties();
    },
  },

  created() {
    this.viewerUrl = this.$route.query.viewerUrl;
  },
};
</script>
