﻿var CAPACITY = function () {

    var departmentcapacity = function () {
        var hputable = null;

        function searchdata() {
            var pdline = $.trim($('#productlines').val());
            var quarter = $.trim($('#quarterlist').val());

            $.post('/DataAnalyze/DepartmentCapacityData', {
                pdline: pdline,
                quarter: quarter
            }, function (output) {
                if (output.success) {
                    var idx = 0;
                    var datacont = output.data.length;

                    if (hputable) {
                        hputable.destroy();
                    }
                    $("#hpumaintableid").empty();

                    for (idx = 0; idx < datacont; idx++) {
                        var line = output.data[idx];

                        var pn = line.TypicalPN;
                        if (line.DetailLink != '') {
                            pn = '<a href="/DataAnalyze/PNHPU?PNLink=' + line.DetailLink + '" target="_blank">' + pn + '</a>';
                        }

                        $("#hpumaintableid").append('<tr>' +
                            '<td>' + pn + '</td>' +
                            '<td>' + line.ProductLine + '</td>' +
                            '<td>' + line.Serial + '</td>' +
                            '<td>' + line.Phase + '</td>' +
                            '<td>' + line.WeeklyCapacity + '</td>' +
                            '<td>' + line.SeasonCapacity + '</td>' +
                            '<td>' + line.YieldHPU + '</td>' +
                            '<td>' + line.Remark + '</td>' +
                            + '</tr>');
                    }


                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": []
                    });
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })
    }

    var capacitytrend = function () {
        $.post('/DataAnalyze/GetAllSerial', {}, function (output) {
            $('.pd_serial_cla').tagsinput({
                freeInput: false,
                typeahead: {
                    source: output.data,
                    minLength: 0,
                    showHintOnFocus: true,
                    autoSelect: false,
                    selectOnBlur: false,
                    changeInputOnSelect: false,
                    changeInputOnMove: false,
                    afterSelect: function (val) {
                        this.$element.val("");
                    }
                }
            });

            defaultsearch();
        });

        function searchdata() {
            var serial = $.trim($('#pd_serial').tagsinput('items'));
            if (serial == '') {
                serial = $.trim($('#pd_serial').parent().find('input').eq(0).val());
            }

            if (serial == '') {
                alert("Product serial need to be input!");
                return false;
            }

            $.post('/DataAnalyze/CapacityTrendData', {
                serial: serial
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.hpuarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawline(val);
                    })
                    //setTimeout(function () {
                    //    $('#loadcomplete').html('TRUE');
                    //}, 10000);
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        function defaultsearch() {
            var serial = $.trim($('#pd_serial').tagsinput('items'));
            if (serial == '') {
                serial = $.trim($('#pd_serial').parent().find('input').eq(0).val());
            }
            if (serial != '') {
                searchdata();
            }
        }
    }

    var searialcapacity = function () {
        var hputable = null;

        $.post('/DataAnalyze/GetAllSerial', {}, function (output) {
            $('.pd_serial_cla').tagsinput({
                freeInput: false,
                typeahead: {
                    source: output.data,
                    minLength: 0,
                    showHintOnFocus: true,
                    autoSelect: false,
                    selectOnBlur: false,
                    changeInputOnSelect: false,
                    changeInputOnMove: false,
                    afterSelect: function (val) {
                        this.$element.val("");
                    }
                }
            });
            defaultsearch();
        });

        function searchdata() {
            var serial = $.trim($('#pd_serial').tagsinput('items'));
            if (serial == '') {
                serial = $.trim($('#pd_serial').parent().find('input').eq(0).val());
            }

            if (serial == '') {
                alert("Product serial need to be input!");
                return false;
            }

            $.post('/DataAnalyze/SerialCapacityData', {
                serial: serial
            }, function (output) {
                if (output.success) {
                    var idx = 0;
                    var datacont = output.data.length;

                    if (hputable) {
                        hputable.destroy();
                    }
                    $("#hpumaintableid").empty();

                    for (idx = 0; idx < datacont; idx++) {
                        var line = output.data[idx];

                        var hpupn = line.TypicalPN;
                        if (line.DetailLink != '') {
                            hpupn = '<a href="/DataAnalyze/PNHPU?PNLink=' + line.DetailLink + '" target="_blank">' + hpupn + '</a>';
                        }

                        $("#hpumaintableid").append('<tr>' +
                            '<td>' + line.Quarter + '</td>' +
                            '<td>' + hpupn + '</td>' +
                            '<td>' + line.ProductLine + '</td>' +
                            '<td>' + line.Serial + '</td>' +
                            '<td>' + line.Phase + '</td>' +
                            '<td>' + line.WeeklyCapacity + '</td>' +
                            '<td>' + line.SeasonCapacity + '</td>' +
                            '<td>' + line.YieldHPU + '</td>' +
                            '<td>' + line.Remark + '</td>' +
                            + '</tr>');
                    }


                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": []
                    });
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        function defaultsearch() {
            var serial = $.trim($('#pd_serial').tagsinput('items'));
            if (serial == '') {
                serial = $.trim($('#pd_serial').parent().find('input').eq(0).val());
            }
            if (serial != '') {
                searchdata();
            }
        }
    }

    var drawline = function (line_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xAxis.data
            },
            yAxis: [
                {
                    min: 0,
                    max: line_data.maxcapacity,
                    title: {
                        text: 'CAPACITY'
                    }
                },
                {
                    opposite: true,
                    min: 0,
                    max: line_data.maxhpu,
                    title: {
                        text: 'YIELD HPU'
                    }
                }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            if (line_data.url != '') {
                                window.open(line_data.url);
                            }
                        }
                    }
                }
            },
            series: [
                {
                    name: line_data.yieldhpu.name,
                    type: 'line',
                    data: line_data.yieldhpu.data,
                    yAxis: 1
                },
                {
                    name: line_data.capacity.name,
                    type: 'column',
                    data: line_data.capacity.data,
                    yAxis: 0
                }
            ],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + line_data.id).parent().toggleClass('chart-modal');
                            $('#' + line_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
                    },
                    exportdata: {
                        onclick: function () {
                            //var filename = line_data.title + '.csv';
                            //var outputCSV = 'Time,';
                            //$(line_data.xAxis.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //outputCSV += line_data.generalscraprate.name + ',';
                            //$(line_data.generalscraprate.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //outputCSV += line_data.nonchinascraprate.name + ',';
                            //$(line_data.nonchinascraprate.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //outputCSV += line_data.nonchinascrap.name + ',';
                            //$(line_data.nonchinascrap.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //outputCSV += line_data.generalscrap.name + ',';
                            //$(line_data.generalscrap.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //outputCSV += line_data.output.name + ',';
                            //$(line_data.output.data).each(function (i, val) {
                            //    outputCSV += val + ",";
                            //});
                            //outputCSV += "\r\n";

                            //var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                            //$(exportLink).attr({
                            //    'download': filename,
                            //    'href': window.URL.createObjectURL(blobby),
                            //    'target': '_blank'
                            //});
                            //exportLink.click();
                        },
                        text: 'Export Data'
                    },
                    datalabel: {
                        onclick: function () {
                            var labelflag = !this.series[0].options.dataLabels.enabled;
                            $.each(this.series, function (idx, val) {
                                var opt = val.options;
                                opt.dataLabels.enabled = labelflag;
                                val.update(opt);
                            })
                        },
                        text: 'Data Label'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(line_data.id, options);
    }

    return {
        DEPARTMENTINIT: function () {
            departmentcapacity();
        },
        TRENDINIT: function () {
            capacitytrend();
        },
        SERIALINIT: function () {
            searialcapacity();
        },
    }
}();