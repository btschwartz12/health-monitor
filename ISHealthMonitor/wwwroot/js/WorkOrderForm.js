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


function CheckWorkOrderStatus() {
	$('#spinnerWorkOrder').removeClass('d-none');

	var siteId = $('#SiteID').val();
	var modal = new bootstrap.Modal(document.getElementById('confirmationModal'));

	return new Promise((resolve, reject) => {
		$.ajax({
			type: "get",
			url: '/api/WorkOrder/CheckWorkOrderStatus?siteId=' + siteId,
			async: true,
			cache: false,
			processData: false,
			traditional: true,
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				$('#spinnerWorkOrder').addClass('d-none');
				console.log(data);

				if (data.message == "Warning") {
					var workOrderObjectId = data.warningData.workOrderObjectId;
					var workOrderUrl = data.warningData.workOrderViewUrl;
					var submissionDate = data.warningData.workOrderSubmissionDate;
					var siteCertEffectiveDate = data.warningData.siteCertEffectiveDate;

					$('#workOrderObjectId').text(workOrderObjectId);
					$('#workOrderLink').attr('href', workOrderUrl);
					$('#workOrderSubmissionDate').text(submissionDate);
					$('#effectiveDate').text(siteCertEffectiveDate);

					modal.show();
					$('#proceedButton').click(function () {
						modal.hide();
						resolve(true); // Resolve the promise with true when "proceed" is clicked
					});

					$('#cancelButton').click(function () {
						modal.hide();
						resolve(false); // Resolve the promise with false when "cancel" is clicked
					});
				}
				else if (data.message == "Ok") {
					resolve(true);
				}
			},
			error: function (resp) {
				console.log(resp);
				var errorMessage = resp.responseJSON && resp.responseJSON.message ? resp.responseJSON.message : 'Error checking work order status.';
				$('#errorMessage').text(errorMessage);
				$('#errorMessage').removeClass('d-none');
				$('#spinnerWorkOrder').addClass('d-none');
				$('#resetButton').prop('disabled', false);
				$('#submitButton').prop('disabled', false);
				reject(false); // Reject the promise when an error occurs
			}
		});
	});
}




function SubmitWorkOrderForm() {

	CheckWorkOrderStatus().then(valid => {
		if (valid) {
			var form = $('#workOrderForm');
			$.validator.unobtrusive.parse(form);

			var urgencyControl = $('#select-urgency')[0].selectize;

			var selectedUrgency = urgencyControl.options[urgencyControl.getValue()].title;

			var emergencyReason = $("#EmergencyReason").val();
			emergencyReason = (emergencyReason === '') ? 'null' : emergencyReason;

			var data = {
				'IssueType': $("#IssueType").val(),
				'Category': $("#Category").val(),
				'System': $("#System").val(),
				'Urgency': selectedUrgency,
				'ShortDescription': $("#ShortDescription").val(),
				'Description': $("#Description").val(),
				'EmergencyReason': emergencyReason,
				'SiteID': $('#SiteID').val()
			};

			console.log(data);

			$('#spinnerWorkOrder').removeClass('d-none');

			$('#resetButton').prop('disabled', true);
			$('#submitButton').prop('disabled', true);

			if ($(form).valid()) {
				$.ajax({
					type: "post",
					url: '/api/WorkOrder/CreateWorkOrder',
					data: JSON.stringify(data),
					traditional: true,
					dataType: 'json',
					contentType: "application/json; charset=utf-8",
					success: function (data) {
						$('#spinnerWorkOrder').addClass('d-none');
						console.log(data);
						console.log('yes');
						$('#resetButton').prop('disabled', false);
						$('#submitButton').prop('disabled', false);
						ReturnHome();
					},
					error: function (resp) {
						console.log(resp);
						var errorMessage = resp.responseJSON && resp.responseJSON.message ? resp.responseJSON.message : 'Error creating work order.';
						$('#errorMessage').text(errorMessage);
						$('#errorMessage').removeClass('d-none');
						$('#spinnerWorkOrder').addClass('d-none');
						$('#resetButton').prop('disabled', false);
						$('#submitButton').prop('disabled', false);
					}
				});
			}
			return false;
		}
	}).catch(error => {
		console.error(error);
	});

	return false;


	
}