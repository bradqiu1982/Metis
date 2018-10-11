var YIELD = function () {
    var departmentyield = function () {
        var yieldtable = null;
        function getallyield()
        {
            $.post('/Yield/DepartmentYieldData',
                {},
                function (output) {
                    if (yieldtable) {
                        yieldtable.destroy();
                    }

                    $("#yieldtabheadid").empty();
                    $("#yieldtabcontentid").empty();

                    $("#yieldtabheadid").append("<tr>");
                    $.each(output.tabletitle, function (i, head) {
                        $("#yieldtabheadid").append("<th>"+head+"</th>");
                    });
                    $("#yieldtabheadid").append("</tr>");

                    $.each(output.tablecontent, function (i, line) {
                        $("#yieldtabcontentid").append("<tr>");
                        $.each(line, function (idx, val) {
                            $("#yieldtabcontentid").append("<td>" + val + "</td>");
                        });
                        $("#yieldtabcontentid").append("</tr>");
                    });

                    yieldtable = $('#yieldmaintable').DataTable({
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
            getallyield();
        });
    }

    var productyield = function () {
    
    }

    return {
        DEPARTMENTINIT: function () {
            departmentyield();
        },
        PRODUCTINIT: function () {
            productyield();
        },
    }
}();
