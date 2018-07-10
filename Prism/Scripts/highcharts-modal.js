var HighChartModal = function () {
    var common = function () {
        $('body').on('click', '.chart-modal', function (e) {
            var cname = ' ' + e.target.className;
            if (cname.indexOf('chart-modal') !== -1) {
                $(this).removeClass('chart-modal');
                $(this).children().eq(0).highcharts().reflow();
            }
        });
    }
    return {
        init: function () {
            common();
        }
    };
}();