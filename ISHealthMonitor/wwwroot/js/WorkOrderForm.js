function initUrgencySelectize(model) {

	// Create the selectize for sites
	var $selectUrgency = $('#select-urgency').selectize({
		options: [
			'1 - Emergency',
			'2 - High',
			'3 - Normal',
			'4 - Low'
		],

		maxItems: 1,
		valueField: 'id',
		labelField: 'title',
		searchField: 'title',
		options: [
			{ id: 1, title: '1 - Emergency' },
			{ id: 2, title: '2 - High', },
			{ id: 3, title: '3 - Normal', },
			{ id: 4, title: '4 - Low', }
		],
		create: false,

		plugins: {
			dropdown_header: {
				title: 'Select an urgency level'
			},
			remove_button: {}
		},
	});


	var urgencyControl = $selectUrgency[0].selectize;

	urgencyControl.setValue(model.urgency);

	//urgencyControl.disable();
	//urgencyControl.settings.placeholder = "Select an ...";
	//urgencyControl.updatePlaceholder();

	urgencyControl.on('change', function () {
		if (urgencyControl.getValue() == 1) {
			$('#emergencyDiv').prop('hidden', false);
		}
		else {
			$('#emergencyDiv').prop('hidden', true);
		}
	});

}


function initWorkOrderForm(model) {

	initUrgencySelectize(model);




	// disable some
	$('#IssueType').prop('disabled', true);
	$('#Category').prop('disabled', true);
	$('#System').prop('disabled', true);

}



function SubmitWorkOrderForm() {
	var form = $('#workOrderForm');
	$.validator.unobtrusive.parse(form);

	var urgencyControl = $('#select-urgency')[0].selectize;

	var selectedUrgency = urgencyControl.options[urgencyControl.getValue()].title;

	var data = {
		'IssueType': $("#IssueType").val(),
		'Category': $("#Category").val(),
		'System': $("#System").val(),
		'Urgency': selectedUrgency,
		'ShortDescription': $("#ShortDescription").val(),
		'Description': $("#Description").val(),
		'EmergencyReason': $("#EmergencyReason").val() + 'x',
	};

	console.log(data);

	if ($(form).valid()) {
		$.ajax({
			type: "post",
			url: '/api/WorkOrder/CreateWorkOrder',
			data: JSON.stringify(data),
			traditional: true,
			dataType: 'json',
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				console.log(data);
				console.log('yes');
				returnHome();
			},
			error: function (resp) {
				console.log(resp);
				var errorMessage = resp.responseJSON && resp.responseJSON.message ? resp.responseJSON.message : 'Error creating users.';
				$('#errorMessage').text(errorMessage);
				$('#errorMessage').removeClass('d-none');
			}
		});
	}
	return false;
}