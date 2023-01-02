export default {
    highlight(elements, highlightClass) {
        if (!elements) return;

        if (!Array.isArray(elements)) elements = [elements];

        for (const element of elements) {
            if (element.classList?.contains(highlightClass)) continue;

            element.classList.add(highlightClass);
        }
    },

    unhighlight(elements, highlightClass) {
        if (!elements) return;

        if (!Array.isArray(elements)) elements = [elements];

        for (const element of elements) {
            if (!element.classList?.contains(highlightClass)) continue;

            element.classList.remove(highlightClass);
        }
    },
}