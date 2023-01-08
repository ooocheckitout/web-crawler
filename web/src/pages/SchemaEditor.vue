<template>
  <div>
    <div class="flex flex-row min-h-screen">
      <div class="w-1/2 p-4 border">
        <p class="text-lg font-bold">Schema</p>
        <DynamicTable :objects="properties" @itemClick="removePropertyHandler" :key="isLoaded" />
        <p class="text-lg font-bold">Data</p>
        <DynamicTable :objects="datas" :columns="['*', 'values.length']" />
        <p class="text-lg font-bold">Suggestions</p>
        <DynamicTable :objects="suggestions"
          :columns="['property.name', 'suggestedXpath', 'suggestedElements.length', 'from']"
          @itemMouseOver="highlightSuggestionHandler" @itemMouseOut="unhighlightSuggestionHandler"
          @itemClick="applySuggestionHandler" />
      </div>
      <div class="w-1/2 p-4 border">
        <IFrameViewer id="viewer" :url="viewerUrl" :previewElements="suggestedElements"
          :highlightElements="selectedElements" @selected="selectHandler" @loaded="loadedHandler" />
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
      selectedElements: [],
      suggestedElements: [],
      properties: [],
      contextProperties: [],
      overwatch_properties: [
        {
          "name": "TopHero_QuickPlay_TimePlayed_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[2]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_TimePlayed_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[2]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_GamesWon_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[3]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_GamesWon_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[3]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_WeaponAccuracy_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[4]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_WeaponAccuracy_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[4]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_EliminationsPerLife_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[5]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_EliminationsPerLife_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[5]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_CriticalHitAccuracy_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[6]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_CriticalHitAccuracy_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[6]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_MultikillBest_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[7]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_MultikillBest_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[7]/div/div/div[2]/div[2]"
        },
        {
          "name": "TopHero_QuickPlay_ObjectiveKills_Hero",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[8]/div/div/div[2]/div[1]"
        },
        {
          "name": "TopHero_QuickPlay_ObjectiveKills_Statistic",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[2]/div[8]/div/div/div[2]/div[2]"
        },
        {
          "name": "Username",
          "xpath": "/html/body/div[1]/blz-section/div/div[1]/div/h1"
        },
        {
          "name": "Platform",
          "xpath": "/html/body/div[1]/blz-section/div/div[3]/blz-button"
        },
        {
          "name": "GameMode",
          "xpath": "/html/body/div[1]/div[1]/blz-section[1]/div[1]/blz-button"
        }
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
    removePropertyHandler(property) {
      let index = this.properties.indexOf(property)
      this.properties.splice(index, 1)
    },

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

    loadedHandler(contextDocument) {
      this.contextDocument = contextDocument;

      this.isLoaded = true;

      // this.properties = this.overwatch_properties;

      this.updateProperties();
    },
  },

  created() {
    this.viewerUrl = this.$route.query.viewerUrl;
  },
};
</script>
