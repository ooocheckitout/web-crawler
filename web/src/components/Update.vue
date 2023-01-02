<template>
  <div>
    <div class="flex flex-row">
      <div class="flex flex-col w-1/2 h-screen">
        <div class="h-1/2">
          <p>Schema</p>
          <pre>{{ schema }}</pre>
        </div>
        <div class="h-1/2">
          <p>Data</p>
          <pre>{{ datas }}</pre>
        </div>
      </div>
      <div class="w-1/2">
        <div
          id="viewer"
          v-html="htmlContent"
          @mousemove="highlightHandler"
          @click="selectHandler"
        ></div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
      htmlContent: null,
      _properties: [
        {
          name: "Fuels",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]",
        },
        {
          name: "Prices",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[2]",
        },
        {
          name: "Percentage",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[3]",
        },
        {
          name: "Region",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[1]/ul/li[5]/span",
        },
        {
          name: "Urls",
          xpath:
            "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/ul/li[2]/ul/li",
        },
      ],
      properties: [
        {
          name: "FuelPricesPerCompany",
          properties: [
            {
              name: "Fuel",
              xpath:
                "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]",
            },
            {
              name: "A 95+ Price",
              xpath:
                "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[1]",
            },
            {
              name: "Company",
              xpath:
                "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/div[4]/table/tbody/tr/td[1]/a",
            },
          ],
        },
      ],

      schema: {
        name: "FuelPricesPerCompany",
        properties: [
          {
            name: "Fuel",
            xpath:
              "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]",
          },
          {
            name: "Price",
            xpath:
              "/html/body/div/div/div/div/div[2]/div/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[2]",
          },
        ],
      },
      datas: [],
    };
  },
  updated() {
    if (this.datas.length != 0) return;

    this.updateData();
  },
  watch: {
    url: {
      async handler(newValue, oldValue) {
        let response = await fetch(newValue);
        this.htmlContent = await response.text();
      },
      immediate: true,
    },
    schema: {
      handler(newValue, oldValue) {
        this.updateData();
        this.suggest();
      },
      deep: true,
    },
  },
  methods: {
    updateData() {
      this.datas = {};

      let propertyWithValues = this.schema.properties.map((property) => {
        var elements = this.evaluateXPath(document, property.xpath);
        return { property, values: elements.map((x) => x.innerText) };
      });

      let numberOfElements = propertyWithValues[0].values.length;
      if (!propertyWithValues.every((x) => x.values.length == numberOfElements))
        throw "Number of elements should be equal!";

      console.log(propertyWithValues);

      let objects = [];
      for (let index = 0; index < numberOfElements; index++) {
        let currentObject = {};
        for (const propertyWithValue of propertyWithValues) {
          currentObject[propertyWithValue.property.name] =
            propertyWithValue.values[index];
        }
        objects.push(currentObject);
      }

      console.log(objects);

      this.datas = { [this.schema.name]: objects };
    },

    suggest() {
      for (const property of this.properties) {
        const array = [...property.xpath.matchAll(/\[.*?\]/g)];

        var suggestions = array
          .map(
            (x) =>
              property.xpath.substring(0, x.index) +
              property.xpath.substring(
                x.index + x[0].length,
                property.xpath.length
              )
          )
          .map((x) => {
            return { xpath: x, elements: this.evaluateXPath(document, x) };
          })
          .filter((x) => x.elements.length > 1);

        console.log(suggestions);
      }
    },

    highlight(elements, highlightClass) {
      if (!Array.isArray(elements)) elements = [elements];

      for (const element of elements) {
        if (element.classList?.contains(highlightClass)) continue;

        element.classList.add(highlightClass);
      }
    },

    unhighlight(elements, highlightClass) {
      if (!Array.isArray(elements)) elements = [elements];

      for (const element of elements) {
        if (!element.classList?.contains(highlightClass)) continue;

        element.classList.remove(highlightClass);
      }
    },

    highlightHandler(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      if (currentElement == this.lastHighlightedElement) return;

      const highlightClass = "bg-sky-500";

      if (this.lastHighlightedElement != undefined) {
        this.unhighlight(this.lastHighlightedElement, highlightClass);
      }

      this.highlight(currentElement, highlightClass);

      this.lastHighlightedElement = currentElement;
    },

    selectHandler(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      const highlightClass = "bg-red-500";

      if (this.lastHighlightedElement != undefined) {
        this.unhighlight(this.lastHighlightedElement, highlightClass);
      }

      this.highlight(currentElement, highlightClass);

      this.schema.properties.push({
        name: `Property-${this.schema.properties.length + 1}`,
        xpath: this.getElementXPath(currentElement),
      });
    },

    
  },
};
</script>