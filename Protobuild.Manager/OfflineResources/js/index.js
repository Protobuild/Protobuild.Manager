var windowState = {};

function stateChange() {
    $('h1[data-value="title"]').text(windowState.productName);

    if (windowState.recentProjectsCount !== undefined) {
        // remove all children of the recent UL that aren't template
        // items.
        $("ul.recent").children(":not([data-template])").remove();

        if (windowState.recentProjectsCount == 0) {
            window.addTemplate("norecentitems", {});
        } else {
            for (var i = 0; i < windowState.recentProjectsCount; i++) {
                var tpl = window.addTemplate("recentitem", {
                    title: windowState["recentProjectTitle" + i],
                    path: windowState["recentProjectPath" + i],
                });
                $(tpl).find("a").attr("href", "app:///open-recent?path=" + encodeURIComponent(windowState["recentProjectPath" + i]));
            }
        }

        windowState.recentProjectsFilled = true;
    }
}

$(document).bind("statechange", function (event, state) {
    windowState = state;
    stateChange();
});

$(document).ready(function (event) {
    stateChange();
});