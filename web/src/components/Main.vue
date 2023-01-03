<template>
  <div class="flex flex-row" v-if="isLoaded">
    <div class="w-1/2">
      <Schema :schema="schema" />
    </div>
    <div class="w-1/2">
      <Viewer :html="html" :schema="schema" @selected="selectHandler" />
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
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
      html: null,
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
      };

      this.schema.push(property);
    },
    suggestionHandler(property, suggestedXpath) {
      property.suggestedXpath = suggestedXpath;
    },
  },
};
</script>