var MACHINE = function () {
    var departmentmachine = function () {
        var yieldtable = null;
        var testyieldtable = null;
        var machinelistobj = {}
        function getallyield() {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Machine/DepartmentMachineData',
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

                    machinelistobj = {};
                    var testyieldlength = output.machinelist.length;
                    idx = 0;
                    for (idx = 0; idx < testyieldlength; idx++) {
                        var myid = output.machinelist[idx].id;
                        var mlist = output.machinelist[idx].mlist;
                        machinelistobj[myid] = mlist;
                    }

                });
        }

        $(function () {
            getallyield();
        });

        $('body').on('click', '.WTESTDATA', function () {
            ShowWTestModal($(this).attr('myid'));
        })

        function ShowWTestModal(id) {
            var mytestlist = machinelistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }

            $("#testyieldheadid").empty();
            $("#testyieldcontentid").empty();

            var appendstr = "";
            appendstr += "<tr>";
            appendstr += "<th>WhichTest</th>";
            appendstr += "<th>Machines</th>";
            appendstr += "<th>Hours</th>";
            appendstr += "<th>Rate</th>";
            appendstr += "</tr>";
            $("#testyieldheadid").append(appendstr);

            appendstr = "";
            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.WhichTest + "</td>";
                appendstr += "<td>" + val.Machines + "</td>";
                appendstr += "<td>" + val.Hours + "</td>";
                appendstr += "<td>" + val.Rate + "</td>";
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


        $('body').on('click', '.YIELDDATA', function () {
            ShowMachineModal($(this).attr('myid'));
        })

        function ShowMachineModal(id) {
            var mytestlist = machinelistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }
            $("#testyieldheadid").empty();
            $("#testyieldcontentid").empty();

            var appendstr = "";
            appendstr += "<tr>";
            appendstr += "<th>Machine</th>";
            appendstr += "<th>WhichTest</th>";
            appendstr += "<th>Hours</th>";
            appendstr += "<th>Rate</th>";
            appendstr += "</tr>";
            $("#testyieldheadid").append(appendstr);

            appendstr = "";
            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.Machine + "</td>";
                appendstr += "<td>" + val.MainTest + "</td>";
                appendstr += "<td>" + val.Hours + "</td>";
                appendstr += "<td>" + val.Rate + "</td>";
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
                    visible: false
                },
                yAxis: {
                    title: {
                        text: "Use Rate%"
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
                                    var id = chartidx + "-" + event.point.index + "-0";
                                    ShowMachineModal(id);
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

    var productmachine = function () {
        var yieldtable = null;
        var testyieldtable = null;
        var machinelistobj = {}

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


            $.post('/Machine/ProductMachineData',
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

                    machinelistobj = {};
                    var testyieldlength = output.machinelist.length;
                    idx = 0;
                    for (idx = 0; idx < testyieldlength; idx++) {
                        var myid = output.machinelist[idx].id;
                        var mlist = output.machinelist[idx].mlist;
                        machinelistobj[myid] = mlist;
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

        $('body').on('click', '.WTESTDATA', function () {
            ShowWTestModal($(this).attr('myid'));
        })

        function ShowWTestModal(id) {
            var mytestlist = machinelistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }

            $("#testyieldheadid").empty();
            $("#testyieldcontentid").empty();

            var appendstr = "";
            appendstr += "<tr>";
            appendstr += "<th>WhichTest</th>";
            appendstr += "<th>Machines</th>";
            appendstr += "<th>Hours</th>";
            appendstr += "<th>Rate</th>";
            appendstr += "</tr>";
            $("#testyieldheadid").append(appendstr);

            appendstr = "";
            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.WhichTest + "</td>";
                appendstr += "<td>" + val.Machines + "</td>";
                appendstr += "<td>" + val.Hours + "</td>";
                appendstr += "<td>" + val.Rate + "</td>";
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

        $('body').on('click', '.YIELDDATA', function () {
            ShowMachineModal($(this).attr('myid'));
        })

        function ShowMachineModal(id) {
            var mytestlist = machinelistobj[id];

            if (testyieldtable) {
                testyieldtable.destroy();
            }
            
            $("#testyieldheadid").empty();
            $("#testyieldcontentid").empty();

            var appendstr = "";
            appendstr += "<tr>";
            appendstr += "<th>Machine</th>";
            appendstr += "<th>WhichTest</th>";
            appendstr += "<th>Hours</th>";
            appendstr += "<th>Rate</th>";
            appendstr += "</tr>";
            $("#testyieldheadid").append(appendstr);

            appendstr = "";
            $.each(mytestlist, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.Machine + "</td>";
                appendstr += "<td>" + val.MainTest + "</td>";
                appendstr += "<td>" + val.Hours + "</td>";
                appendstr += "<td>" + val.Rate + "</td>";
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
                    visible: false
                },
                yAxis: {
                    title: {
                        text: "Use Rate %"
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
                                    var id = chartidx + "-" + event.point.index + "-0";
                                    ShowMachineModal(id);
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

    var hydramachine = function () {
        var testyieldtable = null;
        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        function searchdata() {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var tester = $.trim($('#hydratesterlist').val());

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Machine/HydraMachineUsageData', {
                sdate: sdate,
                edate: edate,
                tester:tester
            }, function (output) {
                $.bootstrapLoading.end();
                if (output.success) {
                    $('#chart-content').empty();

                    var appendstr = "";

                    $.each(output.chartlist, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('#chart-content').append(appendstr);
                        drawworkstatus(val);
                    })

                    if (testyieldtable) {
                        testyieldtable.destroy();
                    }
                    $("#testyieldcontentid").empty();
                    if (output.chartlist.length == 0)
                    {
                        return false;
                    }

                    appendstr = "";
                    $.each(output.chartlist[0].pendindlist, function (i, val) {
                        appendstr += "<tr>";
                        appendstr += "<td>Pending</td>";
                        appendstr += "<td>" + val.StartDateStr + "</td>";
                        appendstr += "<td>" + val.EndDateStr + "</td>";
                        appendstr += "<td>" + val.SpendSec + "</td>";
                        appendstr += "<td>" + val.TotalSpend + "</td>";
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

                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });

        function drawworkstatus(line_data)
        {
            var options = {
                title: {
                    text: line_data.title
                },

                yAxis: {
                    uniqueNames: true
                },

                tooltip: {
                    xDateFormat: '%y-%m-%d,%H:%M:%S'
                },

                series: line_data.series
            };
            Highcharts.ganttChart(line_data.id,options);
        }

    }

    return {
        DEPARTMENTINIT: function () {
            departmentmachine();
        },
        PRODUCTINIT: function () {
            productmachine();
        },
        HYDRAINIT: function () {
            hydramachine();
        },
    }
}();
