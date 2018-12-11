var INVENTORY = function () {

    var departmentinvent = function () {

        var inventtable = null;
        var inventdatatable = null;

        function getallinventory() {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Inventory/DepartmentInventoryData',
                {},
                function (output) {
                    $.bootstrapLoading.end();

                    if (inventtable) {
                        inventtable.destroy();
                    }
                    $("#inventorytabheadid").empty();
                    $("#inventorytabcontentid").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr += "<tr>";
                    for (idx = 0; idx < titlelength; idx++) {
                        appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "<th>Chart</th>";
                    appendstr += "</tr>";
                    $("#inventorytabheadid").append(appendstr);

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
                    $("#inventorytabcontentid").append(appendstr);

                    for (idx = 0; idx < contentlength; idx++) {
                        drawline(output.chartdatalist[idx]);
                    }

                    inventtable = $('#inventorymaintable').DataTable({
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
            getallinventory();
        });


        $('body').on('click', '.YIELDDATA', function () {
            showinventdata($(this).attr('qt'), $(this).attr('pd'));
        })

        function showinventdata(qt,pd) {
            $.post('/Inventory/DepartmentDetailDataDP',
                {
                    qt: qt,
                    pd: pd
                },
                function (output)
                {
                    if (inventdatatable) {
                        inventdatatable.destroy();
                    }
                    $("#inventorycontentid").empty();

                    var appendstr = "";

                    $.each(output.invtdata, function (i, val) {
                        appendstr += "<tr>";
                        appendstr += "<td>" + val.Quarter + "</td>";
                        appendstr += "<td>" + val.Department + "</td>";
                        appendstr += "<td>" + val.Product + "</td>";
                        appendstr += "<td>" + val.COGS + "</td>";
                        appendstr += "<td>" + val.Inventory + "</td>";
                        appendstr += "<td>" + val.InventoryTurns + "</td>";
                        appendstr += "</tr>";
                    })
                    $("#inventorycontentid").append(appendstr);

                    inventdatatable = $('#inventorydatatable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                    $('#inventorymodal').modal('show');
                }
                );
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
                        text: 'Dollar'
                    },
                    visible: false
                },
                {
                    opposite: true,
                    title: {
                        text: "Turns"
                    },
                    visible: false,
                    plotLines: [{
                        value: 4.0,
                        color: 'red',
                        width: 2,
                        label: {
                            text: 'Turns Target:' + 4.0,
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
                                    console.log(event);
                                    showinventdata(event.point.category, line_data.pd);
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

    var productinvent = function () {

        $.post('/Inventory/GetAllProductList', {}, function (output) {
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

            getproductinventory();
        });

        var inventtable = null;
        var inventdatatable = null;

        function getproductinventory() {
            var prodtype = $('#producttype').val();
            var prod = $.trim($('#productlist').tagsinput('items'));
            if (prod == '') {
                prod = $.trim($('#productlist').parent().find('input').eq(0).val());
            }

            if (prodtype == '' && prod == '')
            { return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Inventory/ProductInventoryData',
                {
                    prodtype: prodtype,
                    prod: prod
                },
                function (output) {
                    $.bootstrapLoading.end();

                    if (inventtable) {
                        inventtable.destroy();
                    }
                    $("#inventorytabheadid").empty();
                    $("#inventorytabcontentid").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr += "<tr>";
                    for (idx = 0; idx < titlelength; idx++) {
                        appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "<th>Chart</th>";
                    appendstr += "</tr>";
                    $("#inventorytabheadid").append(appendstr);

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
                    $("#inventorytabcontentid").append(appendstr);

                    for (idx = 0; idx < contentlength; idx++) {
                        drawline(output.chartdatalist[idx]);
                    }

                    inventtable = $('#inventorymaintable').DataTable({
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
            getproductinventory();
        })


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
                        text: 'Dollar'
                    },
                    visible: false
                },
                {
                    opposite: true,
                    title: {
                        text: "Turns"
                    },
                    visible: false,
                    plotLines: [{
                        value: 4.0,
                        color: 'red',
                        width: 2,
                        label: {
                            text: 'Turns Target:' + 4.0,
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
            departmentinvent();
        },
        PRODUCTINIT: function () {
            productinvent();
        }
    }
}();