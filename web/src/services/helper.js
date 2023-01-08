export default {
    async waitForLoadAsync(contextWindow) {
        console.log("before");
        await Promise.all([new Promise((resolve, _) => { contextWindow.addEventListener("load", resolve); })]);
        console.log("after");
    },

    async waitStylesheetsAsync(contextDocument) {
        let linkElements = Array.from(contextDocument.querySelectorAll("link[rel=stylesheet]"));
        let onloadPromises = linkElements.map(element => {
            return new Promise((resolve, _) => { element.addEventListener("load", resolve); });
        });

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

