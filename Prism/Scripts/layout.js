var LAYOUT = function () {
    var LAYOUTINIT = function () {
        
        $('body').on('mouseover', '.top-menum', function () {
            $(this).removeClass("top-menum-def-color").addClass('top-menum-mvin-color');

            $('#submenu').empty();

            var myid = $(this).attr('myid');
            if (myid.indexOf('MAIN') != -1)
            {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">MAIN&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Main/Index">Index</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Main/BoringSearch">Boring Search</span>');
            }
            else if (myid.indexOf('YIELD') != -1)
            {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">YIELD&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Yield/YieldTrend">Yield Trend</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Yield/DepartmentYield">Department Yield</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Yield/ProductYield">Product Yield</span>');
            }
            else if (myid.indexOf('MACHINE') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">MACHINE&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Machine/DepartmentMachine">Department Machine Use Rate</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Machine/ProductMachine">Product Machine Use Rate</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Machine/HydraMachineUsage">Hydra Machine Detail</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Machine/HydraMACRate">Hydra Machine Use Rate</span>');
            }
            else if (myid.indexOf('SHIPMENT') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">SHIPMENT&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/ShipmentData">Shipment Distribution</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/OTDData">OTD Trend</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/LBSDistribution">Shipment LBS</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/OrderData">Order Distribution</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/ShipOutputTrend">Ship Output Trend</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Shipment/RMAWorkLoad">RMA WorkLoad</span>');
            }
            else if (myid.indexOf('SCRAP') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">SCRAP&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Scrap/DepartmentScrap">Department Scrap</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Scrap/CostCenterScrap">Cost Center Scrap</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Scrap/ProductScrap">Product Scrap</span>');
            }
            else if (myid.indexOf('HPU') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">HPU&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/DataAnalyze/HPUTrend">HPU Trend</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/DataAnalyze/DepartmentHPU">Department HPU</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/DataAnalyze/SerialHPU">Serial HPU</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/DataAnalyze/PNHPU">PN HPU</span>');
            }
            else if (myid.indexOf('CAPACITY') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">CAPACITY&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Capacity/DepartmentCapacity">Department Capacity</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Capacity/ProductCapacity">Product Capacity</span>');
            }
            else if (myid.indexOf('INVENTORY') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">INVENTORY&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Inventory/DepartmentInventory">Department Inventory Turns</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/Inventory/ProductInventory">Product Inventory Turns</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="http://wuxinpi.china.ads.finisar.com:8082/BRTrace/ERPComponent">Inventory Item Query</span>');
            }
            else if (myid.indexOf('VCSEL') != -1) {
                $('#submenu').append('<span class="sub-menum-title sub-menum-def-color">VCSEL&nbsp;&nbsp;&nbsp;&nbsp;|</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/VCSEL/VCSELShipmentDppm">VCSEL SHIPMENT DPPM</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/VCSEL/VCSELWaferDppm">VCSEL WAFER DPPM</span>');
                $('#submenu').append('<span class="sub-menum sub-menum-def-color" myurl="/VCSEL/VCSELStatistic">VCSEL RMA STATISTIC</span>');
            }

            $('#defmenudiv').addClass('hide');

            //$('#emptydiv').addClass('hide');
            $('#submenudiv').removeClass('hide');
        });

        $('body').on('mouseout', '.top-menum', function () {
            $(this).removeClass("top-menum-mvin-color").addClass('top-menum-def-color');
        });


        $('body').on('mouseover', '.sub-menum', function () {
            $(this).removeClass("sub-menum-def-color").addClass('sub-menum-mvin-color');
        });

        $('body').on('mouseout', '.sub-menum', function () {
            $(this).removeClass("sub-menum-mvin-color").addClass('sub-menum-def-color');
        });

        $('body').on('click', '.sub-menum', function () {
            var url = $(this).attr('myurl');
            window.open(url, '_self');
        });


        $('body').on('mouseover', '.body-content', function () {
            $('#defmenudiv').removeClass('hide');

            //$('#emptydiv').removeClass('hide');
            $('#submenudiv').addClass('hide');
        });

        $(function () {
            var deftitle = $('#hidetitle').val();
            $('#deftitle').html(deftitle);
        })

    }

    return {
        INIT: function () {
            LAYOUTINIT();
        }
    }
}();