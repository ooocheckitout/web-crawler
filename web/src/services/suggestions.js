import xpathService from "@/services/xpath";

export default {
    suggestAttribute(xpath) {
        // class, href, src

        return []
    },

    suggestText(xpath) {
        return []
    },

    suggestParent(xpath, contextDocument) {
        let elements = xpathService.evaluateXPath(contextDocument, xpath)
        if (elements.length !== 1) return []
        return elements.map(x => xpathService.getElementTreeXPath(x.parentNode));
    },

    suggestChildren(xpath, contextDocument) {
        let elements = xpathService.evaluateXPath(contextDocument, xpath)
        if (elements.length !== 1) return []
        let childElements = elements
            .filter(x => x.children.length > 0)
            .flatMap(x => Array.from(x.children));

        return childElements.map(x => xpathService.getElementTreeXPath(x));
    },

    suggestChangeIndex(xpath) {
        return []
    },

    suggestRemoveIndex(xpath) {
        const matchedXpathIndexes = [...xpath.matchAll(/\[.*?\]/g)];

        return matchedXpathIndexes.map(match => {
            let before = xpath.substring(0, match.index);
            let after = xpath.substring(match.index + match[0].length, xpath.length);
            return before + after;
        });
    },
}