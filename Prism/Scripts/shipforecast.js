var FORECAST = function () {
    var forecastaccuracy = function () {

        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months"});

            var fctable = null;
            var sertable = null;

            function loadallaccuracy(withplm)
            {
                var startdate = $('#sdate').val();

                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }

                $.bootstrapLoading.start(options);
                $.post('/Shipment/ForecastAccuracyData', {
                    startdate:startdate
                },
                    function (output) {

                        $.bootstrapLoading.end();
                        if (fctable) {
                            fctable.destroy();
                            fctable = null;
                        }

                        $("#fchead").empty();
                        $("#fccontent").empty();

                        var appendstr = '';
                        appendstr += '<tr>';
                        appendstr += '<th>BU</th>';
                        appendstr += '<th>Product Group</th>';
                        appendstr += '<th>Series</th>';
                        appendstr += '<th>Accuracy</th>';
                        if (withplm) {  appendstr += '<th>PLM</th>'; }
                        appendstr += '</tr>';
                        $('#fchead').append(appendstr);

                        appendstr = '';

                        $.each(output.accuracylist, function (i, val) {
                            appendstr += '<tr>';
                            appendstr += '<td>' + val.BU + '</td>';
                            appendstr += '<td>' + val.ProjectGroup + '</td>';
                            appendstr += '<td>' + val.Series + '</td>';
                            appendstr += '<td class="DETAILINFO" myid="' + val.Series + '">' + val.Accuracy + '%</td>';
                            if (withplm) { appendstr += '<td>' + val.PLM + '</td>'; }
                            appendstr += '</tr>';
                        })
                        $('#fccontent').append(appendstr);

                        fctable = $('#fctable').DataTable({
                            'iDisplayLength': -1,
                            'aLengthMenu': [[20, 50, 100, -1],
                            [20, 50, 100, "All"]],
                            "columnDefs": [
                                { "className": "dt-center", "targets": "_all"}
                            ],
                            "aaSorting": [],
                            "order": [],
                            dom: 'lBfrtip',
                            buttons: ['copyHtml5', 'csv', 'excelHtml5']
                        });
                    });
            }


            $('body').on('click', '#btn-search', function () {
                 loadallaccuracy(false);
            })

            $('body').on('click', '#btn-searchplm', function () {
                loadallaccuracy(true);
            })

            $.fn.dataTable.ext.buttons.closetable = {
                    text: 'CLOSE TABLE',
                    action: function (e, dt, node, config) {
                        $('.table-modal').addClass('hide');
                    }
                };

            function ShowForecastModal(series)
            {
                var startdate = $('#sdate').val();

                    $.post('/Shipment/SeriesAccuracyData', {
                        series: series,
                        startdate: startdate
                    }, function (output) {

                        if (sertable) {
                            sertable.destroy();
                            sertable = null;
                        }

                        $("#serhead").empty();
                        $("#sercontent").empty();

                        var idx = 0;
                        var titlelength = output.tabletitle.length;
                        var appendstr = "";
                        appendstr += "<tr>";
                        for (idx = 0; idx < titlelength; idx++) {
                            appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                        }
                        appendstr += "</tr>";
                        $("#serhead").append(appendstr);

                        appendstr = "";
                        idx = 0;
                        var contentlength = output.tablecontent.length;
                        for (idx = 0; idx < contentlength; idx++) {
                            appendstr += '<tr style="font-size:10px">';
                            var line = output.tablecontent[idx];
                            var jdx = 0;
                            var linelength = line.length;
                            for (jdx = 0; jdx < linelength; jdx++) {
                                appendstr += "<td>" + line[jdx] + "</td>";
                            }
                            appendstr += "</tr>";
                        }
                        $("#sercontent").append(appendstr);

                        $('#seriesmodalLabel').html(series + " Forecast Data");

                        sertable = $('#sertable').DataTable({
                            'iDisplayLength': 50,
                            'aLengthMenu': [[20, 50, 100, -1],
                            [20, 50, 100, "All"]],
                            "aaSorting": [],
                            "order": [],
                            dom: 'lBfrtip',
                            buttons: ['copyHtml5', 'csv', 'excelHtml5', 'closetable']
                        });

                        
                        $('.table-modal').removeClass('hide');

                        //$('#seriesmodal').modal('show');
                    });
                }

                $('body').on('click', '.DETAILINFO', function () {
                    ShowForecastModal($(this).attr('myid'));
                })
        }

    var forecastmerge = function () {

        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        var fctable = null;
        var sertable = null;

        function loadallaccuracy() {
            var startdate = $('#sdate').val();

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }

            $.bootstrapLoading.start(options);
            $.post('/Shipment/ForecastMergeData', {
                startdate: startdate
            },
                function (output) {

                    $.bootstrapLoading.end();
                    if (fctable) {
                        fctable.destroy();
                        fctable = null;
                    }

                    $("#fchead").empty();
                    $("#fccontent").empty();

                    var appendstr = '';
                    appendstr += '<tr>';
                    appendstr += '<th>BU</th>';
                    appendstr += '<th>Product Group</th>';
                    appendstr += '<th>Series</th>';
                    appendstr += '<th>Accuracy</th>';
                    appendstr += '</tr>';
                    $('#fchead').append(appendstr);

                    appendstr = '';

                    $.each(output.table, function (i, line) {
                        appendstr += '<tr>';
                        $.each(line, function (i, val) {

                            if (val.indexOf(':rowspan') != -1)
                            {
                                var idx = val.indexOf(':rowspan');
                                var v = val.substring(0, idx);
                                var cla = val.substring(idx + 1);
                                appendstr += '<td '+cla+'>' + v + '</td>';
                            }
                            else if (val.indexOf(':hide') != -1) {
                                var idx = val.indexOf(':hide');
                                var v = val.substring(0, idx);
                                appendstr += '<td class="hide">' + v + '</td>';
                            }
                            else {
                                if (val.indexOf('%') != -1)
                                { appendstr += '<td class="DETAILINFO" myid="' + line[2] + '">' + val + '</td>'; }
                                else
                                { appendstr += '<td>' + val + '</td>'; }
                            }
                        })
                        appendstr += '</tr>';
                    });

                    $('#fccontent').append(appendstr);

                    fctable = $('#fctable').DataTable({
                        'iDisplayLength': -1,
                        'aLengthMenu': [[ -1],
                        ["All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all", "bSortable": false }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                });
        }


        $('body').on('click', '#btn-search', function () {
            loadallaccuracy();
        })

        $.fn.dataTable.ext.buttons.closetable = {
            text: 'CLOSE TABLE',
            action: function (e, dt, node, config) {
                $('.table-modal').addClass('hide');
            }
        };

        function ShowForecastModal(series) {
            var startdate = $('#sdate').val();

            $.post('/Shipment/SeriesAccuracyData', {
                series: series,
                startdate: startdate
            }, function (output) {

                if (sertable) {
                    sertable.destroy();
                    sertable = null;
                }

                $("#serhead").empty();
                $("#sercontent").empty();

                var idx = 0;
                var titlelength = output.tabletitle.length;
                var appendstr = "";
                appendstr += "<tr>";
                for (idx = 0; idx < titlelength; idx++) {
                    appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                }
                appendstr += "</tr>";
                $("#serhead").append(appendstr);

                appendstr = "";
                idx = 0;
                var contentlength = output.tablecontent.length;
                for (idx = 0; idx < contentlength; idx++) {
                    appendstr += '<tr style="font-size:10px">';
                    var line = output.tablecontent[idx];
                    var jdx = 0;
                    var linelength = line.length;
                    for (jdx = 0; jdx < linelength; jdx++) {
                        appendstr += "<td>" + line[jdx] + "</td>";
                    }
                    appendstr += "</tr>";
                }
                $("#sercontent").append(appendstr);

                $('#seriesmodalLabel').html(series + " Forecast Data");

                sertable = $('#sertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5', 'closetable']
                });


                $('.table-modal').removeClass('hide');

                //$('#seriesmodal').modal('show');
            });
        }

        $('body').on('click', '.DETAILINFO', function () {
            ShowForecastModal($(this).attr('myid'));
        })

    }

    var marginfun = function () {

        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });

        var fctable = null;
        var sertable = null;
        var shipdetailtable = null;

        function loadshipmargin() {
            var startdate = $('#sdate').val();
            var datatype = $('#DATATYPE').val();

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }

            
            $.bootstrapLoading.start(options);
            $.post('/Shipment/ShipMarginData', {
                startdate: startdate,
                datatype: datatype
            },
                function (output) {
                    $.bootstrapLoading.end();

                    if (fctable) {
                        fctable.destroy();
                        fctable = null;
                    }

                    $("#fchead").empty();
                    $("#fccontent").empty();

                    var idx = 0;
                    var titlelength = output.tabletitle.length;
                    var appendstr = "";
                    appendstr += '<tr style="font-size:10px">';
                    for (idx = 0; idx < titlelength; idx++) {
                        appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                    }
                    appendstr += "</tr>";
                    $("#fchead").append(appendstr);

                    appendstr = "";
                    idx = 0;
                    var contentlength = output.tablecontent.length;
                    for (idx = 0; idx < contentlength; idx++) {
                        appendstr += '<tr style="font-size:10px">';
                        var line = output.tablecontent[idx];
                        var jdx = 0;
                        var linelength = line.length;
                        for (jdx = 0; jdx < linelength; jdx++) {
                            if (line[2].trim() != '' && line[jdx].trim() != '' && jdx > 3
                                && ((jdx - 3) % 4 == 1 || (jdx - 3) % 4 == 2)) {
                                appendstr += '<td class="DETAILINFO" myid="' + line[1] + ':::'+ line[2] + ':::' + output.tabletitle[jdx] +  '">' + line[jdx] + '</td>';
                            }
                            else {
                                appendstr += '<td>' + line[jdx] + '</td>';
                            }
                        }
                        appendstr += '</tr>';
                    }
                    $("#fccontent").append(appendstr);


                    fctable = $('#fctable').DataTable({
                        'iDisplayLength': -1,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                });
        }


        $('body').on('click', '#btn-search', function () {
            loadshipmargin();
        })

        $(function () {
            loadshipmargin();
        });



        function ShowForecastModal(series) {
            var datatype = $('#DATATYPE').val();

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);


            $.post('/Shipment/ShipMarginDetail', {
                series: series,
                datatype: datatype
                },
                function (output) {
                    $.bootstrapLoading.end();

                    if (shipdetailtable) {
                        shipdetailtable.destroy();
                        shipdetailtable = null;
                    }

                    $("#shipdetailhead").empty();
                    $("#shipdetailbody").empty();

                    if (output.fun.indexOf('COST') != -1) {
                        $('#shipdetaillabel').html(series.split(':::').join('/'));

                        var appendstr = '<tr>';
                        appendstr += '<th>PN</th>';
                        appendstr += '<th>SHIP QTY</th>';
                        appendstr += '<th>COST</th>';
                        appendstr += '<th>DATE</th>';
                        appendstr += '</tr>';
                        $("#shipdetailhead").append(appendstr);

                        $.each(output.shipdetail, function (i, val) {
                            appendstr = '<tr>';
                            appendstr += '<td>'+val.PN+'</td>';
                            appendstr += '<td>'+val.ShipQty+'</td>';
                            appendstr += '<td>'+val.Cost+'</td>';
                            appendstr += '<td>' + val.ShipDate + '</td>';
                            appendstr += '</tr>';
                            $("#shipdetailbody").append(appendstr);
                        });
                    }
                    else if (output.fun.indexOf('REVENUE') != -1) {
                        $('#shipdetaillabel').html(series.split(':::').join('/').replace('Revenue','Price'));
                        var appendstr = '<tr>';
                        appendstr += '<th>PN</th>';
                        appendstr += '<th>SHIP QTY</th>';
                        appendstr += '<th>PRICE</th>';
                        appendstr += '<th>DATE</th>';
                        appendstr += '</tr>';
                        $("#shipdetailhead").append(appendstr);

                        $.each(output.shipdetail, function (i, val) {
                            appendstr = '<tr>';
                            appendstr += '<td>' + val.PN + '</td>';
                            appendstr += '<td>' + val.ShipQty + '</td>';
                            appendstr += '<td>' + val.SalePrice + '</td>';
                            appendstr += '<td>' + val.ShipDate + '</td>';
                            appendstr += '</tr>';
                            $("#shipdetailbody").append(appendstr);
                        });
                    }

                    shipdetailtable = $('#shipdetailtable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                    $('#shipdetaildata').modal('show');
                });
        }

        $('body').on('click', '.DETAILINFO', function () {
            ShowForecastModal($(this).attr('myid'));
        })
    }


        return {
        ACCURACYINIT: function () {
            forecastaccuracy();
        },
        MERGEINIT: function () {
            forecastmerge();
        },
        MARGININIT: function ()
        { marginfun();}
    }
}();