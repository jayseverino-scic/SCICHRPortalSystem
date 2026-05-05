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
    const _daysOfWeekLookup = {
        '1': 'Monday',
        '2': 'Tuesday',
        '3': 'Wednesday',
        '4': 'Thursday',
        '5': 'Friday',
        '6': 'Saturday',
        '7': 'Sunday'
    };
    let getDayNames = function (days) {
        return _.map(normalizeDays(days), function (day) {
            return _daysOfWeekLookup[day] || day;
        });
    }
    let normalizeDays = function (days) {
        if (!days) {
            return [];
        }

        let normalizedDays = _.chain($.isArray(days) ? days : [days])
            .map(function (day) {
                return day != null ? day.toString().split(/[;,]/) : [];
            })
            .flatten()
            .value();

        return _.chain(normalizedDays)
            .map(function (day) {
                return day != null ? day.toString().trim() : '';
            })
            .filter(function (day) {
                return day !== '';
            })
            .uniq()
            .value();
    }
    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#shift-form').on('submit', onFormSubmit);
        $('#shift-form #RestDays').val(null).trigger('change');
    };
    let daysofWeekSelect2 = function (isMultiple) {
        $('#shift-form #RestDays').select2({
            multiple: isMultiple,
            theme: "bootstrap",
            width: 'element',
            width: 'resolve',
            //closeOnSelect: !params.autoClose,
        });
    }
    let onClickAddModal = function () {
        $('#shift-form')[0].reset();
        $('#shift-form').find(':submit').text('Add');
        $('#shift-modal').modal('show');
    }

    let onFormSubmit = async event => {
        event.preventDefault();
        let selectedAssignmentDays = $('#RestDays').val() || [];
        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';
            let daysOfWeek = selectedAssignmentDays;
            let data = _formHelper.toJsonString(event.target);
            let currentTabTitle = $('.tab-pane.active .title').text();
            let shiftStartFormat = moment().format('YYYY-MM-DD') + 'T' + data.ShiftStart;
            let shiftEndFormat = moment().format('YYYY-MM-DD') + 'T' + data.ShiftEnd;
            const breakStartFormat = data.BreakStart ? moment().format('YYYY-MM-DD') + 'T' + data.BreakStart : null;
            const breakEndFormat = data.BreakEnd ? moment().format('YYYY-MM-DD') + 'T' + data.BreakEnd : null;
            data.ShiftStart = shiftStartFormat;
            data.ShiftEnd = shiftEndFormat;
            data.BreakStart = breakStartFormat;
            data.BreakEnd = breakEndFormat;
            data.RestDays = daysOfWeek.join(';');
            data.DepartmentId = data.DepartmentId ? data.DepartmentId : null;

            data.isApproved = true;

            Object.keys(data).forEach(key => {
                if (data[key] === null || data[key] === undefined || data[key] === '') {
                    delete data[key];
                }
            });
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/Shift',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/Shift',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#shift-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#shift-modal').modal('hide');
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

    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);
        if (data.shiftStart) {
            $(form).find('#ShiftStart').val(moment(data.shiftStart).format('HH:mm'));
        }
        if (data.shiftEnd) {
            $(form).find('#ShiftEnd').val(moment(data.shiftEnd).format('HH:mm'));
        }
        if (data.breakStart) {
            $(form).find('#BreakStart').val(moment(data.breakStart).format('HH:mm'));
        }
        if (data.breakEnd) {
            $(form).find('#BreakEnd').val(moment(data.breakEnd).format('HH:mm'));
        }

    }

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#shift-grid').DataTable({
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
            ajax: async function (params, success, settings) {
                let gridInfo = $('#shift-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Shift/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('#shift-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        data.restDays = normalizeDays(data.restDays);
                        daysofWeekSelect2(true);
                        let form = $('#shift-form');
                        populateForm(form, data);
                        $('#shift-modal').modal('show');
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, data.shiftName, SYSTEM);
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

    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "shiftId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: "Shift Name",
                data: "shiftName",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
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
                title: "Shift Late Minute Grace Period",
                data: "shiftLateMinuteGracePeriod",
                className: 'noVis dt-center'
            },
            {
                title: "Break Late Minute Grace Period",
                data: "breakLateMinuteGracePeriod",
                className: 'noVis dt-center'
            },
            {
                title: "Shift Late Total Minute Limit",
                data: "shiftLateTotalMinuteLimit",
                className: 'noVis dt-center'
            },
            {
                title: "Break Late Total Minute Limit",
                data: "breakLateTotalMinuteLimit",
                className: 'noVis dt-center'
            },
            {
                title: "No Timelogs Count Limit",
                data: "noTimeLogCountLimit",
                className: 'noVis dt-center'
            },
            {
                title: "No Leave Count Limit",
                data: "noLeaveAbsentCountLimit",
                className: 'noVis dt-center'
            },
            {
                title: "Rest Day(s)",
                data: "restDays",
                className: 'noVis dt-center'
            }
        ];
        let lastColumn = {
            data: "shiftId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.shiftId + '" data-endpoint="Authenticated/Shift" data-table="shift-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.shiftId + '" data-endpoint="Authenticated/Shift" data-table="shift-grid"><i class="fas fa-trash border-icon"></i></a>';

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

    let initializeModals = e => {
        $('#enrollment-modal').modal({ backdrop: 'static', keyboard: false });
        $('#reasonModal').modal({ backdrop: 'static', keyboard: false });
        $('#viewApprovalRegister').modal({ backdrop: 'static', keyboard: false });
    }

    let initializeGrids = e => {
        initializeGrid();
    }


    $(document).ready(function () {
        initializeGrids();
        attachEvents();
        initializeModals();
    });

})(jQuery);