var HPU = function () {
    
    var departmenthpu = function () {
        var hputable = null;

        function searchdata() {
            var pdline = $.trim($('#productlines').val());
            var quarter = $.trim($('#quarterlist').val());

            $.post('/DataAnalyze/DepartmentHPUData', {
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

                        var hpucode = line.HPUCode;
                        if (line.DetailLink != '')
                        {
                            hpucode = '<a href="/DataAnalyze/PNHPU?PNLink=' + line.DetailLink + '" target="_blank">' + hpucode + '</a>';
                        }

                        $("#hpumaintableid").append('<tr>'+
                            '<td>' + line.HPUOrder + '</td>' +
                            '<td>' + hpucode + '</td>' +
                            '<td>' + line.ProductLine + '</td>' +
                            '<td>' + line.Serial + '</td>' +
                            '<td>' + line.Phase + '</td>' +
                            '<td>' + line.TypicalPN + '</td>' +
                            //'<td>' + line.WorkingHourMeasure + '</td>' +
                            //'<td>' + line.WorkingHourCollect + '</td>' +
                            //'<td>' + line.WorkingHourChecked + '</td>' +
                            '<td>' + line.YieldHPU + '</td>' +
                            '<td>' + line.Owner + '</td>' +
                            '<td>' + line.UpdateDate + '</td>' +
                            //'<td>' + line.FormMake + '</td>' +
                            '<td>' + line.Remark + '</td>' +
                            + '</tr>');
                    }


                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })
    }

    var hputrend = function () {
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

            $.post('/DataAnalyze/HPUTrendData', {
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

        function defaultsearch()
        {
            var serial = $.trim($('#pd_serial').tagsinput('items'));
            if (serial == '') {
                serial = $.trim($('#pd_serial').parent().find('input').eq(0).val());
            }
            if (serial != '')
            {
                searchdata();
            }
        }

        $('body').on('click', '#editreport', function () {
            var reportid = $.trim($('#reportid').val());
            $('#report-alert').modal('hide');
            window.open("/DataAnalyze/ModifyReport?" + "reportid=" + reportid);
        })
    }


    var searialhpu = function () {
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

            $.post('/DataAnalyze/SerialHPUData', {
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
                            '<td>' + line.YieldHPU + '</td>' +
                            '<td>' + line.Remark + '</td>' +
                            + '</tr>');
                    }


                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
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


    var pnhpu = function () {

        var hputable = null;

        $.post('/DataAnalyze/PNHPUPNLinkList', {}, function (output) {
            $('#pnlink').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.data;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });

            defaultsearch();
        });

        function searchdata()
        {
            var pnlink = $.trim($('#pnlink').val());
            if (pnlink == '') {
                alert("PN need to be input!");
                return false;
            }
            
            $.post('/DataAnalyze/GetPNHPUData', {
                pnlink: pnlink
            }, function (output) {
                if (output.success) {
                    
                    if (hputable) {
                        hputable.destroy();
                    }
                    $("#hpuheadid").empty();
                    $("#hpumaintableid").empty();

                    if (output.data.length == 0)
                    { return false; }

                    var appendstr = '<tr>';
                    $.each(output.title,function (idx, v) {
                        appendstr +='<th>'+v+'</th>'
                    })
                    appendstr += '</tr>';
                    $("#hpuheadid").append(appendstr);

                    appendstr = '';
                    $.each(output.data, function (idx, va)
                    {
                        appendstr += '<tr>';
                        $.each(va, function (i, v){
                            appendstr += '<td>'+v+'</td>';
                        })
                        appendstr += '</tr>';
                    })
                    $("#hpumaintableid").append(appendstr);

                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        function defaultsearch()
        {
            var pnlink = $.trim($('#pnlink').val());
            if (pnlink != '')
            {
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
            yAxis: [{
                min: 0,
                max: line_data.maxhpu,
                title: {
                    text: 'YIELD HPU'
                }
            }, {
                opposite: true,
                min: line_data.minhpureduction,
                max: line_data.maxhpureduction,
                title: {
                    text: 'HPU Reduction (%)'
                },
                plotLines: [{
                    value: line_data.hpuguideline.data,
                    color: line_data.hpuguideline.color,
                    dashStyle: line_data.hpuguideline.style,
                    width: 2,
                    label: {
                        text: line_data.hpuguideline.name + ':' + line_data.hpuguideline.data,
                        align: 'left'
                    }
                }]
            }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            $("#reportid").val(line_data.id +'_'+ event.point.category);
                            $.post('/DataAnalyze/RetrieveReport', {
                                reportid: line_data.id +'_'+ event.point.category,
                                reporttype: 'HPU'
                            }, function (output) {

                                if (line_data.url != '') {
                                    $("#serialhpu").attr('href', line_data.url);
                                }

                                if (output.success) {
                                    $('#rc-info').html(output.report.content);
                                    $('#rc-reporter').html(output.report.reporter);
                                    $('#rc-datetime').html(output.report.time);
                                }
                                $('#report-alert').modal('show');
                            });

                        }
                    }
                },
                column: {
                    colorByPoint: true
                }
            },
            colors : line_data.columncolors,
            series: [
                {
                    name: line_data.yieldhpu.name,
                    type: 'line',
                    data: line_data.yieldhpu.data,
                    yAxis: 0
                },
                {
                    name: line_data.hpureduction.name,
                    type: 'column',
                    data: line_data.hpureduction.data,
                    yAxis: 1
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
            departmenthpu();
        },
        TRENDINIT: function () {
            hputrend();
        },
        SERIALINIT: function ()
        {
            searialhpu();
        },
        PNHPUINIT: function ()
        {
            pnhpu();
        },
    }
}();