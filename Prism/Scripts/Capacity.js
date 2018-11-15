var CAPACITY = function () {

    var departmentcapacity = function () {

        var captable = null;
        var capdatatable = null;
        var rawdataobj = {};

        function getallcapacity() {
                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }
                $.bootstrapLoading.start(options);

                $.post('/Capacity/DepartmentCapacityData',
                    {},
                    function (output) {
                        $.bootstrapLoading.end();
                        rawdataobj = output.rawdata;

                        if (captable) {
                            captable.destroy();
                        }
                        $("#capacitytabheadid").empty();
                        $("#capacitytabcontentid").empty();

                        var idx = 0;
                        var titlelength = output.tabletitle.length;
                        var appendstr = "";
                        appendstr += "<tr>";
                        for (idx = 0; idx < titlelength; idx++) {
                            appendstr += "<th>" + output.tabletitle[idx] + "</th>";
                        }
                        appendstr += "<th>Chart</th>";
                        appendstr += "</tr>";
                        $("#capacitytabheadid").append(appendstr);

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
                            //appendstr += "<td><div id='" + output.chartdatalist[idx].id + "' style='max-width:840px!important'></div></td>";
                            appendstr += "<td></td>";
                            appendstr += "</tr>";
                        }
                        $("#capacitytabcontentid").append(appendstr);

                        captable = $('#capacitymaintable').DataTable({
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
            getallcapacity();
        });


        $('body').on('click', '.YIELDDATA', function () {
            showcapacitydata($(this).attr('myid'));
        })

        function showcapacitydata(id) {
            var capdata = rawdataobj[id];

            if (capdatatable) {
                capdatatable.destroy();
            }
            $("#capacitycontentid").empty();

            var appendstr = "";

            $.each(capdata, function (i, val) {
                appendstr += "<tr>";
                appendstr += "<td>" + val.Quarter + "</td>";
                appendstr += "<td>" + val.Product + "</td>";
                appendstr += "<td>" + val.MaxCapacity + "</td>";
                appendstr += "<td>" + val.ForeCast + "</td>";
                appendstr += "<td>" + val.Usage + "</td>";
                appendstr += "<td>" + val.GAP + "</td>";
                appendstr += "<td>" + val.PN + "</td>";
                appendstr += "</tr>";
            })
            $("#capacitycontentid").append(appendstr);

            capdatatable = $('#capacitydatatable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#capacitymodal').modal('show');
        }
    }

    return {
        DEPARTMENTINIT: function () {
            departmentcapacity();
        }
    }
}();