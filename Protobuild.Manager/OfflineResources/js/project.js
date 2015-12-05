$(document).ready(function() {
        $("#switchPlatform").change(function() {
            var platform = $("#switchPlatform").val();
            location.href = "app:///switch-platform?target=" + platform;
        });
    }
);