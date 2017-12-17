$(document).ready(function () {
    var currentxp = $('.progress-bar').data('currentxp');
    var nextxp = $('.progress-bar').data('nextlevel');
    var xpProgress = (currentxp / nextxp) * 100;
    $('.progress-bar').animate({
        width: (xpProgress + '%')
    }, 200);
});

