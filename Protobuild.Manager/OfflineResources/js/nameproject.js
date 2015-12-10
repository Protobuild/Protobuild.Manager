var pathModifiedByUser = false;
var defaultPath = null;

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
    if (state.prefilledprojectname !== undefined && state.prefilleddestinationdirectory !== undefined) {
        $("#name").val(state.prefilledprojectname).attr('readonly', 'readonly');
        $("#path").val(state.prefilleddestinationdirectory).attr('readonly', 'readonly');
        $("#selectPath").attr('disabled', 'disabled');
    }

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
        }
    }
}