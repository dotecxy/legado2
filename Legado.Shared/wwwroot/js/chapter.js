window.updateChapterStyle = function(selector, backgroundColor) {
    var elements = document.querySelectorAll(selector);
    if (elements.length > 0) {
        for (var i = 0; i < elements.length; i++) {
            elements[i].style.backgroundColor = backgroundColor;
        }
    }
};
