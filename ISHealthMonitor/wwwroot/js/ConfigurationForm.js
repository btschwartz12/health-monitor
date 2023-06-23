var _selections = {
    // this will store the current selections for the single box
    'same_selections': [],
    // this will store the current selections for the selected sites,
    // where the key is the id and the value is the selections
    'dynamic_selections': {}
};

var available_reminder_intervals = [];

var currentGroupID = 0;



function initSelectize(modelData) {


    console.log(modelData);


    initSitesSelectize(modelData);
    initIntervalSelectize(modelData);

    currentGroupID = modelData.groupID;


    let siteIdIntervalMap = new Map();

    modelData.userReminders.forEach(reminder => {
        let siteId = reminder.site.id;
        let intervalId = reminder.reminderInterval.id;

        // If the map already has the siteId as key, append the new intervalId
        // Otherwise, create a new key-value pair with siteId as key and an array with intervalId as value
        if (siteIdIntervalMap.has(siteId)) {
            siteIdIntervalMap.get(siteId).push(intervalId);
        } else {
            siteIdIntervalMap.set(siteId, [intervalId]);
        }
    });


    console.log(siteIdIntervalMap);


    fetchAvailableReminderIntervals(modelData)
        .then(() => fetchAvailableSites(modelData))
        .then(() => {

            renderReminderSection();

            let availableReminderIntervalIDs = new Set(available_reminder_intervals.map(interval => interval.id));


            siteIdIntervalMap.forEach((intervalIds, siteId) => {
                let siteBox = $('#select-reminders-' + siteId)[0];
                if (siteBox === undefined) {
                    console.error('Undefined site box for site id: ' + siteId)
                } else {
                    let filteredIntervalIds = intervalIds.filter(intervalId => {
                        if (!availableReminderIntervalIDs.has(intervalId)) {
                            console.error('Unknown interval found with id: ' + intervalId);
                            return false;
                        }
                        return true;
                    });
                    console.log('setting site: ' + siteId + ' to: ' + filteredIntervalIds);
                    siteBox.selectize.setValue(filteredIntervalIds);
                }
            });




        })
        .catch((error) => {
            console.error('Error:', error);
        });

    


    // Every time the checkbox is toggled, either show the single selection
    // box or the section that has as many boxes as sites selected
    $('#sameReminder').on('change', function () {
        if ($(this).is(':checked')) {
            var siteControl = $('#select-sites')[0].selectize;
            var selectedSites = siteControl.getValue().map(function (id) {
                return siteControl.options[id];
            });

            var firstSiteSelections = null;

            for (let site of selectedSites) {
                let siteSelectize = $('#select-reminders-' + site.id)[0].selectize;
                let siteSelections = siteSelectize.getValue().join(',');

                if (firstSiteSelections === null) {
                    firstSiteSelections = siteSelections;
                } else if (firstSiteSelections !== siteSelections) {
                    var result = confirm('Beware! Your changes will be overwritten, do you wish to continue?');
                    if (!result) {
                        $(this).prop('checked', false);
                        return;
                    } else {
                        break;
                    }
                }
            }
            $('.same-reminder-row').show();
            $('.site-reminder-section').show();
        }

        else {
            $('.same-reminder-row').hide();
            $('.site-reminder-section').show();
        }


        // Whenever the checkbox is toggled, re-render all the selections
        renderReminderSection();

        validateForm(false);
    });

    
}

