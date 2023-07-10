

console.log(model);

var GLOBAL_MODEL = model;

var CUR_START_DATE = GLOBAL_MODEL.lastWeek;
var CUR_END_DATE = GLOBAL_MODEL.today;

$('#demo').daterangepicker({
    "timePicker": true,
    "timePicker": true,
    "timePicker24Hour": true,
    "timePickerIncrement": 30,
    //"startDate": moment(CUR_START_DATE, "MM/DD/YYYY"),
    "startDate": moment(CUR_END_DATE, "MM/DD/YYYY"), // Have it just be todays date
    "endDate": moment(CUR_END_DATE, "MM/DD/YYYY"),
}, function (start, end, label) {
    //CUR_START_DATE = start;
    //CUR_END_DATE = end;
    console.log("New date range selected: " + start.format('YYYY-MM-DD HH:mm') + ' to ' + end.format('YYYY-MM-DD HH:mm'));
    //filterAndRender(start, end); // Call this function when the date range is changed
});

$("#filterButton").click(function () {
    CUR_START_DATE = $('#demo').data('daterangepicker').startDate;
    CUR_END_DATE = $('#demo').data('daterangepicker').endDate;
    filterAndRender(CUR_START_DATE, CUR_END_DATE); 
});

function filterAndRender(start, end) {
    var isSystemLogChecked = $('#isSystemLogCheck').prop('checked');
    var checkedTypes = [];
    $("input:checkbox:checked").each(function () {
        checkedTypes.push($(this).attr('id'));
    });

    $('#submitLogSpinner').removeClass('d-none');

    var formattedStart = start.format('YYYY-MM-DD');
    var formattedEnd = end.format('YYYY-MM-DD');

    var payload = "?startInclusive=" + formattedStart + "&endInclusive=" + formattedEnd; 


    $('#alertLogs').addClass('d-none')

    var accordion = $('#accordion');
    accordion.empty();

    fetch('/api/Log/GetLogsInRange' + payload)
        .then(response => response.json())
        .then(respData => {

            $('#submitLogSpinner').addClass('d-none');


            var logViewerModel = respData;

            if (logViewerModel.LogFiles.length == 0) {
                $('#alertLogs').removeClass('d-none').text('No logs found for specified range');
                return;
            }

            var filteredModel = filterModel(logViewerModel, isSystemLogChecked, checkedTypes);
            generateAccordion(filteredModel);

            

        })
        .catch(error => console.error('Error:', error));


    
}

function filterModel(logViewerModel, isSystemLogChecked, checkedTypes) {


    var filteredModel = logViewerModel;


    filteredModel.LogFiles.forEach(file => {
        file.LogEntries = file.LogEntries.filter(entry => {
            var isWantedType = checkedTypes.includes(entry.Type);

            var isWantedSystemFilter = isSystemLogChecked || !entry.IsSystemLog;

            return isWantedType && isWantedSystemFilter;
        });
    });

    return filteredModel;
}




function generateAccordion(filteredModel) {
    var accordion = $('#accordion');
    accordion.empty();


    const options = { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' };


    for (var i = 0; i < filteredModel.LogFiles.length; i++) {

        var logFile = filteredModel.LogFiles[i];

        const date = new Date(logFile.Date);
        var dateStr = date.toLocaleDateString('en-US', options);

        
        var card = $('<div>').addClass('card');
        var cardHeader = $('<div>').addClass('card-header').attr('id', 'heading' + i);
        var h5 = $('<h5>').addClass('mb-0');
        var button = $('<button>').addClass('btn').attr({
            'data-toggle': 'collapse',
            'data-target': '#collapse' + i,
            'aria-expanded': 'true',
            'aria-controls': 'collapse' + i
        }).text(dateStr);

        var collapse = $('<div>').addClass('collapse').attr({
            'id': 'collapse' + i,
            'aria-labelledby': 'heading' + i,
            'data-parent': '#accordion'
        });


        var cardBody = $('<div>').addClass('card-body table-responsive');

        // Create the table
        var table = $('<table>').addClass('table table-striped');

        // Create the table header
        var thead = $('<thead>');
        var headerRow = $('<tr>');
        headerRow.append($('<th>').text('Timestamp'));
        headerRow.append($('<th>').text('Type'));
        headerRow.append($('<th>').text('Content'));
        thead.append(headerRow);
        table.append(thead);

        // Create the table body
        var tbody = $('<tbody>');


        // Add log entries to the table body
        for (var j = 0; j < logFile.LogEntries.length; j++) {
            var logEntry = logFile.LogEntries[j];

            // Format the timestamp to only include the time
            var timestamp = moment(logEntry.Timestamp).format('HH:mm:ss');

            var row = $('<tr>');
            row.append($('<td>').text(timestamp));
            row.append($('<td>').text(logEntry.Type));

            var contentCell = $('<td>').text(logEntry.Content);
            contentCell.css("word-wrap", "break-word");
            contentCell.css("white-space", "pre");
            row.append(contentCell);
            tbody.append(row);
        }

        table.append(tbody);

        // Add the table to the card body
        cardBody.append(table);



        h5.append(button);
        cardHeader.append(h5);
        collapse.append(cardBody);
        card.append(cardHeader);
        card.append(collapse);
        accordion.append(card);
    }
}
