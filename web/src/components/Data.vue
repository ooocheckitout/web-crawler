<template>
  <div>
    <p>Data</p>
    <pre>{{ data }}</pre>
  </div>
</template>

<script>
import xpathService from "../services/xpath";

export default {
  props: {
    schema: {
      type: Array,
      default: [],
    },
  },
  data() {
    return {
      data: {},
    };
  },
  watch: {
    schema: {
      async handler(schema) {
        // ensure html is completely loaded to the dom
        this.$nextTick(() => {
          for (const property of schema) {
            this.data[property.name] = this.retrieveValues(property);
          }

          console.log("data", this.data);
        });
      },
      immediate: true,
      deep: true,
    },
  },
  methods: {
    retrieveValues(property) {
      var elements = xpathService.evaluateXPath(
        document,
        property.suggestedXpath ?? property.xpath
      );

      if (property.attribute) {
        return elements.map((x) => x.attribute[property.attribute].innerText);
      }

      return elements.map((x) => x.innerText);
    },
  },
};
</script>