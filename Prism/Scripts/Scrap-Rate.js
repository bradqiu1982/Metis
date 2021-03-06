﻿var SCRAPRATE = function () {

    
    var departmentscrap = function () {
        function searchdata() {
            var fyear = $.trim($('#fyearlist').val());
            var fquarter = $.trim($('#fquarterlist').val());
            var department = $.trim($('#department').val());

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Scrap/DepartmentScrapRateData', {
                fyear: fyear,
                fquarter: fquarter,
                department: department
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.scrapratearray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawline(val,false);
                    })
                    $.bootstrapLoading.end();
                    //setTimeout(function () {
                    //    $('#loadcomplete').html('TRUE');
                    //}, 10000);
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });
    }

    var costcentscrap = function () {
        //$('.date').datepicker({ autoclose: true, viewMode: "years", minViewMode: "years" });

        $.post('/Scrap/CostCenterAutoCompelete', {}, function (output) {
            $('.project-no').tagsinput({
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
            var fyear = $.trim($('#fyearlist').val());
            var fquarter = $.trim($('#fquarterlist').val());

            var pj_no = $.trim($('#pj-no').tagsinput('items'));
            if (pj_no == '') {
                pj_no = $.trim($('#pj-no').parent().find('input').eq(0).val());
            }

            if (pj_no == '') {
                alert("Please input project code query condition.");
                return false;
            }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Scrap/CostCenterScrapRateData', {
                fyear: fyear,
                fquarter: fquarter,
                costcenter: pj_no
            }, function (output) {
                 if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.scrapratearray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawline(val,true);
                    })

                    $.bootstrapLoading.end();

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
            var pj_no = $.trim($('#pj-no').tagsinput('items'));
            if (pj_no == '') {
                pj_no = $.trim($('#pj-no').parent().find('input').eq(0).val());
            }
            if (pj_no != '') {
                searchdata();
            }
        }

        $('body').on('click', '#editreport', function () {
            var reportid = $.trim($('#reportid').val());
            $('#report-alert').modal('hide');
            window.open("/DataAnalyze/ModifyReport?" + "reportid=" + reportid);
        })
    }

    var productscrap = function () {

        $.post('/Scrap/CostCenterAutoCompelete', {}, function (output) {
            $('.project-no').tagsinput({
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
            var fyear = $.trim($('#fyearlist').val());
            var fquarter = $.trim($('#fquarterlist').val());

            var pj_no = $.trim($('#pj-no').tagsinput('items'));
            if (pj_no == '') {
                pj_no = $.trim($('#pj-no').parent().find('input').eq(0).val());
            }

            if (pj_no == '') {
                alert("Please input project code query condition.");
                return false;
            }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Scrap/ProductScrapRateData', {
                fyear: fyear,
                fquarter: fquarter,
                costcenter: pj_no
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.scrapratearray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawline(val,true);
                    })

                    $.bootstrapLoading.end();

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
            var pj_no = $.trim($('#pj-no').tagsinput('items'));
            if (pj_no == '') {
                pj_no = $.trim($('#pj-no').parent().find('input').eq(0).val());
            }
            if (pj_no != '') {
                searchdata();
            }
        }

        $('body').on('click', '#editreport', function () {
            var reportid = $.trim($('#reportid').val());
            $('#report-alert').modal('hide');
            window.open("/DataAnalyze/ModifyReport?" + "reportid=" + reportid);
        })

    }

    var scraptrend = function () {

        function searchdata() {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            var defpd = $('#defpd').val();
            $.post('/Scrap/ScrapTrendData',
                {
                    defpd:defpd
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.scrapratearray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawline(val,false);
                    })

                    $.bootstrapLoading.end();

                    //setTimeout(function () {
                    //    $('#loadcomplete').html('TRUE');
                    //}, 10000);
                }
            })
        }

        $(function () {
            searchdata();
        });

        $('body').on('click', '#editreport', function () {
            var reportid = $.trim($('#reportid').val());
            $('#report-alert').modal('hide');
            window.open("/DataAnalyze/ModifyReport?" + "reportid=" + reportid);
        })

    }

    var drawline = function (line_data, withreport) {
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
            yAxis: [{
                min: line_data.minYrate,
                max: line_data.maxYrate,
                title: {
                    text: 'Scrap Rate (%)'
                },
                plotLines: [{
                    value: line_data.bugetscraprate.data,
                    color: line_data.bugetscraprate.color,
                    dashStyle: line_data.bugetscraprate.style,
                    width: 2,
                    label: {
                        text: 'Buget Scrap Rate:' + line_data.bugetscraprate.data,
                        align: 'left'
                    }
                }]
            }, {
                opposite: true,
                title: {
                    text: 'Amount'
                },
                plotLines: [{
                    value: line_data.bugetscrapval.data,
                    color: line_data.bugetscrapval.color,
                    dashStyle: line_data.bugetscrapval.style,
                    width: 2,
                    label: {
                        text: 'Buget Scrap:' + line_data.bugetscrapval.data,
                        align: 'left'
                    }
                }]
            }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            if (withreport) {
                                $("#reportid").val(line_data.id);
                                $.post('/DataAnalyze/RetrieveReport', {
                                    reportid: line_data.id,
                                    reporttype: 'SCRAP'
                                }, function (output) {

                                    if (line_data.url != '') {
                                        var fyear = $.trim($('#fyearlist').val());
                                        var fquarter = $.trim($('#fquarterlist').val());
                                        $("#productscrap").attr('href',line_data.url + event.point.category + '&defyear=' + fyear + '&defqrt=' + fquarter);
                                    }

                                    if (output.success) {
                                        $('#rc-info').html(output.report.content);
                                        $('#rc-reporter').html(output.report.reporter);
                                        $('#rc-datetime').html(output.report.time);
                                    }
                                    $('#report-alert').modal('show');
                                });
                            }
                            else {
                                    if (line_data.url != '')
                                    {
                                        var fyear = $.trim($('#fyearlist').val());
                                        var fquarter = $.trim($('#fquarterlist').val());
                                        window.open(line_data.url + encodeURIComponent(event.point.category) + '&defyear=' + fyear + '&defqrt=' + fquarter);
                                    }
                            }
                         }
                    }
                }
            },
            series: line_data.chartlist,
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
                            var filename = line_data.title + '.csv';
                            var outputCSV = 'Time,';
                            $(line_data.xAxis.data).each(function (i, val) {
                                outputCSV += val + "," ;
                            });
                            outputCSV += "\r\n";

                            $(line_data.chartlist).each(function (i, val) {
                                outputCSV += val.name + ',';
                                $(val.data).each(function (i, val1) {
                                    outputCSV += val1 + ",";
                                });
                                outputCSV += "\r\n";
                            });

                            var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                            $(exportLink).attr({
                                'download': filename,
                                'href': window.URL.createObjectURL(blobby),
                                'target': '_blank'
                            });
                            exportLink.click();
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


    var drawcolumn = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'column'
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                categories: col_data.xAxis.data
            },
            legend: {
                enabled: false,
            },
            yAxis: {
                min: col_data.yAxis.min,
                max: col_data.yAxis.max,
                title: {
                    text: col_data.yAxis.title
                }
            },
            tooltip: {
                headerFormat: '',
                pointFormatter: function () {
                    return (this.y == 0) ? '' : '<span style="color:' + this.color + '">' + this.name + '</span>: <b>' + ((col_data.coltype == 'percent') ? this.percentage : this.y) + '%</b><br/>';
                },
                shared: true
            },
            plotOptions: {
                column: {
                    stacking: col_data.coltype
                }
            },
            series: col_data.data,
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + col_data.id).parent().toggleClass('chart-modal');
                            $('#' + col_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
                    },
                    exportdata: {
                        onclick: function () {
                            var filename = col_data.title + '.csv';
                            var outputCSV = ' ,Failure Mode,Failure Percent\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.data[i].name != '' && this.data[i].y != 0) {
                                        outputCSV += val + "," + this.data[i].name + "," + this.data[i].y + ",\r\n";
                                    }
                                });
                            })
                            var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                            $(exportLink).attr({
                                'download': filename,
                                'href': window.URL.createObjectURL(blobby),
                                'target': '_blank'
                            });
                            exportLink.click();
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
        Highcharts.chart(col_data.id, options);
    }

    var drawboxplot = function (boxplot_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'boxplot'
            },

            title: {
                text: boxplot_data.title
            },

            legend: {
                enabled: false
            },

            xAxis: {
                categories: boxplot_data.xAxis.data,
                title: {
                    text: boxplot_data.xAxis.title
                }
            },

            yAxis: {
                title: {
                    text: boxplot_data.yAxis.title
                }
            },
            //plotOptions: {
            //    series: {
            //        cursor: 'pointer',
            //        events: {
            //            click: function (event) {
            //                $('#m-wf-no').val(event.point.category);
            //                $('#boxplot-alert').modal('show')
            //            }
            //        }
            //    }
            //},

            series: [{
                name: boxplot_data.data.name,
                data: boxplot_data.data.data,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            },
            {
                name: boxplot_data.line.name,
                color: boxplot_data.line.color,
                type: 'line',
                data: boxplot_data.line.data,
                lineWidth: boxplot_data.line.lineWidth
            },
            {
                name: 'Outlier',
                color: Highcharts.getOptions().colors[0],
                type: 'scatter',
                data: boxplot_data.outlierdata,
                marker: {
                    fillColor: Highcharts.getOptions().colors[0],
                    lineWidth: 1,
                    radius: 2,
                    lineColor: Highcharts.getOptions().colors[0]
                },
                tooltip: {
                    headerFormat: '',
                    pointFormat: "{point.y}"
                }
            }],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + boxplot_data.id).parent().toggleClass('chart-modal');
                            $('#' + boxplot_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
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
                        menuItems: ['fullscreen', 'datalabel', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(boxplot_data.id, options);
    }
    var drawdbboxplot = function (dbboxplot_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'boxplot'
            },

            title: {
                text: dbboxplot_data.title
            },

            legend: {
                enabled: false
            },

            xAxis: {
                categories: dbboxplot_data.xAxis.data,
                title: {
                    text: dbboxplot_data.xAxis.title
                }
            },

            yAxis: [{
                title: {
                    text: dbboxplot_data.left.yAxis.title
                }
            }, {
                opposite: true,
                title: {
                    text: dbboxplot_data.right.yAxis.title
                }

            }],
            //plotOptions: {
            //    series: {
            //        cursor: 'pointer',
            //        events: {
            //            click: function (event) {
            //                $('#m-wf-no').val(event.point.category);
            //                $('#boxplot-alert').modal('show')
            //            }
            //        }
            //    }
            //},

            series: [{
                name: dbboxplot_data.left.data.name,
                data: dbboxplot_data.left.data.data,
                color: dbboxplot_data.left.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            },
            {
                name: dbboxplot_data.left.line.name,
                color: dbboxplot_data.left.line.color,
                type: 'line',
                data: dbboxplot_data.left.line.data,
                lineWidth: dbboxplot_data.left.line.lineWidth
            },
            {
                name: dbboxplot_data.right.data.name,
                data: dbboxplot_data.right.data.data,
                color: dbboxplot_data.right.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                },
                yAxis: 1
            },
             {
                 name: dbboxplot_data.right.line.name,
                 color: dbboxplot_data.right.line.color,
                 type: 'line',
                 data: dbboxplot_data.right.line.data,
                 lineWidth: dbboxplot_data.right.line.lineWidth,
                 yAxis: 1
             }
            ],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + dbboxplot_data.id).parent().toggleClass('chart-modal');
                            $('#' + dbboxplot_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
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
                        menuItems: ['fullscreen', 'datalabel', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(dbboxplot_data.id, options);
    }
    return {
        COSTCENTINIT: function () {
            costcentscrap();
        },
        DEPARTMENTINIT: function () {
            departmentscrap();
        },
        PRODUCTINIT: function () {
            productscrap();
        },
        TRENDINIT: function () {
            scraptrend();
        }
    }
}();