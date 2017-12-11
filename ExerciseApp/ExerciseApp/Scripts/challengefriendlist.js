    $('.challenged').click(function () {
        var challengedName = $(this).parent().parent().children('.challenged-name').children().text();
        var challengedId = $(this).parent().parent().children('.challenged-name').data('id');
        Cookies.set('challengedName', "" + challengedName + "", { expires: 1 });
        Cookies.set('challengedId', "" + challengedId + "", { expires: 1 });
        window.location.href = '/manage/ChallengeCategories';
    });


