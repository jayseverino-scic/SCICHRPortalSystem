//GLOBAL FUNCTIONS

var noAccessAlert = () => {
    Swal.fire(
        'No Permission!',
        'Action is not allowed.',
        'error'
    );
}

var customizePdf = (doc, tableSelector) => {
    var colCount = new Array();
    var tr = $(tableSelector + ' tbody tr:first-child');
    var trWidth = $(tr).width();

    $(tableSelector).find('tbody tr:first-child td').each(function () {
        var tdWidth = $(this).width();
        var widthFinal = parseFloat(tdWidth * 115);
        widthFinal = widthFinal.toFixed(2) / trWidth.toFixed(2);
        if ($(this).attr('colspan')) {
            for (var i = 1; i <= $(this).attr('colspan'); $i++) {
                colCount.push('*');
            }
        } else {
            colCount.push(parseFloat(widthFinal.toFixed(2)) + '%');
        }
    });
    doc.content[1].table.body[0] = doc.content[1].table.body[0].map(data => {
        return { ...data, alignment: 'left' }
    });
    doc.content[1].table.widths = colCount;
}

(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _numberHelper = new NumberHelper();
    //Global Variables
    let _currentUser;
    let _adminSettingPages = ['CalendarTimeDesignations', 'ConveyorInformation', 'EmployeeInformation', 'StatusAndRemarks'];
    let _adminSettingDashboard = 'AdminSettings';

    //Helpers
    const _cookieHelper = new CookieHelper();

    let attachEvents = () => {
        $('#close-sidebar-icon').on(CLICK_EVENT, onClickClosedSidebar);
        $('#log-out').on(CLICK_EVENT, onClickLogOutButton);
        $('#main-menu').on(CLICK_EVENT, onClickMainMenuButton);
        $('#hamburger-menu').on('click', toggleSidebar);
        $('.main-wrapper,#close-sidebar-icon').on('click', blurSidebar);

    };

    let onClickMainMenuButton = () => {
        window.location = '/Home/Index';
    };

    let onClickLogOutButton = () => {
        _cookieHelper.deleteAllContains('jsonWebToken');
        _cookieHelper.delete('refreshToken');
        window.location = '/Login/Index';
    };

    let retrievedCurrentUser = async () => {
        getCurrentLogIn();
    };

    let getWindowLocationPathname = () => {
        let pathname = window.location.pathname;

        if (_adminSettingPages.includes(pathname.split('/')[2]) && pathname.includes(_adminSettingDashboard)) {
            $('#dashboard-nav-item').addClass('d-none');
            $('#admin-setting-nav-item').removeClass('d-none');

        } else if (pathname.includes(_adminSettingDashboard)) {
            $('#dashboard-nav-item').removeClass('d-none');
            $('#admin-setting-nav-item').addClass('d-none');

        } else {
            $('#dashboard-nav-item').addClass('d-none');
            $('#admin-setting-nav-item').removeClass('d-none');

        }
    };

    let toggleSidebar = function () {
        if ($("#sidebar-container").hasClass("is-active")) {
            $("#sidebar-container").removeClass("is-active");
        } else {
            $("#sidebar-container").addClass("is-active");
        }
    };

    let blurSidebar = function () {
        $("#sidebar-container").removeClass("is-active");
    };

    let onClickClosedSidebar = () => {
        $('#wrapper').removeClass('toggled');
    };

    let onWindowLoaded = () => {
        attachEvents();
        inactivityTime();
        getWindowLocationPathname();
        retrievedCurrentUser();
        validateToken();
    };

    let getCurrentLogIn = async () => {
        let api = document.getElementById('base-api-url').value;

        let response = await fetch(`${api}/${`Authenticated/User/GetAuthenticatedAsync`}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${_cookieHelper.get('jsonWebToken')}`
            },
            body: JSON.stringify()
        });

        if (response.ok) {
            _currentUser = await response.json();

            $('#current-user').html(_currentUser.username);
            $('#user-role').html(_currentUser.roleName);
            $('#userId').val(_currentUser.userId);


            let type = 'Create';
            let tableName = 'StatementOfAccount'

            //let settingResp = await _apiHelper.get({
            //    url: `Lookups/Settings`
            //});

            //if (settingResp.ok) {
            //    let settings = await settingResp.json();
            //    if (settings) {
            //        let lastRun = settings.lastRunDate;
            //        let alreadyRun = moment(lastRun).isSame(new Date(), "day");

            //        if (!alreadyRun) {

            //            let resercationUpdates = await _apiHelper.put({
            //                url: `Authenticated/Reservation/Updates`,
            //                requestOrigin: `System`,
            //                requesterName: _currentUser.username
            //            });
            //            //let updateRegistrationResp = await _apiHelper.put({
            //            //    url: `Authenticated/Enrollment/SchoolYear`,
            //            //    requestOrigin: `System`,
            //            //    requesterName: _currentUser.username
            //            //});

            //            let updateAdmissionUserResp = await _apiHelper.put({
            //                url: `Authenticated/User/SchoolYear`,
            //                requestOrigin: `System`,
            //                requesterName: _currentUser.username
            //            });

            //            let registrationUpdates = await _apiHelper.put({
            //                url: `Authenticated/Registration/Updates`,
            //                requestOrigin: `System`,
            //                requesterName: _currentUser.username
            //            });

            //            let SoaSettingsUpdate = await _apiHelper.put({
            //                url: `Authenticated/SoaSetting/Updates`,
            //                requestOrigin: `System`,
            //                requesterName: _currentUser.username
            //            });

            //        }
            //    }
            //}

            let auditResp = await _apiHelper.get({
                url: `Authenticated/Audit/hasRecord?tableName=${tableName}&type=${type}&dateTime=${moment().format("MM-DD-YYYY")}`
            });

            if (auditResp.ok) {
                let hasRecordToday = await auditResp.json();
                //if (!hasRecordToday) {
                //    let enrollmentResp = await _apiHelper.get({
                //        url: `Authenticated/Enrollment/SchoolYear`
                //    });

                //    if (enrollmentResp.ok) {
                //        let enrollStudents = await enrollmentResp.json();
                //        $.each(enrollStudents, function (i, enrollStudent) {
                //            sendStatementOfAccount(enrollStudent);
                //        });

                //    }
                //}
            }

        }
    };

    let checkDaysBeforeDue = function (isFiveDaysBeforeDueDate, isThreeDaysBeforeDueDate) {
        let dayBeforeDueDate = 0;
        if (isFiveDaysBeforeDueDate)
            dayBeforeDueDate = 5;
        else if (isThreeDaysBeforeDueDate)
            dayBeforeDueDate = 3

        return dayBeforeDueDate;
    }
   let validateToken = () => {
        if (!_cookieHelper.get('jsonWebToken'))
            window.location = '/Login/Index';
    };

    var inactivityTime = function () {
        var time;
        resetTimer();
        document.onload = resetTimer;
        document.onmousemove = resetTimer;
        document.onmousedown = resetTimer; // touchscreen presses
        document.ontouchstart = resetTimer;
        document.onclick = resetTimer;     // touchpad clicks
        document.onkeydown = resetTimer;   // onkeypress is deprectaed
        document.addEventListener('scroll', resetTimer, true);

        function logout() {
            _cookieHelper.deleteAllContains('jsonWebToken');
            _cookieHelper.delete('refreshToken');
            Swal.fire({
                title: 'Session Timeout!',
                text: 'You will be redirected to login page.',
                icon: 'warning',
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: 'Ok'
            }).then(async (result) => {
                window.location = '/Login/Index';
            });
        }

        function resetTimer() {
            clearTimeout(time);
            time = setTimeout(logout, 900000) //15mins
        }
    };


    window.addEventListener(LOAD_EVENT, onWindowLoaded);
})(jQuery);