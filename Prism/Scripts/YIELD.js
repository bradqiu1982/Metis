var YIELD = function () {
    var departmentyield = function () {
        var yieldtable = null;
        var testyieldtable = null;
        var testyieldlistobj = {}
        function getallyield()
        {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Yield/DepartmentYieldData',
                {},
                function (output) {
                    $.bootstrapLoading.end();

                    if (yieldtable) {
                        yieldtable.destroy();
                    }

                    $("#yieldtabheadid").empty();
                    $("#yieldtabcontentid").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr +="<tr>";
                    for (idx = 0; idx < titlelength; idx++)
                    {
                        appendstr +="<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "<th>Chart</th>";
                    appendstr +="</tr>";
                    $("#yieldtabheadid").append(appendstr);

                    appendstr = "";
                    idx = 0;
                    var contentlength = output.tablecontent.length;
                    for(idx = 0;idx < contentlength;idx++)
                    {
                        appendstr += "<tr>";
                        var line = output.tablecontent[idx];
                        var jdx = 0;
                        var linelength = line.length;
                        for(jdx = 0;jdx < linelength;jdx++)
                        {
                            appendstr += "<td>" + line[jdx] + "</td>";
                        }
                        appendstr += "<td><div id='" + output.chartdatalist[idx].id + "' style='max-width:840px!important'></div></td>";
                        appendstr += "</tr>";
                    }
                    $("#yieldtabcontentid").append(appendstr);

                    for (idx = 0; idx < contentlength; idx++) {
                        drawline(output.chartdatalist[idx],idx);
                    }

                    yieldtable = $('#yieldmaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                    testyieldlistobj = {};
                    var testyieldlength = output.testyieldlist.length;
                    idx = 0;
                    for (idx = 0; idx < testyieldlength; idx++)
                    {
                        var myid = output.testyieldlist[idx].id;
                        var testlist = output.testyieldlist[idx].testlist;
                        testyieldlistobj[myid] = testlist;
                    }

                });
        }

        $(function () {
            getallyield();
        });

        $('body').on('click', '.YIELDDATA', function () {
            ShowTestYieldModal($(this).attr('myid'));
        })

        function ShowTestYieldModal(id)
        {
            var mytestlist = testyieldlistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }
            $("#testyieldcontentid").empty();

            var appendstr = "";
            
            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.WhichTest + "</td>";
                appendstr += "<td>" + val.Failed + "</td>";
                appendstr += "<td>" + val.Pass + "</td>";
                appendstr += "<td>" + val.Yield + "</td>";
                appendstr += "</tr>";
            })
            $("#testyieldcontentid").append(appendstr);

            testyieldtable = $('#testyieldtable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#testyieldmodal').modal('show');
        }

        var drawline = function (line_data,chartidx) {
            var options = {
                chart: {
                    height: 80,
                    width: 100,
                    type: 'line'
                },
                title: {
                    text: ''
                },
                dataLabels: {
                    enabled: false
                },
                xAxis: {
                    title: {
                        text: "Quarter"
                    },
                    categories: line_data.xlist,
                    visible: false
                },
                yAxis: {
                    title: {
                        text:"Yield %"
                    },
                    visible: false
                },
                credits: {
                    enabled: false
                },
                exporting:
                  {
                      enabled: false
                  },
                legend: {
                    enabled: false
                },
                plotOptions: {
                    series: {
                        cursor: 'pointer',
                        events: {
                            click: function (event) {
                                if (!$(this)[0].chart.xAxis[0].visible) {
                                    $('#' + line_data.id).parent().toggleClass('chart-modal-zoom');
                                    $(this)[0].chart.setSize(840, 420);
                                    $(this)[0].chart.xAxis[0].update({ visible: true });
                                    $(this)[0].chart.yAxis[0].update({ visible: true });
                                    $(this)[0].chart.legend.update({ enabled: true });
                                    $(this)[0].chart.exporting.update({ enabled: true });
                                    $(this)[0].chart.reflow();
                                }
                                else {
                                    var id = chartidx + "-" + event.point.index + "-";
                                    if (event.point.series.name.indexOf("FPY") != -1) {
                                        id += "0";
                                    }
                                    else {
                                        id += "1";
                                    }
                                    ShowTestYieldModal(id);
                                }
                            }
                        }
                    }
                },
                series: line_data.series
            };

            Highcharts.chart(line_data.id, options);
        }
    }

    var productyield = function () {
            var yieldtable = null;
            var testyieldtable = null;
            var testyieldlistobj = {}

            function getpdyield(pdf) {

                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }
                $.bootstrapLoading.start(options);


                $.post('/Yield/ProductYieldData',
                    {
                        pdf: pdf
                    },
                    function (output) {

                        $.bootstrapLoading.end();

                        if (yieldtable) {
                            yieldtable.destroy();
                        }

                        $("#yieldtabheadid").empty();
                        $("#yieldtabcontentid").empty();

                        var idx = 0;
                        var titlelength = output.tabletitle.length;
                        var appendstr = "";
                        appendstr += "<tr>";
                        for (idx = 0; idx < titlelength; idx++) {
                            appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                        }
                        appendstr += "<th>Chart</th>";
                        appendstr += "</tr>";

                        $("#yieldtabheadid").append(appendstr);

                        appendstr = "";
                        idx = 0;
                        var contentlength = output.tablecontent.length;
                        for (idx = 0; idx < contentlength; idx++) {
                            appendstr += "<tr>";
                            var line = output.tablecontent[idx];
                            var jdx = 0;
                            var linelength = line.length;
                            for (jdx = 0; jdx < linelength; jdx++) {
                                appendstr += "<td>" + line[jdx] + "</td>";
                            }
                            appendstr += "<td><div id='" + output.chartdatalist[idx].id + "' style='max-width:840px!important'></div></td>";
                            appendstr += "</tr>";
                        }
                        $("#yieldtabcontentid").append(appendstr);

                        for (idx = 0; idx < contentlength; idx++) {
                            drawline(output.chartdatalist[idx], idx);
                        }

                        yieldtable = $('#yieldmaintable').DataTable({
                            'iDisplayLength': 50,
                            'aLengthMenu': [[20, 50, 100, -1],
                            [20, 50, 100, "All"]],
                            "aaSorting": [],
                            "order": [],
                            dom: 'lBfrtip',
                            buttons: ['copyHtml5', 'csv', 'excelHtml5']
                        });

                        testyieldlistobj = {};
                        var testyieldlength = output.testyieldlist.length;
                        idx = 0;
                        for (idx = 0; idx < testyieldlength; idx++) {
                            var myid = output.testyieldlist[idx].id;
                            var testlist = output.testyieldlist[idx].testlist;
                            testyieldlistobj[myid] = testlist;
                        }

                    });
            }

            $(function () {
                var pdf = $('#productfamily').val();
                if (pdf != '') {
                    getpdyield(pdf);
                }
            });

            $.post('/Yield/GetAllYieldProductList', {}, function (output) {
                $('#yieldproductlist').tagsinput({
                    freeInput: false,
                    typeahead: {
                        source: output.pdlist,
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
            });

            $('body').on('click', '#btn-search', function () {
                var pdfs = $.trim($('#yieldproductlist').tagsinput('items'));
                if (pdfs == '') {
                    pdfs = $.trim($('#yieldproductlist').parent().find('input').eq(0).val());
                }
                if (pdfs != '') {
                    getpdyield(pdfs);
                }
            })

            $('body').on('click', '.YIELDDATA', function () {
                ShowTestYieldModal($(this).attr('myid'));
            })

            function ShowTestYieldModal(id) {
                var mytestlist = testyieldlistobj[id];

                if (testyieldtable) {
                    testyieldtable.destroy();
                }
                $("#testyieldcontentid").empty();

                var appendstr = "";

                $.each(mytestlist, function (i, val) {
                    appendstr += "<tr>";
                    appendstr += "<td>" + val.WhichTest + "</td>";
                    appendstr += "<td>" + val.Failed + "</td>";
                    appendstr += "<td>" + val.Pass + "</td>";
                    appendstr += "<td>" + val.Yield + "</td>";
                    appendstr += "</tr>";
                })
                $("#testyieldcontentid").append(appendstr);

                testyieldtable = $('#testyieldtable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('#testyieldmodal').modal('show');
            }

            var drawline = function (line_data, chartidx) {
                var options = {
                    chart: {
                        height: 80,
                        width: 100,
                        type: 'line'
                    },
                    title: {
                        text: ''
                    },
                    dataLabels: {
                        enabled: false
                    },
                    xAxis: {
                        title: {
                            text: "Quarter"
                        },
                        categories: line_data.xlist,
                        plotlines: [{

                        }],
                        visible: false
                    },
                    yAxis: {
                        title: {
                            text: "Yield %"
                        },
                        visible: false,
                        Max:120,
                        plotLines: [{
                            value: line_data.fpytg,
                            color: 'red',
                            width: 2,
                            label: {
                                text: 'FPY Target:' + line_data.fpytg,
                                align: 'left'
                            }
                        },
                        {
                            value: line_data.fytg,
                            color: 'green',
                            width: 1,
                            label: {
                                text: 'FY Target:' + line_data.fytg,
                                align: 'left'
                            }
                        }]
                    },
                    credits: {
                        enabled: false
                    },
                    exporting:
                      {
                          enabled: false
                      },
                    legend: {
                        enabled: false
                    },
                    plotOptions: {
                        series: {
                            cursor: 'pointer',
                            events: {
                                click: function (event) {
                                    if (!$(this)[0].chart.xAxis[0].visible) {
                                        $('#' + line_data.id).parent().toggleClass('chart-modal-zoom');
                                        $(this)[0].chart.setSize(840, 420);
                                        $(this)[0].chart.xAxis[0].update({ visible: true });
                                        $(this)[0].chart.yAxis[0].update({ visible: true });
                                        $(this)[0].chart.legend.update({ enabled: true });
                                        $(this)[0].chart.exporting.update({ enabled: true });
                                        $(this)[0].chart.reflow();
                                    }
                                    else {
                                        var id = chartidx + "-" + event.point.index + "-";
                                        if (event.point.series.name.indexOf("FPY") != -1) {
                                            id += "0";
                                        }
                                        else {
                                            id += "1";
                                        }
                                        ShowTestYieldModal(id);
                                    }
                                }
                            }
                        }
                    },
                    series: line_data.series
                };

                Highcharts.chart(line_data.id, options);
            }
    }

    var trendyield = function () {
        var yieldtable = null;
        var testyieldtable = null;
        var testyieldlistobj = {}

        function getpdyield() {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);


            $.post('/Yield/YieldTrendData',
                {
                },
                function (output) {

                    $.bootstrapLoading.end();

                    if (yieldtable) {
                        yieldtable.destroy();
                    }

                    $("#yieldtabheadid").empty();
                    $("#yieldtabcontentid").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr += "<tr>";
                    for (idx = 0; idx < titlelength; idx++) {
                        appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "<th>Chart</th>";
                    appendstr += "</tr>";

                    $("#yieldtabheadid").append(appendstr);

                    appendstr = "";
                    idx = 0;
                    var contentlength = output.tablecontent.length;
                    for (idx = 0; idx < contentlength; idx++) {
                        appendstr += "<tr>";
                        var line = output.tablecontent[idx];
                        var jdx = 0;
                        var linelength = line.length;
                        for (jdx = 0; jdx < linelength; jdx++) {
                            appendstr += "<td>" + line[jdx] + "</td>";
                        }
                        appendstr += "<td><div id='" + output.chartdatalist[idx].id + "' style='max-width:840px!important'></div></td>";
                        appendstr += "</tr>";
                    }
                    $("#yieldtabcontentid").append(appendstr);

                    for (idx = 0; idx < contentlength; idx++) {
                        drawline(output.chartdatalist[idx], idx);
                    }

                    yieldtable = $('#yieldmaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                    testyieldlistobj = {};
                    var testyieldlength = output.testyieldlist.length;
                    idx = 0;
                    for (idx = 0; idx < testyieldlength; idx++) {
                        var myid = output.testyieldlist[idx].id;
                        var testlist = output.testyieldlist[idx].testlist;
                        testyieldlistobj[myid] = testlist;
                    }

                });
        }

        $(function () {
                getpdyield();
        });



        $('body').on('click', '.YIELDDATA', function () {
            ShowTestYieldModal($(this).attr('myid'));
        })

        function ShowTestYieldModal(id) {
            var mytestlist = testyieldlistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }
            $("#testyieldcontentid").empty();

            var appendstr = "";

            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.WhichTest + "</td>";
                appendstr += "<td>" + val.Failed + "</td>";
                appendstr += "<td>" + val.Pass + "</td>";
                appendstr += "<td>" + val.Yield + "</td>";
                appendstr += "</tr>";
            })
            $("#testyieldcontentid").append(appendstr);

            testyieldtable = $('#testyieldtable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#testyieldmodal').modal('show');
        }

        var drawline = function (line_data, chartidx) {
            var options = {
                chart: {
                    height: 80,
                    width: 100,
                    type: 'line'
                },
                title: {
                    text: ''
                },
                dataLabels: {
                    enabled: false
                },
                xAxis: {
                    title: {
                        text: "Quarter"
                    },
                    categories: line_data.xlist,
                    plotlines: [{

                    }],
                    visible: false
                },
                yAxis: {
                    title: {
                        text: "Yield %"
                    },
                    visible: false,
                    Max: 120,
                    plotLines: [{
                        value: line_data.fpytg,
                        color: 'red',
                        width: 2,
                        label: {
                            text: 'FPY Target:' + line_data.fpytg,
                            align: 'left'
                        }
                    },
                    {
                        value: line_data.fytg,
                        color: 'green',
                        width: 2,
                        label: {
                            text: 'FY Target:' + line_data.fytg,
                            align: 'left'
                        }
                    }]
                },
                credits: {
                    enabled: false
                },
                exporting:
                  {
                      enabled: false
                  },
                legend: {
                    enabled: false
                },
                plotOptions: {
                    series: {
                        cursor: 'pointer',
                        events: {
                            click: function (event) {
                                if (!$(this)[0].chart.xAxis[0].visible) {
                                    $('#' + line_data.id).parent().toggleClass('chart-modal-zoom');
                                    $(this)[0].chart.setSize(840, 420);
                                    $(this)[0].chart.xAxis[0].update({ visible: true });
                                    $(this)[0].chart.yAxis[0].update({ visible: true });
                                    $(this)[0].chart.legend.update({ enabled: true });
                                    $(this)[0].chart.exporting.update({ enabled: true });
                                    $(this)[0].chart.reflow();
                                }
                                else {
                                    var id = chartidx + "-" + event.point.index + "-";
                                    if (event.point.series.name.indexOf("FPY") != -1) {
                                        id += "0";
                                    }
                                    else {
                                        id += "1";
                                    }
                                    ShowTestYieldModal(id);
                                }
                            }
                        }
                    }
                },
                series: line_data.series
            };

            Highcharts.chart(line_data.id, options);
        }
    }

    return {
        DEPARTMENTINIT: function () {
            departmentyield();
        },
        PRODUCTINIT: function () {
            productyield();
        },
        TRENDINIT: function () {
            trendyield();
        }
    }
}();
