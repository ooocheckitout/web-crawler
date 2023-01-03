<template>
  <div class="flex flex-row" v-if="isLoaded">
    <div class="sticky top-0 w-1/2 h-screen">
      <Schema :schema="schema" @suggested="suggestionHandler" />
    </div>
    <div class="w-1/2">
      <Viewer
        :html="html"
        :schema="schema"
        @selected="selectHandler"
        ref="viewer"
      />
    </div>
  </div>
</template>

<script>
import Schema from "./Schema.vue";
import Viewer from "./Viewer.vue";

import xpathService from "../services/xpath";

export default {
  components: {
    Schema,
    Viewer,
  },
  data() {
    return {
      // url: "https://tailwindcss.com/docs/customizing-colors",
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
      html: null,
      schema: [],
      schema_colors: [
        {
          name: "Property-1",
          xpath:
            "/html/body/div/div/div/div[2]/div/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[1]/div/div",
        },
        {
          name: "Property-2",
          xpath:
            "/html/body/div/div/div/div[2]/div/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[2]/div/div/div[2]/div[1]",
        },
        {
          name: "Property-3",
          xpath:
            "/html/body/div/div/div/div[2]/div/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[2]/div/div/div[2]/div[2]",
        },
      ],
      schema: [
        {
          name: "Operator",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[1]",
        },
        {
          name: "A95+",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[3]",
        },
        {
          name: "A95",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[4]",
        },
        {
          name: "A92",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[5]",
        },
        {
          name: "Disel",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[6]",
        },
        {
          name: "Gasoline",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[7]",
        },
      ],
      isLoaded: false,
    };
  },
  watch: {
    url: {
      async handler() {
        let response = await fetch(this.url);
        this.html = await response.text();
        this.isLoaded = true;
        console.log(this.url, "loaded");
      },
      immediate: true,
    },
  },
  methods: {
    selectHandler(element) {
      let property = {
        name: `Property-${this.schema.length + 1}`,
        xpath: xpathService.getElementXPath(element),
        viewerXPath: xpathService.getElementXPath(this.$refs.viewer.$el),
      };

      this.schema.push(property);
    },
    suggestionHandler(property, suggestedXpath) {
      console.log(property, suggestedXpath);
      property.xpath = suggestedXpath;
    },
  },
};
</script>