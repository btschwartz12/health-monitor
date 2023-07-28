$(document).ready(function () {
    // Fetch tables and operations from the API
    fetch('/api/SplunkBuilder/GetTables')
        .then(response => response.json())
        .then(data => {
            // Populate the tables dropdown
            var tableSelect = $("#tableSelect");
            $.each(data, function (i, item) {
                tableSelect.append(new Option(item, i));
            });
        });

    var operations;
    fetch('/api/SplunkBuilder/GetOperations')
        .then(response => response.json())
        .then(data => {
            operations = data; // Save operations for later use

            // Populate the operation dropdown in the filter modal
            var operationSelect = $("#filterOperation");
            $.each(data, function (i, item) {
                operationSelect.append(new Option(item, i));
            });
        });

    // Add Filter button click in the modal
    $("#addFilterBtn").click(function () {
        var filterContainer = $('<div>').addClass('filter');
        var columnInput = $('<input>').attr({ type: 'text', placeholder: 'Column', value: $("#filterColumn").val() }).addClass('form-control');
        var operationSelect = $('<select>').addClass('form-control');
        $.each(operations, function (i, item) {
            operationSelect.append(new Option(item, i));
        });
        operationSelect.val($("#filterOperation").val());
        var valueInput = $('<input>').attr({ type: 'text', placeholder: 'Value', value: $("#filterValue").val() }).addClass('form-control');
        filterContainer.append(columnInput, operationSelect, valueInput);
        $("#filtersContainer").append(filterContainer);
        $('#filterModal').modal('hide'); // Close the modal

        // Clear the input fields after adding the filter
        $("#filterColumn").val('');
        $("#filterOperation").val('');
        $("#filterValue").val('');
    });

    // Add Sort button click in the modal
    $("#addSortBtn").click(function () {
        $("#sortContainer").empty();
        var sortContainer = $('<div>').addClass('sort');
        var columnInput = $('<input>').attr({ type: 'text', placeholder: 'Column', value: $("#sortColumn").val() }).addClass('form-control');
        var orderSelect = $('<select>').addClass('form-control').append(new Option('ASC'), new Option('DESC'));
        orderSelect.val($("#sortOrder").val());
        sortContainer.append(columnInput, orderSelect);
        $("#sortContainer").append(sortContainer);
        $('#sortModal').modal('hide'); // Close the modal

        // Clear the input fields after adding the sort rule
        $("#sortColumn").val('');
        $("#sortOrder").val('');
    });

    // Build Query button click
    $("#buildQueryBtn").click(function () {
        var table = $("#tableSelect").val();
        var filters = $(".filter").map(function () {
            return {
                column: $(this).children('input').eq(0).val(),
                operation: operations[$(this).children('select').eq(0).val()],
                value: $(this).children('input').eq(1).val()
            };
        }).get();
        var sort = $(".sort").map(function () {
            return {
                column: $(this).children('input').eq(0).val(),
                order: $(this).children('select').eq(0).val()
            };
        }).get();

        var data = {
            table: table,
            filters: filters,
            sort: sort.length > 0 ? sort[0] : null
        };

        fetch('/api/SplunkBuilder/BuildQuery', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        }).then(response => response.text())
            .then(query => {
                $("#queryResult").text(query);
            });
    });
});
