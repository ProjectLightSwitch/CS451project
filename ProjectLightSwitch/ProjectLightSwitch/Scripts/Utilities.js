// http://stackoverflow.com/questions/4656843/jquery-get-querystring-from-url
function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function getUrlPath() {
    var parts = window.location.href.split('/');
    if (parts.length > 0) {
        return parts[parts.length - 1];
    }
    return null;
}

function getUrlHash() {
    var parts = window.location.href.split('#');
    if (parts.length > 1) {
        return parts[parts.length - 1];
    }
    return null;
}