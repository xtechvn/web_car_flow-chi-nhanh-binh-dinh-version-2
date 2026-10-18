/**
 * Widget OBD nhận hàng dùng chung (CartoFactory + WeighedInput).
 * API: _obdToken.bindEvents(), _obdToken.render(item), _obdToken.onChanged($wrap)
 */
var _obdToken = {
    DELIVERY_REGEX: /^\d{1,18}$/,
    _obdSuggestTimer: null,
    _obdBound: false,
    _obdScrollParents: null,
    /** Callback sau gắn/gỡ OBD — WeighedInput dùng để cập nhật Net weight / Check */
    onChanged: null,

    formatChip: function (obdId, delivery, netWeight) {
        var nwRaw = netWeight == null || netWeight === '' ? '0' : netWeight;
        var nwNum = _obdToken.parseNetWeight(nwRaw);
        var nwLabel = _obdToken.formatNetWeightVi(nwNum);
        var tip = 'Net Weight: ' + nwLabel + ' kg';
        return '<span class="obd-token__chip" data-obd-id="' + obdId + '" data-delivery="' + delivery + '" data-net-weight="' + nwRaw + '" title="' + tip + '">'
            + '<span class="obd-token__chip-text">' + delivery + ': ' + nwLabel + ' kg</span>'
            + '<button type="button" class="obd-token__chip-remove" title="Gỡ">&times;</button>'
            + '</span>';
    },

    parseNetWeight: function (text) {
        if (text == null || text === '') return 0;
        var n = parseFloat(String(text).replace(/,/g, ''));
        return isNaN(n) ? 0 : n;
    },

    formatNetWeightVi: function (value) {
        var n = Number(value) || 0;
        return n.toLocaleString('vi-VN', { maximumFractionDigits: 3, minimumFractionDigits: 0 });
    },

    sumNetWeight: function ($wrap) {
        var sum = 0;
        $wrap.find('.obd-token__chip').each(function () {
            sum += _obdToken.parseNetWeight($(this).attr('data-net-weight'));
        });
        return sum;
    },

    syncTotal: function ($wrap) {
        var sum = _obdToken.sumNetWeight($wrap);
        var $total = $wrap.find('.obd-token__total-value');
        if (!$total.length) {
            $wrap.prepend('<div class="obd-token__total">Tổng: <b class="obd-token__total-value">0</b> kg</div>');
            $total = $wrap.find('.obd-token__total-value');
        }
        $total.text(_obdToken.formatNetWeightVi(sum));
        if (typeof _obdToken.onChanged === 'function') {
            _obdToken.onChanged($wrap);
        }
    },

    render: function (item) {
        var chips = '';
        var list = (item && (item.obdList || item.ObdList)) || [];
        var sum = 0;
        var vehicleId = item && (item.id != null ? item.id : item.Id);
        for (var i = 0; i < list.length; i++) {
            var obd = list[i];
            var obdId = obd.obdId != null ? obd.obdId : obd.ObdId;
            var delivery = obd.deliveryText || obd.DeliveryText || obd.delivery || obd.Delivery || '';
            var netWeight = obd.netWeightText || obd.NetWeightText || obd.netWeight || obd.NetWeight || '0';
            sum += _obdToken.parseNetWeight(netWeight);
            chips += _obdToken.formatChip(obdId, delivery, netWeight);
        }
        return '<div class="obd-token" data-vehicle-id="' + (vehicleId || '') + '">'
            + '<div class="obd-token__total">Tổng: <b class="obd-token__total-value">' + _obdToken.formatNetWeightVi(sum) + '</b> kg</div>'
            + '<div class="obd-token__chips">' + chips + '</div>'
            + '<span class="obd-token__input-wrap">'
            + '<input type="text" class="obd-token__input" placeholder="Nhập mã OBD" inputmode="numeric" autocomplete="off" maxlength="18" />'
            + '<button type="button" class="obd-token__input-clear" title="Xóa" style="display:none;">&times;</button>'
            + '</span>'
            + '<div class="obd-token__dropdown" style="display:none;"></div>'
            + '</div>';
    },

    syncClearBtn: function ($input) {
        var $clear = $input.closest('.obd-token__input-wrap').find('.obd-token__input-clear');
        if ($.trim($input.val())) $clear.show();
        else $clear.hide();
    },

    closeDropdown: function () {
        $(window).off('scroll.obdSuggest resize.obdSuggest');
        if (_obdToken._obdScrollParents) {
            _obdToken._obdScrollParents.off('scroll.obdSuggest');
            _obdToken._obdScrollParents = null;
        }
        $('.obd-token__dropdown.is-portal').remove();
        $('.obd-token__dropdown').not('.is-portal').hide().empty();
    },

    getPortalDropdown: function ($wrap) {
        var vehicleId = $wrap.attr('data-vehicle-id');
        return $('.obd-token__dropdown.is-portal[data-vehicle-id="' + vehicleId + '"]');
    },

    positionDropdown: function ($wrap, $dd) {
        var $ref = $wrap.find('.obd-token__input-wrap');
        if (!$ref.length) $ref = $wrap;
        var rect = $ref[0].getBoundingClientRect();
        var menuW = Math.max(280, Math.min(360, rect.width || 280));
        $dd.css({
            position: 'fixed',
            display: 'block',
            visibility: 'hidden',
            width: menuW,
            maxHeight: 220,
            left: 0,
            top: 0,
            zIndex: 999999
        });
        var menuH = $dd.outerHeight() || 220;
        var spaceBelow = window.innerHeight - rect.bottom;
        var spaceAbove = rect.top;
        var left = rect.left;
        if (left + menuW > window.innerWidth - 8) {
            left = Math.max(8, window.innerWidth - menuW - 8);
        }
        var top;
        if (spaceBelow < menuH + 8 && spaceAbove > spaceBelow) {
            top = Math.max(8, rect.top - menuH - 2);
            $dd.addClass('drop-up');
            var maxUp = Math.max(120, rect.top - 10);
            if (menuH > maxUp) $dd.css('max-height', maxUp + 'px');
            menuH = $dd.outerHeight() || menuH;
            top = Math.max(8, rect.top - menuH - 2);
        } else {
            top = rect.bottom + 2;
            $dd.removeClass('drop-up');
            if (top + menuH > window.innerHeight - 8) {
                $dd.css('max-height', Math.max(120, window.innerHeight - top - 8) + 'px');
            }
        }
        $dd.css({ top: top, left: left, visibility: 'visible' });
    },

    showDropdown: function ($wrap, html) {
        _obdToken.closeDropdown();
        var vehicleId = $wrap.attr('data-vehicle-id');
        var $dd = $('<div class="obd-token__dropdown is-portal"></div>')
            .attr('data-vehicle-id', vehicleId)
            .html(html)
            .appendTo('body');
        _obdToken.positionDropdown($wrap, $dd);
        var reposition = function () {
            if (!$dd.closest('body').length || !$wrap.closest('body').length) {
                _obdToken.closeDropdown();
                return;
            }
            _obdToken.positionDropdown($wrap, $dd);
        };
        $(window).on('scroll.obdSuggest resize.obdSuggest', reposition);
        _obdToken._obdScrollParents = $wrap.parents();
        _obdToken._obdScrollParents.on('scroll.obdSuggest', reposition);
    },

    bindEvents: function () {
        if (_obdToken._obdBound) return;
        _obdToken._obdBound = true;

        $(document).on('input', '.obd-token__input', function () {
            var $input = $(this);
            var raw = $.trim($input.val()).replace(/\s+/g, '');
            $input.val(raw.replace(/[^\d]/g, ''));
            _obdToken.syncClearBtn($input);
            clearTimeout(_obdToken._obdSuggestTimer);
            _obdToken._obdSuggestTimer = setTimeout(function () {
                _obdToken.suggest($input);
            }, 250);
        });

        $(document).on('focus', '.obd-token__input', function () {
            _obdToken.syncClearBtn($(this));
            _obdToken.suggest($(this));
        });

        $(document).on('click', '.obd-token__input-clear', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var $wrap = $(this).closest('.obd-token');
            var $input = $wrap.find('.obd-token__input');
            $input.val('').focus();
            $(this).hide();
            _obdToken.closeDropdown();
            _obdToken.suggest($input);
        });

        $(document).on('keydown', '.obd-token__input', function (e) {
            var $wrap = $(this).closest('.obd-token');
            var $dd = _obdToken.getPortalDropdown($wrap);
            var $items = $dd.find('.obd-token__dropdown-item, .obd-token__dropdown-create');
            var $active = $items.filter('.is-active');
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();
                if (!$items.length) return;
                var idx = $items.index($active);
                $items.removeClass('is-active');
                if (e.key === 'ArrowDown') idx = idx < $items.length - 1 ? idx + 1 : 0;
                else idx = idx > 0 ? idx - 1 : $items.length - 1;
                $items.eq(idx).addClass('is-active');
                return;
            }
            if (e.key === 'Enter') {
                e.preventDefault();
                if ($active.length) $active.trigger('click');
                else _obdToken.commitInput($wrap);
                return;
            }
            if (e.key === 'Escape') _obdToken.closeDropdown();
        });

        $(document).on('click', '.obd-token__dropdown-item', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var $item = $(this);
            var vehicleId = $item.closest('.obd-token__dropdown').attr('data-vehicle-id')
                || $item.closest('.obd-token').attr('data-vehicle-id');
            var $wrap = $('.obd-token[data-vehicle-id="' + vehicleId + '"]').first();
            if (!$wrap.length) return;
            _obdToken.attach($wrap, parseInt($item.attr('data-obd-id'), 10), $item.attr('data-delivery'), $item.attr('data-net-weight'));
        });

        $(document).on('click', '.obd-token__dropdown-create', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var $el = $(this);
            var vehicleId = $el.closest('.obd-token__dropdown').attr('data-vehicle-id')
                || $el.closest('.obd-token').attr('data-vehicle-id');
            var $wrap = $('.obd-token[data-vehicle-id="' + vehicleId + '"]').first();
            if (!$wrap.length) return;
            _obdToken.attach($wrap, 0, $el.attr('data-delivery'), '0');
        });

        $(document).on('click', '.obd-token__chip-remove', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var $chip = $(this).closest('.obd-token__chip');
            var $wrap = $(this).closest('.obd-token');
            _obdToken.detach($wrap, parseInt($chip.attr('data-obd-id'), 10), $chip);
        });

        $(document).on('click', function (e) {
            if ($(e.target).closest('.obd-token, .obd-token__dropdown').length) return;
            _obdToken.closeDropdown();
        });
    },

    suggest: function ($input) {
        var $wrap = $input.closest('.obd-token');
        var keyword = $.trim($input.val());
        var selectedDelivery = {};
        $wrap.find('.obd-token__chip').each(function () {
            selectedDelivery[String($(this).attr('data-delivery') || $(this).find('.obd-token__chip-text').text())] = true;
        });

        $.ajax({
            url: '/Car/SuggestObd',
            type: 'GET',
            data: { keyword: keyword },
            success: function (result) {
                if (!$input.is(':focus')) return;
                if (result.status != 0) {
                    _obdToken.closeDropdown();
                    return;
                }
                var rows = result.data || [];
                var html = '<div class="obd-token__dropdown-head"><span>OBD</span><span>Net Weight</span><span>Loading Date</span></div>';
                var hasVisible = false;
                var exactMatch = false;
                var alreadyOnVehicle = !!selectedDelivery[keyword];
                for (var i = 0; i < rows.length; i++) {
                    var row = rows[i];
                    if (String(row.delivery) === keyword) exactMatch = true;
                    if (selectedDelivery[String(row.delivery)]) continue;
                    hasVisible = true;
                    html += '<div class="obd-token__dropdown-item" data-obd-id="' + row.obdId + '" data-delivery="' + row.delivery + '" data-net-weight="' + (row.netWeight != null ? row.netWeight : '0') + '">'
                        + '<span>' + row.delivery + '</span>'
                        + '<span>' + (row.netWeight != null ? row.netWeight : '') + '</span>'
                        + '<span>' + row.loadingDate + '</span></div>';
                }
                if (!hasVisible && !keyword) {
                    html += '<div class="obd-token__dropdown-empty">Không có mã OBD (version mới nhất)</div>';
                }
                if (keyword && alreadyOnVehicle) {
                    html += '<div class="obd-token__dropdown-empty">Mã OBD đã gắn trên xe này</div>';
                } else if (keyword && _obdToken.DELIVERY_REGEX.test(keyword) && !/^0+$/.test(keyword) && !exactMatch) {
                    html += '<div class="obd-token__dropdown-create" data-delivery="' + keyword + '">'
                        + '<button type="button" class="obd-token__create-btn">'
                        + '<span class="obd-token__create-icon" aria-hidden="true">+</span>'
                        + '<span class="obd-token__create-text">Tạo mã OBD mới</span>'
                        + '<span class="obd-token__create-code">' + keyword + '</span>'
                        + '</button></div>';
                } else if (keyword && (!_obdToken.DELIVERY_REGEX.test(keyword) || /^0+$/.test(keyword))) {
                    html += '<div class="obd-token__dropdown-empty">Mã OBD chỉ gồm chữ số, tối đa 18 số</div>';
                }
                _obdToken.showDropdown($wrap, html);
            }
        });
    },

    commitInput: function ($wrap) {
        var delivery = $.trim($wrap.find('.obd-token__input').val());
        if (!delivery) return;
        if (!_obdToken.DELIVERY_REGEX.test(delivery) || /^0+$/.test(delivery)) {
            _msgalert.error('Mã OBD chỉ gồm chữ số, tối đa 18 số');
            return;
        }
        var $exact = _obdToken.getPortalDropdown($wrap)
            .find('.obd-token__dropdown-item[data-delivery="' + delivery + '"]').first();
        if ($exact.length) {
            _obdToken.attach($wrap, parseInt($exact.attr('data-obd-id'), 10), delivery, $exact.attr('data-net-weight'));
        } else {
            _obdToken.attach($wrap, 0, delivery, '0');
        }
    },

    attach: function ($wrap, obdId, delivery, netWeight) {
        var vehicleId = parseInt($wrap.attr('data-vehicle-id'), 10);
        if (!vehicleId) return;
        if ($wrap.find('.obd-token__chip[data-delivery="' + delivery + '"]').length) {
            $wrap.find('.obd-token__input').val('');
            _obdToken.syncClearBtn($wrap.find('.obd-token__input'));
            _obdToken.closeDropdown();
            return;
        }
        $wrap.addClass('is-busy');
        $.ajax({
            url: '/Car/AttachVehicleObd',
            type: 'POST',
            data: { vehicleInspectionId: vehicleId, obdId: obdId || 0, delivery: delivery },
            success: function (result) {
                $wrap.removeClass('is-busy');
                if (result.status == 0 && result.data) {
                    var nw = result.data.netWeight != null ? result.data.netWeight : (netWeight || '0');
                    $wrap.find('.obd-token__chips').append(
                        _obdToken.formatChip(result.data.obdId, result.data.delivery, nw)
                    );
                    _obdToken.syncTotal($wrap);
                    var $input = $wrap.find('.obd-token__input');
                    $input.val('');
                    _obdToken.syncClearBtn($input);
                    _obdToken.closeDropdown();
                } else {
                    _msgalert.error(result.msg || 'Không gắn được OBD');
                }
            },
            error: function () {
                $wrap.removeClass('is-busy');
                _msgalert.error('Không gắn được OBD, vui lòng thử lại');
            }
        });
    },

    detach: function ($wrap, obdId, $chip) {
        var vehicleId = parseInt($wrap.attr('data-vehicle-id'), 10);
        if (!vehicleId || !obdId) return;
        $wrap.addClass('is-busy');
        $.ajax({
            url: '/Car/DetachVehicleObd',
            type: 'POST',
            data: { vehicleInspectionId: vehicleId, obdId: obdId },
            success: function (result) {
                $wrap.removeClass('is-busy');
                if (result.status == 0) {
                    $chip.remove();
                    _obdToken.syncTotal($wrap);
                } else {
                    _msgalert.error(result.msg || 'Không gỡ được OBD');
                }
            },
            error: function () {
                $wrap.removeClass('is-busy');
                _msgalert.error('Không gỡ được OBD, vui lòng thử lại');
            }
        });
    }
};
