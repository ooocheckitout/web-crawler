<template>
  <div class="flex flex-row" v-if="isLoaded">
    <div class="flex flex-col w-1/2">
      <div class="min-h-[50vh]">
        <Schema :schema="schema" @suggested="suggestionHandler" />
      </div>
      <div>
        <Data :schema="schema" />
      </div>
    </div>
    <div class="w-1/2">
      <Viewer :html="html" :schema="schema" @selected="selectHandler" />
    </div>
  </div>
</template>

<script>
import Schema from "./Schema.vue";
import Data from "./Data.vue";
import Viewer from "./Viewer.vue";

import xpathService from "../services/xpath";

export default {
  components: {
    Schema,
    Data,
    Viewer,
  },
  data() {
    return {
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
      html: null,
      schema: [
        {
          name: "Fuel",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr[2]/td[1]",
          suggestedXpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]",
        },
        {
          name: "Price",
          xpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr[2]/td[2]",
          suggestedXpath:
            "/html/body/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[2]",
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
      };

      this.schema.push(property);
    },
    suggestionHandler(property, suggestedXpath) {
      property.suggestedXpath = suggestedXpath;
    },
  },
};
</script>