$(document).ajaxStart(function () {
    $("#loading").show();
});
$(document).ajaxStop(function () {
    $("#loading").hide();
});
$(document).ajaxError(function (e, xhr, settings, exception) {
    //Notify("Ajax error", xhr.statusText);
});

function Popup(data, width) {
    //
    var left = (getScrollerWidth() / 2) - (width / 2);
    $.blockUI({ message: '<div>'
                          + '<div style="float:right;"><a href="#" OnClick="javascript:$.unblockUI();return false;" class="Cancel" title="Close Popup" ></a></div>'
                          + '<br/>' + data
                          + '</div>',
        fadeIn: 300,
        fadeOut: 300,

        css: {
            top: "30px",
            left: left + "px",
            background: "#fff",
            width: width + "px",
            padding: "10px",
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: '0.9'
        }
    });
}
function getScrollerWidth() {
    if (window.innerHeight || window.innerWidth)
        return window.innerWidth
    else
        return document.documentElement.clientWidth;
}

