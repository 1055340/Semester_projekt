$(document).ready(function () {
   


    //Ajax kald som tjekker om brugeren har fået noget xp som de ikke har fået en popup for endnu
    $.ajax({
        url: '/Manage/GetXpPopup',
        type: 'Post',
        datatype: 'json',
        data: { order: 'Client_Call' },
        success: function (data) {

            //Hvis xp-gained kaldet returnerer et tomt array ("[]"), altså er længden på data = 2, og derfor skal achievement-popuppen vises selvstændigt
            if (data.length === 2) {
                $.ajax({
                    url: '/Manage/GetUserAchievements',
                    type: 'Post',
                    datatype: 'json',
                    data: { order: 'Client_Call' },
                    success: function (data) {
                        if (data.length === 2) {
                            return;
                        } else {

                        
                        var JsonArray = JSON.parse(data);
                        var i = 0, l = JsonArray.length;

                        var obj = JsonArray[i];
                        $('.xp-popup').addClass('popup-fadein');
                        (function iterator() {
                            var achievementimg = '<img style="width:125px;height:125px;" src = "/images/ach-' + JsonArray[i].AchievementId + '.png" />';
                            $('.achievement-popup').empty().css("height","100vh");
                            //$(achievementimg).hide().appendTo('.achievement-popup').fadeIn(1000).addClass('animated tada');
                            $('<div style="position: relative;top:50%;transform:translateY(-50%);">' + achievementimg + '<h3 style="color:#fff">Achievement unlocked!</h3><h4 style="color:#fff">' + JsonArray[i].AchievementName + '</h4></div>').hide().appendTo('.achievement-popup').fadeIn(1000);
                            if (++i < l) {
                                setTimeout(iterator, 4000);
                            } else {
                                $('.achievement-popup').delay(4000).fadeOut(1000);
                                setTimeout(function () {
                                    $('.xp-popup').removeClass('popup-fadein');
                                }, 4000);

                            }
                        })();
                        }
                    },
                    error: function (data) {
                        console.log("error");
                    },
                });


            }
            //Er data = 2 ikke sandt, har brugeren fået xp, og derfor skal xp-popuppen vises i stedet (achievement-popuppen kommer senere i dette kald)
            else {
                var JsonArray = JSON.parse(data);
                for (var i = 0; i < JsonArray.length; i++) {
                    var obj = JsonArray[i];
                    //Popup'ens indhold bygges
                    $('.xp-popup').addClass('popup-fadein');

                    var xpProgress = (obj.XpForCurrentLevelEquals / obj.XpForNextLevelEquals) * 100;
                    var ImgBuilder = "<img src='/images/cat-" + obj.ExerciseId + ".png' />";
                    var xpGained = "Rewarding " + obj.ExerciseScore + " XP";
                    var currentXp = obj.XpForCurrentLevelEquals;
                    var targetXp = obj.XpForNextLevelEquals;
                    $('.xp-popup').append("<div class='row'><div class='col-xs-12 popup-img'>" + ImgBuilder + "</div><div style='display:flex' class='level-container col-xs-12'><div class='current-level col-xs-1'><span> " + obj.OldCurrentLevel + "</span><p style='font-size:.6em;'>" + currentXp + "</p></div><div class='xp-bar col-xs-10'><div style='text-align:center'></div></div><div class='next-level col-xs-1 pull-right'><span> " + obj.OldNextLevel + "</span><p style='font-size:.6em;'>" + targetXp + "</p></div></div></div><div style='text-align:center; color:#fff;' class='col-xs-12'><p class='xp-reward'> " + xpGained + "</p></div>");
                    $('.xp-popup > div > .level-container > .xp-bar > div').width(xpProgress + 2 + '%');

                    //xp baren animeres i et for loop, delayet med 2ms
                    var xpGained = 0 + obj.XpForCurrentLevelEquals;
                    setTimeout(function () {
                        (function theLoop(i) {
                            setTimeout(function () {
                                //hvis xp'en man har er lig ex'en levelet kræver, har man fået et nyt level og "ding" kaldes.
                                if (xpGained > obj.XpForNextLevelEquals) {
                                    i = i + 1;
                                    ding(i);
                                    return;
                                }
                                else
                                    xpGained++;
                                currentXp++;
                                $('.current-level > p').text(currentXp);
                                $('.xp-popup > div > .xp-reward').text("Rewarding " + i + " XP");
                                var xpProgress = (xpGained / obj.XpForNextLevelEquals) * 100;
                                $('.xp-popup > div > .level-container > .xp-bar > div').width(xpProgress + 2 + '%');
                                if (--i) {
                                    theLoop(i);
                                } else {
                                    $('.xp-popup > div > .xp-reward').text("Rewarded " + obj.ExerciseScore + " XP.. There you go!");
                                    setTimeout(function () {

                                        //Når xp'en er uddelt, kan achievements poppe op, inden at overlayet lukkes.
                                        $.ajax({
                                            url: '/Manage/GetUserAchievements',
                                            type: 'Post',
                                            datatype: 'json',
                                            data: { order: 'Client_Call' },
                                            success: function (data) {
                                                if (data.length === 2) {
                                                    setTimeout(function () {
                                                        $('.xp-popup').removeClass('popup-fadein');
                                                    }, 4000);
                                                } else {
                                                    var JsonArray = JSON.parse(data);
                                                    var i = 0, l = JsonArray.length;

                                                    var obj = JsonArray[i];
                                                    $('.xp-popup').addClass('popup-fadein');
                                                    (function iterator() {
                                                        var achievementimg = '<img style="width:125px;height:125px;" src = "/images/ach-' + JsonArray[i].AchievementId + '.png" />';
                                                        $('.achievement-popup').empty();
                                                        $(achievementimg).hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        $('<h3 style="color:#fff">Achievement unlocked!</h3>').hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        $('<h4 style="color:#fff">' + JsonArray[i].AchievementName + '</h4>').hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        if (++i < l) {
                                                            setTimeout(iterator, 4000);
                                                        } else {
                                                            $('.achievement-popup').delay(4000).fadeOut(1000);
                                                            setTimeout(function () {
                                                                $('.xp-popup').removeClass('popup-fadein');
                                                            }, 4000);

                                                        }
                                                    })();
                                                }
                                            },
                                            error: function (data) {
                                                console.log("error");
                                            },
                                        });
                                    }, 1500);
                                    return;
                                }
                            }, 2500 / obj.ExerciseScore);
                            return;
                        })(obj.ExerciseScore);
                    }, 1500);

                    //Dette funktion håndterer den resterende xp som skal gives i det nye level
                    function ding(i) {
                        currentXp = 0;
                        $('.xp-popup > div > .level-container > .xp-bar > div').width('0%');
                        //$('.xp-popup > div').remove();
                        $('.xp-popup').append("<div id='container1'> <div id='fireworks'> <span></span> <span></span> <span></span> <span></span> <span></span> <span></span> <span></span> <span></span> <span></span> <span></span> </div> </div><div class='row'><div class='col-xs-12 popup-img'>" + ImgBuilder + "</div><div style='display:flex' class='level-container col-xs-12'><div class='current-level col-xs-1'><span> " + obj.currentUserLevel + "</span><p style='font-size:.6em;'>" + currentXp + "</p></div><div class='xp-bar col-xs-10'><div style='text-align:center'></div></div><div class='next-level col-xs-1 pull-right'><span> " + obj.nextUserLevel + "</span><p style='font-size:.6em;'>" + targetXp + "</p></div></div></div><div style='text-align:center; color:#fff;' class='col-xs-12'><p class='xp-reward'> " + obj.ExerciseScore + " XP gained!</p></div>");

                        var iEquals = i;
                        var xpGained = 0;

                        (function theLoop(i) {
                            setTimeout(function () {
                                if (iEquals > obj.XpForNextLevelEquals) {
                                    i = i + 1;
                                    ding(iEquals);
                                    return;
                                }
                                else
                                    xpGained++;
                                currentXp++;
                                $('.current-level > p').text(currentXp);
                                iEquals--;
                                $('.xp-popup > div > .xp-reward').text("Rewarding " + i + " XP");
                                var xpProgress = (xpGained / obj.totalXpForNextLevel) * 100;
                                $('.xp-popup > div > .level-container > .xp-bar > div').width(xpProgress + 2 + '%');
                                //Hvis i er lig 0, er der ikke mere xp som skal uddeles, og popup'en kan forsvinde
                                if (--i) {
                                    theLoop(i);
                                } else {
                                    $('.xp-popup > div > .xp-reward').text("Rewarded " + obj.ExerciseScore + " XP.. There you go!");
                                    setTimeout(function () {
                                        //Når xp'en er uddelt, kan achievements poppe op, inden at overlayet lukkes.
                                        $.ajax({
                                            url: '/Manage/GetUserAchievements',
                                            type: 'Post',
                                            datatype: 'json',
                                            data: { order: 'Client_Call' },
                                            success: function (data) {
                                                
                                                if (data.length === 2) {
                                                    setTimeout(function () {
                                                        $('.xp-popup').removeClass('popup-fadein');
                                                    }, 4000);
                                                } else {
                                                    
                                                    var JsonArray = JSON.parse(data);
                                                    var i = 0, l = JsonArray.length;

                                                    var obj = JsonArray[i];
                                                    $('.xp-popup').addClass('popup-fadein');
                                                    (function iterator() {
                                                        var achievementimg = '<img style="width:125px;height:125px;" src = "/images/ach-' + JsonArray[i].AchievementId + '.png" />';
                                                        $('.achievement-popup').empty();
                                                        $(achievementimg).hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        $('<h3 style="color:#fff">Achievement unlocked!</h3>').hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        $('<h4 style="color:#fff">' + JsonArray[i].AchievementName + '</h4>').hide().appendTo('.achievement-popup').fadeIn(1000);
                                                        if (++i < l) {
                                                            setTimeout(iterator, 4000);
                                                        } else {
                                                            $('.achievement-popup').delay(4000).fadeOut(1000);
                                                            setTimeout(function () {
                                                                $('.xp-popup').removeClass('popup-fadein');
                                                            }, 4000);

                                                        }
                                                    })();
                                                }
                                            },
                                            error: function (data) {
                                                console.log("error");
                                            },
                                        });
                                    }, 1500);
                                    return;
                                }
                            }, 2500 / obj.ExerciseScore);
                            return;
                        })(iEquals);
                    }
                }
            }
            }, error: function (data) {
                console.log("error");
            }
    });


    
    var currentxp = $('.progress-bar').data('currentxp');
    var nextxp = $('.progress-bar').data('nextlevel');
    var xpProgress = (currentxp / nextxp) * 100;
    $('.progress-bar').animate({
        width: (xpProgress + '%')
    }, 200);
    $('.loader-div').fadeOut(500, function () { $(this).remove(); });

});
