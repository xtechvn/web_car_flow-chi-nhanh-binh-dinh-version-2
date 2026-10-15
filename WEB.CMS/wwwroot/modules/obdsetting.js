$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    _obd_setting.Init();
});

var _obd_setting = {
    IMPORT_MAX_FILE_SIZE: 5 * 1024 * 1024,
    DELIVERY_REGEX: /^\d{1,18}$/,
    HISTORY_ICON: '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#374151" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 12a9 9 0 1 0 3-6.7L3 8"></path><path d="M3 3v5h5"></path><path d="M12 7v5l3 2"></path></svg>',
    state: {
        loadingDate: '',
        version: null,
        delivery: '',
        pageIndex: 1,
        pageSize: 20,
        isBusinessDate: true,
        isLatest: true,
        totalVersion: 0
    },
    searchTimer: null,

    Init: function () {
        _obd_setting.state.loadingDate = $('#obd_loading_date').val();
        _obd_setting.BindEvents();
        _obd_setting.LoadSummary();
    },

    BindEvents: function () {
        $('#obd_keyword').on('keyup', function () {
            clearTimeout(_obd_setting.searchTimer);
            _obd_setting.searchTimer = setTimeout(function () {
                _obd_setting.state.delivery = $.trim($('#obd_keyword').val());
                _obd_setting.state.pageIndex = 1;
                _obd_setting.LoadList();
            }, 400);
        });

        $('#obd_loading_date').on('change', function () {
            var value = $(this).val();
            if (!value) {
                value = $(this).attr('data-business-date');
                $(this).val(value);
            }
            _obd_setting.state.loadingDate = value;
            _obd_setting.state.version = null;
            _obd_setting.state.pageIndex = 1;
            _obd_setting.LoadSummary();
        });

        $('#obd_version').on('change', function () {
            _obd_setting.state.version = parseInt($(this).val()) || null;
            _obd_setting.state.pageIndex = 1;
            _obd_setting.LoadSummary();
        });

        $('#btn_reset_obd').on('click', function () {
            var businessDate = $('#obd_loading_date').attr('data-business-date');
            $('#obd_keyword').val('');
            $('#obd_loading_date').val(businessDate);
            _obd_setting.state.delivery = '';
            _obd_setting.state.loadingDate = businessDate;
            _obd_setting.state.version = null;
            _obd_setting.state.pageIndex = 1;
            _obd_setting.LoadSummary();
        });

        $('#obd_version_banner').on('click', '.obd-banner__link', function () {
            _obd_setting.state.version = null;
            _obd_setting.state.pageIndex = 1;
            _obd_setting.LoadSummary();
        });

        $('#btn_import_obd').on('click', function () {
            $('#obd_import_file').val('').trigger('click');
        });
        $('#obd_import_file').on('change', function () {
            if (this.files && this.files.length > 0) {
                _obd_setting.Import(this.files[0]);
            }
        });
        $('#obd_import_errors_close').on('click', function () {
            $('#obd_import_errors').hide();
        });

        $('#btn_add_row_obd').on('click', function () {
            _obd_setting.AddRow();
        });
        $('#btn_tour_obd').on('click', function () {
            _obd_tour.Start();
        });

        var grid = $('#grid_data');
        grid.on('click', '.obd-page-link', function () {
            if ($(this).hasClass('disabled') || $(this).hasClass('active')) return;
            _obd_setting.state.pageIndex = parseInt($(this).attr('data-page')) || 1;
            _obd_setting.LoadList();
        });
        grid.on('change', '#obd_page_size', function () {
            _obd_setting.state.pageSize = parseInt($(this).val()) || 20;
            _obd_setting.state.pageIndex = 1;
            _obd_setting.LoadList();
        });

        grid.on('click', '.obd-btn-edit', function () {
            _obd_setting.ToggleEdit($(this).closest('tr'), true);
        });
        grid.on('click', '.obd-btn-history', function () {
            var row = $(this).closest('tr');
            var delivery = $.trim(row.find('.obd-delivery-text').text());
            $('#magnific-popup-large').addClass('obd-history-popup');
            _magnific.OpenLargerPopup('Lịch sử thao tác - Delivery ' + delivery, '/ObdSetting/History', { obdId: row.attr('data-id') });
        });
        grid.on('click', '.obd-btn-cancel', function () {
            _obd_setting.ToggleEdit($(this).closest('tr'), false);
        });
        grid.on('click', '.obd-btn-save', function () {
            _obd_setting.SaveDelivery($(this).closest('tr'));
        });
        grid.on('keydown', '.obd-delivery-input', function (e) {
            var row = $(this).closest('tr');
            if (e.key === 'Enter') _obd_setting.SaveDelivery(row);
            if (e.key === 'Escape') _obd_setting.ToggleEdit(row, false);
        });

        grid.on('click', '.obd-btn-new-save', function () {
            _obd_setting.SaveNewRow();
        });
        grid.on('click', '.obd-btn-new-cancel', function () {
            _obd_setting.RemoveNewRow();
        });
        grid.on('keydown', '.obd-new-row input', function (e) {
            if (e.key === 'Enter') _obd_setting.SaveNewRow();
            if (e.key === 'Escape') _obd_setting.RemoveNewRow();
        });

        grid.on('keydown', '.obd-new-delivery, .obd-delivery-input', function (e) {
            if (_obd_setting.IsBlockedKey(e, /\d/)) e.preventDefault();
        });
        grid.on('input', '.obd-new-delivery, .obd-delivery-input', function () {
            _obd_setting.FormatDeliveryInput(this);
        });
        grid.on('keydown', '.obd-new-weight', function (e) {
            if (_obd_setting.IsBlockedKey(e, /[\d.]/)) e.preventDefault();
        });
        grid.on('input', '.obd-new-weight', function () {
            _obd_setting.FormatWeightInput(this);
        });
        grid.on('blur', '.obd-new-weight', function () {
            this.value = this.value.replace(/\.$/, '');
        });
    },

    IsBlockedKey: function (e, allowedChar) {
        if (e.ctrlKey || e.metaKey || e.altKey) return false;
        if (!e.key || e.key.length !== 1) return false;
        return !allowedChar.test(e.key);
    },

    FormatDeliveryInput: function (el) {
        var caret = el.selectionStart || 0;
        var digitsBefore = el.value.slice(0, caret).replace(/\D/g, '').length;
        var digits = el.value.replace(/\D/g, '').slice(0, 18);
        if (digits === el.value) return;
        el.value = digits;
        var pos = Math.min(digitsBefore, digits.length);
        el.setSelectionRange(pos, pos);
    },

    FormatWeightInput: function (el) {
        var caret = el.selectionStart || 0;
        var charsBefore = el.value.slice(0, caret).replace(/[^\d.]/g, '').length;

        var raw = el.value.replace(/[^\d.]/g, '');
        var dot = raw.indexOf('.');
        var intPart = (dot >= 0 ? raw.slice(0, dot) : raw).replace(/^0+(?=\d)/, '').slice(0, 15);
        var decPart = dot >= 0 ? raw.slice(dot + 1).replace(/\./g, '').slice(0, 3) : null;
        if (dot >= 0 && intPart === '') intPart = '0';

        var formatted = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',') + (decPart !== null ? '.' + decPart : '');
        if (formatted === el.value) return;
        el.value = formatted;

        var pos = 0, count = 0;
        while (pos < formatted.length && count < charsBefore) {
            if (formatted[pos] !== ',') count++;
            pos++;
        }
        el.setSelectionRange(pos, pos);
    },

    LoadSummary: function (skipList) {
        var state = _obd_setting.state;
        $.ajax({
            url: '/ObdSetting/GetSummary',
            type: 'POST',
            data: { LoadingDate: state.loadingDate, Version: state.version },
            success: function (result) {
                if (result.status != 0) {
                    _msgalert.error(result.msg);
                    return;
                }
                _obd_setting.RenderSummary(result.data);
                if (!skipList) _obd_setting.LoadList();
            },
            error: function () {
                _msgalert.error('Không tải được thông tin version, vui lòng thử lại');
            }
        });
    },

    RenderSummary: function (data) {
        var state = _obd_setting.state;
        state.loadingDate = data.loadingDate;
        state.version = data.selectedVersion > 0 ? data.selectedVersion : null;
        state.isBusinessDate = data.isBusinessDate;
        state.isLatest = data.isLatest;
        state.totalVersion = data.totalVersion;

        $('#obd_loading_date').attr('data-business-date', data.businessDate).val(data.loadingDate);

        var select = $('#obd_version').empty();
        if (data.versions.length === 0) {
            select.append('<option value="">--</option>').prop('disabled', true);
        } else {
            $.each(data.versions, function (_, item) {
                var label = 'Version ' + item.version + (item.version === data.latestVersion ? ' (mới nhất)' : '');
                select.append($('<option>').val(item.version).text(label));
            });
            select.val(data.selectedVersion).prop('disabled', false);
        }

        $('#obd_total').text(Number(data.totalObd).toLocaleString('en-US'));
        $('#obd_total_version').text(data.totalVersion);
        $('#obd_total_weight').text(data.totalNetWeightText);

        var banner = $('#obd_version_banner').removeClass('obd-banner--latest obd-banner--old');
        if (data.totalVersion === 0) {
            banner.text('Ngày ' + data.loadingDateText + ' chưa có dữ liệu OBD.'
                + (data.isBusinessDate ? ' Hãy import file Excel hoặc thêm dòng để tạo Version 1.' : ''));
        } else if (data.isLatest) {
            banner.addClass('obd-banner--latest').text('Đang xem Version ' + data.selectedVersion + ' (mới nhất) của ngày ' + data.loadingDateText
                + (data.lastUpdateText ? ' - cập nhật lần cuối lúc ' + data.lastUpdateText : ''));
        } else {
            banner.addClass('obd-banner--old').text('Đang xem Version ' + data.selectedVersion + ' của ngày ' + data.loadingDateText
                + '. Version mới nhất là Version ' + data.latestVersion + '.')
                .append('<a class="obd-banner__link">Xem version mới nhất</a>');
        }

        var canAdd = data.isBusinessDate && (data.isLatest || data.totalVersion === 0);
        $('#btn_add_row_obd').prop('disabled', !canAdd);
    },

    LoadList: function () {
        var state = _obd_setting.state;
        $.ajax({
            url: '/ObdSetting/GetList',
            type: 'POST',
            data: {
                LoadingDate: state.loadingDate,
                Version: state.version,
                Delivery: state.delivery,
                PageIndex: state.pageIndex,
                PageSize: state.pageSize
            },
            success: function (html) {
                $('#grid_data').html(html);
                _obd_tour.StartIfFirstTime();
            },
            error: function () {
                _msgalert.error('Không tải được danh sách OBD, vui lòng thử lại');
            }
        });
    },

    Import: function (file) {
        var name = (file.name || '').toLowerCase();
        if (name.slice(-5) !== '.xlsx') {
            _obd_setting.ShowImportErrors('Chỉ hỗ trợ file Excel định dạng .xlsx');
            return;
        }
        if (file.size > _obd_setting.IMPORT_MAX_FILE_SIZE) {
            _obd_setting.ShowImportErrors('File vượt quá dung lượng cho phép 5MB');
            return;
        }

        var formData = new FormData();
        formData.append('file', file);
        var button = $('#btn_import_obd').prop('disabled', true);
        $('#obd_import_errors').hide();

        $.ajax({
            url: '/ObdSetting/Import',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result.status == 0) {
                    _obd_setting.ShowImportResult(true, result.msg);
                    $('#obd_keyword').val('');
                    _obd_setting.state.delivery = '';
                    _obd_setting.state.loadingDate = result.loadingDate;
                    _obd_setting.state.version = null;
                    _obd_setting.state.pageIndex = 1;
                    _obd_setting.LoadSummary();
                } else {
                    _obd_setting.ShowImportErrors(result.msg, result.errors);
                }
            },
            error: function () {
                _obd_setting.ShowImportErrors('Import không thành công, vui lòng thử lại');
            },
            complete: function () {
                button.prop('disabled', false);
                $('#obd_import_file').val('');
            }
        });
    },

    ShowImportErrors: function (title, errors) {
        _obd_setting.ShowImportResult(false, title, errors);
    },

    ShowImportResult: function (isSuccess, title, errors) {
        var list = $('#obd_import_errors_list').empty();
        $.each(errors || [], function (_, message) {
            list.append($('<li>').text(message));
        });
        list.toggle(list.children().length > 0);
        $('#obd_import_errors_title').text(title);
        $('#obd_import_errors').toggleClass('obd-errors--success', isSuccess).show();
    },

    ToggleEdit: function (row, isEdit) {
        var input = row.find('.obd-delivery-input');
        if (!isEdit) input.val(row.find('.obd-delivery-text').text());
        row.find('.obd-delivery-text').toggle(!isEdit);
        input.toggle(isEdit);
        row.find('.obd-actions--view').toggle(!isEdit);
        row.find('.obd-actions--edit').toggle(isEdit);
        if (isEdit) input.focus().select();
    },

    SaveDelivery: function (row) {
        var oldValue = $.trim(row.find('.obd-delivery-text').text());
        var delivery = $.trim(row.find('.obd-delivery-input').val());
        if (!_obd_setting.DELIVERY_REGEX.test(delivery) || /^0+$/.test(delivery)) {
            _msgalert.error('Delivery chỉ gồm chữ số, tối đa 18 số');
            return;
        }
        if (delivery === oldValue) {
            _obd_setting.ToggleEdit(row, false);
            return;
        }

        var buttons = row.find('.obd-btn-save, .obd-btn-cancel').prop('disabled', true);
        $.ajax({
            url: '/ObdSetting/UpdateDelivery',
            type: 'POST',
            data: { obdId: row.attr('data-id'), delivery: delivery },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    _obd_setting.LoadSummary();
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function () {
                _msgalert.error('Cập nhật không thành công, vui lòng thử lại');
            },
            complete: function () {
                buttons.prop('disabled', false);
            }
        });
    },

    AddRow: function () {
        var tbody = $('#obd_tbody');
        if (tbody.length === 0) return;
        if (tbody.find('.obd-new-row').length > 0) {
            tbody.find('.obd-new-row .obd-new-delivery').focus();
            return;
        }
        var state = _obd_setting.state;
        var parts = (state.loadingDate || '').split('-');
        var dateText = parts.length === 3 ? parts[2] + '/' + parts[1] + '/' + parts[0] : '';
        var versionText = state.totalVersion > 0 ? state.version : 1;

        tbody.find('.obd-empty-row').hide();
        tbody.prepend(
            '<tr class="obd-new-row">'
            + '<td>Mới</td>'
            + '<td><input type="text" class="obd-inline-input obd-new-delivery" maxlength="18" inputmode="numeric" placeholder="Delivery"></td>'
            + '<td>' + dateText + '</td>'
            + '<td>' + versionText + '</td>'
            + '<td><input type="text" class="obd-inline-input obd-new-weight" maxlength="25" placeholder="VD: 7,000.000"></td>'
            + '<td></td><td></td>'
            + '<td><span class="obd-actions">'
            + '<button type="button" class="obd-btn obd-btn--primary obd-btn--sm obd-btn-new-save">Lưu</button>'
            + '<button type="button" class="obd-btn obd-btn--outline obd-btn--sm obd-btn-new-cancel">Hủy</button>'
            + '</span></td>'
            + '</tr>');
        tbody.find('.obd-new-delivery').focus();
    },

    ReplaceNewRow: function (newRow, data) {
        var text = function (value) { return $('<td>').text(value == null ? '' : value); };
        var saved = $('<tr class="obd-row-just-added">').attr('data-id', data.obdId)
            .append($('<td>').append('<span class="obd-badge-new">Mới</span>'))
            .append($('<td class="fw-600 obd-delivery-cell">')
                .append($('<span class="obd-delivery-text">').text(data.delivery))
                .append($('<input type="text" class="obd-inline-input obd-delivery-input" maxlength="18" inputmode="numeric" style="display:none;">').val(data.delivery)))
            .append(text(data.loadingDateText))
            .append(text(data.version))
            .append(text(data.netWeightText).addClass('text-right'))
            .append(text(data.userCreateName))
            .append(text(''))
            .append($('<td>').append(
                '<span class="obd-actions obd-actions--view"><i class="icon-edit obd-btn-edit" title="Sửa Delivery"></i>'
                + '<span class="obd-btn-history" title="Lịch sử thao tác">' + _obd_setting.HISTORY_ICON + '</span></span>'
                + '<span class="obd-actions obd-actions--edit" style="display:none;">'
                + '<button type="button" class="obd-btn obd-btn--primary obd-btn--sm obd-btn-save">Lưu</button>'
                + '<button type="button" class="obd-btn obd-btn--outline obd-btn--sm obd-btn-cancel">Hủy</button>'
                + '</span>'));
        newRow.replaceWith(saved);
        $('#obd_tbody .obd-empty-row').remove();
    },

    RemoveNewRow: function () {
        var tbody = $('#obd_tbody');
        tbody.find('.obd-new-row').remove();
        tbody.find('.obd-empty-row').show();
    },

    SaveNewRow: function () {
        var row = $('#obd_tbody .obd-new-row');
        var delivery = $.trim(row.find('.obd-new-delivery').val());
        var netWeight = $.trim(row.find('.obd-new-weight').val());
        if (!_obd_setting.DELIVERY_REGEX.test(delivery) || /^0+$/.test(delivery)) {
            _msgalert.error('Delivery chỉ gồm chữ số, tối đa 18 số');
            row.find('.obd-new-delivery').focus();
            return;
        }
        if (!netWeight) {
            _msgalert.error('Vui lòng nhập Net Weight');
            row.find('.obd-new-weight').focus();
            return;
        }

        var buttons = row.find('button').prop('disabled', true);
        $.ajax({
            url: '/ObdSetting/Insert',
            type: 'POST',
            data: { delivery: delivery, netWeight: netWeight.replace(/,/g, '') },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    var state = _obd_setting.state;
                    var sameView = result.loadingDate === state.loadingDate;
                    state.loadingDate = result.loadingDate;
                    state.version = null;
                    if (sameView && result.row) {
                        _obd_setting.ReplaceNewRow(row, result.row);
                        _obd_setting.LoadSummary(true);
                    } else {
                        _obd_setting.LoadSummary();
                    }
                } else {
                    _msgalert.error(result.msg);
                    buttons.prop('disabled', false);
                }
            },
            error: function () {
                _msgalert.error('Thêm mới không thành công, vui lòng thử lại');
                buttons.prop('disabled', false);
            }
        });
    }
};

