window.addTemplate = function(name, values, insert) {
  console.log("requested to spawn template with name '" + name + "'");

  var original = $('[data-template="' + name + '"]');
  if (original.length == 0) {
    console.log("ERROR! Unable to find template with name '" + name + "'");
  }

  var copy = original.clone();
  
  for (var k in values){
      if (values.hasOwnProperty(k)) {
          copy.find('[data-template-value="' + k + '"]').each(function (idx, elem) {
              $(elem).text(values[k]);
            });
    }
  }

  copy.removeAttr("data-template");
    if (insert)
        copy.insertAfter(original);
    else
        original.parent().append(copy);

  console.log("template spawned and added to parent");

  return copy;
}

var consoleReady = false;
var consoleState = null;
var consoleLastId = 0;
var consoleTimeout = null;
var didOpen = false;
var didReallySetState = false;

$(document).ready(function () {
    $(document.body).append($(
        '<div id="console">' +
        '<div id="console_header">' +
        '<span id="long">0 messages (click to close)</span>' +
        '<span id="short"><i class="fa fa-check-circle" id="short_icon"></i><span id="short_text">&#160;0</span></span>' +
        '</div>' +
        '<div id="console_contents">' +
        '<ul id="logs">' +
        '</ul>' +
        '</div>' +
        '</div>'));
    $("#console_header").click(function() {
        var cls = $("#console").attr('class');
        if (cls == "open") {
            $("#console").removeAttr('class');
            if (consoleState !== undefined) {
                consoleState.rememberedState = "closed";
            }
            location.href = "app:///set-console-state?state=closed";
        } else {
            $("#console").attr('class', 'open');
            if (consoleState !== undefined) {
                consoleState.rememberedState = "open";
            }
            location.href = "app:///set-console-state?state=open";
            $("#console_contents").scrollTop(1000000);
        }
    });
    consoleReady = true;
    updateConsole();
});

$(document).bind("statechange", function (event, state) {
    consoleState = state;
    didReallySetState = true;
    if (consoleReady) {
        updateConsole();
    }
});

function updateConsole() {
    if (!didOpen && didReallySetState) {
        if (consoleState.rememberedState === "open") {
            $("#console").attr('class', 'open');
        }
        didOpen = true;
    }
    if (consoleState === undefined || consoleState === null) {
        consoleState = {};
    }
    if (consoleState.processLogLineCount === undefined || consoleState.processLogLineCount === null) {
        consoleState.processLogLineCount = 0;
    }
    var oldId = consoleLastId;
    for (var i = consoleLastId; i < consoleState.processLogLineCount; i++) {
        if (consoleState["processLogLine" + i + "Color"] === undefined) {
            $("#logs").append($("<li></li>").text("missing colour for id " + i + ", text was " + consoleState["processLogLine" + i + "Text"]));
        } else if (consoleState["processLogLine" + i + "Color"] === undefined) {
            $("#logs").append($("<li></li>").text("missing text for id " + i));
        } else {
            $("#logs").append($("<li></li>").attr('style', 'color: ' + consoleState["processLogLine" + i + "Color"] + ';').text(consoleState["processLogLine" + i + "Text"]));
        }
    }
    consoleLastId = consoleState.processLogLineCount;
    $("#long").text(consoleLastId + " messages (click to close)");
    $("#short_text").html('&#160;' + consoleLastId);
    $("#console_contents").scrollTop(1000000);

    if (oldId < consoleLastId) {
        $("#short_icon").removeClass("fa-check-circle");
        $("#short_icon").addClass("fa-spinner");
        $("#short_icon").addClass("fa-spin");
        if (consoleTimeout != null) {
            window.clearTimeout(consoleTimeout);
        }
        consoleTimeout = window.setTimeout(function () {
            $("#short_icon").addClass("fa-check-circle");
            $("#short_icon").removeClass("fa-spinner");
            $("#short_icon").removeClass("fa-spin");
        }, 1000);
    }

    
}