function fetchAvailableSites(modelData) {

    var sitesControl = $('#select-sites')[0].selectize;

    return fetch('/api/Sites/GetSitesToSelect')
        .then(response => response.json())
        .then(availableSites => {
            for (let site of availableSites) {
                sitesControl.addOption(site);
            }

            sitesControl.enable();
            sitesControl.settings.placeholder = "Select sites...";
            sitesControl.updatePlaceholder();

            // Now that the sites are loaded, individual selection sections
            // are render, we can fetch the reminder intervals and populate those
            var siteIDs = modelData.userReminders.map(ur => ur.site.id); 

            
            let availableSiteIDs = new Set(availableSites.map(site => site.id));

            // We need to make sure that an unknown siteID doesn't make its way to the selectize box
            let filteredSiteIDs = siteIDs.filter(siteID => {
                if (!availableSiteIDs.has(siteID)) {
                    console.error('Unknown site found with id: ' + siteID);
                    return false;
                }
                return true;
            });

            // Now use the filteredSiteIDs
            sitesControl.setValue(filteredSiteIDs);


            if (!$('#sameReminder').is(':checked')) {
                $('.same-reminder-row').hide();
            }
        })
        .catch(error => console.error('Error:', error));
}


function fetchAvailableReminderIntervals(modelData) {

    var remindersControl = $('#select-reminders')[0].selectize;

    return fetch('/api/ReminderIntervals/GetIntervalsToSelect')
        .then(response => response.json())
        .then(reminderIntervals => {
            available_reminder_intervals = reminderIntervals;
            for (let reminderInterval of available_reminder_intervals) {
                remindersControl.addOption(reminderInterval);
            }
            remindersControl.enable();
            remindersControl.settings.placeholder = "Select reminders for all sites...";
            remindersControl.updatePlaceholder();
        })
        .catch(error => console.error('Error:', error));
}


function initSitesSelectize(modelData) {

    // Create the selectize for sites
    var $selectSites = $('#select-sites').selectize({
        options: [],
        optionGroupRegister: function (optgroup) {
            var capitalised = optgroup.charAt(0).toUpperCase() + optgroup.substring(1);
            var group = {
                label: 'Category: ' + capitalised
            };

            group[this.settings.optgroupValueField] = optgroup;

            return group;
        },
        optgroupField: 'siteCategory',
        labelField: 'siteName',
        searchField: ['siteName'],
        sortField: 'siteName',
        valueField: 'id',
        maxItems: null,
        
        plugins: {
            dropdown_header: {
                title: 'Select sites that you want to be reminded when their SSL certs expire'
            },
            clear_button: {},
            remove_button: {}
        },                                                                                                                                                                                                                                                                                                                           
        //render: {
        //    item: function (data) {
        //        return '<div class="selected-item"><a href="' + data.siteURL + '" class="item-button" target="_blank">' + data.siteName + '</a></div>';

        //    }
        //},
        //onItemAdd: function (value, $item) {
        //    $item.find('.item-button').on('click', function (event) {
        //        event.preventDefault();
        //        var url = $(event.target).attr('href');
        //        window.location.href = url;
        //    });
        //}
    });

    // Now disable the sites selection until api fetches

    var sitesControl = $selectSites[0].selectize;
    sitesControl.disable();
    sitesControl.settings.placeholder = "Loading...";
    sitesControl.updatePlaceholder();


    

    // Whenever a site is added or removed, re-render all the selections
    sitesControl.on('change', function () {
        renderReminderSection();
        validateForm(false);
    });
}

function initIntervalSelectize(modelData) {

    
    // Set up selection box for when user wants all sites to have same reminders
    var $selectReminders = $('#select-reminders').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'displayName',
        searchField: 'displayName',
        options: [],
        plugins: ['remove_button'],
        create: false
    });

    var remindersControl = $selectReminders[0].selectize;
    remindersControl.disable();
    remindersControl.settings.placeholder = "Loading...";
    remindersControl.updatePlaceholder();

    remindersControl.on('change', function () {
        var sameValues = remindersControl.getValue();
        propagateSameSelections(sameValues);
        validateForm(false);
    });
}

function propagateSameSelections(sameValues) {

    if (!$('#sameReminder').is(':checked')) {
        return
    }

    var siteControl = $('#select-sites')[0].selectize;
    var selectedSites = siteControl.getValue().map(function (id) {
        return siteControl.options[id];
    });

    for (let site of selectedSites) {
        let siteSelectize = $('#select-reminders-' + site.id)[0].selectize;
        siteSelectize.enable();
        siteSelectize.setValue(sameValues);
        siteSelectize.disable();
    }
}

