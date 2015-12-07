var oldPlatform = "Windows";
var windowState = null;
var initedPlatformSwitch = false;
var readied = false;

$(document).ready(function () {
    $("#switchProtobuildPlatform").change(function () {
        var platform = $("#switchProtobuildPlatform").val();
        if (platform !== null && platform !== undefined) {
            location.href = "app:///switch-platform?target=" + platform + "&old=" + oldPlatform + "&protobuild=true";
            oldPlatform = platform;
        }
    });
    $("#switchStandardPlatform").change(function () {
        var platform = $("#switchStandardPlatform").val();
        if (platform !== null && platform !== undefined) {
            location.href = "app:///switch-platform?target=" + platform + "&old=" + oldPlatform + "&protobuild=false";
            oldPlatform = platform;
        }
    });
    if (windowState != null) {
        stateChange(windowState);
    }
});

function stateChange(state) {
    if (state.disableStateUpdate) {
        return;
    }
    windowState = state;
    if (state.status !== undefined) {
        $("#status").text(state.status);
    }
    if (state.setplatform !== undefined && state.setplatform !== null) {
        $("#switchProtobuildPlatform").val(state.setplatform);
        oldPlatform = state.setplatform;
    }
    if (state.supportedPlatformCount != undefined) {
        var select = $("#switchStandardPlatform");
        var oldValue = select.val();
        select.children().remove();
        for (var i = 0; i < state.supportedPlatformCount; i++) {
            select.append($('<option>')
                .attr('value', state["supportedPlatform" + i])
                .text(state["supportedPlatform" + i]));
        }
        if (!initedPlatformSwitch) {
            if (state.supportedPlatformCount > 0) {
                oldPlatform = state.supportedPlatform0;
            }
            if (readied) {
                window.setTimeout(function() {
                    location.href = "app:///switch-platform?target=" + oldPlatform + "&old=" + oldPlatform;
                }, 100);
            }
        } else {
            select.val(oldValue);
        }
        initedPlatformSwitch = true;
    }
    if (state.isStandard) {
        $("#standardProject").show();
        $("#protobuildProject").hide();
    } else {
        $("#standardProject").hide();
        $("#protobuildProject").show();
    }

    if (state.loadedModuleName !== null) {
        $("[data-value=\"title\"]").text(state.loadedModuleName);
    }
}

$(document).bind("statechange", function (event, state) {
    stateChange(state);
});

$(document).ready(function () {
    if (initedPlatformSwitch) {
        location.href = "app:///switch-platform?target=" + oldPlatform + "&old=" + oldPlatform;
    }
    readied = true;
})