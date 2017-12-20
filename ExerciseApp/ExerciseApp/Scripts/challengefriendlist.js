    $('.challenged').click(function () {
        //var challengedName = $(this).parent().parent().children('.challenged-name').children().text();
        var challengedName = $(this).children('.challenged-name').children('span').text();
        var challengedId = $(this).children('.challenged-name').data('id');
        //var challengedId = $(this).parent().parent().children('.challenged-name').data('id');
        Cookies.set('challengedName', "" + challengedName + "", { expires: 1 });
        Cookies.set('challengedId', "" + challengedId + "", { expires: 1 });
        window.location.href = '/manage/ChallengeCategories';
    });

