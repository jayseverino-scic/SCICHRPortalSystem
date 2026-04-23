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
    const SYSTEM = 'hr portal';

    let _dataList = null;
    let _userId = 0;
    let isRegistrar = false;


    let attachEvents = () => {
        $('.nav-link').on(CLICK_EVENT, reDrawTable);
        $('.cancel-form').on(CLICK_EVENT, onFormCancel);
        $('#payment-form #reprint-btn').on('click', onClickPrint);

        $('#register-student-form #UserId').on('change', populateEmail);

        $('#edit-user-detail').on('click', onClickEditIcon);
        $('#user-detail-form').on('submit', onSaveUserDetailForm);
        $('#change-password-form').on('submit', onResetPasswordSubmitted);

        $('#print-collection').on('click', onClickPrintCollection);
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
     
    //let onToggleSchoolYearActive = function () {
    //    let id = $(this).data('id');
    //    let currentText = $(this).text();
    //    UpdateSchoolYearAsync(id);
    //    let newText = currentText == "Deactivate" ? "Activate" : "Deactivate"
    //    $(this).text(newText);

    //    if (newText == "Deactivate") {
    //        $(this).removeClass("btn-primary").addClass("btn-danger");
    //    } else {
    //        $(this).addClass("btn-primary").removeClass("btn-danger");
    //    }
        
    //}

 
    //let onSearchParentEmail = async (email) => {
    //    let response = await _apiHelper.get({
    //        url: 'Authenticated/User/Email/' + email,
    //    });

    //    if (response.ok) {
    //        let data = await response.json();
    //        if (data) {
    //            $('#register-student-form #UserId').val(data.userId);
    //        }
    //    }
    //}

    //let onBlurRegisterStudentEmail = function(e) {
    //    let email = $(this).val();
    //    onSearchParentEmail(email);
    //}

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

    //let numberToWords = function (number) {
    //    var digit = ['Zero', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine'];
    //    var elevenSeries = ['Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
    //    var countingByTens = ['Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];
    //    var shortScale = ['', 'Thousand', 'Million', 'Billion', 'Trillion'];

    //    number = number.toString(); number = number.replace(/[\, ]/g, '');
    //    if (number != parseFloat(number)) return 'not a number';
    //    var x = number.indexOf('.');
    //    if (x == -1) x = number.length;
    //    if (x > 15) return 'too big'; var n = number.split('');
    //    var str = '';
    //    var sk = 0;
    //    for (var i = 0; i < x; i++) {
    //        if ((x - i) % 3 == 2) {
    //            if (n[i] == '1') {
    //                str += elevenSeries[Number(n[i + 1])] + ' '; i++; sk = 1;
    //            } else if (n[i] != 0) {
    //                str += countingByTens[n[i] - 2] + ' '; sk = 1;
    //            }
    //        } else if (n[i] != 0) {
    //            str += digit[n[i]] + ' ';
    //            if ((x - i) % 3 == 0)
    //                str += 'Hundred '; sk = 1;
    //        }
    //        if ((x - i) % 3 == 1) {
    //            if (sk)
    //                str += shortScale[(x - i - 1) / 3] + ' '; sk = 0;

    //        }
    //    }
    //    str = str.trim() + " pesos ";
    //    if (x != number.length) {
    //        var y = number.length; str += 'and ';
    //        for (var i = x + 1; i < y; i++) {
    //            if ((y - i) % 3 == 2) {
    //                if (n[i] == '1') {
    //                    str += elevenSeries[Number(n[i + 1])] + ' '; i++; sk = 1;
    //                } else if (n[i] != 0) {
    //                    str += countingByTens[n[i] - 2] + ' '; sk = 1;
    //                }
    //            } else if (n[i] != 0) {
    //                str += digit[n[i]] + ' ';
    //                if ((y - i) % 3 == 0) str += 'Hundred '; sk = 1;
    //            } if ((y - i) % 3 == 1) { if (sk) str += shortScale[(y - i - 1) / 3] + ' '; sk = 0; }

    //        }
    //    }
    //    str = str.replace(/\number+/g, ' ');
    //    if (x != number.length)
    //        return str.trim() + " centavos.";
    //    return str.trim() + ".";
    //}
    
   
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

    //let onClickPrint = function () {
    //    let _form = document.getElementById('payment-form');
    //    let data = _formHelper.toJsonString(_form);
    //    populatePaymentPrintForm(data);
    //    printPayment(_form);
    //}
   
    //let onFormSubmit = async event => {
    //    event.preventDefault();

    //    let data = _formHelper.toJsonString(event.target);
    //    let endpoint = $(event.target).data('form-endpoint');
    //    let gridId = $(event.target).data('grid-id');
    //    let currentTabTitle = $('.tab-pane.active .title').text();
    //    let button = $(event.target).find(':submit').text().toLowerCase();

    //    if (gridId == 'grade-level-grid') {
    //        let lastLevel = $('#grade-level-form #LastLevel').is(':checked');
    //        data.LastLevel = lastLevel;
    //    }
    //    if (gridId == 'reservation-student-grid') {
    //        data.Expired = $('#reservation-student-form #Expired').is(':checked');
    //    }

    //    if (gridId == 'school-year-grid') {

    //    } data.CopyPreviousYearPaymentScheme = $('#school-year-form #CopyPreviousYearPaymentScheme').is(':checked');

    //    $(event.target).validate();
    //    if ($(event.target).valid()) {
    //        $('#busy-indicator-container').removeClass('d-none');

    //        if (button == 'add') {
    //            response = await _apiHelper.post({
    //                url: `${endpoint}`,
    //                data: data,
    //                requestOrigin: `${currentTabTitle} Tab`,
    //                requesterName: $('#current-user').text(),
    //                requestSystem: SYSTEM
    //            });
    //        } else {
    //            response = await _apiHelper.put({
    //                url: `${endpoint}`,
    //                data: data,
    //                requestOrigin: `${currentTabTitle} Tab`,
    //                requesterName: $('#current-user').text(),
    //                requestSystem: SYSTEM
    //            });
    //        }

    //        if (response.ok) {
    //            $('#' + gridId).DataTable().ajax.reload(null, false);
    //            $(event.target)[0].reset();
    //            $(event.target)[0].elements[1].focus();
               
    //            $(event.target).find(':submit').text('Add');
    //            toastr.success('Success');
    //            renderDropDowns();
    //        } else if (response.status === 409 || response.status === 404) {
    //            let json = await response.json();
    //            Swal.fire(
    //                'Error!',
    //                json.message,
    //                'error'
    //            );
    //        } else {
    //            //$('#' + gridId).DataTable().draw();
    //            $('#' + gridId).DataTable().ajax.reload(null, false);
    //            $(event.target)[0].reset();
    //        }
                

    //        $('#busy-indicator-container').addClass('d-none');
    //    }

    //};
    
    let drawTable = event => {
        let gridId = $(event.currentTarget).data('grid-id');
        if ($.fn.DataTable.isDataTable('#' + gridId))
            $('#' + gridId).DataTable().draw();
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
        initializeAnnouncementGrid();
        initializeModuleGrid();
        initializeUserRoleGrid();
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