var _obd_tour = {
    STORAGE_KEY: 'obd_setting_tour_done',
    steps: [
        {
            target: '#btn_import_obd',
            title: 'Thêm cấu hình OBD',
            body: 'Bấm để chọn file Excel (.xlsx). Hàng 1 là tiêu đề <b>Delivery</b> và <b>Net Weight</b>. Mỗi lần import thành công sẽ tạo một <b>version mới</b> của ngày hiện tại.'
        },
        {
            target: '.obd-import__hint',
            title: 'Giới hạn file',
            body: 'File tối đa 5MB / 10.000 dòng. Nếu file có lỗi, toàn bộ file bị từ chối và danh sách lỗi theo từng dòng sẽ hiện ngay phía trên bộ lọc.'
        },
        {
            target: '#obd_filter',
            title: 'Bộ lọc',
            body: 'Tìm theo mã <b>Delivery</b>, chọn <b>Loading Date</b> (mặc định là ngày nghiệp vụ theo giờ cut-off) và chọn <b>Version</b> để xem lại các lần import trước.'
        },
        {
            target: '#obd_version_banner',
            title: 'Trạng thái version',
            body: 'Màu xanh: đang xem version mới nhất. Màu cam: đang xem version cũ, bấm "Xem version mới nhất" để quay lại.'
        },
        {
            target: '#obd_stats',
            title: 'Thống kê',
            body: 'Tổng số Delivery, tổng số version trong ngày và tổng Net Weight của version đang xem.'
        },
        {
            target: '#btn_add_row_obd',
            title: 'Thêm dòng thủ công',
            body: 'Thêm nhanh một Delivery ngay trên bảng. Chỉ dùng được khi đang xem ngày hiện tại và version mới nhất.'
        },
        {
            target: '#grid_data .obd-table th:last-child',
            title: 'Sửa Delivery và xem lịch sử',
            body: 'Bấm icon sửa ở cột Thao tác để sửa Delivery trực tiếp trên dòng. Nhấn <b>Enter</b> để lưu, <b>Esc</b> để hủy. Bấm icon đồng hồ để xem lịch sử thao tác của dòng đó.'
        }
    ],
    index: 0,
    activeSteps: [],
    started: false,

    StartIfFirstTime: function () {
        if (_obd_tour.started) return;
        try {
            if (sessionStorage.getItem(_obd_tour.STORAGE_KEY)) return;
        } catch (e) { return; }
        _obd_tour.Start();
    },

    Start: function () {
        _obd_tour.Close(false);
        _obd_tour.started = true;
        _obd_tour.activeSteps = $.grep(_obd_tour.steps, function (step) {
            return $(step.target).filter(':visible').length > 0;
        });
        if (_obd_tour.activeSteps.length === 0) return;

        $('body').append('<div class="obd-tour-spot"></div>'
            + '<div class="obd-tour-pop" role="dialog">'
            + '<div class="obd-tour-pop__title"></div>'
            + '<div class="obd-tour-pop__body"></div>'
            + '<div class="obd-tour-pop__foot">'
            + '<span class="obd-tour-pop__step"></span>'
            + '<span class="obd-tour-pop__skip">Bỏ qua</span>'
            + '<button type="button" class="obd-btn obd-btn--outline obd-btn--sm obd-tour-prev">Quay lại</button>'
            + '<button type="button" class="obd-btn obd-btn--primary obd-btn--sm obd-tour-next">Tiếp</button>'
            + '</div></div>');

        $('.obd-tour-pop__skip').on('click', function () { _obd_tour.Close(true); });
        $('.obd-tour-prev').on('click', function () { _obd_tour.Show(_obd_tour.index - 1); });
        $('.obd-tour-next').on('click', function () {
            if (_obd_tour.index >= _obd_tour.activeSteps.length - 1) _obd_tour.Close(true);
            else _obd_tour.Show(_obd_tour.index + 1);
        });
        $(window).on('resize.obdtour scroll.obdtour', _obd_tour.Position);
        $(document).on('keydown.obdtour', function (e) {
            if (e.key === 'Escape') _obd_tour.Close(true);
        });

        _obd_tour.Show(0);
    },

    Show: function (index) {
        var steps = _obd_tour.activeSteps;
        if (index < 0 || index >= steps.length) return;
        _obd_tour.index = index;
        var step = steps[index];
        var isLast = index === steps.length - 1;

        $('.obd-tour-pop__title').text(step.title);
        $('.obd-tour-pop__body').html(step.body);
        $('.obd-tour-pop__step').text((index + 1) + ' / ' + steps.length);
        $('.obd-tour-prev').toggle(index > 0);
        $('.obd-tour-next').text(isLast ? 'Hoàn tất' : 'Tiếp');

        var target = $(step.target).filter(':visible').first()[0];
        if (target && target.scrollIntoView) target.scrollIntoView({ block: 'center', behavior: 'smooth' });
        setTimeout(_obd_tour.Position, 250);
        _obd_tour.Position();
    },

    Position: function () {
        var step = _obd_tour.activeSteps[_obd_tour.index];
        var target = step ? $(step.target).filter(':visible').first()[0] : null;
        var pop = $('.obd-tour-pop');
        if (!target || pop.length === 0) return;

        var padding = 6;
        var rect = target.getBoundingClientRect();
        $('.obd-tour-spot').css({
            top: rect.top - padding,
            left: rect.left - padding,
            width: rect.width + padding * 2,
            height: rect.height + padding * 2
        });

        var popWidth = pop.outerWidth();
        var popHeight = pop.outerHeight();
        var gap = 12;
        var top = rect.bottom + padding + gap;
        if (top + popHeight > window.innerHeight - 8) {
            top = Math.max(8, rect.top - padding - gap - popHeight);
        }
        var left = rect.left + rect.width / 2 - popWidth / 2;
        left = Math.max(8, Math.min(left, window.innerWidth - popWidth - 8));
        pop.css({ top: top, left: left });
    },

    Close: function (markDone) {
        $('.obd-tour-spot, .obd-tour-pop').remove();
        $(window).off('.obdtour');
        $(document).off('.obdtour');
        if (markDone) {
            try { sessionStorage.setItem(_obd_tour.STORAGE_KEY, '1'); } catch (e) { }
        }
    }
};

