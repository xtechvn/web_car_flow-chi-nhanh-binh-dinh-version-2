$(document).ready(function () {

    $('.edit-time').on('click', function () {
        $('.capnhat_time').show();
        $('.edit-time').hide();
    });
    $('.check-time').on('click', function () {
        _configuration.LuuTime()
        //$('.capnhat_time').hide();
        //$('.edit-time').show();
    });



});
var _configuration = {

    LuuTime: function () {
        var time = $('#ExpireDate').val()
        var Id = $('#Id').val()
        $.ajax({
            url: "/Configuration/Setup",
            type: "post",
            data: { time: time, id: Id },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    setTimeout(function () { location.reload() }, 1000);
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    }

}
