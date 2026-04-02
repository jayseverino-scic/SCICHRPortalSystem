(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load';
    const SYSTEM = 'administration';

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();

    let _employee = [];
    let _employeeShift = [];
    let pageSize = 10;
    let searchKeyword = '';
    let dataTable = null;
    let isDataTableInitialized = false; // Track initialization state

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#employee-time-log-form').on('submit', onFormSubmit);
        $('#employee-time-log-form #employeeName').on('change', onChangeEmployeeName);
        $('#employee-time-log-form #employeeNo').on('change', onChangeEmployeeNo);
        $('#end-date-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#start-date-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#end-import-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#start-import-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#filter').on(CLICK_EVENT, onClickFilter);
        $('#start-date-filter').on('change', onChangeStartFilter)
        $('#start-import-filter').on('change', onChangeStartImport)
        $('#import').on(CLICK_EVENT, onClickImport);
    };

    let onClickImport = () => {
        let startImportDate = $('#start-import-filter').val();
        let endImportDate = $('#end-date-filter').val() + 'T23:59:59' ;
        let pageNumber = 1;
        var request = _apiHelper.ajaxRequest('POST', {
            url: `Authenticated/EmployeeTimeLog/Import?pageNumber=${pageNumber}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&startImportDate=${startImportDate}&endImportDate=${endImportDate}`,
            xhr: function () {
                let xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = ((evt.loaded / evt.total) * 100);
                        $(".progress-bar").width(percentComplete + '%');
                        $(".progress-bar").html(percentComplete + '%');
                    }
                }, false);
                return xhr;
            },
            beforeSend: function () {
                $(".progress-bar").width('0%');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert('Import failed!');
            },
            success: function (response) {
                loadDataIntoGrid(response);
                alert('Import successful!');
            }
        });
    }

    let onChangeStartFilter = () => {
        let startDateVal = $('#start-date-filter').val();
        let endDateVal = $('#end-date-filter').val();
        if (new Date(startDateVal) > new Date(endDateVal))
            $('#end-date-filter').val(startDateVal);

        $('#end-date-filter').attr('min', startDateVal);
    };

    let onChangeStartImport = () => {
        let startDateVal = $('#start-import-filter').val();
        let endDateVal = $('#end-import-filter').val();
        if (new Date(startDateVal) > new Date(endDateVal))
            $('#end-import-filter').val(startDateVal);

        $('#end-import-filter').attr('min', startDateVal);
    };

    let onClickFilter = async e => {
        e.preventDefault();
        let startDate = $('#start-date-filter').val();
        let endDate = $('#end-date-filter').val();

        // Check if DataTable exists before trying to get page info
        let pageNumber = 1;

        if (isDataTableInitialized && dataTable) {
            let gridInfo = dataTable.page.info();
            pageNumber = gridInfo.page + 1;
        }

        let response = await _apiHelper.get({
            url: `Authenticated/EmployeeTimeLog/Filter?pageNumber=${pageNumber}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&startDate=${startDate}&endDate=${endDate}`,
        });

        if (response.ok) {
            let json = await response.json();
            let dataRetrieved = json.data;
            loadDataIntoGrid(dataRetrieved);
        }
    };

    let onChangeEmployeeName = function () {
        let id = $(this).val();
        var shift = _.filter(_employeeShift, function (o) { return o.employeeId == id });
        if (id) {
            var shift = _.filter(_employeeShift, function (o) { return o.employeeId == id });
            $('#employee-time-log-form #timeIn').val(moment(shift[0].shiftStart).format('HH:mm'));
            $('#employee-time-log-form #timeOut').val(moment(shift[0].shiftEnd).format('HH:mm'));
            $('#employee-time-log-form #breakOut').val(moment(shift[0].breakStart).format('HH:mm'));
            $('#employee-time-log-form #breakIn').val(moment(shift[0].breakEnd).format('HH:mm'));
            $('#employee-time-log-form #shiftStart').val(moment(shift[0].shiftStart).format('HH:mm'));
            $('#employee-time-log-form #shiftEnd').val(moment(shift[0].shiftEnd).format('HH:mm'));
            $('#employee-time-log-form #breakStart').val(moment(shift[0].breakStart).format('HH:mm'));
            $('#employee-time-log-form #breakEnd').val(moment(shift[0].breakEnd).format('HH:mm'));
            $('#employee-time-log-form #isFlexibleShift').prop("checked", shift[0].isFlexibleShift);
            $('#employee-time-log-form #isFlexibleBreak').prop("checked", shift[0].isFlexibleBreak);
            $('#employee-time-log-form #isNoShift').prop("checked", shift[0].isNoShift);
            $('#employee-time-log-form #isNoBreak').prop("checked", shift[0].isNoBreak);
            $('#employee-time-log-form #employeeNo').val(id).trigger('change.select2');
        }
    };

    let onChangeEmployeeNo = function () {
        let id = $(this).val();
        if (id) {
            var shift = _.filter(_employeeShift, function (o) { return o.employeeId == id });
            console.log(shift);
            $('#employee-time-log-form #timeIn').val(moment(shift[0].shiftStart).format('HH:mm'));
            $('#employee-time-log-form #timeOut').val(moment(shift[0].shiftEnd).format('HH:mm'));
            $('#employee-time-log-form #breakOut').val(moment(shift[0].breakStart).format('HH:mm'));
            $('#employee-time-log-form #breakIn').val(moment(shift[0].breakEnd).format('HH:mm'));
            $('#employee-time-log-form #shiftStart').val(moment(shift[0].shiftStart).format('HH:mm'));
            $('#employee-time-log-form #shiftEnd').val(moment(shift[0].shiftEnd).format('HH:mm'));
            $('#employee-time-log-form #breakStart').val(moment(shift[0].breakStart).format('HH:mm'));
            $('#employee-time-log-form #breakEnd').val(moment(shift[0].breakEnd).format('HH:mm'));
            $('#employee-time-log-form #isFlexibleShift').prop("checked", shift[0].isFlexibleShift);
            $('#employee-time-log-form #isFlexibleBreak').prop("checked", shift[0].isFlexibleBreak);
            $('#employee-time-log-form #isNoShift').prop("checked", shift[0].isNoShift);
            $('#employee-time-log-form #isNoBreak').prop("checked", shift[0].isNoBreak);
            $('#employee-time-log-form #employeeName').val(id).trigger('change.select2');
        }
    };

    let onClickAddModal = function () {
        hideShowColumnBaseOnAction(true);
        $('#employee-time-log-form')[0].reset();
        $('#employee-time-log-form').find(':submit').text('Add');
        $('#employee-time-log-modal').modal('show');
    };

    let onFormSubmit = async event => {
        event.preventDefault();

        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();

        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');

            try {
                let data = _formHelper.toJsonString(event.target);
                console.log('Form data:', data);

                const timeIn = data.DateIn + 'T' + data.TimeIn;
                const shiftStart = data.ShiftStart ? data.DateIn + 'T' + data.ShiftStart : null;
                const shiftEnd = data.ShiftEnd ? data.DateIn + 'T' + data.ShiftEnd : null;
                const breakStart = data.BreakStart ? data.DateIn + 'T' + data.BreakStart : null;
                const breakEnd = data.BreakIn ? data.DateIn + 'T' + data.BreakEnd : null;
                let timeOut = data.TimeOut ? data.DateIn + 'T' + data.TimeOut : null;
                const breakOut = data.DateBreakOut && data.BreakOut ? data.DateBreakOut + 'T' + data.BreakOut : null;
                let breakIn = data.DateBreakIn && data.BreakIn ? data.DateBreakIn + 'T' + data.BreakIn : null;

                data.TimeIn = timeIn;
                data.TimeOut = timeOut;
                data.BreakOut = breakOut;
                data.BreakIn = breakIn;
                data.ShiftStart = shiftStart;
                data.ShiftEnd = shiftEnd;
                data.BreakStart = breakStart;
                data.BreakEnd = breakEnd;
                data.employeeId = $('#employee-time-log-form #employeeNo').val();
                data.IsFlexibleShift = $("#isFlexibleShift").is(':checked');
                data.IsFlexibleBreak = $("#isFlexibleBreak").is(':checked');
                data.IsNoShift = $("#isNoShift").is(':checked');
                data.IsNoBreak = $("#isNoBreak").is(':checked');

                Object.keys(data).forEach(key => {
                    if (data[key] === null || data[key] === undefined || data[key] === '') {
                        delete data[key];
                    }
                });

                let currentTabTitle = $('.tab-pane.active .title').text();
                let response;

                if (button == 'add') {
                    response = await _apiHelper.post({
                        url: 'Authenticated/EmployeeTimeLog',
                        data: data,
                        requestOrigin: `${currentTabTitle} Tab`,
                        requesterName: $('#current-user').text(),
                        requestSystem: SYSTEM
                    });
                } else {
                    response = await _apiHelper.put({
                        url: 'Authenticated/EmployeeTimeLog',
                        data: data,
                        requestOrigin: `${currentTabTitle} Tab`,
                        requesterName: $('#current-user').text(),
                        requestSystem: SYSTEM
                    });
                }

                // Handle response properly
                if (response.ok) {
                    // Parse the response to ensure it's valid JSON
                    let responseData;
                    try {
                        responseData = await response.json();
                        console.log('API Response:', responseData);
                    } catch (jsonError) {
                        console.log('Response is not JSON, but operation was successful');
                    }

                    // Refresh the grid data by triggering the filter
                    if (isDataTableInitialized && dataTable) {
                        // Use your existing filter to refresh the data
                        $('#filter').trigger('click');
                    }

                    toastr.success('Record ' + (button == 'add' ? 'created' : 'updated') + ' successfully');
                    $(event.target)[0].reset();
                    $(event.target)[0].elements[1].focus();
                    $(event.target).find(':submit').prop("disabled", false).text('Add');
                    $('#employee-time-log-modal').modal('hide');
                }
                else if (response.status == 403) {
                    noAccessAlert();
                }
                else if (response.status === 409) {
                    let json = await response.json();
                    Swal.fire('Error!', json.message, 'error');
                }
                else {
                    let errorText = await response.text();
                    console.error('API Error:', errorText);
                    toastr.error('Operation failed: ' + errorText);
                }
            } catch (error) {
                console.error('Form submission error:', error);
                toastr.error('An error occurred while processing your request');
            } finally {
                $('#busy-indicator-container').addClass('d-none');
            }
        }
    };
    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);
        let dateIn = moment(data.dateIn).format('YYYY-MM-DD');
        let dateOut = moment(data.dateOut).format('YYYY-MM-DD');
        let dateBreakOut = moment(data.dateBreakOut).format('YYYY-MM-DD');
        let dateBreakIn = moment(data.dateBreakIn).format('YYYY-MM-DD');
        $(form).find('#dateIn').val(dateIn);
        $(form).find('#dateOut').val(dateOut);
        $(form).find('#dateBreakOut').val(dateBreakOut);
        $(form).find('#dateBreakIn').val(dateBreakIn);
        $(form).find('#isFlexibleShift').prop("checked", data.isFlexibleShift);
        $(form).find('#isFlexibleBreak').prop("checked", data.isFlexibleBreak);
        $(form).find('#isNoShift').prop("checked", data.isNoShift);
        $(form).find('#isNoBreak').prop("checked", data.isNoBreak);
        console.log(data);
        if (data.timeIn) {
            $(form).find('#timeIn').val(moment(data.timeIn).format('HH:mm'));
        }
        if (data.timeOut) {
            $(form).find('#timeOut').val(moment(data.timeOut).format('HH:mm'));
        }
        if (data.breakOut) {
            $(form).find('#breakOut').val(moment(data.breakOut).format('HH:mm'));
        }
        if (data.breakIn) {
            $(form).find('#breakIn').val(moment(data.breakIn).format('HH:mm'));
        }
        if (data.shiftStart) {
            $(form).find('#shiftStart').val(moment(data.shiftStart).format('HH:mm'));
        }
        if (data.shiftEnd) {
            $(form).find('#shiftEnd').val(moment(data.shiftEnd).format('HH:mm'));
        }
        if (data.breakStart) {
            $(form).find('#breakStart').val(moment(data.breakStart).format('HH:mm'));
        }
        if (data.breakEnd) {
            $(form).find('#breakEnd').val(moment(data.breakEnd).format('HH:mm'));
        }

        if (data.employeeId) {
            $(form).find('#employeeNo').val(data.employeeId).trigger('change.select2');
            $(form).find('#employeeName').val(data.employeeId).trigger('change.select2');
        }
    };

    let renderDropDowns = async () => {
        await getDropdownData();
        _formHelper.renderDropdown({ name: 'employee-time-log-form #employeeNo', valueName: 'employeeId', data: _employee, text: 'employeeNo', placeHolder: '-', isSelect2: true, dropdownParent: '#employee-time-log-modal' });
        _formHelper.renderDropdown({ name: 'employee-time-log-form #employeeName', valueName: 'employeeId', data: _employee, text: 'firstName', secondText: 'lastName', placeHolder: '-', isSelect2: true, dropdownParent: '#employee-time-log-modal' });
    };

    let getDropdownData = async () => {
        let [employeeResp, employeeShiftResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/Employee`
            }),
            _apiHelper.get({
                url: `Authenticated/EmployeeShift`
            }),
        ]);

        let [employee, employeeShift] = await Promise.all(
            [
                employeeResp.json(),
                employeeShiftResp.json(),
            ]
        );
        _employee = employee;
        _employeeShift = employeeShift;
    };

    let destroyDataTable = () => {
        if (isDataTableInitialized && dataTable && $.fn.DataTable.isDataTable('#employee-time-log-grid')) {
            dataTable.destroy(true);
            $('#employee-time-log-grid').empty();
            isDataTableInitialized = false;
            dataTable = null;
        }
    };

    let loadDataIntoGrid = (gridData) => {
        var data = [];
        try {
            if (gridData && Array.isArray(gridData)) {
                data = gridData;
            } else if (gridData && gridData.data && Array.isArray(gridData.data)) {
                data = gridData.data;
            }
        } catch (e) {
            console.error('Error processing grid data:', e);
            data = [];
        }

        // If DataTable is not initialized, initialize it first
        if (!isDataTableInitialized) {
            initializeGrid(data);
            return;
        }

        // If already initialized, use the existing DataTable
        try {
            dataTable.clear();
            dataTable.rows.add(data);
            dataTable.draw();
        } catch (error) {
            console.error('Error updating DataTable data:', error);
            // If update fails, reinitialize
            destroyDataTable();
            initializeGrid(data);
        }
    };

    let initializeGrid = async (initialData = []) => {
        // Destroy existing DataTable if it exists
        destroyDataTable();

        let columns = await getColumns();
        try {
            dataTable = $('#employee-time-log-grid').DataTable({
                bLengthChange: true,
                lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
                bFilter: true,
                bInfo: true,
                serverSide: false, // Changed to false since you're loading data directly
                bSort: true,
                scrollY: "350px",
                scrollX: true,
                order: [[1, 'asc']],
                data: initialData,
                columns: columns,
                pageLength: 5,
                dom: '<"pull-left">lBf<"pull-right">tipr',
                language: {
                    emptyTable: "No employee time log data available",
                    zeroRecords: "No matching records found"
                }
            });

            isDataTableInitialized = true;

            // Attach event handlers after DataTable is initialized
            attachTableEvents();

        } catch (error) {
            console.error('DataTables initialization error:', error);
            createFallbackTable(initialData);
        }
    };

    let attachTableEvents = () => {
        $('#employee-time-log-grid tbody').on('click', '.icon-edit', function () {
            var rowData = dataTable.row($(this).closest('tr')).data();
            let form = $('#employee-time-log-form');
            populateForm(form, rowData);
            $('#employee-time-log-modal').modal('show');
        });

        $('#employee-time-log-grid tbody').on('click', '.return-btn', function () {
            var rowData = dataTable.row($(this).closest('tr')).data();
            let form = $('#employee-time-log-form');
            let dateIn = moment(rowData.dateIn).format('YYYY-MM-DD');
            let dateOut = moment(rowData.dateOut).format('YYYY-MM-DD');
            let dateBreakOut = moment(rowData.dateBreakOut).format('YYYY-MM-DD');
            let dateBreakIn = moment(rowData.dateBreakIn).format('YYYY-MM-DD');
            form.find('#dateIn').val(dateIn);
            form.find('#dateOut').val(dateOut);
            form.find('#dateBreakOut').val(dateBreakOut);
            form.find('#dateBreakIn').val(dateBreakIn);

            populateForm(form, rowData);
            hideShowColumnBaseOnAction(false);
            $('#employee-time-log-modal').modal('show');
        });

        $('#employee-time-log-grid tbody').on('click', '.icon-delete', function (e) {
            var rowData = dataTable.row($(this).closest('tr')).data();
            _formHelper.deleteRecord(e, rowData.timeLogId + ' log', SYSTEM);
        });
    };

    let createFallbackTable = (data) => {
        $('#employee-time-log-grid').html('<table class="table table-striped"><thead><tr><th>Error loading table. Please refresh the page.</th></tr></thead></table>');
    };

    let hideShowColumnBaseOnAction = function (isAdd) {
        if (isAdd) {
            $('.borrow-initial').prop('disabled', false);
            $('#shiftStart, #shiftEnd, #breakStart, #breakEnd').prop('disabled', true);
            $('.return-container').addClass('d-none');
        } else {
            $('.borrow-initial').prop('disabled', true);
            $('.return-container').removeClass('d-none');
        }
    };

    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "timeLogId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Employee No',
                data: "employeeNo",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Employee Name',
                data: "employeeName",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Date In",
                data: "dateIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Time In",
                data: "timeIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Date Out",
                data: "dateOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Time Out",
                data: "timeOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Date Break Out",
                data: "dateBreakOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Break Out",
                data: "breakOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Date Break In",
                data: "dateBreakIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Break In",
                data: "breakIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Shift Start",
                data: "shiftStart",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Shift End",
                data: "shiftEnd",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Break Start",
                data: "breakStart",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Break End",
                data: "breakEnd",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Flexible Shift",
                data: "isFlexibleShift",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "Flexible Break",
                data: "isFlexibleBreak",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "No Shift",
                data: "isNoShift",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "No Break",
                data: "isNoBreak",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "Actions",
                data: "timeLogId",
                width: "10em",
                render: function (data, type, full) {
                    let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.timeLogId + '" data-endpoint="Authenticated/EmployeeTimeLog" data-table="employee-time-log-grid"><i class="fas fa-edit"></i></a>';
                    buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.timeLogId + '" data-endpoint="Authenticated/EmployeeTimeLog" data-table="employee-time-log-grid"><i class="fas fa-trash border-icon"></i></a>';
                    return type === 'display' ? buttons : "";
                },
                className: 'noVis dt-center',
                orderable: false
            }
        ];
        return columns;
    };

    let initializeModals = e => {
        $('#employee-time-log-modal').modal({ backdrop: 'static', keyboard: false });
        $('#employee-time-log-form #dateIn').attr('value', moment().format('YYYY-MM-DD'));
        $('#employee-time-log-form #dateOut').attr('value', moment().format('YYYY-MM-DD'));
        $('#employee-time-log-form #dateBreakOut').attr('value', moment().format('YYYY-MM-DD'));
        $('#employee-time-log-form #dateBreakIn').attr('value', moment().format('YYYY-MM-DD'));
    };

    let initializeGrids = e => {
        initializeGrid([]);
        const button = document.getElementById("filter");
        if (button) {
            button.click();
        }
    };

    $(document).ready(function () {
        if ($('#employee-time-log-grid').length === 0) {
            console.error('Table element not found:', 'employee-time-log-grid');
            return;
        }
        renderDropDowns();
        initializeGrids();
        attachEvents();
        initializeModals();
    });

})(jQuery);