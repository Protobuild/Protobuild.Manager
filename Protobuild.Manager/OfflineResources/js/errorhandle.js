window.onerror = function (errormsg, url, linenumber) {
    if (window.external) {
        window.external.ReportError(errormsg, url, linenumber);
    } else {
        console.log(url + ":" + linenumber + ": " + errormsg);
    }
}

window.log = function (msg) {
    if (window.external) {
        window.external.Log(msg);
    } else {
        console.log(msg);
    }
}