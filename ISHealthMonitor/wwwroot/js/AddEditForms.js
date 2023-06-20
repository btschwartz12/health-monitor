


function initUserSelectize(selectedUserGuid) {

	// Create the selectize for sites
	var $selectUser = $('#select-user').selectize({
		options: [],

		labelField: 'displayName',
		searchField: ['displayName'],
		sortField: 'lastName',
		valueField: 'guid',
		maxItems: null,

		plugins: {
			dropdown_header: {
				title: 'Select a user to set admin privliges for'
			},
			remove_button: {}
		},
	});

	var userControl = $selectUser[0].selectize;
	userControl.disable();
	userControl.settings.placeholder = "Loading...";
	userControl.updatePlaceholder();


	fetch('/api/UsersApi/GetAvailableUsers')
		.then(response => response.json())
		.then(availableUsers => {
			for (let user of availableUsers) {
				userControl.addOption(user);
			}

			userControl.enable();
			userControl.settings.placeholder = "Select user...";
			userControl.updatePlaceholder();

			try {
				if (selectedUserGuid != "") {
					userControl.enable();
					userControl.setValue([selectedUserGuid]);
					userControl.disable();
				}
			} catch (error) {
				console.error('Error setting value:', error);
			}

		})
		.catch(error => console.error('Error:', error));


	userControl.on('change', function () {
		if (userControl.getValue().length > 0) {
			$('#IsAdmin').prop('disabled', false);
			$('#submitButton').prop('disabled', false);
		}
		else {
			$('#IsAdmin').prop('disabled', true);
			$('#submitButton').prop('disabled', true);
		}
	});



}



function SubmitCreateUserForm() {
	var form = $('#userForm');
	$.validator.unobtrusive.parse(form);


	var userControl = $('#select-user')[0].selectize;

	if (userControl.getValue().length == 0) {
		$('#errorMessage').text('Please select a user.');
		$('#errorMessage').removeClass('d-none');
		return;
	}


	var selectedUsers = userControl.getValue().map(function (guid) {
		return userControl.options[guid];
	});

	var data = selectedUsers.map(function (selectedUser) {
		return {
			'ID': $('#ID').val(),
			"IsAdmin": $('#IsAdmin').prop('checked'),
			'Guid': selectedUser.guid,
			"DisplayName": selectedUser.displayName,
			'Action': "x"
		};
	});

	console.log(data);

	if ($(form).valid()) {
		$.ajax({
			type: "post",
			url: '/api/UsersApi/CreateUser',
			data: JSON.stringify(data),
			traditional: true,
			dataType: 'json',
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				if (data) {
					$('#userAddEditModal').modal('hide');
					DATATABLE_REQUESTS.LoadUsersTable();
				}
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

function FetchSSLCertificate() {
	const urlInput = $('#SiteURL');
	const url = urlInput.val();

	// Basic URL validation
	//if (!url || !url.startsWith('http://') && !url.startsWith('https://')) {
	//	$('#errorMessage').text('Please enter a valid URL.');
	//	$('#errorMessage').removeClass('d-none');
	//	return;
	//}

	// Show spinner
	$('#spinner').removeClass('d-none');

	$.ajax({
		url: '/api/SitesApi/GetSiteCertificate',
		type: 'POST',
		dataType: 'json',
		contentType: 'application/json',
		data: JSON.stringify(url), // make it work with from body HERE
		success: function (data) {
			$('#SSLEffectiveDate').val(data.EffectiveDate);
			$('#SSLExpirationDate').val(data.ExpDate);
			$('#SSLIssuer').val(data.Issuer);
			$('#SSLSubject').val(data.Subject);
			$('#SSLCommonName').val(data.CommonName);
			$('#SSLThumbprint').val(data.Thumbprint);

			if (data.ErrorCommonName) {
				$('#errorMessage').text('Invalid Common Name');
				$('#errorMessage').removeClass('d-none');
			}
			else {
				$('#submitButton').prop('disabled', false);
				$('#errorMessage').addClass('d-none');
				
			}

			// Hide spinner
			$('#spinner').addClass('d-none');
		},
		error: function (xhr, textStatus, error) {
			console.log(error);
			$('#errorMessage').text('Failed to fetch the certificate info.');
			$('#errorMessage').removeClass('d-none');

			// Hide spinner
			$('#spinner').addClass('d-none');
		}
	});
}


function SubmitCreateSiteForm() {
	var form = $('#siteForm');
	$.validator.unobtrusive.parse(form);
	$.datepicker.formatDate('dd/mm/yy', new Date());

	var data = {
		'ID': $('#ID').val(),
		'SiteURL': $('#SiteURL').val(),
		'SiteName': $('#SiteName').val(),
		'SiteCategory': $('#SiteCategory').val(),
		'SSLEffectiveDate': $('#SSLEffectiveDate').val(),
		'SSLExpirationDate': $('#SSLExpirationDate').val(),
		'SSLIssuer': $('#SSLIssuer').val(),
		'SSLSubject': $('#SSLSubject').val(),
		'SSLCommonName': $('#SSLCommonName').val(),
		'SSLThumbprint': $('#SSLThumbprint').val(),
		'Action': "x"
	};

	console.log(data);

	if ($(form).valid()) {
		$.ajax({
			type: "POST",
			url: '/api/SitesApi/CreateSite',
			data: JSON.stringify(data),
			traditional: true,
			dataType: 'json',
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				if (data) {
					$('#siteAddEditModal').modal('hide');
					DATATABLE_REQUESTS.LoadSiteTable();
				}
			}
		});
	}
	return false;
}


function SubmitCreateReminderForm() {
	var form = $('#reminderForm');
	$.validator.unobtrusive.parse(form);

	var data = {
		'ID': $('#ID').val(),
		"ISHealthMonitorSiteID": $('#ISHealthMonitorSiteID').val(),
		"ISHealthMonitorIntervalID": $('#ISHealthMonitorIntervalID').val(),
		"ISHealthMonitorGroupSubmissionID": $('#ISHealthMonitorGroupSubmissionID').val(),
		'UserName': "x",
		'Action': "x",

	};

	console.log(data);

	if ($(form).valid()) {
		$.ajax({
			type: "post",
			url: '/api/RemindersApi/CreateReminder',
			data: JSON.stringify(data),
			traditional: true,
			dataType: 'json',
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				if (data) {
					$('#reminderAddEditModal').modal('hide');
					DATATABLE_REQUESTS.LoadRemindersTable();
				}
			}
		});
	}
	return false;
}



function SubmitCreateReminderIntervalForm() {
	var form = $('#reminderIntervalForm');
	$.validator.unobtrusive.parse(form);

	var data = {
		'ID': $('#ID').val(),
		"DurationInMinutes": $('#DurationInMinutes').val(),
		"Type": $('#Type').val(),
		"DisplayName": $('#DisplayName').val(),
		'Action': "x"
	};

	console.log(data);

	if ($(form).valid()) {
		$.ajax({
			type: "post",
			url: '/api/ReminderIntervalsApi/CreateReminderInterval',
			data: JSON.stringify(data),
			traditional: true,
			dataType: 'json',
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				if (data) {
					$('#intervalAddEditModal').modal('hide');
					DATATABLE_REQUESTS.LoadReminderIntervalsTable();
				}
			}
		});
	}

	return false;
}