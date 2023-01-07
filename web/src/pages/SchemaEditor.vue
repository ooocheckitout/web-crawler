<template>
  <div>
    <div class="flex flex-row min-h-screen">
      <div class="w-1/2 p-4 border">
        <p class="text-lg font-bold">Schema</p>
        <DynamicTable :objects="contextProperties" :key="isLoaded" />
        <p class="text-lg font-bold">Data</p>
        <DynamicTable :objects="datas" :columns="['*', 'values.length']" />
        <p class="text-lg font-bold">Suggestions</p>
        <DynamicTable
          :objects="suggestions"
          :columns="['property.name', 'suggestedXpath', 'suggestedElements.length', 'from']"
          @itemMouseOver="highlightSuggestionHandler"
          @itemMouseOut="unhighlightSuggestionHandler"
          @itemClick="applySuggestionHandler"
        />
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
import suggestionService from "@/services/suggestions";

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
      contextElementXPath: null,
      selectedElements: [],
      suggestedElements: [],
      properties: [],
      contextProperties: [],
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

        this.contextProperties = this.properties.map(x => {
          return {
            name: x.name,
            xpath: x.xpath.replace(this.contextElementXPath, "/html/body"),
            attribute: x.attribute,
          };
        });
      },
    },
  },

  methods: {
    applySuggestionHandler(suggestion) {
      suggestion.property.xpath = suggestion.suggestedXpath;
      this.unhighlightSuggestionHandler(suggestion);
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
      this.selectedElements = [];

      for (const property of this.properties) {
        let originalXpath = property.xpath;
        let originalElements = xpathService.evaluateXPath(this.contextDocument, originalXpath);

        this.selectedElements.push(...originalElements);

        let suggestions = [
          { type: "attribute", xpaths: suggestionService.suggestAttribute(originalXpath) },
          { type: "text", xpaths: suggestionService.suggestText(originalXpath) },
          { type: "parent", xpaths: suggestionService.suggestParent(originalXpath, this.contextDocument) },
          { type: "children", xpaths: suggestionService.suggestChildren(originalXpath, this.contextDocument) },
          { type: "changeIndex", xpaths: suggestionService.suggestChangeIndex(originalXpath) },
          { type: "removeIndex", xpaths: suggestionService.suggestRemoveIndex(originalXpath) },
        ];

        for (const suggestion of suggestions) {
          for (const suggestedXpath of suggestion.xpaths) {
            if (this.suggestions.find(x => x.suggestedXpath == suggestedXpath)) continue; // was already suggested

            let suggestedElements = xpathService.evaluateXPath(this.contextDocument, suggestedXpath);

            if (originalElements.compare(suggestedElements)) continue; // same results as the original xpath

            if (this.suggestions.find(x => x.suggestedElements.compare(suggestedElements))) continue; // results were already suggested

            if (suggestedElements.length == 0 || suggestedElements.length > 1000) continue; // too little or too many results

            this.suggestions.push({
              property,
              originalElements,
              suggestedXpath,
              suggestedElements,
              from: suggestion.type,
            });
          }
        }

        this.datas.push({
          name: property.name,
          values: property.attribute
            ? originalElements.map(x => x.attributes[property.attribute].value)
            : originalElements.map(x => x.innerText),
        });
      }

      console.log("suggestions", this.suggestions);
      console.log("datas", this.datas);
    },

    loadedHandler(contextDocument, viewerXpath) {
      this.contextElementXPath = viewerXpath;
      this.contextDocument = contextDocument;

      this.isLoaded = true;

      // this.properties = this.overwatch_properties;
      this.properties = [
        {
          name: "Property-1",
          xpath: "/html/body/div/div/div/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a",
          attribute: "href",
        },
      ];

      this.updateProperties();
    },
  },

  created() {
    this.viewerUrl = this.$route.query.viewerUrl;
  },
};
</script>