function updateSelections() {

    // Grab the current selections for the same reminders section
    var remindersControl = $('#select-reminders')[0].selectize;
    var selectedReminders = remindersControl.getValue().map(function (id) {
        return remindersControl.options[id];
    });
    _selections['same_selections'] = selectedReminders;


    // Grab the selected sites, then use that to find out the selections for those
    var siteControl = $('#select-sites')[0].selectize;
    var selectedSites = siteControl.getValue().map(function (id) {
        return siteControl.options[id];
    });
    // temporarily store each selection
    var selectedRemindersDict = {};



    for (let site of selectedSites) {

        try {
            remindersControl = $('#select-reminders-' + site.id)[0].selectize;
            selectedReminders = remindersControl.getValue().map(function (id) {
                return remindersControl.options[id];
            });
            selectedRemindersDict[site.id] = selectedReminders;
        }
        catch (e) {
            // This will error when it tries to call selectize() on a section
            // that has not yet been rendered, which is okay in some cases

            //console.log(site);
            //console.error(e);
        }
    }

    
     _selections['dynamic_selections'] = selectedRemindersDict;
    

    //console.log(_selections);

}



function renderReminderSection() {


    updateSelections();

    var siteControl = $('#select-sites')[0].selectize;
    var selectedSites = siteControl.getValue().map(function (id) {
        return siteControl.options[id];
    });
    //console.log(selectedSites);
    var $reminderSection = $('.site-reminder-section');
    $reminderSection.empty();

    //if ($('#sameReminder').is(':checked')) {
    //    $('.site-reminder-section').hide();
    //}

    for (var i = 0; i < selectedSites.length; i++) {
        var color = i % 2 === 0 ? 'black' : 'blue';
        var $row = $('<div class="row justify-content-center" style="text-align: center; margin-bottom: 10px;"></div>');
        var $group = $('<div class="form-group col-6"></div>'); 


        var $label = $('<a href="' + selectedSites[i].siteURL + '" style="display: block; margin-bottom: 10px; color: ' + color + ';" target="_blank">' + selectedSites[i].siteName + ' </a>');


        var $select = $('<select id="select-reminders-' + selectedSites[i].id + '" class="site-reminder" placeholder="Select reminders..." style="margin: 0 auto;"></select>');

        $row.append($label);
        $row.append($select);





        $group.append($label);
        $group.append($select);
        $row.append($group);
        $reminderSection.append($row);

        $select.selectize({
            maxItems: null,
            valueField: 'id',
            labelField: 'displayName',
            searchField: 'displayName',
            options: available_reminder_intervals,
            plugins: ['remove_button'],
            create: false,
            
        });

        let siteID = selectedSites[i].id;
        let siteSelectize = $('#select-reminders-' + siteID)[0].selectize;


        siteSelectize.on('change', function () {
            validateForm(false);
        });


        if ($('#sameReminder').is(':checked')) {
            let sameSelectize = $('#select-reminders')[0].selectize;
            let sameValues = sameSelectize.getValue();
            siteSelectize.setValue(sameValues);
            siteSelectize.settings.placeholder = "Select reminders above..."
            siteSelectize.updatePlaceholder();
            siteSelectize.disable();
        } else {
            let siteID = selectedSites[i].id;
            if (_selections['dynamic_selections'].hasOwnProperty(siteID)) {
                let siteSelections = _selections['dynamic_selections'][siteID].map(selection => selection.id);
                siteSelectize.setValue(siteSelections);
            }
        }
    }
}


