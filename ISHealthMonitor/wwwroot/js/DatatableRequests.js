var DATATABLE_REQUESTS = [];

var sitesDataTable;

var remindersDataTable;

var reminderIntervalsDataTable;

var usersDataTable;

var siteConfigurationDataTable;

var configurationHistoryDataTable;


DATATABLE_REQUESTS.LoadSiteTable = function () {

    DATATABLE_REQUESTS.ClearSiteTable();
    var sitelocation = "/api/Sites/GetSites";

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

        sitesDataTable.rows.add(json).draw();
        sitesDataTable.columns.adjust().draw();
        $("#divLoading").hide();
    });
}

DATATABLE_REQUESTS.ClearSiteTable = function () {
    var table = $('#siteTable').DataTable();
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}


DATATABLE_REQUESTS.LoadRemindersTable = function () {

    DATATABLE_REQUESTS.ClearRemindersTable();
    var sitelocation = "/api/Reminders/GetReminders";

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

        remindersDataTable.rows.add(json).draw();
        $("#divLoading").hide();
    });
}

DATATABLE_REQUESTS.ClearRemindersTable = function () {
    var table = $('#reminderTable').DataTable();
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}

DATATABLE_REQUESTS.LoadReminderIntervalsTable = function () {

    DATATABLE_REQUESTS.ClearReminderIntervalsTable();
    var sitelocation = "/api/ReminderIntervals/GetReminderIntervals";

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

        reminderIntervalsDataTable.rows.add(json).draw();
        $("#divLoading").hide();
    });
}

DATATABLE_REQUESTS.ClearReminderIntervalsTable = function () {
    var table = $('#reminderIntervalsTable').DataTable();
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}


DATATABLE_REQUESTS.LoadUsersTable = function () {

    DATATABLE_REQUESTS.ClearUsersTable();
    var sitelocation = "/api/Users/GetUsers";

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

        usersDataTable.rows.add(json).draw();
        $("#divLoading").hide();
    });
}

DATATABLE_REQUESTS.ClearUsersTable = function () {
    var table = $('#userTable').DataTable();
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}


DATATABLE_REQUESTS.LoadSiteConfigurationsTable = function () {

    DATATABLE_REQUESTS.ClearSiteConfigurationsTable();
    var sitelocation = "/api/Sites/GetSiteReminderConfigurations";

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

        siteConfigurationDataTable.rows.add(json).draw();
        $("#divLoading").hide();
    });
}



DATATABLE_REQUESTS.ClearSiteConfigurationsTable = function () {
    //var table = $('siteConfigurationTable').DataTable();
    var table = siteConfigurationDataTable;
    
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}


DATATABLE_REQUESTS.LoadConfigurationHistoryTable = function (siteID) {

    console.log(siteID);

    DATATABLE_REQUESTS.ClearConfigurationHistoryTable();
    var sitelocation = "/api/Reminders/GetReminderConfigurationData?siteID=" + siteID;

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

        configurationHistoryDataTable.rows.add(json).draw();
        $("#divLoading").hide();
    });
}

DATATABLE_REQUESTS.ClearConfigurationHistoryTable = function () {
    //var table = $('configurationHistoryTable').DataTable();
    var table = configurationHistoryDataTable;
    table
        .search('')
        .columns().search('')
        .draw();

    table.clear();
}