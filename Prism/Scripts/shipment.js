﻿var Shipment = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        function searchdata()
        {
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

            $.post('/Shipment/ShipmentDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();
                if (output.success) {
                    $('.v-content').empty();
                    $('.v-content').append('<div class="col-xs-12"><span class="mysptooltip" title="This is my span tooltip message!"></span></div>');

                    var appendstr = "";

                    $.each(output.shipdataarray, function (i, val) {
                        appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
                        $('.v-content').append(appendstr);
                        drawcolumn(val);
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

        $('body').on('click', '#btn-download', function () {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            var myurl = '/Shipment/DownloadShipmentData?sdate=' + sdate + '&edate=' + edate;
            window.open(myurl, '_blank');
        })

    }

    var otddata = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        function searchotddata()
        {
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

            $.post('/Shipment/OTDDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.otdarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawotdline(val);
                    })
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchotddata();
        })

        $(function () {
            searchotddata();
        });

    }

    var lbsdata = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        
        function searchlbsdata()
        {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            $.post('/Shipment/ShipmentLBSDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    console.log(output.chartarray);
                    var appendstr = "";

                    $.each(output.chartarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '" style="height: 500px;width: 900px;margin: 0 auto;"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawlbsdistribution(val);
                    })
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchlbsdata();
        })

        $(function(){
            searchlbsdata();
        });
    }

    var orderdata = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $('body').on('click', '#btn-search', function () {
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

            $.post('/Shipment/OrderDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();
                if (output.success) {
                    $('.v-content').empty();
                    $('.v-content').append('<div class="col-xs-12"><span class="mysptooltip" title="This is my span tooltip message!"></span></div>');

                    var appendstr = "";

                    $.each(output.orderdataarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawordercolumn(val);
                    })

                }
            })
        })

    }

    var rmaworkloaddata = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $('body').on('click', '#btn-search', function () {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            $.post('/Shipment/RMAWorkLoadData', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.chartarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawrmaworkloaddline(val);
                    })
                }
            })
        })

    }

    var shipoutput = function ()
    {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        function getshipoutdata()
        {
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

            $.post('/Shipment/ShipOutputTrendData',
                {
                    sdate: sdate,
                    edate: edate
                },
                function (output) {
                    $.bootstrapLoading.end();
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.chartlist, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawoutputcolumn(val);
                    })
                });
        }

        $(function () {
            getshipoutdata();
        });

        $('body').on('click', '#btn-search', function () {
            getshipoutdata();
        });

    }

    var myrmatable = null;

    var showvcesldata = function (event, col_data)
    {
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

    var showallrmadata = function (event, col_data) {
        var datestr = event.point.category;
        var pdtype = col_data.producttype;
        //$('#waferval').html(datestr);

        $.post('/Shipment/RetrieveRMARawDataByMonth',
            {
                datestr: datestr,
                pdtype: pdtype
            },
            function (outputdata) {
                if (myrmatable) {
                    myrmatable.destroy();
                }

                $('#ramrawhead').empty();
                $('#ramrawbody').empty();

                var appendstr = '<tr>' +
                                '<th>Sum Type</th>' +
                                '<th>Name</th>' +
                                '<th>QTY</th>' +
                                '<th>Rate</th>' +
                            '</tr>';
                $('#ramrawhead').append(appendstr);

                $.each(outputdata.rmadatalist, function (i, val) {
                    var appendstr = '<tr>' +
                        '<td>' + val.SumType + '</td>' +
                        '<td>' + val.RMAName + '</td>' +
                        '<td>' + val.RMAQty + '</td>' +
                        '<td>' + val.RMARate + '%</td>' +
                        '</tr>';
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

                $('#shipdistributiondetaillabel').html(pdtype + ' ' + datestr + ' RMA Summary');
                $('#rmarawdata').modal('show')
            })
    }

    var showshipoutdata4shipdistribution = function (event, col_data) {
        var qt = event.point.category;
        var dp = col_data.producttype;
        $.post('/Shipment/ShipOutputDetailData',
            {
                dp: dp,
                qt: qt
            },
            function (outputdata) {
                if (myrmatable) {
                    myrmatable.destroy();
                }
                $('#ramrawhead').empty();
                $('#ramrawbody').empty();

                var appendstr0 = '<tr>' +
                        '<th>Product</th>' +
                        '<th>QTY</th>' +
                        '<th>Output</th>' +
                        '</tr>';
                $('#ramrawhead').append(appendstr0);

                $.each(outputdata.shipoutlist, function (i, val) {
                    var appendstr = '<tr>' +
                        '<td>' + val.MarketFamily + '</td>' +
                        '<td>' + val.ShipQty + '</td>' +
                        '<td>' + val.Output + '</td>' +
                        '</tr>';
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
                $('#shipdistributiondetaillabel').html(dp + ' ' + qt + ' Ship Info');
                $('#rmarawdata').modal('show')
            });
    }

    var showworkloadrmadata = function (event, col_data) {
        var datestr = event.point.category;
        var pdtype = col_data.producttype
        $('#waferval').html(datestr);

        $.post('/Shipment/RetrieveRMAWorkLoadDataByMonth',
            {
                datestr: datestr,
                pdtype: pdtype
            },
            function (outputdata) {
                if (myrmatable) {
                    myrmatable.destroy();
                }

                $('#ramrawhead').empty();
                $('#ramrawbody').empty();

                var appendstr = '<tr>' +
                                '<th>&nbsp;</th>' +
                                '<th>RMA Num</th>' +
                                '<th>PN</th>' +
                                '<th>QTY</th>' +
                                '<th>Issue OPen</th>' +
                                '<th>Init FAR</th>' +
                                '<th>SN</th>' +
                                '<th>RootCause</th>' +
                            '</tr>';
                $('#ramrawhead').append(appendstr);

                $.each(outputdata.rmadatalist, function (i, val) {
                    var appendstr = '<tr>' +
                        '<td>' + (i + 1) + '</td>' +
                        '<td>' + val.RMANum + '</td>' +
                        '<td>' + val.PN + '</td>' +
                        '<td>' + val.QTY + '</td>' +
                        '<td>' + val.IssueDateStr + '</td>' +
                        '<td>' + val.InitFARStr + '</td>' +
                        '<td>' + val.SN + '</td>' +
                        '<td>' + val.RootCause + '</td>' +
                        '</tr>';
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

                $('#rmarawdata').modal('show')
            })
    }

    var myshipouttable = null;
    var showalloutdata = function (event) {
        var qt = event.point.series.name;
        var dp = event.point.category;
        $.post('/Shipment/AllOutputDetailData',
            {
                dp: dp,
                qt: qt
            },
            function (outputdata) {
                if (myshipouttable) {
                    myshipouttable.destroy();
                }
                $('#shipoutputheadid').empty();
                $('#shipoutputcontentid').empty();

                var appendstr0 = '<tr>' +
                        '<th>Product</th>' +
                        '<th>QTY</th>' +
                        '<th>Output</th>' +
                        '</tr>';
                $('#shipoutputheadid').append(appendstr0);

                $.each(outputdata.shipoutlist, function (i, val) {
                    var appendstr = '<tr>' +
                        '<td>' + val.PRODUCT + '</td>' +
                        '<td>' + val.PRIMARY_QUANTITY_1 + '</td>' +
                        '<td>' + val.Transaction_Value_Usd_1 + '</td>' +
                        '</tr>';
                    $('#shipoutputcontentid').append(appendstr);
                });

                myshipouttable = $('#shipoutdatatable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });
                $('#shipoutputmodalLabel').html(dp + ' ' + qt + ' Output Info');
                $('#shipoutputmodal').modal('show')
            });
    }
    var showshipoutdata = function (event) {
        var qt = event.point.series.name;
        var dp = event.point.category;
        $.post('/Shipment/ShipOutputDetailData',
            {
                dp: dp,
                qt: qt
            },
            function (outputdata) {
                if (myshipouttable) {
                    myshipouttable.destroy();
                }
                $('#shipoutputheadid').empty();
                $('#shipoutputcontentid').empty();

                var appendstr0 = '<tr>' +
                        '<th>Product</th>' +
                        '<th>QTY</th>' +
                        '<th>Output</th>' +
                        '</tr>';
                $('#shipoutputheadid').append(appendstr0);

                $.each(outputdata.shipoutlist, function (i, val) {
                    var appendstr = '<tr>' +
                        '<td>' + val.MarketFamily + '</td>' +
                        '<td>' + val.ShipQty + '</td>' +
                        '<td>' + val.Output + '</td>' +
                        '</tr>';
                    $('#shipoutputcontentid').append(appendstr);
                });

                myshipouttable = $('#shipoutdatatable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });
                $('#shipoutputmodalLabel').html(dp + ' ' + qt + ' Ship Output Info');
                $('#shipoutputmodal').modal('show')
            });
    }

    var eventfunction = function (chart, legend, i, col_data)
    {
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

    var drawcolumn = function (col_data) {
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
                    text: 'DPPM'
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
                            if (event.point.series.name.indexOf('VCSEL RMA DPPM') != -1)
                            {
                                showvcesldata(event, col_data);
                            }
                            else if (event.point.series.name.indexOf('ALL RMA DPPM') != -1
                                || event.point.series.name.indexOf('QUARTER RMA DPPM') != -1) {
                                showallrmadata(event, col_data);
                            }
                            else
                            {
                                if (event.point.series.name.indexOf('DPPM') == -1
                                    && col_data.title.indexOf('25G') == -1
                                    && col_data.title.indexOf('10G_14G') == -1)
                                {
                                    showshipoutdata4shipdistribution(event, col_data);
                                }
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
                    //exportdata: {
                    //    onclick: function () {
                    //        var filename = col_data.title + '.csv';
                    //        var outputCSV = ' ,Customer,Ship QTY\r\n';
                    //        $(col_data.xAxis.data).each(function (i, val) {
                    //            $(col_data.data).each(function () {
                    //                if (this.name != '' && (this.data[i] != 0 || this.name.indexOf('DPPM')>=0)) {
                    //                    outputCSV += val + "," + this.name + "," + this.data[i] + ",\r\n";
                    //                }
                    //            });
                    //        })
                    //        var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                    //        $(exportLink).attr({
                    //            'download': filename,
                    //            'href': window.URL.createObjectURL(blobby),
                    //            'target': '_blank'
                    //        });
                    //        exportLink.click();
                    //    },
                    //    text: 'Export Data'
                    //},
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

    var drawordercolumn = function (col_data) {
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
                                });
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
                    //exportdata: {
                    //    onclick: function () {
                    //        var filename = col_data.title + '.csv';
                    //        var outputCSV = ' ,Customer,Order QTY\r\n';
                    //        $(col_data.xAxis.data).each(function (i, val) {
                    //            $(col_data.data).each(function () {
                    //                if (this.name != '' && (this.data[i] != 0 || this.name.indexOf('DPPM') >= 0)) {
                    //                    outputCSV += val + "," + this.name + "," + this.data[i] + ",\r\n";
                    //                }
                    //            });
                    //        })
                    //        var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                    //        $(exportLink).attr({
                    //            'download': filename,
                    //            'href': window.URL.createObjectURL(blobby),
                    //            'target': '_blank'
                    //        });
                    //        exportLink.click();
                    //    },
                    //    text: 'Export Data'
                    //},
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

    var shipmenttable = null;

    var drawotdline = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                title: {
                    text: 'date'
                },
                categories: col_data.xdata
            },
            legend: {
                enabled: true,
            },
            yAxis: [{
                title: {
                    text: 'Rate %'
                }
            },
            {
                opposite: true,
                title: {
                    text: 'Amount'
                }
            }],
            tooltip: {
                pointFormat: '{series.name} : <b>{point.y}</b>'
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            var datestr = event.point.category;
                            //var rate = col_data.rate
                            $('#opdval').html(datestr);

                            if (shipmenttable) {
                                shipmenttable.destroy();
                            }

                            $('#shiprawbody').empty();

                            $.each(col_data.orderdatadict[datestr], function (i, val) {
                                var appendstr = '<tr>' +
                                    '<td>' + val.ShipID + '</td>' +
                                    '<td>' + val.OPDStr + '</td>' +
                                    '<td>' + val.ShipDateStr + '</td>' +
                                    '<td>' + val.OTD + '</td>' +
                                    '<td>' + val.OrderQty + '</td>' +
                                    '<td>' + val.MarketFamily + '</td>' +
                                    '<td>' + val.Customer1 + '</td>' +
                                     '</tr>';
                                $('#shiprawbody').append(appendstr);
                            });

                            shipmenttable = $('#shipmenttable').DataTable({
                                'iDisplayLength': 50,
                                'aLengthMenu': [[20, 50, 100, -1],
                                [20, 50, 100, "All"]],
                                "aaSorting": [],
                                "order": [],
                                dom: 'lBfrtip',
                                buttons: ['copyHtml5', 'csv', 'excelHtml5']
                            });

                            $('#fsrshipdata').modal('show')
                        }
                    }
                }
            },
            series: col_data.chartdata,
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

    var drawrmaworkloaddline = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                title: {
                    text: 'date'
                },
                categories: col_data.xdata
            },
            legend: {
                enabled: true,
            },
            yAxis: [{
                title: {
                    text: 'Days'
                }
            },
            {
                opposite: true,
                title: {
                    text: 'Amount'
                }
            }],
            tooltip: {
                pointFormat: '{series.name} : <b>{point.y}</b>'
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            showworkloadrmadata(event, col_data);
                        }
                    }
                }
            },
            series: col_data.chartdata,
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

    var drawlbsdistribution = function (col_data)
    {
        var options = {
            chart: {
                map: 'custom/world'
            },
            title: {
                text: col_data.title
            },
            mapNavigation: {
                enabled: true,
                buttonOptions: {
                    verticalAlign: 'bottom'
                }
            },
            tooltip: {
                backgroundColor: 'none',
                borderWidth: 0,
                shadow: false,
                useHTML: true,
                padding: 0,
                formatter: function () {
                    if (this.series.name == 'Shipment QTY')
                    {
                        return '<b>' + this.series.name + '</b><br>' +
                            '<span class="f32"><span class="flag ' + this.point.properties['hc-key'] + '">' + '</span></span> ' + this.point.name + '<br>' +
                            '<span style="font-size:24px">'+ this.point.value+' PCS</span>';
                    }
                },
                positioner: function () {
                    return { x: 0, y: 250 };
                }
            },
            plotOptions: {
                mapline: {
                    colorAxis: false
                }
            },
            colorAxis: {
                min: 1,
                max: col_data.maxval,
                type: 'logarithmic',
                stops: [
                    [0, '#E0E6F8'],
                    [0.3, '#6da5ff'],
                    [0.6, '#ffff00'],
                    [0.9, '#ff0000']
                ]
            },

            series: [{
                data: col_data.data,
                joinBy: ['iso-a2', 'code'],
                name: 'Shipment QTY',
                states: {
                    hover: {
                        color: '#a4edba'
                    }
                },
                legend: {
                    title: {
                        text: 'Shipment QTY',
                        style: {
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'black'
                        }
                    }
                }
            },
            {
                type: 'mappoint',
                name: 'Cities',
                dataLabels: {
                    format: '{point.id}'
                },
                marker: {
                    enabled:false
                },
                states: {
                    hover: {
                        enabled:false
                    }
                },
                showInLegend:true,
                data: col_data.capitallist
            }
            ]
        };
        var chart = Highcharts.mapChart(col_data.id, options);

        function pointsToPath(from, to) {
            var arcPointX = (from.x + to.x) / 1.95,
                arcPointY = (from.y + to.y) / 1.95;
            return 'M' + from.x + ',' + from.y + 'Q' + arcPointX + ' ' + arcPointY +
                    ',' + to.x + ' ' + to.y;
        }
        var wux = chart.get('wux');
        var pathdata = new Array();

        $.each(col_data.capitallist, function (i, val) {
            if (val.id != 'wux')
            {
                pathdata.push({
                    id: 'wux - ' + val.id,
                    path: pointsToPath(wux, chart.get(val.id))
                });
            }
        });

        chart.addSeries({
                name: 'ShipPath',
                type: 'mapline',
                lineWidth: 1,
                color: '#DAEB00',
                data: pathdata
            });
    }

    var drawoutputcolumn = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'column'
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                title: {
                    text: 'Department'
                },
                categories: col_data.xlist
            },
            legend: {
                enabled: true,
            },
            yAxis: {
                title: {
                    text: 'Output'
                }
            },
            tooltip: {
                pointFormat: '{series.name} : <b>{point.y} USD</b>'
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            if (col_data.id.indexOf('shipout_id') != -1) {
                                showshipoutdata(event);
                            }
                            else { showalloutdata(event); }
                            
                        }
                    }
                }
            },
            series: col_data.chartseris,
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

    return {
        init: function () {
            show();
        },
        otdinit: function () {
            otddata();
        },
        lbsinit: function () {
            lbsdata();
        },
        orderinit: function () {
            orderdata();
        },
        workloadinit: function () {
            rmaworkloaddata();
        },
        outputinit: function () {
            shipoutput();
        }
    }
}();