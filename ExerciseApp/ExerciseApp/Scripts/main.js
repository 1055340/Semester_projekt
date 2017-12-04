$(document).ready(function () {

    $.ajax({
        url: '/Manage/GetUserAchievements',
        type: 'Post',
        datatype: 'json',
        data: { order: 'Client_Call' },
        success: function (data) {
            var JsonArray = JSON.parse(data);
            var i = 0, l = JsonArray.length;
            
            var obj = JsonArray[i];
            $('.xp-popup').addClass('popup-fadein');
            (function iterator() {
                console.log(JsonArray[i].AchievementId);
                var achievementimg = '<img style="width:100px;height:100px;" src = "/images/ach-' + JsonArray[i].AchievementId + '.png" />';
                $('.achievement-popup').empty();
                $(achievementimg).hide().appendTo('.achievement-popup').fadeIn(1000);
                $('<p style="color:#fff">Achievement unlocked!</p>').hide().appendTo('.achievement-popup').fadeIn(1000);
                $('<p style="color:#fff">' + JsonArray[i].AchievementName + '</p>').hide().appendTo('.achievement-popup').fadeIn(1000);
                if (++i < l) {
                    setTimeout(iterator, 2000);
                } else {
                    $('.achievement-popup').delay(2000).fadeOut(1000);
                    setTimeout(function () {
                        $('.xp-popup').removeClass('popup-fadein');
                    }, 2000);
                    
                }
            })();
        },
        error: function (data) {
            console.log("error");
        },
    });

    $.ajax({
        url: '/Manage/GetXpPopup',
        type: 'Post',
        datatype: 'json',
        data: { order: 'Client_Call' },
        success: function (data) {
            var JsonArray = JSON.parse(data);
            for (var i = 0; i < JsonArray.length; i++) {
                var obj = JsonArray[i];
                console.log("Uid " + obj.UserId);
                console.log("EXid " + obj.ExerciseId);
                console.log("Score " + obj.ExerciseScore);
                console.log("XpThisLevel " + obj.totalXpForThisLevelEquals);
                console.log("XpNextLevel " + obj.totalXpForNextLevel);
                console.log("Currentlevel " + obj.currentUserLevel);
                console.log("Nextlevel " + obj.nextUserLevel);
                console.log("Currentxp " + obj.currentUserXp);
                console.log("Level before " + obj.OldCurrentLevel);
                console.log("Level after " + obj.OldNextLevel);
                console.log("xp above old level " + obj.XpForCurrentLevelEquals)
                console.log("xp below next level " + obj.XpForNextLevelEquals)

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
                        console.log("i is = " + i);
                        $('.xp-popup > div > .level-container > .xp-bar > div').width(xpProgress + 2 + '%'); 
                        if (--i) {
                            theLoop(i);
                        } else {
                            $('.xp-popup > div > .xp-reward').text("Rewarded " + obj.ExerciseScore + " XP.. There you go!");
                            setTimeout(function () {
                                $('.xp-popup').removeClass('popup-fadein');
                            }, 1500);
                            return;
                        }
                    }, 2500/obj.ExerciseScore);
                    return;
                })(obj.ExerciseScore);
                }, 1500);
                //Dette funktion håndterer den resterende xp som skal gives i det nye level
                function ding(i) {
                    currentXp = 0;
                    console.log("Xp remaining: " + i);
                    $('.xp-popup > div > .level-container > .xp-bar > div').width('0%');
                    $('.xp-popup > div').remove();
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
                            console.log("i is = " + i);
                            $('.xp-popup > div > .level-container > .xp-bar > div').width(xpProgress + 2 + '%');
                            //Hvis i er lig 0, er der ikke mere xp som skal uddeles, og popup'en kan forsvinde
                            if (--i) {
                                theLoop(i);
                            } else {
                                $('.xp-popup > div > .xp-reward').text("Rewarded " + obj.ExerciseScore + " XP.. There you go!");
                                setTimeout(function () {
                                    //$('.xp-popup').removeClass('popup-fadein');
                                }, 1500);
                                return;
                            }
                        }, 2500/obj.ExerciseScore);
                        return;
                    })(iEquals);
                }
            }
        }, error: function (data) {
            console.log("error");
        }
    });


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
    var currentxp = $('.level-container').data('currentxp');
    var nextxp = $('.level-container').data('nextlevel');
    var xpProgress = (currentxp / nextxp) * 100;
    $('.level-container > .xp-bar > div').animate({
        width: (xpProgress + '%')
    }, 1000);
}); 

