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
      properties: [],
      overwatch_properties: [
        {
          name: "Username",
          xpath: "/html/body/div/div/div/div[1]/blz-section/div/div[1]/div/h1",
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
          name: "TopHero_Category",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div[1]/select/option",
        },
        {
          name: "TopHero_QuickPlay_Hero",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div/div/div/div[2]/div[1]",
        },
        {
          name: "TopHero_QuickPlay_Statistic",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div/div/div/div[2]/div[2]",
        },
        {
          name: "TopHero_CompetitivePlay_Hero",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[3]/div/div/div/div[2]/div[1]",
        },
        {
          name: "TopHero_CompetitivePlay_Value",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[3]/div/div/div/div[2]/div[2]",
        },
        {
          name: "CareerStats_Category",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/div[4]/select/option",
        },
        {
          name: "CareerStats_QuickPlay_All_Group",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[1]/div/div/div[1]/p",
        },
        {
          name: "CareerStats_QuickPlay_All_Title",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[1]/div/div/div[@class='stat-item']/p[1]",
        },
        {
          name: "CareerStats_QuickPlay_All_Values",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[1]/div/div/div[@class='stat-item']/p[2]",
        },
        {
          name: "CareerStats_QuickPlay_Ana_Group",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[2]/div/div/div[1]/p",
        },
        {
          name: "CareerStats_QuickPlay_Ana_Title",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[2]/div/div/div[@class='stat-item']/p[1]",
        },
        {
          name: "CareerStats_QuickPlay_Ana_Values",
          xpath: "/html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[2]/div/div/div[@class='stat-item']/p[2]",
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
        let childSuggestedXpaths = childElementsXpaths.flatMap(x => this.suggestXpaths(x));

        // TODO: class suggestions
        // example: /html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[1]/div/div/div[@class='stat-item']/p[1]

        // TODO: xpath similarity suggestions
        // example1: /html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[1]/div/div/div[1]/p
        // example2: /html/body/div/div/div/div[1]/div[1]/blz-section[2]/span[2]/div/div/div[1]/p

        // xpath suggestions
        let originalSuggestedXpaths = this.suggestXpaths(originalXpath);
        for (const suggestedXpath of originalSuggestedXpaths.concat(childSuggestedXpaths)) {
          if (this.suggestions.find(x => x.suggestedXpath == suggestedXpath)) continue;

          let suggestedElements = xpathService.evaluateXPath(this.contextDocument, suggestedXpath);
          if (suggestedElements.length == 0 || originalElements.compare(suggestedElements)) continue;

          this.suggestions.push({ property, originalElements, suggestedXpath, suggestedElements });
        }

        // data
        this.datas.push({
          property: property.name,
          values: originalElements.map(x => x.innerText),
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
