$(document).ready(function () {
    $("#projectFormat").change(function () { updateFormState(); });
    $("#libraryRefType").change(function () { updateFormState(); });

    updateFormState();
});

$(document).bind("statechange", function (event, state) {
    updateFormOptions(state);
});

function submitForm() {
    $("#form").submit();
}

function updateFormOptions(state) {
    if (state.templateProtobuildVariantsCount !== undefined) {
        var old = $("#projectFormat").val();
        $("#projectFormat").children(":not([data-template])").remove();

        for (var i = 0; i < state.templateProtobuildVariantsCount; i++) {
            tpl = window.addTemplate("variantitem", {});
            tpl.attr("value", "protobuild-" + state["templateProtobuildVariantsID" + i]).text(state["templateProtobuildVariantsName" + i]);
        }

        tpl = window.addTemplate("variantitem", {});
        tpl.attr("value", "standard").text("Standard C# Projects");

        for (var i = 0; i < state.templateStandardVariantsCount; i++) {
            tpl = window.addTemplate("variantitem", {});
            tpl.attr("value", "standard-" + state["templateStandardVariantsID" + i]).text(state["templateStandardVariantsName" + i]);
        }

        $("#projectFormat").val(old);
    }

    if (state.templateOptionalVariantsCount !== undefined) {
        $("#form").children(".optionalvariant:not([data-template])").remove();

        // Traverse in reverse order because we're inserting
        for (var i = state.templateOptionalVariantsCount - 1; i >= 0; i--) {
            tpl = window.addTemplate("optionalvariant", {}, true);
            tpl.children("label")
                .text(state["templateOptionalVariantsName" + i] + ":")
                .attr("for", state["templateOptionalVariantsID" + i]);
            tpl.select("label")
                .attr("name", state["templateOptionalVariantsID" + i])
                .attr("id", state["templateOptionalVariantsID" + i]);
        }
    }

    updateFormState();
}

function updateFormState() {
    if ($("#projectFormat").val().substr(0, 10) == "protobuild") {
        $("#platforms").hide();
        $("#platformsNoSelect").show();
    } else {
        $("#platforms").show();
        $("#platformsNoSelect").hide();
    }
}