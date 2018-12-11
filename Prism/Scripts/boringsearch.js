var BORINGSEARCH = function () {
    var SEARCHINIT = function () {

        var tablearray = new Array();

        var fillsearchrange = function()
        {
            if ($('#SearchRange1').data('autocomplete') || $('#SearchRange1').data('uiAutocomplete')) {
                $('#SearchRange1').autoComplete('destroy');
            }

            $('#SearchRange1').val('');
            $('#SearchRange1').attr('readonly', true);

            var searchfield = $('#SearchFieldList1').val();
            $.post('/Main/BoringSearchRange', {
                searchfield : searchfield
            }, function (output) {
                $('#SearchRange1').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.srange;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    }
                });
                $('#SearchRange1').attr('readonly', false);
            });
        }

        $('body').on('change', '#SearchFieldList1', function () {
            fillsearchrange();
        });

        $(function () {
            fillsearchrange();
        });

        var searchdata = function ()
        {
            var field1 = $('#SearchFieldList1').val();
            var range1 = $('#SearchRange1').val();
            if (range1 == '')
            { return false; }

            $.post('/Main/BoringSearchData', {
                field1: field1,
                range1: range1,
            }, function (output) {
                $.each(tablearray, function (idx, val) {
                    val.destroy();
                });
                tablearray = [];
                $('.v-content').empty();

                $.each(output.obj1, function (idx,val) {
                    var appendstr = '<table class="table table-hover  table-condensed table-striped" id="tab_' + idx + '" style="margin-top:1%">';
                    appendstr += '<thead>';
                    $.each(val.tabletitle, function (i, h) {
                        appendstr+= '<th>'+h+'</th>';
                    })
                    appendstr += '</thead>';
                    appendstr += '<tbody>';
                    $.each(val.tablecontent, function (i, d) {
                        appendstr += '<td>' + d + '</td>';
                    })
                    appendstr += '</tbody>';
                    appendstr += '</table>';
                    appendstr += '<hr/>';
                    $('.v-content').append(appendstr);

                    var tableobj = $('#tab_' + idx).DataTable({
                        "bInfo": false,
                        "aaSorting": [],
                        "order": [],
                        dom: 'Bfrti',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                    tablearray.push(tableobj)
                });
            });
        }

        $('body').on('click', '#searchbtn', function () {
            searchdata();
        });

    }

    return {
        INIT: function () {
            SEARCHINIT();
        }
    }
}();