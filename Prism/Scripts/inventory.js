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

    var productcost = function () {
        var costtabs = new Array();

        function GetMGPMList()
        {
            $.post('/Inventory/ProductCostPMList', {}, function (output) {
                $('#pmlist').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.pmlist;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    }
                });
                $('#pmlist').removeAttr('ReadOnly');
            });

            $.post('/Inventory/ProductCostPNList', {}, function (output) {
                $('#pdlist').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.pnlist;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    }
                });
                $('#pdlist').removeAttr('ReadOnly');
            });
        }

        $.fn.dataTable.ext.buttons.updatecost = {
            text: 'Update Cost',
            action: function (e, dt, node, config) {
                $.post('/Inventory/UpdateProductCost', {}, function (output) {
                    window.location.reload();
                });
            }
        };

        $.fn.dataTable.ext.buttons.costdetail = {
            text: 'Cost Detail',
            action: function (e, dt, node, config) {
                var detailcla = $(e.currentTarget).attr('aria-controls');
                $('.' + detailcla).removeClass('hide');
            }
        };

        $.fn.dataTable.ext.buttons.costcollapse = {
            text: 'Collapse',
            action: function (e, dt, node, config) {
                var detailcla = $(e.currentTarget).attr('aria-controls');
                $('.' + detailcla).removeClass('hide').addClass('hide');
            }
        };

        function CreateTabs(onecost)
        {
                var tabid = onecost.pn + '_tab';
                var tabstr = '<table class="table table-hover  table-condensed table-striped" id="' + tabid + '" style="margin-top:1%">';
                tabstr += '<caption class="tb-caption">' + onecost.pn + ' Cost</caption>';
                $.each(onecost.table, function (ix, tvals) {
                    if (ix == 0) {
                        tabstr += '<thead>';
                        tabstr += '<tr>';
                        $.each(tvals, function (iy, tval) {
                            tabstr += '<th style="font-size:12px;padding:6px 3px!important">' + tval + '</th>';
                        });
                        tabstr += '</tr>';
                        tabstr += '</thead>';
                    }
                    else {
                        if (ix == 1)
                        { tabstr += '<tbody>'; }
                        if (ix == 4 || ix == 14 || ix == 21) {
                            tabstr += '<tr style="font-size:12px;font-weight: bold;color:#49057a;">';
                        }
                        else { tabstr += '<tr class="'+tabid+' hide" style="font-size:10px;">';}
                        

                        $.each(tvals, function (iy, tval) {
                            if (iy == 0) {
                                tabstr += '<td style="padding:6px 3px!important">' + tval + '</td>';
                            }
                            else {
                                tabstr += '<td style="padding:6px 3px!important">' + tval + '</td>';
                            }
                            
                        });
                        tabstr += '</tr>';
                    }
                });
                tabstr += '</tbody>';
                tabstr += '</table>';
                $('.v-content').append(tabstr);

                var tabhandle = $('#'+tabid).DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5', 'updatecost','costdetail','costcollapse']
                    });
                costtabs.push(tabhandle);

        }

        function DrawCostChart1(onecost)
        {
            var chid = onecost.pn + '_chart1';
            var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                               '<div class="v-box" id="' + chid + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
            $('.v-content').append(appendstr);

            var options = {
            chart: {
                zoomType: 'xy',
                type: 'column'
            },
            title: {
                text: onecost.chart1.title
            },
            xAxis: {
                title: {
                    text: 'Quarter'
                },
                categories: onecost.chart1.xlist
            },
            legend: {
                enabled: true,
            },
            yAxis: {
                title: {
                    text: 'Cost (USD)'
                }
            },
            tooltip: {
                pointFormat: '{series.name} : <b>{point.y} USD</b>'
            },
            series: onecost.chart1.chartseris,
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + chid).parent().toggleClass('chart-modal');
                            $('#' + chid).highcharts().reflow();
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
            Highcharts.chart(chid, options);
        }

        function DrawCostChart2(onecost)
        {
            var chid = onecost.pn + '_chart2';
            var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                               '<div class="v-box" id="' + chid + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
            $('.v-content').append(appendstr);


            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'column',
                    alignTicks: false
                },
                title: {
                    text: onecost.chart2.title
                },
                xAxis: {
                    title: {
                        text: 'Quarter'
                    },
                    categories: onecost.chart2.xlist
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'Cost (USD)'
                    }
                }, {
                    opposite: true,
                    title: {
                        text: 'Percent'
                    },
                    min: -5.0,
                    max: 100
                }],
                tooltip: {
                    pointFormat: '{series.name} : <b>{point.y}</b>'
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                series: onecost.chart2.chartseris,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + chid).parent().toggleClass('chart-modal');
                                $('#' + chid).highcharts().reflow();
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
            Highcharts.chart(chid, options);
        }

        function SolveCostData(output)
        {
            $.each(costtabs, function (i, val) {
                val.destroy();
            });
            costtabs = new Array();
            $('.v-content').empty();

            if (output.datalist.length == 0)
            { alert("No authorization to access this data or no such data.");}

            $.each(output.datalist, function (i, onecost) {
                CreateTabs(onecost);
                DrawCostChart1(onecost);
                DrawCostChart2(onecost);
            });
        }

        //function GetPMSelfCostData()
        //{
        //    $.post('/Inventory/ProductCostPMData', {
        //    }, function (output) {
        //        SolveCostData(output);
        //    });
        //}

        function QueryCostData()
        {
            var pm = $('#pmlist').val();
            var pd = $('#pdlist').val();
            if (pm == '' && pd == '')
            {
                alert('At least one query condition need to be input!');
                return 
            }

            $.post('/Inventory/ProductCostQuery', {
                pm: pm,
                pd: pd
            }, function (output) {
                SolveCostData(output);
            });
        }

        $(function () {
            GetMGPMList();
        });

        $('body').on('click', '#btn-search', function () {
            QueryCostData();
        })

    }

    return {
        DEPARTMENTINIT: function () {
            departmentinvent();
        },
        PRODUCTINIT: function () {
            productinvent();
        },
        PRODUCTCOSTINIT: function () {
            productcost();
        }

    }
}();