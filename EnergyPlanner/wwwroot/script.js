
function showPage(pageId) {
    document.getElementById("info").hidden = true;
    document.getElementById("calculation").hidden = true;

    document.getElementById(pageId).hidden = false;
}
