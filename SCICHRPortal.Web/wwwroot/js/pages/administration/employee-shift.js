(function ($) {
    const SYSTEM = 'administration';
    const _apiHelper = new ApiHelper();
    const _dateHelper = new DateHelper();
    const _stringHelper = new StringHelper();
    const _pendingChanges = new Map();
    const _pendingFields = new Map();
    const _originalRows = new Map();
    const _bulkFlags = new Map();
    const _weekdays = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'];
    const _editableFields = ['isSelected', 'isAssigned', 'isFlexibleShift', 'isNoShift', 'isNoBreak', 'shiftId',
        ..._weekdays.flatMap(day => [`${day}ShiftStart`, `${day}ShiftEnd`])];
    let _projects = [];
    let _shift = [];
    let _requestVersion = 0;
    let _searchTimer = null;
    let _isSaving = false;
    let _isExporting = false;
    let _refreshAfterSave = false;
    let _bulkAssignment = null;
    let _activePageFilter = null;
    let _filteredCount = 0;
    let _savedFlagSummary = {};
    let _pageLoading = false;
    let dataTable = null;

    const updatePendingStatus = () => {
        const count = _pendingChanges.size;
        const operation = Number($('#assignment-shift').val()) === -1 ? 'Unassign' : 'Assign';
        const assignment = _bulkAssignment
            ? `${operation} all employees matching ${_bulkAssignment.description} (${_bulkAssignment.count} at selection), with ${count} individual edit(s). Uncheck Select all to cancel.`
            : '';
        const flags = Array.from(_bulkFlags, ([field, selection]) =>
            `${selection.value === false ? 'Turn off ' : ''}${field === 'isFlexibleShift' ? 'Flexible Shift' : field === 'isNoShift' ? 'No Shift' : 'No Break'} for all matching ${selection.description} (${selection.count} at selection)`);
        const pendingMessage = [assignment, ...flags].filter(Boolean).join(' · ') ||
            (count ? `${count} employee(s) with pending changes` : '');
        $('#employee-shift-pending').text(pendingMessage).prop('hidden', !pendingMessage);
        $('#save').prop('disabled', _isSaving || (!_bulkAssignment && !_bulkFlags.size && count === 0));
        $('#cancel-pending').prop('disabled', _isSaving || _refreshAfterSave || (!_bulkAssignment && !_bulkFlags.size && count === 0));
    };

    const matchesFilter = (row, filter) => {
        const words = filter.searchKeyword.toLowerCase().split(/\s+/).filter(Boolean);
        const fields = [row.employeeNo, row.employeeName, row.projectName, row.shiftName].map(value => (value || '').toLowerCase());
        return (!filter.projectId || row.projectId === filter.projectId) &&
            (!filter.shiftId || row.shiftId === filter.shiftId) &&
            (filter.filterType === 'All' || row.isAssigned === (filter.filterType === 'Assigned')) &&
            words.every(word => fields.some(field => field.includes(word)));
    };

    const matchesBulkFilter = row => !!_bulkAssignment && matchesFilter(row, _bulkAssignment.filter);

    const assignmentBaseline = row => {
        const result = matchesBulkFilter(row)
            ? { ...row, isSelected: true, isAssigned: true, isNoShift: false }
            : { ...row, isSelected: false };
        for (const [field, selection] of _bulkFlags) {
            if (!matchesFilter(row, selection.filter)) continue;
            result[field] = selection.value !== false;
            if (field === 'isFlexibleShift' && result[field]) result.isNoShift = false;
            if (field === 'isNoShift' && result[field]) {
                result.isFlexibleShift = false;
                for (const day of _weekdays) result[`${day}ShiftStart`] = result[`${day}ShiftEnd`] = null;
            }
        }
        return result;
    };

    const overlayEditedFields = (row, pending, fields, original) => {
        for (const field of fields) row[field] = pending[field];
        // An explicit flag edit includes its mutually exclusive counterpart,
        // even when that counterpart was already false at the time of editing.
        if (fields.has('isNoShift') && pending.isNoShift) {
            row.isNoShift = true;
            row.isFlexibleShift = false;
        } else if (fields.has('isFlexibleShift') && pending.isFlexibleShift) {
            row.isFlexibleShift = true;
            row.isNoShift = false;
        }
        if (row.isNoShift) row.isFlexibleShift = false;
        for (const day of _weekdays) {
            for (const field of [`${day}ShiftStart`, `${day}ShiftEnd`]) {
                if (row.isNoShift) row[field] = null;
                else if (!fields.has(field)) row[field] = original[field];
            }
        }
        return row;
    };

    const overlayPageRows = rows => rows.map(serverRow => {
        const id = serverRow.employeeId;
        const pending = _pendingChanges.get(id);
        _originalRows.set(id, { ...serverRow });
        if (!pending) {
            return assignmentBaseline(serverRow);
        }
        const row = assignmentBaseline(serverRow);
        overlayEditedFields(row, pending, _pendingFields.get(id) || new Set(), serverRow);
        _pendingChanges.set(id, row);
        return row;
    });

    const applyRowChange = (row, column, value) => {
        if (_isSaving || _refreshAfterSave) return;
        const previous = { ...row };
        const originalRow = _originalRows.get(row.employeeId);
        const original = originalRow ? assignmentBaseline(originalRow) : null;
        row[column] = value;
        if (column === 'isSelected') {
            // Selection is independent of the employee's persisted assignment.
            row.isAssigned = value || (!matchesBulkFilter(originalRow || row) && !!originalRow?.isAssigned);
        }
        if (column === 'isNoShift' && value) {
            row.isFlexibleShift = false;
            for (const day of _weekdays) {
                row[`${day}ShiftStart`] = null;
                row[`${day}ShiftEnd`] = null;
            }
        } else if (column === 'isFlexibleShift' && value) {
            row.isNoShift = false;
        }
        if (column === 'isSelected' || column === 'isAssigned') {
            const target = Number($('#assignment-shift').val()) || 0;
            row.shiftId = value && target === -1 ? 0 : value && target > 0 ? target : original?.shiftId || 0;
        }
        const fields = new Set(_pendingFields.get(row.employeeId) || []);
        for (const field of _editableFields) {
            if (field !== column && (row[field] ?? null) === (previous[field] ?? null)) continue;
            if (original && (row[field] ?? null) === (original[field] ?? null)) fields.delete(field);
            else fields.add(field);
        }
        if (original) Object.assign(row, overlayEditedFields({ ...original }, row, fields, originalRow));
        if (original && fields.size) {
            _pendingChanges.set(row.employeeId, { ...row });
            _pendingFields.set(row.employeeId, fields);
        } else {
            _pendingChanges.delete(row.employeeId);
            _pendingFields.delete(row.employeeId);
        }
        updatePendingStatus();
    };

    const requestGridPage = async (request, callback) => {
        const version = ++_requestVersion;
        const empty = { draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] };
        const parameters = new URLSearchParams({
            projectId: Number($('#project').val()) || 0,
            shiftId: Number($('#shift').val()) || 0,
            filterType: $('#status').val() || 'All',
            skip: request.start,
            take: request.length,
            searchKeyword: (request.search.value || '').trim()
        });
        _pageLoading = true;
        if (dataTable) updateHeaderCheckboxes();
        $('#employee-shift-page-status').text('Loading employees...');
        try {
            const response = await _apiHelper.get({ url: `Authenticated/EmployeeShift/ShiftFilter?${parameters}` });
            if (version !== _requestVersion) return;
            if (!response.ok) throw new Error('Page unavailable');
            const result = await response.json();
            if (version !== _requestVersion) return;
            if (!result || !Array.isArray(result.data) || result.data.length > request.length ||
                !Number.isInteger(result.total) || !Number.isInteger(result.filteredTotal) ||
                result.filteredTotal < 0 || result.total < result.filteredTotal) throw new Error('Invalid page');
            if (request.start > 0 && request.start >= result.filteredTotal) {
                dataTable.page(Math.max(0, Math.ceil(result.filteredTotal / request.length) - 1)).draw('page');
                return;
            }
            _refreshAfterSave = false;
            _pageLoading = false;
            _activePageFilter = {
                projectId: Number(parameters.get('projectId')), shiftId: Number(parameters.get('shiftId')),
                filterType: parameters.get('filterType'), searchKeyword: parameters.get('searchKeyword')
            };
            _filteredCount = result.filteredTotal;
            _savedFlagSummary = result.filteredTotal > 0 ? {
                isFlexibleShift: result.allFlexibleShiftSelected === true,
                isNoShift: result.allNoShiftSelected === true,
                isNoBreak: result.allNoBreakSelected === true
            } : {};
            callback({ draw: request.draw, recordsTotal: result.total, recordsFiltered: result.filteredTotal,
                data: overlayPageRows(result.data) });
            $('#employee-shift-page-status').text('');
        } catch (_) {
            if (version !== _requestVersion) return;
            _refreshAfterSave = false;
            _pageLoading = false;
            _activePageFilter = null;
            _filteredCount = 0;
            _savedFlagSummary = {};
            callback(empty);
            $('#employee-shift-page-status').text('Could not load employees. Change a filter or refresh to try again.');
        }
    };

    const updateHeaderCheckboxes = () => {
        const rows = dataTable.rows({ page: 'current' }).data().toArray();
        for (const field of ['isAssigned', 'isFlexibleShift', 'isNoShift', 'isNoBreak']) {
            const selector = field === 'isAssigned' ? '#select-all' : `.flag-select-all[data-column="${field}"]`;
            if (field === 'isAssigned') {
                $(selector).prop('checked', !!_bulkAssignment).prop('indeterminate', false)
                    .prop('disabled', _isSaving || _refreshAfterSave || _pageLoading || (!_bulkAssignment && !_filteredCount));
                continue;
            }
            let checked = _bulkFlags.has(field) ? _bulkFlags.get(field).value !== false : _savedFlagSummary[field] === true;
            const opposite = field === 'isNoShift' ? 'isFlexibleShift' : field === 'isFlexibleShift' ? 'isNoShift' : null;
            const oppositeSelection = opposite ? _bulkFlags.get(opposite) : null;
            if (!_bulkFlags.has(field) && oppositeSelection && oppositeSelection.value !== false &&
                JSON.stringify(oppositeSelection.filter) === JSON.stringify(_activePageFilter)) checked = false;
            $(selector).prop('checked', checked).prop('indeterminate', false)
                .prop('disabled', _isSaving || _refreshAfterSave || _pageLoading || !_activePageFilter || !_filteredCount);
        }
        dataTable.buttons().enable(!_isExporting && !_isSaving && !_refreshAfterSave && !_pageLoading && !!_activePageFilter && _filteredCount > 0);
    };

    const currentFilterSelection = () => {
        if (!_activePageFilter || !_filteredCount) return null;
        const filter = { ..._activePageFilter };
        const project = filter.projectId ? _projects.find(item => item.id === filter.projectId)?.name || `Project ${filter.projectId}` : 'All projects';
        const shift = filter.shiftId ? _shift.find(item => item.shiftId === filter.shiftId)?.shiftName || `Shift ${filter.shiftId}` : 'All shifts';
        return { filter, count: _filteredCount,
            description: `${project} / ${shift} / ${filter.filterType}${filter.searchKeyword ? ` / Search: ${filter.searchKeyword}` : ''}` };
    };

    const selectAllFiltered = checked => {
        if (_isSaving || _refreshAfterSave || _pageLoading) return;
        if (checked) {
            const selection = currentFilterSelection();
            if (!selection) return;
            _bulkAssignment = selection;
        } else {
            _bulkAssignment = null;
        }
        refreshSelectionRows();
    };

    const refreshSelectionRows = () => {
        // Only explicit edits survive changes to the inherited bulk baseline.
        for (const [id, pending] of _pendingChanges) {
            const original = _originalRows.get(id);
            if (!original) continue;
            const baseline = assignmentBaseline(original);
            const edited = { ...baseline };
            const fields = _pendingFields.get(id) || new Set();
            overlayEditedFields(edited, pending, fields, original);
            for (const field of fields) {
                if (!_bulkAssignment && !_bulkFlags.size && (edited[field] ?? null) === (baseline[field] ?? null)) fields.delete(field);
            }
            if (fields.size) _pendingChanges.set(id, edited);
            else {
                _pendingChanges.delete(id);
                _pendingFields.delete(id);
            }
        }
        dataTable.rows({ page: 'current' }).every(function () {
            const row = this.data();
            this.data(overlayPageRows([_originalRows.get(row.employeeId) || row])[0]);
        });
        // DataTables also clones scrolling headers; update the displayed row inputs
        // without a server draw so both selection and cancellation are immediate.
        $('#content-container').find('#employee-shift-grid tbody .row-check').each(function () {
            const $input = $(this);
            const row = dataTable.row($input.data('row')).data();
            const column = $input.attr('data-column') || $input.attr('data-name');
            if (row && column) $input.prop('checked', !!row[column]);
        });
        updatePendingStatus();
        updateHeaderCheckboxes();
    };

    const selectFilteredFlag = (field, value) => {
        if (_isSaving || _refreshAfterSave || _pageLoading || !['isFlexibleShift', 'isNoShift', 'isNoBreak'].includes(field)) return;
        const selection = currentFilterSelection();
        if (!selection) return;
        const opposite = field === 'isNoShift' ? 'isFlexibleShift' : field === 'isFlexibleShift' ? 'isNoShift' : null;
        if (value && opposite && JSON.stringify(_bulkFlags.get(opposite)?.filter) === JSON.stringify(selection.filter)) _bulkFlags.delete(opposite);
        _bulkFlags.delete(field);
        _bulkFlags.set(field, { ...selection, value });
        refreshSelectionRows();
    };

    const attachEvents = () => {
        $('#project, #shift, #status').on('change.employeeShift', () => {
            clearTimeout(_searchTimer);
            _requestVersion++;
            dataTable.search(($('#employee-shift-grid_filter input').val() || '').trim());
            dataTable.ajax.reload(null, true);
        });
        $('#save').on('click.employeeShift', onEmployeeShiftSubmit);
        $('#assignment-shift').on('change.employeeShift', updatePendingStatus);
        const $table = $('#content-container');
        $table.on('click.employeeShift', '#cancel-pending', event => {
            event.preventDefault();
            if (_isSaving || _refreshAfterSave) return;
            _pendingChanges.clear();
            _pendingFields.clear();
            _bulkAssignment = null;
            _bulkFlags.clear();
            refreshSelectionRows();
        });
        $table.on('change.employeeShift', '.row-check', function () {
            const $input = $(this);
            const index = $input.data('row');
            const row = dataTable.row(index);
            const value = row.data();
            if (!value) return;
            applyRowChange(value, $input.data('column') || $input.data('name'), $input.prop('checked'));
            row.data(value);
            updateHeaderCheckboxes();
        });
        $table.on('change.employeeShift', '#select-all', function () {
            selectAllFiltered($(this).prop('checked'));
        });
        $table.on('change.employeeShift', '.flag-select-all', function () {
            if (_isSaving || _refreshAfterSave || _pageLoading) return;
            const field = $(this).attr('data-column');
            if (!['isFlexibleShift', 'isNoShift', 'isNoBreak'].includes(field)) return;
            const checked = $(this).prop('checked');
            selectFilteredFlag(field, checked);
        });
    };

    const onEmployeeShiftSubmit = async event => {
        event.preventDefault();
        if (_isSaving || (!_bulkAssignment && !_bulkFlags.size && !_pendingChanges.size)) return;
        const selectedSchedule = Number($('#assignment-shift').val()) || 0;
        const unassign = selectedSchedule === -1;
        const shiftId = unassign ? 0 : selectedSchedule;
        const changes = Array.from(_pendingChanges.values());
        const flagFilters = Array.from(_bulkFlags, ([field, selection]) => ({ ...selection.filter, [field]: selection.value !== false,
            ...(selection.value === false ? {} : field === 'isFlexibleShift' ? { isNoShift: false } : field === 'isNoShift' ? { isFlexibleShift: false } : {}) }));
        const filteredSave = !!_bulkAssignment || flagFilters.length > 0;
        const data = changes.map(row => ({
            employeeId: row.employeeId, assignedShiftId: row.assignedShiftId, shiftId: row.shiftId,
            departmentId: row.departmentId ?? 0, projectId: row.projectId ?? 0,
            preserveSchedule: !unassign && row.assignedShiftId > 0 && (!row.isSelected || (!shiftId &&
                ['isFlexibleShift', 'isNoShift', 'isNoBreak'].some(field => _pendingFields.get(row.employeeId)?.has(field)))),
            isAssigned: unassign ? false : row.isAssigned || (row.assignedShiftId === 0 && !_pendingFields.get(row.employeeId)?.has('isSelected')),
            isFlexibleShift: row.isFlexibleShift,
            isNoShift: row.isNoShift, isNoBreak: row.isNoBreak
        }));
        if ((_bulkAssignment || data.some(row => row.isAssigned && !row.preserveSchedule)) && !shiftId && !unassign) {
            toastr.warning('Select a schedule or Unassigned before saving.');
            return;
        }
        _isSaving = true;
        updatePendingStatus();
        $('#project, #shift, #status, #assignment-shift').prop('disabled', true);
        $('#employee-shift-grid_wrapper input[type="checkbox"]').prop('disabled', true);
        try {
            const response = await _apiHelper.post({
                url: filteredSave ? 'Authenticated/EmployeeShift/AssignFiltered' : `Authenticated/EmployeeShift?shiftId=${shiftId}`,
                data: filteredSave ? { ...(_bulkAssignment?.filter || {}), applyAssignmentToFilter: !!_bulkAssignment,
                    flagFilters, scheduleId: !_bulkAssignment && !selectedSchedule ? null : shiftId, changes: data.map(row => ({
                    employeeId: row.employeeId, preserveSchedule: row.preserveSchedule, isAssigned: row.isAssigned, isFlexibleShift: row.isFlexibleShift,
                    isNoShift: row.isNoShift, isNoBreak: row.isNoBreak
                })) } : data,
                requestOrigin: 'Employee Shift Assignment', requesterName: $('#current-user').text(), requestSystem: SYSTEM
            });
            if (!response.ok) {
                toastr.error(response.status === 409 ? 'An assignment changed. Refresh the affected employee and try again.' :
                    response.status === 403 ? 'Access denied.' : 'Could not save assignments. Your pending changes have been kept.');
                return;
            }
            _pendingChanges.clear();
            _pendingFields.clear();
            _originalRows.clear();
            _bulkAssignment = null;
            _bulkFlags.clear();
            _refreshAfterSave = true;
            toastr.success('Employee shift assignments updated.');
            dataTable.ajax.reload(null, false);
        } catch (_) {
            toastr.error('Could not save assignments. Your pending changes have been kept.');
        } finally {
            _isSaving = false;
            $('#project, #shift, #status, #assignment-shift').prop('disabled', false);
            $('#employee-shift-grid_wrapper input[type="checkbox"]').prop('disabled', _refreshAfterSave);
            updatePendingStatus();
            updateHeaderCheckboxes();
        }
    };

    const escapeExportText = value => String(value ?? '').replace(/[&<>"']/g, character =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character]);

    const exportFiltered = async (type, event, table, node, config, buttonContext) => {
        event?.preventDefault?.();
        if (_isExporting || _isSaving || _refreshAfterSave || _pageLoading || !_activePageFilter) return;
        if (!_filteredCount) {
            toastr.info('No employees match these filters.');
            return;
        }
        // Open the print window during the click, before awaiting the API response.
        const printWindow = type === 'print' ? window.open('', '_blank') : null;
        if (type === 'print' && !printWindow) {
            toastr.error('Allow popups to open the print view.');
            return;
        }
        const filter = { ..._activePageFilter };
        const version = _requestVersion;
        _isExporting = true;
        updateHeaderCheckboxes();
        $('#employee-shift-page-status').text('Loading all filtered employees for export...');
        try {
            if (printWindow) {
                printWindow.opener = null;
                printWindow.document.write('<p>Loading filtered employee shifts...</p>');
            }
            // Omitting skip/take preserves the endpoint's full-filter array contract.
            const response = await _apiHelper.get({ url: `Authenticated/EmployeeShift/ShiftFilter?${new URLSearchParams(filter)}` });
            if (!response.ok) throw new Error('Export unavailable');
            const rows = await response.json();
            if (!Array.isArray(rows) || rows.some(row => !row || !Number.isInteger(row.employeeId))) throw new Error('Invalid export');
            if (version !== _requestVersion) {
                printWindow?.close();
                toastr.info('The table changed while loading. Export again with the current filters.');
                return;
            }
            if (!rows.length) {
                printWindow?.close();
                toastr.info('No employees match these filters.');
                return;
            }
            const columns = getEmployeeShiftColumns().filter(column => column.visible !== false && column.data !== 'isSelected');
            const header = columns.map(column => column.title.replace(/<[^>]*>/g, '').trim());
            const body = rows.map((row, index) => columns.map(column => {
                const value = row[column.data];
                if (typeof value === 'boolean') return value ? 'Yes' : 'No';
                return column.render ? column.render(value, 'export', row, { row: index }) : value ?? '-';
            }));
            if (printWindow) {
                if (printWindow.closed) return;
                const cells = (values, tag) => '<tr>' + values.map(value => `<${tag}>${escapeExportText(value)}</${tag}>`).join('') + '</tr>';
                printWindow.document.open();
                printWindow.document.write('<!doctype html><html><head><meta charset="utf-8"><title>Employee Shift Assignments</title>' +
                    '<style>@page{size:A3 landscape;margin:10mm}body{font-family:Arial,sans-serif;font-size:9px}table{width:100%;border-collapse:collapse;table-layout:fixed}th,td{border:1px solid #aaa;padding:4px;overflow-wrap:anywhere}thead{display:table-header-group}tr{break-inside:avoid}</style>' +
                    '</head><body><h1>Employee Shift Assignments</h1><table><thead>' + cells(header, 'th') + '</thead><tbody>' +
                    body.map(values => cells(values, 'td')).join('') + '</tbody></table></body></html>');
                printWindow.document.close();
                printWindow.focus();
                printWindow.print();
            } else {
                await new Promise((resolve, reject) => {
                    // Buttons 2.2 returns before asynchronous XLSX generation ends.
                    // Its processing(false) notification marks native completion.
                    const nativeContext = Object.create(buttonContext);
                    const timeout = setTimeout(() => {
                        reject(new Error('Export generation timed out'));
                        buttonContext.processing?.(false);
                    }, 120000);
                    nativeContext.processing = processing => {
                        buttonContext.processing?.(processing);
                        if (!processing) {
                            clearTimeout(timeout);
                            resolve();
                        }
                        return nativeContext;
                    };
                    try {
                        $.fn.dataTable.ext.buttons[type].action.call(nativeContext, event, table, node, {
                            ...config, footer: false,
                            exportOptions: { ...config.exportOptions, customizeData(data) {
                                data.header = header;
                                data.body = body;
                                data.footer = null;
                            } }
                        });
                    } catch (error) {
                        clearTimeout(timeout);
                        reject(error);
                        buttonContext.processing?.(false);
                    }
                });
            }
        } catch (_) {
            printWindow?.close();
            toastr.error('Could not export employee shifts. Try again.');
        } finally {
            _isExporting = false;
            if (version === _requestVersion) $('#employee-shift-page-status').text('');
            updateHeaderCheckboxes();
        }
    };

    const filteredExportButton = (type, label) => ({
        ...(type === 'print' ? {} : { extend: type }),
        text: `${label} (filtered)`, className: 'btn btn-primary ml-3', title: 'Employee Shift Assignments', filename: 'Employee Shift Assignments - filtered',
        exportOptions: { columns: ':visible:not(:first-child)', modifier: { selected: null } },
        ...(type === 'pdfHtml5' ? { orientation: 'landscape', pageSize: 'A3', customize(document) {
            document.defaultStyle.fontSize = 7;
            document.styles.tableHeader.fontSize = 7;
        } } : {}),
        action: function (event, table, node, config) {
            return exportFiltered(type, event, table, node, config, this);
        }
    });

    let getEmployeeShiftColumns = () => {
        return [
            {
                title: '<input name="select_all" value="1" id="select-all" type="checkbox" aria-label="Select all employees matching the selected filters"> Select all filtered',
                data: "isSelected",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-name="isSelected" data-row="${meta.row}" aria-label="Select employee ${row.employeeId}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Employee",
                data: "employeeName",
                className: 'dt-center',
                render: (data, type) => type === 'display' ? $.fn.dataTable.render.text().display(data ? _stringHelper.capitalize(data) : '-') : data ? _stringHelper.capitalize(data) : '-'
            },
            {
                title: "Mon Shift Start",
                data: "mondayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Mon Shift End',
                data: "mondayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Tue Shift Start",
                data: "tuesdayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Tue Shift End',
                data: "tuesdayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Wed Shift Start",
                data: "wednesdayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Wed Shift End',
                data: "wednesdayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Thu Shift Start",
                data: "thursdayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Thu Shift End',
                data: "thursdayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Fri Shift Start",
                data: "fridayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Fri Shift End',
                data: "fridayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Sat Shift Start",
                data: "saturdayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Sat Shift End',
                data: "saturdayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: "Sun Shift Start",
                data: "sundayShiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: 'Sun Shift End',
                data: "sundayShiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatLocalShortTime(data);
                }
            },
            {
                title: '<input class="flag-select-all" data-column="isFlexibleShift" type="checkbox" aria-label="Set Flexible Shift for all filtered employees"> Is Flexible Shift?',
                data: "isFlexibleShift",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" aria-label="Flexible shift for employee ${row.employeeId}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input class="flag-select-all" data-column="isNoShift" type="checkbox" aria-label="Set No Shift for all filtered employees"> Is No Shift',
                data: "isNoShift",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoShift" data-row="${meta.row}" aria-label="No shift for employee ${row.employeeId}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: '<input class="flag-select-all" data-column="isNoBreak" type="checkbox" aria-label="Set No Break for all filtered employees"> Is No Break',
                data: "isNoBreak",
                width: "50px",
                className: 'dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isNoBreak" data-row="${meta.row}" aria-label="No break for employee ${row.employeeId}" ${data ? 'checked' : ''}>`;
                },
                orderable: false
            },
            {
                title: "Shift",
                data: "shiftName",
                className: 'dt-center',
                render: (data, type) => type === 'display' ? $.fn.dataTable.render.text().display(data ? _stringHelper.capitalize(data) : '-') : data ? _stringHelper.capitalize(data) : '-'
            },
            // {
            //     title: "Department",
            //     data: "departmentName",
            //     className: 'dt-center',
            //     render: (data) => $.fn.dataTable.render.text().display(data ? _stringHelper.capitalize(data) : '-')
            // },
            {
                title: "Project",
                data: "projectName",
                className: 'dt-center',
                render: (data, type) => type === 'display' ? $.fn.dataTable.render.text().display(data ? _stringHelper.capitalize(data) : '-') : data ? _stringHelper.capitalize(data) : '-'
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
            // {
            //     title: "Dept. Id",
            //     data: "departmentId",
            //     visible: false
            // }
            {
                title: "Project Id",
                data: "projectId",
                visible: false
            }
        ];
    };

    const initializeGrid = () => {
        $('#employee-shift-grid').empty();
        dataTable = $('#employee-shift-grid').DataTable({
            dom: 'Bfrtip',
            buttons: [
                filteredExportButton('excelHtml5', 'Excel'),
                filteredExportButton('pdfHtml5', 'PDF'),
                filteredExportButton('print', 'Print')
            ],
            serverSide: true,
            processing: true,
            paging: true,
            pageLength: 10,
            lengthChange: false,
            ordering: false,
            searching: true,
            autoWidth: true,
            scrollX: true,
            ajax: requestGridPage,
            columns: getEmployeeShiftColumns(),
            language: {
                emptyTable: 'No employees available.',
                zeroRecords: 'No employees match these filters.',
                search: 'Search:',
                searchPlaceholder: 'Employee, project or shift'
            },
            drawCallback: function () {
                // Recalculate both scrolling tables for the current page's content.
                this.api().columns.adjust();
                $('#employee-shift-grid_wrapper input[type="checkbox"]').prop('disabled', _isSaving || _refreshAfterSave);
                // Initial draws occur before DataTable() returns its API instance.
                if (dataTable) updateHeaderCheckboxes();
            },
            initComplete: function () {
                // DataTables 1.10 throttles searchDelay; bind a real debounce instead.
                $('#employee-shift-grid_filter input').off('.DT').on('input.employeeShift', function () {
                    clearTimeout(_searchTimer);
                    _requestVersion++;
                    _pageLoading = true;
                    updateHeaderCheckboxes();
                    const search = this.value.trim();
                    _searchTimer = setTimeout(() => dataTable.search(search).draw(), 300);
                });
            }
        });
    };

    const renderDropDowns = () => {
        const render = (selector, items, id, name, firstLabel, allowUnassigned = false) => {
            const $select = $(selector).empty();
            $select.append($('<option>').val('0').text(firstLabel));
            if (allowUnassigned) $select.append($('<option>').val('-1').text('Unassigned'));
            for (const item of items) $select.append($('<option>').val(item[id]).text(item[name] || '-'));
        };
        render('#project', _projects, 'id', 'name', 'All');
        render('#shift', _shift, 'shiftId', 'shiftName', 'All');
        render('#assignment-shift', _shift, 'shiftId', 'shiftName', 'Select schedule', true);
        $('#status').val('All');
    };

    const initializeApplication = async () => {
        toastr.options.escapeHtml = true;
        const results = await Promise.allSettled(['Project', 'Shift'].map(async name => {
            const response = await _apiHelper.get({ url: `Authenticated/${name}` });
            if (!response?.ok) throw new Error('Dropdown unavailable');
            const items = await response.json();
            if (!Array.isArray(items)) throw new Error('Invalid dropdown response');
            return items;
        }));
        if (results[0].status === 'fulfilled') _projects = results[0].value;
        else toastr.error('Could not load project options. Refresh to try again.');
        if (results[1].status === 'fulfilled') _shift = results[1].value;
        else toastr.error('Could not load shift options. Refresh to try again.');
        renderDropDowns();
        initializeGrid();
        attachEvents();
        updatePendingStatus();
    };

    // The layout's jQuery 1.11 does not recognize async functions as ready callbacks.
    $(document).ready(function () {
        initializeApplication();
    });
})(jQuery);
