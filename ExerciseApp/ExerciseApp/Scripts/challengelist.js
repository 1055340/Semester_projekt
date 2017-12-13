$(document).ready(function () {
    $("<a href='/manage/ChallengeFriendList' class='challenge-add'>+</a>").hide().appendTo('.navbar-default').fadeIn(400);


    $('.challenger').each(function (i, obj) {
        if ($(this).data('accept') === 1) {
            console.log("this one is");
            var loc = $(this).children('.acc-btn');
            $(this).children('.acc-btn').empty();
            $('<a class="accept btn btn-success">&#128077;</a><a class="deny btn btn-danger">&#128078;</a>').hide().appendTo(loc).fadeIn(1000);
        } else {
            return;
        }

    });

    $('.acc-btn > a').click(function () {
        $('#challengeModal .modal-body').empty();
        var id = $(this).parent().parent().data('id');
        ;
        $.ajax({
            url: '/Manage/GetChallengeDetails',
            type: 'Post',
            datatype: 'json',
            data: {
                order: 'Client_Call',
                id: id
            },
            success: function (data) {
                var myJSON = JSON.stringify(data);
                var obj = eval('(' + myJSON + ')')[0];
                var ChallengerPercentageToWin = (obj.ChallengerValue / obj.ChallengeGoal) * 100;
                ChallengerPercentageToWin = Math.floor(ChallengerPercentageToWin);
                var ChallengedPercentageToWin = (obj.ChallengedValue / obj.ChallengeGoal) * 100;
                ChallengedPercentageToWin = Math.floor(ChallengedPercentageToWin);
                var date = new Date(parseInt(obj.ChallengeEnd.substr(6)));

                var challengeContent = "<div class='row'><div class='container-fluid' data-id=" + obj.ChallengerId + "><div class='challenge-name-container col-xs-12' ><span class='col-xs-10'>" + obj.ChallengerName + "</span></div><div class='col-xs-12 rival'><div class='progress-container col-xs-10'><div class='progress'><div class='progress-bar' role='progressbar' aria-valuenow=" + ChallengerPercentageToWin + " aria-valuemin='0' aria-valuemax='100' style='width:" + ChallengerPercentageToWin + "%;'>" + ChallengerPercentageToWin + "%</div></div></div><div class='col-xs-2'><img src='/images/challenge-goal.png'/></div></div></div><div class='container-fluid' data-id=" + obj.ChallengedId + "><div class='challenge-name-container col-xs-12'><span class='col-xs-10' >" + obj.ChallengedName + "</span></div><div class='col-xs-12 rival'><div class='progress-container col-xs-10'><div class='progress'><div class='progress-bar' role='progressbar' aria-valuenow=" + ChallengedPercentageToWin + " aria-valuemin='0' aria-valuemax='100' style='width:" + ChallengedPercentageToWin + "%;'>" + ChallengedPercentageToWin + "%</div></div></div><div class='col-xs-2'><img src='/images/challenge-goal.png'/></div></div></div></div><div class='row'><div class='col-xs-12'><h2>" + obj.ChallengeTitle + "</h2></div><div class='col-xs-12'><h3>" + obj.ExerciseName + "</h3></div></div><div class='row'><div class='col-xs-12'><h3>goal: " + obj.ChallengeGoal + "</h3></div></div><div class='row'><div class='col-xs-12'><h3>countdown</h3><h3 id='getting-started'></h3></div></div><div class='btn-container col-xs-12'><btn class='btn btn-danger' data-dismiss='modal'>close</btn></div>";

                $(challengeContent).hide().appendTo('#challengeModal .modal-body').fadeIn(400);

                //Hvis en af personerne har 0% progress, bliver baren lidt længere, så man kan se teksten i baren

                $('.progress-bar').each(function () {
                    if ($(this).text() === "0%") {
                        $(this).width(10 + "%");
                    }
                    var currentprogress = $(this).attr('aria-valuenow');
                    if (currentprogress >= 100) {
                        var winner = 0;
                        $(this).attr('aria-valuenow', '100');
                        $(this).width('100%');
                        $(this).text('100%');
                        var winner = $(this).parent().parent().parent().parent().data('id');
                        $('<p>You Won!</p><btn class="btn btn-success">Claim Reward</btn>').hide().prependTo('.btn-container').fadeIn(400);

                        //Der er her blevet fundet en vinder af challengen, som nu laver et ajax kald for at kunne claime sit reward, og flytte challengen til afsluttede
                        $.ajax({
                            url: '/Manage/?',
                            type: 'Post',
                            datatype: 'json',
                            data: {
                                order: 'Client_Call',
                                id: winner
                            },
                            success: function (data) {
                                console.log("returned successful");
                            },
                            error: function (data) {
                                console.log("returned errorful");
                            }

                        });



                    }
                    
                });

                $("#getting-started")
                    .countdown(date, function (event) {
                        $(this).text(
                            event.strftime('%D days %H:%M:%S')
                        );
                    })

            },
            error: function (data) {
                console.log("error!");
                console.log(data);
            },
        });

    });

    $('.accept').click(function () {
        var loc = $(this).parent();
        var id = $(this).parent().parent().data('id');
        $(this).parent().empty();
        $('<a class="btn btn-success" data-toggle="modal" data-target="#challengeModal">View</a>').hide().appendTo(loc).fadeIn(1000);

        $.ajax({
            url: '/Manage/AcceptChallenge',
            type: 'Post',
            datatype: 'json',
            data: {
                order: 'Client_Call',
                id: id
            },
            success: function (data) {
                console.log("success")
                console.log(data);
            },
            error: function (data) {
                console.log("error!");
                console.log(data);
            },
        });
    });


    $('.deny').click(function () {
        console.log("wowo");
        var id = $(this).parent().parent().data('id');

        $(this).parent().parent().addClass('animated bounceOut');
        $(this).parent().parent().delay(500).fadeOut(300, function () {
            $(this).remove();
        });
        $.ajax({
            url: '/Manage/DeleteChallenge',
            type: 'Post',
            datatype: 'json',
            data: {
                order: 'Client_Call',
                id: id
            },
            success: function (data) {
                console.log("success")
                console.log(data);
            },
            error: function (data) {
                console.log("error!");
                console.log(data);
            },
        });
    });
});
