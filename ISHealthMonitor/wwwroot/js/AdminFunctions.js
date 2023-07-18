function showAdminModal() {
    $('#adminModal').modal('show');
    $('#alertFireReminders').addClass('d-none');
    $('#alertRefreshCerts').addClass('d-none');
    $('#alertUpdateConfluence').addClass('d-none');


    $('#fireRemindersBtn').tooltip({
        title: "Send out certificate renewal reminders to all applicable subscribed users",
        placement: 'right'
    });

    $('#refreshCertsBtn').tooltip({
        title: "Fetch each site's certificate information from the CA and update it in the database (this may take a while)",
        placement: 'right'
    });

    $('#updateConfluenceBtn').tooltip({
        title: "Fetch each site's certificate information from the database and update it in the Confluence site",
        placement: 'right'
    });

}



function FireReminders() {
    $('#spinnerFireReminders').removeClass('d-none');

    $.ajax({
        type: "GET",
        url: `/api/users/getloggedinuser`,
        async: true,
        cache: false,
        contentType: 'application/json',
        processData: false,
    })
        .done(function (userData) {

            if (userData.isAdmin === "false") {
                return;
            }

            $.ajax({
                type: "POST",
                url: `/rest/api/jwToken?grant_type=token&userName=${userData.apiAuthUsername}&password=${userData.apiAuthPassword}`,
                async: true,
                cache: false
            })
                .done(function (tokenData) {

                    var token = 'invalid'

                    try {
                        tokenData = JSON.parse(tokenData);
                        token = tokenData.access_token;
                    }
                    catch { }


                    // The second request to fire reminders
                    $.ajax({
                        type: "GET",
                        url: '/api/adminfunctions/firereminders?username=' + userData.username,
                        async: true,
                        cache: false,
                        contentType: false,
                        enctype: 'multipart/form-data',
                        processData: false,
                        beforeSend: function (xhr) {
                            // Include the bearer token in the header
                            xhr.setRequestHeader('Authorization', `Bearer ${token}`);
                        }
                    }).done(function (data) {
                        console.log(data);
                        $('#alertFireReminders').removeClass('d-none').text('Successfully sent reminders!');
                    }).fail(function (error) {
                        console.log('Error:', error);
                        $('#alertFireReminders').removeClass('d-none').text('Failed to send reminders :(');
                    }).always(function () {
                        $('#spinnerFireReminders').addClass('d-none');
                    });
                })
                .fail(function (error) {
                    console.log('Failed to get token:', error);
                    $('#spinnerFireReminders').addClass('d-none');
                });
        })
        .fail(function (error) {
            console.log(error);
            $('#spinnerFireReminders').addClass('d-none');
        }).
        always(function () {
            
        });

    
}

function RefreshCerts() {
    $('#spinnerRefreshCerts').removeClass('d-none');
    $.ajax({
        type: "GET",
        url: `/api/users/getloggedinuser`,
        async: true,
        cache: false,
        contentType: 'application/json',
        processData: false,
    })
        .done(function (userData) {

            if (userData.isAdmin === "false") {
                return;
            }

            $.ajax({
                type: "POST",
                url: `/rest/api/jwToken?grant_type=token&userName=${userData.apiAuthUsername}&password=${userData.apiAuthPassword}`,
                async: true,
                cache: false
            })
                .done(function (tokenData) {

                    var token = 'invalid'

                    try {
                        tokenData = JSON.parse(tokenData);
                        token = tokenData.access_token;
                    }
                    catch { }

                    // The second request to refresh certificates
                    $.ajax({
                        type: "GET",
                        url: '/api/adminfunctions/refreshcerts?username=' + userData.username,
                        async: true,
                        cache: false,
                        contentType: 'application/json',
                        processData: false,
                        beforeSend: function (xhr) {
                            // Include the bearer token in the header
                            xhr.setRequestHeader('Authorization', `Bearer ${token}`);
                            console.log(xhr);
                        }
                    }).done(function (data) {
                        console.log(data);
                        if (data.message === "Success") {
                            $('#alertRefreshCerts').removeClass('d-none').text('Successfully refreshed certificates!');
                        }
                        else if (data.message === "Failed") {
                            var failedUrlCount = Object.keys(data.failedSiteUrls).length;

                            $('#alertRefreshCerts').removeClass('d-none').text('Successfully refreshed some certificates (' + failedUrlCount + ' failed)');
                        }

                    }).fail(function (error) {
                        console.log('Error:', error);
                        $('#alertRefreshCerts').removeClass('d-none').text('Failed to refresh certificates :(');
                    }).always(function () {
                        $('#spinnerRefreshCerts').addClass('d-none');
                    });
                })
                .fail(function (error) {
                    console.log('Failed to get token:', error);
                    $('#spinnerRefreshCerts').addClass('d-none');
                });
        })
        .fail(function (error) {
            console.log(error)
            $('#spinnerRefreshCerts').addClass('d-none');
        }).
        always(function () {
            
        });
    
}

function UpdateConfluence() {
    $('#spinnerUpdateConfluence').removeClass('d-none');

    $.ajax({
        type: "GET",
        url: `/api/users/getloggedinuser`,
        async: true,
        cache: false,
        contentType: 'application/json',
        processData: false,
    })
        .done(function (userData) {

            if (userData.isAdmin === "false") {
                return;
            }

            $.ajax({
                type: "POST",
                url: `/rest/api/jwToken?grant_type=token&userName=${userData.apiAuthUsername}&password=${userData.apiAuthPassword}`,
                async: true,
                cache: false
            })
                .done(function (tokenData) {

                    var token = 'invalid'

                    try {
                        tokenData = JSON.parse(tokenData);
                        token = tokenData.access_token;
                    }
                    catch { }

                    // The second request to update confluence
                    $.ajax({
                        type: "GET",
                        url: '/api/adminfunctions/updateconfluence?username=' + userData.username,
                        async: true,
                        cache: false,
                        contentType: 'application/json',
                        processData: false,
                        beforeSend: function (xhr) {
                            // Include the bearer token in the header
                            xhr.setRequestHeader('Authorization', `Bearer ${token}`);
                        }
                    }).done(function (data) {
                        console.log(data);
                        $('#alertUpdateConfluence').removeClass('d-none').text('Successfully updated the Confluence page!');
                    }).fail(function (error) {
                        console.log('Error:', error);
                        $('#alertUpdateConfluence').removeClass('d-none').text('Failed to update the Confluence page :(');
                    }).always(function () {
                        $('#spinnerUpdateConfluence').addClass('d-none');
                    });
                })
                .fail(function (error) {
                    console.log('Failed to get token:', error);
                    $('#spinnerUpdateConfluence').addClass('d-none');
                });
        })
        .fail(function (error) {
            console.log(error);
            $('#spinnerUpdateConfluence').addClass('d-none');
        }).
        always(function () {
           
        });


    
}