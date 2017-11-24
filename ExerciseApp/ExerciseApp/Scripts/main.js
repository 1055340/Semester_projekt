$(window).ready( function () {
    $('.categories').on('click', function () {
        var id = $(this).data('id');
        var DOMmultiplier = $(this).data('value') * 100;
        var modalimgsrc = $(this).children('img').attr('src');
        if (isNaN(DOMmultiplier)) {
            var step1 = $(this).data('value').replace(',', '.');
            var step2 = step1 * 100;
            var DOMmultiplier = Math.floor(step2);
        }  
        var strengthContent = "<form action='/Manage/Create' method='post' class='form-horizontal'><input name='ExerciseId' type='hidden' value=" + id + " ><input class='multi' step='0.01' name='ExerciseMultiplier' type='hidden' value=" + DOMmultiplier + " ><div class='row modal-append'><div class='modal-img col-xs-12'><img src=" + modalimgsrc + "></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input name='ExerciseValue1' required placeholder='Kilograms' type='number' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-6'><input name='ExerciseValue2' required placeholder='Sets' type='number' class='form-control'></div><div class='col-xs-6'><input name='ExerciseValue3' required placeholder='Repetitions' type='number' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button type='submit' class='btn btn-success btn-md'>Submit Exercise</button></div></div></div></form>";
        var cardioContent = "<form action='/Manage/Create' method='post' class='form-horizontal'><input name='ExerciseId' type='hidden' value=" + id + " ><input class='multi' step='0.01' name='ExerciseMultiplier' type='hidden' value=" + DOMmultiplier + " ><div class='row modal-append'><div class='modal-img col-xs-12'><img src=" + modalimgsrc + "></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input name='ExerciseValue1' required placeholder='Kilometres' type='number' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-6'><input name='ExerciseValue2' required type='hidden' value='1' class='form-control'></div><div class='col-xs-6'><input name='ExerciseValue3' required type='hidden' value='1' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button type='submit' class='btn btn-success btn-md'>Submit Exercise</button></div></div></div></form>";
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