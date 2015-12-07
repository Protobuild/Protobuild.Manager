var oldPlatform = "Windows";

$(document).ready(function () {
        $("#switchPlatform").change(function() {
            var platform = $("#switchPlatform").val();
            location.href = "app:///switch-platform?target=" + platform + "&old=" + oldPlatform;
            oldPlatform = platform;
        });
    }
);

function stateChange(state) {
    if (state.status !== undefined) {
        $("#status").text(state.status);
    }
    if (state.setplatform !== undefined && state.setplatform !== null) {
        $("#switchPlatform").val(state.setplatform);
        oldPlatform = state.setplatform;
    }

    $("[data-value=\"title\"]").text(state.loadedModuleName);
}

$(document).bind("statechange", function (event, state) {
    stateChange(state);
});

$(document).ready(function() {
    location.href = "app:///switch-platform?target=Windows&old=Windows";
    log("running");
})