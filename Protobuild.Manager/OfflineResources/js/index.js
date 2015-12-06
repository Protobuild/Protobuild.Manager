function stateChange(state) {
    console.log("state change");

    if (state.recentProjectsCount !== undefined) {
        // remove all children of the recent UL that aren't template
        // items.
        $("ul.recent").children(":not([data-template])").remove();

        if (state.recentProjectsCount == 0) {
            window.addTemplate("norecentitems", {});
        } else {
            for (var i = 0; i < state.recentProjectsCount; i++) {
                var tpl = window.addTemplate("recentitem", {
                    title: state["recentProjectTitle" + i],
                    path: state["recentProjectPath" + i],
                });
                $(tpl).find("a").attr("href", "app:///open-recent?path=" + encodeURIComponent(state["recentProjectPath" + i]));
            }
        }

        state.recentProjectsFilled = true;
    }
}

$(document).bind("statechange", function (event, state) {
    stateChange(state);
});