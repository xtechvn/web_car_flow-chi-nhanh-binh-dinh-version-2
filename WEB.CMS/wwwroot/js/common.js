(function ($) {
    "use strict";
    var iOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
    var isMobile = {
        Android: function () {
            return navigator.userAgent.match(/Android/i);
        },
        BlackBerry: function () {
            return navigator.userAgent.match(/BlackBerry/i);
        },
        iOS: function () {
            return navigator.userAgent.match(/iPhone|iPad|iPod/i);
        },
        Opera: function () {
            return navigator.userAgent.match(/Opera Mini/i);
        },
        Windows: function () {
            return navigator.userAgent.match(/IEMobile/i);
        },
        any: function () {
            return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
        }
    }

    //backToTop
    function backToTop() {
        $(window).scroll(function () {
            if ($(window).scrollTop() >= 200) {
                $('#to_top').fadeIn();
            } else {
                $('#to_top').fadeOut();
            }
        });

        $("#to_top").click(function () {
            $("html, body").animate({
                scrollTop: 0
            });
            return false;
        });
    }

    //resizeSite
    function resizeSite() {
        var heightVideo = $('#player_playing').height() - 64;
        $('.detail_right .scrollbar-inner').height(heightVideo);
    }

    //fixSticky
    function fixStickyIE() {
        var stickyElements = $('.sticky');
        if (stickyElements.length > 0) {
            Stickyfill.add(stickyElements);
        }
        $("<div id='box_menu_before'></div>").insertBefore(".section_top");
        $(window).scroll(function () {
            var top_start = $("#box_menu_before").position().top + 0;
            if ($(window).scrollTop() >= top_start) {
                $('.section_top').addClass('fixed');
            } else if ($(window).scrollTop() <= top_start) {

                $('.section_top').removeClass('fixed');
            }
        });

    }

    $(function () {
        backToTop();
        // fixStickyIE();
    });
    $(window).on('load resize', function () {
        resizeSite()
    });
})(jQuery);