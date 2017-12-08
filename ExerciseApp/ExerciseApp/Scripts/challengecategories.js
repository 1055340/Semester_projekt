$('.categories').on('click', function () {
    var id = $(this).data('id');
    var DOMmultiplier = $(this).data('value') * 100;
    var modalimgsrc = $(this).children('img').attr('src');
    if (isNaN(DOMmultiplier)) {
        var step1 = $(this).data('value').replace(',', '.');
        var step2 = step1 * 100;
        var DOMmultiplier = Math.floor(step2);
    }
    var strengthContent = "<h2>Strength</h2>";
    var cardioContent = "<h2>Cardio</h2>";
    isreturn_str = $(this).data('return');

    if (isreturn_str === "True") {
        $('#categoryModal .modal-body').empty();
        $('#categoryModal .modal-title').empty();
        $('#categoryModal .modal-title').append("Cardio");
        $('#categoryModal .modal-body').append(cardioContent);
    } else {
        $('#categoryModal .modal-body').empty();
        $('#categoryModal .modal-title').empty();
        $('#categoryModal .modal-title').append("Strength");
        $('#categoryModal .modal-body').append(strengthContent);
    }
});