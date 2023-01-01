<template>
    <div>
        <div class="flex flex-row">
            <div class="sticky top-0 flex flex-col w-1/2 h-screen">
                <div class="h-1/2">
                    <p>Schema</p>
                    <div v-for="(property, index) in properties" :key="index">
                        <p>{{ property.name }}</p>
                        <p>{{ property.xpath }}</p>
                    </div>
                </div>
                <div class="h-1/2">
                    <p>Data</p>
                    <div v-for="(data, index) in propertyDatas" :key="index">
                        <p>{{ data.property.name }}</p>
                        <p>{{ data.value }}</p>
                    </div>
                </div>
            </div>
            <div v-html="htmlContent" class="w-1/2" id="viewer"></div>
        </div>
    </div>
</template>

<script>
export default {
    data() {
        return {
            url: "https://index.minfin.com.ua/markets/fuel/reg/vinnickaya/",
            htmlContent: null,
            properties: [
                {
                    "name": "Title",
                    "xpath": "/html/body/div/div/div/div/div[2]/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[1]"
                },
                {
                    "name": "Price",
                    "xpath": "/html/body/div/div/div/div/div[2]/main/div/div/div[1]/div/div[1]/article/table/tbody/tr/td[2]/big"
                }
            ],
            propertyDatas: []
        }
    },
    updated() {
        this.updateData();
    },

    watch: {
        url: {
            async handler(newValue, oldValue) {
                let response = await fetch(newValue);
                this.htmlContent = await response.text();
            },
            immediate: true
        },
        properties: {
            handler: (newValue, oldValue) => this.updateData(),
            deep: true
        }
    },
    methods: {
        updateData() {
            this.propertyDatas = []

            for (const property of this.properties) {
                var elements = this.evaluateXPath(document, property.xpath);
                this.propertyDatas.push({
                    property,
                    value: elements.map(x => x.innerText)
                })
            }
        },

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
}
</script>