var tablecatch = function () {
    var show = function ()
    {
        $('body').on('click', '#browserdata', function () {
            var myurl = $('#myurl').val();
            if(myurl != '')
            {
                $('#myframe').attr('src', myurl);
            }
        });

        $('body').on('click', '#gettabdata', function () {
            var iframe = document.getElementById("myframe");
            var iframe_contents = iframe.contentWindow.document.body.innerHTML;
            $.base64.utf8encode = true;
            var bcontent = $.base64.btoa(iframe_contents);

            $.post('/main/GetDataFromPage',
                {
                    content: bcontent
                },
                function (output) {
                    if (output.success) {
                        window.open(output.url, '_blank');
                    }
                    else {
                        alert('Sorry, we failed to get data. Please contact the website developer!');
                    }
                });
        });

    }

    return {
        init: function () {
            show();
        }
    }
}();