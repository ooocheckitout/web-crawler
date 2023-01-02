<template>
  <div class="flex flex-row">
    <div class="flex flex-col w-1/2 h-screen">
      <div class="h-1/2">
        <Schema :schema="schema" />
      </div>
      <div class="h-1/2">
        <Data :data="data" />
      </div>
    </div>
    <div class="w-1/2">
      <Viewer :url="url" @selected="selectHandler" />
    </div>
  </div>
</template>

<script>
import Schema from "./Schema.vue";
import Data from "./Data.vue";
import Viewer from "./Viewer.vue";

export default {
  components: {
    Schema,
    Data,
    Viewer,
  },
  data() {
    return {
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
      schema: [
        {
          name: "Fuel",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]",
        },
      ],
      data: [1, 2]
    };
  },
  methods: {
    selectHandler(element) {
    //     this.schema.properties.push({
    //     name: `Property-${this.schema.properties.length + 1}`,
    //     xpath: this.getElementXPath(currentElement),
    //   });
        console.log(element);
    }
  },
};
</script>