function validateForm(showError) {
    var siteControl = $('#select-sites')[0].selectize;
    var sites = siteControl.getValue().map(function (id) {
        var site = siteControl.options[id];
        return { name: site.siteName, id: site.id };
    });

    var isValid = true; // Start with assuming the form is valid

    if (sites.length === 0) {
        if (showError) {
            $('#error-message').text('Please select at least one site.');
            $('#error-message').removeClass('d-none');
        }
        
        // Add red border to the site selection control
        $('#select-sites')[0].nextElementSibling.style.borderColor = "red";
        isValid = false;
    } else {
        // If sites are selected remove the red border
        $('#select-sites')[0].nextElementSibling.style.borderColor = "";
    }

    for (var site of sites) {

        var remindersControl = $('#sameReminder').is(':checked') ? $('#select-reminders')[0].selectize : $('#select-reminders-' + site.id)[0]?.selectize;

        if (!remindersControl) {
            continue;
        }
        var selectedReminders = remindersControl.getValue().map(function (id) {
            return remindersControl.options[id];
        });

        if (selectedReminders.length === 0) {
            var errorMessage = $('#sameReminder').is(':checked') ? 'Please select at least one reminder.' : 'Please select at least one reminder for site ' + site.name + '.';
            if (showError) {
                $('#error-message').text(errorMessage);
                $('#error-message').removeClass('d-none');
            }
            
            // Add red border to the reminder selection control
            remindersControl.$control[0].style.borderColor = "red";
            isValid = false;
        } else {
            // If reminders are selected remove the red border
            remindersControl.$control[0].style.borderColor = "";
        }
    }

    // If the form is valid, clear the error message
    if (isValid) {
        $('#error-message').text('');
        $('#error-message').addClass('d-none');
    }

    return isValid;
}

function SubmitConfigurationForm() {
    var form = $('#configurationBuilderForm');
    $.validator.unobtrusive.parse(form);

    //var data = getDataTest();
    var data = getData();

    if (!data) {
        return false;
    }

    console.log(data);

    if ($(form).valid()) {
        $.ajax({
            type: "post",
            url: '/api/Reminders/CreateConfiguration',
            data: JSON.stringify(data),
            traditional: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data) {
                    returnHome();
                }
            }
        });
    }
    return false;
}
    
    
function getData() {

    if (!validateForm(true)) {
        return false;
    }

    var siteControl = $('#select-sites')[0].selectize;
    var sites = siteControl.getValue().map(function (id) {
        var site = siteControl.options[id];
        return { SiteName: site.siteName, ID: site.id, SiteURL: "x", 'SiteCategory': "x", SSLExpirationDate: "x", SSLEffectiveDate: "x", SSLIssuer: "x", SSLSubject: "x", SSLCommonName: "x", SSLThumbprint: "x", Action: "x" };
    });

    var userReminders = [];
    if ($('#sameReminder').is(':checked')) {
        var remindersControl = $('#select-reminders')[0].selectize;
        var selectedReminders = remindersControl.getValue().map(function (id) {
            var reminder = remindersControl.options[id];
            return { ID: reminder.id, DisplayName: reminder.displayName, Type: "x", Action: "x", DurationInMinutes: 0 }
        });

        sites.forEach(function (site) {
            selectedReminders.forEach(function (reminder) {
                userReminders.push({
                    Site: site,
                    ReminderInterval: reminder
                });
            });
        });
    } else {
        for (var site of sites) {
            var remindersControl = $('#select-reminders-' + site.ID)[0].selectize;
            var selectedReminders = remindersControl.getValue().map(function (id) {
                var reminder = remindersControl.options[id];
                return { ID: reminder.id, DisplayName: reminder.displayName, Type: "x", Action: "x", DurationInMinutes: 0 }
            });

            selectedReminders.forEach(function (reminder) {
                userReminders.push({
                    ID: 0,
                    UserName: "x",
                    ISHealthMonitorSiteID: 0,
                    ISHealthMonitorIntervalID: 0,
                    ISHealthMonitorGroupSubmissionID: 0,
                    Action: "x",


                    Site: site,
                    ReminderInterval: reminder
                });
            });
        }
    }

    var data = {
        GroupID: currentGroupID,
        UserReminders: userReminders
    }

    return data;
}


