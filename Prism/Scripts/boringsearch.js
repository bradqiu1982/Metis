var BORINGSEARCH = function () {
    var SEARCHINIT = function () {

        $('body').on('change', '#SearchFieldList1', function () {
            if ($('#SearchRange1').data('autocomplete') || $('#SearchRange1').data('uiAutocomplete')) {
                $('#SearchRange1').autoComplete("destroy");
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
        });

        $('body').on('change', '#SearchFieldList2', function () {
            if ($('#SearchRange2').data('autocomplete') || $('#SearchRange2').data('uiAutocomplete')) {
                $('#SearchRange2').autoComplete("destroy");
            }
            $('#SearchRange2').val('');
            $('#SearchRange2').attr('readonly', true);

            var searchfield = $('#SearchFieldList2').val();
            $.post('/Main/BoringSearchRange', {
                searchfield: searchfield
            }, function (output) {
                $('#SearchRange2').autoComplete({
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
                $('#SearchRange2').attr('readonly', false);
            });
        });

        var searchdata = function ()
        {
            var field1 = $('#SearchFieldList1').val();
            var range1 = $('#SearchRange1').val();
            if (range1 == '')
            { return false; }

            var field2 = $('#SearchFieldList2').val();
            var range2 = $('#SearchRange2').val();
                
            $.post('/Main/BoringSearchData', {
                field1: field1,
                range1: range1,
                field2: field2,
                range2: range2
            }, function (output) {

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