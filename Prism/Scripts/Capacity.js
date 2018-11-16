var CAPACITY = function () {

    var departmentcapacity = function () {

        var captable = null;
        var capdatatable = null;
        var rawdataobj = {};

        function getallcapacity() {
                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }
                $.bootstrapLoading.start(options);

                $.post('/Capacity/DepartmentCapacityData',
                    {},
                    function (output) {
                        $.bootstrapLoading.end();
                        rawdataobj = output.rawdata;

                        if (captable) {
                            captable.destroy();
                        }
                        $("#capacitytabheadid").empty();
                        $("#capacitytabcontentid").empty();

                        var idx = 0;
                        var titlelength = output.tabletitle.length;
                        var appendstr = "";
                        appendstr += "<tr>";
                        for (idx = 0; idx < titlelength; idx++) {
                            appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                        }
                        appendstr += "<th>Chart</th>";
                        appendstr += "</tr>";
                        $("#capacitytabheadid").append(appendstr);

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
                        $("#capacitytabcontentid").append(appendstr);

                        for (idx = 0; idx < contentlength; idx++) {
                            drawline(output.chartdatalist[idx]);
                        }

                        captable = $('#capacitymaintable').DataTable({
                            'iDisplayLength': 50,
                            'aLengthMenu': [[20, 50, 100, -1],
                            [20, 50, 100, "All"]],
                            "aaSorting": [],
                            "order": [],
                            dom: 'lBfrtip',
                            buttons: ['copyHtml5', 'csv', 'excelHtml5']
                        });

                    });
        }

        $(function () {
            getallcapacity();
        });


        $('body').on('click', '.YIELDDATA', function () {
            showcapacitydata($(this).attr('myid'));
        })

        function showcapacitydata(id) {
            var capdata = rawdataobj[id];

            if (capdatatable) {
                capdatatable.destroy();
            }
            $("#capacitycontentid").empty();

            var appendstr = "";

            $.each(capdata, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.Quarter + "</td>";
                appendstr += "<td>" + val.Product + "</td>";
                appendstr += "<td>" + val.MaxCapacity + "</td>";
                appendstr += "<td>" + val.ForeCast + "</td>";
                appendstr += "<td>" + val.Usage + "</td>";
                appendstr += "<td>" + val.GAP + "</td>";
                appendstr += "<td>" + val.PN + "</td>";
                appendstr += "</tr>";
            })
            $("#capacitycontentid").append(appendstr);

            capdatatable = $('#capacitydatatable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#capacitymodal').modal('show');
        }

        var drawline = function (line_data) {
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
                yAxis: [{
                    title: {
                        text: 'Amount'
                    },
                    visible: false
                },
                {
                    opposite: true,
                    title: {
                        text: "Consume Rate %"
                    },
                    visible: false,
                    plotLines: [{
                        value: 90,
                        color: 'red',
                        width: 1,
                        label: {
                            text: 'Capacity Warning Line:' + 90+'%',
                            align: 'left'
                        }
                    }]
                }],
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
                                    $(this)[0].chart.yAxis[1].update({ visible: true });
                                    $(this)[0].chart.legend.update({ enabled: true });
                                    $(this)[0].chart.exporting.update({ enabled: true });
                                    $(this)[0].chart.reflow();
                                }
                                else {
                                    showcapacitydata(line_data.pd);
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

    var productcapacity = function () {

        $.post('/Capacity/GetAllProductList', {}, function (output) {
            $('#productlist').tagsinput({
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

            getproductcapacity();
        });

        var captable = null;
        var capdatatable = null;
        var rawdataobj = {};

        function getproductcapacity() {
            var prodtype = $('#producttype').val();
            var prod = $.trim($('#productlist').tagsinput('items'));
            if (prod == '') {
                prod = $.trim($('#productlist').parent().find('input').eq(0).val());
            }

            if (prodtype == '' && prod == '')
            { return false;}

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Capacity/ProductCapacityData',
                {
                    prodtype:prodtype,
                    prod:prod
                },
                function (output) {
                    $.bootstrapLoading.end();
                    rawdataobj = output.rawdata;

                    if (captable) {
                        captable.destroy();
                    }
                    $("#capacitytabheadid").empty();
                    $("#capacitytabcontentid").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr += "<tr>";
                    for (idx = 0; idx < titlelength; idx++) {
                        appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "<th>Chart</th>";
                    appendstr += "</tr>";
                    $("#capacitytabheadid").append(appendstr);

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
                    $("#capacitytabcontentid").append(appendstr);

                    for (idx = 0; idx < contentlength; idx++) {
                        drawline(output.chartdatalist[idx]);
                    }

                    captable = $('#capacitymaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                });
        }


        $('body').on('click', '#btn-search', function () {
            getproductcapacity();
        })


        $('body').on('click', '.YIELDDATA', function () {
            showcapacitydata($(this).attr('myid'));
        })

        function showcapacitydata(id) {
            var capdata = rawdataobj[id];

            if (capdatatable) {
                capdatatable.destroy();
            }
            $("#capacitycontentid").empty();

            var appendstr = "";

            $.each(capdata, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.Quarter + "</td>";
                appendstr += "<td>" + val.Product + "</td>";
                appendstr += "<td>" + val.MaxCapacity + "</td>";
                appendstr += "<td>" + val.ForeCast + "</td>";
                appendstr += "<td>" + val.Usage + "</td>";
                appendstr += "<td>" + val.GAP + "</td>";
                appendstr += "<td>" + val.PN + "</td>";
                appendstr += "</tr>";
            })
            $("#capacitycontentid").append(appendstr);

            capdatatable = $('#capacitydatatable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#capacitymodal').modal('show');
        }

        var drawline = function (line_data) {
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
                yAxis: [{
                    title: {
                        text: 'Amount'
                    },
                    visible: false
                },
                {
                    opposite: true,
                    title: {
                        text: "Consume Rate %"
                    },
                    visible: false,
                    plotLines: [{
                        value: 90,
                        color: 'red',
                        width: 1,
                        label: {
                            text: 'Capacity Warning Line:' + 90 + '%',
                            align: 'left'
                        }
                    }]
                }],
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
                                    $(this)[0].chart.yAxis[1].update({ visible: true });
                                    $(this)[0].chart.legend.update({ enabled: true });
                                    $(this)[0].chart.exporting.update({ enabled: true });
                                    $(this)[0].chart.reflow();
                                }
                                else {
                                    showcapacitydata(line_data.pd);
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
            departmentcapacity();
        },
        PRODUCTINIT: function () {
            productcapacity();
        }
    }
}();