var VcselData = function () {
    var vcselshipdppm = function ()
    {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        function searchdata() {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/VCSEL/VCSELDppmDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();
                if (output.success) {
                    $('.v-content').empty();
                    $('.v-content').append('<div class="col-xs-12"><span class="mysptooltip" title="This is my span tooltip message!"></span></div>');

                    var appendstr = "";

                    $.each(output.shipdataarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawshipdppm(val);
                    })

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

    var myrmatable = null;

    var showvcesldata = function (event, col_data) {
        var datestr = event.point.category;
        var rate = col_data.rate
        //$('#waferval').html(datestr);

        $.post('/Shipment/RetrieveVcselRMARawDataByMonth',
            {
                datestr: datestr,
                rate: rate
            },
            function (outputdata) {
                if (myrmatable) {
                    myrmatable.destroy();
                }

                $('#ramrawhead').empty();
                $('#ramrawbody').empty();

                var appendstr = '<tr>' +
                                '<th>&nbsp;</th>' +
                                '<th>SN</th>' +
                                '<th>PN</th>' +
                                '<th>Wafer</th>' +
                                '<th>VcselType</th>' +
                                '<th>ProductType</th>' +
                                '<th>Ship Date</th>' +
                                '<th>Open Date</th>' +
                                '<th>Customer</th>' +
                                '<th>Report</th>' +
                            '</tr>';
                $('#ramrawhead').append(appendstr);

                $.each(outputdata.waferdatalist, function (i, val) {
                    var rmalink = '<td> </td>';
                    if (val.IssueKey != '') {
                        rmalink = '<td><a href="http://wuxinpi.chn.ii-vi.net/Issue/UpdateIssue?issuekey=' + val.IssueKey + '" target="_blank" >Report</a></td>'
                    }
                    var waferlink = '<td> </td>';
                    if (val.Wafer != '') {
                        waferlink = '<td><a href="http://wuxinpi.chn.ii-vi.net/DataAnalyze/WaferDistribution?defaultwafer=' + val.Wafer + '" target="_blank" >' + val.Wafer + '</a></td>'
                    }
                    appendstr = '<tr>' +
                        '<td>' + (i + 1) + '</td>' +
                        '<td>' + val.SN + '</td>' +
                        '<td>' + val.PN + '</td>' +
                        waferlink +
                        '<td>' + val.VcselType + '</td>' +
                        '<td>' + val.ProductType + '</td>' +
                        '<td>' + val.ShipDate + '</td>' +
                        '<td>' + val.RMAOpenDate + '</td>' +
                        '<td>' + val.Customer + '</td>' +
                        rmalink
                        + '</tr>';
                    $('#ramrawbody').append(appendstr);
                });

                myrmatable = $('#myrmatable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('#shipdistributiondetaillabel').html('Parallel' + ' ' + rate + ' ' + datestr + ' VCSEL RMA Summary');
                $('#rmarawdata').modal('show')
            })
    }

    var eventfunction = function (chart, legend, i, col_data) {
        var item = legend.allItems[i].legendItem;
        item.on('mouseover', function (e) {
            if (col_data.customerrate[i] != '') {
                $('.mysptooltip').attr('style', 'position: fixed;top:' + e.y + 'px;left:' + e.x + 'px;')
                $('.mysptooltip').tooltipster({});
                $('.mysptooltip').tooltipster('content', col_data.customerrate[i]);
                $('.mysptooltip').tooltipster('open');
            }
        }).on('mouseout', function (e) {
            if (col_data.customerrate[i] != '') {
                $('.mysptooltip').tooltipster('close');
                $('.mysptooltip').tooltipster('destroy');
            }
        }).on('click', function (e) {
            var legendname = e.target.innerHTML;
            if (legendname.indexOf('Total Shipment') != -1) {
                var slength = chart.series.length;
                var totalshipvisible = false;
                for (var idx = 0; idx < slength; idx++) {
                    if (chart.series[idx].name.indexOf('Total Shipment') != -1) {
                        if (chart.series[idx].visible) {
                            totalshipvisible = true;
                        }
                        else {
                            totalshipvisible = false;
                        }
                    }
                }

                if (totalshipvisible) {
                    //for (var idx = 0; idx < slength; idx++) {
                    //    if (chart.series[idx].name.indexOf('Total Shipment') == -1) {
                    //        chart.series[idx].update({ visible: true });
                    //    }
                    //    if (chart.series[idx].name.indexOf('DPPM') != -1) {
                    //        chart.series[idx].update({ visible: true });
                    //    }
                    //}
                }
                else {
                    for (var idx = 0; idx < slength; idx++) {
                        if (chart.series[idx].name.indexOf('Total Shipment') == -1) {
                            chart.series[idx].update({ visible: false });
                        }
                        if (chart.series[idx].name.indexOf('DPPM') != -1) {
                            chart.series[idx].update({ visible: true });
                        }
                    }
                }

            }
            else {
                if (legendname.indexOf('DPPM') == -1) {
                    var slength = chart.series.length;
                    for (var idx = 0; idx < slength; idx++) {
                        if (chart.series[idx].name.indexOf('Total Shipment') != -1) {
                            chart.series[idx].update({ visible: false });
                        }
                    }
                }
            }
        });
    }

    var drawshipdppm = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'column',
                events: {
                    load: function () {
                        var chart = this,
                            legend = chart.legend;

                        for (var i = 0, len = legend.allItems.length; i < len; i++) {
                            (function (i) {
                                eventfunction(chart, legend, i, col_data);
                            })(i);
                        }
                    },
                    redraw: function () {
                        var chart = this,
                            legend = chart.legend;

                        for (var i = 0, len = legend.allItems.length; i < len; i++) {
                            (function (i) {
                                eventfunction(chart, legend, i, col_data);
                            })(i);
                        }
                    }
                }
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                categories: col_data.xAxis.data
            },
            legend: {
                enabled: true,
            },
            yAxis: [{
                title: {
                    text: col_data.yAxis.title
                },
                stackLabels: {
                    enabled: true,
                    style: {
                        fontWeight: 'bold',
                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                    }
                }
            }, {
                opposite: true,
                title: {
                    text: 'VCSEL RMA DPPM'
                }
            }],
            tooltip: {
                headerFormat: '',
                pointFormatter: function () {
                    return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                },
                shared: true
            },
            plotOptions: {
                column: {
                    stacking: 'normal'
                },
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            if (event.point.series.name.indexOf('VCSEL RMA DPPM') != -1) {
                                showvcesldata(event, col_data);
                            }
                        }
                    }
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
                    },
                    copycharts: {
                        onclick: function () {
                            var svg = this.getSVG({
                                chart: {
                                    width: this.chartWidth,
                                    height: this.chartHeight
                                }
                            });
                            var c = document.createElement('canvas');
                            c.width = this.chartWidth;
                            c.height = this.chartHeight;
                            canvg(c, svg);
                            var dataURL = c.toDataURL("image/png");
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(col_data.id, options);
    }






    var vcselwaferdppm = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        function searchdata() {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/VCSEL/VCSELWaferDppmData', {
                sdate: sdate,
                edate: edate
                }, function (output) {
                    $.bootstrapLoading.end();

                if (output.success) {
                    $('.v-content').empty();

                    var appendstr = "";
                    $.each(output.ratedataarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawwaferdppm(val);
                    })

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


    var drawwaferdppm = function (line_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xAxis.data,
                plotBands: line_data.plotbands
            },
            yAxis: [{
                title: {
                    text: line_data.yAxis.title
                },
                min: line_data.yAxis.min,
                max: line_data.yAxis.max,
            }, {
                opposite: true,
                title: {
                    text: 'Amount'
                }
            }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            var wafer = event.point.category;
                            $('#waferval').html(wafer);

                            $.post('/VCSEL/RetrieveVcselRMARawData',
                                {
                                    wafer: wafer
                                },
                                function (outputdata) {
                                    $('#ramrawbody').empty();
                                    $.each(outputdata.waferdatalist, function (i, val) {
                                        var rmalink = '<td> </td>';
                                        if (val.IssueKey != '') {
                                            rmalink = '<td><a href="http://wuxinpi.chn.ii-vi.net/Issue/UpdateIssue?issuekey=' + val.IssueKey + '" target="_blank" >Report</a></td>'
                                        }

                                        var appendstr = '<tr>' +
                                            '<td>' + (i + 1) + '</td>' +
                                            '<td>' + val.SN + '</td>' +
                                            '<td>' + val.PN + '</td>' +
                                            '<td>' + val.VcselType + '</td>' +
                                            '<td>' + val.ProductType + '</td>' +
                                            '<td>' + val.ShipDate + '</td>' +
                                            '<td>' + val.RMAOpenDate + '</td>' +
                                            '<td>' + val.Customer + '</td>' +
                                            rmalink
                                            + '</tr>';
                                        $('#ramrawbody').append(appendstr);
                                    });
                                    $('#rmarawdata').modal('show')
                                })
                        }
                    }
                }
            },
            series: [{
                name: line_data.data.cdata.name,
                color: '#0053a2',
                type: 'column',
                data: line_data.data.cdata.data,
                yAxis: 1
            }, {
                name: line_data.data.data.name,
                    color:'#81171b',
                dataLabels: {
                    enabled: false,
                    color: line_data.data.data.color,
                },
                marker: {
                    radius: 4
                },
                enableMouseTracking: true,
                data: line_data.data.data.data
            }],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + line_data.id).parent().toggleClass('chart-modal');
                            $('#' + line_data.id).highcharts().reflow();
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
                    },
                    copycharts: {
                        onclick: function () {
                            var svg = this.getSVG({
                                chart: {
                                    width: this.chartWidth,
                                    height: this.chartHeight
                                }
                            });
                            var c = document.createElement('canvas');
                            c.width = this.chartWidth;
                            c.height = this.chartHeight;
                            canvg(c, svg);
                            var dataURL = c.toDataURL("image/png");
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };

        Highcharts.chart(line_data.id, options);
    }




    var vcselstatistic = function ()
    {
        $(function () {
            
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/VCSEL/VCSELStatisticData', {}, function (output) {
                $.bootstrapLoading.end();

                if (output.success) {
                    $('.v-content').empty();

                    var appendstr = '<div class="col-xs-12">' +
                                   '<div class="v-box" id="' + output.vcseltechdata.id + '"></div>' +
                                   '</div>';
                    $('.v-content').append(appendstr);
                    drawvcseltechdppm(output.vcseltechdata);

                    appendstr = '<div class="col-xs-12">' +
                                   '<div class="v-box" id="' + output.vcselarraydata.id + '"></div>' +
                                   '</div>';
                    $('.v-content').append(appendstr);
                    drawvcseltechdppm(output.vcselarraydata);

                    appendstr = '<div class="col-xs-12">' +
                                   '<div class="v-box" id="' + output.shipdatedata.id + '"></div>' +
                                   '</div>';
                    $('.v-content').append(appendstr);
                    drawramdateline(output.shipdatedata);
                
                    appendstr = '<div class="col-xs-12">' +
                                   '<div class="v-box" id="' + output.accumulatedata.id + '"></div>' +
                                   '</div>';
                    $('.v-content').append(appendstr);
                    drawramdateline(output.accumulatedata);

                    appendstr = '<div class="col-xs-12">' +
                                   '<div class="v-box" id="' + output.vcsel_milestone.id + '"></div>' +
                                   '</div>';
                    $('.v-content').append(appendstr);
                    drawmilestonecolumn(output.vcsel_milestone);
                }
            });

        });

    }

    var drawvcseltechdppm = function (line_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xlist
            },
            yAxis: [{
                title: {
                    text: "DPPM"
                }
            }, {
                opposite: true,
                title: {
                    text: 'SHIPMENT'
                }
            }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                        }
                    }
                }
            },
            series: line_data.series,
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + line_data.id).parent().toggleClass('chart-modal');
                            $('#' + line_data.id).highcharts().reflow();
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
                    },
                    copycharts: {
                        onclick: function () {
                            var svg = this.getSVG({
                                chart: {
                                    width: this.chartWidth,
                                    height: this.chartHeight
                                }
                            });
                            var c = document.createElement('canvas');
                            c.width = this.chartWidth;
                            c.height = this.chartHeight;
                            canvg(c, svg);
                            var dataURL = c.toDataURL("image/png");
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };

        Highcharts.chart(line_data.id, options);
    }

    var drawramdateline = function (line_data) {
        var options = {
            chart: {
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xaxis
            },
            yAxis: {
                title: {
                    text: 'Failures'
                }
            },
            series: line_data.data,
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
                            var outputCSV = 'xAxis\r\n';
                            $(line_data.xaxis).each(function (i, val) {
                                outputCSV += val + ",";
                            })
                            outputCSV += "\r\n";
                            $(line_data.data).each(function (i, val) {
                                outputCSV += val.name + "\r\n";
                                $(val.data).each(function (i, sval) {
                                    outputCSV += sval + ",";
                                })
                                outputCSV += "\r\n";
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
                    },
                    copycharts: {
                        onclick: function () {
                            var svg = this.getSVG({
                                chart: {
                                    width: this.chartWidth,
                                    height: this.chartHeight
                                }
                            });
                            var c = document.createElement('canvas');
                            c.width = this.chartWidth;
                            c.height = this.chartHeight;
                            canvg(c, svg);
                            var dataURL = c.toDataURL("image/png");
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(line_data.id, options);
    }

    var drawmilestonecolumn = function (col_data) {
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
                enabled: true,
            },
            yAxis: [{
                min: col_data.yAxis.min,
                max: col_data.yAxis.max,
                title: {
                    text: col_data.yAxis.title
                },
                plotLines: [{
                    value: col_data.time.data,
                    color: col_data.time.color,
                    dashStyle: col_data.time.style,
                    width: 1
                }]
            }],
            tooltip: {
                shared: false
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
                            var outputCSV = 'Date,Type,Amount\r\n';
                            var milestonecsv = 'Date,Milestone\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.name == 'Milestone') {
                                        $(this.data).each(function () {
                                            if (val == col_data.xAxis.data[this.x]) {
                                                milestonecsv += val + "," + this.name.replace(',', ';').replace('<br/>', ' / ') + "\r\n";
                                            }
                                        })
                                    }
                                    else {
                                        if (this.data[i] != 0) {
                                            outputCSV += val + "," + this.name + "," + this.data[i] + "\r\n";
                                        }
                                    }
                                });
                            })
                            outputCSV += "\r\n\r\n" + milestonecsv;
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
                    },
                    copycharts: {
                        onclick: function () {
                            var svg = this.getSVG({
                                chart: {
                                    width: this.chartWidth,
                                    height: this.chartHeight
                                }
                            });
                            var c = document.createElement('canvas');
                            c.width = this.chartWidth;
                            c.height = this.chartHeight;
                            canvg(c, svg);
                            var dataURL = c.toDataURL("image/png");
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(col_data.id, options);
    }




    return {
        SHIPDPPMINIT: function () {
            vcselshipdppm();
        },
        WAFERDPPMINIT: function () {
            vcselwaferdppm();
        },
        STATISTICINIT: function () {
            vcselstatistic();
        },
    }
}();