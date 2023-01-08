export default {
    async waitEvent(context, eventName) {
        await (new Promise((resolve, _) => context.addEventListener(eventName, resolve)))
    },

    async waitAction(booleanAction, delayMs = 1000) {
        const triggerMeAgainIfNeeded = function (resolve) {
            setTimeout(function () { booleanAction() ? resolve() : triggerMeAgainIfNeeded(); }, delayMs);
        }


        await (new Promise((resolve, _) => triggerMeAgainIfNeeded(resolve)))
    },

    async waitStylesheetsAsync(contextDocument) {
        let linkElements = Array.from(contextDocument.querySelectorAll("link[rel=stylesheet]"));
        let onloadPromises = linkElements.map(element => this.waitEvent(element, "load"));

        await Promise.all(onloadPromises);

        console.log("finished waiting for stylesheets");
    },

    removeHideElements(contextDocument) {
        let externalStylesheets = Array.from(contextDocument.styleSheets).filter(x => x.href);
        let rulesThatHideElement = externalStylesheets
            .flatMap(x => Array.from(x.cssRules))
            .filter(x => x.styleMap?.get("display") == "none");

        rulesThatHideElement.forEach(x => {
            let ruleIndex = Array.from(x.parentStyleSheet.cssRules).indexOf(x);
            x.parentStyleSheet.deleteRule(ruleIndex);
        });

        console.log(`removed ${rulesThatHideElement.length} css rules that hide elements`);
    }
}

