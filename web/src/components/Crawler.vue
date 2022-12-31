<template lang="">
    <div class="flex flex-col h-screen text-center">
        <div>
            <input type="text" v-model="url" class="w-full h-full p-2 border-2 border-indigo-600 border-solid rounded ">
        </div>
        <div class="grid h-full grid-cols-2 gap-2 ">
            <div class="">
                <p>Current element:</p>
                <p>{{lastElement}}</p>
                <p>{{lastElementXpath}}</p>
            </div>
            
            <div @mousemove="selectElement" @click="selectElement">
                <Viewer :url="url" />
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
      url: "https://tailwindcss.com/docs/grid-template-columns",
      lastElement: undefined,
      lastElementXpath: undefined,
    };
  },
  methods: {
    highlightElement(event){
        let element = document.elementFromPoint(
        event.clientX,
        event.clientY
      );

      const highlightClasses = "bg-sky-500";
      element.classList.add(highlightClasses);
    },

    selectElement(event) {
      let currentElement = document.elementFromPoint(
        event.clientX,
        event.clientY
      );
      if (currentElement == this.lastElement) return;

      const highlightClasses = "bg-sky-500";

      if (this.lastElement != undefined) {
        // clean
        this.lastElement.classList.remove(highlightClasses);
        // highlight
        currentElement.classList.add(highlightClasses);
      }

      this.lastElement = currentElement;
      this.lastElementXpath = this.getElementXPath(this.lastElement);
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
