$(document).ready(function() {

});

function stateChange(state) {

}

$(document).bind("statechange", function (event, state) {
    stateChange(state);
});

function submitForm() {
    $("#form").submit();
}