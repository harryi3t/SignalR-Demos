addEventListener('DOMContentLoaded', function () {
    $("#ball-2").hide();
    $("#ball-3").hide();
    $("#red-ball").click(function () {
        if ($("#ball-2").is(":visible") == false)
            $("#ball-2").show();
        else if ($("#ball-3").is(":visible") == false)
            $("#ball-3").show();
        else {
            move('#ball-2').x(100).duration("2s").end();
            move('#ball-3').y(100).duration("2s").end();
        }
    });
    //move('#ball-1').x(600).end();
}, false);