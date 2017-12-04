$(document).ready(function () {
$.ajax({
    url: '/Manage/GetUserAchievements',
    type: 'Post',
    datatype: 'json',
    data: { order: 'Client_Call' },
    success: function (data) {
        var JsonArray = JSON.parse(data);
        for (var i = 0; i < JsonArray.length; i++) {
            var obj = JsonArray[i];
            $(".achievement-container > div:nth-child(" + obj.AchievementId + ")").removeClass('not-achieved');
            console.log($('.achievement-container > div').data('id'));
            //console.log(obj.AchievementId);
        };
    },
    error: function (data) {
        console.log("error");
    }
    });
    });