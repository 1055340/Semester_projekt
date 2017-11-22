$(document).ready(function () {
    


    $('.categories').on('click', function () {
        var modalimgsrc = $(this).children('img').attr('src');
        var strengthContent = "<div class='row modal-append'><div class='modal-img col-xs-12'><img src=" + modalimgsrc + "></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input placeholder='KG' type='number' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-6'><input placeholder='Sets' type='number' class='form-control'></div><div class='col-xs-6'><input placeholder='Reps' type='number' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button class='btn btn-success btn-md'>Submit Exercise</button></div></div></div>";
        var cardioContent = "<div class='row modal-append'><div class='modal-img col-xs-12'><img src=" + modalimgsrc + "></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input placeholder='KM' type='number' class='form-control'></div></div><div class='col-xs-12'></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button class='btn btn-success btn-md'>Submit Exercise</button></div></div></div> ";
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

});