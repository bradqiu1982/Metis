function autoCompleteFill(id, values, flg, container_id) {
    $('#' + id).autoComplete({
        minChars: 0,
        source: function (term, suggest) {
            term = term.toLowerCase();
            var choices = values;
            var suggestions = [];
            for (i = 0; i < choices.length; i++)
                if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
            suggest(suggestions);
        },
        onSelect: function (event, term, item) {
            if (flg) {
                var old_val = $.trim($('#' + container_id).val());
                if (old_val == '') {
                    $('#' + container_id).val(term+";");
                }
                else {
                    var old_arr = old_val.split(";");
                    var exist_flg = false;
                    $.each(old_arr, function (i, val) {
                        if (val == term) {
                            exist_flg = true;
                            return false;
                        }
                    })
                    if (!exist_flg) {
                        $('#' + container_id).val(old_val + ";" + term + ";");
                    }
                }
                $('#' + id).val('');
            }
        }
    });
}
// prevent enter submit
document.onkeydown = function (event) {
    var target, code, tag;
    if (!event) {
        event = window.event;
        target = event.srcElement;
        code = event.keyCode;
        if (code == 13) {
            tag = target.tagName;
            if (tag == "TEXTAREA") { return true; }
            else { return false; }
        }
    }
    else {
        target = event.target;
        code = event.keyCode;
        if (code == 13) {
            tag = target.tagName;
            if (tag == "INPUT") { return false; }
            else { return true; }
        }
    }
};  