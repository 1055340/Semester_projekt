$(document).ready(function () {
    var challengedName = Cookies.get('challengedName');
    var challengedId = Cookies.get('challengedId');
    

$('.categories').on('click', function () {
    var id = $(this).data('id');
    var thiscategory = $(this).children('p').text();
    var DOMmultiplier = $(this).data('value') * 100;
    var modalimgsrc = $(this).children('img').attr('src');
    if (isNaN(DOMmultiplier)) {
        var step1 = $(this).data('value').replace(',', '.');
        var step2 = step1 * 100;
        var DOMmultiplier = Math.floor(step2);
    }
    var strengthContent = "<form action= '/Manage/CreateChallenge' method= 'post' class='form-horizontal' > <input name='ExerciseId' type='hidden' value=" + id + " ><input class='Challenged' name='ChallengedId' type='hidden' value=" + challengedId + "><div class='row modal-append'><div class='challenge-img col-xs-12'><h2>" + thiscategory + "</h2></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input name='ChallengeTitle' required placeholder='Title' type='text' class='form-control'><input name='ChallengeGoal' required placeholder='Goal in total KG' type='text' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'></div><div class='col-xs-6 col-xs-offset-3'><h3>End date</h3><input name='ChallengeEnd' required placeholder='End Date' type='date' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button type='submit' class='btn btn-success btn-md'>Challenge " + challengedName + "!</button><button type='cancel' class='btn btn-danger btn-md' data-dismiss='modal'>Cancel</button></div></div></div></form>";
    var cardioContent = "<form action= '/Manage/CreateChallenge' method= 'post' class='form-horizontal' > <input name='ExerciseId' type='hidden' value=" + id + " ><input class='Challenged' name='ChallengedId' type='hidden' value=" + challengedId + "><div class='row modal-append'><div class='challenge-img col-xs-12'><h2>" + thiscategory + "</h2></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'><input name='ChallengeTitle' required placeholder='Title' type='text' class='form-control'><input name='ChallengeGoal' required placeholder='Goal in total KM' type='text' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-6 col-xs-offset-3'></div><div class='col-xs-6 col-xs-offset-3'><h3>End date</h3><input name='ChallengeEnd' required placeholder='End Date' type='date' class='form-control'></div></div><div class='col-xs-12'><div class='col-xs-8 col-xs-offset-2'><button type='submit' class='btn btn-success btn-md'>Challenge " + challengedName + "!</button><button type='cancel' class='btn btn-danger btn-md' data-dismiss='modal'>Cancel</button></div></div></div></form>";
   
    isreturn_str = $(this).data('return');

    if (isreturn_str === "True") {
        $('#categoryModal .modal-body').empty();
        $('#categoryModal .modal-title').empty();
        $('#categoryModal .modal-body').append(cardioContent);
    } else {
        $('#categoryModal .modal-body').empty();
        $('#categoryModal .modal-title').empty();
        $('#categoryModal .modal-body').append(strengthContent);
    }
    });
});