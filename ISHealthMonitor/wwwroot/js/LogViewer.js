

console.log(model);

var GLOBAL_MODEL = model;

$('#demo').daterangepicker({
    "timePicker": true,
    "timePicker": true,
    "timePicker24Hour": true,
    "timePickerIncrement": 30,
    "startDate": moment("06/22/2023", "MM/DD/YYYY"),
    "endDate": moment("06/28/2023", "MM/DD/YYYY"),
}, function (start, end, label) {
    console.log("New date range selected: " + start.format('YYYY-MM-DD HH:mm') + ' to ' + end.format('YYYY-MM-DD HH:mm'));
    filterAndRender(start, end); // Call this function when the date range is changed
});

$("#filterButton").click(function () {
    var start = $('#demo').data('daterangepicker').startDate;
    var end = $('#demo').data('daterangepicker').endDate;
    filterAndRender(start, end); // Call this function when the "Filter" button is clicked
});

function filterAndRender(start, end) {
    var isSystemLogChecked = $('#isSystemLogCheck').prop('checked');
    var checkedTypes = [];
    $("input:checkbox:checked").each(function () {
        checkedTypes.push($(this).attr('id'));
    });

    var filteredModel = filterModel(start, end, isSystemLogChecked, checkedTypes);
    generateAccordion(filteredModel);
}

function filterModel(start, end, isSystemLogChecked, checkedTypes) {
    var startDate = moment(start, 'YYYY-MM-DD HH:mm');
    var endDate = moment(end, 'YYYY-MM-DD HH:mm');

    var filteredModel = JSON.parse(JSON.stringify(GLOBAL_MODEL));
    filteredModel.logFiles = filteredModel.logFiles.filter(file => {
        var fileDate = moment(file.date, 'YYYY-MM-DD');
        return fileDate.isBetween(startDate, endDate, 'day', '[]');
    });

    filteredModel.logFiles.forEach(file => {
        file.logEntries = file.logEntries.filter(entry => {
            var isWantedType = checkedTypes.includes(entry.type);

            var isWantedSystemFilter = isSystemLogChecked || !entry.isSystemLog;

            return isWantedType && isWantedSystemFilter;
        });
    });

    return filteredModel;
}




function generateAccordion(filteredModel) {
    var accordion = $('#accordion');
    accordion.empty();


    const options = {
        weekday: 'long',  // Full name of the day of the week (e.g., "Monday")
        day: 'numeric',   // Numeric day of the month (e.g., 1, 2, 3)
        month: 'long',    // Full name of the month (e.g., "January")
        year: 'numeric'   // Full year (e.g., 2023)
    };


    for (var i = 0; i < filteredModel.logFiles.length; i++) {

        var logFile = filteredModel.logFiles[i];

        const date = new Date(logFile.date);
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
        for (var j = 0; j < logFile.logEntries.length; j++) {
            var logEntry = logFile.logEntries[j];

            // Format the timestamp to only include the time
            var timestamp = moment(logEntry.timestamp).format('HH:mm:ss');

            var row = $('<tr>');
            row.append($('<td>').text(timestamp));
            row.append($('<td>').text(logEntry.type));

            var contentCell = $('<td>').text(logEntry.content);
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
