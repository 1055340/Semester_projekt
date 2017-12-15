$(document).ready(function () {
    console.log("wew");
    
$.ajax({
    url: '/Manage/UserAchievementsUpdated',
    type: 'Post',
    datatype: 'json',
    data: { order: 'Client_Call' },
    success: function (data) {
        $('.loader-div').fadeOut(500, function () { $(this).remove(); });
        var JsonArray = JSON.parse(data);
        for (var i = 0; i < JsonArray.length; i++) {
            var obj = JsonArray[i];
            //For hver achievement, tjekker vi om det data-id som div'erne har, matcher et af de resultater vi har fået tilbage fra Json på denne bruger
            $('.achievement-container').children('.not-achieved').each(function () {
                if ($(this).data('id') === obj.AchievementId) 
                {
                    //Hvis dette opfyldes, flyttes de til containeren som indeholder de unlockede achievements, og de får en anden klasse
                    $($(this)).hide().appendTo('.achieved-container').fadeIn(400).removeClass('not-achieved').addClass('achieved');
                }
            });      
        };
    },
    error: function (data) {
        console.log("error");
    }
    });
    });