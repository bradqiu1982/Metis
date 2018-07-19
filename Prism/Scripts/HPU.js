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
                        "order": []
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

            $.post('/DataAnalyze/SerialHPUData', {
                serial: serial
            }, function (output) {
                if (output.success) {

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

                        var hpucode = line.HPUCode;
                        if (line.DetailLink != '') {
                            hpucode = '<a href="/DataAnalyze/PNHPU?PNLink=' + line.DetailLink + '" target="_blank">' + hpucode + '</a>';
                        }

                        $("#hpumaintableid").append('<tr>' +
                            '<td>' + line.TypicalPN + '</td>' +
                            '<td>' + line.ProductLine + '</td>' +
                            '<td>' + line.Serial + '</td>' +
                            '<td>' + line.Phase + '</td>' +
                            '<td>' + line.YieldHPU + '</td>' +
                            '<td>' + line.Quarter + '</td>' +
                            '<td>' + line.Remark + '</td>' +
                            + '</tr>');
                    }


                    hputable = $('#hpumaintable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": []
                    });
                }
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })
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
                        "order": []
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