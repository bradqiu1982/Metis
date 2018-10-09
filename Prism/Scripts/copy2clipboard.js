function copyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.style.position = 'fixed';
    textArea.style.top = 0;
    textArea.style.left = 0;
    textArea.style.width = '2em';
    textArea.style.height = '2em';
    textArea.style.padding = 0;
    textArea.style.border = 'none';
    textArea.style.outline = 'none';
    textArea.style.boxShadow = 'none';
    textArea.style.background = 'transparent';
    textArea.value = text;
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        var successful = document.execCommand('copy');
    } catch (err) {
    }

    document.body.removeChild(textArea);
}

function SelectText(element) {
    var doc = document;
    if (doc.body.createTextRange) {
        var range = document.body.createTextRange();
        range.moveToElementText(element);
        range.select();
    } else if (window.getSelection) {
        var selection = window.getSelection();
        var range = document.createRange();
        range.selectNodeContents(element);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

function copyImgToClipboard(img)
{
    var div = document.createElement('div');
    div.contentEditable = true;
    div.appendChild(img);
    document.body.appendChild(div);
    SelectText(div);
    try {
        var successful = document.execCommand('copy');
        //var msg = successful ? 'successful' : 'unsuccessful';
        //console.log('Copying text command was ' + msg);
    } catch (err) {
        //console.log('Oops, unable to copy');
    }
    document.body.removeChild(div);
}

