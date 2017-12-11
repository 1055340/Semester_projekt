$(document).ready(function () {
    $('.challenger').each(function (i, obj) {
        if ($(this).data('accept') == 1) {
            console.log("this one is");
            var loc = $(this).children('.acc-btn');
            $(this).children('.acc-btn').empty();
            $('<a class="accept btn btn-success">&#128077;</a><a class="deny btn btn-danger">&#128078;</a>').hide().appendTo(loc).fadeIn(1000);
        } else {
            return;
        }
    });
    $('.acc-btn > a').click(function () {
        console.log("works");
        var id = $(this).parent().parent().data('id');

        $.ajax({
            url: '/Manage/GetChallengeDetails',
            type: 'Post',
            datatype: 'json',
            data: {
                order: 'Client_Call',
                id: id
            },
            success: function (data) {
                console.log(data)
                const obj = JSON.parse(data);
                console.log(obj);
                

                
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
        $('<a class="btn btn-success">></a>').hide().appendTo(loc).fadeIn(1000);

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
