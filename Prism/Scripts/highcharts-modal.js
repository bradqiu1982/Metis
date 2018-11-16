var HighChartModal = function () {
    var common = function () {
        $('body').on('click', '.chart-modal', function (e) {
            var cname = ' ' + e.target.className;
            if (cname.indexOf('chart-modal') !== -1) {
                $(this).removeClass('chart-modal');
                $(this).children().eq(0).highcharts().reflow();
            }
        });

        $('body').on('click', '.chart-modal-zoom', function (e) {
            var cname = ' ' + e.target.className;
            if (cname.indexOf('chart-modal-zoom') !== -1) {
                $(this).removeClass('chart-modal-zoom');
                $(this).children().eq(0).highcharts().yAxis[0].update({ visible: false });
                if ($(this).children().eq(0).highcharts().yAxis.length > 1)
                {
                    $(this).children().eq(0).highcharts().yAxis[1].update({ visible: false });
                }
                $(this).children().eq(0).highcharts().setSize(100, 80);
                $(this).children().eq(0).highcharts().xAxis[0].update({ visible: false });
                $(this).children().eq(0).highcharts().legend.update({ enabled: false });
                $(this).children().eq(0).highcharts().exporting.update({ enabled: false });
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