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

            let settingResp = await _apiHelper.get({
                url: `Lookups/Settings`
            });

            if (settingResp.ok) {
                let settings = await settingResp.json();
                if (settings) {
                    let lastRun = settings.lastRunDate;
                    let alreadyRun = moment(lastRun).isSame(new Date(), "day");

                    if (!alreadyRun) {

                        let resercationUpdates = await _apiHelper.put({
                            url: `Authenticated/Reservation/Updates`,
                            requestOrigin: `System`,
                            requesterName: _currentUser.username
                        });
                        let updateRegistrationResp = await _apiHelper.put({
                            url: `Authenticated/Enrollment/SchoolYear`,
                            requestOrigin: `System`,
                            requesterName: _currentUser.username
                        });

                        let updateAdmissionUserResp = await _apiHelper.put({
                            url: `Authenticated/User/SchoolYear`,
                            requestOrigin: `System`,
                            requesterName: _currentUser.username
                        });

                        let registrationUpdates = await _apiHelper.put({
                            url: `Authenticated/Registration/Updates`,
                            requestOrigin: `System`,
                            requesterName: _currentUser.username
                        });

                        let SoaSettingsUpdate = await _apiHelper.put({
                            url: `Authenticated/SoaSetting/Updates`,
                            requestOrigin: `System`,
                            requesterName: _currentUser.username
                        });

                    }
                }
            }

            let auditResp = await _apiHelper.get({
                url: `Authenticated/Audit/hasRecord?tableName=${tableName}&type=${type}&dateTime=${moment().format("MM-DD-YYYY")}`
            });

            if (auditResp.ok) {
                let hasRecordToday = await auditResp.json();
                if (!hasRecordToday) {
                    let enrollmentResp = await _apiHelper.get({
                        url: `Authenticated/Enrollment/SchoolYear`
                    });

                    if (enrollmentResp.ok) {
                        let enrollStudents = await enrollmentResp.json();
                        $.each(enrollStudents, function (i, enrollStudent) {
                            sendStatementOfAccount(enrollStudent);
                        });

                    }
                }
            }

        }
    };

    let getCurrentSchoolYear = function () {
        let thisYear = moment().year();
        let nextYear = moment().add(1, 'years').year();
        let schoolYear = thisYear + '-' + nextYear;
        return schoolYear
    }

    let evaluateFeeParticulars = function (isWithSSFee, isWithFSFee, isWithICard, isWithVisaExt, paymentScheme) {
        let particulars = '.'

        if (isWithSSFee && paymentScheme.withSSFee) {
            let message = `SSP Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(paymentScheme.ssFee))}`;
            if (particulars != '')
                particulars += ' and ';
            particulars += message;
        }
        if (isWithFSFee && paymentScheme.withFSFee) {
            let message = `FS Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(paymentScheme.fsFee))}`;
            if (particulars != '')
                particulars += ' and ';
            particulars += message;
        }
        if (isWithICard && paymentScheme.withICard) {
            let message = `ICard Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(paymentScheme.iCard))}`;
            if (particulars != '')
                particulars += ' and ';
            particulars += message;
        }
        if (isWithVisaExt && paymentScheme.withVisaExt) {
            let message = `Visa Ext Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(paymentScheme.visaExt))}`;
            if (particulars != '')
                particulars += ' and ';
            particulars += message;
        }

        return particulars;
    }

    let checkDaysBeforeDue = function (isFiveDaysBeforeDueDate, isThreeDaysBeforeDueDate) {
        let dayBeforeDueDate = 0;
        if (isFiveDaysBeforeDueDate)
            dayBeforeDueDate = 5;
        else if (isThreeDaysBeforeDueDate)
            dayBeforeDueDate = 3

        return dayBeforeDueDate;
    }
    let sendStatementOfAccount = async (enrollStudent) => {

        let paymentScheme = enrollStudent.paymentScheme;
        let reservationPayment = 0;
        if (enrollStudent.reservationId) {
            let reservationPaymentResp = await _apiHelper.get({
                url: `Authenticated/Payment/reservation/${enrollStudent.reservationId}`,
            });
            if (reservationPaymentResp.ok) {
                let payments = await reservationPaymentResp.json();
                reservationPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
            }
        }
        let isWithSSFee = enrollStudent.withSSFee;
        let isWithFSFee = enrollStudent.withFSFee;
        let isWithICard = enrollStudent.withICard;
        let isWithVisaExt = enrollStudent.withVisaExt;
        let isWithVoucher = enrollStudent.withVoucher;
        let balance = enrollStudent.balance;
        let sendEmail = false;
        let payments = enrollStudent.payments;
        let penalty = enrollStudent.penalty;
        let hasEnrollmentPenalty = (enrollStudent.penalty.length > 0);
        if (isWithSSFee)
            balance -= paymentScheme.ssFee;
        if (isWithFSFee)
            balance -= paymentScheme.fsFee;
        if (isWithICard)
            balance -= paymentScheme.iCard;
        if (isWithVisaExt)
            balance -= paymentScheme.visaExt;

        let paymentBasis = enrollStudent.paymentScheme.paymentBasisId;
        let soaSettings = enrollStudent.paymentScheme.paymentBasis.soaSettings;
        let unsentSetting = _.find(soaSettings, (s => s.sent == false));

        let settingDueDate = moment(unsentSetting?.date);

        let initialFee = enrollStudent.paymentScheme.initialFee - reservationPayment;
        let remainingFee = enrollStudent.paymentScheme.remainingFee;
        let totalFee = initialFee + remainingFee;

        let penaltyAmount = 0;
        if (hasEnrollmentPenalty) {
            penaltyAmount = penalty[0].amount;
        }

        let totalPayment = 0;
        let totalPenaltyPayment = 0;
        let totalSsFeePayment = 0;
        let totalFsPayment = 0;
        let totalIcardPayment = 0;
        let totalVisaPayment = 0;
        let totalEnrollmentPayment = 0;
        let totalTutionPayment = 0;
        if (payments.length > 0) {
            totalPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
            totalPenaltyPayment = payments.reduce((n, { penaltyAmount }) => n + penaltyAmount, 0);
            totalSsFeePayment = payments.reduce((n, { ssFeeAmount }) => n + ssFeeAmount, 0);
            totalFsPayment = payments.reduce((n, { fsFeeAmount }) => n + fsFeeAmount, 0);
            totalIcardPayment = payments.reduce((n, { iCardAmount }) => n + iCardAmount, 0);
            totalVisaPayment = payments.reduce((n, { visaExtAmount }) => n + visaExtAmount, 0);
            totalEnrollmentPayment = payments.reduce((n, { enrollmentAmount }) => n + enrollmentAmount, 0);
            totalTutionPayment = payments.reduce((n, { tutionAmount }) => n + tutionAmount, 0);
        }
        let discount = remainingFee * (enrollStudent.discount / 100);
        let numberOfSchoolYearMonth = enrollStudent.schoolYear.numberOfMonth;
        let voucherMessage = '';
        if (isWithVoucher && paymentScheme.voucherAmount > 0) {
            initialFee -= paymentScheme.voucherAmount;
            voucherMessage = ` with voucher of P${_numberHelper.formatCommaSeperator(paymentScheme.voucherAmount.toFixed(0))}`;
        }
        if (discount > 0) {
            remainingFee -= discount;
        }

        if (unsentSetting == undefined)
            return true;

        //let currentDay = moment().add(4, 'months').subtract(28, 'days');
        let currentDay = moment();
        let dayBeforeDueDate = 0;
        let particulars = '';
        let totalAmountDue = 0;

        if (hasEnrollmentPenalty) {
            isPenaltyPaid = (penaltyAmount - totalPenaltyPayment) <= 0;
            if (!isPenaltyPaid) {
                let amount = penaltyAmount - totalPenaltyPayment;
                remainingPenaltyAmount = amount;
                let enrollmentMessage = `Penalty: P${_numberHelper.formatCommaSeperator(amount)}`;
                if (particulars != '')
                    particulars += ' <br /> ';
                particulars += enrollmentMessage;
                totalPayment = totalPayment - totalPenaltyPayment;
                totalAmountDue += amount;
            } else {
                totalPayment = totalPayment - penaltyAmount;
            }
        }

        if (isWithSSFee && paymentScheme.withSSFee) {
            let message = '';
            isSSPPaid = (paymentScheme.ssFee - totalSsFeePayment) <= 0;
            if (!isSSPPaid) {
                if (paymentScheme.ssFee) {
                    let amount = paymentScheme.ssFee - totalSsFeePayment;
                    message = `SSP Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(amount))}`;
                    totalPayment -= amount;
                    if (particulars != '')
                        particulars += ' <br /> ';
                    particulars += message;
                    totalAmountDue += amount;
                }
            } else {
                totalPayment = totalPayment - paymentScheme.ssFee;
            }

        }
        if (isWithFSFee && paymentScheme.withFSFee) {
            let message = '';
            isFSPaid = (paymentScheme.fsFee - totalFsPayment) <= 0;
            if (!isFSPaid) {
                if (paymentScheme.fsFee) {
                    let amount = paymentScheme.fsFee - totalFsPayment;
                    message = `FS Fee: P ${_numberHelper.formatCommaSeperator(amount)}`;
                    totalPayment -= amount;
                    if (particulars != '')
                        particulars += ' <br /> ';
                    particulars += message;
                    totalAmountDue += amount;
                }
            } else {
                totalPayment = totalPayment - paymentScheme.fsFee;
            }

        }
        if (isWithICard && paymentScheme.withICard) {
            let message = '';
            isiCardPaid = (paymentScheme.iCard - totalIcardPayment) <= 0;
            if (!isiCardPaid) {
                if (paymentScheme.iCard) {
                    let amount = paymentScheme.iCard - totalIcardPayment;
                    message = `ICard Fee: P ${_numberHelper.formatCommaSeperator(amount)}`;
                    totalPayment -= amount;
                    if (particulars != '')
                        particulars += ' <br /> ';
                    particulars += message;
                    totalAmountDue += amount;
                }
            } else {
                totalPayment = totalPayment - paymentScheme.iCard;
            }

        }
        if (isWithVisaExt && paymentScheme.withVisaExt) {
            let message = '';
            isVisaExtPaid = (paymentScheme.visaExt - totalVisaPayment) <= 0;
            if (!isVisaExtPaid) {
                if (paymentScheme.visaExt) {
                    let amount = paymentScheme.visaExt - totalVisaPayment;
                    message = `VisaExt Fee: P ${_numberHelper.formatCommaSeperator(amount)}`;
                    totalPayment -= amount;
                    if (particulars != '')
                        particulars += ' <br /> ';
                    particulars += message;
                    totalAmountDue += amount;
                }
            } else {
                totalPayment = totalPayment - paymentScheme.visaExt;
            }

        }


        if (paymentBasis == 4) {
            let perMonthPay = remainingFee / 10;
            let diffOnDays = moment(currentDay).diff(settingDueDate, 'days', true);
            let currentSeries = unsentSetting.series;
            let isDueDate = Math.floor(diffOnDays) == 0;
            let isFiveDaysBeforeDueDate = Math.floor(diffOnDays + 5) == 0;
            let isThreeDaysBeforeDueDate = Math.floor(diffOnDays + 3) == 0;
            let discountPerMonth = discount / numberOfSchoolYearMonth;
            let isInitalFeePaid = totalEnrollmentPayment >= initialFee;
            dayBeforeDueDate = checkDaysBeforeDue(isFiveDaysBeforeDueDate, isThreeDaysBeforeDueDate);
            let months = ['First', 'Second', 'Third', 'Fourth', 'Fifth', 'Sixth', 'Seventh', 'Eighth', 'Ninth', 'Nenth', 'Eleventh', ' Twelfth'];
            if (!isInitalFeePaid) {
                let amount = initialFee - totalEnrollmentPayment;

                let enrollmentMessage = `Enrollment Fee: P ${_numberHelper.formatCommaSeperator(amount)}${voucherMessage}`;
                if (particulars != '')
                    particulars += ' <br /> ';
                particulars += enrollmentMessage;
                totalAmountDue += amount;
            }

            let monthPayment = totalTutionPayment;
            if (isDueDate) {
                sendEmail = true;
                for (let index = 1; index <= Math.round(currentSeries); ++index) {

                    let isLast = index == Math.round(currentSeries);
                    let month = months.shift();
                    if (month == undefined)
                        break;
                    monthPayment = monthPayment - perMonthPay;
                    let discountMessage = '';
                    if (discountPerMonth > 0) {
                        discountMessage = ` with discount of ${discountPerMonth.toFixed(0)}`;
                    }
                    if (monthPayment <= (perMonthPay * -1)) {
                        let message = `${month} month Fee: P ${_numberHelper.formatCommaSeperator(perMonthPay)}${discountMessage}`;
                        totalAmountDue += perMonthPay;
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    } else if (monthPayment < 0) {
                        let message = `${month} month Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(monthPayment))}${discountMessage}`;
                        totalAmountDue += Math.abs(monthPayment);
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    }
                }
            }
            console.log(particulars)

        } else if (paymentBasis == 3) {
            let discountPerQuarter = discount / 4;
            let diffOnDays = moment(currentDay).diff(settingDueDate, 'days', true);
            let currentSeries = unsentSetting.series;
            let isDueDate = Math.floor(diffOnDays) == 0;
            let isFiveDaysBeforeDueDate = Math.floor(diffOnDays + 5) == 0;
            let isThreeDaysBeforeDueDate = Math.floor(diffOnDays + 3) == 0;
            let perQuarterPay = remainingFee / 4;
            let isInitalFeePaid = totalEnrollmentPayment >= initialFee;
            let quarters = ['First', 'Second', 'Third', 'Fourth'];

            dayBeforeDueDate = checkDaysBeforeDue(isFiveDaysBeforeDueDate, isThreeDaysBeforeDueDate);
            if (!isInitalFeePaid) {
                let amount = initialFee - totalEnrollmentPayment;

                let enrollmentMessage = `Enrollment Fee: P ${_numberHelper.formatCommaSeperator(amount)}${voucherMessage}`;
                totalAmountDue += amount;
                if (particulars != '')
                    particulars += ' <br /> ';
                particulars += enrollmentMessage;

            }

            let quarterPayment = totalTutionPayment;
            if (isDueDate) {
                sendEmail = true;
                for (let index = 1; index <= Math.round(currentSeries); ++index) {
                    let quarter = quarters.shift();
                    let isLast = index == Math.round(currentSeries);
                    quarterPayment = quarterPayment - perQuarterPay;
                    if (quarter == undefined)
                        break;
                    let discountMessage = '';
                    if (discountPerQuarter > 0) {
                        discountMessage = ` with discount of ${_numberHelper.formatCommaSeperator(discountPerQuarter.toFixed(0))}`;
                    }
                    if (quarterPayment <= (perQuarterPay * -1)) {
                        let message = `${quarter} quarter Fee: P ${_numberHelper.formatCommaSeperator(perQuarterPay)}${discountMessage}`;
                        totalAmountDue += perQuarterPay;
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    } else if (quarterPayment < 0) {
                        let message = `${quarter} quarter Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(quarterPayment))}${discountMessage}`;
                        totalAmountDue += Math.abs(quarterPayment);
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    }

                }
            }

        } else if (paymentBasis == 2) {
            let perSemiAnnualPay = remainingFee / 2;
            let isInitalFeePaid = totalEnrollmentPayment >= initialFee;
            let diffOnDays = moment(currentDay).diff(settingDueDate, 'days', true);
            let currentSeries = unsentSetting.series;
            let isDueDate = Math.floor(diffOnDays) == 0;
            let isFiveDaysBeforeDueDate = Math.floor(diffOnDays + 5) == 0;
            let isThreeDaysBeforeDueDate = Math.floor(diffOnDays + 3) == 0;
            let quarters = ['First', 'Second', 'Third', 'Fourth'];
            dayBeforeDueDate = checkDaysBeforeDue(isFiveDaysBeforeDueDate, isThreeDaysBeforeDueDate);
            if (!isInitalFeePaid) {
                let amount = initialFee - totalEnrollmentPayment;
                totalAmountDue += amount;
                let enrollmentMessage = `Enrollment Fee P ${_numberHelper.formatCommaSeperator(amount)}${voucherMessage}`;

                if (particulars != '')
                    particulars += ' <br /> ';
                particulars += enrollmentMessage;

            }

            let remainingPayment = totalTutionPayment;
            if (isDueDate) {
                sendEmail = true;
                for (let index = 1; index <= Math.round(currentSeries); ++index) {
                    remainingPayment = remainingPayment - perSemiAnnualPay;
                    let isLast = index == Math.round(currentSeries);
                    let quarter = quarters.shift();
                    let discountMessage = '';
                    if (discount > 0) {
                        discountMessage = ` with discount of ${_numberHelper.formatCommaSeperator(discount.toFixed(0))}`;
                    }
                    if (remainingPayment <= (perSemiAnnualPay * -1)) {
                        let message = `${quarter} Sem tuition Fee: P ${_numberHelper.formatCommaSeperator(perSemiAnnualPay)}${discountMessage}`;
                        totalAmountDue += perSemiAnnualPay
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    } else if (remainingPayment < 0) {
                        let message = `${quarter} Sem tuition Fee: P ${_numberHelper.formatCommaSeperator(Math.abs(remainingPayment))}${discountMessage}`;
                        totalAmountDue += Math.abs(remainingPayment)
                        if (particulars != '')
                            particulars += ' <br /> '
                        particulars += message;
                    }
                }
            }

        } else {
            let remainingPay = remainingFee;
            let isInitalFeePaid = (initialFee - totalEnrollmentPayment) <= 0;

            let diffOnDays = moment(currentDay).diff(settingDueDate, 'days', true);

            let isFiveDaysBeforeStartDate = Math.floor(diffOnDays) == -5;
            let isThreeDaysBeforeStartDate = Math.floor(diffOnDays) == -3;
            let dueDate = Math.floor(diffOnDays) == 0;
            dayBeforeDueDate = checkDaysBeforeDue(isFiveDaysBeforeStartDate, isThreeDaysBeforeStartDate);
            if (!isInitalFeePaid) {
                let amount = initialFee - totalEnrollmentPayment;

                let enrollmentMessage = `Enrollment Fee: P ${_numberHelper.formatCommaSeperator(amount)}${voucherMessage}`;
                totalAmountDue += amount
                if (particulars != '')
                    particulars += ' <br /> ';
                particulars += enrollmentMessage;

            }

            if (dueDate) {
                sendEmail = true;
                let remainingPayment = totalTutionPayment;
                let remainingBalance = remainingFee - remainingPayment;
                if (remainingPayment > 0 && remainingBalance >= 1) {
                    let actualPayAmount = 0;
                    let amount = remainingFee;
                    if (remainingPayment < amount)
                        actualPayAmount = (remainingFee - remainingPayment);
                    else
                        actualPayAmount = amount;
                    let enrollmentMessage = `Tuition Fee P ${_numberHelper.formatCommaSeperator(actualPayAmount)}`;
                    totalAmountDue += actualPayAmount
                    if (particulars != '')
                        particulars += ' <br /> ';
                    particulars += enrollmentMessage;
                } else if (remainingPayment == 0) {

                    let amount = remainingFee;

                    if (amount > 0) {
                        let enrollmentMessage = `Tuition Fee P ${_numberHelper.formatCommaSeperator(amount)}`;
                        totalAmountDue += amount
                        if (particulars != '')
                            particulars += ' <br /> ';
                        particulars += enrollmentMessage;
                    }
                }

            }
        }

        let request = {
            createdBy: _currentUser.name,
            enrollmentId: enrollStudent.enrollmentId,
            statements: particulars,
            userId: _currentUser.userId,
            dateIssued: currentDay,
            dueDate: moment().add(dayBeforeDueDate, 'days'),
            totalDue: totalAmountDue
        };

        console.log(particulars, sendEmail, enrollStudent.fullName, totalAmountDue)

        if (particulars != '' && sendEmail) {
            let enrollmentResp = _apiHelper.post({
                url: `Authenticated/StatementOfAccount`,
                data: request,
                requestOrigin: `Statement`,
                requesterName: _currentUser.username
            });
        }
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