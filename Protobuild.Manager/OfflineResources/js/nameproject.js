var pathModifiedByUser = false;
var defaultPath = null;
var initializedAdditionalPlatforms = false;

$(document).ready(function () {
    $("#projectFormat").change(function () { updateFormState(); });
    $("#libraryRefType").change(function () { updateFormState(); });

    updateFormState();
    updatePath();

    $("#path").keydown(function () {
        pathModifiedByUser = true;
    });

    $("#name").keyup(function () {
        updatePath();
    });
});

$(document).bind("statechange", function (event, state) {
    defaultPath = state.defaultpath;
    updatePath();

    updateFormOptions(state);
});

function updatePath() {
    if (!pathModifiedByUser) {
        var path = $("#path");
        path.val(defaultPath + $("#name").val());
    }
}

function submitForm() {
    $("#form").submit();
}

var variantOptions = {};

function updateFormOptions(state) {
    if (!initializedAdditionalPlatforms) {
        if (state.additionalPlatformsCount !== undefined && state.additionalPlatformsCount !== null) {
            for (var i = 0; i < state.additionalPlatformsCount; i++) {
                var pl = addTemplate("additionalplatform", { name: state["additionalPlatforms" + i] });
                $("input", pl).attr("name", "platform_" + state["additionalPlatforms" + i]);
            }
            initializedAdditionalPlatforms = true;
        }
    }

    if (state.selectedProjectDir !== undefined && state.selectedProjectDir !== null) {
        $("#path").val(state.selectedProjectDir);
        pathModifiedByUser = true;
    }

    if (state.prefilledprojectname !== undefined && state.prefilleddestinationdirectory !== undefined) {
        $("#name").val(state.prefilledprojectname).attr('readonly', 'readonly');
        $("#path").val(state.prefilleddestinationdirectory).attr('readonly', 'readonly');
        $("#selectPath").attr('disabled', 'disabled');
    }

    if (state.templateProtobuildVariantsCount !== undefined) {
        var old = $("#projectFormat").val();
        $("#projectFormat").children().remove();

        var makeOption = function(k, v) {
            $("#projectFormat").append($("<option></option>").attr('value', k).text(v));
        }

        makeOption("protobuild", "Protobuild");

        for (var i = 0; i < state.templateProtobuildVariantsCount; i++) {
            makeOption("protobuild-" + state["templateProtobuildVariantsID" + i], state["templateProtobuildVariantsName" + i]);
        }

        makeOption("standard", "Standard C# Projects");

        for (var i = 0; i < state.templateStandardVariantsCount; i++) {
            makeOption("standard-" + state["templateStandardVariantsID" + i], state["templateStandardVariantsName" + i]);
        }

        if (old === null) {
            $("#projectFormat").val("protobuild");
        } else {
            $("#projectFormat").val(old);
        }
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

            var id = state["templateOptionalVariantsID" + i];
            var select = tpl.find("select");
            select
                .attr("id", id)
                .attr("name", id);
            variantOptions[id] = {
                'protobuild': [],
                'standard': []
            };
            for (var a = 0; a < state["templateOptionalVariantsProtobuildOptionCount" + i]; a++) {
                variantOptions[id].protobuild.push({
                    'id': state["templateOptionalVariantsProtobuildOption" + i + "ID" + a],
                    'name': state["templateOptionalVariantsProtobuildOption" + i + "Name" + a]
                });
            }
            for (var a = 0; a < state["templateOptionalVariantsStandardOptionCount" + i]; a++) {
                variantOptions[id].standard.push({
                    'id': state["templateOptionalVariantsStandardOption" + i + "ID" + a],
                    'name': state["templateOptionalVariantsStandardOption" + i + "Name" + a]
                });
            }
        }
    }

    updateFormState();
}

function updateFormState() {
    if ($("#projectFormat").val() === null) {
        return;
    }
    if ($("#projectFormat").val().substr(0, 10) == "protobuild") {
        $("#platforms").hide();
        $("#platformsNoSelect").show();
        updateOptionalVariants('protobuild');
    } else {
        $("#platforms").show();
        $("#platformsNoSelect").hide();
        updateOptionalVariants('standard');
    }
}

function updateOptionalVariants(type) {
    for (var id in variantOptions) {
        if (variantOptions.hasOwnProperty(id)) {
            var select = $("select#" + id);
            select.children().remove();
            $.each(variantOptions[id][type], function (key, value) {
                select.append($('<option>').attr('value', value.id).text(value.name));
            });
            if (select.children().length == 0) {
                select.parent().parent().hide();
            } else {
                select.parent().parent().show();
            }
        }
    }
}