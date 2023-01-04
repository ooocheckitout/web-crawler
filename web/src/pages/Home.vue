<template>
  <div>
    <div class="flex flex-row">
      <div class="sticky top-0 w-1/2 h-screen border">
        <p>Schema</p>
        <p>{{ isLoaded }}</p>
        <pre>{{ properties }}</pre>
      </div>
      <div class="w-1/2 border">
        <IFrameViewer
          :previewElements="suggestedElements"
          :highlightedElements="selectedElements"
          @selected="selectHandler"
          @loaded="loadedHandler"
        />
      </div>
    </div>
  </div>
</template>

<script>
import IFrameViewer from "@/components/IFrameViewer.vue";
import xpathService from "@/services/xpath";

export default {
  components: {
    IFrameViewer,
  },
  data() {
    return {
      isLoaded: false,
      selectedElements: [],
      suggestedElements: [],
      properties: [
        {
          name: "Property-1",
          xpath:
            "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div[2]/div[1]/div/div[2]/div[1]",
        },
        {
          name: "Property-2",
          xpath:
            "/html/body/div/div/div/div[1]/div[1]/blz-section[1]/div[2]/div[2]/div[1]/div/div[2]/div[2]",
        },
      ],
    };
  },
  watch: {
    properties: {
      deep: true,
      handler() {
        this.updateSelectedElements();
      },
    },
  },
  methods: {
    updateSelectedElements() {
      for (const property of this.properties) {
        let el = xpathService.evaluateXPath(
          this.contextDocument,
          property.xpath
        );
        this.selectedElements.push(el);
      }
    },

    selectHandler(element) {
      let xpath = xpathService.getElementXPath(element);

      let property = {
        name: `Property-${this.properties.length + 1}`,
        xpath,
      };
      this.properties.push(property);
    },

    loadedHandler(contextDocument) {
      this.contextDocument = contextDocument;
      this.updateSelectedElements();
    },
  },
};
</script>