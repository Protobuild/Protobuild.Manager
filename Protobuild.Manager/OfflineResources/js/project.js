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
    if (state.busy !== undefined) {
        var elementsToDisable = [
            "#switchProtobuildPlatform",
            "#syncNow",
            "#regenerateProjects",
            "#createPackage",
            "#automatedBuild"
        ];
        if (state.busy) {
            $.each(elementsToDisable, function(idx, elem) {
                $(elem).addClass('disabled').attr('disabled', 'disabled');
            });
        } else {
            $.each(elementsToDisable, function (idx, elem) {
                $(elem).removeClass('disabled').removeAttr('disabled');
            });
        }
    }
    if (state.status !== undefined) {
        var classes = "fa fa-fw ";
        switch (state.statusMode) {
            case "Okay":
                classes += "green fa-check-circle";
                break;
            case "Error":
                classes += "red fa-times-circle";
                break;
            case "Processing":
                classes += "sky fa-spinner fa-spin";
                break;
        }

        $("#status").html($("<li></li>")
          .append($("<i></i>")
            .addClass(classes))
          .append(" " + state.status).html());
    }
    if (state.setplatform !== undefined && state.setplatform !== null) {
        $("#switchProtobuildPlatform").val(state.setplatform);
        oldPlatform = state.setplatform;
    }
    if (state.supportedPlatformCount != undefined && state.isStandard !== undefined) {
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
                    location.href = "app:///switch-platform?target=" + oldPlatform + "&old=" + oldPlatform + "&protobuild=" + (state.isStandard ? "false" : "true");
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

    if (oldPlatform !== null) {
        $("#syncNow").attr('href', 'app:///sync-projects?platform=' + oldPlatform);
        $("#regenerateProjects").attr('href', 'app:///generate-projects?platform=' + oldPlatform);
        $("#createPackage").attr('href', 'app:///create-package?platform=' + oldPlatform);
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
        location.href = "app:///switch-platform?target=" + oldPlatform + "&old=" + oldPlatform + "&protobuild=" + (windowState.isStandard ? "false" : "true");
    }
    readied = true;
})