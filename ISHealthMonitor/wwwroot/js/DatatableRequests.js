var DATATABLE_REQUESTS = [];

DATATABLE_REQUESTS.LoadTable = function () {
   
    DATATABLE_REQUESTS.ClearTable();
    var sitelocation = "/api/Site/GetSites";

    $.ajax({
        type: "GET",
        url: sitelocation,
        async: false,
        cache: false,
        contentType: false,
        enctype: 'multipart/form-data',
        processData: false
    }).done(function (data) {
        var json = JSON.parse(data);
        
        formResults.rows.add(json).draw();
        $("#divLoading").hide();
    });
   
    
}

DATATABLE_REQUESTS.ClearTable = function () {
    var table = $('#siteTable').DataTable();
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}
DATATABLE_REQUESTS.EditRecord = function (id) {
    window.location ="Home/AddEditSite?id=" + id;
    console.log(id);
}

DATATABLE_REQUESTS.DeleteRecord= function (id) {
    console.log(id);
}

