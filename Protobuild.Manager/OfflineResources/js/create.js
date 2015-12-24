$(document).bind("statechange", function (event, state) {
    updateState(state);
});

function updateState(state) {
    $("#checks").html("");

    if (state.showCancel == true) {
        $("#toplinks").show();
        $("#header2").text("Failed to create project!");
    }

    for (var i = 0; i < state.stepCount; i++) {
        var text = state["stepName" + i];

        var message = state["stepMessage" + i];

        var classes = "fa fa-fw ";
        switch (state["stepState" + i]) {
            case "Passed":
                classes += "green fa-check-circle";
                break;
            case "Failed":
                classes += "red fa-times-circle";
                break;
            case "Processing":
                classes += "sky fa-spinner fa-spin";
                break;
            case "Pending":
                classes += "sky fa-circle-o";
                break;
            case "Warning":
                classes += "orange fa-exclamation-circle";
                break;
            case "Waiting":
                classes += "sky fa-dot-circle-o";
                break;
            case "Installing":
                classes += "yellow fa-chevron-circle-right";
                break;
        }

        var item = $("<li></li>")
          .append($("<i></i>")
            .addClass(classes))
          .append(text);

        if (message != null) {
            item.append($("<br/>"));
            item.append($("<i></i>")
              .addClass("fa fa-fw"));

            prefix = "";
            switch (state["stepState" + i]) {
                case "Failed":
                    prefix = "Error:";
                    break;
                case "Warning":
                    prefix = "Warning:";
                    break;
            }

            item.append($("<i></i>").append($("<strong></strong>").append(prefix)).append(" " + message));
        }

        $("#checks").append(item);
    }
}