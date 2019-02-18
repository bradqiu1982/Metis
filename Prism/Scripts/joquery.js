var JOQUERY = function () {
    var QUERYINIT = function () {
        var myjotable = null;
        var joholddict = {};

        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        $.post('/Main/JOQueryProducts', {
  
        }, function (output) {
            $('#SearchRange1').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.pdlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
        });

        function searchdata()
        {
            var pdf = $('#SearchRange1').val();
            var pn = $('#PN').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            var dbstr = $('#dblist').val();

            if (pdf == '' && pn == '')
            {
                alert('Input your query condition!');
                return false;
            }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/main/JOQueryData', {
                pdf: pdf,
                pn: pn,
                sdate: sdate,
                edate: edate,
                dbstr: dbstr
            }, function (output) {

                $.bootstrapLoading.end();

                if (myjotable) {
                    myjotable.destroy();
                    myjotable = null;
                }

                $('#jocontentid').empty();

                $('#chartdiv').empty();

                if (output.success == false)
                { return false; }

                var appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                               '</div>';
                $('#chartdiv').append(appendstr);
                drawcolumn(output.chartdata);

                joholddict = output.joholddict;
                $.each(output.jodatalist, function (i, val) {

                    var holdstr = '';
                    if(val.SNHold > 0)
                    {
                        holdstr = '<td class="holddata" myid="' + val.JO + '" style="background-color:aqua;cursor:pointer;">' + val.SNHold + '</td>';
                    }
                    else
                    {
                        holdstr = '<td>' + val.SNHold + '</td>';
                    }

                    var linkstr = '';
                    if (output.datafrom.indexOf('ATE') != -1) {
                        linkstr = '/Main/JOProgress?jo=' + encodeURIComponent(val.JO);
                    }
                    else {
                        linkstr = 'http://wuxinpi.china.ads.finisar.com/CustomerData/JOMesProgress?jo=' + encodeURIComponent(val.JO);
                    }

                     appendstr = '<tr>' +
                        '<td><a href="'+linkstr +'" target="_blank">' + val.JO + '</a></td>' +
                        '<td>' + val.PN + '</td>' +
                        '<td>' + val.Qty + '</td>' +
                        '<td>' + val.ReleaseDate + '</td>' +
                        '<td>' + val.JOStatus + '</td>' +
                        '<td>' + val.SNActive + '</td>' +
                        '<td>' + val.SNClosed + '</td>' +
                        holdstr +
                        '</tr>';
                    $('#jocontentid').append(appendstr);
                });

                myjotable = $('#jodatatable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });
            })
        }

        $('body').on('click', '#searchbtn', function () {
            searchdata();
        });

        $('body').on('click', '.holddata', function () {
            var holdsns = '';
            $.each(joholddict[$(this).attr("myid")], function (i, val) {
                holdsns += val + ',';
            });
            alert(holdsns);
        });
    }

    var PROCESS = function ()
    {
        function searchdata()
        {
            var jo = $('#defjo').val();
            if (jo == '')
            {
                alert('Default JO Number is not offered!');
                return false;
            }

            $.post('/Main/JOProgressData', {
                jo:jo
            }, function (output) {
                $('#chartdiv').empty();
                if (output.success == false)
                { return false; }

                var appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                               '</div>';
                $('#chartdiv').append(appendstr);
                drawcolumn(output.chartdata);
            })
        }

        $(function () {
            searchdata();
        });
    }

    var drawcolumn = function (col_data) {

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
            yAxis: {
                title: {
                    text: col_data.yAxis.title
                }
            },
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
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };

        Highcharts.chart(col_data.id, options);
    }

    return {
        INIT: function () {
            QUERYINIT();
        },
        JOPROCESS:function(){
            PROCESS();
        }
    }
}();