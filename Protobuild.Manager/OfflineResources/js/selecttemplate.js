function stateChange(state) {
    console.log("state change");

    if (state.templateCount !== undefined) {
        // remove all children of the recent UL that aren't template
        // items.
        $("ul.recent").children(":not([data-template])").remove();

        if (state.templateCount == 0) {
        } else {
            for (var i = 0; i < state.templateCount; i++) {
                var tpl = window.addTemplate("templateitem", {
                    title: state["templateName" + i],
                    description: state["templateDescription" + i],
                });
                $(tpl).find("a").attr("href", "app:///select-template?url=" + encodeURIComponent(state["templateURI" + i]));
            }
        }
    }
}

$(document).bind("statechange", function (event, state) {
    stateChange(state);
});