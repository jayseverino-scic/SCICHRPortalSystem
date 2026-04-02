(function ($) {
    // Events
    const CLICK_EVENT = 'click';

    // Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _stringHelper = new StringHelper();

    // State management
    let _department = [];
    let _shift = [];
    let _currentFilterType = 'All';
    let _currentDepartmentId = 0;
    let _currentShiftId = 0;
    let dataTable = null;

    // NEW: Cell dependency handler
    // UPDATED: Cell dependency handler with shift time management
    let handleCellDependencies = function (rowData, changedColumn, rowIndex) {
        const updatedColumns = [];

        console.log(`Handling dependencies for ${changedColumn} in row ${rowIndex}`);

        switch (changedColumn) {
            case 'isNoShift':
                if (rowData.isNoShift) {
                    // If "No Shift" is checked, uncheck other shift-related options AND nullify shift times
                    if (rowData.isAssigned) {
                        rowData.isAssigned = false;
                        updatedColumns.push('isAssigned');
                    }
                    if (rowData.isFlexibleShift) {
                        rowData.isFlexibleShift = false; // CORRECTED: was isFlexibleBreak
                        updatedColumns.push('isFlexibleShift');
                    }

                    // Nullify shift start and end times
                    rowData.shiftStart = null;
                    rowData.shiftEnd = null;
                    rowData.breakStart = null;
                    rowData.breakEnd = null;

                    console.log(`No Shift selected - cleared shift times for row ${rowIndex}`);
                }
                break;

            case 'isNoBreak':
                if (rowData.isNoBreak) {
                    // If "No Break" is checked, uncheck other break-related options AND nullify break times
                    if (rowData.isFlexibleBreak) {
                        rowData.isFlexibleBreak = false;
                        updatedColumns.push('isFlexibleBreak');
                    }

                    // Nullify break times only
                    rowData.breakStart = null;
                    rowData.breakEnd = null;

                    console.log(`No Break selected - cleared break times for row ${rowIndex}`);
                }
                break;

            case 'isFlexibleShift':
                if (rowData.isFlexibleShift && rowData.isNoShift) {
                    rowData.isNoShift = false;
                    updatedColumns.push('isNoShift');
                }
                break;

            case 'isFlexibleBreak':
                if (rowData.isFlexibleBreak && rowData.isNoBreak) {
                    rowData.isNoBreak = false;
                    updatedColumns.push('isNoBreak');
                }
                break;

            case 'isAssigned':
                if (rowData.isAssigned && rowData.isNoShift) {
                    rowData.isNoShift = false;
                    updatedColumns.push('isNoShift');
                }
                break;

            // Handle when shift times are manually set - this might indicate a shift is being assigned
            case 'shiftStart':
            case 'shiftEnd':
            case 'breakStart':
            case 'breakEnd':
                // If any time is set and "No Shift" is checked, uncheck "No Shift"
                if ((rowData.shiftStart || rowData.shiftEnd || rowData.breakStart || rowData.breakEnd) && rowData.isNoShift) {
                    rowData.isNoShift = false;
                    updatedColumns.push('isNoShift');
                    console.log(`Time value set - automatically unchecked No Shift for row ${rowIndex}`);
                }
                break;
        }

        console.log(`Dependencies handled. Updated columns: ${updatedColumns.join(', ')}`);
        return { updatedRowData: rowData, updatedColumns };
    };

    // Event attachment
    let attachEvents = () => {
        console.log('Attaching events...');

        // Remove existing events to prevent duplicates
        $('#assigned').off(CLICK_EVENT);
        $('#unAssigned').off(CLICK_EVENT);
        $('#all').off(CLICK_EVENT);
        $('#save').off(CLICK_EVENT);
        $('#department').off('change');
        $('#shift').off('change');

        $('#assigned').on(CLICK_EVENT, (event) => {
            event.preventDefault();
            setActiveFilterButton(event.currentTarget);
            _currentFilterType = 'Assigned';
            loadEmployeeShiftData();
        });

        $('#unAssigned').on(CLICK_EVENT, (event) => {
            event.preventDefault();
            setActiveFilterButton(event.currentTarget);
            _currentFilterType = 'Unassigned';
            loadEmployeeShiftData();
        });

        $('#all').on(CLICK_EVENT, (event) => {
            event.preventDefault();
            setActiveFilterButton(event.currentTarget);
            _currentFilterType = 'All';
            loadEmployeeShiftData();
        });

        $('#save').on(CLICK_EVENT, onEmployeeShiftSubmit);

        $('#department').on('change', () => {
            _currentDepartmentId = $('#department').val() || 0;
            loadEmployeeShiftData();
        });

        $('#shift').on('change', () => {
            _currentShiftId = $('#shift').val() || 0;
            loadEmployeeShiftData();
        });

        console.log('Events attached successfully');
    };

    let setActiveFilterButton = (activeButton) => {
        $('#assigned, #unAssigned, #all').removeClass('active').addClass('btn-outline-secondary');
        $(activeButton).removeClass('btn-outline-secondary').addClass('active btn-primary');
    };

    let loadEmployeeShiftData = async () => {
        console.log('Loading employee shift data...');
        await getEmployeeShiftData(_currentDepartmentId, _currentShiftId, _currentFilterType);
    };

    let getEmployeeShiftData = async (departmentId, shiftId, filterType) => {
        console.log('Fetching employee shift data...');

        try {
            let response = await _apiHelper.get({
                url: `Authenticated/EmployeeShift/ShiftFilter?departmentId=${departmentId}&shiftId=${shiftId}&filterType=${filterType}`,
            });

            if (response.ok) {
                let data = await response.json();
                console.log('Employee shift data loaded:', data ? data.length : 0, 'records');
                renderEmployeeShiftGrid(data || []);
            } else {
                console.error('Failed to load employee shift data:', response.status);
                renderEmployeeShiftGrid([]);
            }
        } catch (error) {
            console.error('Error loading employee shift data:', error);
            renderEmployeeShiftGrid([]);
        }
    };

    let getEmployeeShiftColumns = () => {
        return [
            {
                title: '<input name="select_all" value="1" id="select-all" type="checkbox"> Assign/Unassign',
                data: "isAssigned",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-name="isAssigned" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Employee",
                data: "employeeName",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Shift Start",
                data: "shiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Shift End',
                data: "shiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Break Start',
                data: "breakStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Break End',
                data: "breakEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="isFlexibleShift" type="checkbox"> Is Flexible Shift?',
                data: "isFlexibleShift",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="isFlexibleBreak" type="checkbox"> Is Flexible Break',
                data: "isFlexibleBreak",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleBreak" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="isNoShift" type="checkbox"> Is No Shift',
                data: "isNoShift",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoShift" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input name="select_all" value="1" class="select-all" data-column="isNoBreak" type="checkbox"> Is No Break',
                data: "isNoBreak",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoBreak" data-row="${meta.row}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Shift",
                data: "shiftName",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Department",
                data: "departmentName",
                className: 'dt-center',
                render: (data) => data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Date Assigned",
                data: "shiftDate",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data);
                }
            },
            // Hidden columns for data storage
            {
                title: "Assigned Id",
                data: "assignedShiftId",
                visible: false
            },
            {
                title: "Employee Id",
                data: "employeeId",
                visible: false
            },
            {
                title: "ShiftId",
                data: "shiftId",
                visible: false
            },
            {
                title: "Dept. Id",
                data: "departmentId",
                visible: false
            }
        ];
    };

    let onEmployeeShiftSubmit = async event => {
        event.preventDefault();

        if (!dataTable) {
            alert('Data table not initialized.');
            return;
        }

        const shiftId = $('#shift').val() || 0;
        const data = dataTable.rows().data().toArray();
        console.log(data);
        let shiftDate = data.ShiftDate ? data.DateShiftDate : null;
        const shiftStart = data.ShiftStart ? data.ShiftStart : null;
        const shiftEnd = data.ShiftEnd ? data.ShiftEnd : null;
        const breakStart = data.BreakStart ? data.BreakStart : null;
        const breakEnd = data.BreakEnd ? data.BreakEnd : null;

        data.ShiftDate = shiftDate;
        data.ShiftStart = shiftStart;
        data.ShiftEnd = shiftEnd;
        data.BreakStart = breakStart;
        data.BreakEnd = breakEnd;

        console.log('Submitting data:', data);

        if (data.length === 0) {
            alert('No data to save.');
            return;
        }

        try {
            const response = await _apiHelper.post({
                url: `Authenticated/EmployeeShift?shiftId=${shiftId}`,
                data: data
            });

            if (response.ok) {
                // Reload the grid with current filters
                await loadEmployeeShiftData();

                alert('Employee shift assignments have been updated successfully!');
            } else if (response.status === 403) {
                alert('Access denied.');
            } else if (response.status === 409) {
                const errorText = await response.text();
                alert('Conflict: ' + errorText);
            } else {
                alert('Failed to save employee shifts.');
            }
        } catch (error) {
            console.error('Error saving employee shifts:', error);
            alert('An error occurred while saving.');
        }
    };

    let renderEmployeeShiftGrid = (data) => {
        const tableId = '#employee-shift-grid';
        const $table = $(tableId);

        console.log('Rendering grid with', data.length, 'records');

        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable(tableId)) {
            dataTable.destroy();
            $table.empty();
        }

        // Create a complete table structure
        $table.html(`
            <thead>
                <tr>
                    <th>No.</th>
                    <th>Employee</th>
                    <th>Shift Start</th>
                    <th>Shift End</th>
                    <th>Break Start</th>
                    <th>Break End</th>
                    <th>Is Flexible Shift</th>
                    <th>Is Flexible Break</th>
                    <th>Is No Shift</th>
                    <th>Is No Break</th>
                    <th>Shift</th>
                    <th>Department</th>
                    <th>Date Assigned</th>
                </tr>
            </thead>
            <tbody></tbody>
        `);

        // Initialize DataTable
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
            columns: getEmployeeShiftColumns(),
            drawCallback: function () {
                attachSelectAllEvents();
            },
            initComplete: function () {
                console.log('DataTable initialized successfully');
                console.log('Columns configured:', this.api().columns().count());
                attachSelectAllEvents();
            }
        });
    };

    let renderDropDowns = () => {
        console.log('Rendering dropdowns...');

        // Simple dropdown rendering without relying on _formHelper
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
                console.log(`Rendered ${data.length} options for ${elementId}`);
            } else {
                $select.append('<option value="">No data available</option>');
                console.warn(`No data available for ${elementId}`);
            }
        };

        // Render department dropdown
        renderSimpleDropdown('#department', _department, 'departmentId', 'departmentName', 'Select Department');

        // Render shift dropdown  
        renderSimpleDropdown('#shift', _shift, 'shiftId', 'shiftName', 'Select Shift');
    };

    let getDropdownData = async () => {
        console.log('Loading dropdown data...');

        try {
            const departmentResponse = await _apiHelper.get({ url: 'Authenticated/Department' });
            const shiftResponse = await _apiHelper.get({ url: 'Authenticated/Shift' });

            if (departmentResponse.ok) {
                _department = await departmentResponse.json();
                console.log('Loaded departments:', _department.length);
            } else {
                console.error('Failed to load departments:', departmentResponse.status);
                _department = [];
            }

            if (shiftResponse.ok) {
                _shift = await shiftResponse.json();
                console.log('Loaded shifts:', _shift.length);
            } else {
                console.error('Failed to load shifts:', shiftResponse.status);
                _shift = [];
            }
        } catch (error) {
            console.error('Error loading dropdown data:', error);
            _department = [];
            _shift = [];
        }
    };

    let attachSelectAllEvents = () => {
        console.log('Attaching select all events...');

        // Use event delegation for dynamic elements
        $(document).off('change', '#select-all');
        $(document).off('change', '.select-all');
        $(document).off('change', '.row-check');
        $(document).off('change', 'input[type="time"]');

        $(document).on('change', 'input[type="time"]', function () {
            const $this = $(this);
            const rowIndex = $this.closest('tr').index();
            const fieldName = $this.attr('name') || $this.data('field');
            const newValue = $this.val();

            console.log(`Time input changed: row=${rowIndex}, field=${fieldName}, value=${newValue}`);

            if (dataTable && rowIndex !== undefined) {
                let rowData = dataTable.row(rowIndex).data();
                if (rowData) {
                    rowData[fieldName] = newValue;

                    // Handle dependencies for time changes
                    const { updatedRowData, updatedColumns } = handleCellDependencies(rowData, fieldName, rowIndex);

                    // Update the row data
                    dataTable.row(rowIndex).data(updatedRowData);

                    // Update UI for dependent columns
                    updatedColumns.forEach(col => {
                        const $dependentCheckbox = $(`.row-check[data-row="${rowIndex}"][data-column="${col}"]`);
                        if ($dependentCheckbox.length) {
                            $dependentCheckbox.prop('checked', updatedRowData[col]);
                        }

                        const $dependentCheckboxByName = $(`.row-check[data-row="${rowIndex}"][data-name="${col}"]`);
                        if ($dependentCheckboxByName.length) {
                            $dependentCheckboxByName.prop('checked', updatedRowData[col]);
                        }
                    });
                }
            }
        });

        // Main select all functionality
        $(document).on('change', '#select-all', function () {
            const isChecked = $(this).prop('checked');
            console.log('Main select-all changed:', isChecked);

            $('.row-check[data-name="isAssigned"]').prop('checked', isChecked);

            // Update data in DataTable
            if (dataTable) {
                dataTable.rows().every(function (rowIdx) {
                    const rowData = this.data();
                    rowData.isAssigned = isChecked;

                    // Handle dependencies when bulk updating
                    if (isChecked) {
                        // If assigning shifts, uncheck "No Shift"
                        if (rowData.isNoShift) {
                            rowData.isNoShift = false;
                        }
                    }
                    this.data(rowData);
                });
                // Don't redraw here to avoid recreating elements
                updateSelectAllCheckbox();
            }
        });

        // Column-specific select all functionality
        $(document).on('change', '.select-all', function () {
            const $this = $(this);
            const columnName = $this.data('column');
            const isChecked = $this.prop('checked');

            console.log(`Column select-all changed: ${columnName} = ${isChecked}`);

            // Update all checkboxes in this column
            $(`.row-check[data-column="${columnName}"]`).prop('checked', isChecked);

            // Update data in DataTable and handle dependencies
            if (dataTable) {
                dataTable.rows().every(function (rowIdx) {
                    const rowData = this.data();
                    rowData[columnName] = isChecked;

                    // Handle dependencies for bulk column updates
                    const { updatedRowData, updatedColumns } = handleCellDependencies(rowData, columnName, rowIdx);
                    this.data(updatedRowData);
                });
                // Don't redraw here to avoid recreating elements
                updateSelectAllCheckboxForColumn(columnName);
            }
        });

        // UPDATED: Individual row checkboxes with dependency handling
        $(document).on('change', '.row-check', function () {
            const $this = $(this);
            const rowIndex = $this.data('row');
            const columnName = $this.data('column') || $this.data('name');
            const isChecked = $this.prop('checked');

            console.log(`Row checkbox changed: row=${rowIndex}, column=${columnName}, checked=${isChecked}`);

            if (dataTable && rowIndex !== undefined) {
                let rowData = dataTable.row(rowIndex).data();
                if (rowData) {
                    // Update the changed column
                    rowData[columnName] = isChecked;

                    // NEW: Handle cell dependencies
                    const { updatedRowData, updatedColumns } = handleCellDependencies(rowData, columnName, rowIndex);

                    // Update the row data in DataTable
                    dataTable.row(rowIndex).data(updatedRowData);

                    // NEW: Update UI for dependent columns
                    updatedColumns.forEach(col => {
                        console.log(`Updating dependent column: ${col} for row ${rowIndex}`);

                        // Update checkboxes with data-column attribute
                        const $dependentCheckbox = $(`.row-check[data-row="${rowIndex}"][data-column="${col}"]`);
                        if ($dependentCheckbox.length) {
                            $dependentCheckbox.prop('checked', updatedRowData[col]);
                            console.log(`Updated checkbox for ${col} to ${updatedRowData[col]}`);
                        }

                        // Also update if it's using data-name attribute (for isAssigned)
                        const $dependentCheckboxByName = $(`.row-check[data-row="${rowIndex}"][data-name="${col}"]`);
                        if ($dependentCheckboxByName.length) {
                            $dependentCheckboxByName.prop('checked', updatedRowData[col]);
                            console.log(`Updated checkbox (by name) for ${col} to ${updatedRowData[col]}`);
                        }
                    });
                }
            }

            // Update the appropriate select-all checkbox
            if (columnName === 'isAssigned') {
                updateSelectAllCheckbox();
            } else {
                updateSelectAllCheckboxForColumn(columnName);
            }
        });

        // Initialize all select-all checkboxes
        setTimeout(() => {
            updateSelectAllCheckbox();
            updateSelectAllCheckboxForColumn('isFlexibleShift');
            updateSelectAllCheckboxForColumn('isFlexibleBreak');
            updateSelectAllCheckboxForColumn('isNoShift');
            updateSelectAllCheckboxForColumn('isNoBreak');
        }, 100);
    };

    let updateSelectAllCheckbox = () => {
        const $selectAll = $('#select-all');
        const $rowChecks = $('.row-check[data-name="isAssigned"]');

        console.log('Updating main select-all:', $rowChecks.length, 'checkboxes found');

        if ($rowChecks.length === 0) {
            $selectAll.prop('checked', false);
            $selectAll.prop('indeterminate', false);
            return;
        }

        const checkedCount = $rowChecks.filter(':checked').length;
        console.log('Main select-all - checked:', checkedCount, 'total:', $rowChecks.length);

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

    let updateSelectAllCheckboxForColumn = (columnName) => {
        const $selectAll = $(`.select-all[data-column="${columnName}"]`);
        const $rowChecks = $(`.row-check[data-column="${columnName}"]`);

        console.log(`Updating ${columnName} select-all:`, $rowChecks.length, 'checkboxes found');

        if ($rowChecks.length === 0) {
            $selectAll.prop('checked', false);
            $selectAll.prop('indeterminate', false);
            return;
        }

        const checkedCount = $rowChecks.filter(':checked').length;
        console.log(`${columnName} select-all - checked:`, checkedCount, 'total:', $rowChecks.length);

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
        console.log('Initializing application...');

        try {
            // Step 1: Load dropdown data
            await getDropdownData();

            // Step 2: Render dropdowns
            renderDropDowns();

            // Step 3: Set initial active button
            setActiveFilterButton($('#all')[0]);

            // Step 4: Load initial grid data
            await loadEmployeeShiftData();

            // Step 5: Attach events
            attachEvents();

            console.log('Application initialized successfully');
        } catch (error) {
            console.error('Failed to initialize application:', error);
            // Show basic error to user
            $('#employee-shift-grid').html('<div class="alert alert-danger">Failed to initialize application. Please refresh the page.</div>');
        }
    };

    // Initialize when document is ready
    $(document).ready(function () {
        console.log('Document ready - starting initialization...');
        initializeApplication();
    });

})(jQuery);