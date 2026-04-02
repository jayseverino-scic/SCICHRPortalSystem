(function ($) {
    const CLICK_EVENT = 'click';

    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _stringHelper = new StringHelper();

    let _department = [];
    let _cutOff = [];
    let _currentDepartmentId = 0;
    let _currentCutOffId = 0;
    let dataTable = null;

    let attachEvents = () => {
        $('#filter').off(CLICK_EVENT);
        $('#filter').on(CLICK_EVENT, loadEmployeeAttendanceData);
    };

    let loadEmployeeAttendanceData = async () => {
        _currentDepartmentId = $('#department').val() || 0;
        _currentCutOffId = $('#cutOff').val() || 0;
        await getEmployeeAttendanceData(_currentDepartmentId, _currentCutOffId);
    };

    let getEmployeeAttendanceData = async (departmentId, cutOffId) => {
        try {
            let response = await _apiHelper.get({
                url: `Authenticated/EmployeeAttendance/AttendanceCutOffFilter?departmentId=${departmentId}&cutOffId=${cutOffId}`,
            });

            if (response.ok) {
                let data = await response.json();
                renderEmployeeAttendanceGrid(data || []);
            } else {
                renderEmployeeAttendanceGrid([]);
            }
        } catch (error) {
            renderEmployeeAttendanceGrid([]);
        }
    };

    let getEmployeeAttendanceColumns = () => {
        return [
            {
                title: "Employee No.",
                data: "employeeNo",
                className: 'dt-center',
            },
            {
                title: "Employee",
                data: "employeeName",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Shift Total Hours",
                data: "shiftTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Regular Total Hours",
                data: "regularTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Total Logged Hours",
                data: "totalLoggedHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Shift Late Total Minutes",
                data: "shiftLateTotalMinutes",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Shift Undertime Total Minutes",
                data: "shiftUndertimeTotalMinutes",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Break Undertime Total Minutes",
                data: "breakUndertimeTotalMinutes",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Break Late Total Minutes",
                data: "breakLateTotalMinutes",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Overtime Total Hours",
                data: "overtimeTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Night Differential Total Hours",
                data: "nightDifferentialTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Holiday Total Hours",
                data: "holidayTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Holiday Overtime Total Hours",
                data: "holidayOvertimeTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Holiday Night Differential Total Hours",
                data: "holidayNightDifferentialTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Special Holiday Total Hours",
                data: "specialHolidayTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Special Holiday Overtime Total Hours",
                data: "specialHolidayOvertimeTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Special Holiday Night Differential Total Hours",
                data: "specialHolidayNightDifferentialTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Rest Day Total Hours",
                data: "restDayTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Rest Day Overtime Total Hours",
                data: "restDayOvertimeTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
            {
                title: "Rest Day Night Differential Total Hours",
                data: "restDayNightDifferentialTotalHours",
                className: 'dt-right',
                render: (data) => data ? _numberHelper.formatCommaSeperator(data) : 0,
                orderable: false
            },
        ];
    };

    let renderEmployeeAttendanceGrid = (data) => {
        const tableId = '#employee-attendance-grid';
        const $table = $(tableId);

        if ($.fn.DataTable.isDataTable(tableId)) {
            dataTable.destroy();
            $table.empty();
        }

        dataTable = $table.DataTable({
            //dom: '<"top">rt<"bottom"ip><"clear">',
            paging: true,
            pageLength: 10,
            searching: true,
            info: true,
            autoWidth: false,
            lengthChange: false,
            //ordering: true,
            data: data,
            columns: getEmployeeAttendanceColumns()
        });
    };

    let renderDropDowns = () => {
        const renderSimpleDropdown = (elementId, data, valueField, textField, placeholder) => {
            const $select = $(elementId);
            $select.empty();

            if (placeholder) {
                $select.append(`<option value="">${placeholder}</option>`);
            }

            if (data && data.length > 0) {
                data.forEach(item => {
                    $select.append(`<option value="${item[valueField]}">${item[textField]}</option>`);
                });
            } else {
                $select.append('<option value="">No data available</option>');
            }
        };
        _cutOff.forEach(item => {
            item.combinedDate = `${moment(item.startDate).format('MM/DD/yyyy')} - ${moment(item.endDate).format('MM/DD/yyyy') }`;
        });
        renderSimpleDropdown('#department', _department, 'departmentId', 'departmentName', 'Select Department');
        renderSimpleDropdown('#cutOff', _cutOff, 'cutOffId', 'combinedDate', 'Select Cut-Off Period');
    };

    let getDropdownData = async () => {
        try {
            const departmentResponse = await _apiHelper.get({ url: 'Authenticated/Department' });

            if (departmentResponse.ok) {
                _department = await departmentResponse.json();
            } else {
                _department = [];
            }

        } catch (error) {
            _department = [];
        }
        try {
            const cutOffResponse = await _apiHelper.get({ url: 'Authenticated/CutOff' });

            if (cutOffResponse.ok) {
                _cutOff = await cutOffResponse.json();
            } else {
                _cutOff= [];
            }

        } catch (error) {
            _cutOff = [];
        }
    };

    let initializeApplication = async () => {
        try {
            await getDropdownData();
            renderDropDowns();

            attachEvents();
            await loadEmployeeAttendanceData();
        } catch (error) {
            $('#employee-attendance-grid').html('<div class="alert alert-danger">Failed to initialize application. Please refresh the page.</div>');
        }
    };

    $(document).ready(function () {
        initializeApplication();
    });

})(jQuery);