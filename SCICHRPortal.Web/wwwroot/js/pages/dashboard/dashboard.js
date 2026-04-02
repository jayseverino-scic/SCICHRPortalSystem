(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    const SYSTEM = 'enrolment';

    let _sections = [];
    let _gradeLevels = [];
    let _paymentSchemes = [];
    let _enrollStudents = [];
    let _parentUsers = [];
    let _schoolYears = [];
    let _reservations = [];
    let _dataList = null;
    let isCurrentSchoolYearConfigure = false;
    let _userId = 0;
    let isRegistrar = false;
    let penaltyObject = { lateEnrollee: false, hasPenalty: false };
    let _collectionTitlePage = "";
    let ENROLLMENT_AMOUNT = 0;
    let TUITION_AMOUNT = 0;
    let PENALTY_AMOUNT = 0;
    let SSFEE_AMOUNT = 0;
    let FSFEE_AMOUNT = 0;
    let ICARD_AMOUNT = 0;
    let VISA_AMOUNT = 0;

    let attachEvents = () => {
        $('.nav-link').on(CLICK_EVENT, reDrawTable);
        $('#student-type-form,#gender-form,#class-schedule-form,#grade-level-form,#payment-type-form,#payment-basis-form,#payment-method-form,#module-form,#section-form,#user-role-form,#requirement-form,#announcement-form,#school-year-form,#reservation-student-form').on('submit', onFormSubmit);
        $('.cancel-form').on(CLICK_EVENT, onFormCancel);
        $('#register-user-form').on('submit', onRegisterFormSubmit);
        $('#payment-form').on('submit', onPaymentFormSubmit);
        $('#payment-form #void-payment-btn').on('click', onPaymentToggleVoid);
        $('#payment-form #reprint-btn').on('click', onClickPrint);
      
        $('#register-student-form').on('submit', onRegisterStudent);
        $('#delete-register-student-btn').on('click', onClickDeleteRegisterBtn);
        $('#approve-register-student-btn').on('click', onClickApproveRegisterBtn);
        $('#reason-form').on('submit', onReasonFormSubmit);
        $('#view-student-form').on('submit', onSubmitRegisterStudentModal);
        $('#enrollment-form').on('submit', onSubmitEnrollmentModal);
        $('#payment-scheme-form').on('submit', onPaymentSchemeFormSubmit);
        $('#enrollment-form #StudentTypeId, #enrollment-form #PaymentBasisId').on('change', onEvaluatePaymentSchemeChange);
        $('#enrollment-form #SectionId').on('change', onEnrollmentSectionChange);
        $('#register-student-form #StudentTypeId').on('change', populateRegistrationRequirements);
        $('#register-student-form #UserId').on('change', populateEmail);
        $('#enrollment-form #GradeLevelId').on('change', onEnrollmentGradeLevelChange);
        $('#enrolled-report-grade').on('change', onEnrollmentReportGradeFilterChange);
        $('#reserved-report-grade').on('change', onReservationReportGradeFilterChange);

        $('#reservation-student-form #GradeLevelId').on('change', onReservationGradeLevelChange);
        $('#payment-scheme-form #GradeLevelId, #payment-scheme-form #StudentTypeId,#payment-scheme-form #PaymentBasisId,#payment-scheme-form #WithFSFee,#payment-scheme-form #WithSSFee,#payment-scheme-form #WithICard,#payment-scheme-form #WithVisaExt,#payment-scheme-form #WithVoucher').on('change', onPaymentSchemeGradeLevelChange);
        //$('#payment-scheme-form #StudentTypeId, #payment-scheme-form #GradeLevelId').on('change', onEvaluatePaymentSchemeColumns);
        $('#payment-scheme-form #WithFSFee,#payment-scheme-form #WithSSFee,#payment-scheme-form #WithICard,#payment-scheme-form #WithVisaExt,#payment-scheme-form #WithVoucher').on('change', onEvaluatePaymentSchemeColumns);
        //$('#enrollment-form #StudentTypeId, #enrollment-scheme-form #GradeLevelId').on('change', onEvaluateEnrollementColumns);
        $('#register-student-form #birthdayDate, #view-student-form #birthdayDate').on('change', onBirthdayChange);
        $('#register-student-form #student-email').on('blur', onBlurRegisterStudentEmail);
        $('#payment-form #StudentNumber').on('change', onPaymentStudentNumberChange);
        $('#payment-form #FullName').on('change', onPaymentStudentNameChange);
        $('#payment-form #PaymentType').on('change', onPaymentTypeChange);
        $('#payment-form #PaymentAmount').on('blur', onPaymentAmountBlur);
        $('#payment-form #PaymentAmount').on('keyup', onPaymentAmountChange);
        $('#statement-of-account-container #FullName-account').on('change', onReDrawStatementOfAccountGrid);
        $('#school-year-form #startYearDate').on('change', onSchoolStartYearFormChanges);
        $('#school-year-form #endYearDate').on('change', onSchoolEndYearFormChanges);
        $('#school-year-form #numberOfMonth').on('change', onSchoolYearMonthChange);
        $('#school-year-form #recompute').on('click', onSchoolStartYearFormChanges);
        $('.toggle-register-form').on('click', onToggleRegistrationForm);
        $('#send-requirement-email').on('click', sendRequiremets);
        $('.requirement-container').delegate('input[name="requirement"]', 'change', onRequirementChange);
        $("input[name=active-status]").on('change', onChangeRegistrationActiveStatus);
        $('#school-year-form #periodFrom').on('change', onEnrollmentStartDateFilterChange);
        $('#school-year-form #periodTo').on('change', onEnrollmentEndDateFilterChange);

        $('#edit-user-detail').on('click', onClickEditIcon);
        $('#user-detail-form').on('submit', onSaveUserDetailForm);
        $('#change-password-form').on('submit', onResetPasswordSubmitted);
        $('#school-year-form #toggle-active').on('click', onToggleSchoolYearActive);
        $("input[name=registration-status]").on('change', onChangeEnrollmentWithBalanceStatus);

        $('#reservation-student-form #StudentNumber').on('blur', onReservationStudentNumberBlur);
        $('#reservation-student-form input[name=LastName], #reservation-student-form input[name=FirstName]').on('blur', populateReservationFullNameBlur);
        $("input[name=payment-option]").on('change', onChangePaymentOption);
        $("#collectionType").on('change', onPaymentCollectionTypeChange);
        $("#filter-payment-collection").on('click', onPaymentCollectionFilter);
        $("#list-enrolled-student-report-filter").on('submit', onEnrolledStudentReportFilter);
        $("#list-reserved-student-report-filter").on('submit', onReservedStudentReportFilter);
        $("#list-payment-history-report-filter").on('submit', onPaymentHistoryReportFilter);

        $("#soa-download-pdf").on('click', onDownloadSOAPdf);
        $('#print-collection').on('click', onClickPrintCollection);
        $('#print-enrolled-student').on('click', onClickPrintEnrolledStudentReport);
        $('#print-reserved-student').on('click', onClickPrintReservedStudentReport);

    };

    let onClickPrintCollection = function () {
        $('#paymentCollectionBreakdown').printThis({
            // import page CSS
            printContainer: true,
            importCSS: true,
            loadCSS: 'css/printable.css',
            canvas: true,
            // import styles
            importStyle: true,
            copyTagClasses: true,
            copyTagStyles: true,
            removeInline: true,
            pageTitle: _collectionTitlePage,
            header: '<h3>' + _collectionTitlePage + '</h3>'
            //loadCSS: "/app/css/site.css"

        });
    }

    let onClickPrintEnrolledStudentReport = function () {
        let grade = $('#enrolled-report-grade option:selected').text();
        let section = $('#enrolled-report-section option:selected').text();
        let schoolYear = $('#enrolled-report-school-year option:selected').text();
        let title = `${grade} ${section} - S.Y. ${schoolYear}`;
        $('#enrolled-student-report-grid').printThis({
            // import page CSS
            printContainer: true,
            importCSS: true,
            loadCSS: 'css/printable.css',
            canvas: true,
            // import styles
            importStyle: true,
            copyTagClasses: true,
            copyTagStyles: true,
            removeInline: true,
            pageTitle: title,
            header: '<h3>' + title + '</h3>'
            //loadCSS: "/app/css/site.css"

        });
    }

    let onClickPrintReservedStudentReport = function () {
        let grade = $('#reserved-report-grade option:selected').text();
        let section = $('#reserved-report-section option:selected').text();
        let schoolYear = $('#reserved-report-school-year option:selected').text();
        let title = `${grade} ${section} - S.Y. ${schoolYear}`;
        $('#reserved-student-report-grid').printThis({
            // import page CSS
            printContainer: true,
            importCSS: true,
            loadCSS: 'css/printable.css',
            canvas: true,
            // import styles
            importStyle: true,
            copyTagClasses: true,
            copyTagStyles: true,
            removeInline: true,
            pageTitle: title,
            header: '<h3>' + title + '</h3>'
            //loadCSS: "/app/css/site.css"

        });
    }

    let onDownloadSOAPdf = async event => {
        $('#busy-indicator-container').removeClass('d-none');
        event.preventDefault();
        let _form = document.getElementById('statement-of-account-form');
        let data = _formHelper.toJsonString(_form);
        let id = data.StatementOfAccountId;

        response = await _apiHelper.post({
            url: 'Authenticated/StatementOfAccount/download/' + id,
        });

        if (response.ok) {
            let blob = await response.blob();
            //emulate download click
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = 'SOA' + Date.now() + '.pdf';
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            $('#busy-indicator-container').addClass('d-none');
        } else {
            $('#busy-indicator-container').addClass('d-none');
        }
    }


    let onEnrolledStudentReportFilter = async event => {
        event.preventDefault();
        let grade = $('#enrolled-report-grade').val();
        let section = $('#enrolled-report-section').val();
        let schoolYear = $('#enrolled-report-school-year').val();

        let gradeId = grade ? grade : 0;
        let sectionId = section ? section : 0;
        let schoolYearId = schoolYear ? schoolYear : 0;
        await getEnrolledStudentReportData(gradeId, sectionId, schoolYearId);

    }

    let onReservedStudentReportFilter = async event => {
        event.preventDefault();
        let grade = $('#reserved-report-grade').val();
        let section = $('#reserved-report-section').val();
        let schoolYear = $('#reserved-report-school-year').val();

        let gradeId = grade ? grade : 0;
        let sectionId = section ? section : 0;
        let schoolYearId = schoolYear ? schoolYear : 0;
        await getReservedStudentReportData(gradeId, sectionId, schoolYearId);

    }

    let onPaymentHistoryReportFilter = async event => {
        event.preventDefault();
        let grade = $('#payment-history-report-grade').val();
        let schoolYear = $('#rpayment-history-report-school-year').val();
        let id = $("#payment-history-id").val();
        let gradeId = grade ? grade : 0;
        let schoolYearId = schoolYear ? schoolYear : 0;
        await onGetAndPopulatePaymentHistoryModal(id, gradeId, schoolYearId);
    }


    let onPaymentCollectionFilter = function () {
        let schoolYear = getSchoolYear();
        let startOfSchool = moment(schoolYear.startDate);
        let endOfSchool = moment(schoolYear.endDate);
        let numberOfMonth = schoolYear.numberOfMonth;
        let startOfYear = moment(startOfSchool).startOf('year').format('yyyy-MM-DD');
        let endOfYear = moment(startOfSchool).endOf('year').format('yyyy-MM-DD');

        let type = $('#collectionType').val();
        let option  = $('#collectionTypeOption').val();
        let date = $('#daily-collection-date').val();
        let ANNUAL = 1,
            SEMI_ANNUAL = 2,
            QUARTERLY = 3,
            MONTHLY = 4,
            DAILY = 5;
        if (type == ANNUAL) {
            getPaymentCollectionByDate(startOfSchool.format('yyyy-MM-DD'), endOfSchool.format('yyyy-MM-DD'));
        } else if (type == SEMI_ANNUAL) {
            let monthPerQurter = 12 / 2;
            let end = moment(startOfYear).add(monthPerQurter * option, 'months').format('yyyy-MM-DD');
            let start = moment(end).subtract(monthPerQurter, 'months').format('yyyy-MM-DD');
            getPaymentCollectionByDate(start, end);

        } else if (type == QUARTERLY) {
            let monthPerQurter = 12 / 4;
            let end = moment(startOfYear).add(monthPerQurter * option, 'months').format('yyyy-MM-DD');
            let start = moment(end).subtract(monthPerQurter, 'months').format('yyyy-MM-DD');
            getPaymentCollectionByDate(start, end);

        } else if (type == MONTHLY) {
            let end = moment(startOfYear).add(option, 'months').format('yyyy-MM-DD');
            let start = moment(end).subtract(1, 'months').format('yyyy-MM-DD');
            getPaymentCollectionByDate(start, end);
        } else if (type == DAILY) {
            if (date) {
                let end = moment(date).format('yyyy-MM-DD');
                let start = moment(date).format('yyyy-MM-DD');
                getPaymentCollectionByDate(start, end);
            }
        }
    }

    let populatePaymentCollectionOption = function (data) {
        let dropdown = $('#collectionTypeOption');
        dropdown.empty();

        $.each(data, function (i, item) {
            dropdown.append($("<option />").val(item.id).text(item.text));
        });
    }

    let onPaymentCollectionTypeChange = function () {
        let schoolYear = getSchoolYear();
     
        let numberOfMonth = schoolYear.numberOfMonth;
        let months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
        let val = $(this).val();
        let ANNUAL = 1,
            SEMI_ANNUAL = 2,
            QUARTERLY = 3,
            MONTHLY = 4,
            DAILY = 5;
     
        $('.collection-option-container').removeClass('d-none');
        $('.collection-date-container').addClass('d-none');
        if (val == ANNUAL) {
            populatePaymentCollectionOption([{ id: 1, text: schoolYear.description }]);
        } else if (val == SEMI_ANNUAL) {
            populatePaymentCollectionOption([{ id: 1, text: 'First Quarter' }, { id: 2, text: 'Second Quarter' }]);
        } else if (val == QUARTERLY) {
            populatePaymentCollectionOption([{ id: 1, text: 'First Quarter' }, { id: 2, text: 'Second Quarter' },
                { id: 3, text: 'Third Quarter' }, { id: 4, text: 'Fourth Quarter' } ]);
        } else if (val == MONTHLY) {
            let data = [];
            for (var i = 0; i < 12; i++) {
                data.push({ id: i + 1, text: months[i] });
            }
            populatePaymentCollectionOption(data);
        } else if (val == DAILY){
            $('.collection-option-container').addClass('d-none');
            $('.collection-date-container').removeClass('d-none');
        }
      
    }

    let getNextGradeBaseOnLastEnrollment = function (enrollments) {
        let nextGradeLevel;
        let sortedRecord = enrollments.sort((a, b) => {
            return a.numberLevel - b.numberLevel;
        });
        lastEnrollment = sortedRecord.reverse()[0];

        let sortedGrade = _gradeLevels.sort((a, b) => {
            return a.numberLevel - b.numberLevel;
        });

        if (lastEnrollment) {
            let gradeLevelIndex = sortedGrade.findIndex(e => e.gradeLevelId == lastEnrollment.gradeLevelId);
            nextGradeLevel = sortedGrade[gradeLevelIndex + 1];
        }

        return nextGradeLevel;
    }

    let populateReservation = async (studentNumber, form) => {
        let data = await onGetRegistrationByStudentNumber(studentNumber);
        if (data) {
            data.contactNumber = data.contactNumber1;
            let birthDate = moment(data.birthdayDate).format('yyyy-MM-DD');
            $('#reservation-student-form #birthdayDate').val(birthDate);
            $('#reservation-student-form .reserved-auto').prop("disabled", true);
            let nextGradeLevel;
            if (data.enrollments) {
                nextGradeLevel = getNextGradeBaseOnLastEnrollment(data.enrollments);
            }
            if (nextGradeLevel)
                data.gradeLevelId = nextGradeLevel.gradeLevelId;
            _formHelper.populateForm(form, data);
        }
    }
    let onReservationStudentNumberBlur = async () => {

        let studentNumber = $('#reservation-student-form #StudentNumber').val();
        let form = $('#reservation-student-form');
        if (studentNumber) {
            await populateReservation(studentNumber, form);
        } else {
            form[0].reset();
            $('#reservation-student-form .reserved-auto').prop("disabled", false);
            populateSchoolYear('#reservation-student-form #SchoolYearId');
        }
    }

    let populateReservationFullNameBlur = function () {
        let value = $(this).val();
        onPopulateReservationByRegistrationId(value);
    }

    let onPopulateReservationByRegistrationId = async (value) => {
        let form = $('#reservation-student-form');
        var splitedFull = value.split('-');
        let registrationId = splitedFull[1];

        if (registrationId) {

            let data = await onGetRegistrationById(registrationId.trim());
            if (data) {
                data.contactNumber = data.contactNumber1;
                let birthDate = moment(data.birthdayDate).format('yyyy-MM-DD');
                $('#reservation-student-form #birthdayDate').val(birthDate);
                $('#reservation-student-form .reserved-auto').prop("disabled", true);
                let nextGradeLevel;
                if (data.enrollments) {
                    nextGradeLevel = getNextGradeBaseOnLastEnrollment(data.enrollments);
                }
                if (nextGradeLevel)
                    data.gradeLevelId = nextGradeLevel.gradeLevelId;

                _formHelper.populateForm(form, data);
            }
        } else {
            form[0].reset();
            $('#reservation-student-form .reserved-auto').prop("disabled", false);
            populateSchoolYear('#reservation-student-form #SchoolYearId');
        }
    }

    let onGetRegistrationByStudentNumber = async (studentNumber) => {
        let data;
        response = await _apiHelper.get({
            url: `Authenticated/Registration/StudentNumber/${studentNumber}`,
        });
        if (response.ok) {
            data = await response.json();
        }

        return data;
    }

    let onGetRegistrationById = async (id) => {
        let data;
        response = await _apiHelper.get({
            url: `Authenticated/Registration/${id}`,
        });
        if (response.ok) {
            data = await response.json();
        }

        return data;
    }
    let onToggleSchoolYearActive = function () {
        let id = $(this).data('id');
        let currentText = $(this).text();
        UpdateSchoolYearAsync(id);
        let newText = currentText == "Deactivate" ? "Activate" : "Deactivate"
        $(this).text(newText);

        if (newText == "Deactivate") {
            $(this).removeClass("btn-primary").addClass("btn-danger");
        } else {
            $(this).addClass("btn-primary").removeClass("btn-danger");
        }
        
    }

    let populatePenaltyOptions = function () {
        let dropdown = $('#PenaltyAmount');
        dropdown.empty();
        for (let i = 0; i < 31; i++) {
            dropdown.append($("<option />").val(i).text(i + "%"));
        }
    }

    let UpdateSchoolYearAsync = async (id) => {
        let response = await _apiHelper.put({
            url: 'Lookups/SchoolYear/Active/' + id,
        });

        if (response.ok) {
            $('#school-year-grid').DataTable().draw();
            renderDropDowns();
            $('.cancel-form').trigger('click');
        }
    }

    let onSearchParentEmail = async (email) => {
        let response = await _apiHelper.get({
            url: 'Authenticated/User/Email/' + email,
        });

        if (response.ok) {
            let data = await response.json();
            if (data) {
                $('#register-student-form #UserId').val(data.userId);
            }
        }
    }

    let onBlurRegisterStudentEmail = function(e) {
        let email = $(this).val();
        onSearchParentEmail(email);
    }

    let onSaveUserDetailForm = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('user-detail-form');
        if ($(_form).valid()) {
            let data = _formHelper.toJsonString(_form);
            let response;
            response = await _apiHelper.put({
                url: 'Authenticated/User/' + data.userId,
                data: data
            });

            if (response.ok) {
                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };
                $('#edit-user-detail').trigger('click');
                toastr.success('Updated!');
            } else {
                toastr.error('Error!');
            }
        }
    }

    let onClickEditIcon = function (e) {
        let form = $('#user-detail-form');
        let buttonContainer = $('#button-container');
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
            form.find('input').prop("disabled", true);
            buttonContainer.addClass('d-none');
        } else {
            $(this).addClass('active');
            form.find('input').removeAttr('disabled');
            buttonContainer.removeClass('d-none');
        }

    }

    let onResetPasswordSubmitted = async event => {
        event.preventDefault();
        $('.unmatched-password-validation').addClass('d-none');

        if ($('#new-password').val().toLowerCase() !== $('#confirm-password').val().toLowerCase()) {
            $('.unmatched-password-validation').removeClass('d-none');
        }
        else {
            let data = _formHelper.toJsonString(event.target);
            data.userId = _userId;
            let response;
            let _form = document.getElementById('change-password-form');

            if ($(_form).valid()) {
                response = await _apiHelper.put({
                    url: 'Authenticated/User/Reset',
                    data: data
                });

                if (response.ok) {
                    Swal.fire({
                        title: 'Updated',
                        text: "Do you want to logout, to verify your password?",
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Logout'
                    }).then(async (result) => {
                        _cookieHelper.delete('jsonWebToken');
                        _cookieHelper.delete('refreshToken');
                        window.location = '/Login/Index';

                    });
                } else {

                    toastr.options = {
                        "preventDuplicates": true,
                        "preventOpenDuplicates": true
                    };

                    if (response.status === 409) {
                        toastr.error('Wrong Password');
                    }

                    if (response.status === 400) {
                        toastr.error('Password must contain at least 8 characters , one letter and one number.');
                    }

                }
            }

        }
    };

    let onEnrollmentStartDateFilterChange = () => {
        let selectedStartDate = $('#school-year-form #periodFrom').val();
        let endDateVal = $('#school-year-form #periodTo').val();
        let endMinDate = moment(selectedStartDate).add(1, 'days').format('yyyy-MM-DD');

        if (selectedStartDate) {
            $('#school-year-form #periodFrom').html(_dateHelper.formatShortLocalDate(selectedStartDate));
            $('#school-year-form #periodTo').attr('min', endMinDate);
        } else {
            $('#school-year-form #periodFrom').html('___________');
            $('#school-year-form #periodTo').attr('min', '');
        }
        if (new Date(selectedStartDate) >= new Date(endDateVal))
            $('#school-year-form #periodTo').val(endMinDate);

        $('#school-year-form #periodTo').attr('min', endMinDate);
    };

    let onEnrollmentEndDateFilterChange = () => {
        let selectedEndDate = $('#school-year-form #periodTo').val();
        let startMaxDate = moment(selectedEndDate).subtract(1, 'days').format('yyyy-MM-DD');
        if (selectedEndDate) {
            $('#school-year-form #periodTo').html(_dateHelper.formatShortLocalDate(selectedEndDate));
            $('#school-year-form #periodFrom').attr('max', startMaxDate);
        } else {
            $('#school-year-form #periodTo').html('___________');
            $('#school-year-form #periodFrom').attr('max', '');

        }
    };

    let onChangeRegistrationActiveStatus = function () {
        let statusVal = $(this).val();
        $('.active-date').addClass('d-none')
        if (statusVal == 1) {
            $('.date-active').removeClass('d-none')
        } else {
            $('.date-inactive').removeClass('d-none')
        }
    }
    let onChangePaymentOption = function () {
        let optionVal = $(this).val();
        if (optionVal == 0) {
            $('.regular-payment-input').removeClass('d-none');
            renderDropdown({ name: 'payment-form #FullName', valueName: 'enrollmentId', data: _enrollStudents, text: 'fullName', placeHolder: '-', isSelect2: true });
            renderDropdown({ name: 'payment-form #StudentNumber', valueName: 'enrollmentId', data: _enrollStudents, text: 'studentNumber', placeHolder: '-', isSelect2: true });
        } else {
            $('.balance-details').addClass('d-none');
            $('.regular-payment-input').addClass('d-none');
            renderDropdown({ name: 'payment-form #FullName', valueName: 'reservationId', data: _reservations, text: 'fullName', placeHolder: '-', isSelect2: true });
            renderDropdown({ name: 'payment-form #StudentNumber', valueName: 'reservationId', data: _reservations, text: 'studentNumber', placeHolder: '-', isSelect2: true });
        }

        $('#payment-grid').DataTable().draw();
    }

    let onChangeEnrollmentWithBalanceStatus = function () {
        $('#student-with-balances-grid').DataTable().draw();
    }
    let onRequirementChange = function () {

        if ($('input[name="requirement"]:checked').length == $('input[name="requirement"]').length) {
            $("#send-requirement-email").addClass('d-none');
        } else {
            $("#send-requirement-email").removeClass('d-none');
        }
    }
    let populateEmail = function() {

        let userId = $(this).val();
        let parent = _parentUsers.find(p => p.userId == userId);
        let email = $('#register-student-form #student-email');
        if (email.val() == '') {
            email.val(parent.email);
        }
    }
    let populateRegistrationRequirements = async () => {

        let studentTypeId = $('#register-student-form #StudentTypeId').val();
        if (studentTypeId) {
            response = await _apiHelper.get({
                url: `Authenticated/Requirement/StudentType/${studentTypeId}`,
            });

            if (response.ok) {
                let data = await response.json();
                if (data) {
                    let source = document.getElementById("register-requirement-template").innerHTML;
                    let template = Handlebars.compile(source);
                    let container = $(".register-requirement-container");
                    let html = template({ requirements: data });
                    container.empty();
                    container.append(html);
                }

            }
        } else {
            $(".register-requirement-container").empty();
        }

    }
    let populateRequirements = async (studentTypeId, registrationId) => {

        response = await _apiHelper.get({
            url: `Authenticated/Requirement/StudentType/${studentTypeId}`,
        });

        if (response.ok) {
            let data = await response.json();
            if (data) {
                let source = document.getElementById("requirement-template").innerHTML;
                let template = Handlebars.compile(source);
                let container = $(".requirement-container");
                let html = template({ requirements: data });
                container.empty();
                container.append(html);
                let res = await _apiHelper.get({
                    url: `Authenticated/StudentRequirement/Registration/${registrationId}`,
                });
                $("#send-requirement-email").data("registrationId", registrationId);
                if (res.ok) {
                    let data = await res.json();
                    $.each(data, function (i, val) {
                        $('.requirement-container #' + val.requirementId).prop('checked', true);
                    });
                    if ($('input[name="requirement"]:checked').length == $('input[name="requirement"]').length) {
                        $("#send-requirement-email").addClass('d-none');
                    } else {
                        $("#send-requirement-email").removeClass('d-none');
                    }
                }


            }

        }
    }

    let sendRequiremets = async () => {
        var registrationId = $("#send-requirement-email").data("registrationId");
        $('#busy-indicator-container').removeClass('d-none');
        let response = await _apiHelper.post({
            url: `Authenticated/StudentRequirement/Email?registrationId=${registrationId}`,
        });

        if (response.ok) {
            $('#busy-indicator-container').addClass('d-none');
            toastr.options = {
                "preventDuplicates": true,
                "preventOpenDuplicates": true
            };
            toastr.success("Email Sent!");
        } else {
            $('#busy-indicator-container').addClass('d-none');
        }

    }

    let onToggleRegistrationForm = function () {
        let container = $(this).data('container');
        $('.toggle-register-form').removeClass('active');
        $(this).addClass('active');
        $('.registration-form-body').addClass('d-none');
        $('.' + container).removeClass('d-none');
    }

    let evaluateBirthDateRestriction = function () {
        var pastTwoYears = moment().subtract(2, 'years').format('yyyy-MM-DD');
        $('#register-student-form #birthdayDate').attr('max', pastTwoYears);
        $('#view-student-form #birthdayDate').attr('max', pastTwoYears);
        $('#reservation-student-form #birthdayDate').attr('max', pastTwoYears);
    }

    let evaluateAgeToCompute = function () {
        let schoolYear = getSchoolYear();
        if (schoolYear) {
            $('.month-age-compute').text('Age This ' + schoolYear.month);
        }
    }

    let onSchoolYearMonthChange = function () {
        let numberOfMonth = $('#school-year-form #numberOfMonth').val();
        let startYearDate = $('#school-year-form #startYearDate').val();
        if (numberOfMonth > 0) {
            $('#school-year-form #endYearDate').val(moment(startYearDate).add(numberOfMonth, 'months').format('yyyy-MM-DD'));
            let description = moment($('#school-year-form #startYearDate').val()).year() + '-' + moment($('#school-year-form #endYearDate').val()).year();
            $('#school-year-form #description').val(description);
        }
        onEnrollmentStartDateFilterChange();
        onEnrollmentEndDateFilterChange();
    }

    let onSchoolStartYearFormChanges = function () {
        let startYearDate = $('#school-year-form #startYearDate').val();
        let numberOfMonth = $('#school-year-form #numberOfMonth').val();

        $('#school-year-form #endYearDate').val(moment(startYearDate).add(numberOfMonth, 'months').format('yyyy-MM-DD'));
        $('#school-year-form #periodFrom').val(moment(startYearDate).subtract(2, 'months').format('yyyy-MM-DD'));
        $('#school-year-form #periodTo').val(moment(startYearDate).add(2, 'weeks').format('yyyy-MM-DD'));
        let description = moment($('#school-year-form #startYearDate').val()).year() + '-' + moment($('#school-year-form #endYearDate').val()).year();
        onEnrollmentStartDateFilterChange();
        onEnrollmentEndDateFilterChange();
        $('#school-year-form #description').val(description);
       
    }

    let onSchoolEndYearFormChanges = function () {
        let startYearDate = $('#school-year-form #startYearDate').val();
        let endYearDate = $('#school-year-form #endYearDate').val();
        let diffOnMonths = moment(endYearDate).diff(moment(startYearDate), 'months', true);
        let description = moment($('#school-year-form #startYearDate').val()).year() + '-' + moment($('#school-year-form #endYearDate').val()).year();
        $('#school-year-form #description').val(description);
        $('#school-year-form #numberOfMonth').val(Math.round(diffOnMonths));
    }


    let onPaymentAmountBlur = async () => {
        let value = parseFloat($('#payment-form #PaymentAmount').val());
        let paymentOption = $("input[name=payment-option]:checked").val();
        let words = numberToWords(value);
        $('#amount-in-words').val(words);
        let balance = parseFloat($('#payment-form #Balance').val().replace(/,/g, ''));
        if (value > balance) {
            $('#PaymentAmount-error').text("Please enter amount not greater than the total balance.")
        }
        let enrollmentId = $('#payment-form #FullName').val();
        if (enrollmentId) {
            if (paymentOption == 0) {
                let particulars = await buildParticulars(enrollmentId, Number(value));
                let paymentId = $('#payment-form #PaymentId').val();
                if (paymentId == 0)
                    $('#payment-form #Particulars').val(particulars);
            } else {
                $('.balance-details').addClass('d-none');
                let schoolYear = getSchoolYear();
                let paymentAmount = parseFloat(value);
                $('#payment-form #Particulars').val(`P${paymentAmount} Reservation Fee for S.Y. ${schoolYear.description}`);
            }
        }
      
       
        $('.payment-error-container').removeClass('d-none');

    }
    let onPaymentAmountChange = async () => {
        $('.payment-error-container').addClass('d-none');
        
    }
    let numberToWords = function (number) {
        var digit = ['Zero', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine'];
        var elevenSeries = ['Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
        var countingByTens = ['Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];
        var shortScale = ['', 'Thousand', 'Million', 'Billion', 'Trillion'];

        number = number.toString(); number = number.replace(/[\, ]/g, '');
        if (number != parseFloat(number)) return 'not a number';
        var x = number.indexOf('.');
        if (x == -1) x = number.length;
        if (x > 15) return 'too big'; var n = number.split('');
        var str = '';
        var sk = 0;
        for (var i = 0; i < x; i++) {
            if ((x - i) % 3 == 2) {
                if (n[i] == '1') {
                    str += elevenSeries[Number(n[i + 1])] + ' '; i++; sk = 1;
                } else if (n[i] != 0) {
                    str += countingByTens[n[i] - 2] + ' '; sk = 1;
                }
            } else if (n[i] != 0) {
                str += digit[n[i]] + ' ';
                if ((x - i) % 3 == 0)
                    str += 'Hundred '; sk = 1;
            }
            if ((x - i) % 3 == 1) {
                if (sk)
                    str += shortScale[(x - i - 1) / 3] + ' '; sk = 0;

            }
        }
        str = str.trim() + " pesos ";
        if (x != number.length) {
            var y = number.length; str += 'and ';
            for (var i = x + 1; i < y; i++) {
                if ((y - i) % 3 == 2) {
                    if (n[i] == '1') {
                        str += elevenSeries[Number(n[i + 1])] + ' '; i++; sk = 1;
                    } else if (n[i] != 0) {
                        str += countingByTens[n[i] - 2] + ' '; sk = 1;
                    }
                } else if (n[i] != 0) {
                    str += digit[n[i]] + ' ';
                    if ((y - i) % 3 == 0) str += 'Hundred '; sk = 1;
                } if ((y - i) % 3 == 1) { if (sk) str += shortScale[(y - i - 1) / 3] + ' '; sk = 0; }

            }
        }
        str = str.replace(/\number+/g, ' ');
        if (x != number.length)
            return str.trim() + " centavos.";
        return str.trim() + ".";
    }
    let onPaymentTypeChange = function () {

        let typeVal = $(this).val();
        if (typeVal == 1 || typeVal == 4) {
            $('#payment-form #Bank').val('');
            $('#payment-form #ChequeNumber').val('');
            $('.online-bank-container').addClass('d-none');
            $('.cheque-related-container').addClass('d-none');
        } else if (typeVal == 2) {
            $('.online-bank-container').addClass('d-none');
            $('.cheque-related-container').removeClass('d-none');
        } else if (typeVal == 3) {
            $('#payment-form #Bank').val('');
            $('#payment-form #ChequeNumber').val('');
            $('.cheque-related-container').addClass('d-none');
            $('.online-bank-container').removeClass('d-none');
        }
    }
  
    let onCancelPaymentForm = function () {
        $('#payment-form #StudentNumber').val("").trigger('change.select2');
        $('#payment-form #FullName').val("").trigger('change.select2');
        $('.cheque-related-container').addClass('d-none');
        $('.online-bank-container').addClass('d-none');
        $('#payment-form .form-control').prop("disabled", false);
        $('#payment-form .disable').prop("disabled", true);
        $('.total-fee-details').addClass('d-none')
    }
    let onCancelPaymentSchemeForm = function () {
        hideViewPaymentSchemeInputs();
        $('#payment-scheme-form .text-end').attr("type", 'number');
        $('#payment-scheme-form input').prop('disabled', false);
        $('.fee-column').prop('disabled', true);
    }

    let buildRemainingBalance = async (enrollmentId) => {
        let balance = 0;
        let TOTAL_BALANCE = 0;
        let TOTAL_PAYMENT = 0;
        let enrollmentResp = await _apiHelper.get({
            url: `Authenticated/Enrollment/${enrollmentId}`,
        });
        let paymentResp = await _apiHelper.get({
            url: `Authenticated/Payment/enrollment/${enrollmentId}`,
        });
        let particulars = '';
        let schoolYear;
        let enrollment;
        let reservationPayment = 0;
        if (enrollmentResp.ok) {
            enrollment = await enrollmentResp.json();
            balance = enrollment.balance;
            if (enrollment.reservationId) {
                let reservationPaymentResp = await _apiHelper.get({
                    url: `Authenticated/Payment/reservation/${enrollment.reservationId}`,
                });
                if (reservationPaymentResp.ok) {
                    let payments = await reservationPaymentResp.json();
                    reservationPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                }
            }
        }
        if (paymentResp.ok && enrollment) {
            let PaymentAmount = balance;
            TOTAL_BALANCE = balance;
            TOTAL_PAYMENT += balance;
            let payments = await paymentResp.json();
            let hasEnrollmentPenalty = (enrollment.penalty.length > 0);
            let penalty = enrollment.penalty;          

            schoolYear = enrollment.schoolYear.description;

            let paymentScheme = enrollment.paymentScheme;

            let initialFee = paymentScheme.initialFee - reservationPayment;
            let remainingFee = paymentScheme.remainingFee;
            let discount = paymentScheme.remainingFee * (enrollment.discount / 100);

            if (discount > 0) {
                remainingFee -= discount;
            }
            let withVoucher = enrollment.withVoucher && paymentScheme.voucherAmount;
            if (withVoucher) {
                remainingFee -= paymentScheme.voucherAmount;
            }

            let penaltyAmount = 0;
            if (hasEnrollmentPenalty) {
                penaltyAmount = penalty[0].amount;
            }

            let totalPayment = 0;
            let isInitalFeePaid = false;
            let isPenaltyPaid = false;
            let isSSPPaid = false;
            let isFSPaid = false;
            let isiCardPaid = false;
            let isVisaExtPaid = false;
            if (payments.length > 0) {
                totalPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                isPenaltyPaid = (penaltyAmount - totalPayment) <= 0;
                TOTAL_PAYMENT += totalPayment;
            }
            if (hasEnrollmentPenalty && penaltyAmount > 0 && PaymentAmount > 0) {
                isPenaltyPaid = (penaltyAmount - totalPayment) <= 0;
                if (!isPenaltyPaid) {
                    let amount = penaltyAmount - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;

                    let enrollmentMessage = `<label>Penalty: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(actualPayAmount)} </span> </br>`;

                    particulars += enrollmentMessage;
                    PaymentAmount = PaymentAmount - actualPayAmount;
                } else {
                    totalPayment = totalPayment - penaltyAmount;
                }
            }

            if (PaymentAmount > 0 && enrollment.withSSFee && paymentScheme.withSSFee) {
                let message = '';
                isSSPPaid = (paymentScheme.ssFee - totalPayment) <= 0;
                if (!isSSPPaid) {
                    if (paymentScheme.ssFee) {
                        let projectedAmount = PaymentAmount - paymentScheme.ssFee;
                      
                        if (projectedAmount >= 0) {
                            message = `<label>SSP Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(paymentScheme.ssFee.toFixed(0))} </span> </br>`;
                            PaymentAmount -= paymentScheme.ssFee.toFixed(0);
                        } else {
                            message = `<label>SSP Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} </span> </br>`;
                            PaymentAmount -= PaymentAmount.toFixed(0)
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.ssFee;
                }

            }
            if (PaymentAmount > 0 && enrollment.withFSFee && paymentScheme.withFSFee) {
                let message = '';
                isFSPaid = (paymentScheme.fsFee - totalPayment) <= 0;
                if (!isFSPaid) {
                    if (paymentScheme.fsFee) {
                        let projectedAmount = PaymentAmount - paymentScheme.fsFee;
                       
                        if (projectedAmount >= 0) {
                            message = `<label>FS Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(paymentScheme.fsFee.toFixed(0))} </span>  </br>`;
                            PaymentAmount -= paymentScheme.fsFee.toFixed(0);
                        } else {
                            message = `<label>FS Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} </span> </br> `;
                            PaymentAmount -= PaymentAmount.toFixed(0)
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.fsFee;
                }

            }
            if (PaymentAmount > 0 && enrollment.withICard && paymentScheme.withICard) {
                let message = '';
                isiCardPaid = (paymentScheme.iCard - totalPayment) <= 0;
                if (!isiCardPaid) {
                    if (paymentScheme.iCard) {
                        let projectedAmount = PaymentAmount - paymentScheme.iCard;
                        if (projectedAmount >= 0) {
                            message = `<label>iCard Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(paymentScheme.iCard.toFixed(0))} </span> </br> `;
                            PaymentAmount -= paymentScheme.iCard.toFixed(0);
                        } else {
                            message = `<label>iCard Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} </span> </br> `;
                            PaymentAmount -= PaymentAmount.toFixed(0)
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.iCard;
                }

            }
            if (PaymentAmount > 0 && enrollment.withVisaExt && paymentScheme.withVisaExt) {
                let message = '';
                isVisaExtPaid = (paymentScheme.visaExt - totalPayment) <= 0;
                if (!isVisaExtPaid) {
                    if (paymentScheme.visaExt) {
                        let projectedAmount = PaymentAmount - paymentScheme.visaExt;
                        if (projectedAmount >= 0) {
                            message = `<label>VisaExt Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(paymentScheme.visaExt.toFixed(0))} </span> </br> `;
                            PaymentAmount -= paymentScheme.visaExt.toFixed(0);
                        } else {
                            message = `<label>VisaExt Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} </span> </br> `;
                            PaymentAmount -= PaymentAmount.toFixed(0)
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.visaExt;
                }

            }
            let enrollmentActualAmount = 0;
            let totalFeeExcess = 0;
            if (PaymentAmount > 0 && !withVoucher) {
                isInitalFeePaid = (initialFee - totalPayment) <= 0;
                if (paymentScheme.paymentBasisId == 4 || paymentScheme.paymentBasisId == 3 || paymentScheme.paymentBasisId == 2) {
                    if (!isInitalFeePaid) {

                        let amount = initialFee - totalPayment;
                        let actualPayAmount = 0;
                        let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                        if (PaymentAmount < amount)
                            actualPayAmount = PaymentAmount;
                        else
                            actualPayAmount = amount;
                        let voucherMessage = '';
                        if (paymentType == 'full' && withVoucher) {
                            voucherMessage = ` with Voucher`;
                        }
                        let enrollmentMessage = `<label>Enrollment Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(actualPayAmount)}${voucherMessage} </span></br> `;

                        particulars += enrollmentMessage;
                        PaymentAmount = PaymentAmount - actualPayAmount;
                        enrollmentActualAmount += actualPayAmount;
                    } else {
                        totalFeeExcess = Math.abs(initialFee - totalPayment);
                    }
                }
            } else {
                totalFeeExcess = totalPayment;
            }

            if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 3) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;
                    let perQuarterPay = Number((currentFee / 4).toFixed(2));
                    let discountPerQuarter = 0;
                    let quarters = ['1st', '2nd', '3rd', '4th'];
                    let currentQuarter = Number(totalFeeExcess.toFixed(2)) / perQuarterPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perQuarterPay;
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    let voucherMessage2 = withVoucher ? 'Enrollment' : 'Tuition';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPerQuarter = voucherMessage != '' ? 0 : (discount / 4);
                    let totalCount = currentQuarter + paymentCount;
                    for (let index = 0; index < 4; ++index) {
                        let quarter = quarters.shift();
                        if (currentQuarter < 1 && paymentCount != 0) {

                            let balance = perQuarterPay * currentQuarter;
                            if (currentQuarter <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perQuarterPay * totalCount) : perQuarterPay;
                            let actualAmount = (amount) - balance;

                            let paymentType = (paymentCount + currentQuarter) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';

                            if (discountPerQuarter > 0) {
                                discountMessage = ` with ${enrollment.discount.toFixed()}% discount = P${_numberHelper.formatCommaSeperator(actualAmount)}`;
                            }
                            let message = `<label> ${quarter} Qtr ${voucherMessage2} Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator((actualAmount + discountPerQuarter).toFixed(2))}${voucherMessage}${discountMessage} </span></br>`;
                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                            if (quarter == '1st') {
                                enrollmentActualAmount += actualAmount;
                                let message = `<label> Upon Enrolment Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(enrollmentActualAmount)}</span><hr class="solid">`;
                                particulars = message + particulars;
                            }
                        } else {
                            totalFeeExcess = totalFeeExcess - perQuarterPay;
                        }

                        currentQuarter = currentQuarter - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }

            } else if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 4) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;

                    let numberOfSchoolYearMonth = enrollment.schoolYear.numberOfMonth;
                    let perMonthPay = Number((currentFee / numberOfSchoolYearMonth).toFixed(2));
                    let discountPerMonth = discount / numberOfSchoolYearMonth;

                    let months = ['first', 'second', 'third', 'fourth', 'fifth', 'sixth', 'seventh', 'eighth', 'ninth', 'tenth'];
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    let voucherMessage2 = withVoucher ? 'Enrollment' : 'Month';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPerMonth = voucherMessage != '' ? 0 : (discount / numberOfSchoolYearMonth);


                    let currentMonth = Number(totalFeeExcess.toFixed(2)) / perMonthPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perMonthPay;

                    let totalCount = currentMonth + paymentCount;
                    for (let index = 0; index < 10; ++index) {
                        let month = months.shift();
                        if (currentMonth < 1 && paymentCount != 0) {

                            let balance = perMonthPay * currentMonth;
                            if (currentMonth <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perMonthPay * totalCount) : perMonthPay;
                            let actualAmount = (amount) - balance;
                            let paymentType = (paymentCount + currentMonth) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';
                            if (discountPerMonth > 0) {
                                discountMessage = ` with ${enrollment.discount.toFixed()}% discount = P${_numberHelper.formatCommaSeperator(actualAmount)}`;
                            }
                            let message = `<label> ${month} ${voucherMessage2} Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator((actualAmount + discountPerMonth).toFixed(2))}${voucherMessage}${discountMessage} </span> </br>`;
                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                        } else {
                            totalFeeExcess = totalFeeExcess - perMonthPay;
                        }
                        currentMonth = currentMonth - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }

            } else if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 2) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;

                    let perSemesterPay = Number((currentFee / 2).toFixed(2));
                    let discountPeSemester = discount / 2;

                    let semesters = ['first', 'second'];
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    let voucherMessage2 = withVoucher ? 'Enrollment' : 'Semester';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPeSemester = voucherMessage != '' ? 0 : (discount / 2);

                    let currentMonth = Number(totalFeeExcess.toFixed(2)) / perSemesterPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perSemesterPay;

                    let totalCount = currentMonth + paymentCount;
                    for (let index = 0; index < 10; ++index) {
                        let semester = semesters.shift();
                        if (currentMonth < 1 && paymentCount != 0) {

                            let balance = perSemesterPay * currentMonth;
                            if (currentMonth <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perSemesterPay * totalCount) : perSemesterPay;
                            let actualAmount = (amount) - balance;
                            let paymentType = (paymentCount + currentMonth) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';
                            if (discountPeSemester > 0) {
                                discountMessage = ` with ${enrollment.discount.toFixed()}% discount = P${_numberHelper.formatCommaSeperator(actualAmount)}`;
                            }
                            let message = `<label> ${semester} ${voucherMessage2} Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator((actualAmount + discountPeSemester).toFixed(2))}${voucherMessage}${discountMessage} </span> </br>`;
                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                        } else {
                            totalFeeExcess = totalFeeExcess - perSemesterPay;
                        }
                        currentMonth = currentMonth - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }
            } else {
                let isInitalFeePaid = false;
                let discountPerAnnual = discount;
                isInitalFeePaid = (initialFee - totalPayment) <= 0;
              
                if (!isInitalFeePaid) {
                    let amount = initialFee - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;

                    let voucherMessage = '';
                    if (paymentType == 'full' && paymentScheme.voucherAmount > 0) {
                        voucherMessage = ` with Voucher`;
                    }
                    let enrollmentMessage = `<label>Enrollment Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(actualPayAmount)}${voucherMessage}</span> </br>`;
                    particulars += enrollmentMessage;
                    PaymentAmount = PaymentAmount - actualPayAmount;
                    if (PaymentAmount > 0) {
                        let amount = remainingFee;
                        let actualPayAmount = 0;
                        let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                        if (PaymentAmount < amount)
                            actualPayAmount = PaymentAmount;
                        else
                            actualPayAmount = amount;

                        let discountMessage = '';
                        if (discountPerAnnual > 0) {
                            discountMessage = ` with ${enrollment.discount.toFixed()}% discount = P${_numberHelper.formatCommaSeperator(actualPayAmount)}`;
                        }
                        let enrollmentMessage = `<label>Tuition Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator((actualPayAmount + discountPerAnnual).toFixed(2))}${discountMessage}</span></br> `;
                        particulars += enrollmentMessage;
                    }
                } else {
                    let amount = (initialFee + remainingFee) - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= remainingFee ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;
                    let discountMessage = '';
                    if (discountPerAnnual > 0) {
                        discountMessage = ` with ${enrollment.discount.toFixed()}% discount = P${_numberHelper.formatCommaSeperator(actualPayAmount)}`;
                    }
                    let enrollmentMessage = '';
                    if (actualPayAmount > 0) {
                        enrollmentMessage = `<label>Tuition Fee: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator((actualPayAmount + discountPerAnnual).toFixed(2))}${discountMessage}</span> </br>`;
                    }
                    particulars += enrollmentMessage;
                }
            }

        }

        if (particulars != '') {
            $('.balance-details').removeClass('d-none');
            let container = $('.breakdown-balance');
            container.empty();
            let totalBalMessage = `<hr class="solid"><label>Total Balance: </label> <span class="bal-amount">P${_numberHelper.formatCommaSeperator(TOTAL_BALANCE)}</span> </br>`;
            particulars += totalBalMessage;
            container.append(particulars);
        }
        $('.total-fee-details').removeClass('d-none');
        $('#payment-total-fee-label').text('P'+_numberHelper.formatCommaSeperator(+TOTAL_PAYMENT))
       
    }


    let buildParticulars = async (enrollmentId, PaymentAmount) => {
        ENROLLMENT_AMOUNT = 0;
        TUITION_AMOUNT = 0;
        PENALTY_AMOUNT = 0;
        SSFEE_AMOUNT = 0;
        FSFEE_AMOUNT = 0;
        ICARD_AMOUNT = 0;
        VISA_AMOUNT = 0;
        let button = $(event.target).find(':submit').text().toLowerCase();
        let enrollmentResp = await _apiHelper.get({
            url: `Authenticated/Enrollment/${enrollmentId}`,
        });
        let paymentResp = await _apiHelper.get({
            url: `Authenticated/Payment/enrollment/${enrollmentId}`,
        });
        let particulars = '';
        let schoolYear;
        let enrollment;
        let reservationPayment = 0;
        let balance = 0;
        if (enrollmentResp.ok) {
            enrollment = await enrollmentResp.json();
            balance = enrollment.balance;
            $("#payment-form #Balance").val(_numberHelper.formatCommaSeperator(balance.toFixed(2)));
            $("#payment-form #GradeLabel").val(enrollment.gradeLevel.description);
            $("#payment-form #SectionLabel").val(enrollment.section.description);
            if (enrollment.reservationId) {
                let reservationPaymentResp = await _apiHelper.get({
                    url: `Authenticated/Payment/reservation/${enrollment.reservationId}`,
                });
                if (reservationPaymentResp.ok) {
                    let payments = await reservationPaymentResp.json();
                    reservationPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                }
            }
            //$("#payment-form #PaymentAmount").attr('data-val-range-max', balance);

            if (balance <= 0) {
                //$('#payment-form').find(':submit').prop("disabled", true);
                Swal.fire(
                    'Fully Paid!',
                    'The student is already paid',
                    'error'
                );
            } else {
                $('#payment-form').find(':submit').prop("disabled", false);
            }

        }
        if (paymentResp.ok && enrollment) {
            let payments = await paymentResp.json();
            let hasEnrollmentPenalty = (enrollment.penalty.length > 0);
            let penalty = enrollment.penalty;
            schoolYear = enrollment.schoolYear.description;
       
            if (button != 'add') {
                let paymentId = $('#payment-form #PaymentId').val();
                let editPaymentAmount = 0;
                let editPayment = payments.find(p => p.paymentId == paymentId);
                payments = payments.filter(p => p.paymentId != paymentId);
                if (editPayment) {
                    editPaymentAmount = editPayment.paymentAmount;
                }
                $("#payment-form #PaymentAmount").attr('max', (balance.toFixed(2) + editPaymentAmount));
            } else {
                $("#payment-form #PaymentAmount").attr('max', (balance.toFixed(2)));
            }

            let paymentScheme = enrollment.paymentScheme;

            let initialFee = paymentScheme.initialFee - reservationPayment;
            let remainingFee = paymentScheme.remainingFee;
            let totalFee = paymentScheme.initialFee + paymentScheme.remainingFee;
            let discount = Number((paymentScheme.remainingFee * (enrollment.discount / 100)).toFixed(2))
        
            if (discount > 0) {
                remainingFee -= discount;
            }
            let withVoucher = enrollment.withVoucher && paymentScheme.voucherAmount;
            if (withVoucher) {
                remainingFee -= paymentScheme.voucherAmount;
            }

            let penaltyAmount = 0;
            if (hasEnrollmentPenalty) {
                penaltyAmount = penalty[0].amount;
            }
         
            let totalPayment = 0;
            let isInitalFeePaid = false;
            let isPenaltyPaid = false;
            let isSSPPaid = false;
            let isFSPaid = false;
            let isiCardPaid = false;
            let isVisaExtPaid = false;
            if (payments.length > 0) {
                totalPayment = payments.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                isPenaltyPaid = (penaltyAmount - totalPayment) <= 0;
            }
            if (hasEnrollmentPenalty && penaltyAmount > 0 && PaymentAmount > 0) {
                isPenaltyPaid = (penaltyAmount - totalPayment) <= 0;
                if (!isPenaltyPaid) {
                    let amount = penaltyAmount - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;

                    let enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for penalty. `;

                    particulars += enrollmentMessage;
                    PaymentAmount = PaymentAmount - actualPayAmount;
                    PENALTY_AMOUNT = actualPayAmount;
                } else {
                    totalPayment = totalPayment - penaltyAmount;
                }
            }

            if (PaymentAmount > 0 && enrollment.withSSFee && paymentScheme.withSSFee) {
                let message = '';
                isSSPPaid = (paymentScheme.ssFee - totalPayment) <= 0;
                if (!isSSPPaid) {
                    if (paymentScheme.ssFee) {
                        let projectedAmount = PaymentAmount - paymentScheme.ssFee;
                        if (projectedAmount >= 0) {
                            message = `P${_numberHelper.formatCommaSeperator(paymentScheme.ssFee.toFixed(0))} as full payment for SSP Fee. `;
                            PaymentAmount -= paymentScheme.ssFee.toFixed(0);
                            SSFEE_AMOUNT = paymentScheme.ssFee;
                        } else {
                            message = `P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} as partial payment for SSP Fee. `;
                            SSFEE_AMOUNT = PaymentAmount;
                            PaymentAmount -= PaymentAmount.toFixed(0);
                        }
                        particulars += message;
                       
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.ssFee;
                }

            }
            if (PaymentAmount > 0 && enrollment.withFSFee && paymentScheme.withFSFee) {
                let message = '';
                isFSPaid = (paymentScheme.fsFee - totalPayment) <= 0;
                if (!isFSPaid) {
                    if (paymentScheme.fsFee) {
                        let projectedAmount = PaymentAmount - paymentScheme.fsFee;
                        if (projectedAmount >= 0) {
                            message = `P${_numberHelper.formatCommaSeperator(paymentScheme.fsFee.toFixed(0))} as full payment for FS Fee. `;
                            PaymentAmount -= paymentScheme.fsFee.toFixed(0);
                            FSFEE_AMOUNT = paymentScheme.fsFee;
                        } else {
                            message = `P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} as partial payment for FS Fee. `;
                            FSFEE_AMOUNT = PaymentAmount;
                            PaymentAmount -= PaymentAmount.toFixed(0);
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.fsFee;
                }

            }
            if (PaymentAmount > 0 && enrollment.withICard && paymentScheme.withICard) {
                let message = '';
                isiCardPaid = (paymentScheme.iCard - totalPayment) <= 0;
                if (!isiCardPaid) {
                    if (paymentScheme.iCard) {
                        let projectedAmount = PaymentAmount - paymentScheme.iCard;
                        if (projectedAmount >= 0) {
                            message = `P${_numberHelper.formatCommaSeperator(paymentScheme.iCard.toFixed(0))} as full payment for ICard Fee. `;
                            PaymentAmount -= paymentScheme.iCard.toFixed(0);
                            ICARD_AMOUNT = paymentScheme.iCard;
                        } else {
                            message = `P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} as partial payment for ICard Fee. `;
                            ICARD_AMOUNT = PaymentAmount;
                            PaymentAmount -= PaymentAmount.toFixed(0);
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.iCard;
                }

            }
            if (PaymentAmount > 0 && enrollment.withVisaExt && paymentScheme.withVisaExt) {
                let message = '';
                isVisaExtPaid = (paymentScheme.visaExt - totalPayment) <= 0;
                if (!isVisaExtPaid) {
                    if (paymentScheme.visaExt) {
                        let projectedAmount = PaymentAmount - paymentScheme.visaExt;
                        if (projectedAmount >= 0) {
                            message = `P${_numberHelper.formatCommaSeperator(paymentScheme.visaExt.toFixed(0))} as full payment for Visa Ext Fee. `;
                            PaymentAmount -= paymentScheme.visaExt.toFixed(0);
                            VISA_AMOUNT = paymentScheme.visaExt;
                        } else {
                            message = `P${_numberHelper.formatCommaSeperator(PaymentAmount.toFixed(0))} as partial payment for Visa Ext Fee. `;
                            VISA_AMOUNT = PaymentAmount;
                            PaymentAmount -= PaymentAmount.toFixed(0);
                        }
                        particulars += message;
                    }
                } else {
                    totalPayment = totalPayment - paymentScheme.visaExt;
                }

            }

            let totalFeeExcess = 0;
            if (PaymentAmount > 0 && !withVoucher) {
                isInitalFeePaid = (initialFee - totalPayment) <= 0;
                if (paymentScheme.paymentBasisId == 4 || paymentScheme.paymentBasisId == 3 || paymentScheme.paymentBasisId == 2) {
                    if (!isInitalFeePaid) {

                        let amount = initialFee - totalPayment;
                        let actualPayAmount = 0;
                        let paymentType = PaymentAmount >= initialFee ? 'full' : 'partial';
                        if (PaymentAmount < amount)
                            actualPayAmount = PaymentAmount;
                        else
                            actualPayAmount = amount;
                        let voucherMessage = '';
                        let reservationPaymementMessage = '';
                        if (paymentType == 'full' && paymentScheme.voucherAmount > 0) {
                            voucherMessage = ` with Voucher.`;
                        }
                        if (totalPayment == 0 && reservationPayment > 0) {
                            reservationPaymementMessage = `with P${_numberHelper.formatCommaSeperator(reservationPayment)} as reservation fee.`
                        }
                        let enrollmentMessage = '';

                        if (totalPayment == 0 && enrollment.discount == 100) {
                            let discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discount)}.`;
                            enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for enrollment fee${discountMessage} ${reservationPaymementMessage}`;

                        } else {
                            enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for enrollment fee${voucherMessage} ${reservationPaymementMessage}`;
                        }

                      
                        particulars += enrollmentMessage;
                        PaymentAmount = PaymentAmount - actualPayAmount;
                        ENROLLMENT_AMOUNT = actualPayAmount;
                    } else {
                        totalFeeExcess = Math.abs(initialFee - totalPayment);
                    }
                }
            } else {
                totalFeeExcess = totalPayment;
            }
            TUITION_AMOUNT = PaymentAmount;
            if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 3) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;
                    let perQuarterPay = Number((currentFee / 4).toFixed(2));
                    let discountPerQuarter = 0;
                    let quarters = ['first', 'second', 'third', 'fourth'];
                    let currentQuarter = Number(totalFeeExcess.toFixed(2)) / perQuarterPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perQuarterPay;
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPerQuarter = voucherMessage != '' ? 0 : (discount / 4);
                    $("#payment-form #Discount").val(_numberHelper.formatCommaSeperator(discountPerQuarter.toFixed(2)));

                    let totalCount = currentQuarter + paymentCount;
                    for (let index = 0; index < 4; ++index) {
                        let quarter = quarters.shift();
                        if (currentQuarter < 1 && paymentCount != 0) {

                            let balance = perQuarterPay * currentQuarter;
                            if (currentQuarter <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perQuarterPay * totalCount) : perQuarterPay;
                            let actualAmount = (amount) - balance;

                            let paymentType = (paymentCount + currentQuarter) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';

                            if (currentQuarter <= 0 && discountPerQuarter > 0) {
                                discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerQuarter.toFixed(2))}`;
                            }
                            let message = `P${_numberHelper.formatCommaSeperator(actualAmount.toFixed(2))} as ${paymentType} payment for ${quarter} quarter${voucherMessage}${discountMessage}. `;

                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                        } else {
                            totalFeeExcess = totalFeeExcess - perQuarterPay;
                        }

                        currentQuarter = currentQuarter - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }

            } else if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 4) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;
                    let numberOfSchoolYearMonth = enrollment.schoolYear.numberOfMonth;
                    let perMonthPay = Number((currentFee / numberOfSchoolYearMonth).toFixed(2));
                    let discountPerMonth = discount / numberOfSchoolYearMonth;
                    let months = ['first', 'second', 'third', 'fourth', 'fifth', 'sixth', 'seventh', 'eighth', 'ninth', 'tenth'];
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPerMonth = voucherMessage != '' ? 0 : (discount / numberOfSchoolYearMonth);
                    $("#payment-form #Discount").val(_numberHelper.formatCommaSeperator(discountPerMonth.toFixed(2)));

                    let currentMonth = Number(totalFeeExcess.toFixed(2)) / perMonthPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perMonthPay;

                    let totalCount = currentMonth + paymentCount;
                    for (let index = 0; index < 10; ++index) {
                        let month = months.shift();
                        if (currentMonth < 1 && paymentCount != 0) {

                            let balance = perMonthPay * currentMonth;
                            if (currentMonth <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perMonthPay * totalCount) : perMonthPay;
                            let actualAmount = (amount) - balance;
                            let paymentType = (paymentCount + currentMonth) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';
                            if (currentMonth <= 0 && discountPerMonth > 0) {
                                discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerMonth.toFixed(2))}`;
                            }
                            let message = `P${_numberHelper.formatCommaSeperator(actualAmount)} as ${paymentType} payment for ${month} month${voucherMessage}${discountMessage}. `;
                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                        } else {
                            totalFeeExcess = totalFeeExcess - perMonthPay;
                        }

                        currentMonth = currentMonth - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }

            } else if (PaymentAmount > 0 && paymentScheme.paymentBasisId == 2) {
                let count = withVoucher ? 2 : 1;
                for (let i = 0; i < count; ++i) {
                    let currentFee = i == 0 ? initialFee : remainingFee;
                    if (count == 1)
                        currentFee = remainingFee;
                    let perSemesterPay = Number((currentFee / 2).toFixed(2));
                    let discountPerSemester = discount / 2;
                    let months = ['first', 'second'];
                    let voucherMessage = i == 0 ? ' on enrollment fee' : ' on tuition fee';
                    if (!withVoucher) {
                        voucherMessage = '';
                    }
                    discountPerSemester = voucherMessage != '' ? 0 : (discount / 2);
                    $("#payment-form #Discount").val(_numberHelper.formatCommaSeperator(discountPerSemester.toFixed(2)));

                    let currentSemester = Number(totalFeeExcess.toFixed(2)) / perSemesterPay;
                    let paymentCount = Number(PaymentAmount.toFixed(2)) / perSemesterPay;

                    let totalCount = currentSemester + paymentCount;
                    for (let index = 0; index < 10; ++index) {
                        let month = months.shift();
                        if (currentSemester < 1 && paymentCount != 0) {

                            let balance = perSemesterPay * currentSemester;
                            if (currentSemester <= 0)
                                balance = 0;
                            let amount = (totalCount < 1 && totalCount != 0) ? (perSemesterPay * totalCount) : perSemesterPay;
                            let actualAmount = (amount) - balance;
                            let paymentType = (paymentCount + currentSemester) >= 1 ? 'full' : 'partial';
                            let discountMessage = '';
                            if (currentSemester <= 0 && discountPerSemester > 0) {
                                discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerSemester.toFixed(2))}`;
                            }
                            let message = `P${_numberHelper.formatCommaSeperator(actualAmount)} as ${paymentType} payment for ${month} semester${voucherMessage}${discountMessage}. `;
                            PaymentAmount -= actualAmount.toFixed(0);
                            particulars += message;
                            totalFeeExcess = totalFeeExcess - balance;
                        } else {
                            totalFeeExcess = totalFeeExcess - perSemesterPay;
                        }

                        currentSemester = currentSemester - 1;
                        totalCount = totalCount - 1;
                        if (totalCount <= 0)
                            break;
                    }
                }
            } else if (PaymentAmount > 0) {
                let isInitalFeePaid = false;
                let discountPerAnnual = discount;
                $("#payment-form #Discount").val(_numberHelper.formatCommaSeperator(discountPerAnnual.toFixed(2)));
                isInitalFeePaid = (initialFee - totalPayment) <= 0;
               
                if (!isInitalFeePaid) {
                    let amount = initialFee - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= initialFee ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;

                    let voucherMessage = '';
                    if (paymentType == 'full' && withVoucher) {
                        voucherMessage = ` with Voucher`;
                    }

                    let reservationPaymementMessage = '';
                    if (totalPayment == 0 && reservationPayment > 0) {
                        reservationPaymementMessage = `P${_numberHelper.formatCommaSeperator(reservationPayment)} as reservation fee.`
                    }
                    let enrollmentMessage = '';
                    if (totalPayment == 0 && enrollment.discount == 100) {
                        let discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerAnnual.toFixed(2))}`;
                        enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for enrollment fee${discountMessage}. ${reservationPaymementMessage}`;
                    } else {
                        enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for enrollment fee${voucherMessage}. ${reservationPaymementMessage}`;
                    }
                    particulars += enrollmentMessage;
                    ENROLLMENT_AMOUNT = actualPayAmount;
                    PaymentAmount = PaymentAmount - actualPayAmount;
                    if (PaymentAmount > 0) {
                        let amount = remainingFee;
                        let actualPayAmount = 0;
                        let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                        if (PaymentAmount < amount)
                            actualPayAmount = PaymentAmount;
                        else
                            actualPayAmount = amount;

                        let discountMessage = '';
                        if (totalPayment == 0 && discountPerAnnual > 0) {
                            discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerAnnual.toFixed(2))}`;
                        }
                        let enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for tuition fee${discountMessage}. `;
                        particulars += enrollmentMessage;
                        TUITION_AMOUNT = actualPayAmount;
                    }
                } else {
                    let amount = (initialFee + remainingFee) - totalPayment;
                    let actualPayAmount = 0;
                    let paymentType = PaymentAmount >= amount ? 'full' : 'partial';
                    if (PaymentAmount < amount)
                        actualPayAmount = PaymentAmount;
                    else
                        actualPayAmount = amount;
                    let discountMessage = '';
                    if (totalPayment == 0 && discountPerAnnual > 0) {
                        discountMessage = ` with discount of P${_numberHelper.formatCommaSeperator(discountPerAnnual.toFixed(2))}`;
                    }
                    let enrollmentMessage = `P${_numberHelper.formatCommaSeperator(actualPayAmount)} as ${paymentType} payment for tuition fee${discountMessage}. `;
                    particulars += enrollmentMessage;
                    TUITION_AMOUNT = actualPayAmount;
                }
            } else {
                $("#payment-form #Discount").val(0);
            }

        }
       
        return `${particulars}for S.Y. ${schoolYear}`;
    }
    let onPaymentStudentNameChange = async () => {
      
        let enrollmentId = $('#payment-form #FullName').val();
        let paymentOption = $("input[name=payment-option]:checked").val();
        $('#payment-form #StudentNumber').val(enrollmentId).trigger('change.select2');
        if (enrollmentId) {

            if (paymentOption == 0) {
                let particulars = await buildParticulars(enrollmentId, parseFloat($('#payment-form #PaymentAmount').val()));
                let paymentId = $('#payment-form #PaymentId').val();
                if(paymentId == 0)
                    $('#payment-form #Particulars').val(particulars);
                await buildRemainingBalance(enrollmentId);
            } else {
                $("#payment-form #Balance").val(0);
                $("#payment-form #Discount").val(0);
                $('.balance-details').addClass('d-none');
                let schoolYear = getSchoolYear();
                let paymentAmount = parseFloat($('#payment-form #PaymentAmount').val());
                $('#payment-form #Particulars').val(`P${paymentAmount} Reservation Fee for S.Y. ${schoolYear.description}`);
            }
        } else {
            $('.balance-details').addClass('d-none');
        }
    }
    let onPaymentStudentNumberChange = async () => {

        let enrollmentId = $('#payment-form #StudentNumber').val();
        let paymentOption = $("input[name=payment-option]:checked").val();
        $('#payment-form #FullName').val(enrollmentId).trigger('change.select2');
        if (enrollmentId) {
         
            if (paymentOption == 0) {
                let particulars = await buildParticulars(enrollmentId, Number($('#payment-form #PaymentAmount').val()));
                await buildRemainingBalance(enrollmentId);
                let paymentId = $('#payment-form #PaymentId').val();
                if (!paymentId)
                    $('#payment-form #Particulars').val(particulars);
            } else {
                $("#payment-form #Balance").val(0);
                $("#payment-form #Discount").val(0);
                $('.balance-details').addClass('d-none');
                let schoolYear = getSchoolYear();
                let paymentAmount = parseFloat($('#payment-form #PaymentAmount').val());
                $('#payment-form #Particulars').val(`P${paymentAmount} Reservation Fee for S.Y. ${schoolYear.description}`);
            }

        } else {
            $('.balance-details').addClass('d-none');
        }
    }

    let onBirthdayChange = function (e) {
        let form = $(this).closest('form');
        let date = $(this).val();
        let age = computeAge(date);
        let ageThisJuly = computeAgeThisJuly(date);
        form.find('#Age').val(Math.round(age));
        form.find('#AgeThisJuly').val(ageThisJuly.toFixed(2));
    }

    let computeAge = function (date) {
        let age = moment().diff(date, 'year', true);
        return age;
    }

    let computeAgeThisJuly = function (date) {
        let schoolyear = getSchoolYear();
        let month = 6;
        if (schoolyear) {
            month = schoolyear.monthToComputeAge;
        }
        let year = moment().startOf('year').add(month - 1, 'M');
 
        let age = moment(year).diff(date, 'year', true);

        return age;
    }

    let onFormCancel = async event => {
        event.preventDefault();
        $(event.target).closest('form')[0].reset();
        $(event.target).closest('form')[0].elements[1].focus();
        $(event.target).siblings(':submit').prop("disabled", false).text('Add');
        removeValidation();
        hideViewPaymentSchemeInputs();
        populateSchoolYear('#payment-scheme-form #SchoolYearId');
        populateSchoolYear('#reservation-student-form #SchoolYearId');
        setDefaultSchoolYear();
        $('.action-container').removeClass('d-none');
        $('.action-btn').removeClass('d-none');
        $('.add-new-btn').addClass('d-none');
        $('.add-new-btn').addClass('d-none');
        $('#void-payment-btn').addClass('d-none');
        $('#payment-form #reprint-btn').addClass('d-none');
        $('.user-disabled').prop('disabled', false);
        $('.toggle-active-container').addClass('d-none');
        $('.expired-container').addClass('d-none');
        $('#reservation-student-form .reserved-auto').prop("disabled", false);
        onCancelPaymentForm();
        onCancelPaymentSchemeForm();
    };

    let onValidateConfirmPassword = event => {
        $('.unmatched-password-validation').addClass('d-none');

        if ($('#confirm-password').val())
            if ($('#new-password').val().toLowerCase() !== $('#confirm-password').val().toLowerCase()) {
                $('.unmatched-password-validation').removeClass('d-none');
                return false;
            }

        return true;
    };

    let onSubmitRegisterStudentModal = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('view-student-form');

        if ($(_form).valid()) {

            let currentTabTitle = $('.tab-pane.active .title').text();
            $('#busy-indicator-container').removeClass('d-none');
            let data = _formHelper.toJsonString(_form);
            data.isApproved = true;
            data.isActive = $("input[name=active-status]:checked").val() == 1 ? true : false;
            let message = '';
            let response;
            let registerId = parseInt($('#view-student-form #RegistrationId').val());
            data.UserId = data.UserId == "" ? null : data.UserId;

            var requirementIds = new Array();
            $('#view-student-form input[name="requirement"]:checked').each(function () {
                requirementIds.push($(this).val());
            });
            data.requirementIds = requirementIds.join(',');
            if (data.LeftDate == "")
                data.LeftDate = null;
            if (data.DateReturn == "")
                data.DateReturn = null;

            if (registerId) {
                message = 'Updated Successfully';
                response = await _apiHelper.put({
                    url: 'Authenticated/Registration',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            }
            if (response.ok) {
                let gridId = $('.nav-link.active').data('grid-id');
                $('#' + gridId).DataTable().ajax.reload(null, false);
                renderDropDowns();
                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };
                toastr.success(message);
                $(e.target)[0].reset();
                $('#viewApprovalRegister').modal('hide');
                renderDropDowns();
            } else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }

            $('#busy-indicator-container').addClass('d-none');
        }

    };
    let onEvaluateEnrollementColumns = function (e) {
        let studentTypeText = $('#enrollment-form #StudentTypeId').find("option:selected").text();
        let gradeLevelText = $('#enrollment-form #GradeLevelId').find("option:selected").text();
        let gradeLevel = gradeLevelText.replace(' ', '').toLocaleLowerCase();
        if (studentTypeText.toLocaleLowerCase().includes('local')) {
            $('.foreign-column-container').addClass('d-none');

        }

        if (studentTypeText.toLocaleLowerCase().includes('local') && (gradeLevel.includes('grade11') || gradeLevel.includes('grade12'))) {
            $('.local-column-container').removeClass('d-none');
        }

        if (studentTypeText.toLocaleLowerCase().includes('foreign')) {
            $('.foreign-column-container').removeClass('d-none');
            $('.local-column-container').addClass('d-none');
        }
    }
    let onEvaluatePaymentSchemeColumns = function (e) {

        let isCheck = $(this).is(':checked');
        let container = $(this).data('container');
        $('.' + container).prop('disabled', !isCheck);

    }
    let onEnrollmentSectionChange = function () {
        let sectionVal = $(this).val();
        let enrolledBySection = _enrollStudents.filter(e => e.sectionId == sectionVal);
        let totalEnrolled = enrolledBySection.length;
        if (totalEnrolled > 0) {
            let sectionLimit = enrolledBySection[0].section.studentLimit;
            if (sectionLimit < (totalEnrolled + 1)) {
                Swal.fire(
                    'Error!',
                    'This section has Exceeded the number of limited student.',
                    'error'
                );
                $(this).val(0)
            }

        }

    }
    let onEvaluatePaymentSchemeChange = function () {
        let gradeLevelId = $('#enrollment-form #GradeLevelId').val();
        let studentTypeId = $('#enrollment-form #StudentTypeId').val();
        let schoolYearId = $('#enrollment-form #SchoolYearId').val();
        let paymentBasisId = $('#enrollment-form #PaymentBasisId').val();

        let paymentScheme = _paymentSchemes.filter(p => p.gradeLevelId == gradeLevelId && p.studentTypeId == studentTypeId && p.schoolYearId == schoolYearId && p.paymentBasisId == paymentBasisId);
 
        if (paymentScheme) {
            renderPaymentSchemeDropdown('#enrollment-form #PaymentSchemeId', paymentScheme);
            //$('#enrollment-form #PaymentSchemeId').val(paymentScheme.paymentSchemeId);
        } else {
            $('#enrollment-form #PaymentSchemeId').val('');
        }
    }
    let onReservationGradeLevelChange = function () {
        let gradeLevelId = $('#reservation-student-form #GradeLevelId').val();

        let sectionData = _sections.filter(s => s.gradeLevelId == gradeLevelId);
        if (sectionData) {
            renderDropdown({ name: 'reservation-student-form #SectionId', valueName: 'sectionId', data: sectionData, text: 'description', placeHolder: '-' });
        }
    }

    let onPaymentSchemeGradeLevelChange = function () {
        let gradeLevel = $('#payment-scheme-form #GradeLevelId option:selected').text();
        let studentType = $('#payment-scheme-form #StudentTypeId option:selected').text();
        let paymentBasis = $('#payment-scheme-form #PaymentBasisId option:selected').text();
        let withFSFee = $('#payment-scheme-form #WithFSFee').is(':checked');
        let withICard = $('#payment-scheme-form #WithICard').is(':checked');
        let withSSFee = $('#payment-scheme-form #WithSSFee').is(':checked');
        let withVisaExt = $('#payment-scheme-form #WithVisaExt').is(':checked');
        let withVoucher = $('#payment-scheme-form #WithVoucher').is(':checked');

        let description = `${gradeLevel} / ${studentType} / ${paymentBasis}`;
        if (withFSFee)
            description += ' / FS Fee';
        if (withICard)
            description += ' / ICard';
        if (withSSFee)
            description += ' / SSP Fee';
        if (withVisaExt)
            description += ' / Visa Ext';
        if (withVoucher)
            description += ' / Voucher';
        $('#payment-scheme-form .payment-scheme-description').val(description);
    }
    let onEnrollmentGradeLevelChange = function () {
        let gradeLevelId = $('#enrollment-form #GradeLevelId').val();
        let studentTypeId = $('#enrollment-form #StudentTypeId').val();
        let schoolYearId = $('#enrollment-form #SchoolYearId').val();
        let paymentBasisId = $('#enrollment-form #PaymentBasisId').val();

        let sectionData = _sections.filter(s => s.gradeLevelId == gradeLevelId);
        if (sectionData) {
            renderDropdown({ name: 'enrollment-form #SectionId', valueName: 'sectionId', data: sectionData, text: 'description', placeHolder: '-' });
        }

        let paymentScheme = _paymentSchemes.filter(p => p.gradeLevelId == gradeLevelId && p.studentTypeId == studentTypeId && p.schoolYearId == schoolYearId && p.paymentBasisId == paymentBasisId);
  
        if (paymentScheme) {
            renderPaymentSchemeDropdown('#enrollment-form #PaymentSchemeId', paymentScheme);
            //$('#enrollment-form #PaymentSchemeId').val(paymentScheme.paymentSchemeId);
        } else {
            $('#enrollment-form #PaymentSchemeId').val('');
        }
    }
    let onEnrollmentReportGradeFilterChange = function () {
        let gradeLevelId = $(this).val();
        let sectionData = _sections.filter(s => s.gradeLevelId == gradeLevelId);
        if (sectionData) {
            renderDropdown({ name: 'enrolled-report-section', valueName: 'sectionId', data: sectionData, text: 'description', placeHolder: '-' });
        }
    }
    let onReservationReportGradeFilterChange = function () {
        let gradeLevelId = $(this).val();
        let sectionData = _sections.filter(s => s.gradeLevelId == gradeLevelId);
        if (sectionData) {
            renderDropdown({ name: 'reserved-report-section', valueName: 'sectionId', data: sectionData, text: 'description', placeHolder: '-' });
        }
    }
    let populateRadio = data => {
        let busRiderVal = data.busRider ? 1 : 2;
        let onlineClassVal = data.onlineClass ? 1 : 2;
        let classScheduleVal = data.classScheduleId ? data.classScheduleId : 1;
        $("input[name=classScheduleId][value=" + classScheduleVal + "]").prop('checked', true);
        $("input[name=busRiderRadio][value=" + busRiderVal + "]").prop('checked', true);
        $("input[name=onlineClassRadio][value=" + onlineClassVal + "]").prop('checked', true);
    }

    let populateActive = data => {
        let isActive = data.isActive ? 1 : 0;
        $("input[name=active-status][value=" + isActive + "]").prop('checked', true);
        $('.active-date').addClass('d-none');
        if (isActive) {
            $('.date-active').removeClass('d-none')
        } else {
            $('.date-inactive').removeClass('d-none')
        }

        if (data.isApproved) {
            $('.approved-student').removeClass('d-none');
        } else {
            $('.approved-student').addClass('d-none');
        }
    }

    let evaluateLateEnrollee = function () {
        let schoolYear = getSchoolYear()
        let endOfEnrollmentPeriod = moment(schoolYear.enrollmentPeriodTo);
        let lateEnrolled = endOfEnrollmentPeriod.isBefore(moment(), "day");
        penaltyObject.lateEnrollee.false;
        if (lateEnrolled) {
            $('.penalty-container').removeClass('d-none');
            penaltyObject.lateEnrollee = true;
            Swal.fire({
                title: 'Late Enrollee',
                text: "Do you want to apply penalty?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes',
                cancelButtonText: 'No',
                allowOutsideClick: false,
                allowEscapeKey: false
            }).then(async (result) => {

                if (result.isConfirmed) {
                    penaltyObject.hasPenalty = true;
                    hasPenalty = true;
                    $('#PenaltyAmount').val(5);
                } else if (result.dismiss === Swal.DismissReason.cancel) {
                    penaltyObject.hasPenalty = false;
                    $('#PenaltyAmount').val(0);
                }
            });
        } else {
            $('.penalty-container').addClass('d-none');
        }
    }

    let onSubmitEnrollmentModal = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('enrollment-form');
        let data = _formHelper.toJsonString(_form);
      
        if ($(_form).valid()) {
            let hasPenalty = false;
            $('#busy-indicator-container').removeClass('d-none');
            let data = _formHelper.toJsonString(_form);
            data.busRider = $("input[name=busRiderRadio]").val() == 1 ? true : false;
            data.onlineClass = $("input[name=onlineClassRadio]").val() == 1 ? true : false;
            data.classScheduleId = $("input[name=classScheduleId]").val();
            data.WithFSFee = $('#enrollment-form #WithFSFee').is(':checked');
            data.WithICard = $('#enrollment-form #WithICard').is(':checked');
            data.WithSSFee = $('#enrollment-form #WithSSFee').is(':checked');
            data.WithVisaExt = $('#enrollment-form #WithVisaExt').is(':checked');
            data.WithVoucher = $('#enrollment-form #WithVoucher').is(':checked');
            let message = '';
            let response;
            let currentTabTitle = $('.tab-pane.active .title').text();
          
            if (data.reservationId == 0 || data.reservationId == '')
                data.reservationId = null;
            if (data.enrollmentId == 0 || data.enrollmentId == undefined) {
                message = 'Create Successfully';
                response = await _apiHelper.post({
                    url: 'Authenticated/Enrollment?hasPenalty=' + penaltyObject.lateEnrollee + '&penaltyAmount=' + data.PenaltyAmount,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });

                if (response.ok) {
                    $('#enrollment-grid').DataTable().draw();
                    renderDropDowns();
                    toastr.options = {
                        "preventDuplicates": true,
                        "preventOpenDuplicates": true
                    };
                    toastr.success(message);
                    $(e.target)[0].reset();
                    $('#enrollment-modal').modal('hide');
                }

            } else {
                message = 'Updated Successfully';
                response = await _apiHelper.put({
                    url: 'Authenticated/Enrollment?penaltyAmount=' + data.PenaltyAmount,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                if (response.ok) {
                    $('#enrollment-grid').DataTable().ajax.reload(null, false);
                    renderDropDowns();
                    toastr.options = {
                        "preventDuplicates": true,
                        "preventOpenDuplicates": true
                    };
                    toastr.success(message);
                    $(e.target)[0].reset();
                    $('#enrollment-modal').modal('hide');
                } else if (response.status == 409) {
                    let json = await response.json();
                    Swal.fire(
                        'Error!',
                        json.message,
                        'error'
                    );
                }
            }


            $('#busy-indicator-container').addClass('d-none');
        }

    };

    let onClickApproveRegisterBtn = async (e) => {
        e.preventDefault();
        let _form = $('#view-student-form');
        let id = _form.find('#RegistrationId').val();
        let currentTabTitle = $('.tab-pane.active .title').text();
        let response = await _apiHelper.put({
            url: `Authenticated/Registration/Approved/${id}`,
            requestOrigin: `${currentTabTitle} Tab`,
            requesterName: $('#current-user').text(),
            requestSystem: SYSTEM
        });
        if (response.ok) {
            let table = $('#register-student-approval-grid').DataTable();
            table.draw();
            renderDropDowns();
            Swal.fire(
                'Approved!',
                'Successfuly approved.',
                'success'
            );


        }
        else if (response.status === 409)
            Swal.fire(
                'Error!',
                'Is in use.',
                'error'
            );

        $('#viewApprovalRegister').modal('hide');


        $('#busy-indicator-container').addClass('d-none');
    };

    let onClickDeleteRegisterBtn = async (e) => {
        e.preventDefault();
        $('#reasonModal').modal('toggle');
    };

    let onRegisterStudent = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('register-student-form');

        if ($(_form).valid()) {

            $('#busy-indicator-container').removeClass('d-none');
            let data = _formHelper.toJsonString(_form);
            data.isApproved = true;
            var requirementIds = new Array();
            $('#register-student-form input[name="requirement"]:checked').each(function () {
                requirementIds.push($(this).val());
            });
            data.UserId = data.UserId == "" ? null : data.UserId;
            data.requirementIds = requirementIds.join(',');
            let message = '';
            let response;
            let currentTabTitle = $('.tab-pane.active .title').text();
            message = 'Registered Successfully';
            response = await _apiHelper.post({
                url: 'Authenticated/Registration',
                data: data,
                requestOrigin: `${currentTabTitle} Tab`,
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            });

            if (response.ok) {

                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };
                toastr.success(message);
                $(e.target)[0].reset();
            } else if (response.status == 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            
            }
        }

        $('#busy-indicator-container').addClass('d-none');
    };

    let onReasonFormSubmit = async event => {
        event.preventDefault();
        let form = $(event.target);
        $(event.target).validate();
        let data = _formHelper.toJsonString(event.target);
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            let viewForm = document.getElementById('view-student-form');
            let viewFormData = _formHelper.toJsonString(viewForm);
            let currentTabTitle = $('.tab-pane.active .title').text();
            data.RegistrationId = viewFormData.registrationId;

            response = await _apiHelper.post({
                url: 'Authenticated/Registration/Reason/Email',
                data: data,
                requestOrigin: `${currentTabTitle} Tab`,
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            });
            status = 'Created!';
            if (response.ok) {
                let table = $('#register-student-approval-grid').DataTable();
                table.draw();
                renderDropDowns();
                Swal.fire(
                    'Declined!',
                    'Successfuly declined.',
                    'success'
                );
            }
            else if (response.status == 403) {
                noAccessAlert();
            }
            else if (response.status == 409) {
                toastr.error('Duplicated');
            }
            $('#reasonModal').modal('hide');
            $('#viewApprovalRegister').modal('hide');
            $(event.target)[0].reset();
            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let populatePaymentSchemeBaseOnPaymentBasis = (form, data) => {

        $('#inital-fee-container').removeClass('d-none');
        $('#remaining-fee-container').removeClass('d-none');
        $('.quarterly-container').addClass('d-none');
        $('.monthly-container').addClass('d-none');
        $('.semi-annual-container').addClass('d-none');
        if (data.paymentBasisId == 1) {
            $('#inital-fee').text('Enrollment Fee');
        } else if (data.paymentBasisId == 2) {
            $('#inital-fee').text('Enrollment Fee');
            $('#remaining-fee-container').addClass('d-none');
            $('.semi-annual-container').removeClass('d-none');
            let semiAnnualFee = data.remainingFee / 2;
            $('.semi-annual-fee').val(semiAnnualFee);
        } else if (data.paymentBasisId == 3) {
            $('#inital-fee').text('Enrollment Fee');
            $('#remaining-fee-container').addClass('d-none');
            $('.monthly-container').addClass('d-none');
            $('.quarterly-container').removeClass('d-none');
            let quarterlyFee = data.remainingFee / 4;
            data.firstQuarterFee = quarterlyFee.toFixed(2);
            data.secondQuarterFee = quarterlyFee.toFixed(2);
            data.thirdQuarterFee = quarterlyFee.toFixed(2);
            data.fourthQuarterFee = quarterlyFee.toFixed(2);
        } else if (data.paymentBasisId == 4) {
            $('#inital-fee').text('Enrollment Fee');
            $('#remaining-fee-container').addClass('d-none');
            $('.quarterly-container').addClass('d-none');
            $('.monthly-container').removeClass('d-none');
            let monthlyFee = data.remainingFee / 10;
            $('.monthlyFee').val(monthlyFee);
        }

        _formHelper.populateForm(form, data);
    }
    let onPaymentToggleVoid = async event => {
        let isVoid = $(event.target).data('isVoid');
        let id = $(event.target).data('id');
        let currentTabTitle = $('.tab-pane.active .title').text();

        let response = await _apiHelper.put({
            url: `Authenticated/Payment/Void/${id}/${!isVoid}`,
            requestOrigin: `${currentTabTitle} Tab`,
            requesterName: $('#current-user').text(),
            requestSystem: SYSTEM
        });

        if (response.ok) {
            $('#payment-grid').DataTable().draw();
            toastr.success('Updated');
            onCancelPaymentForm();
            $('.cancel-form').trigger('click');
        } else if (response.status == 409) {
            Swal.fire(
                'Error!',
                'Conflict, Only The Latest Payment can be void.',
                'error'
            );
        }
    }
    let populatePaymentPrintForm = function (data) {
        let studentNumber = $('#payment-form #StudentNumber option:selected').text();
        let fullName = $('#payment-form #FullName option:selected').text();
        let amountInWords = $('#payment-form #amount-in-words').val();
        let date = $('#payment-form #paymentDate').val();
        $('#printable-payment #studentNumberPrint').val(studentNumber);
        $('#printable-payment #fullNamePrint').val(fullName);
        $('#printable-payment #paymentDatePrint').val(date);
        $('#printable-payment #TINNumberPrint').val(data.TINNumber);
        $('#printable-payment #IsCashPrint').prop('checked', data.IsCash);
        $('#printable-payment #IsChequePrint').prop('checked', data.IsCheque);
        $('#printable-payment #bankPrint').val(data.Bank);
        $('#printable-payment #ChequeNumberPrint').val(data.ChequeNumber);
        $('#printable-payment #amount-in-words-print').val(amountInWords);
        $('#printable-payment #ParticularsPrint').val(data.Particulars);
        $('#printable-payment #PaymentAmountPrint').val(data.PaymentAmount);
    }
    let onClickPrint = function () {
        let _form = document.getElementById('payment-form');
        let data = _formHelper.toJsonString(_form);
        populatePaymentPrintForm(data);
        printPayment(_form);
    }
    let printPayment = function (form) {
        $('#printable-payment').removeClass('d-none').printThis({
            // import page CSS
            printContainer: true,
            importCSS: true,
            loadCSS: 'css/printable.css',
            canvas: true,
            // import styles
            importStyle: true,
            copyTagClasses: true,
            copyTagStyles: true,
            removeInline: true,
            afterPrint: function () {
                $('#printable-payment')[0].reset();
                $('#printable-payment').addClass('d-none');
                $(form)[0].reset();
                $(form).find(':submit').text('Add');
                onCancelPaymentForm();
                $('.cancel-form').trigger('click');
            }
            //loadCSS: "/app/css/site.css"

        });
    }

    let onPaymentFormSubmit = async event => {
        event.preventDefault();

        let data = _formHelper.toJsonString(event.target);
        let endpoint = $(event.target).data('form-endpoint');
        let gridId = $(event.target).data('grid-id');
        let currentTabTitle = $('.tab-pane.active .title').text();
        let button = $(event.target).find(':submit').text().toLowerCase();
     
       
        $(event.target).validate();
        if ($(event.target).valid()) {
            let paymentOption = $("input[name=payment-option]:checked").val();
            let paymentTypeVal = $('#payment-form #PaymentType').val();
            $('#busy-indicator-container').removeClass('d-none');
            data.IsCash = paymentTypeVal == 1;
            data.IsCheque = paymentTypeVal == 2;
            data.IsOnlineBanking = paymentTypeVal == 3;
            data.IsPayrollDeduction = paymentTypeVal == 4;
            data.EnrollmentAmount = ENROLLMENT_AMOUNT;
            data.TutionAmount = TUITION_AMOUNT;
            data.PenaltyAmount = PENALTY_AMOUNT;
            data.FSFeeAmount = FSFEE_AMOUNT;
            data.SSFeeAmount = SSFEE_AMOUNT;
            data.ICardAmount = ICARD_AMOUNT;
            data.VisaExtAmount = ICARD_AMOUNT;

            if (paymentOption == 0)
                data.EnrollmentId = $('#payment-form #FullName').val();
            else 
                data.ReservationId = $('#payment-form #FullName').val();
            data.userId = $('#userId').val();

            data.Balance = 0;
            //let discountOnParticulars = 0;
            //let particular = data.Particulars.split('.');
            //$.each(particular, function (i, val) {
            //    if (val.includes('with discount')) {
            //        discountOnParticulars++;
            //    }
            //})

            //let totalDiscount = Number(data.Discount) * discountOnParticulars;

            //if (Number(data.Discount))
            //    data.Balance -= totalDiscount;

            if (button == 'add') {
                response = await _apiHelper.post({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            } else {
                response = await _apiHelper.put({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            }

            if (response.ok) {
                $('#' + gridId).DataTable().ajax.reload(null, false);
                toastr.success('Success');
                populatePaymentPrintForm(data);
                printPayment(event.target);
                await renderDropDowns();
                $('.balance-details').addClass('d-none');

            } else if (response.status == 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }


            $('#busy-indicator-container').addClass('d-none');
        }

    };

    let onRegisterFormSubmit = async event => {
        event.preventDefault();

        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            let currentTabTitle = $('.tab-pane.active .title').text();
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/User/Register',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/User/' + data.userId,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#register-user-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                form.find('.disabled').prop("disabled", false);
                form.find('.user-disabled').prop("disabled", false);
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                renderDropDowns();
            }
            else if (response.status == 403) {
                noAccessAlert();
            }
            else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }

            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let onFormSubmit = async event => {
        event.preventDefault();

        let data = _formHelper.toJsonString(event.target);
        let endpoint = $(event.target).data('form-endpoint');
        let gridId = $(event.target).data('grid-id');
        let currentTabTitle = $('.tab-pane.active .title').text();
        let button = $(event.target).find(':submit').text().toLowerCase();

        if (gridId == 'grade-level-grid') {
            let lastLevel = $('#grade-level-form #LastLevel').is(':checked');
            data.LastLevel = lastLevel;
        }
        if (gridId == 'reservation-student-grid') {
            data.Expired = $('#reservation-student-form #Expired').is(':checked');
        }

        if (gridId == 'school-year-grid') {

        } data.CopyPreviousYearPaymentScheme = $('#school-year-form #CopyPreviousYearPaymentScheme').is(':checked');

        $(event.target).validate();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');

            if (button == 'add') {
                response = await _apiHelper.post({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            } else {
                response = await _apiHelper.put({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            }

            if (response.ok) {
                $('#' + gridId).DataTable().ajax.reload(null, false);
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
               
                $(event.target).find(':submit').text('Add');
                toastr.success('Success');
                renderDropDowns();
            } else if (response.status === 409 || response.status === 404) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            } else {
                //$('#' + gridId).DataTable().draw();
                $('#' + gridId).DataTable().ajax.reload(null, false);
                $(event.target)[0].reset();
            }
                

            $('#busy-indicator-container').addClass('d-none');
        }

    };
    let onPaymentSchemeFormSubmit = async event => {
        event.preventDefault();

        let data = _formHelper.toJsonString(event.target);
        let endpoint = $(event.target).data('form-endpoint');
        let gridId = $(event.target).data('grid-id');
        let currentTabTitle = $('.tab-pane.active .title').text();
        let button = $(event.target).find(':submit').text().toLowerCase();
        data.WithFSFee = $('#payment-scheme-form #WithFSFee').is(':checked');
        data.WithICard = $('#payment-scheme-form #WithICard').is(':checked');
        data.WithSSFee = $('#payment-scheme-form #WithSSFee').is(':checked');
        data.WithVisaExt = $('#payment-scheme-form #WithVisaExt').is(':checked');
        data.WithVoucher = $('#payment-scheme-form #WithVoucher').is(':checked');
        $(event.target).validate();

        if ($(event.target).valid()) {

            if (Number(data.InitialFee) > Number(data.TotalFee)) {
                Swal.fire(
                    'Error!',
                    'Please enter enrollment fee not greater than the total fee',
                    'error'
                );
                return;
            }
            $('#busy-indicator-container').removeClass('d-none');

            if (button == 'add') {
                response = await _apiHelper.post({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            } else {
                response = await _apiHelper.put({
                    url: `${endpoint}`,
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
            }

            if (response.ok) {
                $('#' + gridId).DataTable().ajax.reload(null, false);
                $(event.target)[0].reset();
                $(event.target).find(':submit').text('Add');
                toastr.success('Success');
                renderDropDowns();
            } else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            } else if (response.status === 404) {
                Swal.fire(
                    'Error!',
                    'Bad Request',
                    'error'
                );
            }
                

            $('#busy-indicator-container').addClass('d-none');
        }

    };
    let onEntryChanged = async event => {
        let entry = event.target.value;
        let gridId = $(event.currentTarget).data('grid-id');
        if ($.fn.DataTable.isDataTable('#' + gridId)) {
            $('#' + gridId).DataTable().page.len(entry);
            $('#' + gridId).DataTable().draw();

        }
    };

    let drawTable = event => {
        let gridId = $(event.currentTarget).data('grid-id');
        if ($.fn.DataTable.isDataTable('#' + gridId))
            $('#' + gridId).DataTable().draw();
    }

    let setGradeLevelNumber = function () {
        let lastRecord = null;
        if (_gradeLevels.length > 0) {
            let sortedRecord = _gradeLevels.sort((a, b) => {
                return a.numberLevel - b.numberLevel;
            });
            lastRecord = sortedRecord.reverse()[0];
        }

        let nextNumberLevel = lastRecord ? lastRecord.numberLevel + 1 : 1;
        $("#grade-level-form #NumberLevel").val(nextNumberLevel);
    }

    let setDefaultSchoolYear = function () {
        let lastRecord = null;
        if (_schoolYears.length > 0) {
            lastRecord = _schoolYears.reverse()[0]
        }

        let defaultMonths = 10;
        let form = $('#school-year-form');
        let schoolYearJune = moment().startOf('year').startOf('month').add(6, 'months');

        if (lastRecord != null) {
            defaultMonths = lastRecord.numberOfMonth;
            schoolYearJune = moment(lastRecord.endDate).startOf('year').startOf('month').add(6, 'months');
        }
        form.find('#numberOfMonth').val(defaultMonths);
        form.find('#startYearDate').val(moment(schoolYearJune).format('yyyy-MM-DD'));
        let startYear = form.find('#startYearDate').val();
        form.find('#endYearDate').val(moment(startYear).add(defaultMonths, 'months').format('yyyy-MM-DD'));
        form.find('#periodFrom').val(moment(startYear).subtract(2, 'months').format('yyyy-MM-DD'));
        form.find('#periodTo').val(moment(startYear).add(2, 'weeks').format('yyyy-MM-DD'));
        let endYear = form.find('#endYearDate').val();
        let description = moment(startYear).year() + '-' + moment(endYear).year();

        onEnrollmentStartDateFilterChange();
        form.find('#description').val(description);
        setGradeLevelNumber();

        $('#announcement-form #createdAt').attr('value', moment().format('yyyy-MM-DD'));
        $('#reservation-student-form #createdAt').attr('value', moment().format('yyyy-MM-DD'));
    }

    let populateSchoolYear = function (schoolYearInputId) {
        let currentSchoolYear = getSchoolYear();
        if (currentSchoolYear)
            $(schoolYearInputId).val(currentSchoolYear.schoolYearId);
    }

    let getSchoolYear = function () {
        let currentSchoolYear = _schoolYears.find(s => s.active);
        return currentSchoolYear;
    }

    let getCurrentSchoolYear = function () {
        let thisYear = moment().year();
        let nextYear = moment().add(1, 'years').year();
        let schoolYear = thisYear + '-' + nextYear;
        return schoolYear
    }


    let hideViewPaymentSchemeInputs = function () {
        $('#inital-fee-container').removeClass('d-none');
        $('#remaining-fee-container').addClass('d-none');
        $('.quarterly-container').addClass('d-none');
        $('.monthly-container').addClass('d-none');
        $('.semi-annual-container').addClass('d-none');
    }

    let onReDrawStatementOfAccountGrid = e => {
        if ($.fn.DataTable.isDataTable('#statement-of-account-grid'))
            $('#statement-of-account-grid').DataTable().draw();
    }

    let removeValidation = function () {
        $('.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid').html('');
        $('.field-validation-valid span').html('');
    }

    let evaluateNotConfigureSchoolYear = function (gridId) {

        if (gridId == 'enrollment-grid'
            || gridId == 'payment-grid'
            || gridId == 'payment-scheme-grid') {
            Swal.fire(
                'Warning!',
                'The School Year for this Year is not yet configured. Please go to the School Year module in Master Data to setup the details for this School Year.',
                'error'
            );
        }
    }

    let reDrawTable = event => {
        
        let gridId = $(event.currentTarget).data('grid-id');
      
        $('.nav-panel .nav-pills .nav-link').removeClass('active');
        $(event.currentTarget).addClass('active');
        $('.cancel-form').trigger('click');
        removeValidation();
        if (!isCurrentSchoolYearConfigure) {
            evaluateNotConfigureSchoolYear(gridId);
        }

        if ($(event.currentTarget).hasClass('report')) {
            
            setTimeout(function () { //Add delay for rendering complete ui.
                if ($(event.currentTarget).hasClass('payment-collection-tab'))
                    getPaymentCollectionByDate();
                else if ($(event.currentTarget).hasClass('enrolled-student-report')) {
                    getEnrolledStudentReportData(0, 0, 0);
                }
                else if ($(event.currentTarget).hasClass('reserved-student-report')) {
                    getReservedStudentReportData(0, 0, 0);
                }
                else {
                    if ($.fn.DataTable.isDataTable('#' + gridId)) {
                        $('#' + gridId).DataTable().search('').draw();
                    }
                }
            }, 200);
            
        } else {
            if (!$(event.currentTarget).hasClass('meta-data')) {
                $('#master-data-menu').addClass('d-none');
                $('#register-student-form')[0].reset();
                resetFormToggle();
                $('#paymentDate').attr('value', moment().format('yyyy-MM-DD'));
                $('#announcement-form #createdAt').attr('value', moment().format('yyyy-MM-DD'));
            }
            setTimeout(function () { //Add delay for rendering complete ui.
                if ($.fn.DataTable.isDataTable('#' + gridId)) {
                    $('#' + gridId).DataTable().search('').draw();
                }

            }, 200);
        }
    }

    let resetFormToggle = function () {
        $('.toggle-register-form').removeClass('active');
        $('.registration-form-body').addClass('d-none');
        $('.registration-body').removeClass('d-none');
        $('.registration-body-form').removeClass('d-none');
        $('.student-info-toggle').addClass('active');
        $('.balance-details').addClass('d-none');
    }

    let initializeStudentTypeGrid = async () => {
        let columns = await getStudentTypeColumns();
        let table = $('#student-type-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Student Type"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#student-type-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;

                let response = await _apiHelper.get({
                    url: `Lookups/StudentType/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    //renderStudentTypeDropdown(json.data);

                    renderDropdown({ name: 'requirement-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'view-student-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'register-student-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'payment-scheme-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'enrollment-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'reservation-student-form #StudentTypeId', valueName: 'studentTypeId', data: json.data, text: 'description', placeHolder: '-' });
                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    $('#student-type-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#student-type-form');
                        populateForm(form, data);
                        $('#student-type-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeSchoolYearGrid = async () => {
        let columns = await getSchoolYearColumns();
        let table = $('#school-year-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("School Year"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#school-year-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;

                let response = await _apiHelper.get({
                    url: `Lookups/SchoolYear/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });
                    $('#school-year-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#school-year-form');
                        form.find('#numberOfMonth').val(data.numberOfMonth);
                        form.find('#startYearDate').val(moment(data.startDate).format('yyyy-MM-DD'));
                        form.find('#endYearDate').val(moment(data.endDate).format('yyyy-MM-DD'));
                        form.find('#periodFrom').val(moment(data.enrollmentPeriodFrom).format('yyyy-MM-DD'));
                        form.find('#periodTo').val(moment(data.enrollmentPeriodTo).format('yyyy-MM-DD'));
                        form.find('#description').val(data.description);

                        let activeName = data.active ? "Deactivate" : "Activate";
                        form.find('#toggle-active').text(activeName).data("id", data.schoolYearId);
                        form.find('.toggle-active-container').removeClass('d-none');
                        if (activeName == "Deactivate") {
                            form.find('#toggle-active').removeClass("btn-primary").addClass("btn-danger");
                        } else {
                            form.find('#toggle-active').addClass("btn-primary").removeClass("btn-danger");
                        }
                        onEnrollmentStartDateFilterChange();
                        onEnrollmentEndDateFilterChange();
                        populateForm(form, data);
                        $('#school-year-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });

    }

    let initializeGenderGrid = async () => {
        let columns = await getGenderColumns();
        let table = $('#gender-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Gender"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#gender-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;

                let response = await _apiHelper.get({
                    url: `Lookups/Gender/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });
                    renderDropdown({ name: 'GenderId', valueName: 'genderId', data: json.data, text: 'genderCode', placeHolder: '-' });
                    renderDropdown({ name: 'register-student-form #GenderId', valueName: 'genderId', data: json.data, text: 'genderCode', placeHolder: '-' });
                    renderDropdown({ name: 'reservation-student-form #GenderId', valueName: 'genderId', data: json.data, text: 'genderCode', placeHolder: '-' });
                    //$('.icon-edit').on('click', onClickEdit);
                    $('#gender-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#gender-form');
                        populateForm(form, data);
                        $('#gender-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });

    }
    let initializePaymentSchemeGrid = async () => {
        let columns = await getPaymentSchemeColumns();
        let table = $('#payment-scheme-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Payment Scheme"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-scheme-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;

                let response = await _apiHelper.get({
                    url: `Authenticated/PaymentScheme/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    populateSchoolYear('#payment-scheme-form #SchoolYearId');

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });
                    $('#payment-scheme-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-scheme-form');
                        $('#payment-scheme-form input').prop('disabled', false);
                        hideViewPaymentSchemeInputs();
                        data.totalFee = data.initialFee + data.remainingFee;
                        data.FSFee = data.fsFee;
                        data.SSFee = data.ssFee;
                        populateForm(form, data);
                        $('#payment-scheme-form #WithFSFee,#payment-scheme-form #WithSSFee,#payment-scheme-form #WithICard,#payment-scheme-form #WithVisaExt,#payment-scheme-form #WithVoucher').trigger('change');
                        $('#payment-scheme-form .text-end').attr("type", 'number');
                        $('#payment-scheme-form .add-button').text('Update');
                        $('#payment-scheme-form .action-btn').removeClass('d-none');
                        $('#payment-scheme-form .add-new-btn').addClass('d-none');
                    });
                    $('#payment-scheme-grid tbody').on('click', '.icon-view', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-scheme-form');
                        data.FSFee = data.fsFee;
                        data.SSFee = data.ssFee;
                        data.totalFee = data.initialFee + data.remainingFee;
                        populatePaymentSchemeBaseOnPaymentBasis(form, data);
                        $('.fee-column').prop('disabled', true);
                        $('#payment-scheme-form input').prop('disabled', true);
                        $('#payment-scheme-form .action-btn').addClass('d-none');
                        $('#payment-scheme-form .text-end').attr("type", 'text');
                        $('#payment-scheme-form .text-end').each(function () {
                            let formattedValue = _numberHelper.formatCommaSeperator($(this).val());
                            $(this).val(formattedValue)
                        });
                        $('#payment-scheme-form .add-new-btn').removeClass('d-none');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let populatePaymentType = function (data) {
        if (data.isCash) {
            $('#payment-form #PaymentType').val(1);
        } else if (data.isCheque) {
            $('#payment-form #PaymentType').val(2);
        } else if (data.isOnlineBanking) {
            $('#payment-form #PaymentType').val(3);
        } else {
            $('#payment-form #PaymentType').val(4);
        }
        $('#payment-form #PaymentType').trigger('change');
    }

    let initializePaymentGrid = async () => {
        let columns = await getPaymentColumns();
        let table = $('#payment-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            pagingType: "full",
            order: [1, 'asc'],
            buttons: initializeButton("Payment"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let paymentOption = $("input[name=payment-option]:checked").val();
                let isRegular = paymentOption == 0 ? true : false;
                let response = await _apiHelper.get({
                    url: `Authenticated/Payment/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&regular=${isRegular}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                      
                        let message = '';
                        if (data.enrollment) {
                            message = `${data.enrollment.registration.lastName}, ${data.enrollment.registration.firstName}, ${data.enrollment.registration.middleName}`;

                        } else if (data.reservation) {
                            message = `${data.reservation.lastName}, ${data.reservation.firstName}, ${data.reservation.middleName}`;
                        }
                        message += ` - ${data.orNumber} payment`
                        onClickDelete(e, message)
                    });
                    $('#payment-grid tbody .icon-edit').on('click', function () {
                        var data = table.row($(this).closest('tr')).data();
                      
                        let form = $('#payment-form');
                        form.find('.form-control').prop("disabled", false);
                        form.find('.disable').prop("disabled", true);
                        hideViewPaymentSchemeInputs();
                       
                        if (data.enrollment)
                            data.fullName = data.enrollment.enrollmentId;
                        else 
                            data.fullName = data.reservation.reservationId;
                        data.ORNumber = data.orNumber;
                        data.TINNumber = data.tinNumber;
                        if (isRegular) {
                            data.GradeLabel = data.enrollment.gradeLevel.description;
                            data.SectionLabel = data.enrollment.section.description;
                        } else {
                            data.GradeLabel = data.reservation.gradeLevel.description;
                            data.SectionLabel = data.reservation.section.description;
                        }
                       
                        populatePaymentType(data);
                        populateForm(form, data);
                        $('#payment-form .add-button').text('Update');
                        let date = moment(data.paymentDate);
                        let words = numberToWords(data.paymentAmount);
                        $('#amount-in-words').val(words);
                      
                        form.find('#paymentDate').val(date.format('yyyy-MM-DD'));
                        $('#payment-form .action-btn').removeClass('d-none');
                        $('#payment-form .add-new-btn').addClass('d-none');
                        $('#payment-form #void-payment-btn').addClass('d-none');
                        $('#payment-form #reprint-btn').addClass('d-none');
                        

                    });
                    $('#payment-grid tbody .icon-view').on('click', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-form');
                        form.find('.form-control').prop("disabled", true);
                        form.find('.disable').prop("disabled", true);
                        if (data.enrollment)
                            data.fullName = data.enrollment.enrollmentId;
                        else
                            data.fullName = data.reservation.reservationId;
                        data.ORNumber = data.orNumber;
                        data.TINNumber = data.tinNumber;
                        if (isRegular) {
                            data.GradeLabel = data.enrollment.gradeLevel.description;
                            data.SectionLabel = data.enrollment.section.description;
                        } else {
                            data.GradeLabel = data.reservation.gradeLevel.description;
                            data.SectionLabel = data.reservation.section.description;
                        }
                        populatePaymentType(data);
                        populatePaymentSchemeBaseOnPaymentBasis(form, data);
                        let date = moment(data.paymentDate);
                        let words = numberToWords(data.paymentAmount);
                        $('#amount-in-words').val(words);
                   
                        form.find('#paymentDate').val(date.format('yyyy-MM-DD'));
                        $('#payment-form .action-btn').addClass('d-none');
                        $('#payment-form .add-new-btn').removeClass('d-none');
                        let voidText = data.isVoid ? 'Unvoid' : 'Void';
                        $('#payment-form #void-payment-btn').removeClass('d-none').data('isVoid', data.isVoid).data('id', data.paymentId).text(voidText);
                        $('#payment-form #reprint-btn').removeClass('d-none');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let populatePaymentCollectionBreakdown = function (data) {
        let cash = data.filter(d => d.isCash).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;
        let cheque = data.filter(d => d.isCheque).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;;
        let online = data.filter(d => d.isOnlineBanking).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;;
        let payrollDeduction = data.filter(d => d.isPayrollDeduction).reduce((n, { paymentAmount }) => n + paymentAmount, 0);
        let voided = data.filter(d => d.isVoid).reduce((n, { paymentAmount }) => n + paymentAmount, 0);
        let totalWithoutVoid = data.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
        let total = totalWithoutVoid - voided;
    
        let reservation = data.filter(d => d.reservationId != null).reduce((n, { paymentAmount }) => n + paymentAmount, 0);
        let ssFeeAmount = data.reduce((n, { ssFeeAmount }) => n + ssFeeAmount, 0);
        let fsFeeAmount = data.reduce((n, { fsFeeAmount }) => n + fsFeeAmount, 0);
        let iCardAmount = data.reduce((n, { iCardAmount }) => n + iCardAmount, 0);
        let visaExtAmount = data.reduce((n, { visaExtAmount }) => n + visaExtAmount, 0);
        let penaltyAmount = data.reduce((n, { penaltyAmount }) => n + penaltyAmount, 0);
        let tutionAmount = data.reduce((n, { tutionAmount }) => n + tutionAmount, 0);
        let enrollmentAmount = data.reduce((n, { enrollmentAmount }) => n + enrollmentAmount, 0);
     
        $("#paymentCollectionBreakdown #collection-cash").val(_numberHelper.formatCommaSeperator(cash));
        $("#paymentCollectionBreakdown #collection-cheque").val(cheque?_numberHelper.formatCommaSeperator(cheque) : "00");
        $("#paymentCollectionBreakdown #collection-online").val(online ? _numberHelper.formatCommaSeperator(online) : "00");
        $("#paymentCollectionBreakdown #collection-payroll-deducted").val(payrollDeduction ? _numberHelper.formatCommaSeperator(payrollDeduction) : "00");
        $("#paymentCollectionBreakdown #collection-total-without-void").val("P"+_numberHelper.formatCommaSeperator(totalWithoutVoid));
        $("#paymentCollectionBreakdown #collection-total-with-void").val(voided? _numberHelper.formatCommaSeperator(voided) : "00");
        $("#paymentCollectionBreakdown #collection-total").val("P" + _numberHelper.formatCommaSeperator(total));

        $("#paymentCollectionBreakdown #collection-ssfee").val(ssFeeAmount? _numberHelper.formatCommaSeperator(ssFeeAmount): "00");
        $("#paymentCollectionBreakdown #collection-fsfee").val(fsFeeAmount ? _numberHelper.formatCommaSeperator(fsFeeAmount) : "00");
        $("#paymentCollectionBreakdown #collection-icard").val(iCardAmount ? _numberHelper.formatCommaSeperator(iCardAmount) : "00");
        $("#paymentCollectionBreakdown #collection-visa").val(visaExtAmount ? _numberHelper.formatCommaSeperator(visaExtAmount) : "00");
        $("#paymentCollectionBreakdown #collection-penalty").val(penaltyAmount ? _numberHelper.formatCommaSeperator(penaltyAmount): "00");
        $("#paymentCollectionBreakdown #collection-reservation").val(reservation ? _numberHelper.formatCommaSeperator(reservation) : "00");
        $("#paymentCollectionBreakdown #collection-enrollment").val("P" + enrollmentAmount? _numberHelper.formatCommaSeperator(enrollmentAmount): "00");
        $("#paymentCollectionBreakdown #collection-tuition").val("P" + tutionAmount? _numberHelper.formatCommaSeperator(tutionAmount): "00");
    }

    let getPaymentCollectionByDate = async (startDate, endDate) => {
        let query = "";
     
        if (startDate && endDate)
            query = `startDate=${startDate}&endDate=${endDate}`;
        let response = await _apiHelper.get({
            url: `Authenticated/Payment/Filter/Date?${query}`,
        });

        if (response.ok) {
            let data = await response.json();
            if(data)
                renderPaymentCollectionGrid(data, startDate, endDate);
        } 
    };

    let getEnrolledStudentReportData = async (gradeLevelId, sectionId, schoolYearId) => {
        let response = await _apiHelper.get({
            url: `Authenticated/Enrollment/Report/Filter?gradeLevelId=${gradeLevelId}&sectionId=${sectionId}&schoolYearId=${schoolYearId}`,
        });

        if (response.ok) {
            let data = await response.json();
            if (data)
                renderEnrolledStudentReportGrid(data);
        }
    };

    let getReservedStudentReportData = async (gradeLevelId, sectionId, schoolYearId) => {
        let response = await _apiHelper.get({
            url: `Authenticated/Reservation/Report/Filter?gradeLevelId=${gradeLevelId}&sectionId=${sectionId}&schoolYearId=${schoolYearId}`,
        });

        if (response.ok) {
            let data = await response.json();
            if (data)
                renderReservedStudentReportGrid(data);
        }
    };

    let renderReservedStudentReportGrid = (data) => {
        let grade = $('#reserved-report-grade option:selected').text();
        let section = $('#reserved-report-section option:selected').text();
        let schoolYear = $('#reserved-report-school-year option:selected').text();
        let title = `${grade} ${section} - S.Y. ${schoolYear}`;
        let totalMale = data.filter(g => g.gender.description.toLowerCase() == 'male').length;
        let columns = getReservedStudentReportColumns(totalMale);
        let tableId = '#reserved-student-report-grid';
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().clear().destroy();
        }
        $(tableId).DataTable({
            filter: true,
            paging: false,
            bFilter: true,
            bInfo: true,
            bLengthChange: false,
            bSort: false,
            info: true,
            scrollY: "350px",
            scrollX: true,
            dom: 'Bfrtip',
            order: [[5, 'desc'], [2, 'asc']],
            buttons: [
                {
                    extend: 'excel',
                    text: 'Excel <i class="fas fa-download"></i>',
                    className: 'btn btn-primary ml-3',
                    title: title,
                    footer: true,


                },
                {
                    extend: 'pdf',
                    text: 'PDF <i class="fas fa-download"></i>',
                    className: 'btn btn-primary  ml-3',
                    title: title,
                    footer: true
                },
            ],
            data,
            columns,
            drawCallback: function (settings) {
                var api = this.api();
                var rows = api.rows().nodes();
                var last = null;
                api
                    .column(5)
                    .data()
                    .each(function (group, i) {
                        if (last !== group.description) {
                            $(rows)
                                .eq(i)
                                .before('<tr class="group"><td colspan="9">' + group.description + '</td></tr>');

                            last = group.description;
                        }
                    });
            },
            footerCallback: function () {
                var api = this.api();
                let totalMale = api.column(5).data().filter(g => g.description.toLowerCase() == 'male').length;
                let totalFemale = api.column(5).data().filter(g => g.description.toLowerCase() == 'female').length;
                $('#reserved-student-repoort-foolter-total-male').text('Total Male: ' + totalMale);
                $('#reserved-student-repoort-foolter-total-female').text('Total Female: ' + totalFemale);
                $('#reserved-student-repoort-foolter-total').text('Total: ' + data.length);
            },
        })
    };

    let renderEnrolledStudentReportGrid = (data) => {
        let grade = $('#enrolled-report-grade option:selected').text();
        let section = $('#enrolled-report-section option:selected').text();
        let schoolYear = $('#enrolled-report-school-year option:selected').text();
        let title = `${grade} ${section} - S.Y. ${schoolYear}`;
        let totalMale = data.filter(g => g.registration.gender.description.toLowerCase() == 'male').length;
        let columns = getEnrolledStudentReportColumns(totalMale);
        let tableId = '#enrolled-student-report-grid';
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().clear().destroy();
        }
        $(tableId).DataTable({
            filter: true,
            paging: false,
            bFilter: true,
            bInfo: true,
            bLengthChange: false,
            bSort: false,
            info: true,
            scrollY: "350px",
            scrollX: true,
            dom: 'Bfrtip',
            order: [[5, 'desc'], [2, 'asc']],
            buttons: [
                {
                    extend: 'excel',
                    text: 'Excel <i class="fas fa-download"></i>',
                    className: 'btn btn-primary ml-3',
                    title: title,
                    footer: true,
                   
                   
                },
                {
                    extend: 'pdf',
                    text: 'PDF <i class="fas fa-download"></i>',
                    className: 'btn btn-primary  ml-3',
                    title: title,
                    footer: true
                },
            ],
            data,
            columns,
            drawCallback: function (settings) {
                var api = this.api();
                var rows = api.rows().nodes();
                var last = null;
                api
                    .column(5)
                    .data()
                    .each(function (group, i) {
                        if (last !== group.description) {
                            $(rows)
                                .eq(i)
                                .before('<tr class="group"><td colspan="9">' + group.description + '</td></tr>');

                            last = group.description;
                        }
                    });
            },
            footerCallback: function () {
                var api = this.api();
                let totalMale = api.column(5).data().filter(g => g.description.toLowerCase() == 'male').length;
                let totalFemale = api.column(5).data().filter(g => g.description.toLowerCase() == 'female').length;

                $('#enrolled-student-repoort-foolter-total-male').text('Total Male: ' + totalMale );
                $('#enrolled-student-repoort-foolter-total-female').text('Total Female: ' + totalFemale);
                $('#enrolled-student-repoort-foolter-total').text('Total: ' + data.length);
            },
        });
    };

    let buildPaymentCollectionExportTitle = function (start, end) {
        let startDate = moment(start).format('MM-DD-yyyy');
        let endDate = moment(end).format('MM-DD-yyyy');
        let type = $('#collectionType').val();
        let typeText = $('#collectionType option:selected').text();
        let optionText = $('#collectionTypeOption option:selected').text();
        let date = $('#daily-collection-date').val();

        let ANNUAL = 1,
            SEMI_ANNUAL = 2,
            QUARTERLY = 3,
            MONTHLY = 4,
            DAILY = 5;
        if (type == ANNUAL) {
            return `Payment Collection - ${typeText} - (${optionText})`;
        } else if (type == SEMI_ANNUAL) {
            return `Payment Collection - ${typeText} - (${startDate} - ${endDate})`;
        } else if (type == QUARTERLY) {
            return `Payment Collection - ${typeText} - (${startDate} - ${endDate})`;
        } else if (type == MONTHLY) {
            return `Payment Collection - ${typeText} - (${optionText})`;
        } else if (type == DAILY) {
            if(date)
                return `Payment Collection - ${typeText} - (${moment(date).format('MM-DD-yyyy')})`;
            return `Payment Collection - ${typeText}`;
        }
    }
    let renderPaymentCollectionGrid = (data, start, end) => {
        populatePaymentCollectionBreakdown(data);
      
        let title = buildPaymentCollectionExportTitle(start, end);
        _collectionTitlePage = title;
        let columns = getPaymentCollectionReportColumns();
        let tableId = '#payment-collection-grid';
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().clear().destroy();
        }
        $(tableId).DataTable({
            filter: true,
            paging: true,
            //ordering: true,
            bFilter: true,
            bInfo: true,
            bLengthChange: true,
            info: true,
            scrollY: "350px",
            scrollX: true,
            dom: 'Bfrtip',
            order: [[1, 'asc'], [6, 'asc']],
            buttons: [
                {
                    extend: 'excel',
                    text: 'Excel <i class="fas fa-download"></i>',
                    className: 'btn btn-primary ml-3',
                    title: title,
                    footer: true
                    
                },
                {
                    extend: 'pdf',
                    text: 'PDF <i class="fas fa-download"></i>',
                    className: 'btn btn-primary  ml-3',
                    title: title,
                    footer: true
                   
                },
                {
                    extend: 'print',
                    text: 'Print <i class="fas fa-download"></i>',
                    className: 'btn btn-primary ml-3',
                    title: title,
                    footer: true
                },
            ],
            data,
            columns,
            footerCallback: function (row, start, end, display) {
                let cash = data.filter(d => d.isCash).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;
                let cheque = data.filter(d => d.isCheque).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;;
                let online = data.filter(d => d.isOnlineBanking).reduce((n, { paymentAmount }) => n + paymentAmount, 0);;;
                let payrollDeduction = data.filter(d => d.isPayrollDeduction).reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                let voided = data.filter(d => d.isVoid).reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                let totalWithoutVoid = data.reduce((n, { paymentAmount }) => n + paymentAmount, 0);
                let total = totalWithoutVoid - voided;

                let cashText = cash ? _numberHelper.formatCommaSeperator(cash) : "00";
                let chequeText = cheque ? _numberHelper.formatCommaSeperator(cheque) : "00";
                let onlineText = online ? _numberHelper.formatCommaSeperator(online) : "00";
                let payrollDeductionText = payrollDeduction ? _numberHelper.formatCommaSeperator(payrollDeduction) : "00";
                let voidedText = totalWithoutVoid ? _numberHelper.formatCommaSeperator(totalWithoutVoid) : "00";
                let totalWithoutVoidText = voided ? _numberHelper.formatCommaSeperator(voided) : "00";
                let totalText = total ? _numberHelper.formatCommaSeperator(total) : "00";

                $("#collection-cash-footer").text('Cash:' + cashText);
                $("#collection-cheque-footer").text('Cheque:' + chequeText);
                $("#collection-online-footer").text('Online Banking:' + onlineText);
                $("#collection-payroll-deducted-footer").text('Payroll Deducted:' + payrollDeductionText);
                $("#collection-total-without-void-footer").text('Total:' + "P" + voidedText);
                $("#collection-total-with-void-footer").text('Less Amount Void:' + totalWithoutVoidText);
                $("#collection-total-footer").text('Total Collection:' + "P" + totalText);
            },
        });

        
    };

    let initializePaymentCollectionReportGrid = async () => {
        let columns = await getPaymentCollectionReportColumns();
        let table = $('#payment-collection-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Payment"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-collection-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let paymentOption = $("input[name=payment-option]:checked").val();
                let isRegular = paymentOption == 0 ? true : false;
                let response = await _apiHelper.get({
                    url: `Authenticated/Payment/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&regular=${isRegular}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                   
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });
                  
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeStatementOfAccountGrid = async () => {
        let columns = await getStatementOfAccountColumns();
        let table = $('#statement-of-account-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Statement of account"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#statement-of-account-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let enrollmentId = $('#statement-of-account-container #FullName-account').val();
                if (enrollmentId == null || enrollmentId == '')
                    enrollmentId = 0;

                let response = await _apiHelper.get({
                    url: `Authenticated/StatementOfAccount/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&enrollmentId=${enrollmentId}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('#statement-of-account-grid tbody .icon-view').on('click', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#statement-of-account-form');
                        let date = moment(data.dateIssued);
                     
                        form.find('#dateIssued').val(date.format('yyyy-MM-DD'));
                        let soaContainer = form.find('#soa-container');
                        soaContainer.empty();
                        soaContainer.append(data.statements);
                      
                        _formHelper.populateForm(form, data);
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let populateClassSchedule = data => {
        let container = $('#class-schedule-container');
        container.empty();
        $.each(data.reverse(), function (i, val) {
            let radio = `<input id="${val.classScheduleCode}" type="radio" name="classScheduleId" value="${val.classScheduleId}" ><label class="radio-label" for="${val.classScheduleCode}"><span class="bal-amount"></span>${val.classScheduleCode}</label>`;
            container.append(radio);
        });
    }


    let initializeClassScheduleGrid = async () => {
        let columns = await getClassScheduleColumns();
        let table = $('#class-schedule-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Class Schedule"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#class-schedule-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;

                let response = await _apiHelper.get({
                    url: `Lookups/ClassSchedule/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    populateClassSchedule(json.data);

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#class-schedule-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#class-schedule-form');
                        populateForm(form, data);
                        $('#class-schedule-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });


    }

    let initializeGradeLevelGrid = async () => {
        let columns = await getGradeLevelColumns();
        let table = $('#grade-level-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Grade Level"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#grade-level-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Lookups/GradeLevel/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    //renderGradeLevelDropdown(json.data);
                
                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });
                    //$('.icon-edit').on('click', onClickEdit);
                    $('#grade-level-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#grade-level-form');
                        populateForm(form, data);
                        $('#grade-level-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializePaymentTypeGrid = async () => {
        let columns = await getPaymentTypeColumns();
        let table = $('#payment-type-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Payment Type"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-type-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Lookups/PaymentType/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#payment-type-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-type-form');
                        populateForm(form, data);
                        $('#payment-type-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',

        });
    }

    let initializePaymentBasisGrid = async () => {
        let columns = await getPaymentBasisColumns();
        let table = $('#payment-basis-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Payment Basis"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-basis-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Lookups/PaymentBasis/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    renderDropdown({ name: 'payment-scheme-form #PaymentBasisId', valueName: 'paymentBasisId', data: json.data, text: 'description', placeHolder: '-' });
                    renderDropdown({ name: 'enrollment-form #PaymentBasisId', valueName: 'paymentBasisId', data: json.data, text: 'description', placeHolder: '-' });
                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#payment-basis-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-basis-form');
                        populateForm(form, data);
                        $('#payment-basis-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }
    let initializePaymentMethodGrid = async () => {
        let columns = await getPaymentMethodColumns();
        let table = $('#payment-method-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Payment Method"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#payment-method-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Lookups/PaymentMethod/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#payment-method-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#payment-method-form');
                        populateForm(form, data);
                        $('#payment-method-form .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeUserRoleGrid = async () => {
        let columns = await getUserRoleColumns();
        let table = $('#user-role-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("User Role"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#user-role-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Role/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    renderRoleDropdown(json.data);
                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.description);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#user-role-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#user-role-form');
                        populateForm(form, data);
                        $('#user-role-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeAnnouncementGrid = async () => {
        let columns = await getAnnouncementColumns();
        let table = $('#announcement-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Announcement"),
            "createdRow": function (row, data, dataIndex) {
                if (data.isTodayAnnouncement) {
                    $(row).addClass('red-background');
                }
            },
            ajax: async function (params, success, settings) {
                let gridInfo = $('#announcement-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Announcement/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.title);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#announcement-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#announcement-form');
                        populateForm(form, data);
                        let date = moment(data.createdAt);
                        form.find('#createdAt').val(date.format('yyyy-MM-DD'));
                        $('#announcement-grid .add-button').text('Update');
                    });

                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let getReservationStudentColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "reservationId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Student Number',
                data: "studentNumber",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    if(data)
                        return data;
                    return '-';
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data;
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data;
                },
            },
            {
                title: "Middle Name",
                data: "middleName",
                render: (data, type, row) => {
                    return data;
                },
            },
            {
                title: "Date Reserved",
                data: "createdAt",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "School Year",
                data: "schoolYear.description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Grade Level",
                data: "gradeLevel.description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Section",
                data: "section.description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Expired",
                data: "expired",
                render: (data, type, row) => {
                    return data ? "Yes" : "No";
                },
            },
            {
                title: "Send Notification",
                data: "reservationId",
                render: (data, type, row) => {
                    return '<button class="btn btn-info m-1 send-notification-btn" data-id="' + data + '" data-endpoint="Authenticated/Reservation/Send" data-table="reservation-student-grid" >Send</button>';
                },
            },
        ];
        let lastColumn = {
            data: "reservationId",
            width: "15%",
            render: function (data, type, full) {
                let buttons = ''
    
                if ((!full.enrollment || (full.enrollment && full.enrollment.deleted)) && full.payments.length > 0 && !full.expired)
                    buttons += '<button class="btn btn-primary m-1 enroll-btn" data-id="' + full.reservationId + '" data-endpoint="Authenticated/Reservation/Enroll" data-table="enrollment-grid" >Enroll</button>';
              
                buttons += '<a href="#" class="m-1 icon-edit" data-id="' + full.reservationId + '" data-endpoint="Authenticated/Reservation" data-table="reservation-student-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.reservationId + '" data-endpoint="Authenticated/Reservation" data-table="reservation-student-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let enrollReservation = async (data, endpoint, gridId) => {
        let currentTabTitle = $('.tab-pane.active .title').text();
        let response = await _apiHelper.post({
            url: `${endpoint}`,
            data: data,
            requestOrigin: `${currentTabTitle} Tab`,
            requesterName: $('#current-user').text(),
            requestSystem: SYSTEM
        });

        if (response.ok) {
            let json = await response.json();
          
            $('#busy-indicator-container').addClass('d-none');
            let table = $('#' + gridId).DataTable();
            table.draw();
            $('#v-pills-enrollment-tab').trigger('click');

            let form = $('#enrollment-form');
            form[0].reset();
            json.gradeLevelId = data.gradeLevelId;
            json.sectionId = data.sectionId;
            json.schoolYearId = data.schoolYearId;
            json.reservationId = data.reservationId;
            json.fullName = `${json.firstName} ${json.lastName}`;
            _formHelper.populateForm(form, json);

            $('#enrollment-form .date-container').addClass('d-none');
            $('#enrollment-modal').modal('show');
        } 

    }

    let sendReservationNotification = async (endpoint, id) => {
        $('#busy-indicator-container').removeClass('d-none');
        let response = await _apiHelper.post({
            url: `${endpoint}/${id}`,
        });

        if (response.ok) {
            toastr.success('Sending of email notification successful!');
            $('#busy-indicator-container').addClass('d-none');
        } else {
            $('#busy-indicator-container').addClass('d-none');
        }

    }
    let initializeReservationGrid = async () => {
        let columns = await getReservationStudentColumns();
        let table = $('#reservation-student-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Reservation"),
            "createdRow": function (row, data, dataIndex) {
                if (data.willExpireSoon) {
                    $(row).addClass('red-background');
                }
            },
            ajax: async function (params, success, settings) {
                let gridInfo = $('#reservation-student-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Reservation/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    populateSchoolYear('#reservation-student-form #SchoolYearId');

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        onClickDelete(e, message);
                    });

                    $('#reservation-student-grid tbody .enroll-btn').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        let data = table.row($(this).closest('tr')).data();
                        let taget = $(e.currentTarget);
                        let id = taget.data('id');
                        let endpoint = taget.data('endpoint');
                        let gridId = taget.data('table');
                        enrollReservation(data, endpoint, gridId);
                        
                    });
                    $('#reservation-student-grid tbody .send-notification-btn').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        let taget = $(e.currentTarget);
                        let id = taget.data('id');
                        let endpoint = taget.data('endpoint');
                        sendReservationNotification(endpoint, id);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#reservation-student-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#reservation-student-form');
                        populateForm(form, data);
                        let date = moment(data.birthdayDate);
                        form.find('.expired-container').removeClass('d-none');
                        form.find('#birthdayDate').val(date.format('yyyy-MM-DD'));
                        form.find('#createdAt').val(moment(data.createdAt).format('yyyy-MM-DD'));
                        $('#reservation-student-form .add-button').text('Update');
                    });

                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeSectionGrid = async () => {
        let columns = await getSectionColumns();
        let table = $('#section-grid').DataTable({
            bLengthChange: true,
            "bStateSave": true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Section"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#section-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Section/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.gradeLevel.gradeLevelCode} - ${data.description}`
                        onClickDelete(e, message);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#section-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#section-form');
                        populateForm(form, data);
                        $('#section-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeRequirementGrid = async () => {
        let columns = await getRequirementColumns();
        let table = $('#requirement-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Requirement"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#requirement-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Requirement/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        onClickDelete(e, data.requirementList);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#requirement-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#requirement-form');
                        populateForm(form, data);
                        $('#requirement-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',

        });
    }

    let initializeModuleGrid = async () => {
        let columns = await getModuleColumns();
        let table = $('#module-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Module"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#module-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Lookups/Module/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', onClickDelete);

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#module-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#module-form');
                        populateForm(form, data);
                        $('#module-grid .add-button').text('Update');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializePendingUserGrid = async () => {
        let columns = await getUnApproveUserColumns();
        let table = $('#pending-user-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Pending User"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#pending-user-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/User/Unapproved/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        onClickDelete(e, message);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('#pending-user-grid tbody').on('click', '.approve-btn', onApproveUser);
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }
    let initializePendingStudentRegistrationGrid = async () => {
        let columns = await getUnApproveStudentRegistrationColumns();
        let table = $('#register-student-approval-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Pending Student"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#register-student-approval-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Registration/Unapproved/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        onClickDelete(e, message);
                    });

                    //$('.icon-edit').on('click', onClickEdit);
                    $('.view-btn').on('click', onClickViewButton);
                    $('#register-student-approval-grid tbody').on('click', '.approve-btn', onApproveUser);
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }
    let initializeButton = function (title) {
        let buttons = [
            //{
            //    extend: 'excel',
            //    text: 'Excel <i class="fas fa-download"></i>',
            //    className: 'btn btn-primary ml-3',
            //    title: title,
            //    exportOptions: {
            //        columns: ':not(:last-child)',
            //    }
            //},
            //{
            //    extend: 'pdf',
            //    text: 'PDF <i class="fas fa-download"></i>',
            //    className: 'btn btn-primary  ml-3',
            //    title: title,
            //    exportOptions: {
            //        columns: ':not(:last-child)',
            //    }
            //},
            //{
            //    extend: 'print',
            //    text: 'Print <i class="fas fa-download"></i>',
            //    className: 'btn btn-primary ml-3',
            //    title: title,
            //    exportOptions: {
            //        columns: ':not(:last-child)',
            //    }
            //},
        ];

        return buttons;
    }
    let initializeApprovedStudentRegistrationGrid = async () => {
        let columns = await getApproveStudentRegistrationColumns();
        let table = $('#registered-student-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Approve Student"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#registered-student-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Registration/Approved/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.view-btn').on('click', onClickViewButton);
                    $('.view-payment-btn').on('click', onClickViewPaymentButton);
                    $('#registered-student-grid .edit-btn').on('click', onClickEditButton);
                  
                    $('.delete-btn').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        onClickDelete(e, message);
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeAuditTrailGrid = async () => {
        let columns = await getAuditTrailColumns();
        let table = $('#audit-trail-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Audit Trail"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#audit-trail-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Audit/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&system=${SYSTEM}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeEnrollmentGrid = async () => {
        let columns = await getEnrollmentColumns();
        let table = $('#enrollment-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Enrollment"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#enrollment-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Enrollment/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&active=${true}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });
             
                    $('#enrollment-grid tbody .delete-btn').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        let enrollment = data.enrollments.find(e => e.schoolYear.active);
                        if (enrollment)
                            onDeleteEnrollment(enrollment.enrollmentId, message);
                    });

                    $('#enrollment-grid tbody').on('click', '.enroll-btn', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        var data = table.row($(this).closest('tr')).data();
                    
                        let nextGradeLevel;
                        if (data.enrollments) {
                            let sortedRecord = data.enrollments.sort((a, b) => {
                                return a.numberLevel - b.numberLevel;
                            });
                            lastEnrollment = sortedRecord.reverse()[0];

                            let sortedGrade = _gradeLevels.sort((a, b) => {
                                return a.numberLevel - b.numberLevel;
                            });

                            if (lastEnrollment) {
                                let gradeLevelIndex = sortedGrade.findIndex(e => e.gradeLevelId == lastEnrollment.gradeLevelId);
                                nextGradeLevel = sortedGrade[gradeLevelIndex + 1];
                            } 
                        }
                        let form = $('#enrollment-form');
                        form[0].reset();
                        data.fullName = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        if (nextGradeLevel)
                            data.gradeLevelId = nextGradeLevel.gradeLevelId;

                        populateSchoolYear('#enrollment-form #SchoolYearId');
                        _formHelper.populateForm(form, data);
                        populateRadio(data);
                        evaluateLateEnrollee(data);
                        $('#enrollment-form .date-container').addClass('d-none');
                        $('#enrollment-modal').modal('show');
                    });
                    $('#enrollment-grid tbody').on('click', '.edit-btn', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#enrollment-form');
                        form[0].reset();
                        populateSchoolYear('#enrollment-form #SchoolYearId');
                        data.fullName = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        let enrollment = data.enrollments.find(e => e.schoolYear.active);
                        let penalty = enrollment.penalty.find(e => e.enrollmentId == enrollment.enrollmentId);
                        
                        if (enrollment) {
                            populateRadio(enrollment);
                            data.gradeLevelId = enrollment.gradeLevelId;
                            data.sectionId = enrollment.sectionId;
                            data.paymentBasisId = enrollment.paymentBasisId;
                            data.paymentSchemeId = enrollment.paymentSchemeId;
                            data.discount = enrollment.discount;
                            data.enrollmentId = enrollment.enrollmentId;
                            data.schoolYearId = enrollment.schoolYear.schoolYearId;
                            data.studentTypeId = enrollment.studentTypeId;
                            if (penalty) {
                                let penaltyPercentage = (penalty.amount / enrollment.paymentScheme.initialFee) * 100;
                                data.penaltyAmount = penaltyPercentage;
                                $('#enrollment-form .penalty-container').removeClass('d-none');
                            } else {
                                $('#enrollment-form .penalty-container').addClass('d-none');
                            }
                            $('#enrollment-form #WithFSFee').prop('checked', enrollment.withFSFee);
                            $('#enrollment-form #WithICard').prop('checked', enrollment.withICard);
                            $('#enrollment-form #WithSSFee').prop('checked', enrollment.withSSFee);
                            $('#enrollment-form #WithVisaExt').prop('checked', enrollment.withVisaExt);
                            $('#enrollment-form #WithVoucher').prop('checked', enrollment.withVoucher);
                            $('#enrollment-form #DateCreated').val(moment(enrollment.createdAt).format('yyyy-MM-DD'));
                            $('#enrollment-form .date-container').removeClass('d-none');
                        }

                        _formHelper.populateForm(form, data);
                        $('#enrollment-form #SectionId').val(data.sectionId);
                      
                        $('#enrollment-modal').modal('show');
                    });
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let initializeStudentWithBalanceEnrollmentGrid = async () => {
        let columns = await getInactiveEnrollmentColumns();
        let table = $('#student-with-balances-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Student Balance"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#student-with-balances-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let statusVal = $("input[name=registration-status]:checked").val();
                let status = null;
                if (statusVal == 1)
                    status = true;
                else if (statusVal == 2)
                    status = false;

                let activeParam = statusVal != 0 ? `&active=${status}` : '';
                let response = await _apiHelper.get({
                    url: `Authenticated/Enrollment/FilterBalance?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}${activeParam}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                  
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('#student-with-balances-grid .view-btn').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        let icon = $(e.currentTarget);
                        let id = icon.data('id');
                        var data = table.row($(this).closest('tr')).data();
                        $('#balance-details-form .student-number').text(data.studentNumber ? data.studentNumber : "");
                        $('#balance-details-form .student-name').text(`${data.lastName}, ${data.firstName}, ${data.middleName}`);
                        $('#balance-details-form .school-year').text(data.enrollments[0].schoolYear.description);
                        $('#balance-details-form .section').text(data.enrollments[0].section.description);
                        $('#balance-details-form .grade-level').text(data.enrollments[0].gradeLevel.description);
                        $('#balance-details-form .total-balance').text(_numberHelper.formatCommaSeperator(data.enrollments[0].balance));
                        onClickViewBalanceDetailButton(id);
                    });
                   
                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }
    let initializeUserGrid = async () => {
        let columns = await getApproveUserColumns();
        let table = $('#register-user-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            buttons: initializeButton("Register User"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#register-user-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/User/Approved/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;

                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        let message = `${data.lastName}, ${data.firstName}`;
                        onClickDelete(e, message);
                    });

                    $('.toggle-active-btn').on('click', onClickToggleActiveUser);

                    $('#register-user-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#register-user-form');
                        data.UserName = data.username;
                        form.find('.user-disabled').prop("disabled", true);

                        populateForm(form, data);
                        $('#register-user-form .add-button').text('Update');
                    });

                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);
    }

    let onApproveUser = async (e) => {
        e.preventDefault();
        $('#busy-indicator-container').removeClass('d-none');
        let taget = $(e.currentTarget);
        let id = taget.data('id');
        let endpoint = taget.data('endpoint');
        let gridId = taget.data('table');
        let currentTabTitle = $('.tab-pane.active .title').text();

        Swal.fire({
            title: 'Are you sure?',
            text: "You want to approve this user?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, approve it!'
        }).then(async (result) => {
            if (result.value) {
                let response = await _apiHelper.put({
                    url: `${endpoint}/${id}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });

                if (response.ok) {
                    $('#busy-indicator-container').addClass('d-none');
                    let table = $('#' + gridId).DataTable();
                    table.draw();

                    Swal.fire(
                        'Approved!',
                        'User Aprrove',
                        'success'
                    );
                }

            }
        });

        $('#busy-indicator-container').addClass('d-none');

    }

    let onDeleteEnrollment = async (id, message) => {
        Swal.fire({
            title: 'Are you sure?',
            text: `Are you sure you want to delete ${message}?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then(async (result) => {
            if (result.value) {
                let currentTabTitle = $('.tab-pane.active .title').text();
                let response = await _apiHelper.delete({
                    url: `Authenticated/Enrollment/${id}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                if (response.ok) {
                    let table = $('#enrollment-grid').DataTable();
                    let info = table.page.info();
                    let isLast = (info.end - info.start) == 1;
                    if (isLast) {
                        table.draw();
                    } else {
                        table.ajax.reload(null, false);
                    }

                    Swal.fire(
                        'Deleted!',
                        'Successfuly deleted.',
                        'success'
                    );

                }
                else if (response.status === 409 || response.status === 400 || response.status === 404)
                    Swal.fire(
                        'Error!',
                        'Is in use.',
                        'error'
                    );

            }
        });
    }

    let onClickDelete = async (e, message) => {
        e.preventDefault();
        let deleteIcon = $(e.currentTarget);
        let id = deleteIcon.data('id');
        let active = deleteIcon.data('active');
        let endpoint = deleteIcon.data('endpoint');
        let gridId = deleteIcon.data('table');
        let textMessage = "You won't be able to revert this!";
        if (active) {
            toastr.error('Cannot delete active school year. Please deactivate first this school year record for it to be deleted.!');
            return;
        }
        if (message)
            textMessage = `Are you sure you want to delete ${message}?`
        Swal.fire({
            title: 'Are you sure?',
            text: textMessage,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then(async (result) => {
            if (result.value) {
                let currentTabTitle = $('.tab-pane.active .title').text();
                let response = await _apiHelper.delete({
                    url: `${endpoint}/${id}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                if (response.ok) {
                    let table = $('#' + gridId).DataTable();
                    let info = table.page.info();
                    let isLast = (info.end - info.start) == 1;
                    if (isLast) {
                        table.draw();
                    } else {
                        table.ajax.reload(null, false);
                    }

                    Swal.fire(
                        'Deleted!',
                        'Successfuly deleted.',
                        'success'
                    );
                    renderDropDowns();

                }
                else if (response.status === 409 || response.status === 400 || response.status === 404)
                    Swal.fire(
                        'Error!',
                        'Is in use.',
                        'error'
                    );

            }
        });
    }

    let onClickToggleActiveUser = async (e) => {
        e.preventDefault();
        let deleteIcon = $(e.currentTarget);
        let id = deleteIcon.data('id');
        let active = deleteIcon.data('active');
        let endpoint = deleteIcon.data('endpoint');
        let gridId = deleteIcon.data('table');
        let status = active ? "Deactivate" : "Activate";
        Swal.fire({
            title: status,
            text: "Are you sure you want to " + status + " this user?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes'
        }).then(async (result) => {
            if (result.value) {
                let currentTabTitle = $('.tab-pane.active .title').text();
                let response = await _apiHelper.put({
                    url: `${endpoint}/${id}/Status/${!active}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                     requestSystem: SYSTEM
                });
                if (response.ok) {
                    let table = $('#' + gridId).DataTable();
                    table.ajax.reload(null, false);
                  
                    Swal.fire(
                        'Updated!',
                        'Successfuly updated.',
                        'success'
                    );
                    renderDropDowns();

                }
                else if (response.status === 404)
                    Swal.fire(
                        'Error!',
                        'Bad Request',
                        'error'
                    );
            }
        });
    }

    let onGetAndPopulateRegistrationModal = async (id, form) => {
        response = await _apiHelper.get({
            url: `Authenticated/Registration/${id}`,
        });
        if (response.ok) {
            let data = await response.json();
            if (data) {
                _formHelper.populateForm(form, data);
                let birthdayDate = moment(data.birthdayDate);
                form.find('#birthdayDate').val(birthdayDate.format('yyyy-MM-DD'));
                let leftDate = moment(data.leftDate);
                form.find('#leftDate').val(leftDate.format('yyyy-MM-DD'));
                let dateRegistered = moment(data.dateRegistered);
                form.find('#date-registed').val(dateRegistered.format('yyyy-MM-DD'));
                let dateReturn = moment(data.dateReturn);
                form.find('#dateReturn').val(dateReturn.format('yyyy-MM-DD'));
                $('#viewApprovalRegister').modal('show');
                populateActive(data);
                populateRequirements(data.studentTypeId, data.registrationId);

            }
        }
    }

    let onGetAndPopulatePaymentHistoryModal = async (id, gradeLevelId, schoolYearId) => {
        response = await _apiHelper.get({
            url: `Authenticated/Registration/${id}/Payment?gradeLevelId=${gradeLevelId}&schoolYearId=${schoolYearId}`,
        });
        if (response.ok) {
            $("#payment-history-id").val(id);
            let data = await response.json();
            if (data) {
                let totalAmount = 0;
                if (data.payments.length > 0) {
                    totalAmount = data.payments.reduce((n, { amount }) => n + amount, 0);
                }
                $('#payment-history-form .student-number').text(data.studentNumber ? data.studentNumber : "");
                $('#payment-history-form .student-name').text(data.studentName);
                $('#payment-history-form .last-school-year').text(data.lastSchoolYearAttended);
                $('#payment-history-form .lastest-grade-level').text(data.latestGradeLevel);
                $('#payment-history-form .status').text(data.status);
                $('#payment-history-form .total-amount').text(totalAmount);
                $('#payment-history-modal').modal('show');
                renderPaymentHistoryGrid(data.payments);
            }
        }
    }

    let getPaymentHistoryColumns = () => {
        let columns = [
            {
                title: 'Grade Level',
                data: "gradeLevel",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "School Year",
                data: "schoolYear",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Date Paid",
                data: "datePaid",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data)
                },
            },
            {
                title: "O.R. No.",
                data: "orNumber",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Amount",
                data: "amount",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Particular",
                data: "particulars",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
       
        return columns;
    };

    let renderPaymentHistoryGrid = (data) => {
        let columns = getPaymentHistoryColumns();
        let tableId = '#payment-history-grid';
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().clear().destroy();
            $(tableId).empty();
        }
        $(tableId).DataTable({
            filter: true,
            paging: true,
            ordering: true,
            bFilter: true,
            bInfo: true,
            bLengthChange: true,
            info: true,
            scrollY: "350px",
            scrollX: true,
            dom: 'Bfrtip',
            buttons: initializeButton("Payment History"),
            data,
            columns
        });
    };

    let onClickViewPaymentButton = async (e) => {
        e.preventDefault();
        e.stopPropagation();

        let icon = $(e.currentTarget);
        let id = icon.data('id');
        let form = $('#view-student-form');
        await onGetAndPopulatePaymentHistoryModal(id, 0, 0);
        
    }

    let onClickViewButton = async (e) => {
        e.preventDefault();
        e.stopPropagation();
       
        let icon = $(e.currentTarget);
        let id = icon.data('id');
        let isApproved = icon.data('approved');
        let form = $('#view-student-form');
        await onGetAndPopulateRegistrationModal(id, form);
        form.find('input').prop("disabled", true);
        form.find('select').prop("disabled", true);
        $('#registration-save-modal-btn').addClass('d-none');
        $('#delete-register-student-btn').removeClass('d-none');
        $('#approve-register-student-btn').removeClass('d-none');
        resetFormToggle();
        if (isApproved) {
            $('#delete-register-student-btn').addClass('d-none');
            $('#approve-register-student-btn').addClass('d-none');
        }
    }

    let onClickViewBalanceDetailButton = async (id) => {
     
        await buildRemainingBalance(id);
     
        $('.balance-details').addClass('d-none');
        $('#balance-details-modal .balance-details').removeClass('d-none');
        $('#balance-details-modal').modal('show');
    }

    let onClickEditButton = async (e) => {
        e.preventDefault();
        e.stopPropagation();
        let icon = $(e.currentTarget);
        let id = icon.data('id');
        let form = $('#view-student-form');
        resetFormToggle();
        removeValidation();
        await onGetAndPopulateRegistrationModal(id, form);
        form.find('input').prop("disabled", false);
        form.find('select').prop("disabled", false);
        form.find('#date-registed').prop("disabled", true);
        form.find('.disabled').prop("disabled", true);
        $('#registration-save-modal-btn').removeClass('d-none');
        $('#delete-register-student-btn').addClass('d-none');
        $('#approve-register-student-btn').addClass('d-none');
    }

    let getStudentTypeColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "studentTypeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "studentTypeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "studentTypeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.studentTypeId + '" data-form="student-type-form"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.studentTypeId + '" data-endpoint="Lookups/StudentType" data-table="student-type-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getSchoolYearColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "schoolYearId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "No. Of Months",
                data: "numberOfMonth",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: 'Start of Year',
                data: "startDate",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: 'End of Year',
                data: "endDate",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Enrollment Start From",
                data: "enrollmentPeriodFrom",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Enrollment End To",
                data: "enrollmentPeriodTo",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Active",
                data: "active",
                render: (data, type, row) => {
                    return data ? "True" : "False";
                },
            },

        ];
        let lastColumn = {
            data: "schoolYearId",
            width: "10em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.schoolYearId + '" data-endpoint="Lookups/SchoolYear" data-table="school-year-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.schoolYearId + '"  data-active="' + full.active + '" data-endpoint="Lookups/SchoolYear" data-table="school-year-grid"><i class="fas fa-trash border-icon"></i></a>';
                
                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getGenderColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "genderId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "genderId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Code.',
                data: "genderCode",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "studentTypeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.genderId + '" data-endpoint="Lookups/Gender" data-table="gender-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.genderId + '" data-endpoint="Lookups/Gender" data-table="gender-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getPaymentSchemeColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentSchemeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "paymentSchemeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'School Year',
                data: "schoolYear",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data.description;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    if(data)
                        return data;

                    return "-";
                },
            },
            {
                title: "Student Type",
                data: "studentType",
                render: (data, type, row) => {
                    return data.description;
                },
            },
            {
                title: "Payment Basis",
                data: "paymentBasis",
                render: (data, type, row) => {
                    return data.description;
                },
            },

        ];
        let lastColumn = {
            data: "paymentSchemeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '';
                if(isRegistrar)
                    buttons += '<a href="#" class="m-1 icon-edit" data-id="' + full.paymentSchemeId + '" data-endpoint="Authenticated/PaymentScheme" data-table="payment-scheme-grid"><i class="fas fa-edit"></i></a>';
                
                buttons += '<a href="#" class="m-1 icon-view" data-id="' + full.paymentSchemeId + '" data-endpoint="Authenticated/PaymentScheme" data-table="payment-scheme-grid"><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.paymentSchemeId + '" data-endpoint="Authenticated/PaymentScheme" data-table="payment-scheme-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getPaymentColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Student Name',
                data: "enrollment.registration",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (full.enrollment) {
                        return `${full.enrollment.registration.lastName}, ${full.enrollment.registration.firstName}, ${full.enrollment.registration.middleName}`;

                    } else if (full.reservation) {
                        return `${full.reservation.lastName}, ${full.reservation.firstName}, ${full.reservation.middleName}`;
                    }

                    return '-';
                },
            },
            {
                title: 'School Year',
                data: "enrollment.schoolYear",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data) 
                        return data.description;
                    return '-';
                }
            },
            {
                title: 'Grade Level',
                data: "enrollment.gradeLevel",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return data.description;
                    return '-';
                }
            },
            {
                title: 'Section',
                data: "enrollment.section",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return data.description;
                    return '-';
                }
            },
            {
                title: "Date",
                data: "paymentDate",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Amount",
                data: "paymentAmount",
                render: (data, type, row) => {
                    return _numberHelper.formatCommaSeperator(data);
                },
            },
            {
                title: "Voided",
                data: "isVoid",
                render: (data, type, row) => {
                    return data ? "Yes" : "No";
                },
            },
            {
                title: "OR Number",
                data: "orNumber",
                render: (data, type, row) => {
                    return data;
                },
            },

        ];
        let lastColumn = {
            data: "paymentId",
            width: "3em",
            render: function (data, type, full) {
              
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.paymentId + '" data-endpoint="Authenticated/Payment" data-table="payment-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-view" data-id="' + full.paymentId + '" data-endpoint="Authenticated/Payment" data-table="payment-grid"><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.paymentId + '" data-full="' + full + '" data-endpoint="Authenticated/Payment" data-table="payment-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getPaymentCollectionReportColumns = () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Date",
                data: "paymentDate",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: 'Student Name',
                data: "enrollment.registration",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (full.enrollment) {
                        return `${full.enrollment.registration.lastName}, ${full.enrollment.registration.firstName}, ${full.enrollment.registration.middleName}`;

                    } else if (full.reservation) {
                        return `${full.reservation.lastName}, ${full.reservation.firstName}, ${full.reservation.middleName}`;
                    }

                    return '-';
                },
            },
            {
                title: 'Grade Level',
                data: "enrollment.gradeLevel",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return data.description;
                    return '-';
                }
            },
           
            {
                title: "Amount",
                data: "paymentAmount",
                className: 'noVis dt-right',
                render: (data, type, row) => {
                    return _numberHelper.formatCommaSeperator(data);
                },
            },
            {
                title: "Voided",
                data: "isVoid",
                render: (data, type, row) => {
                    return data ? "Yes" : "No";
                },
            },
            {
                title: "OR Number",
                data: "orNumber",
                render: (data, type, row) => {
                    return data;
                },
            },

        ];
        return columns;
    };
    let getEnrolledStudentReportColumns = (totalMale) => {
        let columns = [
            {
                title: 'No.',
                data: "enrollementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber;
                    if (row.registration.gender.description.toLocaleLowerCase() == 'female')
                        rowNumber = (Number(meta.row) + 1) - totalMale;
                    else 
                        rowNumber = Number(meta.row) + 1;

                    return rowNumber;
                },
            },
            {
                title: "Student No.",
                data: "registration.studentNumber",
                render: (data, type, row) => {
                    if (data)
                        return data;
                    return '-';
                },
            },
            {
                title: 'Student Name',
                data: "registration",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data) 
                        return `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                    return '-';
                }
            },
            {
                title: 'BDay',
                data: "registration",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return _dateHelper.formatDateFullMonth(data.birthdayDate);
                    return '-';
                }
            },
            {
                title: "Age",
                data: "registration",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (data)
                        return data.ageThisJuly;
                    return '-';
                },
            },
            {
                title: "Gender",
                data: "registration.gender",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (data)
                        return data.description;
                    return '-';
                },
            },
            {
                title: "Parent/Guardian",
                data: "registration.parentDetail",
                render: (data, type, row) => {
                    let view = [];
                    if (data) {
                        if (data.fatherName)
                            view.push(data.fatherName);
                        if (data.motherName)
                            view.push(data.motherName);
                        if (data.guardianName)
                            view.push(data.guardianName);
                    }

                    if(view.length > 0)
                        return view.join("/");
                    return '-';
                },
            },
            {
                title: "Address",
                data: "registration.homeAddress",
                render: (data, type, row) => {
                    if (data)
                        return `${data}`;
                    return '-';
                },
            },
            {
                title: "Contact No.",
                data: "registration",
                render: (data, type, row) => {
                    let view = '';
                    if (data.contactNumber1)
                        view += data.contactNumber1
                    if (data.contactNumber2)
                        view += "/" + data.contactNumber1
                    if (view)
                        return view;
                    return '-';
                },
            },
         
        ];
        return columns;
        };
    let getReservedStudentReportColumns = (totalMale) => {
        let columns = [
            {
                title: 'No.',
                data: "reservationId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber;
                    if (row.gender.description.toLocaleLowerCase() == 'female')
                        rowNumber = (Number(meta.row) + 1) - totalMale;
                    else
                        rowNumber = Number(meta.row) + 1;

                    return rowNumber;
                },
            },
            {
                title: "Student No.",
                data: "studentNumber",
                render: (data, type, row) => {
                    if (data)
                        return data;
                    return '-';
                },
            },
            {
                title: 'Student Name',
                data: "reservationId",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return `${full.lastName}, ${full.firstName}, ${full.middleName}`;
                    return '-';
                }
            },
            {
                title: 'Grade',
                data: "gradeLevel",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, full) => {
                    if (data)
                        return data.description;
                    return '-';
                }
            },
            {
                title: "Section",
                data: "section",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (data)
                        return data.description;
                    return '-';
                },
            },
            {
                title: "Gender",
                data: "gender",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (data)
                        return data.description;
                    return '-';
                },
            },
            {
                title: "School Year",
                data: "schoolYear",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (data)
                        return data.description;
                    return '-';
                },
            },
            {
                title: "Contact No.",
                data: "contactNumber",
                render: (data, type, row) => {
                    if (data)
                        return data;
                    return '-';
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    if (data)
                        return data;
                    return '-';
                },
            },

        ];
        return columns;
    };
    let getStatementOfAccountColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "StatementOfAccountId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Statement No.',
                data: "statementOfAccountId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Date Issued",
                data: "dateIssued",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: 'Student Name',
                data: "fullName",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
        ];
        let lastColumn = {
            data: "paymentId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-view" data-id="' + full.StatementOfAccountId + '" data-endpoint="Authenticated/Payment" data-table="payment-grid"><i class="fas fa-eye"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getClassScheduleColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "classScheduleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "classScheduleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Code',
                data: "classScheduleCode",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "classScheduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.classScheduleId + '" data-endpoint="Lookups/ClassSchedule" data-table="class-schedule-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.classScheduleId + '" data-endpoint="Lookups/ClassSchedule" data-table="class-schedule-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getPaymentTypeColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentTypeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "paymentTypeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "paymentTypeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.paymentTypeId + '" data-endpoint="Lookups/PaymentType" data-table="payment-type-grid"><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.paymentTypeId + '" data-endpoint="Lookups/PaymentType" data-table="payment-type-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getPaymentBasisColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentBasisId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "paymentBasisId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "paymentBasisId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.paymentBasisId + '" data-endpoint="Lookups/PaymentBasis" data-table="payment-basis-grid" ><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.paymentBasisId + '" data-endpoint="Lookups/PaymentBasis" data-table="payment-basis-grid" ><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getPaymentMethodColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "paymentMethodId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "paymentMethodId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "paymentMethodId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.paymentMethodId + '" data-endpoint="Lookups/PaymentMethod" data-table="payment-method-grid" ><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.paymentMethodId + '" data-endpoint="Lookups/PaymentMethod" data-table="payment-method-grid" ><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getModuleColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "moduleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "moduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.moduleId + '" data-endpoint="Lookups/Module" data-table="module-grid" ><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.moduleId + '" data-endpoint="Lookups/Module" data-table="module-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getUnApproveUserColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "userId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },

            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "User Name",
                data: "username",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    return data
                },
            },
            //{
            //    title: "Contact Number",
            //    data: "contactNumber",
            //    render: (data, type, row) => {
            //        return data
            //    },
            //},

        ];
        let lastColumn = {
            data: "moduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<button class="btn btn-primary mr-4 approve-btn" data-id="' + full.userId + '" data-endpoint="Authenticated/User/Approved" data-table="pending-user-grid" >Approve</button>';
                buttons += '<button class="btn btn-danger m-1 delete-btn" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="pending-user-grid">Delete</button>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getUnApproveStudentRegistrationColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "registrationId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Age",
                data: "age",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Gender",
                data: "gender.genderCode",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    return data
                },
            },
            //{
            //    title: "Contact Number",
            //    data: "contactNumber",
            //    render: (data, type, row) => {
            //        return data
            //    },
            //},

        ];
        let lastColumn = {
            data: "moduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<button class="btn btn-info mr-4 view-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Registration/Approved" data-table="register-student-approval-grid" >View</button>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getApproveStudentRegistrationColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "registrationId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Middle Name",
                data: "middleName",
                render: (data, type, row) => {
                    return data
                },
            },

            {
                title: "Student Number",
                data: "studentNumber",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Student Type",
                data: "studentType.description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Status",
                data: "isActive",
                render: (data, type, row) => {
                    return data ? "Active" : "Inactive"
                },
            },
            //{
            //    title: "Contact Number",
            //    data: "contactNumber",
            //    render: (data, type, row) => {
            //        return data
            //    },
            //},

        ];
        let lastColumn = {
            data: "moduleId",
            width: "30%",
            render: function (data, type, full) {
              
                let buttons = '<button class="btn btn-info mr-4 view-btn" data-approved="true" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Registration/Approved" data-table="register-student-approval-grid" >View</button>';
                buttons += '<button class="btn btn-primary m-1 edit-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/User/Approved" data-table="pending-user-grid" >Edit</button>';
                buttons += '<button class="btn btn-danger m-1 delete-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Registration" data-table="registered-student-grid">Delete</button>';
                buttons += '<button class="btn btn-info mr-4 view-payment-btn" data-approved="true" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Registration/Payment" data-table="register-student-approval-grid" >View Payment</button>';
                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getAuditTrailColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "id",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Id',
                data: "id",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "User Name",
                data: "userName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Action",
                data: "type",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Page",
                data: "requestOrigin",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Table Name",
                data: "tableName",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        return columns;
    };

    let getEnrollmentColumns = async () => {
        let columns = [
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },

            {
                title: 'Student Number',
                data: "studentNumber",
                //className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Student Type",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let current = data.find(e => e.schoolYear.active);
                        if (current) {
                            return current.studentType.description;
                        }
                        let last = data.sort((a, b) => {
                            return b.enrollmentId - a.enrollmentId
                        });

                        return last.studentType.description;;
                    } else {
                        return '-';
                    }
                },
            },
            {
                title: "School Year",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let current = data.find(e => e.schoolYear.active);
                        if (current) {
                            return current.schoolYear.description;
                        }
                        let last = data.sort((a, b) => {
                            return b.enrollmentId - a.enrollmentId
                        });

                        return last.schoolYear.description;;
                    } else {
                        return '-';
                    }
                },
            },
            {
                title: "Grade",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let current = data.find(e => e.schoolYear.active);
                        if (current) {
                            return current.gradeLevel.description;
                        }
                        let last = data.sort((a, b) => {
                            return b.enrollmentId - a.enrollmentId
                        });

                        return last.gradeLevel.description;;
                    } else {
                        return '-';
                    }
                },
            },
            {
                title: "Section",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let current = data.find(e => e.schoolYear.active);
                        if (current) {
                            return current.section.description;
                        }
                        let last = data.sort((a, b) => {
                            return b.enrollmentId - a.enrollmentId
                        });

                        return last.section.description;;
                    } else {
                        return '-';
                    }
                },
            },
            {
                title: "Payment Basis",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let current = data.find(e => e.schoolYear.active);
                        if (current) {
                            return current.paymentBasis.description;
                        }
                        let last = data.sort((a, b) => {
                            return b.enrollmentId - a.enrollmentId
                        });

                        return last.paymentBasis.description;;
                    } else {
                        return '-';
                    }
                },
            },
            {
                title: "Current Year Balance",
                data: "enrollments",
                className: 'noVis dt-right',
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let enrollment = data.find(e => e.schoolYear.active);
                        if (enrollment) {
                            if (enrollment.balance > 0)
                                return _numberHelper.formatCommaSeperator(enrollment.balance);

                            return '-'
                        } 
                            
                        return '-';
                    } else {
                        return '-';
                    }

                },
            },
            {
                title: "Previous Year Balance",
                data: "enrollments",
                className: 'noVis dt-right',
                render: (data, type, row) => {
                    if (data.length > 0) {
                        let enrollments = data.filter(e => !e.schoolYear.active);
                        let total = enrollments.reduce((n, { balance }) => n + balance, 0);
                        if (total > 0)
                            return _numberHelper.formatCommaSeperator(total);
                        return '-';
                    } else {
                        return '-';
                    }

                },
            },

        ];
        let lastColumn = {
            data: "moduleId",
            width: "15%",
            render: function (data, type, full) {
                let buttons = '';
                let enrollment = full.enrollments.find(e => e.schoolYear.active);
        
                if (enrollment) {
                    buttons += '<button class="btn btn-primary m-1 edit-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Enrollment/Approved" data-table="enrollment-user-grid" >Edit</button>';
                    buttons += '<button class="btn btn-danger m-1 delete-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Enrollment" data-table="enrollment-user-grid" >Delete</button>';
                } else {
                    if (full.reservations.length == 0)
                        buttons += '<button class="btn btn-primary m-1 enroll-btn" data-id="' + full.registrationId + '" data-endpoint="Authenticated/Enrollment/Approved" data-table="enrollment-user-grid" >Enroll</button>';
                    else
                        buttons += 'Reserved';
                }

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getInactiveEnrollmentColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "registrationId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Midlle Name",
                data: "middleName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Status",
                data: "isActive",
                render: (data, type, row) => {
                    return data ? "Active": "Inactive"
                },
            },
            {
                title: "Contact Number",
                data: "contactNumber1",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Grade",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0)
                        return data[0].gradeLevel.description;
                    return "-"
                },
            },
            {
                title: "Section",
                data: "enrollments",
                render: (data, type, row) => {
                    if (data.length > 0)
                        return data[0].section.description;
                    return "-"
                },
            },
            {
                title: "Latest S.Y.",
                data: "enrollments",
                render: (data, type, row) => {
                   
                    if (data.length > 0) {
                        return data[0].schoolYear.description;
                    }

                    return "-"
                },
            },
            {
                title: "Total Balance",
                data: "enrollments",
                className: 'noVis dt-right',
                render: (data, type, row) => {
                    if (data.length > 0) {
                        //let total = data.reduce((n, { balance }) => n + balance, 0);
                        //let sortedData = data.sort((a, b) => {
                        //    return a.enrollmentId - b.enrollmentId;
                        //});
                        //if (sortedData)
                        if (data[0].balance)
                            return _numberHelper.formatCommaSeperator(data[0].balance);
                   
                    } 
                    return "-";
                },
            },
        ];
        let lastColumn = {
            data: "moduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '';
                if (full.enrollments.length > 0)
                    buttons += '<button class="btn btn-info view-btn" data-id="' + full.enrollments[0].enrollmentId + '" data-endpoint="Authenticated/Registration/Approved" data-table="register-student-approval-grid" >View balance detail(s)</button>';
           
                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getApproveUserColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "userId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },

         
            {
                title: "Role",
                data: "role",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Active",
                data: "active",
                render: (data, type, row) => {
                    return data ? "True" : "False";
                },
            },

        ];
        let lastColumn = {
            data: "userId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '';
                if (full.active)
                    buttons += '<button class="btn btn-danger m-1 toggle-active-btn" data-active="' + full.active + '" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="register-user-grid" >Deactivate</button>';
                else 
                    buttons += '<button class="btn btn-primary m-1 toggle-active-btn" data-active="' + full.active + '" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="register-user-grid" >Activate</button>';

                buttons += '<a href="#" class="m-1 icon-edit" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="register-user-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="register-user-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };


    let getGradeLevelColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "gradeLevelId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "gradeLevelId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Number Level',
                data: "numberLevel",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Code',
                data: "gradeLevelCode",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Last Level",
                data: "lastLevel",
                render: (data, type, row) => {
                    return data ? "True" : "False";
                },
            },

        ];
        let lastColumn = {
            data: "gradeLevelId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.gradeLevelId + '" data-endpoint="Lookups/GradeLevel" data-table="grade-level-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.gradeLevelId + '" data-endpoint="Lookups/GradeLevel" data-table="grade-level-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getUserRoleColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "roleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'No.',
                data: "roleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Name",
                data: "name",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "userRoleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.roleId + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.roleId + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getAnnouncementColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "announcementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Id',
                data: "announcementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Date",
                data: "createdAt",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Title",
                data: "title",
                render: (data, type, row) => {
                    return data
                },
            },


        ];
        let lastColumn = {
            data: "announcementId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.announcementId + '" data-endpoint="Authenticated/Announcement" data-table="announcement-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.announcementId + '" data-endpoint="Authenticated/Announcement" data-table="announcement-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };
    let getSectionColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "sectionId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "sectionId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Section",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Grade Level",
                data: "gradeLevel.gradeLevelCode",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Limit",
                data: "studentLimit",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "sectionId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.sectionId + '" data-endpoint="Authenticated/Section" data-table="section-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.sectionId + '" data-endpoint="Authenticated/Section" data-table="section-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let getRequirementColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "requirementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'ID',
                data: "requirementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "StudentType",
                data: "studentType.description",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Requirement",
                data: "requirementList",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "sectionId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.requirementId + '" data-endpoint="Authenticated/Requirement" data-table="requirement-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.requirementId + '" data-endpoint="Authenticated/Requirement" data-table="requirement-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let renderDropdown = params => {
        let parent = '';
        let name = '';
        let dataName = '';

        if (params.hasOwnProperty('name'))
            name = params.name;

        let dropdown = $('#' + name);
        dropdown.empty();

        let valueName = params.valueName != undefined ? params.valueName : 'id';

        if (params.placeHolder) {
            dropdown.append($("<option />").val("").text(params.placeHolder));
        }
        $.each(params.data, function () {

            let text = `${this[params.text]}`;

            if (this[valueName] && text) {
                dropdown.append($("<option />").val(this[valueName]).text(text));
            }

        });

        if (params.isSelect2) {
            dropdown.select2({
                theme: "bootstrap",
                width: 'element',
                width: 'resolve',
                closeOnSelect: !params.autoClose,
            });
        }

    }
    let renderDataList = (id, data) => {
   
        let dropdown = $(id);
        dropdown.empty();
        //dropdown.append($('<option />').val("").text('-'));
      
        $.each(data, function (i, item) {
            if (typeof item == 'object')
                dropdown.append($('<option data-value="' + item.id + '" />').text(item.value));
            else
                dropdown.append($("<option />").val(item).text(item));
        });
    }

    let renderPaymentSchemeDropdown = (id, data) => {
     
        let dropdown = $(id);
        dropdown.empty();
        dropdown.append($('<option />').val("").text('-'));
        $.each(data, function (i, val) {
            let finalText = `${val.schoolYear.description} - ${val.description}`;
            dropdown.append($("<option />").val(val.paymentSchemeId).text(finalText));

        });
    }

    let renderRoleDropdown = (data) => {

        let dropdown = $('#register-user-form #RoleId');
        dropdown.empty();
        dropdown.append($('<option />').val("").text('Select Role'));
        $.each(data, function (i, val) {
            dropdown.append($("<option />").val(val.roleId).text(val.name));

        });
    }

    let getDropdownData = async () => {
        let [enrollStudentResp, registrationDataListResp, parentUserResp,
            schoolYearResp, sectionResp, paymentSchemeResp, gradeLevelResp,
            reservationResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/Enrollment/Balance`
            }),
            _apiHelper.get({
                url: `Authenticated/Registration/DataList`
            }),
            _apiHelper.get({
                url: `Authenticated/User/Role/3`
            }),
            _apiHelper.get({
                url: `Lookups/SchoolYear`
            }),
            _apiHelper.get({
                url: `Authenticated/Section`
            }),
            _apiHelper.get({
                url: `Authenticated/PaymentScheme`
            }),
            _apiHelper.get({
                url: `Lookups/GradeLevel`
            }),
             _apiHelper.get({
                 url: `Authenticated/Reservation`
            }),

        ]);

        let [enrollStudents, registrationDataList, parentUserList, schoolYearList,
            sectionList, paymentSchemeList, gradeLevelList, reservationList] = await Promise.all(
            [
                enrollStudentResp.json(),
                registrationDataListResp.json(),
                parentUserResp.json(),
                schoolYearResp.json(),
                sectionResp.json(),
                paymentSchemeResp.json(),
                gradeLevelResp.json(),
                reservationResp.json(),
            ]
        );
        _enrollStudents = enrollStudents;
        _dataList = registrationDataList;
        _parentUsers = parentUserList;
        _schoolYears = schoolYearList;
        _sections = sectionList;
        _gradeLevels = gradeLevelList;
        _paymentSchemes = paymentSchemeList;
        _reservations = reservationList;
    }

    let renderDropDowns = async () => {
        await getDropdownData();

        renderDropdown({ name: 'payment-form #StudentNumber', valueName: 'enrollmentId', data: _enrollStudents, text: 'studentNumber', placeHolder: '-', isSelect2: true });
        renderDropdown({ name: 'payment-form #FullName', valueName: 'enrollmentId', data: _enrollStudents, text: 'fullName', placeHolder: '-', isSelect2: true });
        renderDropdown({ name: 'statement-of-account-container #FullName-account', valueName: 'enrollmentId', data: _enrollStudents, text: 'fullName', placeHolder: '-', isSelect2: true });
        renderDropdown({ name: 'register-student-form #UserId', valueName: 'userId', data: _parentUsers, text: 'fullName', placeHolder: '-', isSelect2: true });
        renderDropdown({ name: 'view-student-form #UserId', valueName: 'userId', data: _parentUsers, text: 'fullName', placeHolder: '-', isSelect2: true });
        renderDropdown({ name: 'payment-scheme-form #SchoolYearId', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'enrollment-form #SchoolYearId', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'reservation-student-form #SchoolYearId', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'enrolled-report-school-year', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'reserved-report-school-year', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'payment-history-report-school-year', valueName: 'schoolYearId', data: _schoolYears, text: 'description', placeHolder: '-' });

        renderDropdown({ name: 'section-form #GradeLevelId', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'payment-scheme-form #GradeLevelId', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'enrollment-form #GradeLevelId', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'reservation-student-form #GradeLevelId', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'enrolled-report-grade', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'reserved-report-grade', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });
        renderDropdown({ name: 'payment-history-report-grade', valueName: 'gradeLevelId', data: _gradeLevels, text: 'description', placeHolder: '-' });

   
        renderDataList('#reservation-student-form #StudentNumberList', _dataList.studentNumber);
        renderDataList('#reservation-student-form #LastNameList', _dataList.fullName);
        renderDataList('#reservation-student-form #FirstNameList', _dataList.fullName);

        renderDataList('#register-student-form #CitizenshipList', _dataList.citizenship);
        renderDataList('#register-student-form #ReligionList', _dataList.religion);
        renderDataList('#register-student-form #LanguageSpokenAtHomeList', _dataList.languageSpoken);
        renderDataList('#register-student-form #PreviousSchoolAttendedList', _dataList.previousSchoolAttended);
        renderDataList('#register-student-form #SchoolAddressList', _dataList.schoolAdress);
        renderDataList('#register-student-form #FatherCitizenshipList', _dataList.fatherCitizenship);
        renderDataList('#register-student-form #MotherCitizenshipList', _dataList.motherCitizenship);
        renderDataList('#register-student-form #FatherEducationalAttainmentList', _dataList.fatherEducationalAttainment);
        renderDataList('#register-student-form #MotherEducationalAttainmentList', _dataList.motherEducationalAttainment);
        renderDataList('#register-student-form #FatherNameOfSchoolAttendedList', _dataList.fatherSchoolAttended);
        renderDataList('#register-student-form #MotherNameOfSchoolAttendedList', _dataList.motherSchoolAttended);
        renderDataList('#register-student-form #FatherOccupationList', _dataList.fatherOccupation);
        renderDataList('#register-student-form #MotherOccupationList', _dataList.motherOccupation);
        renderDataList('#register-student-form #FatherPlaceOfWorkList', _dataList.fatherPlaceOfWork);
        renderDataList('#register-student-form #MotherPlaceOfWorkList', _dataList.motherPlaceOfWork);

        renderDataList('#view-student-form #CitizenshipList', _dataList.citizenship);
        renderDataList('#view-student-form #ReligionList', _dataList.religion);
        renderDataList('#view-student-form #LanguageSpokenAtHomeList', _dataList.languageSpoken);
        renderDataList('#view-student-form #PreviousSchoolAttendedList', _dataList.previousSchoolAttended);
        renderDataList('#view-student-form #SchoolAddressList', _dataList.schoolAdress);
        renderDataList('#view-student-form #FatherCitizenshipList', _dataList.fatherCitizenship);
        renderDataList('#view-student-form #MotherCitizenshipList', _dataList.motherCitizenship);
        renderDataList('#view-student-form #FatherEducationalAttainmentList', _dataList.fatherEducationalAttainment);
        renderDataList('#view-student-form #MotherEducationalAttainmentList', _dataList.motherEducationalAttainment);
        renderDataList('#view-student-form #FatherNameOfSchoolAttendedList', _dataList.fatherSchoolAttended);
        renderDataList('#view-student-form #MotherNameOfSchoolAttendedList', _dataList.motherSchoolAttended);
        renderDataList('#view-student-form #FatherOccupationList', _dataList.fatherOccupation);
        renderDataList('#view-student-form #MotherOccupationList', _dataList.motherOccupation);
        renderDataList('#view-student-form #FatherPlaceOfWorkList', _dataList.fatherPlaceOfWork);
        renderDataList('#view-student-form #MotherPlaceOfWorkList', _dataList.motherPlaceOfWork);

        renderPaymentSchemeDropdown('#enrollment-form #PaymentSchemeId', _paymentSchemes);
        evaluateAgeToCompute();
        setDefaultSchoolYear();
        isCurrentSchoolYearConfigure = getSchoolYear() ? true : false;
       
    };

    let initializeModals = e => {
        $('#enrollment-modal').modal({ backdrop: 'static', keyboard: false });
        $('#reasonModal').modal({ backdrop: 'static', keyboard: false });
        $('#viewApprovalRegister').modal({ backdrop: 'static', keyboard: false });
        evaluateBirthDateRestriction();
    }

    let getCurrentLogIn = async () => {

        $('#busy-indicator-container').removeClass('d-none');
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
            user = await response.json();
            let form = $('#user-detail-form');
            _formHelper.populateForm(form, user);
            isRegistrar = user.roleName == "Administrator";
            _userId = user.userId;
            await renderDropDowns();
        }

        $('#busy-indicator-container').addClass('d-none');
    };

    let initializeGrids = e => {
        getCurrentLogIn();
        populatePenaltyOptions();
        initializeAnnouncementGrid();
        initializePendingStudentRegistrationGrid();
        initializeApprovedStudentRegistrationGrid();
        initializeReservationGrid();
        initializePaymentSchemeGrid();
        initializePaymentGrid();
        initializeEnrollmentGrid();
        initializeStudentWithBalanceEnrollmentGrid();
        initializeStatementOfAccountGrid();
        initializeStudentTypeGrid();
        initializeSchoolYearGrid();
        initializeGenderGrid();
        initializeClassScheduleGrid();
        initializeGradeLevelGrid();
        initializePaymentTypeGrid();
        initializePaymentBasisGrid();
        initializePaymentMethodGrid();
        initializeSectionGrid();
        initializeModuleGrid();
        initializeUserRoleGrid();
        initializeRequirementGrid();
        initializeUserGrid();
        initializePendingUserGrid();
        initializeAuditTrailGrid();
    }


    $(document).ready(function () {
        initializeGrids();
        attachEvents();
        initializeModals();
    });

})(jQuery);