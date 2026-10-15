$(document).ready(function () {
    _registration_recovery.init();
});

// Xe có trong MongoDB nhưng chưa vào SQL: icon nhấp nháy + lightbox đẩy lại
var _registration_recovery = {
    pollInterval: 30000,
    timer: null,

    init: function () {
        $('#reg-recovery-icon').on('click', function () {
            _registration_recovery.Open();
        });
        _registration_recovery.RefreshCount();
        _registration_recovery.timer = setInterval(_registration_recovery.RefreshCount, _registration_recovery.pollInterval);
    },

    RefreshCount: function () {
        $.ajax({
            url: '/RegistrationRecovery/Count',
            type: 'get',
            cache: false,
            success: function (result) {
                _registration_recovery.SetCount(result && result.count ? result.count : 0);
            }
        });
    },

    SetCount: function (count) {
        var icon = $('#reg-recovery-icon');
        if (count > 0) {
            icon.find('.reg-recovery-icon__count').text(count);
            icon.attr('title', count + ' xe đăng ký chưa vào danh sách - bấm để đẩy lại');
            icon.show();
        } else {
            icon.hide();
        }
    },

    Open: function () {
        var popup = $('#magnific-popup-large');
        popup.addClass('reg-recovery-popup');
        $(document).one('mfpClose', function () {
            popup.removeClass('reg-recovery-popup');
        });
        _magnific.OpenLargerPopup('Xe đăng ký chưa vào danh sách', '/RegistrationRecovery/Popup', {});
    },

    ReloadPopup: function () {
        $.ajax({
            url: '/RegistrationRecovery/Popup',
            type: 'post',
            success: function (result) {
                $('#magnific-popup-large .magnific-body').html(result);
            }
        });
    },

    Push: function () {
        var ids = $('.reg-recovery-check:checked').map(function () { return $(this).val(); }).get();
        if (ids.length === 0) {
            _msgalert.error('Vui lòng chọn ít nhất một xe để đẩy.');
            return;
        }

        var button = $('#reg-recovery-push');
        button.prop('disabled', true).text('Đang đẩy...');
        $.ajax({
            url: '/RegistrationRecovery/Push',
            type: 'post',
            traditional: true,
            data: { ids: ids },
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                } else {
                    _msgalert.error(result.msg);
                }

                _registration_recovery.SetCount(result.remaining || 0);
                if (result.remaining > 0) {
                    _registration_recovery.ReloadPopup();
                } else {
                    $.magnificPopup.close();
                }

                // Chờ SignalR chèn dòng mới xong rồi tải lại để danh sách đúng thứ tự, không lặp dòng
                if (result.pushed > 0 && typeof _cartofactory !== 'undefined') {
                    setTimeout(function () {
                        _cartofactory.ListCartoFactory();
                    }, 1500);
                }
            },
            error: function () {
                _msgalert.error('Lỗi kết nối, vui lòng thử lại.');
            },
            complete: function () {
                button.prop('disabled', false).text('Đẩy vào hệ thống');
            }
        });
    }
};

$(document).on('change', '#reg-recovery-check-all', function () {
    $('.reg-recovery-check').prop('checked', $(this).is(':checked'));
});
