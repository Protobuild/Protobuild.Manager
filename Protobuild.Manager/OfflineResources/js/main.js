var showMain = function (force) {
  if (force || (!$("#container").is(":visible") && !$("#options_container").is(":visible"))) {
    $("#container").show();
    $("#options_container").hide();
    $("#prereq_container").hide();
    $("#save_and_return_toggle").hide();
  }
  $("#working_container").hide();
  $("#progress_outer").hide();
};

var showOptions = function(state) {
  $("#container").hide();
  $("#options_container").show();
  $("#prereq_container").hide();
  $("#save_and_return_toggle").show();
  $("#working_container").hide();
  $("#progress_outer").hide();
};

var showLaunching = function(state) {
  $("#container").hide();
  $("#prereq_container").hide();
  $("#working_container").show();
};

var setupLoginForm = function(state) {
  var cached = false;

  if (state.cachedPassword) {
    cached = true;
    $("#cached").val("true");
    $("#username").val(state.username);
    $("#password").val('**********');
    $("#password").attr('disabled', 'disabled');
  }

  $("#username").keydown(function(event) {
    if (!cached) {
      return;
    }

    if (event.keyCode == 13) { return; }

    $("#password").val('');
    $("#password").removeAttr('disabled');
    $("#cached").val("false");

    cached = false;

    location.href = 'app:///clearcache';
  });
}

var setupFullCrashDumps = function(state) {
  if (state.canFullCrashDump) {
    if (state.fullCrashDumpsEnabled) {
      $("#full_crash_dumps_enabled").show();
    } else {
      $("#enable_full_crash_dumps").show();
    }

    $("#enable_full_crash_dumps").click(function(event) {
      $("#full_crash_dumps_enabled").show();
      $("#enable_full_crash_dumps").hide();

      location.href = 'app:///enablefullcrashdumps';
    });
  }
}

var setup = function(state) {
  $("#loading_please_wait").hide();
  $("#advanced_options_toggle").show();
  $("#login").show();
  $("#register").show();
  $("#welcome_status").show();

  setupLoginForm(state);

  $("#username").focus();
  $("#login").attr('action', 'app:///login');
  $("#register").attr('action', 'app:///register');
  $("#options_form").attr('action', 'app:///option');

  setupFullCrashDumps(state);

  $("#active_channel").change(function(event) {
    location.href =
      'app:///channel?name=' +
      encodeURIComponent($("#active_channel").val());
  });

  $("#advanced_options_toggle").click(function(event) {
    showOptions();
  });

  $("#save_and_return_toggle").click(function(event) {
    showMain(true);

    var opts = $("#options_form input");
    var results = {};
    for (var a = 0; a < opts.length; a++) {
        var opt2 = opts[a];
        results[opt2.name.substring(7)] = opt2.checked != '';
    }
    location.href = 'app:///option?state=' + encodeURIComponent(JSON.stringify(results));
  });
};

var updateSnapshots = function(state) {

  var availableChannels = [];
  for (i = 0; i < state.availableChannelCount; i++) {
    availableChannels.push(state["availableChannel" + i]);
  }

  var existingOptions = [];
  $("#active_channel option").each(function(k, o) {
    if ($.inArray(o.value, availableChannels) === -1) {
      $(o).remove();
    } else {
      existingOptions.push(o.value);
    }
  });

  $.each(availableChannels, function(index, value) {
    if ($.inArray(value, existingOptions) === -1) {
      var option = $("<option></option")
        .attr("value", value)
        .text(value);
      $("#active_channel").append(option);
    }
  });

}

var updateOptions = function(state) {

  var currentOptions = JSON.parse(state.currentOptions);

  $("#options_form input").each(function(k, a) {
    if (currentOptions[a.name.substring(7)]) {
      $(a).attr('checked', 'checked');
    } else {
      $(a).removeAttr('checked');
    }
  });

};

var updateNews = function(state) {

  if (state.newsCount > 0) {
    $("#news").html("");
  }

  var news = [];
  for (i = 0; i < state.newsCount; i++) {
    var item = {
      title: state["newsTitle" + i],
      author: state["newsAuthor" + i],
      content: state["newsContent" + i],
      date: state["newsDate" + i],
    };

    var div = $("<div class=\"block phame\"></div>");
    div.append(
      $("<div class=\"details\"></div>")
        .append($("<div class\"author\"></div>").text(item.author))
        .append($("<div class\"date\"></div>").text(item.date)));
    div.append($("<h3></h3>").text(item.title));
    div.append(item.content);
    $("#news").append(div);
  }

}

var updatePrereq = function (state) {
  $("#container").hide();
  $("#prereq_container").show();
  $("#options_container").hide();

  $("#checks").html("");

  for (var i = 0; i < state.prereqCount; i++) {
    var text = state["prereqName" + i];
    if (text == null) {
      text = "Unknown Prerequisite";
    }

    var message = state["prereqMessage" + i];

    var classes = "fa fa-fw ";
    switch (state["prereqStatus" + i]) {
      case "Passed":
        classes += "green fa-check-circle";
        break;
      case "Failed":
        classes += "red fa-times-circle";
        break;
      case "Checking":
        classes += "sky fa-chevron-circle-right";
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
      switch (state["prereqStatus" + i]) {
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

var firstRun = true;

$(document).bind("statechange", function(event, state) {

  if (state.view == "prereq") {
    updatePrereq(state);
    return;
  }

  if (state.offline) {
    if (state.offlineplay) {
      $("#news").html("<p>Welcome <strong>" + state.username + "</strong></p>" +
        "<p>You are currently offline, so we can't update the game, but you can still play it.</p>");
    } else {
      $("#news").html("<p>Sorry, it appears you are offline and you haven't logged in before on this computer.</p>" +
        "<p>You need to go online and login at least once so the game files can be downloaded.</p>");

      $("#play").attr('disabled', 'disabled');
    }

    $("#username").attr('disabled', 'disabled');
    $("#password").attr('disabled', 'disabled');
    $("#register_go").attr('disabled', 'disabled');
    $("#active_channel").attr('disabled', 'disabled');
  }

  if (state.view === "launching") {
    if (state.progressAmount !== undefined && state.progressAmount !== null) {
      $("#progress_outer").show();
      $("#progress_inner").css({width: (state.progressAmount * 100) + "%"});
      $("#progress_eta").text(state.progressEta);
    } else {
      $("#progress_outer").hide();
    }

    if (state.welcome !== undefined && state.welcome !== null) {
      $("#welcome_status").text(state.welcome);
    }

    if (state.working !== undefined && state.working !== null) {
      $("#working_status").html(state.working);
    }

    showLaunching(state);
    return;
  } else {
    if (state.error !== undefined && state.error !== null) {
      $("#error_status").text(state.error);
    }

    showMain();
  }

  if (firstRun) {
    setup(state);
  }

  updateSnapshots(state);

  updateOptions(state);

  if (!state.offline) {
    updateNews(state);
  }

  firstRun = false;

});