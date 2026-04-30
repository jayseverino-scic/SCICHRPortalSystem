(function ($) {
    const CLICK_EVENT = 'click';

    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _stringHelper = new StringHelper();

    let _department = [];
    let _currentDepartmentId = 0;
    let _attendanceDate = new Date();
    let dataTable = null;

    let attachEvents = () => {
        $('#compute').off(CLICK_EVENT);
        $('#filter').off(CLICK_EVENT);
        $('#inputdate').attr('value', moment().format('yyyy-MM-DD'));
        $('#compute').on(CLICK_EVENT, onEmployeeAttendanceSubmit);
        $('#filter').on(CLICK_EVENT, loadEmployeeAttendanceData);
    };

    let loadEmployeeAttendanceData = async () => {
        _currentDepartmentId = $('#department').val() || 0;
        _attendanceDate = $('#inputdate').val();
        await getEmployeeAttendanceData(_currentDepartmentId, _attendanceDate);
    };

    let getEmployeeAttendanceData = async (departmentId, attendanceDate) => {
        var queryDate = _dateHelper.formatShortLocalDate(attendanceDate);
        try {
            let response = await _apiHelper.get({
                url: `Authenticated/EmployeeAttendance/AttendanceFilter?departmentId=${departmentId}&attendanceDate=${queryDate}`,
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
                title: "Employee",
                data: "employeeName",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Time Log Id",
                data: "timeLogId",
                className: 'dt-center',
                visible: false

            },
            {
                title: "Time In",
                data: "timeIn",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Time Out",
                data: "timeOut",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Break Out",
                data: "breakOut",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Break In",
                data: "breakIn",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Shift Start",
                data: "shiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: 'Shift End',
                data: "shiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: 'Break Start',
                data: "breakStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: 'Break End',
                data: "breakEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
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
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleBreak" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "No Shift",
                data: "isNoShift",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "No Break",
                data: "isNoBreak",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoBreak" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },
            {
                title: "Shift Hours",
                data: "shiftHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Regular Hours",
                data: "regularHour",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Total Worked Hours",
                data: "totalLoggedHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Late",
                data: "shiftLate",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Undertime",
                data: "shiftUndertime",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Break Undertime",
                data: "breakUndertime",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Break Late",
                data: "breakLate",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedOT" type="checkbox"> Overtime',
                data: "approvedOT",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedOT" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Overtime Hours",
                data: "otHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Night Differential Hours",
                data: "ndHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedHoliday" type="checkbox"> Holiday',
                data: "approvedHoliday",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedHoliday" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedHolidayOT" type="checkbox"> Holiday Overtime',
                data: "approvedHolidayOT",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedHolidayOT" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedSPHoliday" type="checkbox"> Special Holiday',
                data: "approvedSPHoliday",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedSPHoliday" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedSPHolidayOT" type="checkbox"> Special Holiday Overtime',
                data: "approvedSPHolidayOT",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedSPHolidayOT" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedRestDay" type="checkbox"> Rest Day',
                data: "approvedRestDay",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedRestDay" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="approvedRestDayOT" type="checkbox"> Rest Day Overtime',
                data: "approvedRestDayOT",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="approvedRestDayOT" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Department",
                data: "department",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-',
                orderable: true
            },
            {
                title: "Employee Attendance Id",
                data: "employeeAttendanceId",
                visible: false
            },
            {
                title: "Employee Id",
                data: "employeeId",
                visible: false
            },
        ];
    };

    let onEmployeeAttendanceSubmit = async event => {
        event.preventDefault();

        if (!dataTable) {
            alert('Data table not initialized.');
            return;
        }

        const data = dataTable.rows().data().toArray();

        if (data.length === 0) {
            alert('No data to save.');
            return;
        }

        try {
            const response = await _apiHelper.post({
                url: `Authenticated/EmployeeAttendance/Compute`,
                data: data
            });

            if (response.ok) {
                await loadEmployeeAttendanceData();
                alert('Employee attendance have been computed successfully!');
            } else if (response.status === 403) {
                alert('Access denied.');
            } else if (response.status === 409) {
                const errorText = await response.text();
                alert('Conflict: ' + errorText);
            } else {
                alert('Failed to compute employee attendance.');
            }
        } catch (error) {
            alert('An error occurred while saving.');
        }
    };

    let renderEmployeeAttendanceGrid = (data) => {
        const tableId = '#employee-attendance-grid';
        const $table = $(tableId);

        if ($.fn.DataTable.isDataTable(tableId)) {
            dataTable.destroy();
            $table.empty();
        }
        $table.html(`
            <thead>
                <tr>
                    <th>Employee</th>
                    <th>Timelog Id</th>
                    <th>Time In</th>
                    <th>Time Out</th>
                    <th>Break Out</th>
                    <th>Break In</th>
                    <th>Shift Start</th>
                    <th>Shift End</th>
                    <th>Break Start</th>
                    <th>Break End</th>
                    <th>Flexible Shift</th>
                    <th>Flexible Break</th>
                    <th>No Shift</th>
                    <th>No Break</th>
                    <th>Shift Hours</th>
                    <th>Regular Hours</th>
                    <th>Total Logged Hours</th>
                    <th>Shift Late</th>
                    <th>Shift UT</th>
                    <th>Break UT</th>
                    <th>Break Late </th>
                    <th>Approved OT</th>
                    <th>OT Hours</th>
                    <th>ND Hours</th>
                    <th>Approved Holiday</th>
                    <th>Approved Holiday OT</th>
                    <th>Approved Special Holiday</th>
                    <th>Approved Special Holiday OT</th>
                    <th>Approved Rest Day</th>
                    <th>Approved Rest Day OT</th>
                    <th>Department</th>
                </tr>
            </thead>
            <tbody></tbody>
        `);

        dataTable = $table.DataTable({
            dom: '<"top">rt<"bottom"ip><"clear">',
            paging: true,
            pageLength: 10,
            searching: true,
            info: true,
            autoWidth: false,
            lengthChange: false,
            ordering: true,
            data: data,
            columns: getEmployeeAttendanceColumns(),
            drawCallback: function () {
                attachSelectAllEvents();
            },
            initComplete: function () {
                attachSelectAllEvents();
            }
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
        renderSimpleDropdown('#department', _department, 'departmentId', 'departmentName', 'Select Department');
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
    };

    let attachSelectAllEvents = () => {
        // Remove existing event handlers
        $('.select-all').off('change');
        $('.row-check').off('change');

        // Add event handler for each "select all" checkbox
        $('.select-all').on('change', function () {
            const isChecked = $(this).prop('checked');
            const columnName = $(this).data('column');

            // Only check checkboxes in the same column
            $(`.row-check[data-column="${columnName}"]`).prop('checked', isChecked);

            // Update the data in DataTable
            if (dataTable) {
                dataTable.rows().every(function (rowIdx) {
                    const rowData = this.data();
                    if (rowData) {
                        rowData[columnName] = isChecked;
                        this.data(rowData);
                    }
                });
                dataTable.draw(false);
            }
        });

        // Add event handler for individual row checkboxes
        $('.row-check').on('change', function () {
            const rowIndex = $(this).data('row');
            const columnName = $(this).data('column');
            const isChecked = $(this).prop('checked');

            // Update the data in DataTable
            if (dataTable && rowIndex !== undefined) {
                const rowData = dataTable.row(rowIndex).data();
                if (rowData) {
                    rowData[columnName] = isChecked;
                    dataTable.row(rowIndex).data(rowData);
                }
            }

            // Update the "select all" checkbox for this column
            updateSelectAllCheckbox(columnName);
        });

        // Initialize all "select all" checkboxes
        $('.select-all').each(function () {
            const columnName = $(this).data('column');
            updateSelectAllCheckbox(columnName);
        });
    };

    let updateSelectAllCheckbox = (columnName) => {
        const $selectAll = $(`.select-all[data-column="${columnName}"]`);
        const $rowChecks = $(`.row-check[data-column="${columnName}"]`);

        if ($rowChecks.length === 0) {
            $selectAll.prop('checked', false);
            $selectAll.prop('indeterminate', false);
            return;
        }

        const checkedCount = $rowChecks.filter(':checked').length;

        if (checkedCount === 0) {
            $selectAll.prop('checked', false);
            $selectAll.prop('indeterminate', false);
        } else if (checkedCount === $rowChecks.length) {
            $selectAll.prop('checked', true);
            $selectAll.prop('indeterminate', false);
        } else {
            $selectAll.prop('checked', false);
            $selectAll.prop('indeterminate', true);
        }
    };

    let initializeApplication = async () => {
        try {
            await getDropdownData();
            renderDropDowns();

            attachEvents();
            //await loadEmployeeAttendanceData();
        } catch (error) {
            $('#employee-attendance-grid').html('<div class="alert alert-danger">Failed to initialize application. Please refresh the page.</div>');
        }
    };

    $(document).ready(function () {
        initializeApplication();
        loadEmployeeAttendanceData();
    });

})(jQuery);