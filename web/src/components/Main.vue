<template>
  <div>
    <div class="flex flex-row space-x-2">
      <label v-for="(item, index) in examples" :key="index" :for="index">
        <input
          type="radio"
          name="example"
          :value="index"
          :id="index"
          @click="selectExample(item)"
        />
        <span>{{ item.name }}</span>
      </label>
    </div>
    <input
      type="text"
      v-model="url"
      class="w-full p-2 border-2 border-indigo-500 rounded"
    />
    <div class="flex flex-row" :key="isLoaded">
      <div class="sticky top-0 w-1/2 h-screen">
        <Schema
          :schema="schema"
          :rootXPath="rootXPath"
          @suggested="suggestionHandler"
        />
      </div>
      <div class="w-1/2">
        <Viewer
          :html="html"
          :schema="schema"
          :rootXPath="rootXPath"
          @selected="selectHandler"
          ref="viewer"
        />
      </div>
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
      url: null,
      html: null,
      schema: [],
      isLoaded: false,
      rootXPath: null,
      examples: [
        {
          name: "Tailwind Documentation Colors",
          url: "https://tailwindcss.com/docs/customizing-colors",
          schema: [
            {
              name: "Color",
              xpath:
                "/html/body/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[1]/div/div",
            },
            {
              name: "Shade",
              xpath:
                "/html/body/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[2]/div/div/div[2]/div[1]",
            },
            {
              name: "Hex",
              xpath:
                "/html/body/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div/div[2]/div/div/div[2]/div[2]",
            },
          ],
        },
        {
          name: "Minfin Fuel Prices",
          url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya",
          schema: [
            {
              name: "Operator",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[1]",
            },
            {
              name: "A95Plus",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[3]",
            },
            {
              name: "A95",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[4]",
            },
            {
              name: "A92",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[5]",
            },
            {
              name: "Disel",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[6]",
            },
            {
              name: "Gasoline",
              xpath:
                "/html/body/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[7]",
            },
          ],
        },
      ],
    };
  },
  watch: {
    url: {
      async handler() {
        let response = await fetch(this.url);
        this.html = await response.text();
        this.isLoaded = true;
        (this.rootXPath = xpathService.getElementXPath(this.$refs.viewer.$el)),
          console.log(this.url, "loaded");
      },
      immediate: true,
    },
  },
  methods: {
    selectHandler(element) {
      let xpath = xpathService.getElementXPath(element);
      let originalDocumentXPath = xpath.replace(this.rootXPath, "/html/body");

      let property = {
        name: `Property-${this.schema.length + 1}`,
        xpath: originalDocumentXPath,
      };

      this.schema.push(property);
    },
    suggestionHandler(property, suggestedXpath) {
      let originalDocumentXPath = suggestedXpath.replace(
        this.rootXPath,
        "/html/body"
      );
      property.xpath = originalDocumentXPath;
    },

    selectExample(example) {
      this.isLoaded = false;
      this.url = example.url;
      this.$nextTick(() => {
        this.schema = example.schema;
      });
    },
  },
};
</script>