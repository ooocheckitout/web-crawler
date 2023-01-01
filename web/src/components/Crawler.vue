<template lang="">
  <div>
    <input type="text" v-model="url" class="w-full h-full p-2 border-2 border-indigo-600 border-solid rounded " />

    <div class="flex flex-row">
      <div class="sticky top-0 flex flex-col w-1/2 h-screen ">
        <div class="h-1/2">
          <div>
            <p>Selected elements:</p>
            <div v-for="(item, index) in selectedElements" :key="index">
              <p>{{ item.selectedSuggestion ? item.selectedSuggestion.xpath : item.xpath }}</p>

              <p>Suggestions:</p>
              <div v-for="(suggestion, index) in item.suggestedXpaths" :key="index">
                <input type="button" :value="suggestion.xpath"
                    @mouseover="highlight(suggestion.elements, 'bg-sky-500')"
                    @mouseleave="unhighlight(suggestion.elements, 'bg-sky-500')"
                    @click="applySuggestion(suggestion, item)" />
              </div>
            </div>
          </div>
        </div>
        <div class="h-1/2">
          <p>Selected elements:</p><div v-for="(item, index) in selectedElements" :key="index">
            <p>{{item.text}}</p>
          </div>
        </div>  
      </div>
      <div class="w-1/2">
        <!-- <Viewer :url="url" @mousemove="highlightHandler" @click="selectHandler" /> -->
        <iframe :src="url" frameborder="0" class="w-full h-full" @mousemove="highlightHandler" @click="selectHandler"></iframe>
      </div>
    </div>
  </div>
</template>
<script>
import Viewer from "./Viewer.vue";

export default {
  components: {
    Viewer,
  },
  data() {
    return {
      url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/", //"https://tailwindcss.com/docs/grid-template-columns",

      lastHighlightedElement: null,
      selectedElements: [],

      properties: [],
    };
  },
  watch: {
    url: {
      handler: function() {
        this.lastHighlightedElement = null;
        this.selectedElements = []
      }
    }
  },
  methods: {
    applySuggestion(suggestion, item){
      const highlightClass = "bg-red-500";

      if (item.selectedSuggestion)
        this.unhighlight(item.selectedSuggestion.elements, highlightClass)
      
      console.log(item.selectedSuggestion);

      item.selectedSuggestion = suggestion;
      this.unhighlight(item.element, highlightClass)
      this.highlight(suggestion.elements, highlightClass)
    },

    highlight(elements, highlightClass){
      if (!Array.isArray(elements))
        elements = [elements]

      for (const element of elements) {
        if (element.classList?.contains(highlightClass))
          continue;

        element.classList.add(highlightClass);
      }
    },

    unhighlight(elements, highlightClass){
      if (!Array.isArray(elements))
        elements = [elements]

      for (const element of elements) {
        if (!element.classList?.contains(highlightClass))
          continue;

          element.classList.remove(highlightClass);
      }
    },

    highlightHandler(event) {
      let currentElement = document.elementFromPoint(event.clientX, event.clientY);

      if (currentElement == this.lastHighlightedElement) return;

      const highlightClass = "bg-sky-500";

      if (this.lastHighlightedElement != undefined) {
        this.unhighlight(this.lastHighlightedElement, highlightClass);
      }

      this.highlight(currentElement, highlightClass);

      this.lastHighlightedElement = currentElement;
    },

    selectHandler(event) {
      let currentElement = document.elementFromPoint(event.clientX, event.clientY);
      
      if (this.selectedElements.find(x => x.element == currentElement)) return;

      const highlightClass = "bg-red-500";

      if (this.lastHighlightedElement != undefined) {
        this.unhighlight(this.lastHighlightedElement, highlightClass);
      }

      this.highlight(currentElement, highlightClass);

      var xpath = this.getElementXPath(currentElement);
      const array = [...xpath.matchAll(/\[.*?\]/g)];

      var suggestions = array
        .map(x => xpath.substring(0, x.index) + xpath.substring(x.index+x[0].length, xpath.length))
        .map(x => { return {xpath: x, elements: this.evaluateXPath(document, x)} })
        .filter(x => x.elements.length > 1)

      this.selectedElements.push({
        element: currentElement,
        xpath: xpath,
        suggestedXpaths: suggestions,
        selectedSuggestion: null,
        text: currentElement.innerText
      })

      this.properties.push({
        Name: `Property-${this.properties.length}`,
        Xpath: xpath
      })
    },

    // https://github.com/firebug/firebug/blob/master/extension/content/firebug/lib/xpath.js
    getElementXPath(element) {
      if (element && element.id) return '//*[@id="' + element.id + '"]';
      else return this.getElementTreeXPath(element);
    },

    getElementTreeXPath(element) {
      var paths = [];

      // Use nodeName (instead of localName) so namespace prefix is included (if any).
      for (
        ;
        element && element.nodeType == Node.ELEMENT_NODE;
        element = element.parentNode
      ) {
        var index = 0;
        var hasFollowingSiblings = false;
        for (
          var sibling = element.previousSibling;
          sibling;
          sibling = sibling.previousSibling
        ) {
          // Ignore document type declaration.
          if (sibling.nodeType == Node.DOCUMENT_TYPE_NODE) continue;

          if (sibling.nodeName == element.nodeName) ++index;
        }

        for (
          var sibling = element.nextSibling;
          sibling && !hasFollowingSiblings;
          sibling = sibling.nextSibling
        ) {
          if (sibling.nodeName == element.nodeName) hasFollowingSiblings = true;
        }

        var tagName =
          (element.prefix ? element.prefix + ":" : "") + element.localName;
        var pathIndex =
          index || hasFollowingSiblings ? "[" + (index + 1) + "]" : "";
        paths.splice(0, 0, tagName + pathIndex);
      }

      return paths.length ? "/" + paths.join("/") : null;
    },
    
    evaluateXPath(doc, xpath, contextNode, resultType) {
      if (contextNode === undefined) contextNode = doc;

      if (resultType === undefined) resultType = XPathResult.ANY_TYPE;

      var result = doc.evaluate(xpath, contextNode, null, resultType, null);

      switch (result.resultType) {
        case XPathResult.NUMBER_TYPE:
          return result.numberValue;

        case XPathResult.STRING_TYPE:
          return result.stringValue;

        case XPathResult.BOOLEAN_TYPE:
          return result.booleanValue;

        case XPathResult.UNORDERED_NODE_ITERATOR_TYPE:
        case XPathResult.ORDERED_NODE_ITERATOR_TYPE:
          var nodes = [];
          for (
            var item = result.iterateNext();
            item;
            item = result.iterateNext()
          )
            nodes.push(item);
          return nodes;

        case XPathResult.UNORDERED_NODE_SNAPSHOT_TYPE:
        case XPathResult.ORDERED_NODE_SNAPSHOT_TYPE:
          var nodes = [];
          for (var i = 0; i < result.snapshotLength; ++i)
            nodes.push(result.snapshotItem(i));
          return nodes;

        case XPathResult.ANY_UNORDERED_NODE_TYPE:
        case XPathResult.FIRST_ORDERED_NODE_TYPE:
          return result.singleNodeValue;
      }
    },
  },
};
</script>
